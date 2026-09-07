using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="ListView"/>.
    /// </summary>
    public static class ListViewExtensions
    {
        #region BindItem
        /// <summary>
        /// Sets <see cref="ListView.bindItem"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.bindItem = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="ListView.bindItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddBindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.bindItem += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ListView.bindItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveBindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.bindItem -= value;
            return element;
        }
        #endregion

        #region UnbindItem
        /// <summary>
        /// Sets <see cref="ListView.unbindItem"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnbindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.unbindItem = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="ListView.unbindItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback invoked to release bindings from a list item element.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddUnbindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.unbindItem += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ListView.unbindItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveUnbindItem<T>(this T element, Action<VisualElement, int> value)
            where T : ListView
        {
            element.unbindItem -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="ListView.makeItem"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMakeItem<T>(this T element, Func<VisualElement> value)
            where T : ListView
        {
            element.makeItem = value;
            return element;
        }

        #region DestroyItem
        /// <summary>
        /// Sets <see cref="ListView.destroyItem"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDestroyItem<T>(this T element, Action<VisualElement> value)
            where T : ListView
        {
            element.destroyItem = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="ListView.destroyItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddDestroyItem<T>(this T element, Action<VisualElement> value)
            where T : ListView
        {
            element.destroyItem += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ListView.destroyItem"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveDestroyItem<T>(this T element, Action<VisualElement> value)
            where T : ListView
        {
            element.destroyItem -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="ListView.itemTemplate"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The UXML template to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetItemTemplate<T>(this T element, VisualTreeAsset value)
            where T : ListView
        {
            element.itemTemplate = value;
            return element;
        }
    }
}
