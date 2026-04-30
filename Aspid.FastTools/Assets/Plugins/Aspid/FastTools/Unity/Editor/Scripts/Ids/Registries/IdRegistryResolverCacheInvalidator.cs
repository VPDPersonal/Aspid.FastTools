using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class IdRegistryResolverCacheInvalidator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (HasAssetPath(deleted) || HasAssetPath(moved))
            {
                IdRegistryResolver.ResetWarmUp();
                UniqueIdIndex.Reset();
                return;
            }

            for (var i = 0; i < imported.Length; i++)
            {
                if (!imported[i].EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;
                IdRegistryResolver.OnAssetImported(imported[i]);
                UniqueIdIndex.OnAssetChanged(imported[i]);
            }
        }

        private static bool HasAssetPath(string[] paths)
        {
            for (var i = 0; i < paths.Length; i++)
                if (paths[i].EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
