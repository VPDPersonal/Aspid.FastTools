#nullable enable
using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Unity-serializable wrapper around a <see cref="System.Type"/>, stored by its assembly-qualified name.
    /// </summary>
    /// <example>
    /// <code>
    /// [SerializeField] private SerializableType _targetType;
    ///
    /// Type type = _targetType; // implicit conversion
    /// </code>
    /// </example>
    [Serializable]
    public class SerializableType : SerializableTypeBase
    {
        private protected SerializableType() { }

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
    /// <remarks>
    /// Assigned to a field declared as <see cref="SerializableType"/>, it is reloaded without the constraint.
    /// </remarks>
    /// <typeparam name="T">The base type that the picker and the constructor require.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// [SerializeField] private SerializableType<MonoBehaviour> _behaviorType;
    /// ]]></code>
    /// </example>
    [Serializable]
    public sealed class SerializableType<T> : SerializableType
    {
        private SerializableType() { }

        /// <summary>
        /// Creates a wrapper holding <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to store, or <see langword="null"/> for an empty wrapper.</param>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not assignable to <typeparamref name="T"/>.</exception>
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
