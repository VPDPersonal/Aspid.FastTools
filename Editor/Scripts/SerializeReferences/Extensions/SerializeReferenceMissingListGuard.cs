using System;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceMissingListGuard : AssetModificationProcessor
    {
        // Consumed once by the post-save pass and dropped, so a later save re-snapshots from the then-current file.
        private static readonly Dictionary<string, List<Snapshot>> PendingByPath = new();

        // Missing elements the user set to <None> since the last save, by asset path; the next save lets them go.
        private static readonly Dictionary<string, HashSet<(long fileId, long rid)>> ClearedByPath = new();

        // Fires with the file still in its pre-save state; the returned set is never altered.
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!IsGuarded(path))
                {
                    if (!string.IsNullOrEmpty(path)) ClearedByPath.Remove(path);
                    continue;
                }

                var snapshots = SnapshotMissingArrayElements(path, SerializeReferenceHelpers.StoredTypeResolves);
                if (snapshots.Count == 0) continue;

                PendingByPath[path] = snapshots;

                // Anchored to the path, not a SerializedObject: the repair re-reads from disk after Unity writes.
                var captured = path;
                EditorApplication.delayCall += () => RestoreAfterSave(captured);
            }

            return paths;
        }

        // A loaded scene or the open Prefab Mode asset keeps its own copy, so a file rewrite under it would diverge
        // from what the editor shows.
        internal static bool IsGuarded(string path) =>
            SerializeReferenceYaml.IsCandidateAssetPath(path) && SerializeReferenceOpenCopyGuard.IsWritable(path);

        // Called before a missing element is set to <None>, so the next save does not bring it back. The note outlives
        // an Undo of the clear until that save, which then leaves the element to Unity.
        public static void NoteIntentionalClear(SerializedProperty property)
        {
            if (property is null) return;

            var serializedObject = property.serializedObject;
            if (!serializedObject.isEditingMultipleObjects)
            {
                NoteIntentionalClearOfTarget(property);
                return;
            }

            foreach (var target in serializedObject.targetObjects)
            {
                using var single = new SerializedObject(target);
                var targetProperty = single.FindProperty(property.propertyPath);
                if (targetProperty is not null) NoteIntentionalClearOfTarget(targetProperty);
            }
        }

        private static void NoteIntentionalClearOfTarget(SerializedProperty property)
        {
            if (!SerializeReferenceHelpers.TryGetRepairLocation(property, out var assetPath, out var fileId, out var inMemory)) return;
            if (inMemory) return; // an open copy is never rewritten by the guard
            if (!SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var rid)) return;

            NoteIntentionalClear(assetPath, fileId, rid);
        }

        internal static void NoteIntentionalClear(string assetPath, long fileId, long rid)
        {
            if (!ClearedByPath.TryGetValue(assetPath, out var cleared))
                ClearedByPath[assetPath] = cleared = new HashSet<(long fileId, long rid)>();

            cleared.Add((fileId, rid));
        }

        internal static List<Snapshot> SnapshotMissingArrayElements(string assetPath, Func<ManagedTypeName, bool> resolves)
        {
            var result = new List<Snapshot>();

            ClearedByPath.TryGetValue(assetPath, out var cleared);
            ClearedByPath.Remove(assetPath);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(assetPath, resolves);
            if (missing.Count == 0) return result;

            var missingRids = new HashSet<(long fileId, long rid)>();
            foreach (var entry in missing)
                missingRids.Add((entry.FileId, entry.Rid));

            var arrays = new Dictionary<(long fileId, string field), ArrayState>();

            foreach (var entry in missing)
            {
                if (!SerializeReferenceYamlEditor.TryFindTopLevelArrayElementForRid(assetPath, entry.FileId, entry.Rid, out var field, out var index))
                    continue; // a single field or nested pointer is not resized, so not at risk

                if (!arrays.TryGetValue((entry.FileId, field), out var before))
                {
                    if (!SerializeReferenceYamlEditor.TryReadTopLevelArrayRids(assetPath, entry.FileId, field, out var rids)) continue;
                    arrays[(entry.FileId, field)] = before = ArrayState.Build(rids, entry.FileId, missingRids, cleared);
                }

                if (cleared is not null && cleared.Contains((entry.FileId, entry.Rid))) continue;

                var elementPath = $"{field}.Array.data[{index}]";
                if (SerializeReferenceYamlEditor.TryReadArrayElementEntryBlock(assetPath, entry.FileId, elementPath, out _, out var entryLines))
                    result.Add(new Snapshot(entry.FileId, field, index, before, entryLines));
            }

            return result;
        }

        private static void RestoreAfterSave(string assetPath)
        {
            if (!PendingByPath.TryGetValue(assetPath, out var snapshots)) return;
            PendingByPath.Remove(assetPath);

            var restored = RestoreSnapshots(assetPath, snapshots);
            if (restored == 0) return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            UnityEngine.Debug.Log($"[Aspid FastTools] Preserved {restored} missing list reference(s) that saving dropped in '{assetPath}'.");
        }

        internal static int RestoreSnapshots(string assetPath, List<Snapshot> snapshots)
        {
            // Read once per array, before any restore rewrites it, so every snapshot is matched against the saved state.
            var arrays = new Dictionary<(long fileId, string field), List<long>>();

            var restored = 0;
            foreach (var snapshot in snapshots)
            {
                var key = (snapshot.FileId, snapshot.Field);
                if (!arrays.TryGetValue(key, out var after))
                {
                    if (!SerializeReferenceYamlEditor.TryReadTopLevelArrayRids(assetPath, snapshot.FileId, snapshot.Field, out after))
                        after = null;
                    arrays[key] = after;
                }

                if (after is null) continue;
                if (!TryResolveRestoreIndex(snapshot.Before, after, snapshot.Index, out var target)) continue;

                var elementPath = $"{snapshot.Field}.Array.data[{target}]";
                if (SerializeReferenceYamlEditor.TryRestoreArrayElementReference(assetPath, snapshot.FileId, elementPath, snapshot.EntryLines))
                    restored++;
            }

            return restored;
        }

        // Where the missing element before[index] sits after the save, if the save dropped it and nothing else. Unity
        // may write a missing element as a null id on any save (a prefab never keeps one), so an unchanged size keeps
        // it in its slot, as Unity does itself for a ScriptableObject. A grown list prefers the old index, since "+"
        // appends; a shrunk list follows the alignment that ShrunkAlignment picks.
        internal static bool TryResolveRestoreIndex(ArrayState before, IReadOnlyList<long> after, int index, out int target)
        {
            target = -1;
            if (index < 0 || index >= before.Count || !before.Collapsible[index]) return false;

            var candidate = after.Count == before.Count ? index
                : after.Count > before.Count ? GrownCandidate(before, after, index)
                : ShrunkAlignment(before, after)?[index] ?? -1;

            if (candidate < 0) return false;
            if (after[candidate] >= 0) return false; // the element survived, or the slot was re-assigned

            target = candidate;
            return true;
        }

        // The only place in after that before[index] can take while every other element of before keeps its order;
        // the old index when it is one of several.
        private static int GrownCandidate(ArrayState before, IReadOnlyList<long> after, int index)
        {
            // prefix[a, b]: the first b elements of before embed into the first a of after; suffix[a, b]: before from
            // b embeds into after from a.
            var prefix = new bool[after.Count + 1, before.Count + 1];
            for (var a = 0; a <= after.Count; a++)
            {
                prefix[a, 0] = true;
                for (var b = 1; b <= before.Count && a > 0; b++)
                    prefix[a, b] = prefix[a - 1, b] || (prefix[a - 1, b - 1] && before.Accepts(b - 1, after[a - 1]));
            }

            var suffix = new bool[after.Count + 1, before.Count + 1];
            for (var a = after.Count; a >= 0; a--)
            {
                suffix[a, before.Count] = true;
                for (var b = before.Count - 1; b >= 0 && a < after.Count; b--)
                    suffix[a, b] = suffix[a + 1, b] || (before.Accepts(b, after[a]) && suffix[a + 1, b + 1]);
            }

            var candidate = -1;
            for (var j = 0; j < after.Count; j++)
            {
                if (!prefix[j, index] || !before.Accepts(index, after[j]) || !suffix[j + 1, index + 1]) continue;
                if (j == index) return j;
                if (candidate >= 0) return -1;

                candidate = j;
            }

            return candidate;
        }

        // Where each element of before sits in after, or -1 where it was deleted; null when no deletion gives after.
        // A save writes the same nulls whether the user deleted a missing element or a <None> beside it, so the pick
        // keeps as many <None> elements as it can (a deleted missing element stays deleted) and, among those
        // alignments, the earliest elements: a run of missing elements that lost one keeps its first ones in order.
        internal static int[] ShrunkAlignment(ArrayState before, IReadOnlyList<long> after)
        {
            // kept[b, a]: the most <None> elements kept when before from b is aligned onto after from a; -1 if none.
            var kept = new int[before.Count + 1, after.Count + 1];
            for (var b = before.Count; b >= 0; b--)
            {
                for (var a = after.Count; a >= 0; a--)
                {
                    if (a == after.Count) kept[b, a] = 0;
                    else if (b == before.Count) kept[b, a] = -1;
                    else kept[b, a] = Math.Max(kept[b + 1, a], KeepScore(before, after, b, a, kept));
                }
            }

            if (kept[0, 0] < 0) return null;

            var positions = new int[before.Count];
            for (int b = 0, a = 0; b < before.Count; b++)
            {
                var keep = a < after.Count && KeepScore(before, after, b, a, kept) == kept[b, a];
                positions[b] = keep ? a++ : -1;
            }

            return positions;
        }

        private static int KeepScore(ArrayState before, IReadOnlyList<long> after, int b, int a, int[,] kept)
        {
            if (!before.Accepts(b, after[a]) || kept[b + 1, a + 1] < 0) return -1;
            return kept[b + 1, a + 1] + (before.IsNull(b) ? 1 : 0);
        }

        // The pre-save pointers of one array: which slots hold a missing element that the save may write as a null id.
        // A missing element the user cleared counts as a plain null.
        internal sealed class ArrayState
        {
            public readonly long[] Rids;
            public readonly bool[] Collapsible;

            public int Count => Rids.Length;

            public ArrayState(long[] rids, bool[] collapsible)
            {
                Rids = rids;
                Collapsible = collapsible;
            }

            public static ArrayState Build(List<long> rids, long fileId,
                HashSet<(long fileId, long rid)> missingRids, HashSet<(long fileId, long rid)> cleared)
            {
                var slots = new long[rids.Count];
                var collapsible = new bool[rids.Count];

                for (var i = 0; i < slots.Length; i++)
                {
                    var rid = rids[i];
                    var isCleared = cleared is not null && cleared.Contains((fileId, rid));

                    slots[i] = isCleared ? NullRid : rid;
                    collapsible[i] = !isCleared && missingRids.Contains((fileId, rid));
                }

                return new ArrayState(slots, collapsible);
            }

            // A <None> element: a null the user left, not a missing element.
            public bool IsNull(int index) => !Collapsible[index] && Rids[index] < 0;

            // Whether the slot may hold this id after a save that only deleted or added elements.
            public bool Accepts(int index, long rid)
            {
                var previous = Rids[index];
                if (Collapsible[index]) return rid < 0 || rid == previous;
                return previous < 0 ? rid < 0 : rid == previous;
            }
        }

        private const long NullRid = -2;

        internal readonly struct Snapshot
        {
            public readonly long FileId;
            public readonly string Field;
            public readonly int Index;
            public readonly ArrayState Before;
            public readonly List<string> EntryLines;

            public Snapshot(long fileId, string field, int index, ArrayState before, List<string> entryLines)
            {
                FileId = fileId;
                Field = field;
                Index = index;
                Before = before;
                EntryLines = entryLines;
            }
        }
    }
}
