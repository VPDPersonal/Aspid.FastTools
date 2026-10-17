#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    /// <summary>
    /// Allocation-free enumerator over the entries of <see cref="EnumValues{TValue}"/> and
    /// <see cref="EnumValues{TEnum, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">
    /// <see cref="Enum"/> for <see cref="EnumValues{TValue}"/>; the enum for <see cref="EnumValues{TEnum, TValue}"/>.
    /// </typeparam>
    /// <typeparam name="TValue">The type of the mapped values.</typeparam>
    public struct EnumValuesEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue?>>
    {
        private int _index;
        private readonly EnumValue<TValue>[] _values;

        /// <summary>
        /// Gets the entry at the current position; <see langword="default"/> before the first <see cref="MoveNext"/>
        /// and after the end.
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
        /// Advances to the next entry with a resolved key.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator advanced; otherwise, <see langword="false"/>.</returns>
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

            Current = default;
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
