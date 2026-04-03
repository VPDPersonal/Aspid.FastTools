using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Adds a child element to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="child">The child element to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChild<T>(this T element, VisualElement child)
            where T : VisualElement
        {
            element.Add(child);
            return element;
        }

        /// <summary>
        /// Adds a span of child elements to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChildren<T>(this T element, Span<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Adds a list of child elements to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChildren<T>(this T element, List<VisualElement> children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Adds an array of child elements to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChildren<T>(this T element, params VisualElement[] children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Adds an enumerable of child elements to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChildren<T>(this T element, IEnumerable<VisualElement> children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Adds a read-only span of child elements to this element's visual hierarchy.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddChildren<T>(this T element, ReadOnlySpan<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.Add(child);

            return element;
        }
    }
}
