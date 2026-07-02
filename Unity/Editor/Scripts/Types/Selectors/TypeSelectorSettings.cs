using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// The type selector's configurable behaviors: whether the root page composes the Favorites section and how many
    /// picks the Recent list keeps (0 doubles as the Recent section's off switch — the section only composes when it
    /// has rows, so no separate toggle exists). All of these are individual workflow preferences — they change what
    /// one developer sees in their own picker, never how the project serializes — so unlike the SerializeReference
    /// settings asset they are persisted as JSON in project-scoped <see cref="EditorPrefs"/> (keyed by
    /// <see cref="PlayerSettings.productGUID"/>, the established package pattern) and are never committed. Edited from
    /// the SerializeReference window's Settings tab; read by <see cref="NavigationController"/> (section composition)
    /// and <see cref="TypeSelectorPreferences"/> (recents capacity). Fires <see cref="Changed"/> so the settings
    /// controls mirror each other live.
    /// </summary>
    internal static class TypeSelectorSettings
    {
        /// <summary>Raised whenever a setting changes.</summary>
        public static event Action Changed;

        /// <summary>Default number of picks the Recent list keeps.</summary>
        public const int DefaultRecentsCapacity = 5;

        /// <summary>Upper bound for <see cref="RecentsCapacity"/> — past this the section stops being "recent".</summary>
        public const int MaxRecentsCapacity = 20;

        private const string KeyPrefix = "Aspid.FastTools.TypeSelector.Settings.";

        [Serializable]
        private sealed class Store
        {
            public bool showFavorites = true;
            public int recentsCapacity = DefaultRecentsCapacity;
        }

        private static Store _cache;

        private static string Key => KeyPrefix + PlayerSettings.productGUID;
        private static Store Data => _cache ??= Load();

        /// <summary>Whether the picker's root page composes the ★ Favorites section. Hiding the section does not
        /// touch the stored favorites (nor the per-row ★ toggle), so flipping it back restores the same list.</summary>
        public static bool ShowFavorites
        {
            get => Data.showFavorites;
            set
            {
                if (Data.showFavorites == value) return;
                Data.showFavorites = value;
                Save();
            }
        }

        /// <summary>
        /// How many picks the Recent list keeps (MRU, clamped to 0..<see cref="MaxRecentsCapacity"/>). 0 is the
        /// Recent section's off switch: the section disappears and recording pauses, but the already-stored history
        /// is kept, not wiped, so raising the capacity back brings it back.
        /// </summary>
        public static int RecentsCapacity
        {
            get => Mathf.Clamp(Data.recentsCapacity, 0, MaxRecentsCapacity);
            set
            {
                var clamped = Mathf.Clamp(value, 0, MaxRecentsCapacity);
                if (Data.recentsCapacity == clamped) return;
                Data.recentsCapacity = clamped;
                Save();
            }
        }

        /// <summary>
        /// Restores every picker setting to its default: the Favorites section on, the recents capacity back at
        /// <see cref="DefaultRecentsCapacity"/> (matching the <see cref="Store"/> field initializers a fresh machine
        /// starts from). Routed through the public setters so a real change fires <see cref="Changed"/> exactly once
        /// per moved value. The stored Favorites / Recent lists are preference data, not settings — resetting leaves
        /// them alone (the Settings tab's Saved-lists row clears them explicitly).
        /// </summary>
        public static void ResetToDefaults()
        {
            ShowFavorites = true;
            RecentsCapacity = DefaultRecentsCapacity;
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
