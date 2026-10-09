using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="MinMaxSlider"/>.
    /// </summary>
    public static class MinMaxSliderExtensions
    {
        /// <summary>
        /// Sets <see cref="MinMaxSlider.minValue"/>.
        /// </summary>
        /// <remarks>
        /// Unity clamps <paramref name="value"/> to the current <see cref="MinMaxSlider.maxValue"/>: on the default
        /// range of 0 to 10, <c>SetMinValue(20).SetMaxValue(30)</c> gives 10 to 30. To move the range up, call
        /// <see cref="SetMaxValue{T}"/> first or set both ends with
        /// <see cref="INotifyValueChangedExtensions.SetValue{T}(T, UnityEngine.Vector2, bool)"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The lower end of the selected range to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMinValue<T>(this T element, float value)
            where T : MinMaxSlider
        {
            element.minValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="MinMaxSlider.maxValue"/>.
        /// </summary>
        /// <remarks>
        /// Unity clamps <paramref name="value"/> to the current <see cref="MinMaxSlider.minValue"/>. To move the range
        /// down, call <see cref="SetMinValue{T}"/> first or set both ends with
        /// <see cref="INotifyValueChangedExtensions.SetValue{T}(T, UnityEngine.Vector2, bool)"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The upper end of the selected range to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxValue<T>(this T element, float value)
            where T : MinMaxSlider
        {
            element.maxValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="MinMaxSlider.lowLimit"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The lowest value the selected range can reach.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is greater than <see cref="MinMaxSlider.highLimit"/>.
        /// </exception>
        public static T SetLowLimit<T>(this T element, float value)
            where T : MinMaxSlider
        {
            element.lowLimit = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="MinMaxSlider.highLimit"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The highest value the selected range can reach.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is smaller than <see cref="MinMaxSlider.lowLimit"/>.
        /// </exception>
        public static T SetHighLimit<T>(this T element, float value)
            where T : MinMaxSlider
        {
            element.highLimit = value;
            return element;
        }
    }
}
