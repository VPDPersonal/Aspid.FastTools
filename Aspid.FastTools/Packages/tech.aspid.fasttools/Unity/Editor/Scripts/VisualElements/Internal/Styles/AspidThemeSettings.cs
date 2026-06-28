using System;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    /// <summary>
    /// Per-user theme settings for Aspid editor UI. Stores the GUID of an optional user
    /// override <see cref="StyleSheet"/> in <see cref="EditorPrefs"/>; the override is layered on
    /// top of <see cref="AspidStyles.DefaultStyleSheet"/> and may redefine any
    /// <c>--aspid-colors-*</c> / <c>--aspid-icons-*</c> token inside a <c>:root</c> block.
    /// </summary>
    internal static class AspidThemeSettings
    {
        private const string OverrideStyleSheetGuidKey = "Aspid.FastTools.Theme.OverrideStyleSheetGuid";

        /// <summary>
        /// Raised whenever the override style sheet changes so that live elements can re-apply it.
        /// </summary>
        public static event Action Changed;

        /// <summary>
        /// GUID of the user override style sheet asset, or an empty string when none is set.
        /// Setting this value persists it to <see cref="EditorPrefs"/> and raises <see cref="Changed"/>.
        /// Backing storage for <see cref="OverrideStyleSheet"/>; callers go through that typed property.
        /// </summary>
        private static string OverrideStyleSheetGuid
        {
            get => EditorPrefs.GetString(OverrideStyleSheetGuidKey, string.Empty);
            set
            {
                value ??= string.Empty;
                if (OverrideStyleSheetGuid == value) return;

                if (string.IsNullOrEmpty(value)) EditorPrefs.DeleteKey(OverrideStyleSheetGuidKey);
                else EditorPrefs.SetString(OverrideStyleSheetGuidKey, value);

                Changed?.Invoke();
            }
        }

        /// <summary>
        /// The resolved user override style sheet, or <c>null</c> when none is set or the asset is missing.
        /// </summary>
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
    }
}
