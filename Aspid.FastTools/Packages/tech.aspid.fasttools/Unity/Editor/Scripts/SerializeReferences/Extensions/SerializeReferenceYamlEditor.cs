using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Identity of a managed-reference type as it is stored in Unity's serialized YAML
    /// (<c>type: {class: …, ns: …, asm: …}</c>). Used to repair a reference whose type went missing by
    /// rewriting that line directly, since Unity's serialization API cannot reassign a missing type.
    /// </summary>
    internal readonly struct ManagedTypeName
    {
        public readonly string Assembly;
        public readonly string Namespace;
        public readonly string Class;

        public ManagedTypeName(string assembly, string @namespace, string className)
        {
            Assembly = assembly ?? string.Empty;
            Namespace = @namespace ?? string.Empty;
            Class = className ?? string.Empty;
        }

        public bool IsEmpty =>
            string.IsNullOrEmpty(Assembly) && string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Class);

        /// <summary>
        /// Builds the YAML type identity for a resolved <see cref="Type"/>, including the
        /// <c>Name`N[[arg, asm],…]</c> shape Unity uses for closed generics.
        /// </summary>
        public static ManagedTypeName FromType(Type type)
        {
            if (type is null) return default;

            var root = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return new ManagedTypeName(root.Assembly.GetName().Name, root.Namespace, BuildClassName(type));
        }

        private static string BuildClassName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var definition = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments().Select(BuildGenericArgumentName);
            return $"{definition.Name}[[{string.Join("],[", arguments)}]]";
        }

        private static string BuildGenericArgumentName(Type type) =>
            $"{BuildFullClassName(type)}, {type.Assembly.GetName().Name}";

        private static string BuildFullClassName(Type type)
        {
            if (!type.IsGenericType) return type.FullName;

            var definition = type.GetGenericTypeDefinition();
            var prefix = string.IsNullOrEmpty(definition.Namespace) ? string.Empty : $"{definition.Namespace}.";
            return $"{prefix}{BuildClassName(type)}";
        }

        /// <summary>Renders the inline YAML mapping Unity writes for a managed-reference type entry.</summary>
        public string ToYamlType() =>
            $"{{class: {EscapeInline(Class)}, ns: {EscapeInline(Namespace)}, asm: {EscapeInline(Assembly)}}}";

        // A flow-scalar containing any of , [ ] { } would break the inline mapping, so single-quote it
        // (doubling embedded quotes) exactly as Unity does for generic class names like Foo`1[[…]].
        private static string EscapeInline(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOfAny(YamlReservedChars) < 0)
                return value ?? string.Empty;

            return $"'{value.Replace("'", "''")}'";
        }

        private static readonly char[] YamlReservedChars = { ',', '[', ']', '{', '}' };
    }

    /// <summary>
    /// Rewrites the stored type of a managed reference directly in an asset's YAML text. This is the only way
    /// to re-point a <c>[SerializeReference]</c> whose type can no longer be loaded (renamed / moved / deleted),
    /// because Unity drops missing references to <see langword="null"/> through the serialization API and never
    /// exposes them for reassignment. Parser-free: the document and the target <c>RefIds</c> entry are located by
    /// line scanning, and only the inline <c>{ … }</c> on the entry's <c>type:</c> line is replaced.
    /// </summary>
    /// <summary>
    /// A single orphaned managed-reference entry found in an asset's YAML: the document it lives in
    /// (<see cref="FileId"/>), its <c>RefIds</c> id and the stored (unresolvable) type. Surfaced by the
    /// asset-level repair tool, which finds every such entry regardless of nesting depth or child object.
    /// </summary>
    internal readonly struct MissingReferenceEntry
    {
        public readonly long FileId;
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;

        public MissingReferenceEntry(long fileId, long rid, ManagedTypeName storedType)
        {
            FileId = fileId;
            Rid = rid;
            StoredType = storedType;
        }
    }

    internal static class SerializeReferenceYamlEditor
    {
        // "--- !u!114 &11400000" — object document header carrying the local file id as its YAML anchor.
        private static readonly Regex DocumentHeader = new(@"^--- !u!\d+ &(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Scans every object document in the asset and returns each <c>RefIds</c> entry whose stored type fails
        /// the <paramref name="resolves"/> predicate. Because <c>RefIds</c> is a flat per-object list, this finds
        /// missing references at any nesting depth and on any child object — without navigating the Inspector.
        /// </summary>
        public static List<MissingReferenceEntry> FindMissingReferences(string assetPath, Func<ManagedTypeName, bool> resolves)
        {
            var result = new List<MissingReferenceEntry>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = File.ReadAllLines(assetPath);

                var headers = new List<(long fileId, int start)>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var match = DocumentHeader.Match(lines[i]);
                    if (match.Success && long.TryParse(match.Groups[1].Value, out var fileId))
                        headers.Add((fileId, i));
                }

                var ridPattern = new Regex(@"^\s*-\s+rid:\s*(-?\d+)\s*$");
                var typePattern = new Regex(@"^\s*type:\s*\{(?<body>.*)\}\s*$");

                for (var h = 0; h < headers.Count; h++)
                {
                    var (fileId, start) = headers[h];
                    var end = h + 1 < headers.Count ? headers[h + 1].start : lines.Length;

                    var refIdsStart = FindRefIdsStart(lines, start, end);
                    if (refIdsStart < 0) continue;

                    for (var i = refIdsStart + 1; i < end; i++)
                    {
                        var ridMatch = ridPattern.Match(lines[i]);
                        if (!ridMatch.Success || !long.TryParse(ridMatch.Groups[1].Value, out var rid)) continue;

                        for (var j = i + 1; j < end && j <= i + 4; j++)
                        {
                            var typeMatch = typePattern.Match(lines[j]);
                            if (!typeMatch.Success) continue;

                            if (TryParseInlineType(typeMatch.Groups["body"].Value, out var type) &&
                                !type.IsEmpty && !resolves(type))
                            {
                                result.Add(new MissingReferenceEntry(fileId, rid, type));
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Best effort — a parse failure simply yields no repair candidates.
            }

            return result;
        }

        /// <summary>
        /// Replaces the <c>type:</c> mapping of the <c>RefIds</c> entry identified by <paramref name="rid"/> within
        /// the object document anchored at <paramref name="fileId"/>. Returns <see langword="true"/> when the file
        /// was rewritten; the caller is responsible for reimporting the asset.
        /// </summary>
        public static bool TryRewriteType(string assetPath, long fileId, long rid, ManagedTypeName newType)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers ("_sidearms:\n  - rid: 1002") share the "- rid:" shape with RefIds entries, so
                // confine the search to the RefIds block — the entries are the only ones with a following type:.
                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                // Match the list item "    - rid: <id>" (the leading dash distinguishes a RefIds entry from a
                // nested data "rid:" scalar).
                var ridPattern = new Regex($@"^\s*-\s+rid:\s*{rid}\s*$");
                var typePattern = new Regex(@"^(?<indent>\s*type:\s*)\{.*\}\s*$");

                for (var i = refIdsStart; i < end; i++)
                {
                    if (!ridPattern.IsMatch(lines[i])) continue;

                    // The type mapping follows the rid line; scan a few lines to tolerate formatting variance.
                    for (var j = i + 1; j < end && j <= i + 4; j++)
                    {
                        var match = typePattern.Match(lines[j]);
                        if (!match.Success) continue;

                        lines[j] = match.Groups["indent"].Value + newType.ToYamlType();
                        File.WriteAllLines(assetPath, lines);
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SerializeReferenceSelector] Failed to rewrite managed-reference type in '{assetPath}': {exception}");
                return false;
            }
        }

        /// <summary>
        /// Reads the managed-reference id (<c>rid</c>) stored at <paramref name="propertyPath"/> within the object
        /// document anchored at <paramref name="fileId"/>. Needed because Unity reports an invalid id for a property
        /// whose type is missing — the real id only survives in the YAML. Resolves the path at any depth, walking each
        /// segment either into a managed reference's <c>RefIds</c> data block (a <c>rid:</c> pointer) or down through a
        /// plain serializable container (a nested struct/class mapping or a <c>List&lt;T&gt;</c> of them), so paths
        /// such as <c>_weapon._chargeEffect</c>, <c>_config._weapon</c> and <c>_slots.Array.data[0]._weapon</c> all
        /// resolve. An unresolvable segment returns <see langword="false"/> so the caller can fall back.
        /// </summary>
        public static bool TryReadReferenceId(string assetPath, long fileId, string propertyPath, out long rid)
        {
            rid = 0;
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers (and the object's inline serializable data) live before the "references:" block; the
                // RefIds entries and the nested data each managed reference stores live after it.
                var fieldsEnd = end;
                var references = new Regex(@"^\s*references:\s*$");
                for (var i = start; i < end; i++)
                    if (references.IsMatch(lines[i])) { fieldsEnd = i; break; }

                var segments = ParsePathSegments(propertyPath.Replace(".Array.data", string.Empty));
                if (segments is null) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);

                // Cursor over the lines the current segment is resolved against. It starts on the object's own field
                // block, then for each segment either descends into a plain serializable container (a nested mapping
                // or sequence item, by indent) or jumps into a managed reference's RefIds data block (by rid).
                var cursorStart = start;
                var cursorEnd = fieldsEnd;
                var cursorIndent = -1; // the object's top-level fields: match at any indent

                for (var s = 0; s < segments.Count; s++)
                {
                    var kind = ResolveSegment(lines, cursorStart, cursorEnd, cursorIndent, segments[s],
                        out var segmentRid, out var valueStart, out var valueEnd, out var valueIndent);

                    if (kind == SegmentKind.NotFound) return false;

                    if (s == segments.Count - 1)
                    {
                        if (kind != SegmentKind.Reference) return false;
                        rid = segmentRid;
                        return true;
                    }

                    if (kind == SegmentKind.Reference)
                    {
                        if (refIdsStart < 0) return false;
                        if (!TryGetDataBlockRange(lines, refIdsStart, end, segmentRid, out cursorStart, out cursorEnd, out cursorIndent))
                            return false;
                    }
                    else
                    {
                        cursorStart = valueStart;
                        cursorEnd = valueEnd;
                        cursorIndent = valueIndent;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // A single managed-reference path segment: a field name with an optional sequence index ("_alternates[3]").
        private readonly struct PathSegment
        {
            public readonly string Name;
            public readonly bool HasIndex;
            public readonly int Index;

            public PathSegment(string name, bool hasIndex, int index)
            {
                Name = name;
                HasIndex = hasIndex;
                Index = index;
            }
        }

        // Splits a normalised property path ("_weapon._chargeEffect", "_alternates[3]") into ordered segments.
        private static List<PathSegment> ParsePathSegments(string path)
        {
            var result = new List<PathSegment>();
            foreach (var raw in path.Split('.'))
            {
                var match = Regex.Match(raw, @"^(?<name>[^\[\]\.]+)(\[(?<idx>\d+)\])?$");
                if (!match.Success) return null;

                var hasIndex = match.Groups["idx"].Success;
                result.Add(new PathSegment(match.Groups["name"].Value, hasIndex, hasIndex ? int.Parse(match.Groups["idx"].Value) : -1));
            }

            return result.Count > 0 ? result : null;
        }

        // What a resolved segment points at: a managed reference (a "rid:" pointer) or a plain serializable container
        // (a nested mapping/sequence to descend into), or nothing.
        private enum SegmentKind { NotFound, Reference, Container }

        // Resolves one path segment within [rangeStart, rangeEnd). For a plain field the value is either a managed
        // reference (its sole child is a "rid:" scalar) or a container to descend into; for an indexed field it is the
        // segment.Index-th sequence item, itself either a "- rid:" reference or a "- field:" mapping container. When
        // requiredIndent is non-negative only a field at exactly that effective indent matches (a leading "- " counts
        // toward the indent so a sequence-of-mappings item's fields align with their dashless siblings), which keeps
        // resolution on a block's direct children instead of descending into a deeper object that reuses the name.
        private static SegmentKind ResolveSegment(string[] lines, int rangeStart, int rangeEnd, int requiredIndent,
            PathSegment segment, out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var fieldPattern = new Regex($@"^(?<lead>\s*)(?<dash>-\s+)?{Regex.Escape(segment.Name)}:\s*(?<inline>.*)$");

            for (var i = rangeStart; i < rangeEnd; i++)
            {
                var field = fieldPattern.Match(lines[i]);
                if (!field.Success) continue;

                var fieldIndent = field.Groups["lead"].Length + field.Groups["dash"].Length;
                if (requiredIndent >= 0 && fieldIndent != requiredIndent) continue;

                return segment.HasIndex
                    ? ResolveSequenceItem(lines, i + 1, rangeEnd, segment.Index, out rid, out valueStart, out valueEnd, out valueIndent)
                    : ClassifyValue(lines, i, fieldIndent, field.Groups["inline"].Value, rangeEnd, out rid, out valueStart, out valueEnd, out valueIndent);
            }

            return SegmentKind.NotFound;
        }

        // Classifies the value of a plain (non-indexed) field at line i with effective indent fieldIndent: a managed
        // reference when the value is a lone "rid:" scalar (inline or as the only following child), otherwise the
        // indented mapping block to descend into.
        private static SegmentKind ClassifyValue(string[] lines, int i, int fieldIndent, string inline, int rangeEnd,
            out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var inlineMatch = Regex.Match(inline, @"rid:\s*(-?\d+)");
            if (inlineMatch.Success)
                return long.TryParse(inlineMatch.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

            // Gather the indented value block (lines more indented than the field).
            var blockStart = i + 1;
            var blockEnd = rangeEnd;
            var firstChild = -1;
            for (var j = blockStart; j < rangeEnd; j++)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (IndentOf(lines[j]) <= fieldIndent) { blockEnd = j; break; }
                if (firstChild < 0) firstChild = j;
            }

            if (firstChild < 0) return SegmentKind.NotFound; // scalar/empty field: no managed reference here

            // A managed reference's value block is exactly a "rid:" scalar; anything else is a container.
            var ridScalar = Regex.Match(lines[firstChild].Trim(), @"^rid:\s*(-?\d+)$");
            if (ridScalar.Success)
                return long.TryParse(ridScalar.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

            valueStart = blockStart;
            valueEnd = blockEnd;
            valueIndent = IndentOf(lines[firstChild]);
            return SegmentKind.Container;
        }

        // Locates the index-th "- " item of a sequence whose items begin at [itemsStart, rangeEnd). A "- rid: N" item
        // is a managed reference; a "- field: …" item is a mapping container whose fields begin on the dash line.
        private static SegmentKind ResolveSequenceItem(string[] lines, int itemsStart, int rangeEnd, int index,
            out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var itemPattern = new Regex(@"^(?<lead>\s*)-\s");
            var itemIndent = -1;
            var count = 0;

            for (var j = itemsStart; j < rangeEnd; j++)
            {
                if (lines[j].Trim().Length == 0) continue;

                var item = itemPattern.Match(lines[j]);
                if (!item.Success)
                {
                    if (itemIndent >= 0 && IndentOf(lines[j]) <= itemIndent) break; // dedented out of the sequence
                    continue;
                }

                var indent = item.Groups["lead"].Length;
                if (itemIndent < 0) itemIndent = indent;
                else if (indent < itemIndent) break;    // dedented out of the sequence
                else if (indent > itemIndent) continue; // item of a nested sequence, not ours

                if (count == index)
                {
                    var ridMatch = Regex.Match(lines[j].TrimStart(), @"^-\s+rid:\s*(-?\d+)\s*$");
                    if (ridMatch.Success)
                        return long.TryParse(ridMatch.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

                    // Mapping item: it runs until the next sibling "- " or a dedent; its fields start one "- " past
                    // the item indent.
                    var itemEnd = rangeEnd;
                    for (var k = j + 1; k < rangeEnd; k++)
                    {
                        if (lines[k].Trim().Length == 0) continue;
                        var ind = IndentOf(lines[k]);
                        if (ind < itemIndent || (ind == itemIndent && itemPattern.IsMatch(lines[k]))) { itemEnd = k; break; }
                    }

                    valueStart = j;
                    valueEnd = itemEnd;
                    valueIndent = itemIndent + 2;
                    return SegmentKind.Container;
                }

                count++;
            }

            return SegmentKind.NotFound;
        }

        // Locates the RefIds entry for rid and returns the line range of its "data:" block plus the indent of that
        // block's direct children, so a nested segment can be resolved within the right scope.
        private static bool TryGetDataBlockRange(string[] lines, int refIdsStart, int docEnd, long rid, out int blockStart, out int blockEnd, out int childIndent)
        {
            blockStart = blockEnd = childIndent = -1;
            var ridPattern = new Regex($@"^(?<indent>\s*)-\s+rid:\s*{rid}\s*$");
            var dataPattern = new Regex(@"^\s*data:\s*$");

            for (var i = refIdsStart; i < docEnd; i++)
            {
                var match = ridPattern.Match(lines[i]);
                if (!match.Success) continue;

                // The entry runs until the next list item at its own indent, or until the block dedents out of it.
                var entryIndent = match.Groups["indent"].Length;
                var entryEnd = docEnd;
                for (var j = i + 1; j < docEnd; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;
                    var indent = IndentOf(lines[j]);
                    if (indent < entryIndent || (indent == entryIndent && lines[j].TrimStart().StartsWith("- "))) { entryEnd = j; break; }
                }

                for (var j = i + 1; j < entryEnd; j++)
                {
                    if (!dataPattern.IsMatch(lines[j])) continue;

                    blockStart = j + 1;
                    blockEnd = entryEnd;
                    for (var k = blockStart; k < blockEnd; k++)
                        if (lines[k].Trim().Length > 0) { childIndent = IndentOf(lines[k]); break; }

                    return blockStart < blockEnd && childIndent >= 0;
                }

                return false;
            }

            return false;
        }

        private static int IndentOf(string line)
        {
            var count = 0;
            while (count < line.Length && line[count] == ' ') count++;
            return count;
        }

        /// <summary>
        /// Reads the managed-reference id stored at <paramref name="propertyPath"/> and the type recorded for it in
        /// the <c>RefIds</c> block, in a single pass over the asset YAML. This is how a missing reference is found
        /// even when Unity has dropped it from the live object (notably on prefabs / GameObjects): the orphaned
        /// id, type identity and payload all survive in the file.
        /// </summary>
        public static bool TryReadStoredType(string assetPath, long fileId, string propertyPath, out long rid, out ManagedTypeName type)
        {
            rid = 0;
            type = default;

            if (!TryReadReferenceId(assetPath, fileId, propertyPath, out rid)) return false;

            try
            {
                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                var ridPattern = new Regex($@"^\s*-\s+rid:\s*{rid}\s*$");
                var typePattern = new Regex(@"^\s*type:\s*\{(?<body>.*)\}\s*$");

                for (var i = refIdsStart; i < end; i++)
                {
                    if (!ridPattern.IsMatch(lines[i])) continue;

                    for (var j = i + 1; j < end && j <= i + 4; j++)
                    {
                        var match = typePattern.Match(lines[j]);
                        if (!match.Success) continue;

                        return TryParseInlineType(match.Groups["body"].Value, out type);
                    }

                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Parses the inline "class: X, ns: Y, asm: Z" body of a RefIds type mapping, honouring single-quoted values
        // (Unity quotes generic class names such as 'Modifier`1[[…]]').
        private static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = Regex.Match(body,
                @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$");
            if (!match.Success) return false;

            var className = match.Groups["class"].Value.Replace("''", "'");
            type = new ManagedTypeName(match.Groups["asm"].Value, match.Groups["ns"].Value, className);
            return !type.IsEmpty;
        }

        // Returns the [start, end) line range of the document whose anchor equals fileId. Falls back to the single
        // document of a one-object asset (the common ScriptableObject case) when the anchor cannot be matched.
        private static (int start, int end) FindDocumentRange(string[] lines, long fileId)
        {
            var start = -1;
            var end = lines.Length;
            var headerCount = 0;
            var firstHeader = -1;

            for (var i = 0; i < lines.Length; i++)
            {
                var match = DocumentHeader.Match(lines[i]);
                if (!match.Success) continue;

                headerCount++;
                if (firstHeader < 0) firstHeader = i;

                if (start >= 0)
                {
                    end = i;
                    break;
                }

                if (long.TryParse(match.Groups[1].Value, out var anchor) && anchor == fileId)
                    start = i;
            }

            if (start >= 0) return (start, end);
            if (headerCount == 1) return (firstHeader, lines.Length);
            return (-1, -1);
        }

        // Index of the "RefIds:" key line within [start, end), or -1 when the document has no managed references.
        private static int FindRefIdsStart(string[] lines, int start, int end)
        {
            var refIds = new Regex(@"^\s*RefIds:\s*$");
            for (var i = start; i < end; i++)
                if (refIds.IsMatch(lines[i]))
                    return i;

            return -1;
        }
    }
}
