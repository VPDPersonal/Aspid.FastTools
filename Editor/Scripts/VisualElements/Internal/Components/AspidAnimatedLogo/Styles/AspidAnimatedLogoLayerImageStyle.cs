using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
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
