using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Property drawer for <see cref="SerializeReferenceSelectorAttribute"/>. Delegates rendering to the
    /// IMGUI and UIToolkit helpers, constraining the candidate list to the field's declared managed-reference type.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    internal sealed class SerializeReferenceSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ThrowExceptionIfInvalidProperty(property);

            SerializeReferenceIMGUIPropertyDrawer.Draw(
                position: position,
                label: label,
                property: property,
                types: SerializeReferenceHelpers.GetFieldType(property));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return EditorGUIUtility.singleLineHeight;

            return SerializeReferenceIMGUIPropertyDrawer.GetHeight(property);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ThrowExceptionIfInvalidProperty(property);

            return SerializeReferenceUIToolkitPropertyDrawer.Draw(
                label: preferredLabel,
                property: property);
        }

        private static void ThrowExceptionIfInvalidProperty(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                throw new ArgumentException(
                    "[SerializeReferenceSelector] can only be applied to a [SerializeReference] field.",
                    nameof(property));
        }
    }
}
