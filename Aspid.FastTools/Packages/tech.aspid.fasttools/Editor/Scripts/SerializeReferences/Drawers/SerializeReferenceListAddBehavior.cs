using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceListAddBehavior
    {
        // Installs the picker-backed add behavior once on the hosting ListView. The base types come through a
        // provider consulted when the picker opens, since a member-referenced constraint can re-resolve later.
        public static void TryInstall(VisualElement elementField, SerializedProperty elementProperty, Type elementType, Func<Type[]> baseTypesProvider)
        {
            if (elementField is null || elementProperty is null) return;

            var serializedObject = elementProperty.serializedObject;
            if (serializedObject is null) return;

            // The innermost array: a list nested in another array's element must append to itself, not the outer one.
            if (!SerializeReferenceHelpers.TryGetArrayPath(elementProperty.propertyPath, out var arrayPath)) return;

            var targets = serializedObject.targetObjects;
            if (targets.Length == 0 || targets[0] == null) return;

            var listView = elementField.GetFirstAncestorOfType<ListView>();
            if (listView is null || listView.overridingAddButtonBehavior != null) return;

            // Assigning overridingAddButtonBehavior refreshes the items, which throws mid-attach — TryInstall runs
            // from AttachToPanelEvent. Defer a tick and re-check the guard, since siblings queue their own installs.
            listView.schedule.Execute(() =>
            {
                if (listView.overridingAddButtonBehavior != null) return;

                listView.overridingAddButtonBehavior = (_, button) =>
                    OpenAppendPicker(targets, arrayPath, elementType, baseTypesProvider(), button);
            });
        }

        public static void OpenAppendPicker(Object[] targets, string arrayPath, Type elementType, Type[] baseTypes, VisualElement anchor)
        {
            var window = anchor.GetOwnerWindow();
            if (window == null) return;

            var reference = anchor.GetFirstAncestorOfType<ListView>() ?? anchor;

            var width = Mathf.Max(350f, reference.worldBound.width);

            var x = Mathf.Max(
                window.position.x,
                Mathf.Min(window.position.x + reference.worldBound.xMin, window.position.xMax - width));

            var screenRect = new Rect(
                x,
                window.position.y + anchor.worldBound.yMin,
                width,
                anchor.worldBound.height);

            ShowAppendPicker(targets, arrayPath, elementType, baseTypes, screenRect);
        }

        public static void ShowAppendPicker(Object[] targets, string arrayPath, Type elementType, Type[] baseTypes, Rect screenRect)
        {
            TypeSelectorWindow.Show(
                screenRect: screenRect,
                filter: new TypeSelectorFilter
                {
                    Types = new[] { elementType },
                    Predicate = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes),
                    AdditionalTypes = GenericTypeResolver.GetAssignableGenericDefinitions(elementType, baseTypes, SerializeReferenceHelpers.IsAcceptableGenericArgument),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                    InferredArgumentFilter = SerializeReferenceHelpers.IsAcceptableGenericArgument,
                },
                currentAqn: null, // a "+" append has no current value — nothing (not even <None>) wears the check
                onSelected: aqn => Append(targets, arrayPath, aqn));
        }

        // Appends to every target, so a multi-object selection gets one independent instance per object instead of
        // the native add's copied rid. A target without a managed-reference array at the path is skipped.
        public static void Append(Object[] targets, string arrayPath, string assemblyQualifiedName)
        {
            if (targets is null) return;

            var type = string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName, throwOnError: false);

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();

            foreach (var target in targets)
            {
                if (target == null) continue;

                // A fresh SerializedObject avoids a stale-binding hazard; the bound list refreshes on its next update.
                using var serializedObject = new SerializedObject(target);
                var array = serializedObject.FindProperty(arrayPath);
                if (array is null || !SerializeReferenceHelpers.IsManagedReferenceArray(array)) continue;

                // arraySize++ copies the previous last element's rid, so overwrite it in the same modification —
                // an explicit null for <None> too.
                var index = array.arraySize;
                array.arraySize = index + 1;
                array.GetArrayElementAtIndex(index).SetManagedReference(type is null ? null : SerializeReferenceHelpers.CreateInstance(type));
                serializedObject.ApplyModifiedProperties();
            }

            // One Undo step for the whole selection.
            Undo.CollapseUndoOperations(undoGroup);
        }
    }
}
