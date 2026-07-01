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
    /// The single source of truth for the SerializeReference toolset's configurable behaviors. The purely cosmetic,
    /// per-developer breakage-detection toggle is persisted as JSON in project-scoped <see cref="EditorPrefs"/> (keyed
    /// by <see cref="PlayerSettings.productGUID"/>, the established package pattern); the settings that must be
    /// identical for every teammate and for CI (auto-de-alias, excluded scan folders, the build/CI gate) live in the
    /// committed <see cref="SerializeReferenceSharedSettings"/> asset instead. Rid colours are not configurable —
    /// they always identify a shared reference by colour, so there is nothing to opt out of. Edited through the
    /// Project Settings page; read by the de-alias guard, the project scanners and the build/CI gate. Fires
    /// <see cref="Changed"/> so open inspectors can repaint live.
    /// </summary>
    internal static class SerializeReferenceSettings
    {
        /// <summary>Raised whenever a setting changes.</summary>
        public static event Action Changed;

        /// <summary>
        /// Raised only when the <see cref="ExcludedFolders"/> set actually changes — the precise signal the usage index
        /// listens for to drop its warm copy, since <see cref="IsExcluded"/> is consulted only while the index is
        /// (re)built. Kept separate from <see cref="Changed"/> so an unrelated setting (the gate) never triggers a
        /// costly index rebuild.
        /// </summary>
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

        /// <summary>
        /// Persisted in <see cref="SerializeReferenceSharedSettings"/> (committed asset), not <see cref="EditorPrefs"/>
        /// — duplicating a list element must behave the same for every teammate, regardless of who set this.
        /// </summary>
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
            set { Data.breakageDetection = value; Save(); }
        }

        /// <summary>
        /// Persisted in <see cref="SerializeReferenceSharedSettings"/> (committed asset), not <see cref="EditorPrefs"/>
        /// — the usage index and the build/CI gate must scan the same folders for every teammate and on CI.
        /// </summary>
        public static string[] ExcludedFolders
        {
            get => SerializeReferenceSharedSettings.instance.ExcludedFolders;
            set
            {
                var next = value ?? Array.Empty<string>();
                var shared = SerializeReferenceSharedSettings.instance;
                // Detect a genuine change before persisting so the index reset (and its lazy rebuild) only fires when
                // the exclusion set really moved — re-assigning the same paths leaves the warm index untouched.
                if (FoldersEqual(shared.ExcludedFolders, next)) return;

                shared.ExcludedFolders = next;
                Changed?.Invoke();
                ExcludedFoldersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Build/CI gate severity. Persisted in <see cref="SerializeReferenceSharedSettings"/> (committed asset) so it
        /// travels to a clean CI runner instead of defaulting to <see cref="GateSeverity.Warn"/> there. Still fires
        /// <see cref="Changed"/> so open inspectors repaint live.
        /// </summary>
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

        /// <summary>True when <paramref name="path"/> lies under one of the excluded scan folders.</summary>
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
