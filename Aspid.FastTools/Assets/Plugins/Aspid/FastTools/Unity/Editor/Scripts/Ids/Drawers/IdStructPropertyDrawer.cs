#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    [CustomPropertyDrawer(typeof(IId), useForChildren: true)]
    internal sealed class IdStructPropertyDrawer : PropertyDrawer
    {
        private bool? _isUnique;
        private bool IsUnique => _isUnique ??= fieldInfo.GetCustomAttributes(typeof(UniqueIdAttribute), false).Length > 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = IdStructDrawer.GetIMGUIHeight(property);

            if (IsUnique && !string.IsNullOrEmpty(property.FindPropertyRelative(Constants.StringIdFieldName)?.stringValue) && !IsValueUnique(property))
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var drawH    = IdStructDrawer.GetIMGUIHeight(property);
            var drawRect = new Rect(position.x, position.y, position.width, drawH);
            IdStructDrawer.DrawIMGUI(drawRect, property, label, fieldInfo.FieldType);

            if (!IsUnique) return;

            var stringId = property.FindPropertyRelative(Constants.StringIdFieldName)?.stringValue;
            if (string.IsNullOrEmpty(stringId) || IsValueUnique(property)) return;

            var warnY    = position.y + drawH + EditorGUIUtility.standardVerticalSpacing;
            var warnRect = new Rect(position.x, warnY, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warnRect, "ID is not unique among assets of this type", MessageType.Warning);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var mainElement = IdStructDrawer.DrawUIToolkit(property, preferredLabel, fieldInfo.FieldType);

            if (!IsUnique)
                return mainElement;

            var warningLabel = new Label("⚠ ID is not unique among assets of this type")
                .SetDisplay(DisplayStyle.None)
                .SetMargin(top: 2)
                .SetColor(new Color(1f, 0.75f, 0f))
                .SetFontSize(11);

            var declaringType = fieldInfo.DeclaringType;
            var propPath      = property.propertyPath;
            var so            = property.serializedObject;

            void Refresh()
            {
                var p        = so.FindProperty(propPath);
                var stringId = p?.FindPropertyRelative(Constants.StringIdFieldName)?.stringValue;
                var unique   = string.IsNullOrEmpty(stringId)
                            || UniqueIdIndex.IsUnique(declaringType, stringId, GetCurrentAssetGuid(so));
                warningLabel.SetDisplay(unique ? DisplayStyle.None : DisplayStyle.Flex);
            }

            var idProp = property.FindPropertyRelative(Constants.StringIdFieldName);
            if (idProp != null)
                warningLabel.TrackPropertyValue(idProp, _ => Refresh());
            warningLabel.schedule.Execute(Refresh).StartingIn(0);

            return new VisualElement()
                .SetFlexDirection(FlexDirection.Column)
                .AddChild(mainElement)
                .AddChild(warningLabel);
        }

        private bool IsValueUnique(SerializedProperty structProp)
        {
            var idProp = structProp.FindPropertyRelative(Constants.StringIdFieldName);
            if (idProp == null) return true;

            var stringId = idProp.stringValue;
            if (string.IsNullOrEmpty(stringId)) return true;

            return UniqueIdIndex.IsUnique(fieldInfo.DeclaringType, stringId, GetCurrentAssetGuid(structProp.serializedObject));
        }

        private static string GetCurrentAssetGuid(SerializedObject serializedObject)
        {
            var path = AssetDatabase.GetAssetPath(serializedObject.targetObject);
            return string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }
    }
}
