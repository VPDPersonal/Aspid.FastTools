using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    /// <summary>
    /// Provides extension methods for <see cref="VisualElement"/>.
    /// </summary>
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Returns the window hosting <paramref name="element"/> or the current focus or hover fallback.
        /// </summary>
        /// <remarks>
        /// A pointer event can arrive in a floating window before focus moves, so the panel is checked first.
        /// </remarks>
        /// <param name="element">The element to locate, or <see langword="null"/> to use the fallback windows.</param>
        /// <returns>The hosting window, then the focused or hovered window; otherwise, <see langword="null"/>.</returns>
        public static EditorWindow GetOwnerWindow(this VisualElement element)
        {
            var panel = element?.panel;

            if (panel is not null)
            {
                foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    if (window && window.rootVisualElement?.panel == panel)
                        return window;
                }
            }

            return EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;
        }
    }
}
