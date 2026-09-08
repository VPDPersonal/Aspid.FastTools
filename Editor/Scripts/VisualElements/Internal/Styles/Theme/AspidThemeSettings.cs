using System;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeSettings
    {
        private const string LegacyOverrideStyleSheetGuidKey = "Aspid.FastTools.Theme.OverrideStyleSheetGuid";

        public static event Action Changed;

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
