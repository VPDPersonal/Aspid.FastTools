#nullable enable
using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Common contract of the serializable <see cref="System.Type"/> wrappers
    /// (<see cref="SerializableType"/> and <see cref="SerializableType{T}"/>).
    /// Lets code accept either wrapper polymorphically and lets editor tooling
    /// detect wrapper fields via a single interface check.
    /// </summary>
    /// <remarks>
    /// Unity does not serialize interface-typed fields — declare fields as a concrete
    /// wrapper and use this interface in APIs that consume one. Implementations must
    /// keep a public parameterless constructor (editor tooling instantiates the field
    /// type to read <see cref="BaseType"/>).
    /// </remarks>
    public interface ISerializableType
    {
        /// <summary>
        /// The base constraint type: <see cref="object"/> when unconstrained,
        /// otherwise the generic argument of <see cref="SerializableType{T}"/>.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Returns the resolved <see cref="System.Type"/>, or <c>null</c>
        /// if the stored assembly-qualified name could not be matched to any loaded assembly.
        /// </summary>
        Type? Type { get; }
    }
}
