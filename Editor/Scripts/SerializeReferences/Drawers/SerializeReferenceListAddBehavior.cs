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
            if (serializedObject is null || serializedObject.isEditingMultipleObjects) return;

            var path = elementProperty.propertyPath;
            var arrayMarker = path.IndexOf(".Array.data[", StringComparison.Ordinal);
            if (arrayMarker < 0) return;

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

        public static void OpenAppendPicker(Object target, string arrayPath, Type elementType, Type[] baseTypes, VisualElement anchor)
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

            ShowAppendPicker(target, arrayPath, elementType, baseTypes, screenRect);
        }

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
