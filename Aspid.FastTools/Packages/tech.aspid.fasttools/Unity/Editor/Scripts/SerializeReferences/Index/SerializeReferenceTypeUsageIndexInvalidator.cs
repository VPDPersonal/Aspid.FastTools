using System;
using System.Linq;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Keeps <see cref="SerializeReferenceTypeUsageIndex"/> incremental: patches a single asset's usages on import and
    /// coarsely resets on any candidate delete/move (a deleted path can no longer be resolved to a guid for a surgical
    /// strip). Mirrors the import-post-processor strategy of the Id system's cache invalidator.
    /// </summary>
    internal sealed class SerializeReferenceTypeUsageIndexInvalidator : AssetPostprocessor
    {
        // Editor-startup hook: a change to the excluded-scan-folder set must drop the warm index. IsExcluded is consulted
        // only while the index is (re)built (IsScanCandidate), so a warm index would keep serving now-excluded assets
        // until an unrelated import or a domain reload reset it. Reset is lazy — it nulls the index and the next lookup
        // rebuilds — so this is cheap and never warms a cold index on its own.
        [InitializeOnLoadMethod]
        private static void HookSettings() =>
            SerializeReferenceSettings.ExcludedFoldersChanged += SerializeReferenceTypeUsageIndex.Reset;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // An in-place class rename reimports the script (an edited .cs in `imported`, no file move). It changes which
            // stored types resolve without touching any asset YAML, so a surgical per-asset patch would never run and the
            // warm index would keep serving stale Resolves==true entries for the old type. A coarse reset is the only fix
            // that re-evaluates resolution across every cached usage; the next lookup rebuilds.
            if (HasCandidate(deleted) || HasCandidate(moved) || HasScript(imported))
            {
                SerializeReferenceTypeUsageIndex.Reset();
                return;
            }

            foreach (var asset in imported)
                if (SerializeReferenceHelpers.IsScanCandidate(asset))
                    SerializeReferenceTypeUsageIndex.RebuildAsset(asset);
        }

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceHelpers.IsScanCandidate);

        private static bool HasScript(string[] paths) =>
            paths.Any(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
    }
}
