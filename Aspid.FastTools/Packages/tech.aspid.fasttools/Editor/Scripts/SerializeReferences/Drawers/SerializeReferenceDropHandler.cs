using System;
using UnityEditor;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceDropHandler
    {
        // Resolves the first dragged script's class when it is assignable to the field, passes the [TypeSelector]
        // narrowing and is a type the property's objects can hold.
        public static bool TryResolveDroppedType(SerializedProperty property, Type fieldType, Type[] baseTypes, out Type type)
        {
            type = null;

            foreach (var dragged in DragAndDrop.objectReferences)
            {
                if (dragged is not MonoScript script) continue;

                var candidate = script.GetClass();
                if (candidate is null) continue;
                if (!SerializeReferenceHelpers.IsAssignableManagedReference(candidate)) continue;
                if (fieldType != null && !fieldType.IsAssignableFrom(candidate)) continue;
                if (!SerializeReferenceHelpers.BuildAssignableFilter(baseTypes)(candidate)) continue;
                if (!SerializeReferenceWriter.CanHold(property, candidate)) continue;

                type = candidate;
                return true;
            }

            return false;
        }

        // The write goes through a separate SerializedObject; the inspector's property only gets the expansion.
        public static bool Assign(SerializedProperty property, Type type)
        {
            if (property is null || type is null) return false;

            return SerializeReferenceWriter.SetType(property.Persistent(), type, view: property);
        }
    }
}
