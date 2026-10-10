using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceBreakageDetector
    {
        // The baseline maps each asset path to the stored type keys that resolved in it, so re-reading one changed
        // asset replaces its entry and a type whose last usage is gone drops out instead of alarming on a later rename.
        internal const string EstablishedKey = "Aspid.FastTools.SerializeReferences.Breakage.AssetBaselineEstablished";
        internal const string BaselineKey = "Aspid.FastTools.SerializeReferences.Breakage.AssetBaseline";
        private const double SweepBudgetMilliseconds = 8;

        private static readonly BreakageBaseline _baseline = new(BaselineKey);
        private static readonly Queue<string> _pending = new();
        private static readonly Dictionary<string, HashSet<string>> _swept = new(StringComparer.Ordinal);
        private static bool _establishing;
        private static bool _pumping;
        private static bool _reportPending;

        public static event Action<BreakageReport> BreakageDetected;

        internal static bool IsEstablished => SessionState.GetBool(EstablishedKey, false);

        [InitializeOnLoadMethod]
        private static void EstablishBaselineOnce() => EditorApplication.delayCall += EstablishBaseline;

        private static void EstablishBaseline()
        {
            if (Application.isBatchMode) return;
            if (IsEstablished) return;

            RunDetection(report: false, changedAssets: null);
        }

        // The excluded folders decide which assets the baseline covers: a type that only an excluded folder holds must
        // stop alarming, and a folder that is no longer excluded must be covered, so the baseline starts over.
        internal static void ResetBaseline()
        {
            CancelSweep();
            _baseline.Clear();
            SessionState.EraseBool(EstablishedKey);

            EditorApplication.delayCall += EstablishBaseline;
        }

        // changedAssets are the candidate assets imported, deleted or moved since the last scan; their baseline entries
        // are re-read, so a type first assigned during the session is covered too.
        public static void Scan(IReadOnlyCollection<string> changedAssets = null) => RunDetection(report: true, changedAssets);

        private static void RunDetection(bool report, IReadOnlyCollection<string> changedAssets)
        {
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            // Type resolution flaps while scripts compile, so defer (never drop) until the editor settles.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += () => RunDetection(report, changedAssets);
                return;
            }

            // Warming the index here would mean a modal full-project sweep on routine saves, so a cold index falls
            // back to re-resolving the baseline keys directly.
            if (SerializeReferenceTypeUsageIndex.IsWarm) RunDetectionWarm(report);
            else RunDetectionCold(report, changedAssets);
        }

        private static void RunDetectionWarm(bool report)
        {
            var resolvable = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            var unresolved = new List<SerializeReferenceTypeUsageIndex.Usage>();
            var pathsByGuid = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var usage in SerializeReferenceTypeUsageIndex.AllUsages())
            {
                if (!usage.Resolves)
                {
                    unresolved.Add(usage);
                    continue;
                }

                if (!pathsByGuid.TryGetValue(usage.Guid, out var path))
                {
                    path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                    pathsByGuid.Add(usage.Guid, path);
                }

                if (string.IsNullOrEmpty(path)) continue;
                GetOrAdd(resolvable, path).Add(SerializeReferenceHelpers.StoredTypeKey(usage.StoredType));
            }

            BreakageReport result = default;
            if (IsEstablished && report)
                result = BuildReport(unresolved, CollectKeys(_baseline.Entries));

            // The warm index already reflects every pending asset, so a text sweep still in flight is redundant.
            CancelSweep();
            _baseline.Replace(resolvable);
            SessionState.SetBool(EstablishedKey, true);

            if (result.HasAny) BreakageDetected?.Invoke(result);
        }

        // Cold-index path: each baseline key is re-resolved directly, so the report is type-level only — the Project
        // References tab rebuilds the index to list the exact sites.
        private static void RunDetectionCold(bool report, IReadOnlyCollection<string> changedAssets)
        {
            // No baseline yet: a text sweep spread over editor updates establishes it, since warming the index is a
            // modal full-project build. A type that breaks before the sweep completes is not reported.
            if (!IsEstablished)
            {
                if (_establishing) Enqueue(changedAssets);
                else BeginEstablishing();
                return;
            }

            // The changed assets are re-read before judging the baseline: a pulled rename whose assets were re-saved
            // with the new name must not alarm on the entries they held before.
            Enqueue(changedAssets);
            if (!report) return;

            if (_pending.Count == 0) ReportBrokenBaselineTypes();
            else _reportPending = true;
        }

        private static void ReportBrokenBaselineTypes()
        {
            var baseline = _baseline.Entries;
            if (baseline.Count == 0) return;

            var entries = new List<BreakageEntry>();
            var brokenTypes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var key in CollectKeys(baseline))
            {
                if (!TryParseStoredTypeKey(key, out var storedType)) continue;
                if (SerializeReferenceHelpers.StoredTypeResolves(storedType)) continue;

                SerializeReferenceMovedFromResolver.TryResolve(storedType, out var migrationTarget);
                entries.Add(new BreakageEntry(null, 0, 0, storedType, isRepairable: false, topSuggestion: null,
                    migrationTarget));
                brokenTypes.Add(key);
            }

            if (entries.Count == 0) return;

            // Reported once: a broken key leaves the baseline, including the results of a sweep still in flight.
            RemoveKeys(baseline, brokenTypes);
            RemoveKeys(_swept, brokenTypes);
            _baseline.Replace(baseline);

            BreakageDetected?.Invoke(new BreakageReport(entries, brokenTypes.Count));
        }

        private static void BeginEstablishing()
        {
            CancelSweep();
            _establishing = true;

            var paths = new List<string>();
            foreach (var path in AssetDatabase.GetAllAssetPaths())
                if (SerializeReferenceHelpers.IsScanCandidate(path)) paths.Add(path);

            // A project without candidate assets still has to leave the establishing state.
            if (paths.Count == 0) CompleteSweep();
            else Enqueue(paths);
        }

        private static void Enqueue(IEnumerable<string> paths)
        {
            if (paths is null) return;

            foreach (var path in paths)
                if (!string.IsNullOrEmpty(path)) _pending.Enqueue(path);

            if (_pending.Count == 0 || _pumping) return;

            _pumping = true;
            EditorApplication.update += Pump;
        }

        private static void Pump()
        {
            if (!SerializeReferenceSettings.BreakageDetectionEnabled)
            {
                CancelSweep();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            ProcessPending(SweepBudgetMilliseconds);
        }

        // Finishes the sweep in one call instead of across editor updates.
        internal static void CompleteSweep() => ProcessPending(double.PositiveInfinity);

        private static void ProcessPending(double budgetMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            while (_pending.Count > 0 && stopwatch.Elapsed.TotalMilliseconds < budgetMilliseconds)
            {
                var path = _pending.Dequeue();
                _swept[path] = CollectResolvableKeys(path);
            }

            if (_pending.Count > 0) return;

            StopPump();
            if (!_establishing && !IsEstablished)
            {
                _swept.Clear();
                _reportPending = false;
                return;
            }

            var baseline = _establishing
                ? new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
                : _baseline.Entries;

            foreach (var (path, keys) in _swept)
            {
                if (keys.Count == 0) baseline.Remove(path);
                else baseline[path] = keys;
            }

            _swept.Clear();
            _baseline.Replace(baseline);

            if (_establishing)
            {
                _establishing = false;
                SessionState.SetBool(EstablishedKey, true);
                return;
            }

            if (!_reportPending) return;

            _reportPending = false;
            ReportBrokenBaselineTypes();
        }

        private static void CancelSweep()
        {
            StopPump();
            _pending.Clear();
            _swept.Clear();
            _establishing = false;
            _reportPending = false;
        }

        // Tests start from no sweep in flight, whatever the session's own sweep was doing.
        internal static void ResetForTests() => CancelSweep();

        private static void StopPump()
        {
            if (!_pumping) return;

            _pumping = false;
            EditorApplication.update -= Pump;
        }

        // A deleted, moved-away or no longer scanned asset yields an empty set, which drops its entry. The keys come
        // from the same usages the warm index holds, prefab instance overrides included.
        private static HashSet<string> CollectResolvableKeys(string path)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (!SerializeReferenceHelpers.IsScanCandidate(path)) return keys;

            foreach (var usage in SerializeReferenceTypeUsageIndex.CollectUsages(ReadIfMayHoldReferences(path), guid: null))
                if (usage.Resolves) keys.Add(SerializeReferenceHelpers.StoredTypeKey(usage.StoredType));

            return keys;
        }

        // A byte probe before the parse, since most assets hold no managed references at all and must not be decoded.
        // Returns the lines for the parse, so a matching asset is decoded once, or null when the asset can be skipped.
        private static string[] ReadIfMayHoldReferences(string path) =>
            SerializeReferenceYaml.ReadLinesIfContainsAny(path, SerializeReferenceTypeUsageIndex.UsageMarkers);

        private static bool TryParseStoredTypeKey(string key, out ManagedTypeName storedType)
        {
            storedType = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('|');
            if (parts.Length != 3 || parts[2].Length == 0) return false;

            storedType = new ManagedTypeName(parts[0], parts[1], parts[2]);
            return true;
        }

        public static BreakageReport BuildReport(
            List<SerializeReferenceTypeUsageIndex.Usage> unresolved,
            HashSet<string> baseline)
        {
            var entries = new List<BreakageEntry>();
            var types = new HashSet<string>(StringComparer.Ordinal);

            // Group by owning asset so the constraint map (LoadAllAssetsAtPath + full SerializedObject walk) is built
            // once per asset instead of once per broken reference.
            var byPath = new Dictionary<string, List<SerializeReferenceTypeUsageIndex.Usage>>(StringComparer.Ordinal);

            foreach (var usage in unresolved)
            {
                var key = SerializeReferenceHelpers.StoredTypeKey(usage.StoredType);
                if (!baseline.Contains(key)) continue;

                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (!byPath.TryGetValue(path, out var usages))
                {
                    usages = new List<SerializeReferenceTypeUsageIndex.Usage>();
                    byPath.Add(path, usages);
                }

                usages.Add(usage);
                types.Add(key);
            }

            foreach (var pair in byPath)
            {
                var path = pair.Key;
                var repairable = !string.IsNullOrEmpty(path) && !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

                Dictionary<(long fileId, long rid), Type> constraints = null;
                if (repairable)
                {
                    try
                    {
                        constraints = SerializeReferenceHelpers.BuildConstraintMap(path);
                    }
                    catch (Exception)
                    {
                        // Suggestion priming is best-effort; a parse miss must not suppress the breakage notice itself.
                    }
                }

                // The YAML repair edits RefIds blocks, so it cannot reach a type set by a prefab instance override.
                foreach (var usage in pair.Value)
                    entries.Add(BuildEntry(usage, path, repairable && !usage.IsOverride, constraints));
            }

            return entries.Count == 0 ? default : new BreakageReport(entries, types.Count);
        }

        private static BreakageEntry BuildEntry(
            SerializeReferenceTypeUsageIndex.Usage usage,
            string path,
            bool repairable,
            Dictionary<(long fileId, long rid), Type> constraints)
        {
            SerializeReferenceRepairSuggestions.RepairCandidate? top = null;
            if (repairable)
            {
                try
                {
                    var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(path, usage.FileId, usage.Rid);
                    Type constraint = null;
                    constraints?.TryGetValue((usage.FileId, usage.Rid), out constraint);

                    var ranked = SerializeReferenceRepairSuggestions.GetCached(path, usage.FileId, usage.Rid,
                        () => SerializeReferenceRepairSuggestions.Rank(usage.StoredType, fieldNames, constraint ?? typeof(object), 5));

                    if (ranked.Count > 0) top = ranked[0];
                }
                catch (Exception)
                {
                    // Suggestion priming is best-effort; a parse miss must not suppress the breakage notice itself.
                }
            }

            SerializeReferenceMovedFromResolver.TryResolve(usage.StoredType, out var migrationTarget);
            return new BreakageEntry(path, usage.FileId, usage.Rid, usage.StoredType, repairable, top, migrationTarget);
        }

        internal static IReadOnlyCollection<string> GetBaselineKeys(string assetPath) =>
            _baseline.Entries.TryGetValue(assetPath, out var keys) ? keys : Array.Empty<string>();

        // Tests snapshot, seed and restore the session's own baseline through these, and read what reached SessionState.
        internal static string ExportBaseline() => _baseline.Export();

        internal static void ImportBaseline(string raw) => _baseline.Import(raw);

        internal static void PersistBaseline() => _baseline.Persist();

        private static HashSet<string> CollectKeys(Dictionary<string, HashSet<string>> baseline)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var set in baseline.Values) keys.UnionWith(set);
            return keys;
        }

        private static void RemoveKeys(Dictionary<string, HashSet<string>> baseline, HashSet<string> keys)
        {
            foreach (var set in baseline.Values) set.ExceptWith(keys);
        }

        private static HashSet<string> GetOrAdd(Dictionary<string, HashSet<string>> baseline, string path)
        {
            if (baseline.TryGetValue(path, out var keys)) return keys;

            keys = new HashSet<string>(StringComparer.Ordinal);
            baseline.Add(path, keys);
            return keys;
        }
    }
}
