using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // The IMGUI twin of TypeSelectorListField: a [TypeSelector] list of type names or type wrappers whose elements the
    // drawer paints, since the attribute applies to the collection and Unity no longer hands it each element.
    internal static class TypeSelectorIMGUIList
    {
        // Unity's default array UI insets element rects by this much past the drag handle through the internal
        // m_HasPropertyDrawer flag. That flag is unreachable from package code, so the inset is applied by hand.
        private const float PropertyDrawerPadding = 8f;

        internal static void Draw(Rect position, SerializedProperty listProperty, GUIContent label,
            Func<SerializedProperty, float> getElementHeight, Action<Rect, SerializedProperty, GUIContent> drawElement) =>
            GetOrCreate(listProperty, label, getElementHeight, drawElement).DoList(position);

        internal static float GetHeight(SerializedProperty listProperty, GUIContent label,
            Func<SerializedProperty, float> getElementHeight, Action<Rect, SerializedProperty, GUIContent> drawElement) =>
            GetOrCreate(listProperty, label, getElementHeight, drawElement).GetHeight();

        // The callbacks are refreshed on every call: they carry the constraint the drawer resolved for this pass.
        private static ReorderableList GetOrCreate(SerializedProperty listProperty, GUIContent label,
            Func<SerializedProperty, float> getElementHeight, Action<Rect, SerializedProperty, GUIContent> drawElement)
        {
            var list = ReorderableListCache.GetOrCreate(listProperty, () => Create(listProperty));
            list.Label = label;
            list.GetElementHeight = getElementHeight;
            list.DrawElement = drawElement;
            return list;
        }

        private static ElementList Create(SerializedProperty listProperty)
        {
            var list = new ElementList(listProperty.serializedObject, listProperty,
                draggable: !listProperty.IsNonReorderable());

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, list.Label);

            list.elementHeightCallback = index =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                return list.GetElementHeight(element) + EditorGUIUtility.standardVerticalSpacing * 2f;
            };

            list.drawElementCallback = (rect, index, _, _) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.xMin += PropertyDrawerPadding;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                rect.height = list.GetElementHeight(element);

                list.DrawElement(rect, element, new GUIContent(element.displayName));
            };

            return list;
        }

        private sealed class ElementList : ReorderableList
        {
            public GUIContent Label;
            public Func<SerializedProperty, float> GetElementHeight;
            public Action<Rect, SerializedProperty, GUIContent> DrawElement;

            public ElementList(SerializedObject serializedObject, SerializedProperty elements, bool draggable)
                : base(serializedObject, elements, draggable, displayHeader: true, displayAddButton: true, displayRemoveButton: true) { }
        }
    }
}
