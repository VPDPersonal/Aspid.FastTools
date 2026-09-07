using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="MultiColumnTreeView"/>.
    /// </summary>
    public static class MultiColumnTreeViewExtensions
    {
        /// <summary>
        /// Sets <see cref="MultiColumnTreeView.sortingMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The sorting mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSortingMode<T>(this T element, ColumnSortingMode value)
            where T : MultiColumnTreeView
        {
            element.sortingMode = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="MultiColumnTreeView.columnSortingChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddColumnSortingChanged<T>(this T element, Action callback)
            where T : MultiColumnTreeView
        {
            element.columnSortingChanged += callback;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="MultiColumnTreeView.columnSortingChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveColumnSortingChanged<T>(this T element, Action callback)
            where T : MultiColumnTreeView
        {
            element.columnSortingChanged -= callback;
            return element;
        }
    }
}
