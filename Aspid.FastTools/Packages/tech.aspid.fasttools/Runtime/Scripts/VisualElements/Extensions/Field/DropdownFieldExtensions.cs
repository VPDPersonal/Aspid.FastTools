using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="DropdownField"/>.
    /// </summary>
    /// <remarks>
    /// Counterparts of <see cref="PopupFieldExtensions"/> that need no type arguments.
    /// <see cref="PopupFieldExtensions.SetChoices{TField, TChoice}"/> needs none either, so <see cref="SetChoices{T}"/>
    /// only adds support for sources other than a <see cref="List{T}"/>.
    /// </remarks>
    public static class DropdownFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="BasePopupField{TValueType, TValueChoice}.choices"/> to a copy of <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// Use it for an array or a query. A <see cref="List{T}"/> argument calls
        /// <see cref="PopupFieldExtensions.SetChoices{TField, TChoice}"/>, which keeps the list itself.
        /// </remarks>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The choices to copy.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetChoices<T>(this T element, IEnumerable<string> value)
            where T : DropdownField
        {
            element.choices = value is null ? null : new List<string>(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.index"/>.
        /// </summary>
        /// <remarks>
        /// Unity resolves the index against the current choices: call
        /// <see cref="PopupFieldExtensions.SetChoices{TField, TChoice}"/> or
        /// <see cref="SetChoices{T}"/> first.
        /// </remarks>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The index of the selected choice to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetIndex<T>(this T element, int value)
            where T : DropdownField
        {
            element.index = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.formatListItemCallback"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback that formats a choice shown in the popup list.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFormatListItemCallback<T>(this T element, Func<string, string> value)
            where T : DropdownField
        {
            element.formatListItemCallback = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="PopupField{T}.formatSelectedValueCallback"/>, replacing any existing callback.
        /// </summary>
        /// <remarks>
        /// Unity calls <paramref name="value"/> at once with the current value, which is <see langword="null"/> for a
        /// <see cref="DropdownField"/> before a choice is selected.
        /// </remarks>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback that formats the selected value shown in the field.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFormatSelectedValueCallback<T>(this T element, Func<string, string> value)
            where T : DropdownField
        {
            element.formatSelectedValueCallback = value;
            return element;
        }
    }
}
