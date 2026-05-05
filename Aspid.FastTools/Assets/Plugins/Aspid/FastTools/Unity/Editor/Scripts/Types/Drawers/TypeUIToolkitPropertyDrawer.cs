using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeUIToolkitPropertyDrawer
    {
        public static VisualElement Draw(
            string label,
            SerializedProperty property,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
        { 
            label = string.IsNullOrWhiteSpace(label) ? null : label;
            
            var field = new TypeField(label, property)
            {
                Allow = allow,
                Types = types
            }
            .AddClass(PropertyField.ussClassName) 
            .AddClass(TypeField.alignedFieldUssClassName);
            
            field.labelElement.AddClass(PropertyField.labelUssClassName);
            return field;
        }
    }
}
