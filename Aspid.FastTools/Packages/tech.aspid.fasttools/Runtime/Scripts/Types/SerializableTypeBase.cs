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
    /// <para>
    /// Not meant to be derived from outside the package — use <see cref="SerializableType"/> or
    /// <see cref="SerializableMonoScript"/>. Unity serializes the name under the same field for all of them,
    /// so every wrapper shares one serialized layout.
    /// </para>
    /// <para>
    /// A player resolves the type by the stored name only, which managed code stripping does not see: from
    /// Managed Stripping Level Low up, a class referenced only by this name can be removed from the build and
    /// <see cref="Type"/> returns <see langword="null"/>. Keep such classes with <c>[Preserve]</c> or <c>link.xml</c>.
    /// </para>
    /// <para>
    /// A failed lookup is cached until the stored name changes or the object is deserialized again, so an assembly
    /// loaded later is not picked up before that.
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class SerializableTypeBase :
        ISerializableType,
        ISerializationCallbackReceiver
    {
        [Tooltip("The selected type, stored by its assembly-qualified name.")]
        [SerializeField] private string? _assemblyQualifiedName;

        // Marks a failed lookup, so one reference write publishes the resolved state; null means not resolved yet.
        private static readonly Type _unresolved = typeof(Unresolved);

        private Type? _type;

        private protected SerializableTypeBase() { }

        private protected SerializableTypeBase(Type? type)
        {
            _type = type ?? _unresolved;
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
                    // A failed lookup is cached too: a missing assembly makes every Type.GetType call probe for it.
                    var type = _type ??= ResolveType(_assemblyQualifiedName) ?? _unresolved;
                    return ReferenceEquals(type, _unresolved) ? null : type;
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
            ResetResolvedType();
            _assemblyQualifiedName = assemblyQualifiedName;
        }

        private protected virtual Type? ResolveType(string? assemblyQualifiedName) =>
            GetTypeFromAssemblyQualifiedName(assemblyQualifiedName);

        void ISerializationCallbackReceiver.OnAfterDeserialize() =>
            ResetResolvedType();

        void ISerializationCallbackReceiver.OnBeforeSerialize() =>
            OnBeforeSerialize();

        private protected virtual void OnBeforeSerialize() { }

        private void ResetResolvedType() =>
            _type = null;

        private static Type? GetTypeFromAssemblyQualifiedName(string? assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return null;

            return Type.GetType(
                typeName: assemblyQualifiedName,
                throwOnError: false);
        }

        private sealed class Unresolved { }
    }
}
