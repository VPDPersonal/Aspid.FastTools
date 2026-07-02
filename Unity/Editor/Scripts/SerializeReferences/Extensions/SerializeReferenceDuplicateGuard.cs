using System;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Auto-de-aliases freshly duplicated <c>[SerializeReference]</c> list elements. When the user duplicates an array
    /// element (context-menu <i>Duplicate</i>, Ctrl+D) or adds one with the list <c>+</c> button, Unity copies the
    /// source element's managed-reference <c>rid</c>, so two elements end up backed by a single instance and editing one
    /// silently edits the other. This guard watches the live <see cref="SerializedObject"/>: per (target, array path) it
    /// keeps a snapshot of <c>index → rid</c> and, when a <i>new</i> same-array alias appears between observations, it
    /// replaces the later (appended / higher-index) element with an independent clone via the same
    /// <see cref="SerializeReferenceHelpers.CreateInstancePreservingData"/> machinery the Make-unique flow uses,
    /// registered as a single Undo step.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection is purely live-state (<see cref="SerializedProperty.managedReferenceId"/> / value), so it works for
    /// scene objects, Prefab Mode, and saved assets alike — no YAML is read. The fix is silent by product decision: no
    /// notice, no dialog. Undo reverts to the aliased state; after an Undo/Redo the snapshots are resynced rather than
    /// re-evaluated, so a restored alias is never re-fixed.
    /// </para>
    /// <para>
    /// Intentional cross-<i>field</i> sharing is out of scope — that is handled by the existing shared-reference notice.
    /// This guard only acts on same-array aliasing that <i>appears</i> while the inspector is alive; pre-existing aliases
    /// present on the first observation of an array (inspector just opened, domain reload) are recorded, never fixed.
    /// A fix is considered only when the array <i>grew</i> since the last observation and the duplicated rid's
    /// occurrence count rose with it — the signature of an actual duplicate-element operation. Same-size observations
    /// (a reorder) and shrunk ones (a removal) only resync the snapshot, so shuffling or deleting around a pre-existing
    /// alias never de-aliases it.
    /// </para>
    /// </remarks>
    internal static class SerializeReferenceDuplicateGuard
    {
        // A managed reference with no value reports RefIdNull (-1); a missing-type one reports RefIdUnknown (-2). Only
        // ids >= 0 are real instances that can alias — the rest are excluded from the index → rid map.
        private const long FirstValidReferenceId = 0;

        // Unity's array-element path marker (e.g. "_slots.Array.data[3]"). The text before it is the parent array path.
        private const string ArrayElementMarker = ".Array.data[";

        // Caps the live cache so a session that opens many inspectors cannot grow it without bound. Each entry is a tiny
        // index → rid map; the cap is generous but finite. On overflow the whole cache is dropped (cheap, and the next
        // observation simply re-snapshots — which never auto-fixes, so dropping snapshots only ever loses a fix, the
        // conservative direction).
        private const int MaxTrackedArrays = 512;

        // Per (target instance id, parent array path) snapshot of the last observed index → rid layout. Static, so it is
        // cleared automatically on domain reload; dead-target entries are pruned lazily on access and on Undo resync.
        private static readonly Dictionary<ArrayKey, Snapshot> Snapshots = new();

        // Arrays whose de-alias fix is queued for the next editor tick. Guards against re-detecting (the layout still
        // shows the alias until the deferred fix runs) and re-scheduling on every intervening repaint.
        private static readonly HashSet<ArrayKey> Pending = new();

        private static bool _undoHooked;

        /// <summary>
        /// Observes <paramref name="elementProperty"/> (an element of a <c>[SerializeReference]</c> array) and, when it
        /// detects that the element is part of a freshly created same-array duplicate, <i>schedules</i> a fix that
        /// replaces the later element with an independent clone (single Undo step) and returns <see langword="true"/>.
        /// The mutation runs on the next editor tick (<see cref="EditorApplication.delayCall"/>), never inside the
        /// caller's draw/binding pass, so the inspector's live property iteration is not disturbed mid-frame; the field's
        /// own property tracking re-renders once the fix lands. Returns <see langword="false"/> for the no-op fast paths
        /// (not an array element, multi-object edit, first observation, pre-existing alias, or a fix already pending).
        /// Cheap on the unchanged path: an array-size + rolling-hash compare gates the full map rebuild, so it is safe to
        /// call from IMGUI's per-frame repaint.
        /// </summary>
        public static bool Observe(SerializedProperty elementProperty)
        {
            if (!SerializeReferenceSettings.AutoDeAliasEnabled) return false;
            if (elementProperty is null) return false;
            if (elementProperty.propertyType != SerializedPropertyType.ManagedReference) return false;

            // Multi-object editing is owned by the per-target apply path; the live SerializedObject here walks only the
            // first target, so the guard cannot reason about the others — skip it entirely (conservative).
            if (elementProperty.serializedObject.isEditingMultipleObjects) return false;

            if (!TryGetArrayPath(elementProperty.propertyPath, out var arrayPath)) return false;

            EnsureUndoHook();

            var serializedObject = elementProperty.serializedObject;
            var target = serializedObject.targetObject;
            if (target == null) return false;

            var key = new ArrayKey(target.GetInstanceID(), arrayPath);

            // A fix for this array is already queued for the next tick; do not re-detect (the layout still shows the
            // alias until the deferred fix runs) and do not re-schedule it.
            if (Pending.Contains(key)) return false;

            var arrayProperty = serializedObject.FindProperty(arrayPath);
            if (arrayProperty is null || !arrayProperty.isArray) return false;

            // Fast no-change gate: a size + order-sensitive rolling hash of the element rids. When it matches the stored
            // signature nothing in the array's rid layout moved since the last observation, so the index → rid Dictionary
            // rebuild (and the alias diffing) is skipped. The hash pass itself is O(size) cheap reads — it must touch each
            // rid to detect a change — but allocates no per-observation collection, which is the IMGUI per-repaint path.
            var size = arrayProperty.arraySize;
            var signature = ComputeSignature(arrayProperty, size);

            if (Snapshots.TryGetValue(key, out var snapshot) &&
                snapshot.Size == size && snapshot.Signature == signature)
                return false;

            var current = BuildMap(arrayProperty, size);

            // First time we see this array (fresh inspector, domain reload): record the layout but never auto-fix —
            // pre-existing same-array aliases keep the existing shared notice instead.
            if (snapshot is null)
            {
                Store(key, size, signature, current);
                return false;
            }

            // The only operation that creates a fresh alias is one that grows the array by EXACTLY ONE element (Ctrl+D
            // / Duplicate / list + all append a single element). A same-size or shrunk observation is a reorder or a
            // removal, and a multi-element growth is a bulk restore — Paste Component Values, Revert to Prefab, preset
            // application — which can legitimately bring back an INTENTIONAL alias pair (made with Link-to-Existing)
            // that the stale baseline never saw; silently de-aliasing that would destroy deliberate sharing. All those
            // just resync the baseline and never fix, honouring the "pre-existing aliases are recorded, never fixed"
            // contract.
            if (size == snapshot.Size + 1 &&
                TryFindFreshDuplicate(snapshot.Map, current, out var duplicateIndex))
            {
                // Keep the existing baseline snapshot (do not advance it to the aliased layout): once the deferred fix
                // lands, the next observation compares the de-aliased layout against that same baseline, so the fixed
                // element reads as unique while any *further* rapid duplicate made during the pending window is still
                // caught as fresh. The Pending guard suppresses re-detection only until the queued fix runs.
                ScheduleFix(key, target, arrayPath, duplicateIndex);
                return true;
            }

            // No fresh duplicate (or a same-size / shrunk layout): just advance the snapshot to the observed layout.
            Store(key, size, signature, current);
            return false;
        }

        // Queues the de-alias to the next editor tick. The mutation must not run inside the drawer's draw/binding pass:
        // writing the SerializedObject mid-iteration can invalidate the inspector's active property walk. delayCall runs
        // after the current GUI event, where a fresh SerializedObject can be applied safely.
        private static void ScheduleFix(ArrayKey key, Object target, string arrayPath, int duplicateIndex)
        {
            Pending.Add(key);
            EditorApplication.delayCall += () =>
            {
                // Releasing Pending lets the next observation re-evaluate the now-de-aliased array against the unchanged
                // baseline: the fixed element reads as unique (no re-fix) while a further duplicate made meanwhile is
                // still caught. The fix re-verifies the alias on a fresh read, so a stale schedule is a safe no-op.
                Pending.Remove(key);
                MakeElementUnique(target, arrayPath, duplicateIndex);
            };
        }

        // Replaces the element at duplicateIndex with an independent clone carrying the same data, on a fresh
        // SerializedObject built from the target. SetManagedReferenceAndApply registers a single Undo step; a fresh
        // instance gets a new managedReferenceId on assignment, breaking the alias.
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

            // Re-verify the alias still holds on this fresh read — the layout may have changed between the scheduling
            // tick and now (another Undo, a manual edit) — so a stale schedule never clobbers an already-unique element.
            if (!SharesReferenceWithEarlierElement(arrayProperty, duplicateIndex, element.managedReferenceId)) return;

            // Deep copy: the silent de-alias must hand the duplicate its own nested [SerializeReference] children
            // too — a shallow clone would keep the copy's subtree aliased to the original element's.
            element.managedReferenceValue = SerializeReferenceHelpers.CloneManagedReferenceGraph(current);
            serializedObject.ApplyModifiedProperties();

            // Same-frame IMGUI repaints must not read the pre-split alias memo (it is keyed by frame, not content).
            SerializeReferenceHelpers.InvalidateSharedReferenceCache();
        }

        // True when an element at a lower index of the same array currently holds rid — i.e. the element at index is the
        // later half of a still-live same-array alias pair.
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

        // A fresh duplicate is the LATER element of a pair that now shares an rid where (a) that exact (index, rid)
        // binding was not present in the previous snapshot AND (b) the rid now occurs more times than it did in the
        // snapshot. The occurrence-count gate is what separates a genuine new copy from a reorder: dragging an element
        // of a pre-existing alias to a new index changes its (index, rid) binding but leaves the rid's total count
        // unchanged, so it is not treated as fresh. Walking ascending, the first index satisfying both is the
        // appended/duplicated copy to fix.
        private static bool TryFindFreshDuplicate(
            IReadOnlyDictionary<int, long> previous,
            IReadOnlyDictionary<int, long> current,
            out int duplicateIndex)
        {
            duplicateIndex = -1;

            // rid -> lowest current index holding it, so an aliased later element can be matched to an earlier owner.
            var lowestIndexByRid = new Dictionary<long, int>();
            foreach (var pair in current)
                if (!lowestIndexByRid.TryGetValue(pair.Value, out var existing) || pair.Key < existing)
                    lowestIndexByRid[pair.Value] = pair.Key;

            // rid occurrence counts in each layout, to compare the multiset rather than per-index bindings.
            var previousCount = CountByRid(previous);
            var currentCount = CountByRid(current);

            var best = int.MaxValue;
            foreach (var pair in current)
            {
                var index = pair.Key;
                var rid = pair.Value;

                // Only the later element of an alias pair is a candidate (an earlier owner of the rid keeps its instance).
                if (lowestIndexByRid[rid] >= index) continue;

                // Skip aliases that already existed: a binding present unchanged in the previous snapshot is intentional
                // (or pre-existing) sharing, not a fresh duplicate. A new index, or a changed rid at this index, is fresh.
                if (previous.TryGetValue(index, out var previousRid) && previousRid == rid) continue;

                // Require the rid to have multiplied since the snapshot. A reorder that merely moved a pre-existing
                // alias into a new binding leaves the rid's count unchanged and so is not a fresh duplicate.
                previousCount.TryGetValue(rid, out var before);
                if (currentCount[rid] <= before) continue;

                if (index < best) best = index;
            }

            if (best == int.MaxValue) return false;
            duplicateIndex = best;
            return true;
        }

        // Occurrence count of each rid in an index -> rid map, for multiset comparison between snapshots.
        private static Dictionary<long, int> CountByRid(IReadOnlyDictionary<int, long> map)
        {
            var counts = new Dictionary<long, int>(map.Count);
            foreach (var pair in map)
                counts[pair.Value] = counts.TryGetValue(pair.Value, out var existing) ? existing + 1 : 1;

            return counts;
        }

        // Builds the index → rid map for the array, keeping only elements that are managed references with a real
        // instance id (>= 0). Null and missing-type elements carry no aliasable instance, so they are excluded.
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

        // Cheap order-sensitive rolling hash of the element rids — enough to detect any change in the array's rid layout
        // (a duplicate, a reorder, an add/remove) without rebuilding the index → rid map. A single SerializedProperty is
        // walked across siblings (Next(enterChildren: false) skips each managed reference's own children and lands on the
        // next element), so the no-change gate allocates one property per call rather than one per element.
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
            // Drop the whole cache on overflow rather than evicting one entry: simpler, and a re-snapshot never
            // auto-fixes, so the only cost is losing a not-yet-observed fix — the conservative direction. Pending is
            // cleared alongside so the two never desync (a queued fix re-verifies its alias before applying, so dropping
            // a pending key only ever cancels a fix, never mis-applies one).
            if (!Snapshots.ContainsKey(key) && Snapshots.Count >= MaxTrackedArrays)
            {
                Snapshots.Clear();
                Pending.Clear();
            }

            Snapshots[key] = new Snapshot(size, signature, map);
        }

        // The parent array path of an element path: "_slots.Array.data[3]._weapon..." has no array marker at its own
        // level only when it is not an array element; an element path always ends the marker with an index, so the text
        // up to the marker is the array property's path. Nested arrays resolve to the innermost array (last marker),
        // which is the array this element directly belongs to.
        private static bool TryGetArrayPath(string elementPath, out string arrayPath)
        {
            arrayPath = null;
            if (string.IsNullOrEmpty(elementPath)) return false;

            var marker = elementPath.LastIndexOf(ArrayElementMarker, StringComparison.Ordinal);
            if (marker < 0) return false;

            // The element must be the array entry itself ("...Array.data[N]"), not a sub-field of one
            // ("...Array.data[N]._weapon") — only the entry carries the element's own managed reference. Verify the
            // marker is closed by an index and nothing follows the closing bracket.
            var close = elementPath.IndexOf(']', marker + ArrayElementMarker.Length);
            if (close < 0 || close != elementPath.Length - 1) return false;

            arrayPath = elementPath[..marker];
            return arrayPath.Length > 0;
        }

        // Subscribed once. After an Undo/Redo the restored layout must be re-snapshotted as the new baseline so a
        // reverted alias is not treated as a fresh duplicate and immediately re-fixed; the simplest correct resync is to
        // drop every snapshot, so the next observation of each array re-records (and never auto-fixes) its layout.
        private static void EnsureUndoHook()
        {
            if (_undoHooked) return;
            _undoHooked = true;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private static void OnUndoRedoPerformed()
        {
            // Drop both the baseline snapshots and any queued fixes: an Undo can revert an array to a state a fix was
            // scheduled against (or restore an intentional alias), and re-snapshotting on next observation never
            // auto-fixes — so clearing both guarantees a reverted alias is recorded, not re-fixed.
            Snapshots.Clear();
            Pending.Clear();
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
            public Snapshot(int size, int signature, Dictionary<int, long> map)
            {
                Size = size;
                Signature = signature;
                Map = map;
            }

            public int Size { get; }
            public int Signature { get; }
            public Dictionary<int, long> Map { get; }
        }
    }
}
