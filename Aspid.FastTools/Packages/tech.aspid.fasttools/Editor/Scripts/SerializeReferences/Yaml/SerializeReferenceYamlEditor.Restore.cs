using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        private static readonly System.Random _ridRandom = new();

        private static readonly Regex _anyRid = new(@"rid:\s*(?<rid>-?\d+)", RegexOptions.Compiled);

        // Captures the full RefIds entry block behind a top-level array element, verbatim indentation and all — the
        // exact text needed to re-materialize it later, since a list resize collapses a named missing rid into the
        // anonymous sentinel.
        public static bool TryReadArrayElementEntryBlock(string assetPath, long fileId, string elementPath,
            out long rid, out List<string> entryLines)
        {
            rid = 0;
            entryLines = null;

            try
            {
                if (!TryParseTopLevelArrayElement(elementPath, out _, out _)) return false;
                if (!TryReadReferenceId(assetPath, fileId, elementPath, out rid)) return false;
                if (rid < 0) return false;

                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                var headerIndex = FindEntryHeader(lines, refIdsStart, end, rid, out var entryIndent);
                if (headerIndex < 0) return false;

                var entryEnd = FindEntryEnd(lines, headerIndex, end, entryIndent);

                var captured = new List<string>(entryEnd - headerIndex);
                for (var k = headerIndex; k < entryEnd; k++) captured.Add(lines[k]);

                if (captured.Count < 2) return false;

                entryLines = captured;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to read RefIds entry for '{elementPath}' in '{assetPath}': {exception}");
                return false;
            }
        }

        // Re-points the array element at a free rid and re-inserts the captured entry under it. It acts only while the
        // element holds a null id, so a slot the user has since re-assigned is never clobbered. The caller reimports
        // the asset.
        public static bool TryRestoreArrayElementReference(string assetPath, long fileId, string elementPath,
            IReadOnlyList<string> entryLines)
        {
            if (!TryParseTopLevelArrayElement(elementPath, out var fieldName, out var elementIndex)) return false;

            var restore = new ArrayElementRestore(fileId, entryLines);
            restore.Slots.Add((fieldName, elementIndex));

            return RestoreArrayElementReferences(assetPath, new[] { restore }).Count > 0;
        }

        // The element pointers of a top-level array field, in order. Read straight from disk, never through the probe
        // cache, so a file rewritten within the cache's timestamp resolution is never read stale.
        public static bool TryReadTopLevelArrayRids(string assetPath, long fileId, string fieldName, out List<long> rids)
        {
            rids = null;

            try
            {
                if (string.IsNullOrEmpty(fieldName)) return false;
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return false;

                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                var found = new List<long>();
                if (!TryCollectArrayElementPointers(lines, start, refIdsStart, fieldName, null, found)) return false;

                rids = found;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to read array '{fieldName}' in '{assetPath}': {exception}");
                return false;
            }
        }

        public static bool TryFindTopLevelArrayElementForRid(string assetPath, long fileId, long rid,
            out string field, out int index)
        {
            field = null;
            index = -1;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var fieldsEnd = FindFieldsEnd(lines, start, end);

                var headerPattern = new Regex(@"^(?<lead>\s*)(?<name>[^\s:#-][^:]*):\s*$");
                var itemPattern = new Regex(@"^(?<lead>\s*)-\s+rid:\s*(?<rid>-?\d+)\s*$");

                // Only the object's own fields, at the m_Script indent, own a top-level array; a same-named list nested
                // in an earlier field's container is skipped. Without a script guid any indent is accepted.
                var topIndent = TryReadScriptGuid(lines, start + 1, fieldsEnd, out _, out var scriptIndent)
                    ? scriptIndent
                    : -1;

                string currentField = null;
                var fieldIndent = -1;
                var count = 0;

                for (var i = start; i < fieldsEnd; i++)
                {
                    if (lines[i].Trim().Length == 0) continue;

                    var item = itemPattern.Match(lines[i]);
                    if (item.Success && currentField != null && item.Groups["lead"].Length == fieldIndent)
                    {
                        if (long.TryParse(item.Groups["rid"].Value, out var elementRid) && elementRid == rid)
                        {
                            field = currentField;
                            index = count;
                            return true;
                        }

                        count++;
                        continue;
                    }

                    var header = headerPattern.Match(lines[i]);
                    if (header.Success && (topIndent < 0 || header.Groups["lead"].Length == topIndent))
                    {
                        currentField = header.Groups["name"].Value;
                        fieldIndent = header.Groups["lead"].Length;
                        count = 0;
                    }
                }

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to locate array element for rid {rid} in '{assetPath}': {exception}");
                return false;
            }
        }

        // Parses "_field.Array.data[N]" into (field, index). Returns false for a nested path (e.g. an array inside a
        // managed reference's data block) — the restore only re-points a direct top-level array element, which is the
        // shape Unity's default list "+" destroys; deeper paths are left to a future pass rather than risk a mis-target.
        private static bool TryParseTopLevelArrayElement(string elementPath, out string fieldName, out int index)
        {
            fieldName = null;
            index = -1;
            if (string.IsNullOrEmpty(elementPath)) return false;

            var segments = ParsePathSegments(elementPath.Replace(".Array.data", string.Empty));
            if (segments is null || segments.Count != 1) return false;

            var segment = segments[0];
            if (!segment.HasIndex || segment.Index < 0) return false;

            fieldName = segment.Name;
            index = segment.Index;
            return true;
        }

        // Collects the "- rid: N" element lines of the top-level array field; false when the field is not found or an
        // element id does not parse. pointerLines is optional.
        private static bool TryCollectArrayElementPointers(string[] lines, int start, int fieldsEnd, string fieldName,
            List<int> pointerLines, List<long> rids)
        {
            var fieldPattern = new Regex($@"^(?<lead>\s*){Regex.Escape(fieldName)}:\s*$");
            var itemPattern = new Regex(@"^(?<lead>\s*)-\s+rid:\s*(?<rid>-?\d+)\s*$");

            // The field key is matched at the m_Script indent, as in TryReadReferenceId, so a same-named list nested in
            // an earlier field's container never receives the restored pointer.
            var topIndent = TryReadScriptGuid(lines, start + 1, fieldsEnd, out _, out var scriptIndent)
                ? scriptIndent
                : -1;

            for (var i = start; i < fieldsEnd; i++)
            {
                var field = fieldPattern.Match(lines[i]);
                if (!field.Success) continue;

                var fieldIndent = field.Groups["lead"].Length;
                if (topIndent >= 0 && fieldIndent != topIndent) continue;

                for (var j = i + 1; j < fieldsEnd; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;

                    var item = itemPattern.Match(lines[j]);
                    if (!item.Success || item.Groups["lead"].Length != fieldIndent)
                    {
                        // A line at or above the field indent that is not one of our items ends the sequence.
                        if (IndentOf(lines[j]) <= fieldIndent) break;
                        continue;
                    }

                    if (!long.TryParse(item.Groups["rid"].Value, out var rid)) return false;

                    pointerLines?.Add(j);
                    rids.Add(rid);
                }

                return true;
            }

            return false;
        }

        // Prefers the snapshot's own rid: Unity has just dropped it, and instance overrides keyed to it
        // ("managedReferences[rid].field") keep applying. Otherwise a random positive 63-bit id, as Unity allocates —
        // never "max + 1": Unity hands out ids sequentially within a session, so the base's max + 1 is often the id
        // of an override a variant, nested prefab or scene already holds, and the two would merge into one entry.
        private static long PickRestoreRid(HashSet<long> used, IReadOnlyList<string> entryLines)
        {
            if (SerializeReferenceYaml.TryParseEntryHeaderRid(entryLines[0], out var original)
                && original > 0 && !used.Contains(original))
                return original;

            var buffer = new byte[8];
            while (true)
            {
                _ridRandom.NextBytes(buffer);
                var candidate = BitConverter.ToInt64(buffer, 0) & long.MaxValue;
                if (candidate > 0 && !used.Contains(candidate)) return candidate;
            }
        }

        // Every "rid: N" in the document — field pointers and RefIds entries alike — so a chosen id never aliases a
        // surviving reference.
        private static HashSet<long> CollectRids(string[] lines, int start, int end)
        {
            var result = new HashSet<long>();

            for (var i = start; i < end; i++)
            {
                foreach (Match match in _anyRid.Matches(lines[i]))
                    if (long.TryParse(match.Groups["rid"].Value, out var value))
                        result.Add(value);
            }

            return result;
        }

        // Copies the captured entry, rewriting only its header's rid to freshRid (the type / data lines are preserved
        // verbatim). The header is the first line — a "- rid: N" — so its indentation and dash are kept intact.
        private static List<string> RewriteEntryRid(IReadOnlyList<string> entryLines, long freshRid)
        {
            var result = new List<string>(entryLines.Count);
            var headerPattern = new Regex(@"^(?<indent>\s*-\s+rid:\s*)-?\d+(?<trailer>\s*)$");

            for (var i = 0; i < entryLines.Count; i++)
            {
                if (i == 0)
                {
                    var match = headerPattern.Match(entryLines[0]);
                    result.Add(match.Success
                        ? match.Groups["indent"].Value + freshRid + match.Groups["trailer"].Value
                        : entryLines[0]);
                }
                else
                {
                    result.Add(entryLines[i]);
                }
            }

            return result;
        }
    }
}
