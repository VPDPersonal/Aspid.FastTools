using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Bridges the --aspid-fasttools-colors-gradient USS custom property declared on an AspidInspectorHeader to the
    // Color of its overlay.
    internal readonly struct AspidInspectorHeaderGradientStyle
    {
        public static readonly CustomStyleProperty<Color> StyleProperty = new("--aspid-fasttools-colors-gradient");

        private readonly AspidHoverGradientOverlay _overlay;

        public AspidInspectorHeaderGradientStyle(AspidInspectorHeader element, AspidHoverGradientOverlay overlay)
        {
            _overlay = overlay;
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StyleProperty, out var color))
                _overlay.Color = color;
        }
    }
}
