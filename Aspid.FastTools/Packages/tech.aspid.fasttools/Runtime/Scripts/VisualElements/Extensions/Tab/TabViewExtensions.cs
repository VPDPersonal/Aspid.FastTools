using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TabView"/>.
    /// </summary>
    public static class TabViewExtensions
    {
        #region ActiveTabChanged
        /// <summary>
        /// Subscribes to the <see cref="TabView.activeTabChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe; it receives the previous and the new active tab.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddActiveTabChanged<T>(this T element, Action<Tab, Tab> value)
            where T : TabView
        {
            element.activeTabChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="TabView.activeTabChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveActiveTabChanged<T>(this T element, Action<Tab, Tab> value)
            where T : TabView
        {
            element.activeTabChanged -= value;
            return element;
        }
        #endregion

        #region TabReordered
        /// <summary>
        /// Subscribes to the <see cref="TabView.tabReordered"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe; it receives the old and the new tab index.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddTabReordered<T>(this T element, Action<int, int> value)
            where T : TabView
        {
            element.tabReordered += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="TabView.tabReordered"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveTabReordered<T>(this T element, Action<int, int> value)
            where T : TabView
        {
            element.tabReordered -= value;
            return element;
        }
        #endregion

        #region TabClosed
        /// <summary>
        /// Subscribes to the <see cref="TabView.tabClosed"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe; it receives the closed tab and its former index.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddTabClosed<T>(this T element, Action<Tab, int> value)
            where T : TabView
        {
            element.tabClosed += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="TabView.tabClosed"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveTabClosed<T>(this T element, Action<Tab, int> value)
            where T : TabView
        {
            element.tabClosed -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="TabView.activeTab"/>.
        /// </summary>
        /// <remarks>
        /// Unity throws when <paramref name="value"/> is not a tab of this view, so add the tabs first.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The tab to activate.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetActiveTab<T>(this T element, Tab value)
            where T : TabView
        {
            element.activeTab = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TabView.selectedTabIndex"/>.
        /// </summary>
        /// <remarks>
        /// Unity ignores an index outside the tab range, so add the tabs first.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The index of the tab to activate.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectedTabIndex<T>(this T element, int value)
            where T : TabView
        {
            element.selectedTabIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TabView.reorderable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, tabs can be reordered by dragging their headers.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetReorderable<T>(this T element, bool value)
            where T : TabView
        {
            element.reorderable = value;
            return element;
        }
    }
}
