using System;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceTypeUsageIndexInvalidator : AssetPostprocessor
    {
        // Patching rereads an asset and walks the whole index for each one, while a reset costs nothing until the next
        // consumer rebuilds, so a bigger batch (a mass import from git) is dropped in one reset instead.
        private const int PatchLimit = 64;

        // Exclusion is consulted only while the index is built or the detector baselines are swept, so a warm copy would
        // keep serving now-excluded assets.
        [InitializeOnLoadMethod]
        private static void HookSettings()
        {
            SerializeReferenceSettings.ExcludedFoldersChanged += SerializeReferenceTypeUsageIndex.Reset;
            SerializeReferenceSettings.ExcludedFoldersChanged += SerializeReferenceBreakageDetector.ResetBaseline;
            SerializeReferenceSettings.ExcludedFoldersChanged += TypeNameBreakageDetector.ResetBaseline;
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // A cold index has nothing to patch; the next consumer builds it from the current assets.
            if (!SerializeReferenceTypeUsageIndex.IsWarm) return;

            Apply(imported, deleted, moved, movedFrom);
        }

        internal static void Apply(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // An in-place class rename reimports the .cs without touching any asset YAML, so no per-asset patch runs
            // and only a coarse reset re-evaluates the stale Resolves entries.
            if (HasScript(imported))
            {
                SerializeReferenceTypeUsageIndex.Reset();
                return;
            }

            // The index is keyed by guid, which a move keeps: only a move across the edge of the scanned set changes it.
            var rebuilt = imported.Where(SerializeReferenceHelpers.IsScanCandidate).ToList();
            var removed = deleted.Where(SerializeReferenceHelpers.IsScanCandidate).ToList();

            for (var i = 0; i < moved.Length; i++)
            {
                var wasScanned = i < movedFrom.Length && SerializeReferenceHelpers.IsScanCandidate(movedFrom[i]);
                var isScanned = SerializeReferenceHelpers.IsScanCandidate(moved[i]);

                if (wasScanned && !isScanned) removed.Add(moved[i]);
                else if (!wasScanned && isScanned) rebuilt.Add(moved[i]);
            }

            if (rebuilt.Count + removed.Count > PatchLimit || !SerializeReferenceTypeUsageIndex.RemoveAssets(removed))
            {
                SerializeReferenceTypeUsageIndex.Reset();
                return;
            }

            SerializeReferenceTypeUsageIndex.RebuildAssets(rebuilt);
        }

        private static bool HasScript(string[] paths) =>
            paths.Any(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
    }
}
