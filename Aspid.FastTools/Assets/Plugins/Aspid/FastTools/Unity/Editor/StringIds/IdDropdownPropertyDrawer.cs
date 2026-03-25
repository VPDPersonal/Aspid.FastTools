#nullable enable
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomPropertyDrawer(typeof(IdDropdownAttribute))]
    internal sealed class IdDropdownPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return IdDropdownDrawer.GetIMGUIHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IdDropdownDrawer.DrawIMGUI(position, property, label, fieldInfo.DeclaringType);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return IdDropdownDrawer.DrawUIToolkit(property, preferredLabel, fieldInfo.DeclaringType);
        }
    }
}
