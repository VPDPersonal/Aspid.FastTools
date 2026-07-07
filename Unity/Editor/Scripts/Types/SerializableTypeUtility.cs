#nullable enable
using System;
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
        /// <summary>
        /// True for an <see cref="ISerializableType"/> wrapper field, or an array / <see cref="List{T}"/> of them.
        /// </summary>
        public static bool IsSerializableTypeField(Type fieldType)
        {
            var type = UnwrapCollection(fieldType);
            return type is not null && typeof(ISerializableType).IsAssignableFrom(type);
        }

        /// <summary>
        /// Resolves the wrapper's <see cref="ISerializableType.BaseType"/> from a field's declared type.
        /// Returns <c>false</c> when the field is not an <see cref="ISerializableType"/> wrapper.
        /// </summary>
        public static bool TryGetBaseType(Type fieldType, out Type baseType)
        {
            var type = UnwrapCollection(fieldType);

            if (type is null || !typeof(ISerializableType).IsAssignableFrom(type))
            {
                baseType = null!;
                return false;
            }

            // BaseType is an instance member, so instantiate the wrapper to read it — the interface
            // contract requires implementations to keep a public parameterless constructor for this.
            baseType = ((ISerializableType)Activator.CreateInstance(type)).BaseType;
            return true;
        }

        // Unwraps an array / List<T> to its element. Matches List<> by open definition so a generic
        // wrapper like SerializableType<T> is not mistaken for a collection and unwrapped by accident.
        private static Type? UnwrapCollection(Type fieldType)
        {
            if (fieldType.IsArray)
                return fieldType.GetElementType();

            if (fieldType is { IsGenericType: true } && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                return fieldType.GetGenericArguments()[0];

            return fieldType;
        }
    }
}
