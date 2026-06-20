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
    /// e.g. <c>_weapon</c> or <c>_alternates[0]</c> or <c>_config._slots[2]</c>. A field that holds nothing (its
    /// pointer is Unity's null sentinel) is kept as an <see cref="IsEmpty"/> root so an unassigned / cleared slot
    /// stays visible in the graph rather than silently dropping out — it has no <c>RefIds</c> node behind it.
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

        /// <summary>True when the pointer is a null sentinel (rid &lt; 0) — an unassigned [SerializeReference] slot.</summary>
        public bool IsEmpty => Rid < 0;
    }

    /// <summary>
    /// A parent → child edge inside a document's nested graph: the child <c>RefIds</c> id and the field path that
    /// holds it <i>relative to the parent's data block</i>, with list elements indexed (e.g. <c>_chargeEffect</c> or
    /// <c>_slots[0].weapon</c>). The view joins this onto the parent's full path so a nested reference shows where it
    /// lives from the document root down. A null child slot is kept as an <see cref="IsEmpty"/> edge (the rid is a null
    /// sentinel) so a cleared nested field is visible too; it points at no node and never recurses.
    /// </summary>
    internal readonly struct ReferenceGraphEdge
    {
        public readonly long Rid;
        public readonly string Label;

        public ReferenceGraphEdge(long rid, string label)
        {
            Rid = rid;
            Label = label;
        }

        /// <summary>True when the child pointer is a null sentinel (rid &lt; 0) — an unassigned nested slot.</summary>
        public bool IsEmpty => Rid < 0;
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

        // Parent rid → ordered, de-duplicated child edges of the nested graph (data-block pointers only; roots are
        // tracked separately in Roots). Each edge carries the child rid and the field path holding it relative to the
        // parent's data block. Empty (null-sentinel) child slots are kept here too so a cleared nested field surfaces.
        public readonly Dictionary<long, List<ReferenceGraphEdge>> Edges = new();

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

        public IReadOnlyList<ReferenceGraphEdge> ChildrenOf(long rid) =>
            Edges.TryGetValue(rid, out var children) ? children : Array.Empty<ReferenceGraphEdge>();
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

                // Negative rids are Unity's sentinels, not managed objects: -2 is the shared null entry
                // (ManagedReferenceUtility.RefIdNull) Unity writes for any null [SerializeReference] field, -1 is unknown.
                // They carry an empty type and no payload, so skip them — a field pointing at one is simply null and
                // must not surface as a "rid -2" node (the pointers then read as dangling and drop out of the graph).
                if (rid < 0) continue;

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

                    // A null field pointer (rid -2 = RefIdNull, -1 = unknown): the [SerializeReference] slot exists but
                    // holds nothing. Keep it as an empty root so a cleared / never-assigned field stays visible in the
                    // graph — there is no RefIds node behind it, so the view renders it as an "<None>" leaf. ShortName /
                    // shared / orphan computations skip these sentinels (see ComputeSharedAndOrphans).
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

        // Within each RefIds entry's "data:" block, every "rid:" pointer is a parent → child edge. Each edge records the
        // field path holding the child relative to the parent's data block (BuildEdgePath), so the view can show a
        // nested reference's full path. A null child slot (rid -2) is kept as an empty edge so a cleared nested field
        // surfaces. The entry's own "- rid:" header line is skipped so an entry is never recorded as its own child.
        private static void CollectEdges(string[] lines, int refIdsStart, int end, HashSet<long> knownRids, ReferenceGraphDocument document)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var ridMatch = EntryRid.Match(lines[i]);
                if (!ridMatch.Success || !long.TryParse(ridMatch.Groups["id"].Value, out var parent)) continue;
                if (parent < 0) continue; // a sentinel entry carries no data block of its own

                var entryIndent = ridMatch.Groups["indent"].Length;
                var entryEnd = FindEntryEnd(lines, i + 1, end, entryIndent);

                var dataStart = FindKey(lines, DataKey, i + 1, entryEnd);
                if (dataStart < 0) continue;

                for (var j = dataStart + 1; j < entryEnd; j++)
                {
                    foreach (Match match in RidPointer.Matches(lines[j]))
                    {
                        if (!long.TryParse(match.Groups["id"].Value, out var child)) continue;
                        if (child == parent) continue;

                        // A null nested slot (rid < 0): keep it as an empty edge so a cleared nested [SerializeReference]
                        // field is visible, but it points at no node so it never recurses. A real child must be a known
                        // RefIds node; a dangling pointer is dropped.
                        if (child >= 0 && !knownRids.Contains(child)) continue;

                        AddEdge(document, parent, new ReferenceGraphEdge(child, BuildEdgePath(lines, j, dataStart)));
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

            // Null sentinels (empty roots / edges) are excluded throughout: every cleared field points at the same -2,
            // so counting them would wrongly flag -2 as "shared", and -2 is not a node so it is never reachable/orphan.
            foreach (var root in document.Roots)
                if (root.Rid >= 0) parentCount[root.Rid] = parentCount.GetValueOrDefault(root.Rid) + 1;

            foreach (var pair in document.Edges)
            foreach (var edge in pair.Value)
                if (edge.Rid >= 0) parentCount[edge.Rid] = parentCount.GetValueOrDefault(edge.Rid) + 1;

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

            // A real child rid de-dups (an alias within one parent's data block counts once); empty slots are distinct
            // fields that happen to share the -2 sentinel, so they are always kept.
            if (edge.Rid >= 0 && children.Exists(c => c.Rid == edge.Rid)) return;
            children.Add(edge);
        }

        // A root pointer's full field path from the document body down to the rid pointer on line i, e.g. "_weapon",
        // "_alternates[0]" or "_config._slots[2]". Climbs to (but excludes) the indent-0 document wrapper key, so a
        // top-level field reads as "_weapon", never the wrapper name. Falls back to "reference" when no key is found.
        private static string BuildRootPath(string[] lines, int i, int start)
        {
            var path = BuildPath(lines, i, floor: start, stopIndent: 0);
            return string.IsNullOrEmpty(path) ? "reference" : path;
        }

        // A nested edge's field path relative to its parent's "data:" block, e.g. "_chargeEffect" or "_slots[0].weapon".
        // Climbs to (but excludes) the "data:" key by stopping at the data block's own indent, so the path is parent-
        // relative; the view joins it onto the parent's full path. Empty when no key is found (the view then keeps the
        // parent path alone).
        private static string BuildEdgePath(string[] lines, int pointerLine, int dataStart) =>
            BuildPath(lines, pointerLine, floor: dataStart, stopIndent: IndentOf(lines[dataStart]));

        // Builds a dotted field path by walking up from the rid pointer on <paramref name="pointerLine"/>, collecting
        // mapping keys (with a "[index]" suffix on any list key crossed), until it climbs out of the enclosing scope.
        // <paramref name="floor"/> is the inclusive lowest line the walk may inspect (the document header for a body
        // root, the "data:" line for a nested edge); <paramref name="stopIndent"/> is the indent at or below which the
        // walk stops, so it never crosses the scope's own wrapper key (indent-0 wrapper for a root, the "data:" key for
        // an edge). Returns the dotted path, or an empty string when no path segment is found. Two YAML shapes are
        // handled per ancestor:
        // • a named value over two lines ("_weapon:\n  rid: N") — the owning key is the nearest strictly-shallower line.
        // • a block-sequence item ("_alternates:\n  - rid: N") — Unity writes the dashes at the same column as the list
        //   key, so the key sits at *equal* indent above the dash; the element's index is its position among the same-
        //   indent "- …" siblings. An element that carries its own field key on the dash line ("- _weapon:") keeps it,
        //   so the path reads "_slots[0]._weapon".
        private static string BuildPath(string[] lines, int pointerLine, int floor, int stopIndent)
        {
            var segments = new List<string>();
            var line = pointerLine;

            // YAML nesting is finite; the counter only guards a malformed file from looping the walk forever.
            for (var safety = 0; line > floor && safety < 256; safety++)
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
                    for (var j = line - 1; j >= floor; j--)
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
                    next = ParentLine(lines, ownerLine, floor, IndentOf(lines[ownerLine]));
                }
                else
                {
                    // A mapping line: record its key, or climb past a structural "rid:"/"data:" line that contributes
                    // no path segment of its own.
                    var match = MappingKey.Match(lines[line]);
                    if (match.Success && !IsStructuralKey(match.Groups["key"].Value))
                        segments.Add(match.Groups["key"].Value);

                    next = ParentLine(lines, line, floor, indent);
                }

                if (next < 0 || IndentOf(lines[next]) <= stopIndent) break; // climbed out of the enclosing scope
                line = next;
            }

            if (segments.Count == 0) return string.Empty;

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

            // Scenes are unreadable through LoadAllAssetsAtPath (see SerializeReferenceHelpers.IsScene); their
            // documents simply fall back to the YAML class id label.
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
