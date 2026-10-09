using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="ScrollView"/>.
    /// </summary>
    public static class ScrollViewExtensions
    {
        /// <summary>
        /// Sets <see cref="ScrollView.mode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The scrolling directions to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMode<T>(this T element, ScrollViewMode value)
            where T : ScrollView
        {
            element.mode = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.horizontalScrollerVisibility"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The visibility of the horizontal scroller to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHorizontalScrollerVisibility<T>(this T element, ScrollerVisibility value)
            where T : ScrollView
        {
            element.horizontalScrollerVisibility = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.verticalScrollerVisibility"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The visibility of the vertical scroller to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVerticalScrollerVisibility<T>(this T element, ScrollerVisibility value)
            where T : ScrollView
        {
            element.verticalScrollerVisibility = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.scrollOffset"/>.
        /// </summary>
        /// <remarks>
        /// The scrollers limit the offset to the scrollable range, so add the content and wait for the layout first.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The scroll offset in pixels.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetScrollOffset<T>(this T element, Vector2 value)
            where T : ScrollView
        {
            element.scrollOffset = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.horizontalPageSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal page size in pixels; <c>-1</c> derives it from the scroller.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHorizontalPageSize<T>(this T element, float value)
            where T : ScrollView
        {
            element.horizontalPageSize = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.verticalPageSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical page size in pixels; <c>-1</c> derives it from the scroller.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVerticalPageSize<T>(this T element, float value)
            where T : ScrollView
        {
            element.verticalPageSize = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.mouseWheelScrollSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The distance in pixels that one mouse wheel step scrolls.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMouseWheelScrollSize<T>(this T element, float value)
            where T : ScrollView
        {
            element.mouseWheelScrollSize = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.scrollDecelerationRate"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The deceleration rate of inertial scrolling, clamped to 0 or more.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetScrollDecelerationRate<T>(this T element, float value)
            where T : ScrollView
        {
            element.scrollDecelerationRate = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.elasticity"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The elasticity of the bounce at the content edges, clamped to 0 or more.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetElasticity<T>(this T element, float value)
            where T : ScrollView
        {
            element.elasticity = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.elasticAnimationIntervalMs"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The minimum interval between elastic and inertia animation updates in milliseconds.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetElasticAnimationIntervalMs<T>(this T element, long value)
            where T : ScrollView
        {
            element.elasticAnimationIntervalMs = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.touchScrollBehavior"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">How touch scrolling behaves at the content edges.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTouchScrollBehavior<T>(this T element, ScrollView.TouchScrollBehavior value)
            where T : ScrollView
        {
            element.touchScrollBehavior = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ScrollView.nestedInteractionKind"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">How scrolling passes to a parent scroll view when this one reaches its limit.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetNestedInteractionKind<T>(this T element, ScrollView.NestedInteractionKind value)
            where T : ScrollView
        {
            element.nestedInteractionKind = value;
            return element;
        }
    }
}
