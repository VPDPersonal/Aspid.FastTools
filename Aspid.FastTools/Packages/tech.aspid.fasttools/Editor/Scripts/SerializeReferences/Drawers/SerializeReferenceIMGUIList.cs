using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Provides utility methods for drawing managed-reference lists with a type picker for new elements in IMGUI.
    /// </summary>
    /// <remarks>
    /// The add button creates an independent instance in every selected object; element fields retain their registered property drawers.
    /// A <c>[TypeSelector]</c> on the list field adds its constraints to <c>baseTypes</c>.
    /// </remarks>
    public static class SerializeReferenceIMGUIList
    {
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
        /// Draws a managed-reference list whose add button selects a type and appends an independent instance.
        /// </summary>
        /// <param name="listProperty">An array/list property whose elements are managed references.</param>
        /// <param name="label">The list header; <see langword="null"/> uses the display name of <paramref name="listProperty"/>, <see cref="GUIContent.none"/> displays no label.</param>
        /// <param name="elementType">The declared element type constraining the picker, supplied even when the list is empty.</param>
        /// <param name="baseTypes">Additional constraints below <paramref name="elementType"/>; <see langword="null"/> or an empty array adds none.</param>
        /// <exception cref="ArgumentNullException"><paramref name="listProperty"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="listProperty"/> is not a managed-reference array.</exception>
        public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes) =>
            DrawLayout(listProperty: listProperty, label: label, elementType: elementType, baseTypes: baseTypes);

        /// <summary>
        /// Draws a managed-reference list whose add button selects a type and appends an independent instance, taking the element type from the field declaration.
        /// </summary>
        /// <remarks>
        /// When the list is empty and its field cannot be found by reflection, the element type falls back to <see cref="object"/>; call the overload with <c>elementType</c> then.
        /// </remarks>
        /// <param name="listProperty">An array/list property whose elements are managed references.</param>
        /// <param name="label">The list header; <see langword="null"/> uses the display name of <paramref name="listProperty"/>, <see cref="GUIContent.none"/> displays no label.</param>
        /// <param name="baseTypes">Additional constraints below the element type; <see langword="null"/> or an empty array adds none.</param>
        /// <exception cref="ArgumentNullException"><paramref name="listProperty"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="listProperty"/> is not a managed-reference array.</exception>
        public static void Draw(SerializedProperty listProperty, GUIContent label = null, Type[] baseTypes = null) =>
            DrawLayout(listProperty: listProperty, label: label, elementType: null, baseTypes: baseTypes);

        // A null elementType is resolved from the list field when the list is created, not on every GUI event.
        private static void DrawLayout(SerializedProperty listProperty, GUIContent label, Type elementType, Type[] baseTypes)
        {
            if (listProperty is null) throw new ArgumentNullException(nameof(listProperty));
            if (!SerializeReferenceHelpers.IsManagedReferenceArray(listProperty))
                throw new ArgumentException("Draw expects an array/list property whose elements are [SerializeReference] managed references.", nameof(listProperty));

            label ??= new GUIContent(listProperty.displayName);
            baseTypes = TypeSelectorConstraintResolver.AppendFieldConstraints(listProperty, baseTypes);

            GetOrCreate(listProperty, label, elementType, baseTypes, depth: 0).DoLayoutList();
        }

        // Fixed-rect twin of Draw, for a list nested inside a managed reference the drawer is already laying out —
        // a PropertyDrawer measures before it paints, so a layout list cannot be used there.
        internal static void Draw(Rect position, SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            if (listProperty is null || !listProperty.isArray) return;

            GetOrCreate(listProperty, label, elementType, baseTypes, depth).DoList(position);
        }

        internal static float GetHeight(SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            if (listProperty is null || !listProperty.isArray) return 0f;

            return GetOrCreate(listProperty, label, elementType, baseTypes, depth).GetHeight();
        }

        // The label and constraint are refreshed on every call: a member-referenced [TypeSelector] constraint
        // re-resolves while the inspector is open, and the cached list must pick it up.
        private static ReorderableList GetOrCreate(SerializedProperty listProperty, GUIContent label, Type elementType,
            Type[] baseTypes, int depth)
        {
            var list = ReorderableListCache.GetOrCreate(listProperty, () => Create(listProperty, elementType, depth));
            list.Label = label;
            list.BaseTypes = baseTypes;
            return list;
        }

        private static PickerList Create(SerializedProperty listProperty, Type elementType, int depth)
        {
            elementType ??= SerializeReferenceHelpers.GetArrayElementType(listProperty);

            var targets = GetAppendTargets(listProperty);
            var arrayPath = listProperty.propertyPath;

            var list = new PickerList(listProperty.serializedObject, listProperty,
                draggable: !listProperty.IsNonReorderable());

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, list.Label);

            // The background rect spans the box's full inner width, unlike the inset row rect, so it carries the
            // border the pulse band stops at. Drawn only on Repaint, so the captured edge is always fresh.
            var boxRightEdge = 0f;
            list.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                boxRightEdge = rect.xMax;
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, active, focused, draggable: list.draggable);
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

                _elementRightLimits.Push(boxRightEdge);
                try
                {
                    var content = new GUIContent($"Element {index}");

                    // Unity hands [TypeSelector] to the list, not to its elements, so the header is drawn here, as it
                    // is for a nested list, which carries no attribute at all.
                    if (SerializeReferenceNesting.DrawsOwnHeader(element, depth))
                        SerializeReferenceIMGUIPropertyDrawer.Draw(rect, content, element, depth + 1, list.BaseTypes);
                    else
                        EditorGUI.PropertyField(rect, element, content, includeChildren: true);
                }
                finally
                {
                    _elementRightLimits.Pop();
                }
            };

            list.onAddDropdownCallback = (buttonRect, _) =>
            {
                // Anchoring the picker's right edge to the button grows it leftward, so a "+" near the inspector's
                // right edge does not spill off screen.
                var topLeft = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.xMax - PickerWidth, buttonRect.yMin));
                var screenRect = new Rect(topLeft.x, topLeft.y, PickerWidth, buttonRect.height);
                SerializeReferenceListAddBehavior.ShowAppendPicker(targets, arrayPath, elementType, list.BaseTypes, screenRect);
            };

            return list;
        }

        // Every target, so "+" under a multi-object selection appends to each object, not only the first.
        internal static Object[] GetAppendTargets(SerializedProperty listProperty) =>
            listProperty.serializedObject.targetObjects;

        private static float ElementHeight(SerializedProperty element, int depth) =>
            SerializeReferenceNesting.DrawsOwnHeader(element, depth)
                ? SerializeReferenceIMGUIPropertyDrawer.GetHeight(element, depth + 1)
                : EditorGUI.GetPropertyHeight(element, includeChildren: true);

        private sealed class PickerList : ReorderableList
        {
            public GUIContent Label;
            public Type[] BaseTypes;

            public PickerList(SerializedObject serializedObject, SerializedProperty elements, bool draggable)
                : base(serializedObject, elements, draggable, displayHeader: true, displayAddButton: true, displayRemoveButton: true) { }
        }
    }
}
