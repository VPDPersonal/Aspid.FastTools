using System;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceConstraintCache
    {
        private readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _maps = new(StringComparer.Ordinal);

        private int _loadedSinceUnload;

        // Null (unconstrained) for an orphaned payload or an unresolvable field type. Keyed by exact (fileId, rid),
        // since rids collide across documents.
        public Type Resolve(string assetPath, long fileId, long rid)
        {
            if (!_maps.TryGetValue(assetPath, out var map))
            {
                map = BuildMap(assetPath);
                _maps[assetPath] = map;
            }

            return map.GetValueOrDefault((fileId, rid));
        }

        public void Clear() => _maps.Clear();

        // A map loads every object of its asset, and a loaded asset stays in memory until it is unloaded, so a cache
        // that fills from hundreds of prefabs releases them in batches, as the project sweep does.
        private Dictionary<(long fileId, long rid), Type> BuildMap(string assetPath)
        {
            var wasLoaded = AssetDatabase.IsMainAssetAtPathLoaded(assetPath);
            var map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);

            if (wasLoaded || !AssetDatabase.IsMainAssetAtPathLoaded(assetPath)) return map;
            if (++_loadedSinceUnload < SerializeReferenceGateScanner.UnloadEveryLoadedFiles) return map;

            EditorUtility.UnloadUnusedAssetsImmediate();
            _loadedSinceUnload = 0;

            return map;
        }
    }
}
