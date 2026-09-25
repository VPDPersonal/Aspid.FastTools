using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TextInputBaseField{TValueType}"/> of <see cref="double"/>.
    /// </summary>
    public static class TextInputBaseFieldDoubleExtensions
    {
        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.maxLength"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum character count to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxLength<T>(this T element, int value)
            where T : TextInputBaseField<double>
        {
            element.maxLength = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.maskChar"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The mask character to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaskChar<T>(this T element, char value)
            where T : TextInputBaseField<double>
        {
            element.maskChar = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isDelayed"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the value is committed only on Enter or when the field loses focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDelayed<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.isDelayed = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isReadOnly"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the field is read-only.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetReadOnly<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.isReadOnly = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isPasswordField"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, input characters are masked.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPassword<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.isPasswordField = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.placeholder"/> of <see cref="TextInputBaseField{TValueType}.textEdition"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The placeholder text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPlaceholder<T>(this T element, string value)
            where T : TextInputBaseField<double>
        {
            element.textEdition.placeholder = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.hidePlaceholderOnFocus"/> of <see cref="TextInputBaseField{TValueType}.textEdition"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the placeholder is hidden while the field has focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHidePlaceholderOnFocus<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.textEdition.hidePlaceholderOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.autoCorrection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard auto-corrects input.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAutoCorrection<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.autoCorrection = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.hideMobileInput"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the mobile input field is hidden.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHideMobileInput<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.hideMobileInput = value;
            return element;
        }

#if UNITY_6000_4_OR_NEWER
        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.hideSoftKeyboard"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard is not shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHideSoftKeyboard<T>(this T element, bool value)
            where T : TextInputBaseField<double>
        {
            element.hideSoftKeyboard = value;
            return element;
        }
#endif

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.keyboardType"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The keyboard type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetKeyboardType<T>(this T element, TouchScreenKeyboardType value)
            where T : TextInputBaseField<double>
        {
            element.keyboardType = value;
            return element;
        }
    }
}
