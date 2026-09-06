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
        /// Gets the constraint the stored type must satisfy; <see cref="object"/> when unconstrained.
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the resolved type, or <see langword="null"/> when no type is stored or its stored name cannot be resolved.
        /// </summary>
        public Type? Type { get; }
    }
}
