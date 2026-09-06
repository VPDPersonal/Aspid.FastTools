using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Provides utility methods for drawing an IMGUI <c>[SerializeReference]</c> list whose add button opens the type
    /// picker and appends a fresh instance.
    /// </summary>
    /// <remarks>
    /// In IMGUI a <c>[TypeSelector]</c> drawer is applied to array elements and can never reach the list's own "+", so
    /// an editor that overrides <c>OnInspectorGUI</c> gets Unity's default add — which duplicates the last element and
    /// leaves it rid-aliased. Call <see cref="Draw"/> for those lists instead. Elements still go through
    /// <see cref="EditorGUI.PropertyField(Rect, SerializedProperty, GUIContent, bool)"/>, so the per-element drawer
    /// applies exactly as it would by default.
    /// </remarks>
    public static class SerializeReferenceIMGUIList
    {
        // ReorderableList holds per-list UI state (selection, drag), so it must survive across OnInspectorGUI calls.
        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // Stacked, so a list nested in another list's element restores the outer box's edge when it finishes.
        private static readonly Stack<float> _elementRightLimits = new();

        // Matches TypeSelectorWindow.Show's own floor, so the right-aligned anchor reflects the picker's true width.
        private const float PickerWidth = 350f;

        // Unity's default array UI insets element rects by this much past the drag handle through the internal
        // m_HasPropertyDrawer flag. That flag is unreachable from package code, so the inset is applied by hand.
        private const float PropertyDrawerPadding = 8f;

        // The right edge of the list box whose element is being drawn, NaN outside any element: the drawer's
        // group-navigation pulse stops its band at the box border instead of the inspector's right edge.
        internal static float CurrentElementRightLimit =>
            _elementRightLimits.Count > 0 ? _elementRightLimits.Peek() : float.NaN;

        /// <summary>
        /// Draws a <c>[SerializeReference]</c> list with a picker-backed "+".
        /// </summary>
        /// <param name="listProperty">The array/list property to draw. Its elements must be managed references.</param>
        /// <param name="label">Header label for the list.</param>
        /// <param name="elementType">Declared element type constraining the picker; needed up front because an empty
        /// list has no element to read it from.</param>
        /// <param name="baseTypes">Base types narrowing the candidates below <paramref name="elementType"/>.</param>
        public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
        {
            if (listProperty is null || !listProperty.isArray) return;

            var list = GetOrCreate(listProperty, label, elementType, baseTypes, depth: 0);

            // The property instance is rebuilt every OnInspectorGUI; re-point the cached list at the current one so
            // its callbacks never touch a disposed property.
            list.serializedProperty = listProperty;
            list.DoLayoutList();
        }

        // Fixed-rect twin of Draw, for a list nested inside a managed reference the drawer is already laying out —
        // a PropertyDrawer measures before it paints, so a layout list cannot be used there.
        internal static void Draw(Rect position, SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            if (listProperty is null || !listProperty.isArray) return;

            var list = GetOrCreate(listProperty, label, elementType, baseTypes, depth);
            list.serializedProperty = listProperty;
            list.DoList(position);
        }

        internal static float GetHeight(SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            if (listProperty is null || !listProperty.isArray) return 0f;

            var list = GetOrCreate(listProperty, label, elementType, baseTypes, depth);
            list.serializedProperty = listProperty;

            return list.GetHeight();
        }

        private static ReorderableList GetOrCreate(SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            var serializedObject = listProperty.serializedObject;

            // The SerializedObject is part of the key: an Inspector plus a locked Inspector hold two distinct ones for
            // the same (target, path), and a shared key would rebuild the list on every alternating repaint.
            var key = $"{RuntimeHelpers.GetHashCode(serializedObject)}/" +
                      $"{serializedObject.targetObject.GetInstanceID()}/{listProperty.propertyPath}";

            // A cached list bound to a stale SerializedObject (e.g. after a domain reload) must be rebuilt, not reused.
            if (Lists.TryGetValue(key, out var cached) && cached.serializedProperty.serializedObject == serializedObject)
                return cached;

            // Entries pin their SerializedObject, so a closed editor's entry would live until the next domain reload.
            // Swept on cache misses only, which are already the slow path.
            EvictDeadEntries();

            // Both are stable for the field's lifetime, and Append opens its own fresh SerializedObject.
            var target = serializedObject.targetObject;
            var arrayPath = listProperty.propertyPath;

            // Built before the callbacks so their lambdas can close over `list`; an object-initializer self-reference
            // under `var` would not compile.
            var list = new ReorderableList(serializedObject, listProperty,
                draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);

            // The background rect spans the box's full inner width, unlike the inset row rect, so it carries the
            // border the pulse band stops at. Drawn only on Repaint, so the captured edge is always fresh.
            var boxRightEdge = 0f;
            list.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                boxRightEdge = rect.xMax;
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, active, focused, draggable: true);
            };

            list.elementHeightCallback = index =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                return ElementHeight(element, depth) + EditorGUIUtility.standardVerticalSpacing * 2f;
            };

            list.drawElementCallback = (rect, index, _, _) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.xMin += PropertyDrawerPadding;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                rect.height = ElementHeight(element, depth);

                // The pushed limit tells the element's drawer where this row's box ends.
                _elementRightLimits.Push(boxRightEdge);
                try
                {
                    var content = new GUIContent($"Element {index}");

                    // A nested list has no [TypeSelector] on its elements, so the header is drawn here instead.
                    if (SerializeReferenceNesting.DrawsOwnHeader(element, depth))
                        SerializeReferenceIMGUIPropertyDrawer.Draw(rect, content, element, depth + 1, baseTypes);
                    else
                        EditorGUI.PropertyField(rect, element, content, includeChildren: true);
                }
                finally
                {
                    _elementRightLimits.Pop();
                }
            };

            // Replaces Unity's default add, which duplicates the last element and leaves it rid-aliased.
            list.onAddDropdownCallback = (buttonRect, _) =>
            {
                // Anchoring the picker's right edge to the button grows it leftward, so a "+" near the inspector's
                // right edge does not spill off screen.
                var topLeft = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.xMax - PickerWidth, buttonRect.yMin));
                var screenRect = new Rect(topLeft.x, topLeft.y, PickerWidth, buttonRect.height);
                SerializeReferenceListAddBehavior.ShowAppendPicker(target, arrayPath, elementType, baseTypes, screenRect);
            };

            Lists[key] = list;
            return list;
        }

        // Measured the same way the row is drawn, or the reserved rect and the painted content disagree.
        private static float ElementHeight(SerializedProperty element, int depth) =>
            SerializeReferenceNesting.DrawsOwnHeader(element, depth)
                ? SerializeReferenceIMGUIPropertyDrawer.GetHeight(element, depth + 1)
                : EditorGUI.GetPropertyHeight(element, includeChildren: true);

        private static void EvictDeadEntries()
        {
            List<string> dead = null;

            foreach (var pair in Lists)
            {
                bool alive;
                try
                {
                    alive = pair.Value.serializedProperty.serializedObject.targetObject != null;
                }
                catch (Exception)
                {
                    // A disposed SerializedObject throws on access — the entry is dead either way.
                    alive = false;
                }

                if (!alive) (dead ??= new List<string>()).Add(pair.Key);
            }

            if (dead is null) return;
            foreach (var key in dead) Lists.Remove(key);
        }
    }
}
