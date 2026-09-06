using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    [CustomPropertyDrawer(typeof(EnumValues<>))]
    [CustomPropertyDrawer(typeof(EnumValues<,>))]
    internal sealed class EnumValuesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
            EnumValuesIMGUIPropertyDrawer.Draw(position, label, property, IsTypedVariant());

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EnumValuesIMGUIPropertyDrawer.GetHeight(property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            EnumValuesUIToolkitPropertyDrawer.Draw(property, IsTypedVariant());

        private bool IsTypedVariant()
        {
            // fieldInfo is the array/list field when the drawer renders an element.
            var type = fieldInfo.FieldType;

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(EnumValues<,>);
        }
    }
}
