#nullable enable
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Shared base of the serializable <see cref="System.Type"/> wrappers: stores the type by its assembly-qualified
    /// name and resolves it lazily.
    /// </summary>
    /// <remarks>
    /// A player resolves only the stored name, which managed code stripping does not see: from Managed Stripping
    /// Level Medium up, keep a class referenced only here with <c>[Preserve]</c> or <c>link.xml</c>. A failed lookup
    /// is cached until the name changes or the object is deserialized again, so an assembly loaded later is missed.
    /// </remarks>
    [Serializable]
    public abstract class SerializableTypeBase :
        ISerializableType,
        IEquatable<SerializableTypeBase>,
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
        public string AssemblyQualifiedName => _assemblyQualifiedName ?? string.Empty;

        /// <inheritdoc />
        public Type? Type
        {
            get
            {
                var type = _type;
                if (type is null)
                {
                    // Only the lookup is measured: a marker on the cached hot path would flood the Profiler.
#if !ASPID_FASTTOOLS_UNITY_PROFILER_DISABLED
                    using (this.Marker())
#endif
                    {
                        // A failed lookup is cached too: a missing assembly makes every Type.GetType call probe for it.
                        type = ResolveType(_assemblyQualifiedName, out var cacheable) ?? _unresolved;
                        if (cacheable) _type = type;
                    }
                }

                return ReferenceEquals(type, _unresolved) ? null : type;
            }
        }

        /// <summary>
        /// Gets a value indicating whether no type is stored.
        /// </summary>
        /// <remarks>
        /// On an object created in code the field itself stays <see langword="null"/> until Unity serializes it:
        /// test an unset field with <c>field is null || field.IsEmpty</c>.
        /// </remarks>
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(_assemblyQualifiedName) && Type is null;

        /// <summary>
        /// Gets a value indicating whether a type name is stored but does not resolve, for example after the class
        /// was renamed, moved or stripped from a player build.
        /// </summary>
        public bool IsMissing =>
            !string.IsNullOrWhiteSpace(_assemblyQualifiedName) && Type is null;

        private protected string? StoredAssemblyQualifiedName => _assemblyQualifiedName;

        /// <summary>
        /// Returns the short name of the resolved type, the stored name when it cannot be resolved,
        /// or an empty string when no type is stored.
        /// </summary>
        public override string ToString() =>
            Type?.Name ?? AssemblyQualifiedName;

        /// <summary>
        /// Determines whether <paramref name="other"/> stores the same assembly-qualified name, whatever its wrapper
        /// class or constraint.
        /// </summary>
        /// <remarks>
        /// <c>==</c> still compares references.
        /// </remarks>
        /// <param name="other">The wrapper to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> is not <see langword="null"/> and stores the same
        /// name, also when neither stores a type; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(SerializableTypeBase? other) =>
            other is not null && AssemblyQualifiedName == other.AssemblyQualifiedName;

        /// <summary>
        /// Determines whether <paramref name="obj"/> is a wrapper that stores the same assembly-qualified name.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is such a wrapper; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj) =>
            Equals(obj as SerializableTypeBase);

        /// <summary>
        /// Returns a hash code of the stored assembly-qualified name.
        /// </summary>
        /// <remarks>
        /// Follows the stored name: do not change a wrapper used as a key, also through Unity serialization.
        /// </remarks>
        public override int GetHashCode() =>
            AssemblyQualifiedName.GetHashCode();

        private protected void SetAssemblyQualifiedName(string? assemblyQualifiedName)
        {
            ResetResolvedType();
            _assemblyQualifiedName = assemblyQualifiedName;
        }

        // cacheable is false when the calling thread could not finish the lookup, so a later read tries again.
        private protected virtual Type? ResolveType(string? assemblyQualifiedName, out bool cacheable)
        {
            cacheable = true;
            return GetTypeFromAssemblyQualifiedName(assemblyQualifiedName);
        }

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
