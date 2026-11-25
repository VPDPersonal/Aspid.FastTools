#if !UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidHoverGradientOverlay
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidHoverGradientOverlay, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlIntAttributeDescription _steps = new() { name = "steps" };
            private readonly UxmlColorAttributeDescription _color = new() { name = "color" };
            private readonly UxmlFloatAttributeDescription _lerpRate = new() { name = "lerp-rate" };
            private readonly UxmlFloatAttributeDescription _alphaScale = new() { name = "alpha-scale" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidHoverGradientOverlay)ve;

                var color = element.Color;
                if (_color.TryGetValueFromBag(bag, cc: cc, value: ref color))
                    element.Color = color;

                var steps = element.Steps;
                if (_steps.TryGetValueFromBag(bag, cc: cc, value: ref steps))
                    element.Steps = steps;

                var lerpRate = element.LerpRate;
                if (_lerpRate.TryGetValueFromBag(bag, cc: cc, value: ref lerpRate))
                    element.LerpRate = lerpRate;

                var alphaScale = element.AlphaScale;
                if (_alphaScale.TryGetValueFromBag(bag, cc: cc, value: ref alphaScale))
                    element.AlphaScale = alphaScale;
            }
        }
    }
}
#endif
