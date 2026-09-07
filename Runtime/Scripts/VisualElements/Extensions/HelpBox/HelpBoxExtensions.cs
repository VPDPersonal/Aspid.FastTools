using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="HelpBox"/>.
    /// </summary>
    public static class HelpBoxExtensions
    {
        /// <summary>
        /// Sets <see cref="HelpBox.text"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetText<T>(this T element, string value)
            where T : HelpBox
        {
            element.text = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="HelpBox.messageType"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The message type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMessageType<T>(this T element, HelpBoxMessageType value)
            where T : HelpBox
        {
            element.messageType = value;
            return element;
        }
    }
}
