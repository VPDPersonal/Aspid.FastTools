using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="Foldout"/>.
    /// </summary>
    public static class FoldoutExtensions
    {
        /// <summary>
        /// Sets <see cref="Foldout.text"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetText<T>(this T element, string value)
            where T : Foldout
        {
            element.text = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Foldout.toggleOnLabelClick"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, clicking the label toggles the foldout.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetToggleOnLabelClick<T>(this T element, bool value)
            where T : Foldout
        {
            element.toggleOnLabelClick = value;
            return element;
        }
    }
}
