#nullable enable
using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Defines the common contract of the serializable <see cref="System.Type"/> wrappers.
    /// </summary>
    public interface ISerializableType
    {
        /// <summary>
        /// Gets the base type that types offered by the picker are assignable to; <see cref="object"/> when unconstrained.
        /// </summary>
        /// <remarks>
        /// A loaded type is not checked against it: a name stored before the constraint changed resolves as is.
        /// </remarks>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the resolved type, or <see langword="null"/> when no type is stored or its stored name cannot be resolved.
        /// </summary>
        public Type? Type { get; }
    }
}
