using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the per-character color cycling parameters of an AspidAnimatedTitle. Both stride and speed can be
    // inherited from their USS custom properties or set explicitly in code; once set explicitly they are no longer
    // overridden by USS resolution.
    internal readonly struct AspidAnimatedTitleColorAnimationStyle
    {
        public static readonly CustomStyleProperty<float> StrideProperty =
            new("--aspid-fasttools-prop-animated_title-color_stride");

        public static readonly CustomStyleProperty<float> SpeedProperty =
            new("--aspid-fasttools-prop-animated_title-color_speed");

        private readonly InlineStyle<float> _stride;
        private readonly InlineStyle<float> _speed;

        public AspidAnimatedTitleColorAnimationStyle(
            VisualElement element,
            float stride,
            float speed,
            Action onChanged)
        {
            _stride = new InlineStyle<float>(stride, (_, _) => onChanged?.Invoke());
            _speed = new InlineStyle<float>(speed, (_, _) => onChanged?.Invoke());

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public float Stride => _stride.Value;
        public float Speed => _speed.Value;

        public void SetStride(float value) => _stride.SetInlineValue(value);

        public void SetSpeed(float value) => _speed.SetInlineValue(value);

        public void SetDefaultStride(float value) => _stride.SetDefaultValue(value);

        public void SetDefaultSpeed(float value) => _speed.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StrideProperty, out var stride)) SetDefaultStride(stride);
            if (evt.customStyle.TryGetValue(SpeedProperty, out var speed)) SetDefaultSpeed(speed);
        }
    }
}
