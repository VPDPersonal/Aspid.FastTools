using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Per-project persistence for the type selector's Favorites and Recents, stored as JSON in
    /// <see cref="EditorPrefs"/> under project-scoped keys. Entries that no longer resolve to a
    /// <see cref="Type"/> are dropped silently on load.
    /// </summary>
    internal static class TypeSelectorPreferences
    {
        private const int RecentsCapacity = 8;

        private const string FavoritesKeyPrefix = "Aspid.FastTools.TypeSelector.Favorites.";
        private const string RecentsKeyPrefix = "Aspid.FastTools.TypeSelector.Recents.";

        [Serializable]
        private sealed class Store
        {
            public List<string> entries = new();
        }

        private static string ProjectId => PlayerSettings.productGUID.ToString();

        private static string FavoritesKey => FavoritesKeyPrefix + ProjectId;
        private static string RecentsKey => RecentsKeyPrefix + ProjectId;

        /// <summary>
        /// Returns the favorited assembly-qualified names that still resolve to a loadable type,
        /// in stored order.
        /// </summary>
        public static List<string> LoadFavorites() => LoadResolved(FavoritesKey);

        /// <summary>
        /// Returns the most-recently-used assembly-qualified names that still resolve to a loadable
        /// type, in MRU order (most recent first).
        /// </summary>
        public static List<string> LoadRecents() => LoadResolved(RecentsKey);

        public static bool IsFavorite(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return false;
            return LoadRaw(FavoritesKey).Contains(assemblyQualifiedName);
        }

        /// <summary>
        /// Adds or removes <paramref name="assemblyQualifiedName"/> from the favorites set and persists
        /// the change. New favorites are appended to the end. Returns the new favorite state.
        /// </summary>
        public static bool ToggleFavorite(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return false;

            var entries = LoadRaw(FavoritesKey);

            bool isFavorite;
            if (entries.Remove(assemblyQualifiedName))
            {
                isFavorite = false;
            }
            else
            {
                entries.Add(assemblyQualifiedName);
                isFavorite = true;
            }

            Save(FavoritesKey, entries);
            return isFavorite;
        }

        /// <summary>
        /// Records a successful pick: moves <paramref name="assemblyQualifiedName"/> to the front of the
        /// recents list (deduplicated, capped at <see cref="RecentsCapacity"/>).
        /// </summary>
        public static void RecordRecent(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return;

            var entries = LoadRaw(RecentsKey);
            entries.Remove(assemblyQualifiedName);
            entries.Insert(0, assemblyQualifiedName);

            if (entries.Count > RecentsCapacity)
                entries.RemoveRange(RecentsCapacity, entries.Count - RecentsCapacity);

            Save(RecentsKey, entries);
        }

        private static List<string> LoadResolved(string key)
        {
            var raw = LoadRaw(key);
            var resolved = new List<string>(raw.Count);
            var changed = false;

            foreach (var aqn in raw)
            {
                if (!string.IsNullOrEmpty(aqn) && Type.GetType(aqn, throwOnError: false) is not null)
                    resolved.Add(aqn);
                else
                    changed = true;
            }

            // Prune unresolved entries so the store self-heals over time.
            if (changed) Save(key, resolved);

            return resolved;
        }

        private static List<string> LoadRaw(string key)
        {
            var json = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new List<string>();

            try
            {
                var store = JsonUtility.FromJson<Store>(json);
                return store?.entries ?? new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private static void Save(string key, List<string> entries)
        {
            var store = new Store { entries = entries };
            EditorPrefs.SetString(key, JsonUtility.ToJson(store));
        }
    }
}
