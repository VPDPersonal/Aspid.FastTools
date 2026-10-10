using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The file side of the missing-list guard. Unity writes a missing list element as a null id when a save resizes the
    // list (a prefab on any save) and drops its RefIds entry; the guard snapshots the lists before the save and puts the
    // dropped elements back after it.
    internal static partial class SerializeReferenceYamlEditor
    {
        private const string RefIdsMarker = "RefIds:";

        private static readonly Regex _fieldHeader = new(@"^(?<lead>\s*)(?<name>[^\s:#-][^:]*):\s*$", RegexOptions.Compiled);

        private static readonly Regex _listItem = new(@"^(?<lead>\s*)-\s+rid:\s*(?<rid>-?\d+)\s*$", RegexOptions.Compiled);

        // The lists a save may shrink: each top-level list that holds a missing element, with the RefIds entry of every
        // missing element in it. replaced holds the missing elements the user replaced since the last save; they count
        // as nulls. replacedSlots holds the ones replaced in a single list slot: only that slot counts as a null, or the
        // slot of the rid nearest to it when it no longer holds the rid. A file without a RefIds block is not parsed.
        public static List<MissingListSnapshot> SnapshotMissingLists(string assetPath, Func<ManagedTypeName, bool> resolves,
            ICollection<(long fileId, long rid)> replaced,
            IReadOnlyList<(long fileId, long rid, string field, int index)> replacedSlots = null)
        {
            var result = new List<MissingListSnapshot>();

            try
            {
                var lines = ReadLinesWithRefIds(assetPath);
                if (lines is null) return result;

                var missingRids = new HashSet<(long fileId, long rid)>();
                var storedTypes = new Dictionary<long, Dictionary<long, ManagedTypeName>>();

                foreach (var entry in FindMissingReferences(lines, resolves))
                {
                    // An override lives in the PrefabInstance document, which has no list for a resize to drop.
                    if (entry.IsOverride) continue;

                    missingRids.Add((entry.FileId, entry.Rid));
                    if (!storedTypes.TryGetValue(entry.FileId, out var types))
                        storedTypes[entry.FileId] = types = new Dictionary<long, ManagedTypeName>();

                    types[entry.Rid] = entry.StoredType;
                }

                foreach (var pair in storedTypes)
                    SnapshotObjectLists(lines, pair.Key, pair.Value, missingRids, replaced, replacedSlots, result);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to read the missing list elements of '{assetPath}': {exception}");
                result.Clear();
            }

            return result;
        }

        private static void SnapshotObjectLists(string[] lines, long fileId, Dictionary<long, ManagedTypeName> types,
            HashSet<(long fileId, long rid)> missingRids, ICollection<(long fileId, long rid)> replaced,
            IReadOnlyList<(long fileId, long rid, string field, int index)> replacedSlots, List<MissingListSnapshot> result)
        {
            var (start, end) = FindDocumentRange(lines, fileId);
            if (start < 0) return;

            var refIdsStart = FindRefIdsStart(lines, start, end);
            if (refIdsStart < 0) return;

            var found = new List<(string field, List<long> rids)>();
            foreach (var field in FindTopLevelListsHolding(lines, start, refIdsStart, types.Keys))
            {
                var rids = new List<long>();
                if (TryCollectArrayElementPointers(lines, start, refIdsStart, field, null, rids)) found.Add((field, rids));
            }

            var lists = new List<(string field, MissingListState before)>();
            var wanted = new HashSet<long>();

            foreach (var (field, rids) in found)
            {
                var isReplaced = FindReplacedSlots(fileId, field, rids, found, replaced, replacedSlots);
                var before = MissingListState.Build(rids, fileId, missingRids, isReplaced);
                if (!before.HasMissing) continue;

                lists.Add((field, before));
                for (var i = 0; i < before.Count; i++)
                    if (before.Collapsible[i]) wanted.Add(before.Rids[i]);
            }

            if (lists.Count == 0) return;

            var blocks = ReadEntryBlocks(lines, refIdsStart, end, wanted);
            foreach (var (field, before) in lists)
            {
                var entries = new Dictionary<long, (List<string> entryLines, ManagedTypeName storedType)>();
                for (var i = 0; i < before.Count; i++)
                {
                    var rid = before.Rids[i];
                    if (before.Collapsible[i] && blocks.TryGetValue(rid, out var block)) entries[rid] = (block, types[rid]);
                }

                if (entries.Count > 0) result.Add(new MissingListSnapshot(fileId, field, before, entries));
            }
        }

        // Which slots of a list the user replaced. A note without a slot replaces every slot of its rid. A note with a slot
        // replaces only one slot: the noted one while it still holds the rid, otherwise the slot of the rid nearest to it,
        // since an unsaved edit can shift the in-memory index the note took from the file's.
        private static bool[] FindReplacedSlots(long fileId, string field, List<long> rids,
            List<(string field, List<long> rids)> lists, ICollection<(long fileId, long rid)> replaced,
            IReadOnlyList<(long fileId, long rid, string field, int index)> replacedSlots)
        {
            var result = new bool[rids.Count];

            for (var i = 0; i < rids.Count; i++)
            {
                var rid = rids[i];
                if (replaced is not null && replaced.Contains((fileId, rid)))
                {
                    result[i] = true;
                    continue;
                }

                if (replacedSlots is null) continue;

                foreach (var note in replacedSlots)
                {
                    if (note.fileId != fileId || note.rid != rid) continue;

                    var slot = FindNotedSlot(lists, note.field, note.index, rid);
                    if (slot.field == field && slot.index == i) result[i] = true;
                }
            }

            return result;
        }

        // The slot a note replaced: the noted one when it holds the rid, otherwise the slot of the rid nearest to it in
        // the noted list, otherwise the first slot of the rid in any list.
        private static (string field, int index) FindNotedSlot(List<(string field, List<long> rids)> lists, string field,
            int index, long rid)
        {
            foreach (var list in lists)
            {
                if (list.field != field) continue;

                var nearest = -1;
                for (var i = 0; i < list.rids.Count; i++)
                {
                    if (list.rids[i] != rid) continue;
                    if (nearest < 0 || Math.Abs(i - index) < Math.Abs(nearest - index)) nearest = i;
                }

                if (nearest >= 0) return (field, nearest);
            }

            foreach (var list in lists)
            {
                var first = list.rids.IndexOf(rid);
                if (first >= 0) return (list.field, first);
            }

            return (null, -1);
        }

        // Puts back the missing elements a save dropped from the lists of the snapshots. Each list is matched with the
        // saved one by MissingListAlignment, and the file is written once. The caller reimports the asset.
        public static MissingListReport RestoreMissingLists(string assetPath, IReadOnlyList<MissingListSnapshot> snapshots)
        {
            var report = new MissingListReport();
            if (snapshots is null || snapshots.Count == 0) return report;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return report;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return report;

                // One entry per missing reference, so the slots that shared it before the save share it again.
                var restores = new List<ArrayElementRestore>();
                var restoreOf = new Dictionary<(long fileId, long rid), ArrayElementRestore>();
                var placed = new List<(long fileId, int before, MissingListReport.Element element)>();

                foreach (var snapshot in snapshots)
                {
                    var (start, end) = FindDocumentRange(lines, snapshot.FileId);
                    if (start < 0) continue;

                    var refIdsStart = FindRefIdsStart(lines, start, end);
                    if (refIdsStart < 0) continue;

                    var after = new List<long>();
                    if (!TryCollectArrayElementPointers(lines, start, refIdsStart, snapshot.Field, null, after)) continue;

                    var targets = MissingListAlignment.Align(snapshot.Before, after, out var guessed);

                    for (var b = 0; b < snapshot.Before.Count; b++)
                    {
                        if (!snapshot.Before.Collapsible[b]) continue;

                        var rid = snapshot.Before.Rids[b];
                        if (!snapshot.Entries.TryGetValue(rid, out var entry)) continue;

                        var target = targets[b];
                        if (target >= 0 && after[target] >= 0) continue; // the save kept it

                        if (target < 0)
                        {
                            report.Dropped.Add(new MissingListReport.Element(snapshot.Field, b, rid, entry.storedType, guessed[b]));
                            continue;
                        }

                        if (!restoreOf.TryGetValue((snapshot.FileId, rid), out var restore))
                        {
                            restore = new ArrayElementRestore(snapshot.FileId, entry.entryLines);
                            restoreOf[(snapshot.FileId, rid)] = restore;
                            restores.Add(restore);
                        }

                        restore.Slots.Add((snapshot.Field, target));
                        placed.Add((snapshot.FileId, b,
                            new MissingListReport.Element(snapshot.Field, target, rid, entry.storedType, guessed[b])));
                    }
                }

                var written = RestoreArrayElementReferences(assetPath, restores);
                foreach (var (fileId, before, element) in placed)
                {
                    if (written.Contains((fileId, element.Field, element.Index)))
                    {
                        report.Restored.Add(element);
                        continue;
                    }

                    report.Dropped.Add(new MissingListReport.Element(element.Field, before, element.Rid, element.StoredType, guessed: true));
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to restore the missing list elements of '{assetPath}': {exception}");
            }

            return report;
        }

        // Re-inserts each entry under a free rid and points its slots at it, with one write. A slot is re-pointed only
        // while it holds a null (-2) or missing (-1) id, so a slot the user has since assigned is never overwritten, and an
        // entry left with no such slot is not inserted. Returns the re-pointed slots; the caller reimports the asset.
        public static HashSet<(long fileId, string field, int index)> RestoreArrayElementReferences(string assetPath,
            IReadOnlyList<ArrayElementRestore> restores)
        {
            var written = new HashSet<(long fileId, string field, int index)>();

            try
            {
                if (restores is null || restores.Count == 0) return written;
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return written;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return written;

                var pointers = new Dictionary<int, string>();
                var insertions = new Dictionary<int, List<string>>();
                var usedRids = new Dictionary<int, HashSet<long>>();
                var lists = new Dictionary<(int start, string field), (List<int> pointerLines, List<long> rids)>();

                foreach (var restore in restores)
                {
                    if (restore.EntryLines is null || restore.EntryLines.Count < 2) continue;

                    var (start, end) = FindDocumentRange(lines, restore.FileId);
                    if (start < 0) continue;

                    var refIdsStart = FindRefIdsStart(lines, start, end);
                    if (refIdsStart < 0) continue;

                    var targets = new List<(int line, string field, int index)>();
                    foreach (var (field, index) in restore.Slots)
                    {
                        if (!lists.TryGetValue((start, field), out var list))
                        {
                            list = (new List<int>(), new List<long>());
                            if (!TryCollectArrayElementPointers(lines, start, refIdsStart, field, list.pointerLines, list.rids))
                                list = (null, null);

                            lists[(start, field)] = list;
                        }

                        if (list.rids is null || index < 0 || index >= list.rids.Count) continue;
                        if (list.rids[index] >= 0 || pointers.ContainsKey(list.pointerLines[index])) continue;

                        targets.Add((list.pointerLines[index], field, index));
                    }

                    if (targets.Count == 0) continue;

                    if (!usedRids.TryGetValue(start, out var used)) usedRids[start] = used = CollectRids(lines, start, end);

                    var freshRid = PickRestoreRid(used, restore.EntryLines);
                    used.Add(freshRid);

                    foreach (var (line, field, index) in targets)
                    {
                        pointers[line] = new string(' ', IndentOf(lines[line])) + $"- rid: {freshRid}";
                        written.Add((restore.FileId, field, index));
                    }

                    if (!insertions.TryGetValue(refIdsStart, out var inserted))
                        insertions[refIdsStart] = inserted = new List<string>();

                    inserted.AddRange(RewriteEntryRid(restore.EntryLines, freshRid));
                }

                if (written.Count == 0) return written;

                var result = new List<string>(lines.Length + insertions.Count * 4);
                for (var i = 0; i < lines.Length; i++)
                {
                    result.Add(pointers.TryGetValue(i, out var pointer) ? pointer : lines[i]);
                    if (insertions.TryGetValue(i, out var inserted)) result.AddRange(inserted);
                }

                if (!TryWritePreservingNewlines(assetPath, result))
                {
                    written.Clear();
                    return written;
                }

                SerializeReferenceYamlProbeCache.ClearCache();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to restore managed references in '{assetPath}': {exception}");
                written.Clear();
            }

            return written;
        }

        // The asset's lines, or null when it is not text YAML or holds no RefIds block: such a file has no list element to
        // lose, and the text check skips splitting and parsing it.
        private static string[] ReadLinesWithRefIds(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return null;
            if (!SerializeReferenceYaml.IsTextYamlFile(assetPath)) return null;

            var text = File.ReadAllText(assetPath);
            if (text.IndexOf(RefIdsMarker, StringComparison.Ordinal) < 0) return null;

            var lines = new List<string>();
            using var reader = new StringReader(text);

            for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
                lines.Add(line);

            return lines.ToArray();
        }

        // The object's own top-level list fields that point at any of the rids, in file order. Only fields at the m_Script
        // indent count, so a same-named list nested in an earlier field's container is skipped.
        private static List<string> FindTopLevelListsHolding(string[] lines, int start, int fieldsEnd, ICollection<long> rids)
        {
            var result = new List<string>();

            var topIndent = TryReadScriptGuid(lines, start + 1, fieldsEnd, out _, out var scriptIndent)
                ? scriptIndent
                : -1;

            string currentField = null;
            var fieldIndent = -1;

            for (var i = start; i < fieldsEnd; i++)
            {
                if (lines[i].Trim().Length == 0) continue;

                var item = _listItem.Match(lines[i]);
                if (item.Success && currentField is not null && item.Groups["lead"].Length == fieldIndent)
                {
                    if (long.TryParse(item.Groups["rid"].Value, out var rid) && rids.Contains(rid) && !result.Contains(currentField))
                        result.Add(currentField);

                    continue;
                }

                var header = _fieldHeader.Match(lines[i]);
                if (header.Success && (topIndent < 0 || header.Groups["lead"].Length == topIndent))
                {
                    currentField = header.Groups["name"].Value;
                    fieldIndent = header.Groups["lead"].Length;
                }
            }

            return result;
        }

        // The RefIds entry block of each rid, verbatim; a rid without a complete block is left out.
        private static Dictionary<long, List<string>> ReadEntryBlocks(string[] lines, int refIdsStart, int end, ICollection<long> rids)
        {
            var result = new Dictionary<long, List<string>>();

            var entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
            if (entryIndent < 0) return result;

            for (var i = refIdsStart + 1; i < end; i++)
            {
                if (!SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var rid)) continue;
                if (!rids.Contains(rid) || result.ContainsKey(rid)) continue;

                var entryEnd = FindEntryEnd(lines, i, end, entryIndent);
                if (entryEnd - i < 2) continue;

                var block = new List<string>(entryEnd - i);
                for (var k = i; k < entryEnd; k++) block.Add(lines[k]);

                result[rid] = block;
            }

            return result;
        }

        // One RefIds entry to put back and the top-level list slots to point at it.
        internal sealed class ArrayElementRestore
        {
            public readonly long FileId;
            public readonly IReadOnlyList<string> EntryLines;
            public readonly List<(string field, int index)> Slots = new();

            public ArrayElementRestore(long fileId, IReadOnlyList<string> entryLines)
            {
                FileId = fileId;
                EntryLines = entryLines;
            }
        }
    }
}
