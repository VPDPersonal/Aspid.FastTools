using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Draws a SerializableMonoScript / SerializableMonoScript<T> field. A field that also carries [TypeSelector] is
    // drawn by TypeSelectorPropertyDrawer instead, which intersects both constraints.
    [CustomPropertyDrawer(typeof(SerializableMonoScript), useForChildren: true)]
    internal sealed class SerializableMonoScriptPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => MonoScriptIMGUIPropertyDrawer.Draw(
            position: position,
            label: label,
            wrapperProperty: property,
            allow: TypeAllow.All,
            types: GetBaseType());

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            MonoScriptIMGUIPropertyDrawer.GetHeight(property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) => MonoScriptUIToolkitPropertyDrawer.Draw(
            label: preferredLabel,
            wrapperProperty: property,
            allow: TypeAllow.All,
            types: GetBaseType());

        private Type GetBaseType() =>
            SerializableTypeUtility.TryGetBaseType(fieldInfo.FieldType, out var baseType) ? baseType : typeof(object);
    }
}
