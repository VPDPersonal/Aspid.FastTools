using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the gradient and accent colors of an AspidGradientButton. Each color can be inherited from its USS
    // custom property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidGradientButtonColorsStyle
    {
        public static readonly CustomStyleProperty<Color> GradientProperty = new("--aspid-fasttools-colors-gradient_button-bg");
        public static readonly CustomStyleProperty<Color> AccentProperty = new("--aspid-fasttools-colors-gradient_button-accent");

        private readonly InlineStyle<Color> _gradient;
        private readonly InlineStyle<Color> _accent;

        public AspidGradientButtonColorsStyle(
            VisualElement element,
            Color gradient,
            Color accent,
            Action<Color> onGradientChanged = null,
            Action<Color> onAccentChanged = null)
        {
            _gradient = new InlineStyle<Color>(gradient, (_, value) => onGradientChanged?.Invoke(value));
            _accent = new InlineStyle<Color>(accent, (_, value) => onAccentChanged?.Invoke(value));

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public Color Gradient => _gradient.Value;
        public Color Accent => _accent.Value;

        public void SetGradient(Color value) =>
            _gradient.SetInlineValue(value);

        public void SetAccent(Color value) =>
            _accent.SetInlineValue(value);

        public void SetDefaultGradient(Color value) =>
            _gradient.SetDefaultValue(value);

        public void SetDefaultAccent(Color value) =>
            _accent.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(GradientProperty, out var gradient))
                SetDefaultGradient(gradient);

            if (evt.customStyle.TryGetValue(AccentProperty, out var accent))
                SetDefaultAccent(accent);
        }
    }
}
