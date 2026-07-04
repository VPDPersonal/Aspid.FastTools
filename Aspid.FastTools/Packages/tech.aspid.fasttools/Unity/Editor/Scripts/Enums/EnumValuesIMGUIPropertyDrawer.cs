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
        public static float GetHeight(SerializedProperty property)
        {
            var valuesProperty = property.FindPropertyRelative("_values");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            return EditorGUIUtility.singleLineHeight
                + spacing + EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true)
                + spacing + EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);
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
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            DrawHeader(headerRect, label, enumTypeProperty, isTyped);

            EnumValuesPropertyDrawerHelper.ShowPopulateContextMenu(
                headerRect, serializedObject,
                valuesProperty.propertyPath, enumTypeProperty.propertyPath, defaultValueProperty.propertyPath);

            var valuesHeight = EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true);
            var valuesRect = new Rect(position.x, headerRect.yMax + spacing, position.width, valuesHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(valuesRect, valuesProperty, includeChildren: true);
            if (EditorGUI.EndChangeCheck())
                UpdateValues(valuesProperty, enumTypeProperty);

            var defaultValueHeight = EditorGUI.GetPropertyHeight(defaultValueProperty, includeChildren: true);
            var defaultValueRect = new Rect(position.x, valuesRect.yMax + spacing, position.width, defaultValueHeight);
            EditorGUI.PropertyField(defaultValueRect, defaultValueProperty);
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
