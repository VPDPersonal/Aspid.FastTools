using System;
using System.Text;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TextElement"/>.
    /// </summary>
    public static class TextElementExtensions
    {
        /// <summary>
        /// Sets <see cref="TextElement.text"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, string value)
            where T : TextElement
        {
            element.text = value;
            return element;
        }

#if UNITY_6000_6_OR_NEWER
        /// <summary>
        /// Sets the text to a number via <see cref="TextElement.SetText(int)"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The number to show.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, int value)
            where T : TextElement
        {
            if (element.panel == null) element.text = value.ToString();
            else element.SetText(value);

            return element;
        }

        /// <summary>
        /// Sets the text to a number via <see cref="TextElement.SetText(ReadOnlySpan{char})"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The number to show.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, long value)
            where T : TextElement
        {
            if (element.panel == null)
            {
                element.text = value.ToString();
                return element;
            }

            Span<char> buffer = stackalloc char[20];
            if (value.TryFormat(buffer, out var written)) element.SetText(buffer.Slice(start: 0, length: written));
            else element.text = value.ToString();

            return element;
        }

        /// <summary>
        /// Sets the text to a number via <see cref="TextElement.SetText(ReadOnlySpan{char})"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The number to show.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, ulong value)
            where T : TextElement
        {
            if (element.panel == null)
            {
                element.text = value.ToString();
                return element;
            }

            Span<char> buffer = stackalloc char[20];
            if (value.TryFormat(buffer, out var written)) element.SetText(buffer.Slice(start: 0, length: written));
            else element.text = value.ToString();

            return element;
        }

        /// <summary>
        /// Sets the text to a number via <see cref="TextElement.SetText(float, string)"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The number to show.</param>
        /// <param name="format">A standard or custom numeric format string; <see langword="null"/> for the default format.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, float value, string format = null)
            where T : TextElement
        {
            if (element.panel == null) element.text = value.ToString(format);
            else element.SetText(value, format);

            return element;
        }

        /// <summary>
        /// Sets the text from a character span via <see cref="TextElement.SetText(ReadOnlySpan{char})"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The characters to copy.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, ReadOnlySpan<char> value)
            where T : TextElement
        {
            if (element.panel == null) element.text = new string(value);
            else element.SetText(value);

            return element;
        }

        /// <summary>
        /// Sets the text from a <see cref="StringBuilder"/> via <see cref="TextElement.SetText(StringBuilder)"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The builder whose content to copy.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, StringBuilder value)
            where T : TextElement
        {
            if (element.panel == null) element.text = value?.ToString() ?? string.Empty;
            else element.SetText(value);

            return element;
        }

        /// <summary>
        /// Sets the text from a slice of a character array via <see cref="TextElement.SetText(char[], int, int)"/>, without allocating a string.
        /// </summary>
        /// <remarks>
        /// Allocates a string while the element is not attached to a panel.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The array to copy from.</param>
        /// <param name="start">The index of the first character to copy.</param>
        /// <param name="length">The number of characters to copy.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextSelf<T>(this T element, char[] value, int start, int length)
            where T : TextElement
        {
            if (element.panel == null) element.text = new string(value, startIndex: start, length: length);
            else element.SetText(value, start, length);

            return element;
        }
#endif

        /// <summary>
        /// Sets <see cref="TextElement.enableRichText"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, rich text tags are parsed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetEnableRichText<T>(this T element, bool value)
            where T : TextElement
        {
            element.enableRichText = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextElement.emojiFallbackSupport"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the global emoji fallback list is searched first.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetEmojiFallbackSupport<T>(this T element, bool value)
            where T : TextElement
        {
            element.emojiFallbackSupport = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextElement.parseEscapeSequences"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, escape sequences such as <c>\n</c> are parsed; otherwise, they are shown as raw text.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetParseEscapeSequences<T>(this T element, bool value)
            where T : TextElement
        {
            element.parseEscapeSequences = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextElement.displayTooltipWhenElided"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a tooltip shows the full text when it is elided.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDisplayTooltipWhenElided<T>(this T element, bool value)
            where T : TextElement
        {
            element.displayTooltipWhenElided = value;
            return element;
        }
    }
}
