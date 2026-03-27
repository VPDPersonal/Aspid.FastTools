using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Sets the name of the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetName<T>(this T element, string name)
            where T : VisualElement
        {
            element.name = name;
            return element;
        }

        /// <summary>
        /// Sets the visibility of the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetVisible<T>(this T element, bool visible)
            where T : VisualElement
        {
            element.visible = visible;
            return element;
        }

        /// <summary>
        /// Sets the tooltip text of the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTooltip<T>(this T element, string tooltip)
            where T : VisualElement
        {
            element.tooltip = tooltip;
            return element;
        }

        /// <summary>
        /// Adds a child element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddChild<T>(this T element, VisualElement child)
            where T : VisualElement
        {
            element.Add(child);
            return element;
        }

        /// <summary>
        /// Adds a child element if it is not <see langword="null"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddChildIfNotNull<T>(this T element, VisualElement child)
            where T : VisualElement
        {
            if (child is not null) element.Add(child);
            return element;
        }

        /// <summary>
        /// Adds multiple child elements.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddChildren<T>(this T element, params VisualElement[] children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Adds multiple child elements.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddChildren<T>(this T element, IEnumerable<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.Add(child);

            return element;
        }
    }
}