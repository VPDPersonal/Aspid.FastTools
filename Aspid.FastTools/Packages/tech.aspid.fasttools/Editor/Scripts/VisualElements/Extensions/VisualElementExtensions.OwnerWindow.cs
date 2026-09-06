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
        /// Returns the <see cref="EditorWindow"/> whose panel hosts <paramref name="element"/>, falling back to the
        /// focused or hovered window when no panel matches.
        /// </summary>
        /// <remarks>
        /// Use it instead of <see cref="EditorWindow.focusedWindow"/> when anchoring a dropdown to an element: a
        /// click into an unfocused floating window dispatches its pointer event before focus moves, so a rect built
        /// from the focused window's position lands in the wrong coordinate space.
        /// </remarks>
        /// <param name="element">The element whose hosting window is wanted.</param>
        /// <returns>The hosting window, or <see langword="null"/> when none can be resolved.</returns>
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
