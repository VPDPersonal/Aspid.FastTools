#nullable enable

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    internal static class EnumValueLookup
    {
        // Zero is a subset of everything, so it only matches another zero.
        public static bool FlagsEquals(long value1, long value2) =>
            (value1 & value2) == value2 &&
            (value1 == 0L) == (value2 == 0L);

        public static bool TryFind<TValue>(EnumValue<TValue>[] values, long lookup, bool isFlags, out TValue? value)
        {
            foreach (var entry in values)
            {
                if (entry.IsResolved && entry.NumericKey == lookup)
                {
                    value = entry.Value;
                    return true;
                }
            }

            if (isFlags)
            {
                foreach (var entry in values)
                {
                    if (entry.IsResolved && FlagsEquals(lookup, entry.NumericKey))
                    {
                        value = entry.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        public static TValue? Find<TValue>(EnumValue<TValue>[] values, long lookup, bool isFlags, TValue? defaultValue) =>
            TryFind(values, lookup, isFlags, out var value) ? value : defaultValue;
    }
}
