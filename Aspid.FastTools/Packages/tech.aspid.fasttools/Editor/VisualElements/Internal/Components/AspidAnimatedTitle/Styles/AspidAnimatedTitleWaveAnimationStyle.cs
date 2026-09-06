using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the vertical wave parameters of an AspidAnimatedTitle. Stride, speed and amplitude can be inherited from
    // their USS custom properties or set explicitly in code; once set explicitly they are no longer overridden by USS
    // resolution.
    internal readonly struct AspidAnimatedTitleWaveAnimationStyle
    {
        public static readonly CustomStyleProperty<float> StrideProperty =
            new("--aspid-fasttools-prop-animated_title-wave_stride");

        public static readonly CustomStyleProperty<float> SpeedProperty =
            new("--aspid-fasttools-prop-animated_title-wave_speed");

        public static readonly CustomStyleProperty<float> AmplitudeProperty =
            new("--aspid-fasttools-prop-animated_title-wave_amplitude");

        private readonly InlineStyle<float> _stride;
        private readonly InlineStyle<float> _speed;
        private readonly InlineStyle<float> _amplitude;

        public AspidAnimatedTitleWaveAnimationStyle(
            VisualElement element,
            float stride,
            float speed,
            float amplitude,
            Action onChanged)
        {
            _stride = new InlineStyle<float>(stride, (_, _) => onChanged?.Invoke());
            _speed = new InlineStyle<float>(speed, (_, _) => onChanged?.Invoke());
            _amplitude = new InlineStyle<float>(amplitude, (_, _) => onChanged?.Invoke());

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public float Stride => _stride.Value;
        public float Speed => _speed.Value;
        public float Amplitude => _amplitude.Value;

        public void SetStride(float value) => _stride.SetInlineValue(value);

        public void SetSpeed(float value) => _speed.SetInlineValue(value);

        public void SetAmplitude(float value) => _amplitude.SetInlineValue(value);

        public void SetDefaultStride(float value) => _stride.SetDefaultValue(value);

        public void SetDefaultSpeed(float value) => _speed.SetDefaultValue(value);

        public void SetDefaultAmplitude(float value) => _amplitude.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StrideProperty, out var stride)) SetDefaultStride(stride);
            if (evt.customStyle.TryGetValue(SpeedProperty, out var speed)) SetDefaultSpeed(speed);
            if (evt.customStyle.TryGetValue(AmplitudeProperty, out var amplitude)) SetDefaultAmplitude(amplitude);
        }
    }
}
