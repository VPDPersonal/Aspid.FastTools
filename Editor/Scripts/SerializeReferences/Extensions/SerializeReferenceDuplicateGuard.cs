using System;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Auto-de-aliases freshly duplicated [SerializeReference] list elements. Duplicating an element or adding one with
    // the "+" copies the source's rid, so two elements end up backed by one instance and editing one edits the other.
    // Per (target, array path) the guard snapshots the index -> rid layout and, when a NEW same-array alias appears
    // between observations, replaces the later element with an independent clone in a single Undo step.
    //
    // Detection reads only live state, so it covers scene objects, Prefab Mode and saved assets alike. The fix is
    // silent by product decision. After an Undo/Redo the snapshots are resynced rather than re-evaluated, so a
    // restored alias is never re-fixed.
    //
    // Cross-FIELD sharing is out of scope — the shared-reference notice covers that — and so are aliases already
    // present on the first observation of an array. A fix needs the array to have grown AND the duplicated rid's
    // occurrence count to have risen with it, so a reorder or a removal only resyncs the snapshot.
    internal static class SerializeReferenceDuplicateGuard
    {
        // Unity reports -2 for an empty reference and -1 for a missing type; only ids >= 0 can alias.
        private const long FirstValidReferenceId = 0;

        // The text before this marker in an element path is the parent array's path.
        private const string ArrayElementMarker = ".Array.data[";

        // On overflow the whole cache is dropped. A re-snapshot never auto-fixes, so at worst a fix is lost.
        private const int MaxTrackedArrays = 512;

        // The last observed index -> rid layout per (target, array path). Static, so a domain reload clears it.
        private static readonly Dictionary<ArrayKey, Snapshot> _snapshots = new();

        // Arrays whose fix is queued: the layout still shows the alias until it runs, so without this every
        // intervening repaint would re-detect and re-schedule.
        private static readonly HashSet<ArrayKey> _pending = new();

        private static bool _undoHooked;

        // Observes one array element and returns true when it scheduled a de-alias fix for the next editor tick.
        // Cheap on the unchanged path — a size and rolling-hash compare gates the map rebuild — so it is safe to call
        // from IMGUI's per-frame repaint.
        public static bool Observe(SerializedProperty elementProperty)
        {
            if (!SerializeReferenceSettings.AutoDeAliasEnabled) return false;
            if (elementProperty is null) return false;
            if (elementProperty.propertyType != SerializedPropertyType.ManagedReference) return false;

            // The live SerializedObject walks only the first target, so the guard cannot reason about the others.
            if (elementProperty.serializedObject.isEditingMultipleObjects) return false;

            if (!TryGetArrayPath(elementProperty.propertyPath, out var arrayPath)) return false;

            EnsureUndoHook();

            var serializedObject = elementProperty.serializedObject;
            var target = serializedObject.targetObject;
            if (target == null) return false;

            var key = new ArrayKey(target.GetInstanceID(), arrayPath);

            if (_pending.Contains(key)) return false;

            var arrayProperty = serializedObject.FindProperty(arrayPath);
            if (arrayProperty is null || !arrayProperty.isArray) return false;

            // No-change gate: size plus an order-sensitive hash of the rids, allocating nothing per observation.
            var size = arrayProperty.arraySize;
            var signature = ComputeSignature(arrayProperty, size);

            if (_snapshots.TryGetValue(key, out var snapshot) &&
                snapshot.Size == size && snapshot.Signature == signature)
                return false;

            var current = BuildMap(arrayProperty, size);

            // First sight of this array: record the layout, but never auto-fix a pre-existing alias.
            if (snapshot is null)
            {
                Store(key, size, signature, current);
                return false;
            }

            // Only a growth of exactly one element can be a fresh duplicate. A multi-element growth is a bulk restore
            // — Paste Component Values, Revert to Prefab — that may legitimately bring back an intentional alias.
            if (size == snapshot.Size + 1 &&
                TryFindFreshDuplicate(snapshot.Map, current, out var duplicateIndex))
            {
                // The baseline is deliberately not advanced to the aliased layout: once the fix lands the element
                // reads as unique against it, and a further duplicate is still caught as fresh.
                ScheduleFix(key, target, arrayPath, duplicateIndex);
                return true;
            }

            Store(key, size, signature, current);
            return false;
        }

        // Writing the SerializedObject mid-iteration can invalidate the inspector's active property walk, so the
        // mutation is deferred out of the drawer's pass.
        private static void ScheduleFix(ArrayKey key, Object target, string arrayPath, int duplicateIndex)
        {
            _pending.Add(key);
            EditorApplication.delayCall += () =>
            {
                // The fix re-verifies the alias on a fresh read, so a stale schedule is a safe no-op.
                _pending.Remove(key);
                MakeElementUnique(target, arrayPath, duplicateIndex);
            };
        }

        // A fresh instance gets a new managedReferenceId on assignment, breaking the alias; single Undo step.
        private static void MakeElementUnique(Object target, string arrayPath, int duplicateIndex)
        {
            if (target == null) return;

            using var serializedObject = new SerializedObject(target);
            var arrayProperty = serializedObject.FindProperty(arrayPath);
            if (arrayProperty is null || !arrayProperty.isArray) return;
            if (duplicateIndex < 0 || duplicateIndex >= arrayProperty.arraySize) return;

            var element = arrayProperty.GetArrayElementAtIndex(duplicateIndex);
            if (element.propertyType != SerializedPropertyType.ManagedReference) return;

            var current = element.managedReferenceValue;
            if (current is null) return;

            // The layout may have changed since the schedule, so re-verify before clobbering the element.
            if (!SharesReferenceWithEarlierElement(arrayProperty, duplicateIndex, element.managedReferenceId)) return;

            // Deep copy: a shallow clone would leave the copy's nested references aliased to the original's.
            element.managedReferenceValue = SerializeReferenceHelpers.CloneManagedReferenceGraph(current);
            serializedObject.ApplyModifiedProperties();

            // The alias memo is keyed by frame, not content, so same-frame repaints must not read the stale one.
            SerializeReferenceHelpers.InvalidateSharedReferenceCache();
        }

        private static bool SharesReferenceWithEarlierElement(SerializedProperty arrayProperty, int index, long rid)
        {
            if (rid < FirstValidReferenceId) return false;

            for (var i = 0; i < index; i++)
            {
                var other = arrayProperty.GetArrayElementAtIndex(i);
                if (other.propertyType == SerializedPropertyType.ManagedReference && other.managedReferenceId == rid)
                    return true;
            }

            return false;
        }

        // A fresh duplicate is the later element of a pair whose (index, rid) binding is new AND whose rid occurs
        // more times than before; the count gate keeps a reorder of a pre-existing alias from reading as fresh.
        private static bool TryFindFreshDuplicate(
            IReadOnlyDictionary<int, long> previous,
            IReadOnlyDictionary<int, long> current,
            out int duplicateIndex)
        {
            duplicateIndex = -1;

            var lowestIndexByRid = new Dictionary<long, int>();
            foreach (var pair in current)
                if (!lowestIndexByRid.TryGetValue(pair.Value, out var existing) || pair.Key < existing)
                    lowestIndexByRid[pair.Value] = pair.Key;

            var previousCount = CountByRid(previous);
            var currentCount = CountByRid(current);

            var best = int.MaxValue;
            foreach (var pair in current)
            {
                var index = pair.Key;
                var rid = pair.Value;

                // The earlier owner of the rid keeps its instance, so only the later element is a candidate.
                if (lowestIndexByRid[rid] >= index) continue;

                // A binding unchanged since the previous snapshot is existing sharing, not a fresh duplicate.
                if (previous.TryGetValue(index, out var previousRid) && previousRid == rid) continue;

                // A reorder moves an existing alias into a new binding without changing the rid's count.
                previousCount.TryGetValue(rid, out var before);
                if (currentCount[rid] <= before) continue;

                if (index < best) best = index;
            }

            if (best == int.MaxValue) return false;
            duplicateIndex = best;
            return true;
        }

        private static Dictionary<long, int> CountByRid(IReadOnlyDictionary<int, long> map)
        {
            var counts = new Dictionary<long, int>(map.Count);
            foreach (var pair in map)
                counts[pair.Value] = counts.TryGetValue(pair.Value, out var existing) ? existing + 1 : 1;

            return counts;
        }

        private static Dictionary<int, long> BuildMap(SerializedProperty arrayProperty, int size)
        {
            var map = new Dictionary<int, long>(size);
            for (var i = 0; i < size; i++)
            {
                var element = arrayProperty.GetArrayElementAtIndex(i);
                if (element.propertyType != SerializedPropertyType.ManagedReference) continue;

                var rid = element.managedReferenceId;
                if (rid >= FirstValidReferenceId) map[i] = rid;
            }

            return map;
        }

        // Siblings are walked with a single SerializedProperty, so the gate allocates one per call, not per element.
        private static int ComputeSignature(SerializedProperty arrayProperty, int size)
        {
            unchecked
            {
                var hash = 17;
                if (size == 0) return hash;

                var element = arrayProperty.GetArrayElementAtIndex(0);
                for (var i = 0; i < size; i++)
                {
                    var rid = element.propertyType == SerializedPropertyType.ManagedReference
                        ? element.managedReferenceId
                        : long.MinValue;
                    hash = hash * 31 + rid.GetHashCode();

                    if (i + 1 < size && !element.Next(enterChildren: false)) break;
                }

                return hash;
            }
        }

        private static void Store(ArrayKey key, int size, int signature, Dictionary<int, long> map)
        {
            // Dropped together so the two never desync. A re-snapshot never auto-fixes and a queued fix re-verifies,
            // so at worst a fix is canceled, never mis-applied.
            if (!_snapshots.ContainsKey(key) && _snapshots.Count >= MaxTrackedArrays)
            {
                _snapshots.Clear();
                _pending.Clear();
            }

            _snapshots[key] = new Snapshot(size, signature, map);
        }

        // The parent array path of an element path; nested arrays resolve to the innermost one.
        private static bool TryGetArrayPath(string elementPath, out string arrayPath)
        {
            arrayPath = null;
            if (string.IsNullOrEmpty(elementPath)) return false;

            var marker = elementPath.LastIndexOf(ArrayElementMarker, StringComparison.Ordinal);
            if (marker < 0) return false;

            // Only the array entry itself carries the element's reference, so a sub-field path must not match.
            var close = elementPath.IndexOf(']', marker + ArrayElementMarker.Length);
            if (close < 0 || close != elementPath.Length - 1) return false;

            arrayPath = elementPath[..marker];
            return arrayPath.Length > 0;
        }

        private static void EnsureUndoHook()
        {
            if (_undoHooked) return;
            _undoHooked = true;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private static void OnUndoRedoPerformed()
        {
            // An Undo can revert the state a fix was scheduled against, or restore an intentional alias. Dropping
            // both makes the next observation re-record instead of auto-fixing.
            _snapshots.Clear();
            _pending.Clear();
        }

        private readonly struct ArrayKey : IEquatable<ArrayKey>
        {
            private readonly int _targetInstanceId;
            private readonly string _arrayPath;

            public ArrayKey(int targetInstanceId, string arrayPath)
            {
                _targetInstanceId = targetInstanceId;
                _arrayPath = arrayPath;
            }

            public bool Equals(ArrayKey other) =>
                _targetInstanceId == other._targetInstanceId && _arrayPath == other._arrayPath;

            public override bool Equals(object obj) => obj is ArrayKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_targetInstanceId * 397) ^ (_arrayPath?.GetHashCode() ?? 0);
                }
            }
        }

        private sealed class Snapshot
        {
            public int Size { get; }
            public int Signature { get; }
            public Dictionary<int, long> Map { get; }

            public Snapshot(int size, int signature, Dictionary<int, long> map)
            {
                Size = size;
                Signature = signature;
                Map = map;
            }
        }
    }
}
