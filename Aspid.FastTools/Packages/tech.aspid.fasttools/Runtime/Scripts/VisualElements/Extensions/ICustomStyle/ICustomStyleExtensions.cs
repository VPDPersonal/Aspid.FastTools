using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="ICustomStyle"/> that bridge USS string-typed custom
    /// properties to strongly-typed C# values.
    /// </summary>
    public static class ICustomStyleExtensions
    {
        /// <summary>
        /// Resolves a <see cref="CustomStyleProperty{T}"/> whose USS value is a string and parses
        /// it as the enum <typeparamref name="T"/>. Parsing is case-insensitive.
        /// </summary>
        /// <remarks>
        /// A number is accepted only when it is a defined value, and a comma-separated list only for a
        /// <see cref="FlagsAttribute"/> enum, which accepts names only.
        /// </remarks>
        /// <typeparam name="T">The enum type to parse the USS value as.</typeparam>
        /// <param name="style">The resolved custom-style container, typically obtained from
        /// <see cref="CustomStyleResolvedEvent.customStyle"/>.</param>
        /// <param name="property">The custom style property whose string value should be parsed.</param>
        /// <param name="value">When this method returns <see langword="true"/>, the parsed enum
        /// value; otherwise <see langword="default"/>.</param>
        /// <returns><see langword="true"/> if the property was resolved and parsed to a defined value
        /// of <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetByEnum<T>(this ICustomStyle style, CustomStyleProperty<string> property, out T value)
            where T : struct, Enum
        {
            value = default;

            return style.TryGetValue(property, out var propertyValue)
                && TryParseDefined(propertyValue, out value);
        }

        // Enum.TryParse accepts any number and any comma list, so both are rejected afterwards.
        private static bool TryParseDefined<T>(string text, out T value)
            where T : struct, Enum
        {
            if (!Enum.TryParse(text, ignoreCase: true, out value))
                return false;

            var isDefined = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false)
                ? !HasNumericToken(text)
                : !text.Contains(value: ',') && Enum.IsDefined(typeof(T), value);

            if (isDefined) return true;

            value = default;
            return false;
        }

        // A flags value built from names cannot set a bit that no name has, a number can.
        private static bool HasNumericToken(string text)
        {
            var isTokenStart = true;

            foreach (var symbol in text)
            {
                if (symbol is ',')
                {
                    isTokenStart = true;
                    continue;
                }

                if (char.IsWhiteSpace(symbol)) continue;
                if (isTokenStart && (char.IsDigit(symbol) || symbol is '-' or '+')) return true;

                isTokenStart = false;
            }

            return false;
        }
    }
}
