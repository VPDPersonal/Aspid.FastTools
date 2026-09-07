using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseBoolField"/>.
    /// </summary>
    public static class BaseBoolFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseBoolField.text"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetText<T>(this T element, string value)
            where T : BaseBoolField
        {
            element.text = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseField{TValueType}.label"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The label text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLabel<T>(this T element, string value)
            where T : BaseBoolField
        {
            element.label = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseBoolField.toggleOnLabelClick"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, clicking the label toggles the value.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetToggleOnLabelClick<T>(this T element, bool value)
            where T : BaseBoolField
        {
            element.toggleOnLabelClick = value;
            return element;
        }
    }
}
