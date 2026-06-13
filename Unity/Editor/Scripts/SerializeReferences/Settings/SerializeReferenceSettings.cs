using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>Build/CI gate severity for missing or unset-required managed references.</summary>
    internal enum GateSeverity
    {
        Off,
        Warn,
        Fail,
    }

    /// <summary>
    /// The single source of truth for the SerializeReference toolset's configurable behaviors, persisted as JSON in
    /// project-scoped <see cref="EditorPrefs"/> (keyed by <see cref="PlayerSettings.productGUID"/>, the established
    /// package pattern). Edited through the Project Settings page; read by the rid-colour drawers, the de-alias guard,
    /// the project scanners and the build/CI gate. Fires <see cref="Changed"/> so open inspectors can repaint live.
    /// </summary>
    internal static class SerializeReferenceSettings
    {
        /// <summary>Raised whenever a setting changes.</summary>
        public static event Action Changed;

        private const string KeyPrefix = "Aspid.FastTools.SerializeReference.Settings.";

        [Serializable]
        private sealed class Store
        {
            public bool ridColors = true;
            public bool autoDeAlias = true;
            public string[] excludedFolders = Array.Empty<string>();
            public int buildSeverity = (int)GateSeverity.Warn;
        }

        private static Store _cache;

        private static string Key => KeyPrefix + PlayerSettings.productGUID;
        private static Store Data => _cache ??= Load();

        public static bool RidColorsEnabled
        {
            get => Data.ridColors;
            set { Data.ridColors = value; Save(); }
        }

        public static bool AutoDeAliasEnabled
        {
            get => Data.autoDeAlias;
            set { Data.autoDeAlias = value; Save(); }
        }

        public static string[] ExcludedFolders
        {
            get => Data.excludedFolders ?? Array.Empty<string>();
            set { Data.excludedFolders = value ?? Array.Empty<string>(); Save(); }
        }

        public static GateSeverity BuildSeverity
        {
            get => (GateSeverity)Data.buildSeverity;
            set { Data.buildSeverity = (int)value; Save(); }
        }

        /// <summary>True when <paramref name="path"/> lies under one of the excluded scan folders.</summary>
        public static bool IsExcluded(string path)
        {
            var folders = Data.excludedFolders;
            if (folders is null || folders.Length == 0 || string.IsNullOrEmpty(path)) return false;

            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder)) continue;
                var prefix = folder.EndsWith("/", StringComparison.Ordinal) ? folder : folder + "/";
                if (path.StartsWith(prefix, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        private static Store Load()
        {
            var json = EditorPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new Store();

            try
            {
                return JsonUtility.FromJson<Store>(json) ?? new Store();
            }
            catch (Exception)
            {
                return new Store();
            }
        }

        private static void Save()
        {
            EditorPrefs.SetString(Key, JsonUtility.ToJson(_cache));
            Changed?.Invoke();
        }
    }
}
