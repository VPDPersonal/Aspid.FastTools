using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the dot-grid metrics of an AspidAnimatedDotsBackground: base dot radius, base dot spacing and the
    // reference window size used by the size-scaling curve. Each value can be inherited from its USS custom property
    // or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidAnimatedDotsBackgroundSizeStyle
    {
        public static readonly CustomStyleProperty<float> DotRadiusProperty = new("--aspid-fasttools-metrics-dot_radius");
        public static readonly CustomStyleProperty<float> DotSpacingProperty = new("--aspid-fasttools-metrics-dot_spacing");
        public static readonly CustomStyleProperty<float> ScaleReferenceProperty = new("--aspid-fasttools-metrics-dot_scale_reference");

        private readonly InlineStyle<float> _dotRadius;
        private readonly InlineStyle<float> _dotSpacing;
        private readonly InlineStyle<float> _scaleReference;

        public AspidAnimatedDotsBackgroundSizeStyle(
            VisualElement element,
            float dotRadius,
            float dotSpacing,
            float scaleReference,
            Action onChanged = null)
        {
            _dotRadius = new InlineStyle<float>(dotRadius, (_, _) => onChanged?.Invoke());
            _dotSpacing = new InlineStyle<float>(dotSpacing, (_, _) => onChanged?.Invoke());
            _scaleReference = new InlineStyle<float>(scaleReference, (_, _) => onChanged?.Invoke());

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public float DotRadius => _dotRadius.Value;
        public float DotSpacing => _dotSpacing.Value;
        public float ScaleReference => _scaleReference.Value;

        public void SetDotRadius(float value) =>
            _dotRadius.SetInlineValue(value);

        public void SetDotSpacing(float value) =>
            _dotSpacing.SetInlineValue(value);

        public void SetScaleReference(float value) =>
            _scaleReference.SetInlineValue(value);

        public void SetDefaultDotRadius(float value) =>
            _dotRadius.SetDefaultValue(value);

        public void SetDefaultDotSpacing(float value) =>
            _dotSpacing.SetDefaultValue(value);

        public void SetDefaultScaleReference(float value) =>
            _scaleReference.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(DotRadiusProperty, out var radius))
                SetDefaultDotRadius(radius);
            
            if (evt.customStyle.TryGetValue(DotSpacingProperty, out var spacing)) 
                SetDefaultDotSpacing(spacing);
            
            if (evt.customStyle.TryGetValue(ScaleReferenceProperty, out var reference))
                SetDefaultScaleReference(reference);
        }
    }
}
