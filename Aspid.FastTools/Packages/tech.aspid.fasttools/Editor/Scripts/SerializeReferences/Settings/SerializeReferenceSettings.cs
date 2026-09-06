using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The single source of truth for the SerializeReference toolset's configurable behaviors. The per-developer
    // breakage-detection toggle is persisted as project-scoped EditorPrefs JSON; the settings that must be identical
    // for every teammate and for CI live in the committed shared-settings asset instead.
    internal static class SerializeReferenceSettings
    {
        public static event Action Changed;

        // The precise signal the usage index listens for to drop its warm copy, since exclusion is consulted only
        // while the index is built. Kept apart from Changed so an unrelated setting never forces a costly rebuild.
        public static event Action ExcludedFoldersChanged;

        private const string KeyPrefix = "Aspid.FastTools.SerializeReference.Settings.";

        [Serializable]
        private sealed class Store
        {
            public bool breakageDetection = true;
        }

        private static Store _cache;

        private static string Key => KeyPrefix + PlayerSettings.productGUID;
        private static Store Data => _cache ??= Load();

        // Committed, not per-machine: duplicating a list element must behave the same for every teammate.
        public static bool AutoDeAliasEnabled
        {
            get => SerializeReferenceSharedSettings.instance.AutoDeAlias;
            set
            {
                var shared = SerializeReferenceSharedSettings.instance;
                if (shared.AutoDeAlias == value) return;

                shared.AutoDeAlias = value;
                Changed?.Invoke();
            }
        }

        public static bool BreakageDetectionEnabled
        {
            get => Data.breakageDetection;
            set
            {
                // Changed repaints every open editor window, so an idle write is not free.
                if (Data.breakageDetection == value) return;
                Data.breakageDetection = value;
                Save();
            }
        }

        // Committed, not per-machine: the index and the gate must scan the same folders everywhere.
        public static string[] ExcludedFolders
        {
            get => SerializeReferenceSharedSettings.instance.ExcludedFolders;
            set
            {
                var next = value ?? Array.Empty<string>();
                var shared = SerializeReferenceSharedSettings.instance;
                // Re-assigning the same paths must not fire the costly index reset.
                if (FoldersEqual(shared.ExcludedFolders, next)) return;

                shared.ExcludedFolders = next;
                Changed?.Invoke();
                ExcludedFoldersChanged?.Invoke();
            }
        }

        // Committed, so it travels to a clean CI runner instead of defaulting to Warn there.
        public static GateSeverity BuildSeverity
        {
            get => SerializeReferenceSharedSettings.instance.BuildSeverity;
            set
            {
                var shared = SerializeReferenceSharedSettings.instance;
                if (shared.BuildSeverity == value) return;

                shared.BuildSeverity = value;
                Changed?.Invoke();
            }
        }

        // Routed through the setters so each fires its usual change signals and no-ops when already at the default.
        public static void ResetSharedToDefaults()
        {
            AutoDeAliasEnabled = true;
            BuildSeverity = GateSeverity.Warn;
            ExcludedFolders = Array.Empty<string>();
        }

        public static void ResetUserToDefaults()
        {
            BreakageDetectionEnabled = true;
        }

        public static bool IsExcluded(string path)
        {
            var folders = SerializeReferenceSharedSettings.instance.ExcludedFolders;
            if (folders is null || folders.Length == 0 || string.IsNullOrEmpty(path)) return false;

            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder)) continue;
                var prefix = folder.EndsWith("/", StringComparison.Ordinal) ? folder : folder + "/";
                if (path.StartsWith(prefix, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        // Order-sensitive, so a reorder counts as a change; that only drops the warm index, which the next scan
        // rebuilds anyway.
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
