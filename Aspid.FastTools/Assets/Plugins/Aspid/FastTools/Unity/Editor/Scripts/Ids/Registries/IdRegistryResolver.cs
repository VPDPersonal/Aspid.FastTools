#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Finds and creates Id registry assets bound to a given IdStruct type.
    /// Searches both <see cref="IdRegistry"/> and <see cref="StringIdRegistry"/>,
    /// enforcing one-registry-per-type at lookup time.
    /// </summary>
    /// <remarks>
    /// Cache strategy: lazy single-rebuild on first <see cref="Find"/> after a reset,
    /// then point updates via <see cref="OnAssetImported"/> on subsequent imports.
    /// Asset deletion or move triggers a full reset and the next <c>Find</c> rescans.
    /// </remarks>
    internal static class IdRegistryResolver
    {
        private const string TargetStructTypeField = "_targetStructType";

        private static Dictionary<string, ScriptableObject>? _byAqn;
        private static bool _warmedUp;

        internal static void ClearCache()
        {
            _byAqn = null;
            _warmedUp = false;
        }

        public static ScriptableObject? Find(Type? declaringType)
        {
            if (declaringType == null) return null;

            EnsureWarmedUp();

            var aqn = declaringType.AssemblyQualifiedName ?? string.Empty;
            return _byAqn!.TryGetValue(aqn, out var registry) ? registry : null;
        }

        public static IdRegistry? FindIntOnly(Type? declaringType) =>
            Find(declaringType) as IdRegistry;

        public static StringIdRegistry? FindStringMapped(Type? declaringType) =>
            Find(declaringType) as StringIdRegistry;

        public static StringIdRegistry CreateStringMapped(Type declaringType)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));

            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/StringIdRegistry_{declaringType.Name}.asset");
            var reg = ScriptableObject.CreateInstance<StringIdRegistry>();
            AssetDatabase.CreateAsset(reg, path);

            var so = new SerializedObject(reg);
            so.FindProperty(TargetStructTypeField).stringValue = declaringType.AssemblyQualifiedName ?? string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();

            EnsureWarmedUp();
            _byAqn![declaringType.AssemblyQualifiedName ?? string.Empty] = reg;
            return reg;
        }

        /// <summary>
        /// Called by <see cref="IdRegistryResolverCacheInvalidator"/> on imports of
        /// <c>.asset</c> files. Updates the index in place when warmed up; a no-op otherwise
        /// (the next <see cref="Find"/> will rescan from scratch).
        /// </summary>
        internal static void OnAssetImported(string path)
        {
            if (!_warmedUp || _byAqn == null) return;

            var registry = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (registry is not (IdRegistry or StringIdRegistry)) return;

            var aqn = ReadTargetStructType(registry);

            // The asset's target type may have just changed: drop any prior AQN entry that
            // pointed at this registry so stale bindings don't survive the rename.
            RemoveEntriesPointingTo(registry, exceptKey: aqn);

            if (string.IsNullOrEmpty(aqn)) return;

            if (_byAqn.TryGetValue(aqn, out var existing) && existing != null && existing != registry)
            {
                Debug.LogError(
                    $"Multiple registries found for type AQN={aqn}: "
                    + $"{AssetDatabase.GetAssetPath(existing)}, {path}. "
                    + "Each IdStruct type must be bound to exactly one registry.");
                return;
            }

            _byAqn[aqn] = registry;
        }

        private static void RemoveEntriesPointingTo(ScriptableObject registry, string exceptKey)
        {
            if (_byAqn == null) return;

            List<string>? toRemove = null;
            foreach (var kv in _byAqn)
            {
                if (kv.Value != registry) continue;
                if (kv.Key == exceptKey) continue;
                toRemove ??= new List<string>();
                toRemove.Add(kv.Key);
            }
            if (toRemove == null) return;

            foreach (var key in toRemove)
                _byAqn.Remove(key);
        }

        /// <summary>Forces a full rescan on the next <see cref="Find"/> call.</summary>
        internal static void ResetWarmUp() => ClearCache();

        private static void EnsureWarmedUp()
        {
            if (_warmedUp && _byAqn != null) return;

            _byAqn = new Dictionary<string, ScriptableObject>();
            var duplicates = new Dictionary<string, List<string>>();

            foreach (var path in EnumerateRegistryPaths())
                TryRegisterAt(path, duplicates);

            ReportDuplicates(duplicates);
            _warmedUp = true;
        }

        private static void TryRegisterAt(string path, Dictionary<string, List<string>> duplicates)
        {
            var registry = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (registry == null) return;

            var aqn = ReadTargetStructType(registry);
            if (string.IsNullOrEmpty(aqn)) return;

            if (_byAqn!.TryGetValue(aqn, out var existing))
            {
                if (!duplicates.TryGetValue(aqn, out var list))
                {
                    list = new List<string> { AssetDatabase.GetAssetPath(existing) };
                    duplicates[aqn] = list;
                }
                list.Add(path);
                return;
            }

            _byAqn[aqn] = registry;
        }

        private static void ReportDuplicates(Dictionary<string, List<string>> duplicates)
        {
            foreach (var kv in duplicates)
            {
                Debug.LogError(
                    $"Multiple registries found for type AQN={kv.Key}: "
                    + string.Join(", ", kv.Value)
                    + ". Each IdStruct type must be bound to exactly one registry.");
            }
        }

        private static IEnumerable<string> EnumerateRegistryPaths()
        {
            var guids = AssetDatabase.FindAssets("t:IdRegistry t:StringIdRegistry");
            for (var i = 0; i < guids.Length; i++)
                yield return AssetDatabase.GUIDToAssetPath(guids[i]);
        }

        private static string ReadTargetStructType(ScriptableObject registry)
        {
            var so = new SerializedObject(registry);
            var prop = so.FindProperty(TargetStructTypeField);
            return prop != null ? prop.stringValue : string.Empty;
        }
    }
}
