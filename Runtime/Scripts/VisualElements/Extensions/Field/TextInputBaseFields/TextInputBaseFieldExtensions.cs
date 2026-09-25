using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="TextInputBaseField{TValueType}"/>.
    /// </summary>
    public static class TextInputBaseFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.maxLength"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum character count to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetMaxLength<TField, TValue>(this TField element, int value)
            where TField : TextInputBaseField<TValue>
        {
            element.maxLength = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.maskChar"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The mask character to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetMaskChar<TField, TValue>(this TField element, char value)
            where TField : TextInputBaseField<TValue>
        {
            element.maskChar = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isDelayed"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the value is committed only on Enter or when the field loses focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetDelayed<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.isDelayed = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isReadOnly"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the field is read-only.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetReadOnly<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.isReadOnly = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.isPasswordField"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, input characters are masked.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetPassword<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.isPasswordField = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.placeholder"/> of <see cref="TextInputBaseField{TValueType}.textEdition"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The placeholder text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetPlaceholder<TField, TValue>(this TField element, string value)
            where TField : TextInputBaseField<TValue>
        {
            element.textEdition.placeholder = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextEdition.hidePlaceholderOnFocus"/> of <see cref="TextInputBaseField{TValueType}.textEdition"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the placeholder is hidden while the field has focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetHidePlaceholderOnFocus<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.textEdition.hidePlaceholderOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.autoCorrection"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard auto-corrects input.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetAutoCorrection<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.autoCorrection = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.hideMobileInput"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the mobile input field is hidden.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetHideMobileInput<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.hideMobileInput = value;
            return element;
        }

#if UNITY_6000_4_OR_NEWER
        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.hideSoftKeyboard"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the soft keyboard is not shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetHideSoftKeyboard<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.hideSoftKeyboard = value;
            return element;
        }
#endif

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.keyboardType"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The keyboard type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetKeyboardType<TField, TValue>(this TField element, TouchScreenKeyboardType value)
            where TField : TextInputBaseField<TValue>
        {
            element.keyboardType = value;
            return element;
        }
    }
}
