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
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (HasCandidate(deleted) || HasCandidate(moved))
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
    }
}
