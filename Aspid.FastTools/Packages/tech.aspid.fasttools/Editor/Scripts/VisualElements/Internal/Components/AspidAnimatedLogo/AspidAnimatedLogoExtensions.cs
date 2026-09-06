using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidAnimatedLogoExtensions
    {
        public static AspidAnimatedLogo SetColorCycleIntervalMs(this AspidAnimatedLogo element, long value)
        {
            element.ColorCycleIntervalMs = value;
            return element;
        }

        public static AspidAnimatedLogo SetPulseSpeed(this AspidAnimatedLogo element, float value)
        {
            element.PulseSpeed = value;
            return element;
        }

        public static AspidAnimatedLogo SetPulseHoverAmplitude(this AspidAnimatedLogo element, float value)
        {
            element.PulseHoverAmplitude = value;
            return element;
        }

        public static AspidAnimatedLogo SetImage1(this AspidAnimatedLogo element, Texture2D value)
        {
            element.Image1 = value;
            return element;
        }

        public static AspidAnimatedLogo SetImage2(this AspidAnimatedLogo element, Texture2D value)
        {
            element.Image2 = value;
            return element;
        }

        public static AspidAnimatedLogo SetImage3(this AspidAnimatedLogo element, Texture2D value)
        {
            element.Image3 = value;
            return element;
        }
    }
}
