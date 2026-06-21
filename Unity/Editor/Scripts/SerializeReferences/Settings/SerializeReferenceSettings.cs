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

        /// <summary>
        /// Raised only when the <see cref="ExcludedFolders"/> set actually changes — the precise signal the usage index
        /// listens for to drop its warm copy, since <see cref="IsExcluded"/> is consulted only while the index is
        /// (re)built. Kept separate from <see cref="Changed"/> so an unrelated setting (rid colours, the gate) never
        /// triggers a costly index rebuild.
        /// </summary>
        public static event Action ExcludedFoldersChanged;

        private const string KeyPrefix = "Aspid.FastTools.SerializeReference.Settings.";

        [Serializable]
        private sealed class Store
        {
            public bool ridColors = true;
            public bool autoDeAlias = true;
            public bool breakageDetection = true;
            public string[] excludedFolders = Array.Empty<string>();
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

        public static bool BreakageDetectionEnabled
        {
            get => Data.breakageDetection;
            set { Data.breakageDetection = value; Save(); }
        }

        public static string[] ExcludedFolders
        {
            get => Data.excludedFolders ?? Array.Empty<string>();
            set
            {
                var next = value ?? Array.Empty<string>();
                // Detect a genuine change before persisting so the index reset (and its lazy rebuild) only fires when
                // the exclusion set really moved — re-assigning the same paths leaves the warm index untouched.
                var changed = !FoldersEqual(Data.excludedFolders, next);
                Data.excludedFolders = next;
                Save();
                if (changed) ExcludedFoldersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Build/CI gate severity. Unlike every other setting here (per-machine <see cref="EditorPrefs"/>), this is
        /// persisted in a committed <see cref="SerializeReferenceGateSettings"/> asset so it travels to a clean CI
        /// runner instead of defaulting to <see cref="GateSeverity.Warn"/> there. Still fires <see cref="Changed"/>
        /// so open inspectors repaint live.
        /// </summary>
        public static GateSeverity BuildSeverity
        {
            get => SerializeReferenceGateSettings.instance.BuildSeverity;
            set
            {
                var gate = SerializeReferenceGateSettings.instance;
                if (gate.BuildSeverity == value) return;

                gate.BuildSeverity = value;
                Changed?.Invoke();
            }
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

        // Ordinal, order-sensitive set comparison (null treated as empty). A reorder counts as a change too — harmless,
        // since it only drops the warm index, which the next scan rebuilds.
        private static bool FoldersEqual(string[] a, string[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            var lengthA = a?.Length ?? 0;
            var lengthB = b?.Length ?? 0;
            if (lengthA != lengthB) return false;

            for (var i = 0; i < lengthA; i++)
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;

            return true;
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
