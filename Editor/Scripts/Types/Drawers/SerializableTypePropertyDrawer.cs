using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // SerializableMonoScript is not a SerializableType, so useForChildren only reaches SerializableType<T>.
    [CustomPropertyDrawer(typeof(SerializableType), useForChildren: true)]
    internal sealed class SerializableTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => TypeIMGUIPropertyDrawer.Draw(
            position: position,
            label: label,
            property: SerializableTypeUtility.GetBackingProperty(property),
            allow: TypeAllow.All,
            types: GetBaseType());

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            TypeIMGUIPropertyDrawer.GetHeight(SerializableTypeUtility.GetBackingProperty(property));

        public override VisualElement CreatePropertyGUI(SerializedProperty property) => TypeUIToolkitPropertyDrawer.Draw(
            label: preferredLabel,
            property: SerializableTypeUtility.GetBackingProperty(property),
            allow: TypeAllow.All,
            types: GetBaseType());

        private Type GetBaseType() =>
            SerializableTypeUtility.TryGetBaseType(fieldInfo.FieldType, out var baseType) ? baseType : typeof(object);
    }
}
