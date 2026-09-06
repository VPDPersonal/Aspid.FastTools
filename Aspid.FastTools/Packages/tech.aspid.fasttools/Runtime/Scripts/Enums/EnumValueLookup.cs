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

        public static TValue? Find<TValue>(EnumValue<TValue>[] values, long lookup, bool isFlags, TValue? defaultValue)
        {
            foreach (var value in values)
            {
                if (value.IsResolved && value.NumericKey == lookup)
                    return value.Value;
            }

            if (isFlags)
            {
                foreach (var value in values)
                {
                    if (value.IsResolved && FlagsEquals(lookup, value.NumericKey))
                        return value.Value;
                }
            }

            return defaultValue;
        }
    }
}
