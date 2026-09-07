using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseTreeView"/>.
    /// </summary>
    public static class BaseTreeViewExtensions
    {
        #region ItemExpandedChanged
        /// <summary>
        /// Subscribes to the <see cref="BaseTreeView.itemExpandedChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemExpandedChanged<T>(this T element, Action<TreeViewExpansionChangedArgs> value)
            where T : BaseTreeView
        {
            element.itemExpandedChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseTreeView.itemExpandedChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemExpandedChanged<T>(this T element, Action<TreeViewExpansionChangedArgs> value)
            where T : BaseTreeView
        {
            element.itemExpandedChanged -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="BaseTreeView.autoExpand"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, items are expanded automatically.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAutoExpand<T>(this T element, bool value)
            where T : BaseTreeView
        {
            element.autoExpand = value;
            return element;
        }
    }
}
