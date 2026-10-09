using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceNesting
    {
        // Stop recursive drawing at the cap so cyclic managed references cannot overflow the editor stack.
        internal const int MaxDepth = 8;

        // The answer depends on the field, its attributes and the drawer registry, which change only with a domain
        // reload that also clears this cache. IMGUI asks for every nested reference on every event.
        private static readonly Dictionary<(FieldInfo, bool, bool), bool> DrawnByUnityCache = new();

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

            var isArrayElement = child.IsArrayElement();
            var isArray = child.isArray;

            var key = (field, isArrayElement, isArray);
            if (DrawnByUnityCache.TryGetValue(key, out var drawn)) return drawn;

            return DrawnByUnityCache[key] = DrawnByUnity(field, isArrayElement: isArrayElement, isArray: isArray);
        }

        private static bool DrawnByUnity(FieldInfo field, bool isArrayElement, bool isArray)
        {
            // [TypeSelector] applies to a collection, not to its elements, so its list draws them with the picker
            // itself, ahead of any type drawer, as the drawer did when it reached each element.
            if (field.IsDefined(typeof(TypeSelectorAttribute), inherit: true)) return !isArrayElement;
            if (CustomDrawerRegistry.DeclaresDrawnAttribute(field, isManagedReference: true, isArrayElement: isArrayElement))
                return true;

            // Unity applies a type drawer to each element, never to the list, so the list keeps the picker-backed add.
            if (isArray) return false;

            // The declared type, not the stored one: Unity ships no picker, so a drawer of the stored type would take
            // the dropdown away as soon as that type is picked.
            var declaredType = isArrayElement
                ? field.FieldType.GetCollectionElementTypeOrSelf()
                : field.FieldType;
            return CustomDrawerRegistry.HasDrawerFor(declaredType, isManagedReference: true);
        }

        internal static bool HasVisibleChildren(SerializedProperty property)
        {
            foreach (var _ in property.VisibleChildren()) return true;
            return false;
        }
    }
}
