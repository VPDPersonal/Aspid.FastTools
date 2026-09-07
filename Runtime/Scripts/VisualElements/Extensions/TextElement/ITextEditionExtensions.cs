using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="ITextEdition"/>.
    /// </summary>
    public static class ITextEditionExtensions
    {
        /// <summary>
        /// Sets <see cref="ITextEdition.maxLength"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum character count to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxLength<T>(this T element, int value)
            where T : ITextEdition
        {
            element.maxLength = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.maskChar"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The mask character to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaskChar<T>(this T element, char value)
            where T : ITextEdition
        {
            element.maskChar = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.isDelayed"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the value is committed only on Enter or when the element loses focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDelayed<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.isDelayed = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.isReadOnly"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the element is read-only.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetReadOnly<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.isReadOnly = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.isPassword"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, input characters are masked.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPassword<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.isPassword = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.placeholder"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The placeholder text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPlaceholder<T>(this T element, string value)
            where T : ITextEdition
        {
            element.placeholder = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.autoCorrection"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard auto-corrects input.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAutoCorrection<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.autoCorrection = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.hideMobileInput"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the mobile input field is hidden.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHideMobileInput<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.hideMobileInput = value;
            return element;
        }

#if UNITY_6000_4_OR_NEWER
        /// <summary>
        /// Sets <see cref="ITextEdition.hideSoftKeyboard"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard is not shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHideSoftKeyboard<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.hideSoftKeyboard = value;
            return element;
        }
#endif

        /// <summary>
        /// Sets <see cref="ITextEdition.hidePlaceholderOnFocus"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the placeholder is hidden while the field has focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHidePlaceholderOnFocus<T>(this T element, bool value)
            where T : ITextEdition
        {
            element.hidePlaceholderOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.keyboardType"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The keyboard type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetKeyboardType<T>(this T element, TouchScreenKeyboardType value)
            where T : ITextEdition
        {
            element.keyboardType = value;
            return element;
        }
    }
}
