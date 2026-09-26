using System;
using UnityEditor;
using System.Reflection;
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
                   CustomDrawerRegistry.HasDrawerFor(GetDrawnType(child, field), isManagedReference: true) ||
                   CustomDrawerRegistry.DeclaresDrawnAttribute(field);
        }

        // Unity picks a drawer by the instance type of a managed reference and by the element type of a list.
        private static Type GetDrawnType(SerializedProperty child, FieldInfo field)
        {
            if (child.propertyType is SerializedPropertyType.ManagedReference &&
                SerializeReferenceHelpers.GetTypeFromTypename(child.managedReferenceFullTypename) is { } instanceType)
                return instanceType;

            return child.isArray || child.IsArrayElement()
                ? field.FieldType.GetCollectionElementTypeOrSelf()
                : field.FieldType;
        }

        internal static bool HasVisibleChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            return iterator.NextVisible(enterChildren: true) && !SerializedProperty.EqualContents(iterator, end);
        }
    }
}
