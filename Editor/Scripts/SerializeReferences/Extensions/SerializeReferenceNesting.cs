using UnityEditor;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceNesting
    {
        // Stop recursive drawing at the cap so cyclic managed references cannot overflow the editor stack.
        internal const int MaxDepth = 8;

        internal static bool DrawsOwnHeader(SerializedProperty child, int depth)
        {
            if (depth >= MaxDepth) return false;
            if (child.propertyType is not SerializedPropertyType.ManagedReference &&
                !SerializeReferenceHelpers.IsManagedReferenceArray(child)) return false;

            return !DrawnByUnity(child);
        }

        // Respect custom drawers; decorators alone do not replace the managed-reference picker.
        internal static bool DrawnByUnity(SerializedProperty child)
        {
            var field = child.GetFieldInfo();
            if (field is null) return false;

            return field.IsDefined(typeof(TypeSelectorAttribute), inherit: true) ||
                   CustomDrawerRegistry.HasDrawerFor(field.FieldType) ||
                   CustomDrawerRegistry.DeclaresDrawnAttribute(field);
        }

        internal static bool HasVisibleChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            return iterator.NextVisible(enterChildren: true) && !SerializedProperty.EqualContents(iterator, end);
        }
    }
}
