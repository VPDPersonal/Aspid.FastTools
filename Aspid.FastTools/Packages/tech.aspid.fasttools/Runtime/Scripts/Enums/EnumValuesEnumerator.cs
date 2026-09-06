#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    /// <summary>
    /// Allocation-free enumerator over the resolved entries of an <see cref="EnumValues{TValue}"/>
    /// (<typeparamref name="TKey"/> = <see cref="Enum"/>) or an <see cref="EnumValues{TEnum,TValue}"/>
    /// (<typeparamref name="TKey"/> = the enum type). Boxed only when consumed through the
    /// <see cref="IEnumerable{T}"/> interface (e.g. LINQ).
    /// </summary>
    /// <typeparam name="TKey">The key type the entries are yielded as.</typeparam>
    /// <typeparam name="TValue">The type of the value associated with each enum member.</typeparam>
    public struct EnumValuesEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue?>>
    {
        private int _index;
        private readonly EnumValue<TValue>[] _values;

        /// <summary>
        /// The entry at the current position; <see langword="default"/> before the first
        /// <see cref="MoveNext"/> and after the last one.
        /// </summary>
        public KeyValuePair<TKey, TValue?> Current { get; private set; }

        internal EnumValuesEnumerator(EnumValue<TValue>[] values)
        {
            _index = 0;
            _values = values;
            Current = default;
        }

        readonly object IEnumerator.Current => Current;

        /// <summary>
        /// Advances to the next resolved entry, skipping entries whose key could not be parsed.
        /// </summary>
        /// <returns><see langword="true"/> if an entry was found; <see langword="false"/> at the end.</returns>
        public bool MoveNext()
        {
            while (_index < _values.Length)
            {
                var value = _values[_index++];
                if (value.Key is not { } key) continue;

                // Unboxes for a value-type TKey, plain cast for TKey = Enum; never allocates.
                Current = new KeyValuePair<TKey, TValue?>((TKey)(object)key, value.Value);
                return true;
            }

            return false;
        }

        void IEnumerator.Reset()
        {
            _index = 0;
            Current = default;
        }

        readonly void IDisposable.Dispose() { }
    }
}
