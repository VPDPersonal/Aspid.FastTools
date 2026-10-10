using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="Tab"/>.
    /// </summary>
    public static class TabExtensions
    {
        #region Selected
        /// <summary>
        /// Subscribes to the <see cref="Tab.selected"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddSelected<T>(this T element, Action<Tab> value)
            where T : Tab
        {
            element.selected += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="Tab.selected"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveSelected<T>(this T element, Action<Tab> value)
            where T : Tab
        {
            element.selected -= value;
            return element;
        }
        #endregion

        #region Closing
        /// <summary>
        /// Subscribes to the <see cref="Tab.closing"/> event.
        /// </summary>
        /// <remarks>
        /// When several callbacks are subscribed, Unity runs all of them and uses only the result of the last one.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe; it returns <see langword="false"/> to cancel the closing.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClosing<T>(this T element, Func<bool> value)
            where T : Tab
        {
            element.closing += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="Tab.closing"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClosing<T>(this T element, Func<bool> value)
            where T : Tab
        {
            element.closing -= value;
            return element;
        }
        #endregion

        #region Closed
        /// <summary>
        /// Subscribes to the <see cref="Tab.closed"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClosed<T>(this T element, Action<Tab> value)
            where T : Tab
        {
            element.closed += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="Tab.closed"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClosed<T>(this T element, Action<Tab> value)
            where T : Tab
        {
            element.closed -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="Tab.label"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The header text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLabel<T>(this T element, string value)
            where T : Tab
        {
            element.label = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Tab.iconImage"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The header icon to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetIconImage<T>(this T element, Background value)
            where T : Tab
        {
            element.iconImage = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Tab.closeable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the header shows a close button.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetCloseable<T>(this T element, bool value)
            where T : Tab
        {
            element.closeable = value;
            return element;
        }
    }
}
