using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseSlider{TValueType}"/> of <see cref="int"/>.
    /// </summary>
    /// <remarks>
    /// Kept apart from <see cref="SliderExtensions"/>: overloads that differ only by the constraint on the element type
    /// cannot share one class.
    /// </remarks>
    public static class SliderIntExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.fill"/> controlling whether the track is filled up to the current value.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the track is filled up to the current value.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFill<T>(this T element, bool value)
            where T : BaseSlider<int>
        {
            element.fill = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.inverted"/> reversing the direction of the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the slider direction is reversed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetInverted<T>(this T element, bool value)
            where T : BaseSlider<int>
        {
            element.inverted = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.pageSize"/> controlling how much the value changes per page step.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The page size to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPageSize<T>(this T element, float value)
            where T : BaseSlider<int>
        {
            element.pageSize = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.showInputField"/> controlling whether a numeric input field is shown alongside the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a numeric input field is shown next to the slider.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowInputField<T>(this T element, bool value)
            where T : BaseSlider<int>
        {
            element.showInputField = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.direction"/> controlling the orientation of the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slider direction to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDirection<T>(this T element, SliderDirection value)
            where T : BaseSlider<int>
        {
            element.direction = value;
            return element;
        }
    }
}
