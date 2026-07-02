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
    /// <see cref="Type"/> are hidden from the loaded lists but kept in storage, so a type that is only
    /// transiently unresolvable (e.g. its assembly has not finished loading after a domain reload) reappears
    /// once it resolves again instead of being lost.
    /// </summary>
    internal static class TypeSelectorPreferences
    {
        private const string FavoritesKeyPrefix = "Aspid.FastTools.TypeSelector.Favorites.";
        private const string RecentsKeyPrefix = "Aspid.FastTools.TypeSelector.Recents.";

        private static string ProjectId => PlayerSettings.productGUID.ToString();

        private static string FavoritesKey => FavoritesKeyPrefix + ProjectId;
        private static string RecentsKey => RecentsKeyPrefix + ProjectId;

        // Membership cache for the favorites set, read once and reused across the many per-row IsFavorite calls a
        // single refresh fires. Invalidated whenever ToggleFavorite rewrites the store (the only writer in-session).
        private static HashSet<string> _favorites;

        private static HashSet<string> Favorites => _favorites ??= new HashSet<string>(LoadRaw(FavoritesKey));

        /// <summary>
        /// Returns the favorited assembly-qualified names that still resolve to a loadable type,
        /// in stored order.
        /// </summary>
        public static List<string> LoadFavorites() => LoadResolved(FavoritesKey);

        /// <summary>
        /// Returns the most-recently-used assembly-qualified names that still resolve to a loadable
        /// type, in MRU order (most recent first), capped at <see cref="TypeSelectorSettings.RecentsCapacity"/>
        /// so lowering the capacity hides the surplus immediately (the surplus stays stored until the
        /// next <see cref="RecordRecent"/> trims it).
        /// </summary>
        public static List<string> LoadRecents()
        {
            var resolved = LoadResolved(RecentsKey);
            var capacity = TypeSelectorSettings.RecentsCapacity;

            if (resolved.Count > capacity)
                resolved.RemoveRange(capacity, resolved.Count - capacity);

            return resolved;
        }

        /// <summary>Raw stored favorites count — includes entries that don't currently resolve.</summary>
        public static int FavoritesCount => LoadRaw(FavoritesKey).Count;

        /// <summary>Raw stored recents count — includes entries that don't currently resolve.</summary>
        public static int RecentsCount => LoadRaw(RecentsKey).Count;

        /// <summary>Drops every stored favorite, including entries kept for currently-unresolvable types.</summary>
        public static void ClearFavorites()
        {
            EditorPrefs.DeleteKey(FavoritesKey);
            _favorites = null;
        }

        /// <summary>Drops the whole recents history, including entries kept for currently-unresolvable types.</summary>
        public static void ClearRecents() => EditorPrefs.DeleteKey(RecentsKey);

        public static bool IsFavorite(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return false;
            return Favorites.Contains(assemblyQualifiedName);
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
            _favorites = null;
            return isFavorite;
        }

        /// <summary>
        /// Records a successful pick: moves <paramref name="assemblyQualifiedName"/> to the front of the
        /// recents list (deduplicated, capped at <see cref="TypeSelectorSettings.RecentsCapacity"/>).
        /// A capacity of 0 skips recording entirely — rather than trimming the store to empty — so the
        /// already-collected history survives the setting being turned back up.
        /// </summary>
        public static void RecordRecent(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return;

            var capacity = TypeSelectorSettings.RecentsCapacity;
            if (capacity <= 0) return;

            var entries = LoadRaw(RecentsKey);
            entries.Remove(assemblyQualifiedName);
            entries.Insert(0, assemblyQualifiedName);

            if (entries.Count > capacity)
                entries.RemoveRange(capacity, entries.Count - capacity);

            Save(RecentsKey, entries);
        }

        private static List<string> LoadResolved(string key)
        {
            var raw = LoadRaw(key);
            var resolved = new List<string>(raw.Count);

            foreach (var aqn in raw)
            {
                if (!string.IsNullOrEmpty(aqn) && Type.GetType(aqn, throwOnError: false) is not null)
                    resolved.Add(aqn);
            }

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

        [Serializable]
        private sealed class Store
        {
            public List<string> entries = new();
        }
    }
}
