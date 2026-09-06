using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the background texture of a single AspidAnimatedLogo layer. The texture can be inherited from one of the
    // Layer1StyleProperty, Layer2StyleProperty or Layer3StyleProperty USS custom properties or set explicitly in code;
    // once set explicitly it is no longer overridden by USS resolution. The USS property is resolved on the parent
    // AspidAnimatedLogo (the event source), but applied as background-image to the layer (the target), so a single
    // declaration on the logo configures all three layers.
    internal readonly struct AspidAnimatedLogoLayerImageStyle
    {
        public static readonly CustomStyleProperty<Texture2D> Layer1StyleProperty =
            new("--aspid-fasttools-prop-animated_logo-layer_1");

        public static readonly CustomStyleProperty<Texture2D> Layer2StyleProperty =
            new("--aspid-fasttools-prop-animated_logo-layer_2");

        public static readonly CustomStyleProperty<Texture2D> Layer3StyleProperty =
            new("--aspid-fasttools-prop-animated_logo-layer_3");

        private readonly InlineStyle<Texture2D> _value;
        private readonly CustomStyleProperty<Texture2D> _styleProperty;

        public AspidAnimatedLogoLayerImageStyle(
            VisualElement target,
            VisualElement eventSource,
            CustomStyleProperty<Texture2D> styleProperty,
            Texture2D value)
        {
            _styleProperty = styleProperty;
            _value = new InlineStyle<Texture2D>(value, (_, newValue) =>
            {
                target.style.backgroundImage = newValue is null
                    ? StyleKeyword.Null
                    : new StyleBackground(newValue);
            });

            eventSource.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public Texture2D Value => _value;

        public void SetValue(Texture2D value) =>
            _value.SetInlineValue(value);

        public void SetDefaultValue(Texture2D value) =>
            _value.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(_styleProperty, out var value))
                SetDefaultValue(value);
        }
    }
}
