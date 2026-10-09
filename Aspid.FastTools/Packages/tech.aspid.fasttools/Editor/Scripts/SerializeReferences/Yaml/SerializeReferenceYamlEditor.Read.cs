using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        // A path read from the object's own fields rather than from a reference's data block.
        public const long NoAnchor = long.MinValue;

        private static readonly Regex _entryTypeBody = new(@"^\s*type:\s*\{(?<body>.*)\}\s*$", RegexOptions.Compiled);

        // Reads the rid stored at a property path, which only the YAML still carries: Unity reports an invalid id
        // for a property whose type is missing. Each path segment walks either into a managed reference's data block
        // or down through a plain serializable container, so a path at any depth resolves.
        public static bool TryReadReferenceId(string assetPath, long fileId, string propertyPath, out long rid)
        {
            rid = 0;
            try
            {
                var file = SerializeReferenceYamlProbeCache.Read(assetPath);
                return file is not null && TryReadReferenceId(file, fileId, NoAnchor, propertyPath, out rid);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Reads the rid and its recorded type in one pass. This is how a missing reference is found even after
        // Unity drops it from the live object: the orphaned id, type and payload all survive in the file.
        public static bool TryReadStoredType(string assetPath, long fileId, string propertyPath, out long rid, out ManagedTypeName type) =>
            TryReadStoredType(assetPath, fileId, NoAnchor, propertyPath, out rid, out type);

        // An anchorRid other than NoAnchor reads the path inside that reference's data block, so a field of a reference is
        // found by the reference's id, whichever list slot holds the reference now. The result is kept with the file
        // version: a repaint does not parse an unchanged file again.
        public static bool TryReadStoredType(string assetPath, long fileId, long anchorRid, string propertyPath,
            out long rid, out ManagedTypeName type)
        {
            rid = 0;
            type = default;

            try
            {
                var file = SerializeReferenceYamlProbeCache.Read(assetPath);
                if (file is null) return false;

                var key = (fileId, anchorRid, propertyPath);
                if (!file.StoredTypes.TryGetValue(key, out var read))
                {
                    read.found = TryReadReferenceId(file, fileId, anchorRid, propertyPath, out read.rid)
                        && TryGetEntryType(file, fileId, read.rid, out read.type);

                    file.StoredTypes[key] = read;
                }

                rid = read.rid;
                type = read.type;
                return read.found;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // The element ids of the list of references at listPath, read like TryReadStoredType; a null element reads as -2.
        // False when the path does not end at such a list. The array is shared, so callers must treat it as read-only.
        public static bool TryReadListIds(string assetPath, long fileId, long anchorRid, string listPath, out long[] rids)
        {
            rids = null;

            try
            {
                var file = SerializeReferenceYamlProbeCache.Read(assetPath);
                if (file is null) return false;

                var key = (fileId, anchorRid, listPath);
                if (!file.ListIds.TryGetValue(key, out rids))
                {
                    var read = new List<long>();
                    rids = TryReadListIds(file, fileId, anchorRid, listPath, read) ? read.ToArray() : null;
                    file.ListIds[key] = rids;
                }

                return rids is not null;
            }
            catch (Exception)
            {
                rids = null;
                return false;
            }
        }

        // The type recorded in the RefIds entry of rid. False for a rid without an entry or without a readable type.
        public static bool TryReadEntryType(string assetPath, long fileId, long rid, out ManagedTypeName type)
        {
            type = default;

            try
            {
                var file = SerializeReferenceYamlProbeCache.Read(assetPath);
                return file is not null && TryGetEntryType(file, fileId, rid, out type);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryReadReferenceId(SerializeReferenceYamlProbeCache.ProbedFile file, long fileId, long anchorRid,
            string propertyPath, out long rid)
        {
            rid = 0;

            var lines = file.Lines;
            var (start, end) = file.FindDocumentRange(fileId);
            if (start < 0) return false;

            var segments = ParsePathSegments(propertyPath.Replace(".Array.data", string.Empty));
            if (segments is null) return false;

            var last = segments.Count - 1;
            if (!TryDescend(lines, start, end, anchorRid, segments, last, out var cursorStart, out var cursorEnd, out var cursorIndent))
                return false;

            return ResolveSegment(lines, cursorStart, cursorEnd, cursorIndent, segments[last], out rid, out _, out _, out _)
                == SegmentKind.Reference;
        }

        private static bool TryReadListIds(SerializeReferenceYamlProbeCache.ProbedFile file, long fileId, long anchorRid,
            string listPath, List<long> rids)
        {
            var lines = file.Lines;
            var (start, end) = file.FindDocumentRange(fileId);
            if (start < 0) return false;

            var segments = ParsePathSegments(listPath.Replace(".Array.data", string.Empty));
            if (segments is null) return false;

            var last = segments.Count - 1;
            if (segments[last].HasIndex) return false;

            return TryDescend(lines, start, end, anchorRid, segments, last, out var cursorStart, out var cursorEnd, out var cursorIndent)
                && TryCollectListIds(lines, cursorStart, cursorEnd, cursorIndent, segments[last].Name, rids);
        }

        // Resolves the first count segments and returns the lines the next segment is resolved in. The path starts at the
        // object's own fields, or in the data block of anchorRid.
        private static bool TryDescend(string[] lines, int start, int end, long anchorRid, List<PathSegment> segments, int count,
            out int cursorStart, out int cursorEnd, out int cursorIndent)
        {
            cursorStart = cursorEnd = cursorIndent = -1;

            // Field pointers (and the object's inline serializable data) live before the "references:" block; the
            // RefIds entries and the nested data each managed reference stores live after it.
            var refIdsStart = FindRefIdsStart(lines, start, end);

            if (anchorRid != NoAnchor)
            {
                if (refIdsStart < 0) return false;
                if (!TryGetDataBlockRange(lines, refIdsStart, end, anchorRid, out cursorStart, out cursorEnd, out cursorIndent))
                    return false;
            }
            else
            {
                cursorStart = start;
                cursorEnd = end;
                for (var i = start; i < end; i++)
                    if (_referencesKey.IsMatch(lines[i])) { cursorEnd = i; break; }

                // The object's top-level fields all align with the m_Script line's indent (see TryReadScriptGuid), so
                // the first segment is matched at exactly that indent — otherwise a same-named key nested inside an
                // earlier field's serializable container would shadow the real top-level field. A document without a
                // readable script guid falls back to matching at any indent.
                cursorIndent = TryReadScriptGuid(lines, start + 1, cursorEnd, out _, out var fieldIndent)
                    ? fieldIndent
                    : -1;
            }

            // Each segment either descends into a plain serializable container (a nested mapping or sequence item, by
            // indent) or jumps into a managed reference's RefIds data block (by rid).
            for (var s = 0; s < count; s++)
            {
                var kind = ResolveSegment(lines, cursorStart, cursorEnd, cursorIndent, segments[s],
                    out var segmentRid, out var valueStart, out var valueEnd, out var valueIndent);

                if (kind == SegmentKind.NotFound) return false;

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

            return true;
        }

        // The "- rid: N" items of the list keyed name among the range's direct children; an inline "[]" is an empty list.
        // False when the key is absent or holds anything other than a list of references.
        private static bool TryCollectListIds(string[] lines, int rangeStart, int rangeEnd, int requiredIndent, string name,
            List<long> rids)
        {
            var fieldPattern = new Regex($@"^(?<lead>\s*)(?<dash>-\s+)?{Regex.Escape(name)}:\s*(?<inline>.*?)\s*$");

            for (var i = rangeStart; i < rangeEnd; i++)
            {
                var field = fieldPattern.Match(lines[i]);
                if (!field.Success) continue;

                var fieldIndent = field.Groups["lead"].Length + field.Groups["dash"].Length;
                if (requiredIndent >= 0 && fieldIndent != requiredIndent) continue;

                var inline = field.Groups["inline"].Value;
                if (inline.Length > 0) return inline == "[]";

                // Unity writes the items at the key's own indent.
                for (var j = i + 1; j < rangeEnd; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;

                    var item = _listItem.Match(lines[j]);
                    if (item.Success && item.Groups["lead"].Length == fieldIndent)
                    {
                        if (!long.TryParse(item.Groups["rid"].Value, out var rid)) return false;

                        rids.Add(rid);
                        continue;
                    }

                    // A dedent or the next key ends the list; an item of any other shape is not a reference.
                    var indent = IndentOf(lines[j]);
                    if (indent < fieldIndent) break;
                    if (indent == fieldIndent && !lines[j].TrimStart().StartsWith("-", StringComparison.Ordinal)) break;

                    return false;
                }

                return true;
            }

            return false;
        }

        private static bool TryGetEntryType(SerializeReferenceYamlProbeCache.ProbedFile file, long fileId, long rid,
            out ManagedTypeName type)
        {
            if (!file.EntryTypes.TryGetValue(fileId, out var types))
                file.EntryTypes[fileId] = types = IndexEntryTypes(file, fileId);

            return types.TryGetValue(rid, out type) && !type.IsEmpty;
        }

        // The stored type of each RefIds entry of the document, read in one pass; an entry without a readable type maps
        // to an empty name. The first entry of a rid wins, as FindEntryHeader finds it.
        private static Dictionary<long, ManagedTypeName> IndexEntryTypes(SerializeReferenceYamlProbeCache.ProbedFile file, long fileId)
        {
            var types = new Dictionary<long, ManagedTypeName>();
            var lines = file.Lines;

            var (start, end) = file.FindDocumentRange(fileId);
            if (start < 0) return types;

            var refIdsStart = FindRefIdsStart(lines, start, end);
            if (refIdsStart < 0) return types;

            var entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
            if (entryIndent < 0) return types;

            for (var i = refIdsStart + 1; i < end; i++)
            {
                if (!SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var rid)) continue;

                var entryEnd = FindEntryEnd(lines, i, end, entryIndent);
                if (!types.ContainsKey(rid))
                {
                    var typeLine = FindEntryTypeLine(lines, i, entryEnd);
                    var match = typeLine >= 0 ? _entryTypeBody.Match(lines[typeLine]) : Match.Empty;

                    types[rid] = match.Success && TryParseInlineType(match.Groups["body"].Value, out var type) ? type : default;
                }

                i = entryEnd - 1;
            }

            return types;
        }

        public static List<string> ParseTopLevelFieldNames(string serializedData)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(serializedData)) return result;

            foreach (var raw in serializedData.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length == 0 || char.IsWhiteSpace(line[0]) || line[0] == '-') continue;

                var separator = line.IndexOf(':');
                if (separator <= 0) continue;

                var key = line[..separator].Trim();
                if (key.Length > 0) result.Add(key);
            }

            return result;
        }

        public static List<string> GetReferenceFieldNames(string assetPath, long fileId, long rid)
        {
            var result = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return result;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0 || !TryGetDataBlockRange(lines, refIdsStart, end, rid, out var blockStart, out var blockEnd, out var childIndent)) return result;
                CollectTopLevelKeys(lines, blockStart, blockEnd, childIndent, result);
            }
            catch (Exception)
            {
                // Best effort — an unreadable block simply yields no field-shape signal.
            }

            return result;
        }

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

        // Resolves one path segment within [rangeStart, rangeEnd): a "rid:" value is a managed reference, anything
        // else a container to descend into; an indexed segment resolves the Index-th sequence item. A non-negative
        // requiredIndent matches only the block's direct children (a leading "- " counts toward the indent), so a
        // deeper object that reuses the name is never picked up.
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

            var blockStart = i + 1;
            var blockEnd = rangeEnd;
            var firstChild = -1;

            for (var j = blockStart; j < rangeEnd; j++)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (IndentOf(lines[j]) <= fieldIndent) { blockEnd = j; break; }
                if (firstChild < 0) firstChild = j;
            }

            if (firstChild < 0)
                return SegmentKind.NotFound;

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
        private static SegmentKind ResolveSequenceItem(string[] lines, int itemsStart, int rangeEnd, int index, out long rid, out int valueStart, out int valueEnd, out int valueIndent)
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
                    if (itemIndent >= 0 && IndentOf(lines[j]) <= itemIndent) break;
                    continue;
                }

                var indent = item.Groups["lead"].Length;

                if (itemIndent < 0) itemIndent = indent;
                else if (indent < itemIndent) break;
                else if (indent > itemIndent) continue;

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

                        if (ind < itemIndent || (ind == itemIndent && itemPattern.IsMatch(lines[k])))
                        {
                            itemEnd = k;
                            break;
                        }
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
            var dataPattern = new Regex(@"^\s*data:\s*$");

            var headerIndex = FindEntryHeader(lines, refIdsStart, docEnd, rid, out var entryIndent);
            if (headerIndex < 0) return false;

            var entryEnd = FindEntryEnd(lines, headerIndex, docEnd, entryIndent);

            for (var j = headerIndex + 1; j < entryEnd; j++)
            {
                if (!dataPattern.IsMatch(lines[j])) continue;

                blockStart = j + 1;
                blockEnd = entryEnd;

                for (var k = blockStart; k < blockEnd; k++)
                {
                    if (lines[k].Trim().Length > 0)
                    {
                        childIndent = IndentOf(lines[k]);
                        break;
                    }
                }

                return blockStart < blockEnd && childIndent >= 0;
            }

            return false;
        }

        // Collects the keys of "name: …" entries at exactly childIndent within [blockStart, blockEnd), skipping the
        // deeper lines of nested mappings/sequences so only the block's own top-level fields are reported. Sequence
        // items ("- rid: 7", "- _damage: 5") sit at the parent key's own indent, so the key pattern excludes the dash
        // — they are items of an already-reported field, not keys ("- rid" / "- _damage" would be pseudo-keys).
        private static void CollectTopLevelKeys(string[] lines, int blockStart, int blockEnd, int childIndent, List<string> result)
        {
            var keyPattern = new Regex(@"^(?<indent>\s*)(?<key>[^\s:#-][^:]*):(\s.*|\s*)$");

            for (var i = blockStart; i < blockEnd; i++)
            {
                if (lines[i].Trim().Length == 0) continue;
                if (IndentOf(lines[i]) != childIndent) continue;

                var match = keyPattern.Match(lines[i]);
                if (!match.Success) continue;
                if (match.Groups["indent"].Length != childIndent) continue;

                result.Add(match.Groups["key"].Value.Trim());
            }
        }

        // What a resolved segment points at: a managed reference (a "rid:" pointer) or a plain serializable container
        // (a nested mapping/sequence to descend into), or nothing.
        private enum SegmentKind
        {
            NotFound,
            Reference,
            Container
        }

        // A single managed-reference path segment: a field name with an optional sequence index ("_alternates[3]").
        private readonly struct PathSegment
        {
            public readonly int Index;
            public readonly string Name;
            public readonly bool HasIndex;

            public PathSegment(string name, bool hasIndex, int index)
            {
                Name = name;
                Index = index;
                HasIndex = hasIndex;
            }
        }
    }
}
