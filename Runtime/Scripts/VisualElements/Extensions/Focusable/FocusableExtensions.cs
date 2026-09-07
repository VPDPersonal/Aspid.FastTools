using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="Focusable"/>.
    /// </summary>
    public static class FocusableExtensions
    {
        /// <summary>
        /// Removes focus from the element via <see cref="Focusable.Blur"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T BlurSelf<T>(this T element)
            where T : Focusable
        {
            element.Blur();
            return element;
        }

        /// <summary>
        /// Gives focus to the element via <see cref="Focusable.Focus"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T FocusSelf<T>(this T element)
            where T : Focusable
        {
            element.Focus();
            return element;
        }

        /// <summary>
        /// Returns whether the element currently has keyboard focus.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns><see langword="true"/> if the element holds keyboard focus; otherwise, <see langword="false"/>.</returns>
        public static bool IsFocused(this Focusable element) =>
            element.focusController?.focusedElement == element;

        /// <summary>
        /// Sets <see cref="Focusable.tabIndex"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The tab index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTabIndex<T>(this T element, int value)
            where T : Focusable
        {
            element.tabIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Focusable.focusable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the element can receive focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFocusable<T>(this T element, bool value)
            where T : Focusable
        {
            element.focusable = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Focusable.delegatesFocus"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, focus is delegated to the children.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDelegatesFocus<T>(this T element, bool value)
            where T : Focusable
        {
            element.delegatesFocus = value;
            return element;
        }
    }
}
