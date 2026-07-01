using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// IMGUI parity for the UIToolkit ListView's picker-backed "+": draws a <c>[SerializeReference]</c> list/array whose
    /// add button opens the type picker and appends a fresh typed instance (or an empty <c>&lt;None&gt;</c> element),
    /// mirroring <see cref="SerializeReferenceListAddBehavior"/>.
    /// </summary>
    /// <remarks>
    /// Unity applies a <c>[TypeSelector]</c> <see cref="PropertyDrawer"/> to array <b>elements</b> in the IMGUI path, so
    /// the drawer can never reach the list's own "+" button — the UIToolkit side only manages it by walking up to the
    /// live <c>ListView</c>, which immediate-mode IMGUI has no equivalent of. A custom <see cref="Editor"/> that forces
    /// IMGUI (overrides <c>OnInspectorGUI</c> without <c>CreateInspectorGUI</c>) therefore gets Unity's default add on its
    /// <c>[SerializeReference]</c> lists — duplicating the last element and leaving it rid-aliased. Call
    /// <see cref="Draw"/> for those lists instead to restore the picker-backed, de-aliased add. Elements are drawn with
    /// <see cref="EditorGUI.PropertyField(Rect, SerializedProperty, GUIContent, bool)"/>, so each still routes through the
    /// <c>[TypeSelector]</c> drawer exactly as the default list drawing would.
    /// </remarks>
    public static class SerializeReferenceIMGUIList
    {
        // ReorderableList caches per-list UI state (selection, drag), so it must persist across OnInspectorGUI calls —
        // keyed by (target instance id + property path), which is stable for a given field across repaints.
        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // Minimum picker width, matching TypeSelectorWindow.Show's own floor, so the right-aligned anchor below reflects
        // the picker's true footprint.
        private const float PickerWidth = 350f;

        // Unity's default array UI (ReorderableListWrapper) flags its list with the internal m_HasPropertyDrawer, which
        // makes GetContentRect inset the element rect by Defaults.propertyDrawerPadding (8) past the drag handle. That
        // flag is unreachable from package code, so the same inset is applied manually in drawElementCallback — without
        // it every row starts 8px further left than the geometry SerializeReferenceIMGUIPropertyDrawer is tuned for:
        // the foldout arrow crowds the drag handle and the shared/missing stripe (content.x - StripeOffset) lands ON it.
        private const float PropertyDrawerPadding = 8f;

        /// <summary>
        /// Draws <paramref name="listProperty"/> (a <c>[SerializeReference]</c> list/array) with a picker-backed "+".
        /// </summary>
        /// <param name="listProperty">The array/list property to draw. Its elements must be managed references.</param>
        /// <param name="label">Header label for the list.</param>
        /// <param name="elementType">The declared element type constraining the picker (e.g. the list's <c>T</c>). Needed
        /// up front because an empty list has no element to read it from.</param>
        /// <param name="baseTypes">Optional base types that narrow the candidate list below <paramref name="elementType"/>,
        /// mirroring the <c>[TypeSelector(...)]</c> arguments.</param>
        public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
        {
            if (listProperty is null || !listProperty.isArray) return;

            var list = GetOrCreate(listProperty, label, elementType, baseTypes);

            // The SerializedProperty instance is rebuilt every OnInspectorGUI (a fresh iterator/FindProperty); re-point
            // the cached list at the current one so its callbacks never touch a disposed property.
            list.serializedProperty = listProperty;
            list.DoLayoutList();
        }

        private static ReorderableList GetOrCreate(SerializedProperty listProperty, GUIContent label, Type elementType, Type[] baseTypes)
        {
            var serializedObject = listProperty.serializedObject;
            var key = $"{serializedObject.targetObject.GetInstanceID()}/{listProperty.propertyPath}";

            // A cached list bound to a stale SerializedObject (e.g. after a domain reload) must be rebuilt, not reused.
            if (Lists.TryGetValue(key, out var cached) && cached.serializedProperty.serializedObject == serializedObject)
                return cached;

            // Capture the append target/path once: both are stable for the field's lifetime, and Append opens its own
            // fresh SerializedObject anyway (so no stale-binding hazard from the captured object).
            var target = serializedObject.targetObject;
            var arrayPath = listProperty.propertyPath;

            // Declared and assigned before the callbacks so their lambdas can close over `list` (an object-initializer
            // self-reference under `var` fails to compile — type inference cycle — so build it, then wire callbacks).
            var list = new ReorderableList(serializedObject, listProperty,
                draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);

            list.elementHeightCallback = index =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, includeChildren: true) +
                       EditorGUIUtility.standardVerticalSpacing * 2f;
            };

            list.drawElementCallback = (rect, index, _, _) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.xMin += PropertyDrawerPadding;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUI.GetPropertyHeight(element, includeChildren: true);
                // Drawn through the standard field, so the element still routes through the [TypeSelector] drawer.
                EditorGUI.PropertyField(rect, element, new GUIContent($"Element {index}"), includeChildren: true);
            };

            // Replace Unity's default add (which duplicates the last element, leaving it rid-aliased) with the picker,
            // matching the UIToolkit ListView override. onAddDropdownCallback hands us the "+" button rect to anchor.
            list.onAddDropdownCallback = (buttonRect, _) =>
            {
                // Anchor the picker's RIGHT edge to the button and open flush below it, so a "+" near the inspector's
                // right edge grows the dropdown leftward into the window instead of spilling off to the right.
                var topLeft = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.xMax - PickerWidth, buttonRect.yMin));
                var screenRect = new Rect(topLeft.x, topLeft.y, PickerWidth, buttonRect.height);
                SerializeReferenceListAddBehavior.ShowAppendPicker(target, arrayPath, elementType, baseTypes, screenRect);
            };

            Lists[key] = list;
            return list;
        }
    }
}
