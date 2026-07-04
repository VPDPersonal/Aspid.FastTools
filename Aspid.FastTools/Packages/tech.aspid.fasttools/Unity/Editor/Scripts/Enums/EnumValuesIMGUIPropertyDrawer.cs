using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    /// <summary>
    /// IMGUI rendering for <see cref="EnumValuesPropertyDrawer"/>.
    /// </summary>
    internal static class EnumValuesIMGUIPropertyDrawer
    {
        // Card metrics and colors mirror UI/Enums/Aspid-FastTools-EnumValues.uss — header and
        // container read as one rounded card split by a horizontal seam.
        private const float BorderWidth = 1f;
        private const float CornerRadius = 5f;
        private const float CardPadding = 5f;
        private const float SeamPadding = 2f;

        // --aspid-colors-bg-dark / bg-light / bg-lightness from Aspid-FastTools-Default-Dark.uss.
        private static readonly Color BorderColor = new Color32(36, 36, 36, 255);
        private static readonly Color HeaderColor = new Color32(46, 46, 46, 255);
        private static readonly Color ContainerColor = new Color32(56, 56, 56, 255);

        public static float GetHeight(SerializedProperty property)
        {
            var valuesProperty = property.FindPropertyRelative("_values");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            var headerHeight = BorderWidth + CardPadding + EditorGUIUtility.singleLineHeight + SeamPadding;
            var contentHeight = EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true)
                + spacing + EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);

            return headerHeight + SeamPadding + contentHeight + CardPadding + BorderWidth;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, bool isTyped)
        {
            var serializedObject = property.serializedObject;
            var valuesProperty = property.FindPropertyRelative("_values");
            var enumTypeProperty = property.FindPropertyRelative("_enumType");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");

            // Push the parent enum type into every existing entry up-front, same as the UIToolkit
            // variant's UpdateValues() — IMGUI has no change-event hook, so this simply re-runs
            // (as a cheap no-op once values match) on every repaint instead.
            UpdateValues(valuesProperty, enumTypeProperty);

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var inset = BorderWidth + CardPadding;

            var headerBackgroundHeight = BorderWidth + CardPadding + EditorGUIUtility.singleLineHeight + SeamPadding;
            var headerBackgroundRect = new Rect(position.x, position.y, position.width, headerBackgroundHeight);
            var containerBackgroundRect = new Rect(
                position.x, headerBackgroundRect.yMax,
                position.width, position.height - headerBackgroundHeight);

            DrawCardBackground(headerBackgroundRect, containerBackgroundRect);

            var headerRect = new Rect(
                position.x + inset, position.y + BorderWidth + CardPadding,
                position.width - inset * 2f, EditorGUIUtility.singleLineHeight);

            DrawHeader(headerRect, label, enumTypeProperty, isTyped);

            EnumValuesPropertyDrawerHelper.ShowPopulateContextMenu(
                headerBackgroundRect, serializedObject,
                valuesProperty.propertyPath, enumTypeProperty.propertyPath, defaultValueProperty.propertyPath);

            // Foldout arrows (the Values array header, a multi-line Default Value) render to the
            // LEFT of the supplied rect in the Inspector — inset the content column so they stay
            // inside the card border.
            var contentInset = inset + EnumValueIMGUIPropertyDrawer.FoldoutArrowWidth;

            var valuesHeight = EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true);
            var valuesRect = new Rect(
                position.x + contentInset, containerBackgroundRect.y + SeamPadding,
                position.width - contentInset - inset, valuesHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(valuesRect, valuesProperty, includeChildren: true);
            if (EditorGUI.EndChangeCheck())
                UpdateValues(valuesProperty, enumTypeProperty);

            var defaultValueHeight = EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);
            var defaultValueRect = new Rect(valuesRect.x, valuesRect.yMax + spacing, valuesRect.width, defaultValueHeight);
            EditorGUI.PropertyField(defaultValueRect, defaultValueProperty, includeChildren: true);
        }

        private static void DrawCardBackground(Rect headerRect, Rect containerRect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f,
                HeaderColor, Vector4.zero, new Vector4(CornerRadius, CornerRadius, 0f, 0f));

            GUI.DrawTexture(containerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f,
                ContainerColor, Vector4.zero, new Vector4(0f, 0f, CornerRadius, CornerRadius));

            var cardRect = new Rect(headerRect.x, headerRect.y, headerRect.width, headerRect.height + containerRect.height);
            GUI.DrawTexture(cardRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f,
                BorderColor, Vector4.one * BorderWidth, Vector4.one * CornerRadius);
        }

        private static void DrawHeader(Rect rect, GUIContent label, SerializedProperty enumTypeProperty, bool isTyped)
        {
            var labelRect = rect;
            labelRect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, label);

            var fieldRect = rect;
            fieldRect.x += EditorGUIUtility.labelWidth;
            fieldRect.width -= EditorGUIUtility.labelWidth;

            if (isTyped)
                DrawReadOnlyEnumType(fieldRect, enumTypeProperty);
            else
                EditorGUI.PropertyField(fieldRect, enumTypeProperty, GUIContent.none);
        }

        // Read-only counterpart of InspectorTypeField(IsReadOnly = true) used by the UIToolkit
        // variant — the typed EnumValues<TEnum,TValue> variant fixes the enum at compile time, so
        // there is nothing to pick, only a caption and an open-script shortcut.
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
                EditorGUI.LabelField(captionRect, caption, EditorStyles.popup);

            if (type is not null)
            {
                var openButtonRect = new Rect(captionRect.xMax + 1f, rect.y, rect.height, rect.height);
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, type);
            }
        }

        private static void UpdateValues(SerializedProperty valuesProperty, SerializedProperty enumTypeProperty)
        {
            var enumTypeValue = enumTypeProperty.stringValue;

            for (var i = 0; i < valuesProperty.arraySize; i++)
            {
                var enumTypeElement = valuesProperty
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("_enumType");

                if (enumTypeElement.stringValue != enumTypeValue)
                    enumTypeElement.SetStringAndApply(enumTypeValue);
            }
        }
    }
}
