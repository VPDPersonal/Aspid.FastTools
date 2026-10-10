using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    [CustomPropertyDrawer(typeof(EnumValues<>))]
    [CustomPropertyDrawer(typeof(EnumValues<,>))]
    internal sealed class EnumValuesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
            EnumValuesIMGUIPropertyDrawer.Draw(position, label, property, IsTypedVariant(fieldInfo));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EnumValuesIMGUIPropertyDrawer.GetHeight(property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            EnumValuesUIToolkitPropertyDrawer.Draw(
                property,
                string.IsNullOrEmpty(preferredLabel) ? property.displayName : preferredLabel,
                IsTypedVariant(fieldInfo));

        // For collection elements, fieldInfo describes the array or list itself; it is null when Unity finds no field.
        internal static bool IsTypedVariant(FieldInfo fieldInfo) =>
            fieldInfo?.FieldType.GetCollectionElementTypeOrSelf() is { IsGenericType: true } type
            && type.GetGenericTypeDefinition() == typeof(EnumValues<,>);
    }
}
