using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class SliderExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <remarks>
        /// Call before <c>SetValue</c>: a slider clamps its value to the range.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, float value)
            where T : BaseSlider<float>
        {
            element.lowValue = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.highValue"/>.
        /// </summary>
        /// <remarks>
        /// Call before <c>SetValue</c>: a slider clamps its value to the range.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The high value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHighValue<T>(this T element, float value)
            where T : BaseSlider<float>
        {
            element.highValue = value;
            return element;
        }

#if UNITY_6000_0_OR_NEWER
        /// <inheritdoc cref="SliderFloatExtensions.SetFill{T}(T, bool)"/>
        public static T SetFill<T>(this T element, bool value)
            where T : BaseSlider<float> =>
            SliderFloatExtensions.SetFill(element, value: value);
#endif

        /// <inheritdoc cref="SliderFloatExtensions.SetInverted{T}(T, bool)"/>
        public static T SetInverted<T>(this T element, bool value)
            where T : BaseSlider<float> =>
            SliderFloatExtensions.SetInverted(element, value: value);

        /// <inheritdoc cref="SliderFloatExtensions.SetPageSize{T}(T, float)"/>
        public static T SetPageSize<T>(this T element, float value)
            where T : BaseSlider<float> =>
            SliderFloatExtensions.SetPageSize(element, value: value);

        /// <inheritdoc cref="SliderFloatExtensions.SetShowInputField{T}(T, bool)"/>
        public static T SetShowInputField<T>(this T element, bool value)
            where T : BaseSlider<float> =>
            SliderFloatExtensions.SetShowInputField(element, value: value);

        /// <inheritdoc cref="SliderFloatExtensions.SetDirection{T}(T, SliderDirection)"/>
        public static T SetDirection<T>(this T element, SliderDirection value)
            where T : BaseSlider<float> =>
            SliderFloatExtensions.SetDirection(element, value: value);
    }
}
