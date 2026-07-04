using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static class BaseFieldExtensions
    {
        /// <summary>
        /// Sets the <see cref="BaseField{TValueType}.label"/> property displayed next to the field and returns the element for chaining.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The label text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetLabel<TField, TValue>(this TField element, string value)
            where TField : BaseField<TValue>
        {
            element.label = value;
            return element;
        }

        /// <summary>
        /// Sets the <see cref="BaseField{TValueType}.showMixedValue"/> property and returns the element for chaining.
        /// </summary>
        /// <remarks>
        /// When set to true, the field displays a mixed (dash) value, indicating that its bound targets hold different values.
        /// </remarks>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">Whether the field shows the mixed value.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseField<TValue> SetShowMixedValue<TValue>(this BaseField<TValue> element, bool value = true)
        {
            element.showMixedValue = value;
            return element;
        }
    }
}
