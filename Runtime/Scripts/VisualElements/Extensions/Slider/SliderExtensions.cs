using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseSlider{TValueType}"/>.
    /// </summary>
    public static class SliderExtensions
    {
        #region Int
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, int value)
            where T : BaseSlider<int>
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
        public static T SetHighValue<T>(this T element, int value)
            where T : BaseSlider<int>
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
        public static T SetLowValue<T>(this T element, uint value)
            where T : BaseSlider<uint>
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
        public static T SetHighValue<T>(this T element, uint value)
            where T : BaseSlider<uint>
        {
            element.highValue = value;
            return element;
        }
        #endregion

        #region Long
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, long value)
            where T : BaseSlider<long>
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
        public static T SetHighValue<T>(this T element, long value)
            where T : BaseSlider<long>
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
        public static T SetLowValue<T>(this T element, ulong value)
            where T : BaseSlider<ulong>
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
        public static T SetHighValue<T>(this T element, ulong value)
            where T : BaseSlider<ulong>
        {
            element.highValue = value;
            return element;
        }
        #endregion

        #region Byte
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
        #endregion

        #region Short
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, short value)
            where T : BaseSlider<short>
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
        public static T SetHighValue<T>(this T element, short value)
            where T : BaseSlider<short>
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
        public static T SetLowValue<T>(this T element, ushort value)
            where T : BaseSlider<ushort>
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
        public static T SetHighValue<T>(this T element, ushort value)
            where T : BaseSlider<ushort>
        {
            element.highValue = value;
            return element;
        }
        #endregion

        #region Float
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
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
        #endregion

        #region Double
        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.lowValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The low value to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLowValue<T>(this T element, double value)
            where T : BaseSlider<double>
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
        public static T SetHighValue<T>(this T element, double value)
            where T : BaseSlider<double>
        {
            element.highValue = value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.fill"/> controlling whether the track is filled up to the current value.
        /// </summary>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the track is filled up to the current value.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseSlider<TValue> SetFill<TValue>(this BaseSlider<TValue> element, bool value)
            where TValue : IComparable<TValue>
        {
            element.fill = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.inverted"/> reversing the direction of the element.
        /// </summary>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the slider direction is reversed.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseSlider<TValue> SetInverted<TValue>(this BaseSlider<TValue> element, bool value)
            where TValue : IComparable<TValue>
        {
            element.inverted = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.pageSize"/> controlling how much the value changes per page step.
        /// </summary>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The page size to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseSlider<TValue> SetPageSize<TValue>(this BaseSlider<TValue> element, float value)
            where TValue : IComparable<TValue>
        {
            element.pageSize = value;
            return element;
        }

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

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.showInputField"/> controlling whether a numeric input field is shown alongside the element.
        /// </summary>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a numeric input field is shown next to the slider.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseSlider<TValue> SetShowInputField<TValue>(this BaseSlider<TValue> element, bool value)
            where TValue : IComparable<TValue>
        {
            element.showInputField = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseSlider{TValueType}.direction"/> controlling the orientation of the element.
        /// </summary>
        /// <typeparam name="TValue">The value type of the element.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slider direction to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static BaseSlider<TValue> SetDirection<TValue>(this BaseSlider<TValue> element, SliderDirection value)
            where TValue : IComparable<TValue>
        {
            element.direction = value;
            return element;
        }
    }
}
