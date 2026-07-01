using System;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceUIToolkitPropertyDrawer
    {
        public static VisualElement Draw(string label, SerializedProperty property, params Type[] baseTypes)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;
            return new SerializeReferenceField(label, property, baseTypes);
        }
    }
}
