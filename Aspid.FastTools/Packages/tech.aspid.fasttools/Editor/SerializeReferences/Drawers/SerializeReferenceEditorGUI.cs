using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Provides utility methods for drawing <c>[SerializeReference]</c> properties with the package's type-dropdown
    /// UI from a custom editor's own code, with no <c>[TypeSelector]</c> attribute.
    /// </summary>
    /// <remarks>
    /// Call <see cref="CreateField"/> and <see cref="CreateList"/> from <c>CreateInspectorGUI</c>, and
    /// <see cref="DrawFieldLayout"/> from an IMGUI <c>OnInspectorGUI</c>; IMGUI lists go through
    /// <see cref="SerializeReferenceIMGUIList.Draw"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public override VisualElement CreateInspectorGUI()
    /// {
    ///     var root = new VisualElement();
    ///     root.Add(SerializeReferenceEditorGUI.CreateField(serializedObject.FindProperty("_weapon")));
    ///     root.Add(SerializeReferenceEditorGUI.CreateList(serializedObject.FindProperty("_modifiers")));
    ///     return root;
    /// }
    /// </code>
    /// </example>
    public static class SerializeReferenceEditorGUI
    {
        /// <summary>
        /// Builds the dropdown field for one <c>[SerializeReference]</c> property: a foldout whose header carries the
        /// type dropdown and whose content hosts the instance's fields, with the package's usual notices.
        /// </summary>
        /// <param name="property">A managed-reference property of the editor's <see cref="SerializedObject"/>.</param>
        /// <param name="label">Field label; the property's display name when omitted.</param>
        /// <param name="baseTypes">Base types narrowing the picker below the field's declared type.</param>
        /// <returns>The field to add to the inspector's visual tree.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the property is not a managed reference.</exception>
        public static VisualElement CreateField(SerializedProperty property, string label = null, params Type[] baseTypes)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            return property.propertyType is not SerializedPropertyType.ManagedReference
                ? throw new ArgumentException("CreateField expects a [SerializeReference] managed-reference property; for a list/array of them use CreateList.", nameof(property))
                : new SerializeReferenceField(label ?? property.displayName, property, baseTypes);
        }

        /// <summary>
        /// Builds the list for a <c>[SerializeReference]</c> array: every element renders as the dropdown field and
        /// the "+" opens the type picker, appending a fresh instance instead of a rid-aliased duplicate.
        /// </summary>
        /// <param name="property">An array/list property whose elements are managed references.</param>
        /// <param name="label">Header label; the property's display name when omitted.</param>
        /// <param name="baseTypes">Base types narrowing the picker below the declared element type.</param>
        /// <returns>The list to add to the inspector's visual tree.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the property is not a managed-reference array.</exception>
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
        /// Reserves a layout rect and draws into it the same dropdown field as <see cref="CreateField"/>.
        /// </summary>
        /// <remarks>Lists use <see cref="SerializeReferenceIMGUIList.Draw"/>.</remarks>
        /// <param name="property">A managed-reference property of the editor's <see cref="SerializedObject"/>.</param>
        /// <param name="label">Field label; the property's display name when omitted.</param>
        /// <param name="baseTypes">Base types narrowing the picker below the field's declared type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the property is not a managed reference.</exception>
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
