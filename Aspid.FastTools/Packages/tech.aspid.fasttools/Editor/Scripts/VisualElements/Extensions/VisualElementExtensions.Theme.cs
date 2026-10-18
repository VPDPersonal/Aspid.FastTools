using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Styles <paramref name="element"/> and its children with the Aspid.FastTools palette and the theme picked under
        /// <c>Appearance</c> in the FastTools settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The element declares the tokens of the palette, such as <c>--aspid-colors-surface-card</c>: the dark palette,
        /// or the light one on the light editor skin. A built-in theme or a theme of the project overrides them, and the
        /// element follows a change of the theme or the skin without a rebuild.
        /// </para>
        /// <para>
        /// Call it once per element, usually on <see cref="UnityEditor.EditorWindow.rootVisualElement"/>, and add your
        /// own style sheets after it, so that they read the tokens.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">A <see cref="VisualElement"/> element to configure.</typeparam>
        /// <param name="element">The element that declares the tokens.</param>
        /// <returns><paramref name="element"/> for chaining.</returns>
        public static T AddAspidTheme<T>(this T element)
            where T : VisualElement
        {
            // The light palette applies only inside an Aspid window, so the element becomes one.
            element.AddToClassList(AspidStyles.WindowClass);
            return element.AddAspidThemeStyleSheets();
        }
    }
}
