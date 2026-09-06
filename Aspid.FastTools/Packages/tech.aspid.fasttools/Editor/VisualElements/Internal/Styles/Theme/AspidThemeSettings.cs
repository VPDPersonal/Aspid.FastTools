using System;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Per-user theme settings for Aspid editor UI. Stores the GUID of an optional user override StyleSheet in
    // EditorPrefs; the override is layered on top of DefaultStyleSheet and may redefine any --aspid-colors-* /
    // --aspid-icons-* token inside a :root block.
    internal static class AspidThemeSettings
    {
        // The pre-scoping key. Read once as a fallback and migrated forward, so an override saved before the change
        // keeps working in the project it was set for; elsewhere its GUID never resolved to an asset anyway.
        private const string LegacyOverrideStyleSheetGuidKey = "Aspid.FastTools.Theme.OverrideStyleSheetGuid";

        public static event Action Changed;

        // Project-scoped: the stored GUID only resolves inside the project it was picked in, and the per-user reset
        // must not wipe another project's override — one machine-global slot did both.
        private static string OverrideStyleSheetGuidKey =>
            "Aspid.FastTools.Theme.OverrideStyleSheetGuid." + PlayerSettings.productGUID;

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

        // Backing storage for OverrideStyleSheet, persisted in EditorPrefs as the asset GUID
        // (empty when none). Setting it raises Changed; callers go through the typed property.
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
