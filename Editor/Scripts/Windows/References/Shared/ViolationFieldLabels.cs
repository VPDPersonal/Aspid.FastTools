using System;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class ViolationFieldLabels
    {
        private readonly Dictionary<string, Object[]> _assets = new(StringComparer.Ordinal);

        public string Describe(GateViolation violation)
        {
            var component = ResolveComponentName(violation);
            return string.IsNullOrEmpty(component) ? violation.FieldPath : $"{component}.{violation.FieldPath}";
        }

        public string ResolveComponentName(GateViolation violation)
        {
            if (SerializeReferenceHelpers.IsScene(violation.AssetPath)) return string.Empty;

            if (!_assets.TryGetValue(violation.AssetPath, out var assets))
            {
                assets = AssetDatabase.LoadAllAssetsAtPath(violation.AssetPath);
                _assets[violation.AssetPath] = assets;
            }

            foreach (var asset in assets)
            {
                if (asset == null) continue;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out var fileId) && fileId == violation.FileId)
                    return asset.GetType().Name;
            }

            return string.Empty;
        }
    }
}
