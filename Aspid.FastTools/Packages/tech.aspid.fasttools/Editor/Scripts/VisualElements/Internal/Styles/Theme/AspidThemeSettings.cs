using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeSettings
    {
        // One theme and accent for every project of this machine, like the animated background; the override is an
        // asset of a project.
        private const string ThemeKey = "Aspid.FastTools.Appearance.Theme";
        private const string AccentKey = "Aspid.FastTools.Appearance.Accent";
        private const string LegacyOverrideStyleSheetGuidKey = "Aspid.FastTools.Theme.OverrideStyleSheetGuid";

        public static event Action Changed;

        private static string OverrideStyleSheetGuidKey =>
            "Aspid.FastTools.Theme.OverrideStyleSheetGuid." + PlayerSettings.productGUID;

        public static AspidTheme Theme
        {
            // The name is stored, not the number, so a reordered enum keeps the choice.
            get => Enum.TryParse<AspidTheme>(EditorPrefs.GetString(ThemeKey, defaultValue: string.Empty), out var theme)
                && Enum.IsDefined(typeof(AspidTheme), theme)
                    ? theme
                    : AspidTheme.Default;
            set
            {
                if (Theme == value) return;

                if (value is AspidTheme.Default) EditorPrefs.DeleteKey(ThemeKey);
                else EditorPrefs.SetString(ThemeKey, value.ToString());

                Changed?.Invoke();
            }
        }

        public static StyleSheet ThemeStyleSheet => Theme switch
        {
            AspidTheme.Venom => Resources.Load<StyleSheet>(AspidStyles.VenomThemeStyleSheet),
            _ => null,
        };

        public static AspidAccent Accent
        {
            get => Enum.TryParse<AspidAccent>(
                    EditorPrefs.GetString(AccentKey, defaultValue: string.Empty),
                    out var accent)
                && Enum.IsDefined(typeof(AspidAccent), accent)
                    ? accent
                    : AspidAccent.Green;
            set
            {
                if (Accent == value) return;

                if (value is AspidAccent.Green) EditorPrefs.DeleteKey(AccentKey);
                else EditorPrefs.SetString(AccentKey, value.ToString());

                Changed?.Invoke();
            }
        }

        // Each accent sheet is named after its enum member.
        public static StyleSheet AccentStyleSheet => Accent is AspidAccent.Green
            ? null
            : Resources.Load<StyleSheet>(AspidStyles.AccentStyleSheetPrefix + Accent);

        public static StyleSheet OverrideStyleSheet
        {
            get
            {
                var guid = OverrideStyleSheetGuid;
                if (string.IsNullOrEmpty(guid)) return null;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }
            set => OverrideStyleSheetGuid = value == null
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
        }

        private static string OverrideStyleSheetGuid
        {
            get
            {
                var value = EditorPrefs.GetString(OverrideStyleSheetGuidKey, string.Empty);
                if (value.Length > 0) return value;

                var legacy = EditorPrefs.GetString(LegacyOverrideStyleSheetGuidKey, string.Empty);
                if (legacy.Length == 0) return string.Empty;

                EditorPrefs.SetString(OverrideStyleSheetGuidKey, legacy);
                EditorPrefs.DeleteKey(LegacyOverrideStyleSheetGuidKey);
                return legacy;
            }
            set
            {
                value ??= string.Empty;
                if (OverrideStyleSheetGuid == value) return;

                if (string.IsNullOrEmpty(value)) EditorPrefs.DeleteKey(OverrideStyleSheetGuidKey);
                else EditorPrefs.SetString(OverrideStyleSheetGuidKey, value);

                Changed?.Invoke();
            }
        }
    }
}
