#nullable enable
using System.Text;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class StringExtensions
    {
        public static string ToKebabCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var previousWasSeparator = true;
            var builder = new StringBuilder(value.Length + 4);

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (c is '_' or '-' or ' ')
                {
                    previousWasSeparator = true;
                    continue;
                }

                if (previousWasSeparator)
                {
                    if (builder.Length > 0)
                        builder.Append('-');

                    builder.Append(char.ToLowerInvariant(c));
                }
                else if (char.IsUpper(c))
                {
                    var isNewWord = char.IsLower(value[i - 1]) ||
                        char.IsDigit(value[i - 1]) ||
                        (i + 1 < value.Length && char.IsLower(value[i + 1]));

                    if (isNewWord)
                        builder.Append('-');

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }

                previousWasSeparator = false;
            }

            return builder.ToString();
        }
    }
}
