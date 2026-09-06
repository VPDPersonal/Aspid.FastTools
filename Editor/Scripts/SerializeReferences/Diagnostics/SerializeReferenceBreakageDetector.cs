using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Detects managed references that JUST became missing by diffing the current resolve state against a per-session
    // baseline kept in SessionState — the usage index is wiped on every domain reload and cannot remember one. The
    // baseline is established silently on the first run, so pre-existing breakages never alarm. Reports only; it
    // never repairs anything itself.
    internal static class SerializeReferenceBreakageDetector
    {
        private const string EstablishedKey = "Aspid.FastTools.SerializeReferences.Breakage.Established";
        private const string BaselineKey = "Aspid.FastTools.SerializeReferences.Breakage.Baseline";
        private const char BaselineSeparator = '\n';

        public static event Action<BreakageReport> BreakageDetected;

        [InitializeOnLoadMethod]
        private static void EstablishBaselineOnce() => EditorApplication.delayCall += () =>
        {
            if (Application.isBatchMode) return;
            if (SessionState.GetBool(EstablishedKey, false)) return;

            // First run of the session: record what resolves now; pre-existing breakages are not "new".
            RunDetection(report: false);
        };

        public static void Scan() => RunDetection(report: true);

        private static void RunDetection(bool report)
        {
            if (Application.isBatchMode) return;

            // Opt-out: never baseline or scan while disabled; re-enabling silently re-baselines on the next change.
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            // Type resolution flaps while scripts compile, so defer (never drop) until the editor settles.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += () => RunDetection(report);
                return;
            }

            // Warming the index here would mean a modal full-project sweep on routine saves, so a cold index falls
            // back to re-resolving the baseline keys directly.
            if (!SerializeReferenceTypeUsageIndex.IsWarm)
            {
                RunDetectionCold(report);
                return;
            }

            var resolvable = new HashSet<string>(StringComparer.Ordinal);
            var unresolved = new List<SerializeReferenceTypeUsageIndex.Usage>();

            foreach (var usage in SerializeReferenceTypeUsageIndex.AllUsages())
            {
                if (usage.Resolves) resolvable.Add(SerializeReferenceHelpers.StoredTypeKey(usage.StoredType));
                else unresolved.Add(usage);
            }

            var established = SessionState.GetBool(EstablishedKey, false);
            BreakageReport result = default;

            if (established && report)
            {
                var baseline = LoadBaseline();
                result = BuildReport(unresolved, baseline);
            }

            // Advance the baseline so a key that just broke drops out and is never re-alarmed on the next scan.
            SaveBaseline(resolvable);
            SessionState.SetBool(EstablishedKey, true);

            if (result.HasAny) BreakageDetected?.Invoke(result);
        }

        // Cold-index path: each baseline key is re-resolved directly, so the report is type-level only — the Repair
        // window rebuilds the index to list the exact sites.
        private static void RunDetectionCold(bool report)
        {
            // No baseline yet — nothing to compare against; establishing silently waits for a warm scan.
            if (!report || !SessionState.GetBool(EstablishedKey, false)) return;

            var baseline = LoadBaseline();
            if (baseline.Count == 0) return;

            var entries = new List<BreakageEntry>();
            var brokenTypes = new HashSet<string>(StringComparer.Ordinal);
            var stillResolvable = new HashSet<string>(StringComparer.Ordinal);

            foreach (var key in baseline)
            {
                if (!TryParseStoredTypeKey(key, out var storedType)) continue;

                if (SerializeReferenceHelpers.StoredTypeResolves(storedType))
                {
                    stillResolvable.Add(key);
                    continue;
                }

                // The [MovedFrom] resolver needs no index, so even the cold path tells a rename from a real breakage.
                SerializeReferenceMovedFromResolver.TryResolve(storedType, out var migrationTarget);
                entries.Add(new BreakageEntry(null, 0, 0, storedType, isRepairable: false, topSuggestion: null,
                    migrationTarget));
                brokenTypes.Add(key);
            }

            // Advance the baseline so a just-broken type is never re-alarmed, mirroring the warm path.
            SaveBaseline(stillResolvable);

            if (entries.Count == 0) return;
            BreakageDetected?.Invoke(new BreakageReport(entries, brokenTypes.Count));
        }

        // Parses an "Assembly|Namespace|Class" key (see SerializeReferenceHelpers.StoredTypeKey) back into a ManagedTypeName.
        private static bool TryParseStoredTypeKey(string key, out ManagedTypeName storedType)
        {
            storedType = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('|');
            if (parts.Length != 3 || parts[2].Length == 0) return false;

            storedType = new ManagedTypeName(parts[0], parts[1], parts[2]);
            return true;
        }

        // Reports only the unresolved usages whose stored type was still resolvable in the baseline.
        private static BreakageReport BuildReport(
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
                if (!baseline.Contains(key)) continue; // was already broken (or never resolved) — not new

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

                foreach (var usage in pair.Value)
                    entries.Add(BuildEntry(usage, path, repairable, constraints));
            }

            return entries.Count == 0 ? default : new BreakageReport(entries, types.Count);
        }

        // Pre-ranks the best fix, priming the shared suggestion cache so the Repair window shows Smart Fix without delay.
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

        private static HashSet<string> LoadBaseline()
        {
            var raw = SessionState.GetString(BaselineKey, string.Empty);
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(raw)) return set;

            foreach (var key in raw.Split(BaselineSeparator))
                if (key.Length > 0) set.Add(key);

            return set;
        }

        private static void SaveBaseline(HashSet<string> resolvable) =>
            SessionState.SetString(BaselineKey, string.Join(BaselineSeparator.ToString(), resolvable));
    }
}
