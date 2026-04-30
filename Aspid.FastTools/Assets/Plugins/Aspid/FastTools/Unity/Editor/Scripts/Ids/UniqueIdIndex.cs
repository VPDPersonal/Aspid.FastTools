#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Project-wide index of asset GUIDs that use a given string id on a <c>[UniqueId]</c>
    /// field, keyed by the asset type that declares the field. Replaces the per-OnGUI
    /// <c>AssetDatabase.FindAssets</c> scan that the drawer used to perform.
    /// </summary>
    /// <remarks>
    /// Lazy build on first <see cref="IsUnique"/> call after a reset; point updates from
    /// <see cref="OnAssetChanged"/> on imports. The full reset path is reserved for
    /// asset deletion/move where guids change.
    /// </remarks>
    internal static class UniqueIdIndex
    {
        private static Dictionary<Type, Dictionary<string, HashSet<string>>>? _index;
        private static Dictionary<Type, FieldInfo[]>? _fieldsByType;

        public static bool IsUnique(Type? assetType, string? stringId, string currentAssetGuid)
        {
            if (assetType == null || string.IsNullOrEmpty(stringId)) return true;

            EnsureBuilt();

            if (!_index!.TryGetValue(assetType, out var byId)) return true;
            if (!byId.TryGetValue(stringId!, out var guids)) return true;
            if (guids.Count == 0) return true;
            return guids.Count == 1 && guids.Contains(currentAssetGuid);
        }

        internal static void OnAssetChanged(string path)
        {
            if (_index == null) return;

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) return;

            var assetType = asset.GetType();
            if (!TryGetFieldsFor(assetType, out var fields)) return;

            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            if (!_index.TryGetValue(assetType, out var byId))
            {
                byId = new Dictionary<string, HashSet<string>>();
                _index[assetType] = byId;
            }
            else
            {
                foreach (var bucket in byId.Values)
                    bucket.Remove(guid);
            }

            var so = new SerializedObject(asset);
            foreach (var field in fields)
            {
                var prop = so.FindProperty($"{field.Name}.{Constants.StringIdFieldName}");
                if (prop == null) continue;
                AddToIndex(byId, prop.stringValue, guid);
            }
        }

        internal static void Reset()
        {
            _index = null;
            _fieldsByType = null;
        }

        private static void EnsureBuilt()
        {
            if (_index != null) return;

            _index = new Dictionary<Type, Dictionary<string, HashSet<string>>>();
            _fieldsByType = BuildFieldsByType();

            foreach (var (assetType, fields) in _fieldsByType)
            {
                var byId = new Dictionary<string, HashSet<string>>();
                _index[assetType] = byId;

                var guids = AssetDatabase.FindAssets($"t:{assetType.FullName}");
                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = AssetDatabase.LoadAssetAtPath(path, assetType);
                    if (asset == null) continue;

                    var so = new SerializedObject(asset);
                    foreach (var field in fields)
                    {
                        var prop = so.FindProperty($"{field.Name}.{Constants.StringIdFieldName}");
                        if (prop == null) continue;
                        AddToIndex(byId, prop.stringValue, guids[i]);
                    }
                }
            }
        }

        private static Dictionary<Type, FieldInfo[]> BuildFieldsByType()
        {
            var grouped = new Dictionary<Type, List<FieldInfo>>();
            foreach (var field in TypeCache.GetFieldsWithAttribute<UniqueIdAttribute>())
            {
                if (field.DeclaringType == null) continue;
                if (!grouped.TryGetValue(field.DeclaringType, out var list))
                {
                    list = new List<FieldInfo>();
                    grouped[field.DeclaringType] = list;
                }
                list.Add(field);
            }

            var result = new Dictionary<Type, FieldInfo[]>(grouped.Count);
            foreach (var (type, list) in grouped)
                result[type] = list.ToArray();
            return result;
        }

        private static bool TryGetFieldsFor(Type assetType, out FieldInfo[] fields)
        {
            _fieldsByType ??= BuildFieldsByType();
            return _fieldsByType.TryGetValue(assetType, out fields!);
        }

        private static void AddToIndex(Dictionary<string, HashSet<string>> byId, string stringId, string guid)
        {
            if (string.IsNullOrEmpty(stringId)) return;
            if (!byId.TryGetValue(stringId, out var set))
            {
                set = new HashSet<string>();
                byId[stringId] = set;
            }
            set.Add(guid);
        }
    }
}
