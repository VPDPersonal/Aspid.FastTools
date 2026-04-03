using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class IStyleExtensions
    {
        /// <summary>
        /// Sets the <c>unityFontStyleAndWeight</c> CSS property to <see cref="FontStyle.Normal"/>, removing bold and italic.
        /// </summary>
        /// <param name="style">The style to modify.</param>
        /// <returns>The style, for chaining.</returns>
        public static T SetNormalUnityFontStyleAndWeight<T>(this T style)
            where T : IStyle
        {
            return style.SetUnityFontStyleAndWeight(FontStyle.Normal);
        }

        /// <summary>
        /// Adds bold to the <c>unityFontStyleAndWeight</c> CSS property, preserving any existing italic style.
        /// </summary>
        /// <param name="style">The style to modify.</param>
        /// <returns>The style, for chaining.</returns>
        public static T AddBoldUnityFontStyleAndWeight<T>(this T style)
            where T : IStyle
        {
            return style.unityFontStyleAndWeight.value switch
            {
                FontStyle.Normal => style.SetUnityFontStyleAndWeight(FontStyle.Bold),
                FontStyle.Italic => style.SetUnityFontStyleAndWeight(FontStyle.BoldAndItalic),
                _ => style
            };
        }

        /// <summary>
        /// Removes bold from the <c>unityFontStyleAndWeight</c> CSS property, preserving any existing italic style.
        /// </summary>
        /// <param name="style">The style to modify.</param>
        /// <returns>The style, for chaining.</returns>
        public static T RemoveBoldUnityFontStyleAndWeight<T>(this T style)
            where T : IStyle
        {
            return style.unityFontStyleAndWeight.value switch
            {
                FontStyle.Bold => style.SetUnityFontStyleAndWeight(FontStyle.Normal),
                FontStyle.BoldAndItalic => style.SetUnityFontStyleAndWeight(FontStyle.Italic),
                _ => style
            };
        }

        /// <summary>
        /// Adds italic to the <c>unityFontStyleAndWeight</c> CSS property, preserving any existing bold style.
        /// </summary>
        /// <param name="style">The style to modify.</param>
        /// <returns>The style, for chaining.</returns>
        public static T AddItalicUnityFontStyleAndWeight<T>(this T style)
            where T : IStyle
        {
            return style.unityFontStyleAndWeight.value switch
            {
                FontStyle.Normal => style.SetUnityFontStyleAndWeight(FontStyle.Italic),
                FontStyle.Bold => style.SetUnityFontStyleAndWeight(FontStyle.BoldAndItalic),
                _ => style
            };
        }

        /// <summary>
        /// Removes italic from the <c>unityFontStyleAndWeight</c> CSS property, preserving any existing bold style.
        /// </summary>
        /// <param name="style">The style to modify.</param>
        /// <returns>The style, for chaining.</returns>
        public static T RemoveItalicUnityFontStyleAndWeight<T>(this T style)
            where T : IStyle
        {
            return style.unityFontStyleAndWeight.value switch
            {
                FontStyle.Italic => style.SetUnityFontStyleAndWeight(FontStyle.Normal),
                FontStyle.BoldAndItalic => style.SetUnityFontStyleAndWeight(FontStyle.Bold),
                _ => style
            };
        }
    }
}
