#nullable enable
using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Unity-serializable wrapper around a <see cref="System.Type"/>, stored by its <c>AssemblyQualifiedName</c>
    /// and resolved lazily on first access.
    /// </summary>
    /// <remarks>
    /// Unity serializes a field by its declared type, so a <see cref="SerializableType{T}"/> assigned from code to a
    /// field declared as <see cref="SerializableType"/> is reloaded unconstrained: the type survives, the constraint
    /// does not.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [SerializeField] private SerializableType _targetType;
    ///
    ///     private void Start()
    ///     {
    ///         Type type = _targetType;  // implicit conversion
    ///         if (type != null)
    ///             Debug.Log(type.FullName);
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public class SerializableType : SerializableTypeBase
    {
        /// <summary>
        /// Creates an empty wrapper.
        /// </summary>
        public SerializableType() { }

        /// <summary>
        /// Creates a wrapper holding <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to store, or <see langword="null"/> for an empty wrapper.</param>
        public SerializableType(Type? type)
            : base(type) { }

        /// <inheritdoc />
        public override Type BaseType => typeof(object);

        /// <summary>
        /// Converts the wrapper to the type it holds.
        /// </summary>
        /// <param name="type">The wrapper to convert.</param>
        /// <returns>
        /// The wrapped type, or <see langword="null"/> when the wrapper is <see langword="null"/> or holds no
        /// resolvable type.
        /// </returns>
        public static implicit operator Type?(SerializableType? type) => type?.Type;
    }

    /// <summary>
    /// <see cref="SerializableType"/> constrained to types assignable to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Base constraint type; the picker offers only types assignable to it.</typeparam>
    /// <example>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [SerializeField] private SerializableType&lt;MonoBehaviour&gt; _behaviorType;
    ///
    ///     private void Start()
    ///     {
    ///         Type type = _behaviorType;  // always a MonoBehaviour subtype or null
    ///         if (type != null)
    ///             gameObject.AddComponent(type);
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public sealed class SerializableType<T> : SerializableType
    {
        /// <summary>
        /// Creates an empty wrapper.
        /// </summary>
        public SerializableType() { }

        /// <summary>
        /// Creates a wrapper holding <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to store, or <see langword="null"/> for an empty wrapper.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not assignable to <typeparamref name="T"/>.</exception>
        public SerializableType(Type? type)
            : base(type)
        {
            if (type is not null && !typeof(T).IsAssignableFrom(type))
                throw new ArgumentException($"{type} is not assignable to {typeof(T)}.", nameof(type));
        }

        /// <inheritdoc />
        public override Type BaseType => typeof(T);
    }
}
