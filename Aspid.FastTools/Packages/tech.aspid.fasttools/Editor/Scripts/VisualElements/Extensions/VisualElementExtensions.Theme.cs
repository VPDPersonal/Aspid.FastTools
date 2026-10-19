using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Styles <paramref name="element"/> as an Aspid.FastTools window: the palette of the editor skin and the theme
        /// picked under <c>Appearance</c>.
        /// </summary>
        /// <remarks>
        /// Style sheets added after it read the palette tokens, such as <c>--aspid-colors-surface-card</c>. The element
        /// follows a change of the theme or the skin. Call it once per element.
        /// </remarks>
        /// <typeparam name="T">A <see cref="VisualElement"/> element to configure.</typeparam>
        /// <param name="element">The element to style, usually <see cref="UnityEditor.EditorWindow.rootVisualElement"/>.</param>
        /// <returns><paramref name="element"/> for chaining.</returns>
        public static T AddAspidWindowTheme<T>(this T element)
            where T : VisualElement
        {
            element.AddToClassList(AspidStyles.WindowClass);
            return element.AddAspidThemeStyleSheets();
        }
    }
}
