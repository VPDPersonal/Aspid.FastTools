using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Observational safety net: detects managed references that JUST became missing (a script renamed/deleted) by
    /// diffing the current resolve state against a per-session baseline of what resolved before. Raises
    /// <see cref="BreakageDetected"/> with pre-ranked fix candidates — it never repairs anything itself.
    /// </summary>
    /// <remarks>
    /// The usage index is reset on every domain reload, so it cannot remember a prior state; the detector keeps its own
    /// baseline of resolvable stored-type keys in <see cref="SessionState"/>. The baseline is established silently on the
    /// first run of a session, so pre-existing breakages never alarm — only a key that WAS resolvable and is now missing
    /// is reported. Driven by <see cref="SerializeReferenceBreakageHook"/> on relevant asset/script changes.
    /// </remarks>
    internal static class SerializeReferenceBreakageDetector
    {
        /// <summary>Raised when one or more references became missing since the last scan.</summary>
        public static event Action<BreakageReport> BreakageDetected;

        private const string EstablishedKey = "Aspid.FastTools.SerializeReferences.Breakage.Established";
        private const string BaselineKey = "Aspid.FastTools.SerializeReferences.Breakage.Baseline";
        private const char BaselineSeparator = '\n';

        [InitializeOnLoadMethod]
        private static void EstablishBaselineOnce()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isBatchMode) return;
                if (SessionState.GetBool(EstablishedKey, false)) return;

                // First run of the session: record what currently resolves, report nothing (pre-existing breakages are
                // not "new").
                RunDetection(report: false);
            };
        }

        /// <summary>Re-scans and reports any newly-missing references. Called by the reimport hook on relevant changes.</summary>
        public static void Scan() => RunDetection(report: true);

        private static void RunDetection(bool report)
        {
            if (Application.isBatchMode) return;

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

            // Always advance the baseline to the current resolvable set so a key that just broke (now unresolved) drops
            // out and is never re-alarmed on the next scan.
            SaveBaseline(resolvable);
            SessionState.SetBool(EstablishedKey, true);

            if (result.HasAny) BreakageDetected?.Invoke(result);
        }

        // Builds the report from the unresolved usages whose stored type was resolvable in the baseline (i.e. just broke).
        private static BreakageReport BuildReport(
            List<SerializeReferenceTypeUsageIndex.Usage> unresolved,
            HashSet<string> baseline)
        {
            var entries = new List<BreakageEntry>();
            var types = new HashSet<string>(StringComparer.Ordinal);

            foreach (var usage in unresolved)
            {
                var key = SerializeReferenceHelpers.StoredTypeKey(usage.StoredType);
                if (!baseline.Contains(key)) continue; // was already broken (or never resolved) — not new

                entries.Add(BuildEntry(usage));
                types.Add(key);
            }

            return entries.Count == 0 ? default : new BreakageReport(entries, types.Count);
        }

        // Resolves the asset path, decides repairability and pre-ranks the best fix (priming the shared suggestion cache
        // so the Repair window shows Smart Fix without a delay).
        private static BreakageEntry BuildEntry(SerializeReferenceTypeUsageIndex.Usage usage)
        {
            var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
            var repairable = !string.IsNullOrEmpty(path) && !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

            SerializeReferenceRepairSuggestions.RepairCandidate? top = null;
            if (repairable)
            {
                try
                {
                    var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(path, usage.FileId, usage.Rid);
                    var constraints = SerializeReferenceHelpers.BuildConstraintMap(path);
                    constraints.TryGetValue((usage.FileId, usage.Rid), out var constraint);

                    var ranked = SerializeReferenceRepairSuggestions.GetCached(path, usage.FileId, usage.Rid,
                        () => SerializeReferenceRepairSuggestions.Rank(usage.StoredType, fieldNames, constraint ?? typeof(object), 5));

                    if (ranked.Count > 0) top = ranked[0];
                }
                catch (Exception)
                {
                    // Suggestion priming is best-effort; a parse miss must not suppress the breakage notice itself.
                }
            }

            return new BreakageEntry(path, usage.FileId, usage.Rid, usage.StoredType, repairable, top);
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
