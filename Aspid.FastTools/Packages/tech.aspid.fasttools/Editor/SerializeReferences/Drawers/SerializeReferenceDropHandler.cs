using System;
using UnityEditor;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Assigns a managed reference from a MonoScript dropped on a [SerializeReference] field. Shared by the UIToolkit
    // field and the IMGUI drawer.
    internal static class SerializeReferenceDropHandler
    {
        // Resolves the first dragged script's class when it is assignable to the field and passes the
        // [TypeSelector] narrowing.
        public static bool TryResolveDroppedType(Type fieldType, Type[] baseTypes, out Type type)
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

                type = candidate;
                return true;
            }

            return false;
        }

        // Assigns a fresh instance per target, so a multi-selection drop never aliases one reference across objects.
        public static void Assign(SerializedProperty property, Type type)
        {
            if (property is null || type is null) return;

            var persistent = property.Persistent();
            var previous = persistent.managedReferenceValue;

            if (SerializeReferenceHelpers.IsEditingMultipleObjects(persistent))
            {
                SerializeReferenceHelpers.ApplyManagedReferencePerTarget(persistent,
                    target => SerializeReferenceHelpers.CreateInstancePreservingData(type, target));
            }
            else persistent.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
        }
    }
}
