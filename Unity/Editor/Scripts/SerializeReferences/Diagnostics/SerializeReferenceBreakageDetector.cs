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
    /// is reported. When the index is cold (an in-place rename forces a domain reload that resets it), the detector
    /// re-resolves the baseline keys directly rather than warming the index, so the rename still alarms type-level.
    /// Driven by <see cref="SerializeReferenceBreakageHook"/> on relevant asset/script changes.
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

            // Opt-out: when the user disables breakage detection, never establish a baseline or scan. Re-enabling
            // re-arms it on the next change — the first run silently re-baselines, so a pre-existing miss never alarms.
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            // Type resolution flaps while scripts recompile / the AssetDatabase updates, which would falsely alarm and
            // corrupt the baseline; defer until the editor is settled.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            // Never warm a cold index from the import / domain-reload path: that runs a modal full-project sweep on every
            // routine save (risk register 3/10). Detection is active only once the index is already warm (built by a
            // deliberate Find Usages / Project References scan) and kept warm incrementally on import. When cold — e.g.
            // after the domain reload an in-place class rename forces — fall back to re-resolving the prior baseline
            // keys directly (no index, no modal sweep) so the headline rename case still alarms; the explicit Project
            // References scan continues to find everything.
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

            // Always advance the baseline to the current resolvable set so a key that just broke (now unresolved) drops
            // out and is never re-alarmed on the next scan.
            SaveBaseline(resolvable);
            SessionState.SetBool(EstablishedKey, true);

            if (result.HasAny) BreakageDetected?.Invoke(result);
        }

        // Cold-index path (the index was reset by the domain reload an in-place rename forces): the index cannot be
        // enumerated and must not be warmed here, but the per-session baseline of resolvable stored-type keys survives in
        // SessionState. Each key is re-resolved directly via Type.GetType; a key that WAS resolvable and no longer
        // resolves is a type that just broke. The report is type-level only (no asset/rid sites — those need the index);
        // the toast/console warning still fire and the Repair window rebuilds the index to list the exact sites.
        private static void RunDetectionCold(bool report)
        {
            // No baseline yet means this is the session's first run with a cold index: there is nothing to compare
            // against, so establishing silently (no alarm for pre-existing breakages) waits for a warm scan.
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

                // Type-level entry: no asset path / file id / rid is available without the index. Consumers on this path
                // (the toast + console warning) read only the count and StoredType, and the Repair window rescans.
                entries.Add(new BreakageEntry(null, 0, 0, storedType, isRepairable: false, topSuggestion: null));
                brokenTypes.Add(key);
            }

            // Advance the baseline to the keys that still resolve so a type that just broke drops out and is never
            // re-alarmed on the next cold scan, mirroring the warm path.
            SaveBaseline(stillResolvable);

            if (entries.Count == 0) return;
            BreakageDetected?.Invoke(new BreakageReport(entries, brokenTypes.Count));
        }

        // Parses a "Assembly|Namespace|Class" baseline key (see SerializeReferenceHelpers.StoredTypeKey) back into a
        // ManagedTypeName for direct re-resolution. Returns false for a malformed key or one with no class identity.
        private static bool TryParseStoredTypeKey(string key, out ManagedTypeName storedType)
        {
            storedType = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('|');
            if (parts.Length != 3 || parts[2].Length == 0) return false;

            storedType = new ManagedTypeName(parts[0], parts[1], parts[2]);
            return true;
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
