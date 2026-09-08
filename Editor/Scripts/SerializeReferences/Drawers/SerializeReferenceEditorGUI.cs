using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Provides utility methods for drawing managed-reference type pickers in custom inspectors.
    /// </summary>
    /// <remarks>
    /// Use <see cref="CreateField"/> and <see cref="CreateList"/> in <see cref="Editor.CreateInspectorGUI"/>,
    /// and <see cref="DrawFieldLayout"/> in <see cref="Editor.OnInspectorGUI"/>.
    /// </remarks>
    public static class SerializeReferenceEditorGUI
    {
        /// <summary>
        /// Creates a UI Toolkit type picker with nested fields and managed-reference notices.
        /// </summary>
        /// <param name="property">A managed-reference property of the editor's <see cref="SerializedObject"/>.</param>
        /// <param name="label"><paramref name="property"/> label; <see langword="null"/> uses its display name.</param>
        /// <param name="baseTypes">Additional picker constraints; <see langword="null"/> or an empty array adds no constraints.</param>
        /// <returns>The field to add to the inspector's visual tree.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not a managed reference.</exception>
        public static VisualElement CreateField(SerializedProperty property, string label = null, params Type[] baseTypes)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            return property.propertyType is not SerializedPropertyType.ManagedReference
                ? throw new ArgumentException("CreateField expects a [SerializeReference] managed-reference property; for a list/array of them use CreateList.", nameof(property))
                : new SerializeReferenceField(label ?? property.displayName, property, baseTypes);
        }

        /// <summary>
        /// Creates a UI Toolkit managed-reference list whose add button selects a type and appends an independent instance.
        /// </summary>
        /// <param name="property">An array/list property whose elements are managed references.</param>
        /// <param name="label"><paramref name="property"/> header label; <see langword="null"/> uses its display name.</param>
        /// <param name="baseTypes">Additional element-type constraints; <see langword="null"/> or an empty array adds no constraints.</param>
        /// <returns>The list to add to the inspector's visual tree.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not a managed-reference array.</exception>
        public static VisualElement CreateList(SerializedProperty property, string label = null, params Type[] baseTypes)
        {
            if (property is null) throw new ArgumentNullException(nameof(property));
            if (!SerializeReferenceHelpers.IsManagedReferenceArray(property))
                throw new ArgumentException("CreateList expects an array/list property whose elements are [SerializeReference] managed references.", nameof(property));

            return new SerializeReferenceListField(
                label ?? property.displayName,
                property,
                SerializeReferenceHelpers.GetArrayElementType(property),
                baseTypes);
        }

        /// <summary>
        /// Draws a managed-reference type picker and its nested fields in an IMGUI layout.
        /// </summary>
        /// <remarks>Lists use <see cref="SerializeReferenceIMGUIList.Draw"/>.</remarks>
        /// <param name="property">A managed-reference property of the editor's <see cref="SerializedObject"/>.</param>
        /// <param name="label"><paramref name="property"/> label; <see langword="null"/> uses its display name.</param>
        /// <param name="baseTypes">Additional picker constraints; <see langword="null"/> or an empty array adds no constraints.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not a managed reference.</exception>
        public static void DrawFieldLayout(SerializedProperty property, GUIContent label = null, params Type[] baseTypes)
        {
            if (property is null) throw new ArgumentNullException(nameof(property));
            if (property.propertyType is not SerializedPropertyType.ManagedReference)
                throw new ArgumentException("DrawFieldLayout expects a [SerializeReference] managed-reference property; for a list/array of them use SerializeReferenceIMGUIList.Draw.", nameof(property));

            label ??= new GUIContent(property.displayName);

            var height = SerializeReferenceIMGUIPropertyDrawer.GetHeight(property);
            var rect = EditorGUILayout.GetControlRect(hasLabel: true, height);
            SerializeReferenceIMGUIPropertyDrawer.Draw(rect, label, property, baseTypes);
        }
    }
}
