#nullable enable
using System;
using UnityEngine;
using System.Collections;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    /// <summary>
    /// Serializable table that maps members of an enum chosen in the Inspector to <typeparamref name="TValue"/> values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For a <c>[Flags]</c> enum, an entry matches when the lookup value contains all its bits, and zero matches only
    /// zero. An exact key wins; otherwise, the first match in serialized order.
    /// </para>
    /// <para>
    /// A player finds the enum by its stored assembly-qualified name. From Managed Stripping Level Medium up, keep an
    /// enum used only through this table with <c>[Preserve]</c> or <c>link.xml</c>, or every lookup returns the
    /// default value.
    /// </para>
    /// <para>
    /// Lookups and iteration are thread-safe, the first access included, unless Unity deserializes the table at the
    /// same time.
    /// </para>
    /// </remarks>
    /// <typeparam name="TValue">The type of the mapped values.</typeparam>
    /// <example>
    /// <code>
    /// [SerializeField] private EnumValues&lt;Color&gt; _damageColors;
    /// </code>
    /// </example>
    /// <seealso cref="EnumValues{TEnum, TValue}"/>
    [Serializable]
    public sealed class EnumValues<TValue> :
        IReadOnlyCollection<KeyValuePair<Enum, TValue?>>,
        ISerializationCallbackReceiver
    {
        [Tooltip("The enum whose members the entries are keyed by.")]
        [TypeSelector(typeof(Enum), Allow = TypeAllow.None, Required = true)]
        [SerializeField] private string _enumType = string.Empty;

        [Tooltip("The value returned when no entry matches the lookup key.")]
        [SerializeField] private TValue? _defaultValue;

        [Tooltip("The configured entries, searched in this order.")]
        [SerializeField] private EnumValue<TValue>[] _values = Array.Empty<EnumValue<TValue>>();

        private Type? _type;
        private int _count;
        private bool _isFlags;

        // Volatile: a thread that sees the flag must also see the data written before it.
        private volatile bool _isInitialized;

        /// <summary>
        /// Gets the number of entries with a resolved key, duplicates included; the default value is not counted.
        /// </summary>
        public int Count
        {
            get
            {
                Initialize();
                return _count;
            }
        }

        private void Initialize()
        {
            if (_isInitialized) return;

#if !ASPID_FASTTOOLS_UNITY_PROFILER_DISABLED
            using (this.Marker())
#endif
            {
                if (string.IsNullOrWhiteSpace(_enumType))
                {
                    Debug.LogWarning($"[{nameof(EnumValues<TValue>)}] [{nameof(Initialize)}] " +
                        "No enum type configured — GetValue will always return the default value.");

                    Degrade();
                    return;
                }

                if (Type.GetType(_enumType, throwOnError: false) is not { } type)
                {
                    Debug.LogError($"[{nameof(EnumValues<TValue>)}] [{nameof(Initialize)}] " +
                        $"Couldn't resolve enum type '{_enumType}' — GetValue will always return the default value.");

                    Degrade();
                    return;
                }

                if (!type.IsEnum)
                {
                    Debug.LogError($"[{nameof(EnumValues<TValue>)}] [{nameof(Initialize)}] " +
                        $"Type '{_enumType}' is not an enum — GetValue will always return the default value.");

                    Degrade();
                    return;
                }

                var count = 0;
                foreach (var value in _values)
                {
                    value.Initialize(type);
                    if (value.IsResolved) count++;
                }

                _type = type;
                _count = count;
                _isFlags = EnumInfo.IsFlags(type);
                _isInitialized = true;
            }
        }

        private void Degrade()
        {
            // The type may have been configured before and cleared since; keys resolved back
            // then would otherwise keep matching and being enumerated.
            foreach (var value in _values)
                value.Reset();

            _type = null;
            _count = 0;
            _isFlags = false;
            _isInitialized = true;
        }

        /// <summary>
        /// Returns the value mapped to <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">
        /// The enum member to look up; <see langword="null"/> or a member of another enum never matches.
        /// </param>
        /// <returns>The mapped value; otherwise, the default value.</returns>
        public TValue? GetValue(Enum enumValue)
        {
            Initialize();

            // Another enum type could collide numerically.
            if (enumValue is null || _type is null || enumValue.GetType() != _type)
                return _defaultValue;

            var lookup = EnumInfo.ToInt64(enumValue);
            return EnumValueLookup.Find(_values, lookup, _isFlags, _defaultValue);
        }

        /// <summary>
        /// Returns the value mapped to <paramref name="enumValue"/> without boxing it.
        /// </summary>
        /// <typeparam name="TEnum">The configured enum; another enum never matches.</typeparam>
        /// <param name="enumValue">The enum member to look up.</param>
        /// <returns>The mapped value; otherwise, the default value.</returns>
        public TValue? GetValue<TEnum>(TEnum enumValue)
            where TEnum : struct, Enum
        {
            Initialize();

            // Another enum type could collide numerically.
            if (_type != typeof(TEnum))
                return _defaultValue;

            var lookup = EnumInfo<TEnum>.ToInt64(enumValue);
            return EnumValueLookup.Find(_values, lookup, _isFlags, _defaultValue);
        }

        /// <summary>
        /// Looks up the value mapped to <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">
        /// The enum member to look up; <see langword="null"/> or a member of another enum never matches.
        /// </param>
        /// <param name="value">The mapped value; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if an entry matches; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(Enum enumValue, out TValue? value)
        {
            Initialize();

            // Another enum type could collide numerically.
            if (enumValue is not null && _type is not null && enumValue.GetType() == _type &&
                EnumValueLookup.TryFind(_values, EnumInfo.ToInt64(enumValue), _isFlags, out value))
                return true;

            value = _defaultValue;
            return false;
        }

        /// <summary>
        /// Looks up the value mapped to <paramref name="enumValue"/> without boxing it.
        /// </summary>
        /// <typeparam name="TEnum">The configured enum; another enum never matches.</typeparam>
        /// <param name="enumValue">The enum member to look up.</param>
        /// <param name="value">The mapped value; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if an entry matches; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue<TEnum>(TEnum enumValue, out TValue? value)
            where TEnum : struct, Enum
        {
            Initialize();

            // Another enum type could collide numerically.
            if (_type == typeof(TEnum) &&
                EnumValueLookup.TryFind(_values, EnumInfo<TEnum>.ToInt64(enumValue), _isFlags, out value))
                return true;

            value = _defaultValue;
            return false;
        }

        /// <summary>
        /// Determines whether the lookup value <paramref name="enumValue1"/> matches the entry key
        /// <paramref name="enumValue2"/>.
        /// </summary>
        /// <param name="enumValue1">The lookup value.</param>
        /// <param name="enumValue2">The entry key.</param>
        /// <returns>
        /// <see langword="true"/> if the values are equal or, for a <c>[Flags]</c> enum, <paramref name="enumValue1"/>
        /// contains all bits of <paramref name="enumValue2"/> and zero matches only zero; otherwise, including for
        /// <see langword="null"/> or another enum, <see langword="false"/>.
        /// </returns>
        public bool Equals(Enum enumValue1, Enum enumValue2)
        {
            Initialize();

            if (enumValue1 is null || enumValue2 is null || _type is null)
                return false;

            if (enumValue1.GetType() != _type || enumValue2.GetType() != _type)
                return false;

            var value1 = EnumInfo.ToInt64(enumValue1);
            var value2 = EnumInfo.ToInt64(enumValue2);

            return _isFlags ? EnumValueLookup.FlagsEquals(value1, value2) : value1 == value2;
        }

        /// <summary>
        /// Returns an enumerator over the entries with a resolved key in serialized order, without the default value.
        /// </summary>
        public EnumValuesEnumerator<Enum, TValue> GetEnumerator()
        {
            Initialize();
            return new EnumValuesEnumerator<Enum, TValue>(_values);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        IEnumerator<KeyValuePair<Enum, TValue?>> IEnumerable<KeyValuePair<Enum, TValue?>>.GetEnumerator() =>
            GetEnumerator();

        // Entries added in the Inspector would otherwise stay unresolved.
        void ISerializationCallbackReceiver.OnAfterDeserialize() =>
            _isInitialized = false;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }

    /// <summary>
    /// Serializable table that maps members of <typeparamref name="TEnum"/> to <typeparamref name="TValue"/> values.
    /// </summary>
    /// <remarks>
    /// Lookup rules, thread safety and the serialized layout match <see cref="EnumValues{TValue}"/>: a field can switch
    /// between the two types without losing entries if the enum is the same.
    /// </remarks>
    /// <typeparam name="TEnum">The enum the entries are keyed by.</typeparam>
    /// <typeparam name="TValue">The type of the mapped values.</typeparam>
    /// <example>
    /// <code>
    /// [SerializeField] private EnumValues&lt;DamageType, Color&gt; _damageColors;
    /// </code>
    /// </example>
    [Serializable]
    public sealed class EnumValues<TEnum, TValue> :
        IReadOnlyCollection<KeyValuePair<TEnum, TValue?>>,
        ISerializationCallbackReceiver
        where TEnum : struct, Enum
    {
#if UNITY_EDITOR
        // Keeps the layout compatible with EnumValues<TValue> and feeds the editor drawers;
        // never read at runtime, so player builds strip it.
        [Tooltip("The enum whose members the entries are keyed by, filled in from TEnum.")]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string? _enumType;
#endif

        [Tooltip("The value returned when no entry matches the lookup key.")]
        [SerializeField] private TValue? _defaultValue;

        [Tooltip("The configured entries, searched in this order.")]
        [SerializeField] private EnumValue<TValue>[] _values = Array.Empty<EnumValue<TValue>>();

        private int _count;

        // Volatile: a thread that sees the flag must also see the data written before it.
        private volatile bool _isInitialized;

        /// <inheritdoc cref="EnumValues{TValue}.Count"/>
        public int Count
        {
            get
            {
                Initialize();
                return _count;
            }
        }

        private void Initialize()
        {
            if (_isInitialized) return;

#if !ASPID_FASTTOOLS_UNITY_PROFILER_DISABLED
            using (this.Marker())
#endif
            {
                var count = 0;
                foreach (var value in _values)
                {
                    value.Initialize(typeof(TEnum));
                    if (value.IsResolved) count++;
                }

                _count = count;
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Returns the value mapped to <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">The enum member to look up.</param>
        /// <returns>The mapped value; otherwise, the default value.</returns>
        public TValue? GetValue(TEnum enumValue)
        {
            Initialize();

            var lookup = EnumInfo<TEnum>.ToInt64(enumValue);
            return EnumValueLookup.Find(_values, lookup, EnumInfo<TEnum>.IsFlags, _defaultValue);
        }

        /// <summary>
        /// Looks up the value mapped to <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">The enum member to look up.</param>
        /// <param name="value">The mapped value; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if an entry matches; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TEnum enumValue, out TValue? value)
        {
            Initialize();

            var lookup = EnumInfo<TEnum>.ToInt64(enumValue);
            if (EnumValueLookup.TryFind(_values, lookup, EnumInfo<TEnum>.IsFlags, out value))
                return true;

            value = _defaultValue;
            return false;
        }

        /// <summary>
        /// Determines whether the lookup value <paramref name="enumValue1"/> matches the entry key
        /// <paramref name="enumValue2"/>.
        /// </summary>
        /// <param name="enumValue1">The lookup value.</param>
        /// <param name="enumValue2">The entry key.</param>
        /// <returns>
        /// <see langword="true"/> if the values are equal or, for a <c>[Flags]</c> enum, <paramref name="enumValue1"/>
        /// contains all bits of <paramref name="enumValue2"/> and zero matches only zero; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(TEnum enumValue1, TEnum enumValue2)
        {
            var value1 = EnumInfo<TEnum>.ToInt64(enumValue1);
            var value2 = EnumInfo<TEnum>.ToInt64(enumValue2);

            return EnumInfo<TEnum>.IsFlags
                ? EnumValueLookup.FlagsEquals(value1, value2)
                : value1 == value2;
        }

        /// <inheritdoc cref="EnumValues{TValue}.GetEnumerator"/>
        public EnumValuesEnumerator<TEnum, TValue> GetEnumerator()
        {
            Initialize();
            return new EnumValuesEnumerator<TEnum, TValue>(_values);
        }

        IEnumerator<KeyValuePair<TEnum, TValue?>> IEnumerable<KeyValuePair<TEnum, TValue?>>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        void ISerializationCallbackReceiver.OnAfterDeserialize() =>
            _isInitialized = false;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            _enumType = typeof(TEnum).AssemblyQualifiedName;
#endif
        }
    }
}
