using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// <see cref="TypeField"/> pre-styled as an Inspector property row, so its label aligns with sibling fields.
    /// </summary>
    [UxmlElement]
    public sealed partial class InspectorTypeField : TypeField
    {
        /// <summary>
        /// Creates an unbound field with Inspector label alignment.
        /// </summary>
        public InspectorTypeField()
        {
            Initialize();
        }

        /// <summary>
        /// Creates an Inspector-aligned field labeled with the bound property's display name.
        /// </summary>
        /// <param name="property">The string property storing the assembly-qualified type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="property"/> is not a string property, or its path no longer exists on its targets.
        /// </exception>
        public InspectorTypeField(SerializedProperty property)
            : base(property)
        {
            Initialize();
        }

        /// <summary>
        /// Creates an Inspector-aligned field bound to a type-name property.
        /// </summary>
        /// <remarks>
        /// Before <see cref="ChangeEvent{T}"/> is sent, a pick writes the property and calls
        /// <see cref="SerializedObject.Update"/> on the <see cref="SerializedObject"/> of <paramref name="property"/>:
        /// a change handler reads the picked name from <paramref name="property"/>, and edits it applies through that
        /// object keep the pick. Changes not yet applied to that object are lost.
        /// </remarks>
        /// <param name="label">The field label, or <see langword="null"/> for no label.</param>
        /// <param name="property">The string property storing the assembly-qualified type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="property"/> is not a string property, or its path no longer exists on its targets.
        /// </exception>
        public InspectorTypeField(string label, SerializedProperty property)
            : base(label, property)
        {
            Initialize();
        }

        /// <summary>
        /// Creates an unbound Inspector-aligned field without an initial type.
        /// </summary>
        /// <param name="label">The field label, or <see langword="null"/> for no label.</param>
        public InspectorTypeField(string label)
            : base(label)
        {
            Initialize();
        }

        /// <summary>
        /// Creates an unbound Inspector-aligned field with an initial type.
        /// </summary>
        /// <param name="label">The field label, or <see langword="null"/> for no label.</param>
        /// <param name="defaultValue">The initial type, or <see langword="null"/> for an empty selection.</param>
        public InspectorTypeField(string label, Type defaultValue)
            : base(label, defaultValue)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.AddClass(alignedFieldUssClassName)
                .AddClass(PropertyField.ussClassName);
            
            labelElement.AddClass(PropertyField.labelUssClassName);
        }
    }
}
