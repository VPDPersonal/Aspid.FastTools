#nullable enable
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Shared implementation of the serializable <see cref="System.Type"/> wrappers: stores the type by its
    /// assembly-qualified name and resolves it lazily on first access.
    /// </summary>
    /// <remarks>
    /// Not meant to be derived from outside the package — use <see cref="SerializableType"/> or
    /// <see cref="SerializableMonoScript"/>. Unity serializes the name under the same field for all of them,
    /// so every wrapper shares one serialized layout.
    /// </remarks>
    [Serializable]
    public abstract class SerializableTypeBase : ISerializableType, ISerializationCallbackReceiver
    {
        [Tooltip("The selected type, stored by its assembly-qualified name.")]
        [SerializeField] private string? _assemblyQualifiedName;

        private Type? _type;

        private protected SerializableTypeBase() { }

        private protected SerializableTypeBase(Type? type)
        {
            _type = type;
            _assemblyQualifiedName = type?.AssemblyQualifiedName;
        }

        /// <inheritdoc />
        public abstract Type BaseType { get; }

        /// <summary>
        /// Gets the stored assembly-qualified type name, or an empty string when no type is stored.
        /// </summary>
        /// <remarks>
        /// Kept even when it no longer resolves, so the Inspector can show what the field used to point at.
        /// </remarks>
        public string AssemblyQualifiedName => _assemblyQualifiedName ?? string.Empty;

        /// <inheritdoc />
        public Type? Type
        {
            get
            {
#if !ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED
                using (this.Marker())
#endif
                {
                    return _type ??= GetTypeFromAssemblyQualifiedName(_assemblyQualifiedName);
                }
            }
        }

        private protected string? StoredAssemblyQualifiedName => _assemblyQualifiedName;

        /// <summary>
        /// Returns the short name of the resolved type, the stored name when it cannot be resolved,
        /// or an empty string when no type is stored.
        /// </summary>
        public override string ToString() =>
            Type?.Name ?? AssemblyQualifiedName;

        private protected void SetAssemblyQualifiedName(string? assemblyQualifiedName)
        {
            _assemblyQualifiedName = assemblyQualifiedName;
            _type = null;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() =>
            _type = null;

        void ISerializationCallbackReceiver.OnBeforeSerialize() =>
            OnBeforeSerialize();

        private protected virtual void OnBeforeSerialize() { }

        private static Type? GetTypeFromAssemblyQualifiedName(string? assemblyQualifiedName) => string.IsNullOrWhiteSpace(assemblyQualifiedName)
            ? null
            : Type.GetType(assemblyQualifiedName, throwOnError: false);
    }
}
