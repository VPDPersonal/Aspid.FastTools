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
        public static T SetHighLimit<T>(this T element, float value)
            where T : MinMaxSlider
        {
            element.highLimit = value;
            return element;
        }
    }
}
