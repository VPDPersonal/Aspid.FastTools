using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseSlider{TValueType}"/>.
    /// </summary>
    public static partial class SliderExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T, TValue>(this T element, TValue value)
            where T : BaseSlider<TValue>
            where TValue : IComparable<TValue>
        {
            element.lowValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.highValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The high value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHighValue<T, TValue>(this T element, TValue value)
            where T : BaseSlider<TValue>
            where TValue : IComparable<TValue>
        {
            element.highValue = value;
            return element;
        }
    }
}
