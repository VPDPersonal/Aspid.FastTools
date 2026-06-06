using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceUIToolkitPropertyDrawer
    {
        public static VisualElement Draw(string label, SerializedProperty property)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;
            return new SerializeReferenceField(label, property);
        }
    }
}
