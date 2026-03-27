using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Focuses the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFocus<T>(this T element)
            where T : VisualElement
        {
            element.Focus();
            return element;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the element currently has focus.
        /// </summary>
        public static bool IsFocus(this VisualElement element) =>
            element.focusController.focusedElement == element;
    }
}