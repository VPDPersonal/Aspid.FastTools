using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Adds a USS class to the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddClass<T>(this T element, string className)
            where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }

        /// <summary>
        /// Removes a USS class from the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T RemoveClass<T>(this T element, string className)
            where T : VisualElement
        {
            element.RemoveFromClassList(className);
            return element;
        }

        /// <summary>
        /// Toggles a USS class on the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T ToggleClass<T>(this T element, string className)
            where T : VisualElement
        {
            element.ToggleInClassList(className);
            return element;
        }

        /// <summary>
        /// Adds a style sheet to the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T AddStyleSheets<T>(this T element, StyleSheet styleSheet)
            where T : VisualElement
        {
            element.styleSheets.Add(styleSheet);
            return element;
        }

        /// <summary>
        /// Adds a style sheet to the element by loading it from <c>Resources</c> at the given path.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The resource path passed to <see cref="Resources.Load{T}(string)"/>.</param>
        /// <returns>The element for method chaining.</returns>
        public static T AddStyleSheetsFromResource<T>(this T element, string path)
            where T : VisualElement
        {
            element.styleSheets.Add(styleSheet: Resources.Load<StyleSheet>(path));
            return element;
        }

        /// <summary>
        /// Removes a style sheet from the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T RemoveStyleSheets<T>(this T element, StyleSheet styleSheet)
            where T : VisualElement
        {
            element.styleSheets.Remove(styleSheet);
            return element;
        }

        /// <summary>
        /// Removes a style sheet from the element by loading it from <c>Resources</c> at the given path.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The resource path passed to <see cref="Resources.Load{T}(string)"/>.</param>
        /// <returns>The element for method chaining.</returns>
        public static T RemoveStyleSheetsFromResource<T>(this T element, string path)
            where T : VisualElement
        {
            element.styleSheets.Remove(styleSheet: Resources.Load<StyleSheet>(path));
            return element;
        }
    }
}
