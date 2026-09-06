using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the angular speed of the hover pulse animation for an AspidAnimatedLogo. The value can be inherited from
    // the StyleProperty USS custom property or set explicitly in code; once set explicitly it is no longer overridden
    // by USS resolution.
    internal readonly struct AspidAnimatedLogoPulseSpeedStyle
    {
        public static readonly CustomStyleProperty<float> StyleProperty =
            new("--aspid-fasttools-prop-animated_logo-pulse_speed");

        private readonly InlineStyle<float> _value;

        public AspidAnimatedLogoPulseSpeedStyle(VisualElement element, float value)
        {
            _value = new InlineStyle<float>(value);
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public float Value => _value;

        public void SetValue(float value) =>
            _value.SetInlineValue(value);

        public void SetDefaultValue(float value) =>
            _value.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StyleProperty, out var value))
                SetDefaultValue(value);
        }
    }
}
