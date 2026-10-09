using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TextField"/>.
    /// </summary>
    public static class TextFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="TextField.multiline"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the field accepts several lines of text.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMultiline<T>(this T element, bool value)
            where T : TextField
        {
            element.multiline = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.verticalScrollerVisibility"/>.
        /// </summary>
        /// <remarks>
        /// Named with <c>Self</c>: the obsolete instance method <c>SetVerticalScrollerVisibility</c> of Unity returns
        /// <see cref="bool"/> and hides an extension method of that name.
        /// </remarks>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The visibility of the vertical scroller to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVerticalScrollerVisibilitySelf<T>(this T element, ScrollerVisibility value)
            where T : TextField
        {
            element.verticalScrollerVisibility = value;
            return element;
        }
    }
}
