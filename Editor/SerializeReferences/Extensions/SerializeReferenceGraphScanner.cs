using System;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Builds a document-per-component managed-reference graph from the raw YAML, independent of the live
    // serialization API, so it sees the nested, orphaned and missing references the Inspector cannot navigate to.
    internal static class SerializeReferenceGraphScanner
    {
        // The document headers, RefIds lookup and inline-type grammar live in SerializeReferenceYaml, so this
        // scanner and the repair flow cannot disagree about Unity's RefIds shape.
        private static readonly Regex _referencesKey = new(@"^\s*references:\s*$", RegexOptions.Compiled);
        private static readonly Regex _entryRid = new(@"^(?<indent>\s*)-\s+rid:\s*(?<id>-?\d+)\s*$", RegexOptions.Compiled);
        private static readonly Regex _typeLine = new(@"^\s*type:\s*\{(?<body>.*)\}\s*$", RegexOptions.Compiled);
        private static readonly Regex _dataKey = new(@"^\s*data:\s*$", RegexOptions.Compiled);

        // A "rid:" pointer in any of its shapes. The lookbehind keeps a field whose name ends in "rid" from
        // matching; a match is further validated against the known RefIds set before becoming an edge.
        private static readonly Regex _ridPointer = new(@"(?<!\w)rid:\s*(?<id>-?\d+)", RegexOptions.Compiled);

        // A mapping key on a body line, used to label a root.
        private static readonly Regex _mappingKey = new(@"^\s*(?:-\s+)?(?<key>[A-Za-z_][\w\-]*)\s*:", RegexOptions.Compiled);

        // The graph of every document that has a RefIds block; a read or parse failure yields an empty list.
        // resolveTypeNames loads the asset and its dependencies to name each document, so a data-only caller passes
        // false to keep a project-wide sweep a pure text scan.
        public static List<ReferenceGraphDocument> Build(string assetPath, bool resolveTypeNames = true)
        {
            var result = new List<ReferenceGraphDocument>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = File.ReadAllLines(assetPath);
                var headers = CollectHeaders(lines);
                var typeNames = resolveTypeNames ? ResolveTypeNames(assetPath) : null;

                for (var h = 0; h < headers.Count; h++)
                {
                    var (fileId, classId, start) = headers[h];
                    var end = h + 1 < headers.Count ? headers[h + 1].start : lines.Length;

                    var document = BuildDocument(lines, fileId, start, end);
                    if (document is null) continue;

                    document.TypeName = typeNames != null && typeNames.TryGetValue(fileId, out var name) && !string.IsNullOrEmpty(name)
                        ? name
                        : $"!u!{classId}";

                    result.Add(document);
                }
            }
            catch (Exception)
            {
                // Best effort — a parse failure simply yields no graph to display.
            }

            return result;
        }

        // Null when the document has no RefIds block, or when that block backs neither a node nor a field pointer.
        private static ReferenceGraphDocument BuildDocument(string[] lines, long fileId, int start, int end)
        {
            var referencesStart = FindKey(lines, _referencesKey, start, end);
            var bodyEnd = referencesStart >= 0 ? referencesStart : end;

            var refIdsStart = SerializeReferenceYaml.FindRefIdsStart(lines, start, end);
            if (refIdsStart < 0) return null;

            var document = new ReferenceGraphDocument { FileId = fileId };
            CollectNodes(lines, refIdsStart, end, document);

            var knownRids = new HashSet<long>();
            foreach (var node in document.Nodes) knownRids.Add(node.Rid);

            CollectRoots(lines, start, bodyEnd, knownRids, document);
            CollectEdges(lines, refIdsStart, end, knownRids, document);
            ComputeSharedAndOrphans(document, knownRids);

            // An asset whose fields are all unassigned still has slots to surface as "<None>" leaves.
            if (document.Nodes.Count == 0 && document.Roots.Count == 0) return null;

            return document;
        }

        private static void CollectNodes(string[] lines, int refIdsStart, int end, ReferenceGraphDocument document)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var ridMatch = _entryRid.Match(lines[i]);
                if (!ridMatch.Success || !long.TryParse(ridMatch.Groups["id"].Value, out var rid)) continue;

                // Negative rids are Unity's null/unknown sentinels, not managed objects, so they are not nodes.
                if (rid < 0) continue;

                var type = default(ManagedTypeName);
                for (var j = i + 1; j < end && j <= i + 4; j++)
                {
                    var typeMatch = _typeLine.Match(lines[j]);
                    if (!typeMatch.Success) continue;

                    // On a parse failure type stays default/empty (and the node renders as just "rid N").
                    if (!SerializeReferenceYaml.TryParseInlineType(typeMatch.Groups["body"].Value, out type))
                        type = default;
                    break;
                }

                var resolves = !type.IsEmpty && SerializeReferenceHelpers.StoredTypeResolves(type);
                document.Nodes.Add(new ReferenceGraphNode(rid, type, resolves));
            }
        }

        // Field pointers in the document body are the tree roots. Every pointer is kept, with no de-duplication, so
        // two fields aliasing one reference both render and the alias counts as shared.
        private static void CollectRoots(string[] lines, int start, int bodyEnd, HashSet<long> knownRids, ReferenceGraphDocument document)
        {
            for (var i = start + 1; i < bodyEnd; i++)
            {
                foreach (Match match in _ridPointer.Matches(lines[i]))
                {
                    if (!long.TryParse(match.Groups["id"].Value, out var rid)) continue;

                    // Kept as an empty root so a cleared slot stays visible; the shared/orphan math skips sentinels.
                    if (rid < 0)
                    {
                        document.Roots.Add(new ReferenceGraphRoot(rid, BuildRootPath(lines, i, start)));
                        continue;
                    }

                    if (!knownRids.Contains(rid)) continue; // a dangling pointer, not a graphed reference

                    document.Roots.Add(new ReferenceGraphRoot(rid, BuildRootPath(lines, i, start)));
                }
            }
        }

        // Every "rid:" pointer inside an entry's data block is a parent -> child edge. The entry's own header line
        // is skipped so an entry is never recorded as its own child.
        private static void CollectEdges(string[] lines, int refIdsStart, int end, HashSet<long> knownRids, ReferenceGraphDocument document)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var ridMatch = _entryRid.Match(lines[i]);
                if (!ridMatch.Success || !long.TryParse(ridMatch.Groups["id"].Value, out var parent)) continue;
                if (parent < 0) continue; // a sentinel entry carries no data block of its own

                var entryIndent = ridMatch.Groups["indent"].Length;
                var entryEnd = SerializeReferenceYaml.FindEntryEnd(lines, i, end, entryIndent);

                var dataStart = FindKey(lines, _dataKey, i + 1, entryEnd);
                if (dataStart < 0) continue;

                for (var j = dataStart + 1; j < entryEnd; j++)
                {
                    foreach (Match match in _ridPointer.Matches(lines[j]))
                    {
                        if (!long.TryParse(match.Groups["id"].Value, out var child)) continue;
                        if (child == parent) continue;

                        // A null slot stays visible as an empty edge; a dangling pointer to no known node is dropped.
                        if (child >= 0 && !knownRids.Contains(child)) continue;

                        AddEdge(document, parent, new ReferenceGraphEdge(child, BuildEdgePath(lines, j, dataStart)));
                    }
                }
            }
        }

        // Shared = referenced by 2+ parents in total (root pointers + nested edges each count once).
        // Orphans = nodes reachable from no root.
        private static void ComputeSharedAndOrphans(ReferenceGraphDocument document, HashSet<long> knownRids)
        {
            var parentCount = new Dictionary<long, int>();

            // Sentinels are excluded throughout: every cleared field points at the same -2, so counting them would
            // flag it as shared, and it is not a node, so it is never reachable or orphaned either.
            foreach (var root in document.Roots)
                if (root.Rid >= 0) parentCount[root.Rid] = parentCount.GetValueOrDefault(root.Rid) + 1;

            foreach (var pair in document.Edges)
            {
                foreach (var edge in pair.Value)
                    if (edge.Rid >= 0) parentCount[edge.Rid] = parentCount.GetValueOrDefault(edge.Rid) + 1;
            }

            foreach (var pair in parentCount)
                if (pair.Value >= 2) document.Shared.Add(pair.Key);

            var reachable = new HashSet<long>();
            var queue = new Queue<long>();
            foreach (var root in document.Roots)
                if (root.Rid >= 0 && reachable.Add(root.Rid)) queue.Enqueue(root.Rid);

            while (queue.Count > 0)
            {
                var rid = queue.Dequeue();
                foreach (var edge in document.ChildrenOf(rid))
                    if (edge.Rid >= 0 && reachable.Add(edge.Rid)) queue.Enqueue(edge.Rid);
            }

            foreach (var rid in knownRids)
                if (!reachable.Contains(rid)) document.Orphans.Add(rid);
        }

        private static void AddEdge(ReferenceGraphDocument document, long parent, ReferenceGraphEdge edge)
        {
            if (!document.Edges.TryGetValue(parent, out var children))
            {
                children = new List<ReferenceGraphEdge>();
                document.Edges[parent] = children;
            }

            // A real child de-dups, so an alias within one parent counts once; empty slots are distinct fields that
            // merely share the sentinel, so they are always kept.
            if (edge.Rid >= 0 && children.Exists(c => c.Rid == edge.Rid)) return;
            children.Add(edge);
        }

        // A root pointer's full field path. The indent-0 document wrapper key is excluded, so a top-level field
        // reads as "_weapon" rather than carrying the wrapper name.
        private static string BuildRootPath(string[] lines, int i, int start)
        {
            var path = BuildPath(lines, i, floor: start, stopIndent: 0);
            return string.IsNullOrEmpty(path) ? "reference" : path;
        }

        // A nested edge's field path, stopped at the data block's own indent so it stays parent-relative.
        private static string BuildEdgePath(string[] lines, int pointerLine, int dataStart) =>
            BuildPath(lines, pointerLine, floor: dataStart, stopIndent: SerializeReferenceYaml.IndentOf(lines[dataStart]));

        // Walks up from the rid pointer collecting mapping keys until it climbs past stopIndent or floor. Unity
        // writes block-sequence dashes at the SAME column as the list key, so the owner key sits at equal indent
        // above the dash and the index counts the same-indent dash siblings.
        private static string BuildPath(string[] lines, int pointerLine, int floor, int stopIndent)
        {
            var segments = new List<string>();
            var line = pointerLine;

            // The counter only guards a malformed file from looping the walk forever.
            for (var safety = 0; line > floor && safety < 256; safety++)
            {
                var indent = SerializeReferenceYaml.IndentOf(lines[line]);
                int next;

                if (lines[line].TrimStart().StartsWith("- "))
                {
                    // A field key on the dash line itself is the deepest segment.
                    var elementKey = _mappingKey.Match(lines[line]);
                    if (elementKey.Success && !IsStructuralKey(elementKey.Groups["key"].Value))
                        segments.Add(elementKey.Groups["key"].Value);

                    var index = 0;
                    var ownerLine = -1;
                    for (var j = line - 1; j >= floor; j--)
                    {
                        if (lines[j].Trim().Length == 0) continue;

                        var jIndent = SerializeReferenceYaml.IndentOf(lines[j]);
                        if (jIndent > indent) continue;                          // nested detail of an earlier sibling
                        if (jIndent < indent) break;                             // dedented out of the list
                        if (lines[j].TrimStart().StartsWith("- ")) { index++; continue; }

                        ownerLine = j;
                        break;
                    }

                    if (ownerLine < 0) break;

                    var ownerKey = _mappingKey.Match(lines[ownerLine]);
                    if (!ownerKey.Success || IsStructuralKey(ownerKey.Groups["key"].Value)) break;

                    segments.Add($"{ownerKey.Groups["key"].Value}[{index}]");
                    next = ParentLine(lines, ownerLine, floor, SerializeReferenceYaml.IndentOf(lines[ownerLine]));
                }
                else
                {
                    var match = _mappingKey.Match(lines[line]);
                    if (match.Success && !IsStructuralKey(match.Groups["key"].Value))
                        segments.Add(match.Groups["key"].Value);

                    next = ParentLine(lines, line, floor, indent);
                }

                if (next < 0 || SerializeReferenceYaml.IndentOf(lines[next]) <= stopIndent) break; // climbed out of the enclosing scope
                line = next;
            }

            if (segments.Count == 0) return string.Empty;

            segments.Reverse();
            return string.Join(".", segments);
        }

        // The nearest non-empty line above with strictly shallower indent, or -1.
        private static int ParentLine(string[] lines, int from, int start, int indent)
        {
            for (var j = from - 1; j >= start; j--)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (SerializeReferenceYaml.IndentOf(lines[j]) < indent) return j;
            }

            return -1;
        }

        // YAML scaffolding keys that never make a meaningful root label.
        private static bool IsStructuralKey(string key) =>
            key is "rid" or "data" or "type" or "version" or "references" or "RefIds";

        private static List<(long fileId, int classId, int start)> CollectHeaders(string[] lines)
        {
            var headers = new List<(long, int, int)>();
            for (var i = 0; i < lines.Length; i++)
            {
                var match = SerializeReferenceYaml.DocumentHeader.Match(lines[i]);
                if (match.Success &&
                    long.TryParse(match.Groups["id"].Value, out var fileId) &&
                    int.TryParse(match.Groups["class"].Value, out var classId))
                {
                    headers.Add((fileId, classId, i));
                }
            }

            return headers;
        }

        // One LoadAllAssetsAtPath pass; objects Unity cannot load fall back to the YAML class id.
        private static Dictionary<long, string> ResolveTypeNames(string assetPath)
        {
            var map = new Dictionary<long, string>();

            // Scenes are unreadable through LoadAllAssetsAtPath, so they fall back to the YAML class id label.
            if (SerializeReferenceHelpers.IsScene(assetPath)) return map;

            try
            {
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                {
                    if (obj == null) continue;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var fileId)) continue;

                    map[fileId] = DisplayNameOf(obj);
                }
            }
            catch (Exception)
            {
                // Best effort — the YAML class id is the fallback header label.
            }

            return map;
        }

        private static string DisplayNameOf(Object obj)
        {
            var typeName = obj.GetType().Name;
            return string.IsNullOrEmpty(obj.name) ? typeName : $"{typeName}  ·  {obj.name}";
        }

        private static int FindKey(string[] lines, Regex key, int start, int end)
        {
            for (var i = start; i < end; i++)
                if (key.IsMatch(lines[i])) return i;

            return -1;
        }
    }
}
