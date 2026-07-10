#nullable enable
using System;
using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Reflection helpers for detecting <see cref="ISerializableType"/> wrapper fields
    /// (<see cref="SerializableType"/> / <see cref="SerializableType{T}"/>) from a field's declared type,
    /// including elements of arrays and <see cref="List{T}"/>.
    /// </summary>
    internal static class SerializableTypeUtility
    {
        // The backing string field a SerializableType / SerializableType<T> serializes its type name into.
        private const string TypeNameBackingField = "_assemblyQualifiedName";

        /// <summary>
        /// The field that carries user attributes for <paramref name="property"/>: its backing field as resolved by
        /// <see cref="SerializePropertyExtensions.GetFieldInfo(SerializedProperty)"/> — except for the backing
        /// <c>_assemblyQualifiedName</c> string nested inside an <see cref="ISerializableType"/> wrapper, which is
        /// redirected to the wrapper field the attributes (e.g. <c>[TypeSelector]</c>) are declared on.
        /// </summary>
        public static FieldInfo? GetAttributeField(SerializedProperty property)
        {
            var field = property.GetFieldInfo();
            if (field?.Name != TypeNameBackingField) return field;

            // The property targets the string inside the wrapper — its parent property is the wrapper field itself.
            var path = property.propertyPath;
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return field;

            using var parentProperty = property.serializedObject.FindProperty(path[..lastDotIndex]);
            var parentField = parentProperty?.GetFieldInfo();

            return parentField is not null && IsSerializableTypeField(parentField.FieldType)
                ? parentField
                : field;
        }

        /// <summary>
        /// True for an <see cref="ISerializableType"/> wrapper field, or an array / <see cref="List{T}"/> of them.
        /// </summary>
        public static bool IsSerializableTypeField(Type fieldType) =>
            typeof(ISerializableType).IsAssignableFrom(fieldType.GetCollectionElementType());

        /// <summary>
        /// Resolves the wrapper's <see cref="ISerializableType.BaseType"/> from a field's declared type.
        /// Returns <c>false</c> when the field is not an <see cref="ISerializableType"/> wrapper.
        /// </summary>
        public static bool TryGetBaseType(Type fieldType, out Type baseType)
        {
            var type = fieldType.GetCollectionElementType();

            if (!typeof(ISerializableType).IsAssignableFrom(type))
            {
                baseType = null!;
                return false;
            }

            // BaseType is an instance member, so instantiate the wrapper to read it — the interface
            // contract requires implementations to keep a public parameterless constructor for this.
            baseType = ((ISerializableType)Activator.CreateInstance(type)).BaseType;
            return true;
        }
    }
}
