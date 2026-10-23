using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class EditorWindowWatch
    {
        // An animation redraws its whole window, so it waits while nobody watches it: Unity is in the background, or the
        // window is neither focused nor under the pointer. Each dock area and floating window draws its own panel, so
        // the focused and hovered windows are found by it.
        public static bool IsWatched(VisualElement element)
        {
            if (!InternalEditorUtility.isApplicationActive) return false;

            var panel = element.panel;
            if (panel is null) return false;

            var focused = EditorWindow.focusedWindow;
            if (focused != null && focused.rootVisualElement.panel == panel) return true;

            var hovered = EditorWindow.mouseOverWindow;
            return hovered != null && hovered.rootVisualElement.panel == panel;
        }
    }
}
