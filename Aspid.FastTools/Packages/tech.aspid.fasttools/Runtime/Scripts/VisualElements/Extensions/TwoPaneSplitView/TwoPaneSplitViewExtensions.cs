using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TwoPaneSplitView"/>.
    /// </summary>
    public static class TwoPaneSplitViewExtensions
    {
        /// <summary>
        /// Sets <see cref="TwoPaneSplitView.fixedPaneIndex"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The index of the fixed pane: <c>0</c> or <c>1</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFixedPaneIndex<T>(this T element, int value)
            where T : TwoPaneSplitView
        {
            element.fixedPaneIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TwoPaneSplitView.fixedPaneInitialDimension"/>.
        /// </summary>
        /// <remarks>
        /// Unity uses the value only when it has no saved view data for the split view.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The initial width or height of the fixed pane in pixels.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFixedPaneInitialDimension<T>(this T element, float value)
            where T : TwoPaneSplitView
        {
            element.fixedPaneInitialDimension = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TwoPaneSplitView.orientation"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The orientation of the split to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOrientation<T>(this T element, TwoPaneSplitViewOrientation value)
            where T : TwoPaneSplitView
        {
            element.orientation = value;
            return element;
        }
    }
}
