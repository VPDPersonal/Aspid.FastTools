using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="PopupField{T}"/>.
    /// </summary>
    /// <remarks>
    /// Only <see cref="SetChoices{TField, TChoice}"/> infers the choice type. The other methods need explicit type
    /// arguments; <see cref="DropdownField"/> has overloads without them in <see cref="DropdownFieldExtensions"/>.
    /// </remarks>
    public static class PopupFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="BasePopupField{TValueType, TValueChoice}.choices"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TChoice">The choice type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The list of choices to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetChoices<TField, TChoice>(this TField element, List<TChoice> value)
            where TField : PopupField<TChoice>
        {
            element.choices = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.index"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TChoice">The choice type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The index of the selected choice to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetIndex<TField, TChoice>(this TField element, int value)
            where TField : PopupField<TChoice>
        {
            element.index = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.formatListItemCallback"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TChoice">The choice type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback that formats a choice shown in the popup list.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetFormatListItemCallback<TField, TChoice>(this TField element, Func<TChoice, string> value)
            where TField : PopupField<TChoice>
        {
            element.formatListItemCallback = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.formatSelectedValueCallback"/>, replacing any existing callback.
        /// </summary>
        /// <remarks>
        /// Unity calls <paramref name="value"/> at once with the current value, which is <see langword="null"/> for a
        /// reference type before a choice is selected.
        /// </remarks>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TChoice">The choice type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback that formats the selected value shown in the field.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetFormatSelectedValueCallback<TField, TChoice>(this TField element, Func<TChoice, string> value)
            where TField : PopupField<TChoice>
        {
            element.formatSelectedValueCallback = value;
            return element;
        }
    }
}
