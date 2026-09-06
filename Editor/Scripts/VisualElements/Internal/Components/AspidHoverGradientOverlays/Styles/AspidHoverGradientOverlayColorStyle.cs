using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the base color of an AspidHoverGradientOverlay. The color can be inherited from the StyleProperty USS
    // custom property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidHoverGradientOverlayColorStyle
    {
        public static readonly CustomStyleProperty<Color> StyleProperty = new("--aspid-fasttools-colors-hover_overlay");

        private readonly InlineStyle<Color> _value;

        public AspidHoverGradientOverlayColorStyle(VisualElement element, Color value, Action onChanged = null)
        {
            _value = new InlineStyle<Color>(value, (oldValue, newValue) =>
            {
                if (oldValue != newValue) onChanged?.Invoke();
            });

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public Color Value => _value.Value;

        public void SetValue(Color value) =>
            _value.SetInlineValue(value);

        public void SetDefaultValue(Color value) =>
            _value.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StyleProperty, out var value))
                SetDefaultValue(value);
        }
    }
}
