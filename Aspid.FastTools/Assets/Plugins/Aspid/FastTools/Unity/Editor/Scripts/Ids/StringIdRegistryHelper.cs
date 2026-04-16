#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal class StringIdRegistryCacheInvalidator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Length > 0 || deleted.Length > 0 || moved.Length > 0)
                StringIdRegistryHelper.ClearCache();
        }
    }

    internal static class StringIdRegistryHelper
    {
        private static readonly Dictionary<string, IdRegistry?> _cache = new();

        internal static void ClearCache() => _cache.Clear();

        public static IdRegistry? FindRegistry(Type? declaringType)
        {
            if (declaringType == null) return null;

            var aqn = declaringType.AssemblyQualifiedName ?? string.Empty;
            if (_cache.TryGetValue(aqn, out var cached))
                return cached;

            var guids = AssetDatabase.FindAssets("t:IdRegistry");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var reg = AssetDatabase.LoadAssetAtPath<IdRegistry>(path);
                if (reg != null && reg.TargetStructType == aqn)
                {
                    _cache[aqn] = reg;
                    return reg;
                }
            }

            _cache[aqn] = null;
            return null;
        }

        public static IdRegistry CreateRegistry(Type? declaringType)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));

            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/StringIdRegistry_{declaringType.Name}.asset");
            var reg = ScriptableObject.CreateInstance<IdRegistry>();
            AssetDatabase.CreateAsset(reg, path);

            var so = new SerializedObject(reg);
            so.FindProperty("_targetStructType").stringValue = declaringType.AssemblyQualifiedName ?? string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();

            var aqn = declaringType.AssemblyQualifiedName ?? string.Empty;
            _cache[aqn] = reg;

            return reg;
        }
    }
}
