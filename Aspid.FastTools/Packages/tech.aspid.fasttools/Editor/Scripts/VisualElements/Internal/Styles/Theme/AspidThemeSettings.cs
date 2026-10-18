using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeSettings
    {
        // A sheet in Assets with this label is a theme of the project: the Theme dropdown lists it under the built-in ones.
        public const string CustomThemeLabel = "AspidFastToolsTheme";

        // One built-in theme for every project of this machine, like the animated background; a custom theme is an asset
        // of a project, so its pick is kept per project and wins over the built-in theme there.
        private const string ThemeKey = "Aspid.FastTools.Appearance.Theme";

        public static event Action Changed;

        private static string CustomThemeGuidKey =>
            "Aspid.FastTools.Appearance.CustomTheme." + PlayerSettings.productGUID;

        public static AspidTheme Theme
        {
            // The name is stored, not the number, so a reordered enum keeps the choice.
            get => Enum.TryParse<AspidTheme>(EditorPrefs.GetString(ThemeKey, defaultValue: string.Empty), out var theme)
                && Enum.IsDefined(typeof(AspidTheme), theme)
                    ? theme
                    : AspidTheme.Default;
            set
            {
                // Picking a built-in theme leaves the custom theme of the project.
                var changed = SetCustomThemeGuid(string.Empty);

                if (Theme != value)
                {
                    if (value is AspidTheme.Default) EditorPrefs.DeleteKey(ThemeKey);
                    else EditorPrefs.SetString(ThemeKey, value.ToString());

                    changed = true;
                }

                if (changed) Changed?.Invoke();
            }
        }

        // The theme of the project picked in place of the built-in one; null when a built-in theme is drawn.
        public static StyleSheet CustomTheme
        {
            get
            {
                var guid = EditorPrefs.GetString(CustomThemeGuidKey, defaultValue: string.Empty);
                if (guid.Length == 0) return null;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }
            set
            {
                var guid = value == null
                    ? string.Empty
                    : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));

                if (SetCustomThemeGuid(guid)) Changed?.Invoke();
            }
        }

        // The sheet drawn over the built-in palette: the custom theme, else the sheet of the built-in theme.
        public static StyleSheet ThemeStyleSheet
        {
            get
            {
                var custom = CustomTheme;
                return custom != null ? custom : GetBuiltInStyleSheet(Theme);
            }
        }

        public static StyleSheet GetBuiltInStyleSheet(AspidTheme theme) => theme switch
        {
            AspidTheme.Venom => Resources.Load<StyleSheet>(AspidStyles.VenomThemeStyleSheet),
            AspidTheme.Unity => Resources.Load<StyleSheet>(AspidStyles.UnityThemeStyleSheet),
            _ => null,
        };

        // Only Assets is searched, so the sheets of packages, test sheets among them, stay out of the list.
        public static List<StyleSheet> FindCustomThemes() =>
            FindCustomThemes("Assets");

        internal static List<StyleSheet> FindCustomThemes(params string[] folders)
        {
            var themes = new List<StyleSheet>();

            foreach (var guid in AssetDatabase.FindAssets($"l:{CustomThemeLabel} t:StyleSheet", searchInFolders: folders))
            {
                var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));
                if (sheet != null) themes.Add(sheet);
            }

            themes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            return themes;
        }

        private static bool SetCustomThemeGuid(string guid)
        {
            if (EditorPrefs.GetString(CustomThemeGuidKey, defaultValue: string.Empty) == guid) return false;

            if (string.IsNullOrEmpty(guid)) EditorPrefs.DeleteKey(CustomThemeGuidKey);
            else EditorPrefs.SetString(CustomThemeGuidKey, guid);

            return true;
        }
    }
}
