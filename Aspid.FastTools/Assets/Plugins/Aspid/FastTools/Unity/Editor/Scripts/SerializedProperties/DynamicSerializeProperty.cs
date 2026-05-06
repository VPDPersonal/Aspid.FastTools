using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// Lazy reference to a <see cref="SerializedProperty"/> identified by its target <see cref="Object"/>
    /// and <see cref="SerializedProperty.propertyPath"/>.
    /// </summary>
    /// <remarks>
    /// Useful when a property reference must outlive the originating <see cref="SerializedObject"/>
    /// (for example, after a domain reload or when the property is captured for later use in a callback).
    /// Each call to <see cref="GetProperty"/> — including the implicit conversion to
    /// <see cref="SerializedProperty"/> — allocates a fresh <see cref="SerializedObject"/> and resolves
    /// the property by path, so prefer caching the resolved <see cref="SerializedProperty"/> for hot paths.
    /// </remarks>
    public sealed class DynamicSerializeProperty
    {
        private readonly string _propertyPath;
        private readonly Object _targetObject;

        /// <summary>
        /// Initializes a new instance from an existing <see cref="SerializedProperty"/>,
        /// capturing its <see cref="SerializedProperty.propertyPath"/> and target object.
        /// </summary>
        /// <param name="property">Property whose path and target object should be captured.</param>
        public DynamicSerializeProperty(SerializedProperty property)
            : this(property.propertyPath, property.serializedObject) { }

        /// <summary>
        /// Initializes a new instance from a property path and a <see cref="SerializedObject"/>,
        /// using the object's <see cref="SerializedObject.targetObject"/> as the resolution target.
        /// </summary>
        /// <param name="propertyPath">The serialized property path (as in <see cref="SerializedProperty.propertyPath"/>).</param>
        /// <param name="serializedObject">Owner whose target object is captured.</param>
        public DynamicSerializeProperty(string propertyPath, SerializedObject serializedObject)
            : this(propertyPath, serializedObject.targetObject) { }

        /// <summary>
        /// Initializes a new instance from a property path and the target <see cref="Object"/> directly.
        /// </summary>
        /// <param name="propertyPath">The serialized property path (as in <see cref="SerializedProperty.propertyPath"/>).</param>
        /// <param name="targetObject">The Unity object that owns the serialized property.</param>
        public DynamicSerializeProperty(string propertyPath, Object targetObject)
        {
            _propertyPath = propertyPath;
            _targetObject = targetObject;
        }

        /// <summary>
        /// Resolves the captured path against a freshly created <see cref="SerializedObject"/>
        /// and returns the corresponding <see cref="SerializedProperty"/>.
        /// </summary>
        /// <returns>
        /// The resolved <see cref="SerializedProperty"/>, or <see langword="null"/> if the path
        /// cannot be found on the current state of the target object.
        /// </returns>
        /// <remarks>Each call allocates a new <see cref="SerializedObject"/>.</remarks>
        public SerializedProperty GetProperty() =>
            new SerializedObject(_targetObject).FindProperty(_propertyPath);

        /// <summary>
        /// Implicitly captures a <see cref="SerializedProperty"/> as a <see cref="DynamicSerializeProperty"/>.
        /// </summary>
        /// <param name="property">Property whose path and target object should be captured.</param>
        /// <returns>A new <see cref="DynamicSerializeProperty"/> referencing the same path and target object.</returns>
        public static implicit operator DynamicSerializeProperty(SerializedProperty property) => new(property);

        /// <summary>
        /// Implicitly resolves a <see cref="DynamicSerializeProperty"/> back to a fresh <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The dynamic reference to resolve.</param>
        /// <returns>The resolved <see cref="SerializedProperty"/>.</returns>
        /// <remarks>
        /// Each conversion allocates a new <see cref="SerializedObject"/>; avoid implicit conversion
        /// inside tight loops — call <see cref="GetProperty"/> once and cache the result.
        /// </remarks>
        public static implicit operator SerializedProperty(DynamicSerializeProperty property) => property.GetProperty();
    }
}
