using System;
using UnityEditor;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The one write path of the inspector actions that assign a managed reference: the type picker, a dropped script,
    // Paste, Paste Template, Link to Existing and Create New Script. A write notes a replaced missing list element,
    // refuses a type the targets cannot hold, and expands the field to show the new value.
    internal static class SerializeReferenceWriter
    {
        // A runtime object keeps its references in a player build, which leaves editor-only assemblies out, so a type
        // from one would load there as missing. Null clears the field and always fits.
        public static bool CanHold(SerializedProperty property, Type type) =>
            type is null ||
            !IsEditorOnly(type) ||
            !Array.Exists(property.serializedObject.targetObjects, TypeSelectorHelpers.IsRuntimeObject);

        // A closed generic reports the assembly of its definition, so an editor-only argument needs its own check.
        private static bool IsEditorOnly(Type type) =>
            TypeUtility.IsEditorOnlyAssembly(type.Assembly) ||
            (type.HasElementType && IsEditorOnly(type.GetElementType())) ||
            (type.IsGenericType && Array.Exists(type.GetGenericArguments(), IsEditorOnly));

        // Narrows a menu filter to the types a write accepts, so a menu never offers a type the write refuses.
        public static Func<Type, bool> WithHoldCheck(SerializedProperty property, Func<Type, bool> filter) =>
            type => (filter is null || filter(type)) && CanHold(property, type);

        // Carries over the data the old and new types share. A target that already holds the type keeps its instance,
        // so its rid, its nested state and every alias onto it survive.
        public static bool SetType(SerializedProperty property, Type type, SerializedProperty view = null) =>
            Write(property, type, previous => previous is not null && previous.GetType() == type
                ? previous
                : SerializeReferenceHelpers.CreateInstancePreservingData(type, previous), view);

        // Calls create once per target, so the selected objects never share one value. A null value clears the field
        // only when type is null too: a template removed meanwhile leaves the target as it is.
        public static bool SetValue(SerializedProperty property, Type type, Func<object> create, SerializedProperty view = null) =>
            Write(property, type, _ => create(), view);

        // view is the inspector's own property when the write goes through another SerializedObject: isExpanded is
        // cached per SerializedObject, so a foldout already drawn sees only a write through its own one.
        private static bool Write(SerializedProperty property, Type type, Func<object, object> create, SerializedProperty view)
        {
            if (property is null || !CanHold(property, type)) return false;

            bool expanded;
            var written = property.serializedObject.isEditingMultipleObjects
                ? WritePerTarget(property, type, create, out expanded)
                : WriteSingle(property, type, create, out expanded);

            if (!written) return false;

            SetExpanded(property, view, expanded);
            SerializeReferenceHelpers.InvalidateReferenceMemos();
            return true;
        }

        private static bool WriteSingle(SerializedProperty property, Type type, Func<object, object> create, out bool expanded)
        {
            var previous = property.managedReferenceValue;
            var value = create(previous);

            expanded = value is not null;
            if (Skips(property, type, previous, value)) return false;

            SerializeReferenceMissingListGuard.NoteReplaced(property);
            property.SetManagedReferenceAndApply(value);
            return true;
        }

        // Each target gets its own value, built from its own previous one; one Undo step covers them all.
        private static bool WritePerTarget(SerializedProperty property, Type type, Func<object, object> create, out bool expanded)
        {
            var serializedObject = property.serializedObject;
            var propertyPath = property.propertyPath;
            var written = false;
            expanded = false;

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();

            foreach (var target in serializedObject.targetObjects)
            {
                if (target == null) continue;

                using var single = new SerializedObject(target);
                var targetProperty = single.FindProperty(propertyPath);
                if (targetProperty is null) continue;

                var previous = targetProperty.managedReferenceValue;
                var value = create(previous);
                if (Skips(property, type, previous, value)) continue;

                SerializeReferenceMissingListGuard.NoteReplaced(targetProperty);
                targetProperty.managedReferenceValue = value;
                targetProperty.isExpanded = value is not null;
                single.ApplyModifiedProperties();

                written = true;
                expanded |= value is not null;
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Update() pulls the per-target writes in; applying instead would write the stale value of the
            // multi-object SerializedObject back over them.
            serializedObject.Update();
            return written;
        }

        // The previous value handed back means the target already holds it. A null value of a non-null type means
        // its source is gone.
        private static bool Skips(SerializedProperty property, Type type, object previous, object value) =>
            value is null
                ? type is not null
                : ReferenceEquals(value, previous) || !CanHold(property, value.GetType());

        private static void SetExpanded(SerializedProperty property, SerializedProperty view, bool expanded)
        {
            property.isExpanded = expanded;
            if (view is null) return;

            try
            {
                view.isExpanded = expanded;
            }
            catch (Exception)
            {
                // The inspector disposed its SerializedObject meanwhile; a new one reads the state written above.
            }
        }
    }
}
