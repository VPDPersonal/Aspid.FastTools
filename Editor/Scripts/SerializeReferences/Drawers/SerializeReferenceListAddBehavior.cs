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
    // Replaces the default "+" on a [SerializeReference] list — which duplicates the last element and leaves it
    // rid-aliased — with one that opens the type picker and appends a fresh instance. SerializeReferenceDuplicateGuard
    // stays the fallback for the native add paths (Ctrl+D, paste, multi-object selections).
    internal static class SerializeReferenceListAddBehavior
    {
        // Installs the picker-backed add behavior once on the hosting ListView. The base types come through a
        // provider consulted when the picker opens, since a member-referenced constraint can re-resolve later.
        public static void TryInstall(VisualElement elementField, SerializedProperty elementProperty, Type elementType, Func<Type[]> baseTypesProvider)
        {
            if (elementField is null || elementProperty is null) return;

            var serializedObject = elementProperty.serializedObject;
            if (serializedObject is null || serializedObject.isEditingMultipleObjects) return;

            var path = elementProperty.propertyPath;
            var arrayMarker = path.IndexOf(".Array.data[", StringComparison.Ordinal);
            if (arrayMarker < 0) return; // not a list/array element

            var arrayPath = path[..arrayMarker];
            var target = serializedObject.targetObject;
            if (target == null) return;

            var listView = elementField.GetFirstAncestorOfType<ListView>();
            if (listView is null || listView.overridingAddButtonBehavior != null) return;

            // Assigning overridingAddButtonBehavior refreshes the items, which throws mid-attach — TryInstall runs
            // from AttachToPanelEvent. Defer a tick and re-check the guard, since siblings queue their own installs.
            listView.schedule.Execute(() =>
            {
                if (listView.overridingAddButtonBehavior != null) return;

                listView.overridingAddButtonBehavior = (_, button) =>
                    OpenAppendPicker(target, arrayPath, elementType, baseTypesProvider(), button);
            });
        }

        // Shared with SerializeReferenceListField, whose "+" needs the same picker anchored the same way.
        public static void OpenAppendPicker(Object target, string arrayPath, Type elementType, Type[] baseTypes, VisualElement anchor)
        {
            var window = anchor.GetOwnerWindow();
            if (window == null) return;

            // Anchor to the ListView, not the small "+", so the picker opens as a wide dropdown below the add row.
            var reference = anchor.GetFirstAncestorOfType<ListView>() ?? anchor;

            // Match TypeSelectorWindow.Show's minimum width so the clamp below reflects the picker's real footprint.
            var width = Mathf.Max(350f, reference.worldBound.width);

            // Clamp so the picker's right edge never crosses the inspector window's.
            var x = Mathf.Max(
                window.position.x,
                Mathf.Min(window.position.x + reference.worldBound.xMin, window.position.xMax - width));

            // From the button's top plus its height: anchoring at yMax double-counts the height and drops a row lower.
            var screenRect = new Rect(
                x,
                window.position.y + anchor.worldBound.yMin,
                width,
                anchor.worldBound.height);

            ShowAppendPicker(target, arrayPath, elementType, baseTypes, screenRect);
        }

        // Shared by the UIToolkit add override and the IMGUI list drawer, which differ only in the anchor rect.
        public static void ShowAppendPicker(Object target, string arrayPath, Type elementType, Type[] baseTypes, Rect screenRect)
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
                onSelected: aqn => Append(target, arrayPath, aqn));
        }

        private static void Append(Object target, string arrayPath, string assemblyQualifiedName)
        {
            if (target == null) return;

            // A <None> pick is valid: the "+" always grows the list, appending an element the user can type later.
            var type = string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName, throwOnError: false);

            // A fresh SerializedObject avoids a stale-binding hazard; the bound ListView refreshes on its next update.
            using var serializedObject = new SerializedObject(target);
            var array = serializedObject.FindProperty(arrayPath);
            if (array is null || !array.isArray) return;

            // arraySize++ copies the previous last element's rid, so overwrite it in the same modification —
            // an explicit null for <None> too — collapsing both into one Undo step.
            var index = array.arraySize;
            array.arraySize = index + 1;
            array.GetArrayElementAtIndex(index).SetManagedReference(type is null ? null : SerializeReferenceHelpers.CreateInstance(type));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
