using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    internal static class EnumValueIMGUIPropertyDrawer
    {
        private const float FieldSpacing = 4f;
        internal const float FoldoutArrowWidth = 13f;

        private static readonly GUIContent _valueLabel = new("Value");

        public static float GetHeight(SerializedProperty property)
        {
            var valueProperty = property.FindPropertyRelative("_value");

            if (!valueProperty.HasFoldout())
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight
                + EditorGUIUtility.standardVerticalSpacing
                + EditorGUI.GetPropertyHeight(valueProperty, includeChildren: true);
        }

        public static void Draw(Rect position, SerializedProperty property)
        {
            var keyProperty = property.FindPropertyRelative("_key");
            var valueProperty = property.FindPropertyRelative("_value");
            var enumTypeProperty = property.FindPropertyRelative("_enumType");

            var hasFoldout = valueProperty.HasFoldout();

            Rect keyRect;
            Rect valueRect;
            GUIContent label;

            if (hasFoldout)
            {
                label = _valueLabel;

                keyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                var valueY = keyRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                valueRect = new Rect(
                    position.x + FoldoutArrowWidth,
                    valueY,
                    position.width - FoldoutArrowWidth,
                    position.yMax - valueY);
            }
            else
            {
                label = GUIContent.none;

                var halfWidth = (position.width - FieldSpacing) / 2f;
                keyRect = new Rect(position.x, position.y, halfWidth, position.height);

                valueRect = new Rect(
                    keyRect.xMax + FieldSpacing,
                    position.y,
                    halfWidth,
                    position.height);
            }

            DrawKey(keyRect, keyProperty, enumTypeProperty);

            // GetHeight reserves room for the expanded children, so they must be drawn too.
            EditorGUI.PropertyField(valueRect, valueProperty, label, includeChildren: hasFoldout);
        }

        private static void DrawKey(Rect rect, SerializedProperty keyProperty, SerializedProperty enumTypeProperty)
        {
            if (EnumValuesPropertyDrawerHelper.ResolveKey(keyProperty, enumTypeProperty) is not { } enumValue)
            {
                EditorGUI.PropertyField(rect, keyProperty, GUIContent.none);
                return;
            }

            var selected = EnumInfo.IsFlags(enumValue.GetType())
                ? EditorGUI.EnumFlagsField(rect, enumValue)
                : EditorGUI.EnumPopup(rect, enumValue);

            if (!Equals(selected, enumValue))
                keyProperty.SetStringAndApply(selected.ToString());
        }
    }
}
