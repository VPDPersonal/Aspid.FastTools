using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools
{
    public static class TextElementExtensions
    {
        /// <summary>
        /// Sets the text content of the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetText<T>(this T textElement, string text)
            where T : TextElement
        {
            textElement.text = text;
            return textElement;
        }
    }
}