using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    [CustomPropertyDrawer(typeof(EnumValue<>))]
    internal sealed class EnumValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
            EnumValueIMGUIPropertyDrawer.Draw(position, property);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EnumValueIMGUIPropertyDrawer.GetHeight(property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            EnumValueUIToolkitPropertyDrawer.Draw(property);
    }
}
