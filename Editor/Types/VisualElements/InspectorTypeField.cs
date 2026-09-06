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
    /// <remarks>
    /// Use it in custom property drawers; a stand-alone editor window uses <see cref="TypeField"/> directly.
    /// </remarks>
    [UxmlElement]
    public sealed partial class InspectorTypeField : TypeField
    {
        /// <inheritdoc cref="TypeField()"/>
        public InspectorTypeField()
        {
            Initialize();
        }

        /// <inheritdoc cref="TypeField(SerializedProperty)"/>
        public InspectorTypeField(SerializedProperty property)
            : base(property)
        {
            Initialize();
        }

        /// <inheritdoc cref="TypeField(string, SerializedProperty)"/>
        public InspectorTypeField(string label, SerializedProperty property)
            : base(label, property)
        {
            Initialize();
        }

        /// <inheritdoc cref="TypeField(string, Type)"/>
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
