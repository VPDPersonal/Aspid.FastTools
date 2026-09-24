using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class SliderExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, byte value)
            where T : BaseSlider<byte>
        {
            element.lowValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.highValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The high value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHighValue<T>(this T element, byte value)
            where T : BaseSlider<byte>
        {
            element.highValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, sbyte value)
            where T : BaseSlider<sbyte>
        {
            element.lowValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.highValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The high value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHighValue<T>(this T element, sbyte value)
            where T : BaseSlider<sbyte>
        {
            element.highValue = value;
            return element;
        }
    }
}
