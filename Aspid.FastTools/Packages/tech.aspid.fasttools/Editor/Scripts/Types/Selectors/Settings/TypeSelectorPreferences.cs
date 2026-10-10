using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeSelectorPreferences
    {
        private const string RecentsKeyPrefix = "Aspid.FastTools.TypeSelector.Recents.";
        private const string FavoritesKeyPrefix = "Aspid.FastTools.TypeSelector.Favorites.";

        private static HashSet<string> _favorites;

        internal static int RecentsCount => LoadRaw(RecentsKey).Count;

        internal static int FavoritesCount => LoadRaw(FavoritesKey).Count;

        internal static string RecentsKey => RecentsKeyPrefix + ProjectId;

        internal static string FavoritesKey => FavoritesKeyPrefix + ProjectId;

        private static string ProjectId => PlayerSettings.productGUID.ToString();

        private static HashSet<string> Favorites => _favorites ??= new HashSet<string>(LoadRaw(FavoritesKey));

        // The capacity applies after accepts: the store holds picks of every field, and only those the open picker
        // offers may use up the visible slots.
        internal static List<string> LoadRecents(Predicate<string> accepts = null)
        {
            var resolved = LoadResolved(RecentsKey);
            var capacity = TypeSelectorSettings.RecentsCapacity;

            if (accepts is not null)
                resolved.RemoveAll(aqn => !accepts(aqn));

            if (resolved.Count > capacity)
                resolved.RemoveRange(capacity, resolved.Count - capacity);

            return resolved;
        }

        internal static List<string> LoadFavorites() =>
            LoadResolved(FavoritesKey);

        private static List<string> LoadResolved(string key) =>
            LoadRaw(key).Where(aqn => TypeUtility.GetTypeOrNull(aqn) is not null).ToList();

        private static List<string> LoadRaw(string key)
        {
            var json = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();

            try
            {
                var store = JsonUtility.FromJson<Store>(json);
                return store?.Entries ?? new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        internal static bool IsFavorite(string assemblyQualifiedName) =>
            !string.IsNullOrWhiteSpace(assemblyQualifiedName)
            && Favorites.Contains(assemblyQualifiedName);

        internal static bool ToggleFavorite(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return false;
            var entries = LoadRaw(FavoritesKey);

            var isFavorite = false;
            if (!entries.Remove(assemblyQualifiedName))
            {
                entries.Add(assemblyQualifiedName);
                isFavorite = true;
            }

            Save(FavoritesKey, entries);

            _favorites = null;
            return isFavorite;
        }

        internal static void RecordRecent(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return;

            var capacity = TypeSelectorSettings.RecentsCapacity;
            if (capacity <= 0) return;

            var entries = LoadRaw(RecentsKey);
            entries.Remove(assemblyQualifiedName);
            entries.Insert(0, assemblyQualifiedName);

            // Trimmed to the maximum, not the capacity: picks of other fields would push out the ones this
            // picker can still show.
            const int max = TypeSelectorSettings.MaxRecentsCapacity;
            if (entries.Count > max)
                entries.RemoveRange(max, entries.Count - max);

            Save(RecentsKey, entries);
        }

        internal static void ClearRecents() =>
            EditorPrefs.DeleteKey(RecentsKey);

        internal static void ClearFavorites()
        {
            EditorPrefs.DeleteKey(FavoritesKey);
            _favorites = null;
        }

        private static void Save(string key, List<string> entries)
        {
            var store = new Store
            {
                Entries = entries
            };

            EditorPrefs.SetString(key, JsonUtility.ToJson(store));
        }

        [Serializable]
        private sealed class Store
        {
            public List<string> Entries = new();
        }
    }
}
