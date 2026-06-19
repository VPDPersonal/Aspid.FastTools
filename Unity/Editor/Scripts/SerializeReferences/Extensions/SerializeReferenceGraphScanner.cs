using System;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// A single managed-reference node in a document's graph: its <c>RefIds</c> id, the stored type identity and
    /// whether that type still resolves to a loadable <see cref="Type"/>. Built purely from the asset YAML, so it
    /// surfaces references at any nesting depth — including the orphaned ones Unity drops from the live object.
    /// </summary>
    internal readonly struct ReferenceGraphNode
    {
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;
        public readonly bool Resolves;

        public ReferenceGraphNode(long rid, ManagedTypeName storedType, bool resolves)
        {
            Rid = rid;
            StoredType = storedType;
            Resolves = resolves;
        }

        /// <summary>Short type name (the class identifier without namespace/assembly), for the row label.</summary>
        public string ShortName =>
            string.IsNullOrEmpty(StoredType.Class) ? $"rid {Rid}" : StoredType.Class;

        /// <summary>Full <c>Namespace.Class, Assembly</c> identity, for the row tooltip.</summary>
        public string FullName
        {
            get
            {
                if (StoredType.IsEmpty) return string.Empty;

                var name = string.IsNullOrEmpty(StoredType.Namespace)
                    ? StoredType.Class
                    : $"{StoredType.Namespace}.{StoredType.Class}";

                return string.IsNullOrEmpty(StoredType.Assembly) ? name : $"{name}, {StoredType.Assembly}";
            }
        }
    }

    /// <summary>
    /// A field pointer from a document's body into its <c>RefIds</c> block — a root of the reference tree. The
    /// <see cref="Label"/> is the full field path that holds the reference (best effort), with list elements indexed,
    /// e.g. <c>_weapon</c> or <c>_alternates[0]</c> or <c>_config._slots[2]</c>.
    /// </summary>
    internal readonly struct ReferenceGraphRoot
    {
        public readonly long Rid;
        public readonly string Label;

        public ReferenceGraphRoot(long rid, string label)
        {
            Rid = rid;
            Label = label;
        }
    }

    /// <summary>
    /// The managed-reference graph of one serialized object document: its <c>fileId</c> anchor, an optional
    /// best-effort component/type name for the header, the <c>RefIds</c> nodes, the parent → child edges between
    /// them, the field-pointer roots and the derived shared / orphaned sets.
    /// </summary>
    internal sealed class ReferenceGraphDocument
    {
        public long FileId;
        public string TypeName;

        public readonly List<ReferenceGraphNode> Nodes = new();

        // One entry per field pointer in the document body (the tree's entry points). The same rid may appear under
        // two fields — both are kept, so the window renders each subtree and the shared set flags the alias.
        public readonly List<ReferenceGraphRoot> Roots = new();

        // Parent rid → ordered, de-duplicated child rids of the nested graph (data-block pointers only; roots are
        // tracked separately in Roots).
        public readonly Dictionary<long, List<long>> Edges = new();

        // rids referenced by two or more parents in total (root pointers + nested edges) — aliased managed references.
        public readonly HashSet<long> Shared = new();

        // rids reachable from no root — leftover payloads no field points at.
        public readonly HashSet<long> Orphans = new();

        public ReferenceGraphNode? FindNode(long rid)
        {
            foreach (var node in Nodes)
                if (node.Rid == rid) return node;

            return null;
        }

        public IReadOnlyList<long> ChildrenOf(long rid) =>
            Edges.TryGetValue(rid, out var children) ? children : Array.Empty<long>();
    }

    /// <summary>
    /// Builds, per asset path, a document-per-component managed-reference graph from the raw YAML — independent of
    /// the live serialization API, so it sees nested, orphaned and missing references the Inspector cannot navigate
    /// to. Parsing is local to this scanner: it reuses <see cref="ManagedTypeName"/> /
    /// <see cref="SerializeReferenceHelpers.StoredTypeResolves"/> for type identity only, and does not depend on the
    /// repair-flow helpers in <see cref="SerializeReferenceYamlEditor"/>.
    /// </summary>
    internal static class SerializeReferenceGraphScanner
    {
        // "--- !u!114 &11400000" — object document header carrying the local file id as its YAML anchor and the
        // class id ("!u!114") used as a best-effort fallback label when the live type name is unavailable.
        private static readonly Regex DocumentHeader = new(@"^--- !u!(?<class>\d+) &(?<id>\d+)", RegexOptions.Compiled);
        private static readonly Regex ReferencesKey = new(@"^\s*references:\s*$", RegexOptions.Compiled);
        private static readonly Regex RefIdsKey = new(@"^\s*RefIds:\s*$", RegexOptions.Compiled);
        private static readonly Regex EntryRid = new(@"^(?<indent>\s*)-\s+rid:\s*(?<id>-?\d+)\s*$", RegexOptions.Compiled);
        private static readonly Regex TypeLine = new(@"^\s*type:\s*\{(?<body>.*)\}\s*$", RegexOptions.Compiled);
        private static readonly Regex DataKey = new(@"^\s*data:\s*$", RegexOptions.Compiled);
        private static readonly Regex InlineType = new(
            @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$",
            RegexOptions.Compiled);

        // A "rid:" pointer anywhere in a body/data line (inline "{rid: N}", "- rid: N", or a bare "rid: N" scalar).
        // The leading non-word lookbehind keeps a field whose name ends in "rid" (e.g. "_hybrid: 5") from matching;
        // a matched number is further validated against the known RefIds set before becoming an edge.
        private static readonly Regex RidPointer = new(@"(?<!\w)rid:\s*(?<id>-?\d+)", RegexOptions.Compiled);

        // A mapping key on a body line ("_weapon:", "data:", a sequence item's "- _weapon:"), used to label a root.
        private static readonly Regex MappingKey = new(@"^\s*(?:-\s+)?(?<key>[A-Za-z_][\w\-]*)\s*:", RegexOptions.Compiled);

        /// <summary>
        /// Scans every object document in the asset and returns the managed-reference graph of each one that has a
        /// <c>RefIds</c> block. Documents without managed references are skipped. A read or parse failure yields an
        /// empty list — the window simply shows its empty state.
        /// </summary>
        public static List<ReferenceGraphDocument> Build(string assetPath)
        {
            var result = new List<ReferenceGraphDocument>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = File.ReadAllLines(assetPath);
                var headers = CollectHeaders(lines);
                var typeNames = ResolveTypeNames(assetPath);

                for (var h = 0; h < headers.Count; h++)
                {
                    var (fileId, classId, start) = headers[h];
                    var end = h + 1 < headers.Count ? headers[h + 1].start : lines.Length;

                    var document = BuildDocument(lines, fileId, start, end);
                    if (document is null) continue;

                    document.TypeName = typeNames.TryGetValue(fileId, out var name) && !string.IsNullOrEmpty(name)
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

        // Parses one document's RefIds block into nodes, then walks the body (field pointers = roots) and each
        // entry's data block (nested pointers = edges) to assemble edges, shared and orphan sets. Returns null when
        // the document has no RefIds block (no managed references to graph).
        private static ReferenceGraphDocument BuildDocument(string[] lines, long fileId, int start, int end)
        {
            var referencesStart = FindKey(lines, ReferencesKey, start, end);
            var bodyEnd = referencesStart >= 0 ? referencesStart : end;

            var refIdsStart = FindKey(lines, RefIdsKey, start, end);
            if (refIdsStart < 0) return null;

            var document = new ReferenceGraphDocument { FileId = fileId };
            CollectNodes(lines, refIdsStart, end, document);
            if (document.Nodes.Count == 0) return null;

            var knownRids = new HashSet<long>();
            foreach (var node in document.Nodes) knownRids.Add(node.Rid);

            CollectRoots(lines, start, bodyEnd, knownRids, document);
            CollectEdges(lines, refIdsStart, end, knownRids, document);
            ComputeSharedAndOrphans(document, knownRids);

            return document;
        }

        // Each "- rid: N" entry in the RefIds block becomes a node; its type is read from the following "type:" line.
        private static void CollectNodes(string[] lines, int refIdsStart, int end, ReferenceGraphDocument document)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var ridMatch = EntryRid.Match(lines[i]);
                if (!ridMatch.Success || !long.TryParse(ridMatch.Groups["id"].Value, out var rid)) continue;

                var type = default(ManagedTypeName);
                for (var j = i + 1; j < end && j <= i + 4; j++)
                {
                    var typeMatch = TypeLine.Match(lines[j]);
                    if (!typeMatch.Success) continue;

                    // On a parse failure type stays default/empty (and the node renders as just "rid N").
                    if (!TryParseInlineType(typeMatch.Groups["body"].Value, out type))
                        type = default;
                    break;
                }

                var resolves = !type.IsEmpty && SerializeReferenceHelpers.StoredTypeResolves(type);
                document.Nodes.Add(new ReferenceGraphNode(rid, type, resolves));
            }
        }

        // Field pointers in the document body (everything before the "references:" block) are the tree roots. The
        // label is the full field path the pointer sits under, with list elements indexed (see BuildRootPath). Each
        // pointer is kept (no rid de-duplication) so two fields aliasing one reference both render and the alias is
        // counted as shared.
        private static void CollectRoots(string[] lines, int start, int bodyEnd, HashSet<long> knownRids, ReferenceGraphDocument document)
        {
            for (var i = start + 1; i < bodyEnd; i++)
            {
                foreach (Match match in RidPointer.Matches(lines[i]))
                {
                    if (!long.TryParse(match.Groups["id"].Value, out var rid)) continue;
                    if (!knownRids.Contains(rid)) continue; // a dangling pointer, not a graphed reference

                    var label = BuildRootPath(lines, i, start);
                    document.Roots.Add(new ReferenceGraphRoot(rid, label));
                }
            }
        }

        // Within each RefIds entry's "data:" block, every "rid:" pointer is a parent → child edge. The entry's own
        // "- rid:" header line is skipped so an entry is never recorded as its own child.
        private static void CollectEdges(string[] lines, int refIdsStart, int end, HashSet<long> knownRids, ReferenceGraphDocument document)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var ridMatch = EntryRid.Match(lines[i]);
                if (!ridMatch.Success || !long.TryParse(ridMatch.Groups["id"].Value, out var parent)) continue;

                var entryIndent = ridMatch.Groups["indent"].Length;
                var entryEnd = FindEntryEnd(lines, i + 1, end, entryIndent);

                var dataStart = FindKey(lines, DataKey, i + 1, entryEnd);
                if (dataStart < 0) continue;

                for (var j = dataStart + 1; j < entryEnd; j++)
                {
                    foreach (Match match in RidPointer.Matches(lines[j]))
                    {
                        if (!long.TryParse(match.Groups["id"].Value, out var child)) continue;
                        if (child == parent || !knownRids.Contains(child)) continue;

                        AddEdge(document, parent, child);
                    }
                }
            }
        }

        // Shared = referenced by 2+ parents in total — every root pointer plus every nested edge counts once.
        // Orphans = reachable from no root, found by a BFS from the roots and complementing against the known node
        // set (cycle-safe via the visited set).
        private static void ComputeSharedAndOrphans(ReferenceGraphDocument document, HashSet<long> knownRids)
        {
            var parentCount = new Dictionary<long, int>();

            foreach (var root in document.Roots)
                parentCount[root.Rid] = parentCount.GetValueOrDefault(root.Rid) + 1;

            foreach (var pair in document.Edges)
            foreach (var child in pair.Value)
                parentCount[child] = parentCount.GetValueOrDefault(child) + 1;

            foreach (var pair in parentCount)
                if (pair.Value >= 2) document.Shared.Add(pair.Key);

            var reachable = new HashSet<long>();
            var queue = new Queue<long>();
            foreach (var root in document.Roots)
                if (reachable.Add(root.Rid)) queue.Enqueue(root.Rid);

            while (queue.Count > 0)
            {
                var rid = queue.Dequeue();
                foreach (var child in document.ChildrenOf(rid))
                    if (reachable.Add(child)) queue.Enqueue(child);
            }

            foreach (var rid in knownRids)
                if (!reachable.Contains(rid)) document.Orphans.Add(rid);
        }

        private static void AddEdge(ReferenceGraphDocument document, long parent, long child)
        {
            if (!document.Edges.TryGetValue(parent, out var children))
            {
                children = new List<long>();
                document.Edges[parent] = children;
            }

            if (!children.Contains(child)) children.Add(child);
        }

        // Builds the full field path from the document body down to the rid pointer on line i — the chain of mapping
        // keys, joined by ".", with a "[index]" suffix on any list key the path crosses. The document's root wrapper
        // key (the indent-0 "MonoBehaviour:" / "Transform:" / … line) is excluded — every real field nests under it —
        // so a top-level field reads as "_weapon" and a list element as "_alternates[0]", never the wrapper name. Two
        // YAML shapes are handled per ancestor:
        // • a named value over two lines ("_weapon:\n  rid: N") — the owning key is the nearest strictly-shallower line.
        // • a block-sequence item ("_alternates:\n  - rid: N") — Unity writes the dashes at the same column as the list
        //   key, so the key sits at *equal* indent above the dash; the element's index is its position among the same-
        //   indent "- …" siblings. An element that carries its own field key on the dash line ("- _weapon:") keeps it,
        //   so the path reads "_slots[0]._weapon".
        private static string BuildRootPath(string[] lines, int i, int start)
        {
            var segments = new List<string>();
            var line = i;

            // YAML nesting is finite; the counter only guards a malformed file from looping the walk forever.
            for (var safety = 0; line > start && safety < 256; safety++)
            {
                var indent = IndentOf(lines[line]);
                int next;

                if (lines[line].TrimStart().StartsWith("- "))
                {
                    // The element's own field key, if the dash line carries one ("- _weapon:"), is the deepest segment.
                    var elementKey = MappingKey.Match(lines[line]);
                    if (elementKey.Success && !IsStructuralKey(elementKey.Groups["key"].Value))
                        segments.Add(elementKey.Groups["key"].Value);

                    // The list key (first non-"- " mapping key at this indent above the dashes) and the element index
                    // (its position among the same-indent "- …" siblings).
                    var index = 0;
                    var ownerLine = -1;
                    for (var j = line - 1; j >= start; j--)
                    {
                        if (lines[j].Trim().Length == 0) continue;

                        var jIndent = IndentOf(lines[j]);
                        if (jIndent > indent) continue;                          // nested detail of an earlier sibling
                        if (jIndent < indent) break;                             // dedented out of the list
                        if (lines[j].TrimStart().StartsWith("- ")) { index++; continue; }

                        ownerLine = j;
                        break;
                    }

                    if (ownerLine < 0) break;

                    var ownerKey = MappingKey.Match(lines[ownerLine]);
                    if (!ownerKey.Success || IsStructuralKey(ownerKey.Groups["key"].Value)) break;

                    segments.Add($"{ownerKey.Groups["key"].Value}[{index}]");
                    next = ParentLine(lines, ownerLine, start, IndentOf(lines[ownerLine]));
                }
                else
                {
                    // A mapping line: record its key, or climb past a structural "rid:"/"data:" line that contributes
                    // no path segment of its own.
                    var match = MappingKey.Match(lines[line]);
                    if (match.Success && !IsStructuralKey(match.Groups["key"].Value))
                        segments.Add(match.Groups["key"].Value);

                    next = ParentLine(lines, line, start, indent);
                }

                if (next < 0 || IndentOf(lines[next]) == 0) break; // reached the indent-0 document wrapper key
                line = next;
            }

            if (segments.Count == 0) return "reference";

            segments.Reverse();
            return string.Join(".", segments);
        }

        // The nearest non-empty line above `from` whose indent is strictly shallower than `indent` — the structural
        // parent of the node at `from`. Returns -1 when none exists within the document.
        private static int ParentLine(string[] lines, int from, int start, int indent)
        {
            for (var j = from - 1; j >= start; j--)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (IndentOf(lines[j]) < indent) return j;
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
                var match = DocumentHeader.Match(lines[i]);
                if (match.Success &&
                    long.TryParse(match.Groups["id"].Value, out var fileId) &&
                    int.TryParse(match.Groups["class"].Value, out var classId))
                {
                    headers.Add((fileId, classId, i));
                }
            }

            return headers;
        }

        // Maps each document's fileId to a display name from the live object (component / ScriptableObject type
        // name). Best effort and cheap: a single LoadAllAssetsAtPath pass; objects Unity cannot load are simply
        // omitted and fall back to the YAML class id.
        private static Dictionary<long, string> ResolveTypeNames(string assetPath)
        {
            var map = new Dictionary<long, string>();

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

        // A RefIds entry runs until the next list item at its own indent, or until the block dedents out of it.
        private static int FindEntryEnd(string[] lines, int from, int end, int entryIndent)
        {
            for (var j = from; j < end; j++)
            {
                if (lines[j].Trim().Length == 0) continue;

                var indent = IndentOf(lines[j]);
                if (indent < entryIndent) return j;
                if (indent == entryIndent && lines[j].TrimStart().StartsWith("- ")) return j;
            }

            return end;
        }

        private static int IndentOf(string line)
        {
            var count = 0;
            while (count < line.Length && line[count] == ' ') count++;
            return count;
        }

        // Parses the inline "class: X, ns: Y, asm: Z" body of a RefIds type mapping, honouring the single-quoted
        // class names Unity writes for closed generics (e.g. 'Modifier`1[[…]]'). Mirrors the parser in
        // SerializeReferenceYamlEditor but kept local so the scanner owns its parsing.
        private static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = InlineType.Match(body);
            if (!match.Success) return false;

            var className = match.Groups["class"].Value.Replace("''", "'");
            type = new ManagedTypeName(match.Groups["asm"].Value, match.Groups["ns"].Value, className);
            return !type.IsEmpty;
        }
    }
}
