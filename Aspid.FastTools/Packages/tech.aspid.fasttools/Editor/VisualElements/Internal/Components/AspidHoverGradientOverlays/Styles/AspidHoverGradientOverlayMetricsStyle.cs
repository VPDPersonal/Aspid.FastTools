using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the visual and animation metrics of an AspidHoverGradientOverlay: the number of vertical strips, the
    // lerp rate of the fade animation, and the peak alpha scale of the overlay. Each value can be inherited from its
    // USS custom property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidHoverGradientOverlayMetricsStyle
    {
        public static readonly CustomStyleProperty<int> StepsProperty = new("--aspid-fasttools-metrics-hover_overlay_steps");
        public static readonly CustomStyleProperty<float> LerpRateProperty = new("--aspid-fasttools-metrics-hover_overlay_lerp_rate");
        public static readonly CustomStyleProperty<float> AlphaScaleProperty = new("--aspid-fasttools-metrics-hover_overlay_alpha_scale");

        private readonly InlineStyle<int> _steps;
        private readonly InlineStyle<float> _lerpRate;
        private readonly InlineStyle<float> _alphaScale;

        public AspidHoverGradientOverlayMetricsStyle(
            VisualElement element,
            int steps,
            float lerpRate,
            float alphaScale,
            Action onChanged = null)
        {
            _steps = new InlineStyle<int>(steps, (_, _) => onChanged?.Invoke());
            _lerpRate = new InlineStyle<float>(lerpRate, (_, _) => onChanged?.Invoke());
            _alphaScale = new InlineStyle<float>(alphaScale, (_, _) => onChanged?.Invoke());

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public int Steps => _steps.Value;
        public float LerpRate => _lerpRate.Value;
        public float AlphaScale => _alphaScale.Value;

        public void SetSteps(int value) =>
            _steps.SetInlineValue(value);

        public void SetLerpRate(float value) =>
            _lerpRate.SetInlineValue(value);

        public void SetAlphaScale(float value) =>
            _alphaScale.SetInlineValue(value);

        public void SetDefaultSteps(int value) =>
            _steps.SetDefaultValue(value);

        public void SetDefaultLerpRate(float value) =>
            _lerpRate.SetDefaultValue(value);

        public void SetDefaultAlphaScale(float value) =>
            _alphaScale.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(StepsProperty, out var steps))
                SetDefaultSteps(steps);

            if (evt.customStyle.TryGetValue(LerpRateProperty, out var lerpRate))
                SetDefaultLerpRate(lerpRate);

            if (evt.customStyle.TryGetValue(AlphaScaleProperty, out var alphaScale))
                SetDefaultAlphaScale(alphaScale);
        }
    }
}
