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
        public static T SetText<T>(this T element, string value)
            where T : TextElement
        {
            element.text = value;
            return element;
        }

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
