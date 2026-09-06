using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Per-asset memo over BuildConstraintMap, so a repair surface's many lookups cost one scan per asset — building
    // one map is a LoadAllAssetsAtPath plus a full SerializedObject walk. Clear it after any edit that rewrote the
    // YAML, or the next lookup answers from the pre-edit file.
    internal sealed class SerializeReferenceConstraintCache
    {
        private readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _maps = new(StringComparer.Ordinal);

        // Null (unconstrained) for an orphaned payload or an unresolvable field type. Keyed by exact (fileId, rid),
        // since rids collide across documents.
        public Type Resolve(string assetPath, long fileId, long rid)
        {
            if (!_maps.TryGetValue(assetPath, out var map))
            {
                map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);
                _maps[assetPath] = map;
            }

            return map.GetValueOrDefault((fileId, rid));
        }

        public void Clear() => _maps.Clear();
    }
}
