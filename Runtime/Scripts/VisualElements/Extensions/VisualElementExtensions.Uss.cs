using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class VisualElementExtensions
    {
        #region Class
        /// <summary>
        /// Removes all USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearClasses<T>(this T element)
            where T : VisualElement
        {
            element.ClearClassList();
            return element;
        }

        /// <summary>
        /// Adds a USS class to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The USS class name to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClass<T>(this T element, string value)
            where T : VisualElement
        {
            element.AddToClassList(value);
            return element;
        }

        /// <summary>
        /// Removes a USS class from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The USS class name to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClass<T>(this T element, string value)
            where T : VisualElement
        {
            element.RemoveFromClassList(value);
            return element;
        }

        /// <summary>
        /// Adds the USS class when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The USS class name to toggle.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClass<T>(this T element, string value)
            where T : VisualElement
        {
            element.ToggleInClassList(value);
            return element;
        }

        /// <summary>
        /// Adds or removes the USS class depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="className">The USS class name to enable or disable.</param>
        /// <param name="enable">When <see langword="true"/>, the class is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClass<T>(this T element, string className, bool enable)
            where T : VisualElement
        {
            element.EnableInClassList(className, enable);
            return element;
        }
        #endregion

        #region StyleSheets
        /// <summary>
        /// Adds a style sheet to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The style sheet to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheet<T>(this T element, StyleSheet value)
            where T : VisualElement
        {
            element.styleSheets.Add(value);
            return element;
        }

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and adds it to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetFromResources<T>(this T element, string path)
            where T : VisualElement
        {
            var styleSheet = Resources.Load<StyleSheet>(path);
            if (styleSheet == null)
            {
                Debug.LogWarning($"Failed to load StyleSheet from Resources path: '{path}'");
                return element;
            }

            return element.AddStyleSheet(styleSheet);
        }

        /// <summary>
        /// Removes a style sheet from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The style sheet to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheet<T>(this T element, StyleSheet value)
            where T : VisualElement
        {
            element.styleSheets.Remove(value);
            return element;
        }

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and removes it from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetFromResources<T>(this T element, string path)
            where T : VisualElement
        {
            var styleSheet = Resources.Load<StyleSheet>(path);
            if (styleSheet != null) return element.RemoveStyleSheet(styleSheet);

            Debug.LogWarning($"Failed to load StyleSheet from Resources path: '{path}'");
            return element;
        }
        #endregion
    }
}
