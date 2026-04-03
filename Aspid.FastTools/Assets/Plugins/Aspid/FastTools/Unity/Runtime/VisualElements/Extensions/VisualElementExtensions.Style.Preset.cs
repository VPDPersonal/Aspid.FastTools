using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Sets the <c>style.unityFontStyleAndWeight</c> CSS property to normal, removing bold and italic.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetNormalUnityFontStyleAndWeight<T>(this T element)
            where T : VisualElement
        {
            element.style.SetNormalUnityFontStyleAndWeight();
            return element;
        }

        /// <summary>
        /// Adds bold to the <c>style.unityFontStyleAndWeight</c> CSS property, preserving any existing italic style.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddBoldUnityFontStyleAndWeight<T>(this T element)
            where T : VisualElement
        {
            element.style.AddBoldUnityFontStyleAndWeight();
            return element;
        }

        /// <summary>
        /// Removes bold from the <c>style.unityFontStyleAndWeight</c> CSS property, preserving any existing italic style.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveBoldUnityFontStyleAndWeight<T>(this T element)
            where T : VisualElement
        {
            element.style.RemoveBoldUnityFontStyleAndWeight();
            return element;
        }

        /// <summary>
        /// Adds italic to the <c>style.unityFontStyleAndWeight</c> CSS property, preserving any existing bold style.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItalicUnityFontStyleAndWeight<T>(this T element)
            where T : VisualElement
        {
            element.style.AddItalicUnityFontStyleAndWeight();
            return element;
        }

        /// <summary>
        /// Removes italic from the <c>style.unityFontStyleAndWeight</c> CSS property, preserving any existing bold style.
        /// </summary>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItalicUnityFontStyleAndWeight<T>(this T element)
            where T : VisualElement
        {
            element.style.RemoveItalicUnityFontStyleAndWeight();
            return element;
        }
    }
}
