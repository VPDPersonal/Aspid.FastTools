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
        // an Undo of the clear until that save, which then drops the element if it also resizes the list.
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
            UnityEngine.Debug.Log($"[Aspid FastTools] Preserved {restored} missing reference(s) that a list resize would have dropped in '{assetPath}'.");
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
        // collapses missing list elements into null ids only when the list is resized, so at an unchanged size a null
        // there was set on purpose. A shrunk list restores only when every way to get after by deleting elements of
        // before keeps this element, at one place; a grown list prefers the old index, since "+" appends.
        internal static bool TryResolveRestoreIndex(ArrayState before, IReadOnlyList<long> after, int index, out int target)
        {
            target = -1;
            if (index < 0 || index >= before.Count || !before.Collapsible[index]) return false;
            if (after.Count == before.Count) return false;

            var grown = after.Count > before.Count;
            var bigCount = grown ? after.Count : before.Count;
            var smallCount = grown ? before.Count : after.Count;

            bool Matches(int bigIndex, int smallIndex) => grown
                ? before.Accepts(smallIndex, after[bigIndex])
                : before.Accepts(bigIndex, after[smallIndex]);

            // prefix[b, s]: the first s small elements embed into the first b big ones; suffix[b, s]: small from s
            // embeds into big from b.
            var prefix = new bool[bigCount + 1, smallCount + 1];
            for (var b = 0; b <= bigCount; b++)
            {
                prefix[b, 0] = true;
                for (var s = 1; s <= smallCount && b > 0; s++)
                    prefix[b, s] = prefix[b - 1, s] || (prefix[b - 1, s - 1] && Matches(b - 1, s - 1));
            }

            var suffix = new bool[bigCount + 1, smallCount + 1];
            for (var b = bigCount; b >= 0; b--)
            {
                suffix[b, smallCount] = true;
                for (var s = smallCount - 1; s >= 0 && b < bigCount; s--)
                    suffix[b, s] = suffix[b + 1, s] || (Matches(b, s) && suffix[b + 1, s + 1]);
            }

            var candidate = -1;
            var ambiguous = false;

            if (grown)
            {
                for (var j = 0; j < after.Count; j++)
                {
                    if (!prefix[j, index] || !Matches(j, index) || !suffix[j + 1, index + 1]) continue;
                    if (j == index)
                    {
                        candidate = j;
                        ambiguous = false;
                        break;
                    }

                    ambiguous |= candidate >= 0;
                    candidate = j;
                }
            }
            else
            {
                // Deleted in some alignment: the user may have removed exactly this element, so it stays removed.
                for (var s = 0; s <= after.Count; s++)
                    if (prefix[index, s] && suffix[index + 1, s]) return false;

                for (var s = 0; s < after.Count; s++)
                {
                    if (!prefix[index, s] || !Matches(index, s) || !suffix[index + 1, s + 1]) continue;

                    ambiguous |= candidate >= 0;
                    candidate = s;
                }
            }

            if (candidate < 0 || ambiguous) return false;
            if (after[candidate] >= 0) return false; // the element survived, or the slot was re-assigned

            target = candidate;
            return true;
        }

        // The pre-save pointers of one array: which slots hold a missing element that a resize may collapse to a null
        // id. A missing element the user cleared counts as a plain null.
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

            // Whether the slot may hold this id after a save that only resized the list.
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
