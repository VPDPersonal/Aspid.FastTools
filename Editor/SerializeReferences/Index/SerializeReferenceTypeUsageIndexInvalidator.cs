using System;
using System.Linq;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Keeps the usage index incremental: one asset's usages are patched on import, while a delete or move resets it
    // coarsely, since a deleted path can no longer be resolved to a guid for a surgical strip.
    internal sealed class SerializeReferenceTypeUsageIndexInvalidator : AssetPostprocessor
    {
        // Exclusion is consulted only while the index is built, so a warm one would keep serving now-excluded assets.
        [InitializeOnLoadMethod]
        private static void HookSettings() =>
            SerializeReferenceSettings.ExcludedFoldersChanged += SerializeReferenceTypeUsageIndex.Reset;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // An in-place class rename reimports the .cs without touching any asset YAML, so no per-asset patch runs
            // and only a coarse reset re-evaluates the stale Resolves entries.
            if (HasCandidate(deleted) || HasCandidate(moved) || HasScript(imported))
            {
                SerializeReferenceTypeUsageIndex.Reset();
                return;
            }

            foreach (var asset in imported)
            {
                if (SerializeReferenceHelpers.IsScanCandidate(asset))
                    SerializeReferenceTypeUsageIndex.RebuildAsset(asset);
            }
        }

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceHelpers.IsScanCandidate);

        private static bool HasScript(string[] paths) =>
            paths.Any(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
    }
}
