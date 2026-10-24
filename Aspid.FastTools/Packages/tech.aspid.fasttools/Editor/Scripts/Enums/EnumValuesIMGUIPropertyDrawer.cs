using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    internal static class EnumValuesIMGUIPropertyDrawer
    {
        private const float BorderWidth = 1f;
        private const float CornerRadius = 5f;
        private const float CardPadding = 5f;
        private const float SeamPadding = 2f;
        private const int StyleStateCount = 8;

        private static bool _isPlainFoldoutHeader;
        private static bool _headerLabelStylePro;
        private static GUIStyle _headerLabelStyle;
        private static FontStyle _savedFontStyle;
        private static Texture2D _transparentTexture;
        private static readonly GUIStyleState[] _states = new GUIStyleState[StyleStateCount];
        private static readonly Texture2D[] _savedBackgrounds = new Texture2D[StyleStateCount];
        private static readonly Texture2D[][] _savedScaledBackgrounds = new Texture2D[StyleStateCount][];

        // Keep these colors aligned with the UI Toolkit drawer: --unity-colors-inspector_titlebar-border,
        // --unity-colors-inspector_titlebar-background and --unity-colors-helpbox-background.
        internal static Color BorderColor => EditorGUIUtility.isProSkin
            ? new Color32(26, 26, 26, 255)
            : new Color32(127, 127, 127, 255);

        internal static Color HeaderColor => EditorGUIUtility.isProSkin
            ? new Color32(62, 62, 62, 255)
            : new Color32(203, 203, 203, 255);

        internal static Color ContainerColor => EditorGUIUtility.isProSkin
            ? new Color32(96, 96, 96, 52)
            : new Color32(235, 235, 235, 52);

        public static float GetHeight(SerializedProperty property)
        {
            var valuesProperty = property.FindPropertyRelative("_values");
            var enumTypeProperty = property.FindPropertyRelative("_enumType");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");

            var headerHeight =
                BorderWidth
                + CardPadding
                + GetHeaderContentHeight(enumTypeProperty)
                + SeamPadding
                + BorderWidth;

            var contentHeight =
                EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true)
                + EditorGUIUtility.standardVerticalSpacing
                + EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);

            return
                headerHeight
                + SeamPadding
                + contentHeight
                + CardPadding
                + BorderWidth;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, bool isTyped)
        {
            var serializedObject = property.serializedObject;
            var valuesProperty = property.FindPropertyRelative("_values");
            var enumTypeProperty = property.FindPropertyRelative("_enumType");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");

            var inset = BorderWidth + CardPadding;

            var headerContentHeight = GetHeaderContentHeight(enumTypeProperty);
            var headerBackgroundHeight = BorderWidth + CardPadding + headerContentHeight + SeamPadding + BorderWidth;
            var headerBackgroundRect = new Rect(position.x, position.y, position.width, headerBackgroundHeight);

            var containerBackgroundRect = new Rect(position.x, headerBackgroundRect.yMax, position.width, position.height - headerBackgroundHeight);

            var headerRect = new Rect(
                position.x + inset,
                position.y + BorderWidth + CardPadding,
                position.width - inset * 2f,
                headerContentHeight);

            DrawCardBackground(headerBackgroundRect, containerBackgroundRect);
            DrawHeader(headerRect, label, enumTypeProperty, isTyped);

            EnumValuesPropertyDrawerHelper.ShowPopulateContextMenu(
                headerBackgroundRect, serializedObject,
                valuesProperty.propertyPath, enumTypeProperty.propertyPath, defaultValueProperty.propertyPath);

            // Foldout arrows render left of the supplied rectangle; reserve space inside the border.
            var contentInset = inset + EnumValueIMGUIPropertyDrawer.FoldoutArrowWidth;

            var valuesHeight = EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true);
            var valuesRect = new Rect(
                position.x + contentInset, containerBackgroundRect.y + SeamPadding,
                position.width - contentInset - inset, valuesHeight);

            DrawValues(valuesRect, valuesProperty);

            // A plain default value starts at the card inset as in UI Toolkit; a foldout one keeps room for its arrow.
            var defaultValueInset = defaultValueProperty.HasFoldout() ? contentInset : inset;
            var defaultValueHeight = EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);
            var defaultValueRect = new Rect(
                position.x + defaultValueInset, valuesRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width - defaultValueInset - inset, defaultValueHeight);
            EditorGUI.PropertyField(defaultValueRect, defaultValueProperty, includeChildren: true);
        }

        // Unity draws the list header with EditorStyles.foldoutHeader: bold text on a band. UI Toolkit shows a plain
        // foldout, so the style is swapped while the list draws and restored right after. The elements suspend the
        // swap, so a list inside an element value keeps the stock header.
        // The background must stay non-null: a style without one is painted from the editor USS, band included.
        private static void DrawValues(Rect rect, SerializedProperty valuesProperty)
        {
            var wasPlain = _isPlainFoldoutHeader;
            if (!wasPlain) ApplyPlainFoldoutHeader();

            try
            {
                EditorGUI.PropertyField(rect, valuesProperty, includeChildren: true);
                DrawValuesFoldoutArrow(rect, valuesProperty);
            }
            finally
            {
                if (!wasPlain) RestoreFoldoutHeader();
            }
        }

        /// <returns>Whether the plain header was on; pass it to <see cref="ResumePlainFoldoutHeader"/>.</returns>
        internal static bool SuspendPlainFoldoutHeader()
        {
            var wasPlain = _isPlainFoldoutHeader;
            if (wasPlain) RestoreFoldoutHeader();
            return wasPlain;
        }

        internal static void ResumePlainFoldoutHeader(bool wasPlain)
        {
            if (wasPlain) ApplyPlainFoldoutHeader();
        }

        // The editor USS paints the foldout header arrow together with its band, so the arrow goes with it.
        private static void DrawValuesFoldoutArrow(Rect rect, SerializedProperty valuesProperty)
        {
            if (Event.current.type != EventType.Repaint) return;

            // One point further left than the arrow width lines it up with the stock header arrow.
            var arrowRect = new Rect(
                rect.x - EnumValueIMGUIPropertyDrawer.FoldoutArrowWidth - 1f, rect.y,
                EnumValueIMGUIPropertyDrawer.FoldoutArrowWidth, EditorGUIUtility.singleLineHeight);

            var headerRect = new Rect(arrowRect.x, rect.y, rect.xMax - arrowRect.x, EditorGUIUtility.singleLineHeight);
            var isHover = headerRect.Contains(Event.current.mousePosition);

            EditorStyles.foldout.Draw(arrowRect, GUIContent.none, isHover, isActive: false, on: valuesProperty.isExpanded, hasKeyboardFocus: false);
        }

        private static void ApplyPlainFoldoutHeader()
        {
            var style = EditorStyles.foldoutHeader;
            _isPlainFoldoutHeader = true;

            _savedFontStyle = style.fontStyle;
            style.fontStyle = FontStyle.Normal;

            var states = GetStates(style);
            for (var i = 0; i < states.Length; i++)
            {
                _savedBackgrounds[i] = states[i].background;
                _savedScaledBackgrounds[i] = states[i].scaledBackgrounds;

                states[i].background = TransparentTexture;
                states[i].scaledBackgrounds = Array.Empty<Texture2D>();
            }
        }

        private static void RestoreFoldoutHeader()
        {
            var style = EditorStyles.foldoutHeader;
            _isPlainFoldoutHeader = false;

            style.fontStyle = _savedFontStyle;

            var states = GetStates(style);
            for (var i = 0; i < states.Length; i++)
            {
                states[i].background = _savedBackgrounds[i];
                states[i].scaledBackgrounds = _savedScaledBackgrounds[i];

                _savedBackgrounds[i] = null;
                _savedScaledBackgrounds[i] = null;
            }
        }

        private static Texture2D TransparentTexture
        {
            get
            {
                if (_transparentTexture) return _transparentTexture;

                _transparentTexture = new Texture2D(width: 1, height: 1, TextureFormat.RGBA32, mipChain: false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };

                _transparentTexture.SetPixel(x: 0, y: 0, Color.clear);
                _transparentTexture.Apply();
                return _transparentTexture;
            }
        }

        private static GUIStyleState[] GetStates(GUIStyle style)
        {
            _states[0] = style.normal;
            _states[1] = style.hover;
            _states[2] = style.active;
            _states[3] = style.focused;
            _states[4] = style.onNormal;
            _states[5] = style.onHover;
            _states[6] = style.onActive;
            _states[7] = style.onFocused;
            return _states;
        }

        // The label takes its own line above the type field, as in UI Toolkit.
        private static float GetHeaderContentHeight(SerializedProperty enumTypeProperty) =>
            EditorGUIUtility.singleLineHeight
            + EditorGUIUtility.standardVerticalSpacing
            + GetTypeFieldHeight(enumTypeProperty);

        // The type field adds a notice row when the type is missing or required; a fixed line would let it
        // overlap the list below.
        private static float GetTypeFieldHeight(SerializedProperty enumTypeProperty) => Mathf.Max(
            EditorGUIUtility.singleLineHeight,
            EditorGUI.GetPropertyHeight(enumTypeProperty, GUIContent.none, includeChildren: false));

        private static void DrawCardBackground(Rect headerRect, Rect containerRect)
        {
            if (Event.current.type != EventType.Repaint) return;
            var cardRect = new Rect(headerRect.x, headerRect.y, headerRect.width, headerRect.height + containerRect.height);

            GUI.DrawTexture(
                headerRect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                false,
                0f,
                HeaderColor,
                Vector4.zero,
                new Vector4(CornerRadius, CornerRadius, 0f, 0f));

            // The seam line under the header, like the header's bottom border in UI Toolkit.
            var seamRect = new Rect(headerRect.x, headerRect.yMax - BorderWidth, headerRect.width, BorderWidth);
            EditorGUI.DrawRect(seamRect, BorderColor);

            GUI.DrawTexture(
                containerRect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                true,
                0f,
                ContainerColor,
                Vector4.zero,
                new Vector4(0f, 0f, CornerRadius, CornerRadius));

            GUI.DrawTexture(
                cardRect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                true,
                0f,
                BorderColor,
                Vector4.one * BorderWidth,
                Vector4.one * CornerRadius);
        }

        private static void DrawHeader(Rect rect, GUIContent label, SerializedProperty enumTypeProperty, bool isTyped)
        {
            var labelRect = rect;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, label, GetHeaderLabelStyle());

            var fieldRect = rect;
            fieldRect.yMin = labelRect.yMax + EditorGUIUtility.standardVerticalSpacing;

            if (isTyped) DrawReadOnlyEnumType(fieldRect, enumTypeProperty);
            else EditorGUI.PropertyField(fieldRect, enumTypeProperty, GUIContent.none);
        }

        // A long label ends in an ellipsis inside the card instead of running past its border, as in UI Toolkit.
        private static GUIStyle GetHeaderLabelStyle()
        {
            if (_headerLabelStyle is null || _headerLabelStylePro != EditorGUIUtility.isProSkin)
            {
                _headerLabelStyle = new GUIStyle(EditorStyles.label) { clipping = TextClipping.Ellipsis };
                _headerLabelStylePro = EditorGUIUtility.isProSkin;
            }

            return _headerLabelStyle;
        }

        private static void DrawReadOnlyEnumType(Rect rect, SerializedProperty enumTypeProperty)
        {
            var type = string.IsNullOrEmpty(enumTypeProperty.stringValue)
                ? null
                : Type.GetType(enumTypeProperty.stringValue, throwOnError: false);

            var caption = TypeSelectorHelpers.GetTypeSelectorTitle(type, enumTypeProperty.stringValue);
            var captionRect = rect;

            if (type is not null)
                captionRect.width -= rect.height + 1f;

            using (new EditorGUI.DisabledScope(true))
                TypeIMGUIPropertyDrawer.DrawCaptionLabel(captionRect,
                    new GUIContent(caption, TypeIMGUIPropertyDrawer.GetCaptionIcon(type)), EditorStyles.popup);

            if (type is not null)
            {
                var openButtonRect = new Rect(captionRect.xMax + 1f, rect.y, rect.height, rect.height);
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, type);
            }
        }
    }
}
