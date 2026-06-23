using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Replaces the default "+" on a <c>List&lt;[SerializeReference]&gt;</c> / array (which duplicates the last element
    /// and leaves it rid-aliased) with one that opens the type picker and appends a fresh typed instance — killing the
    /// alias at the source. <see cref="SerializeReferenceDuplicateGuard"/> remains the fallback for the native add path
    /// (Ctrl+D, paste, multi-object selections).
    /// </summary>
    internal static class SerializeReferenceListAddBehavior
    {
        /// <summary>
        /// Installs the picker-backed add behavior on the ListView hosting <paramref name="elementField"/>, once. No-op
        /// for non-list elements, multi-object selections (handled by the de-alias guard), or when an override is
        /// already present.
        /// </summary>
        public static void TryInstall(VisualElement elementField, SerializedProperty elementProperty, Type elementType, Type[] baseTypes)
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

            // Assigning overridingAddButtonBehavior calls ListView.RefreshItems(), which rebuilds the list's item
            // elements — a hierarchy change. TryInstall runs from the element's AttachToPanelEvent (the only point the
            // ListView ancestor is reachable), so the subtree is still mid-attach; mutating it synchronously throws
            // "Modifying the parent of a VisualElement while it's already being modified". Defer one tick — anchored to
            // the long-lived ListView so it survives item recycling — and re-check the guard, since sibling elements
            // attaching in the same pass each queue their own install before the first one lands.
            listView.schedule.Execute(() =>
            {
                if (listView.overridingAddButtonBehavior != null) return;

                listView.overridingAddButtonBehavior = (_, button) =>
                    OpenAppendPicker(target, arrayPath, elementType, baseTypes, button);
            });
        }

        private static void OpenAppendPicker(Object target, string arrayPath, Type elementType, Type[] baseTypes, VisualElement anchor)
        {
            var window = EditorWindow.mouseOverWindow != null ? EditorWindow.mouseOverWindow : EditorWindow.focusedWindow;
            if (window == null) return;

            var screenRect = new Rect(
                window.position.x + anchor.worldBound.xMin,
                window.position.y + anchor.worldBound.yMax,
                Mathf.Max(anchor.worldBound.width, 240f),
                anchor.worldBound.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { elementType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: aqn => Append(target, arrayPath, aqn),
                filter: SerializeReferenceHelpers.BuildAssignableFilter(baseTypes),
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(elementType, baseTypes),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);
        }

        private static void Append(Object target, string arrayPath, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (type is null || target == null) return;

            // A fresh SerializedObject avoids any stale-binding hazard from a captured one; the bound inspector ListView
            // refreshes from the changed target on its next update.
            var serializedObject = new SerializedObject(target);
            var array = serializedObject.FindProperty(arrayPath);
            if (array is null || !array.isArray) return;

            // Grow the array and assign the fresh instance before a single apply. arraySize++ copies the previous last
            // element's managedReferenceId, but assigning a fresh instance overwrites it in the same modification —
            // collapsing both into one Undo step and leaving no rid-aliased duplicate behind.
            var index = array.arraySize;
            array.arraySize = index + 1;
            array.GetArrayElementAtIndex(index).SetManagedReference(SerializeReferenceHelpers.CreateInstance(type));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
