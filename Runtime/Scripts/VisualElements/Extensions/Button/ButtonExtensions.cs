using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="Button"/>.
    /// </summary>
    public static class ButtonExtensions
    {
        #region Clicked
        /// <summary>
        /// Subscribes to the <see cref="Button.clicked"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="action">The action to invoke when the button is clicked.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClicked<T>(this T element, Action action)
            where T : Button
        {
            element.clicked += action;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="Button.clicked"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="action">The action to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClicked<T>(this T element, Action action)
            where T : Button
        {
            element.clicked -= action;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="Button.clickable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The clickable manipulator to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetClickable<T>(this T element, Clickable value)
            where T : Button
        {
            element.clickable = value;
            return element;
        }
        
        /// <summary>
        /// Replaces <see cref="Button.clickable"/> with a new <see cref="Clickable"/> that invokes <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="action">The action to invoke on click.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetClickable<T>(this T element, Action action)
            where T : Button
        {
            element.clickable = new Clickable(action);
            return element;
        }

        /// <summary>
        /// Sets <see cref="Button.iconImage"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The icon image to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetIconImage<T>(this T element, Background value)
            where T : Button
        {
            element.iconImage = value;
            return element;
        }
    }
}
