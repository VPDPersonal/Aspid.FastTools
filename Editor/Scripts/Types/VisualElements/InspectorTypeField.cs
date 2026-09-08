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
        public InspectorTypeField(SerializedProperty property)
            : base(property)
        {
            Initialize();
        }

        /// <summary>
        /// Creates an Inspector-aligned field bound to a type-name property.
        /// </summary>
        /// <param name="label">The field label, or <see langword="null"/> for no label.</param>
        /// <param name="property">The string property storing the assembly-qualified type name.</param>
        public InspectorTypeField(string label, SerializedProperty property)
            : base(label, property)
        {
            Initialize();
        }

        /// <summary>
        /// Creates an unbound Inspector-aligned field with an initial type.
        /// </summary>
        /// <param name="label">The field label, or <see langword="null"/> for no label.</param>
        /// <param name="defaultValue">The initial type, or <see langword="null"/> for an empty selection.</param>
        public InspectorTypeField(string label, Type defaultValue = null)
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
