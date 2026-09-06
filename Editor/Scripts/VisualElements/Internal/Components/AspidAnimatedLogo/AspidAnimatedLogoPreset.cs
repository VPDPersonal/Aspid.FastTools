using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidAnimatedLogoPreset
    {
        public static AspidAnimatedLogoPreset Default => new AspidAnimatedLogoPreset()
            .SetColorCycleIntervalMs(2600)
            .SetPulseSpeed(5f)
            .SetPulseHoverAmplitude(0.04f);

        public long ColorCycleIntervalMs;

        public float PulseSpeed;

        public float PulseHoverAmplitude;

        public Texture2D Image1;

        public Texture2D Image2;

        public Texture2D Image3;

        public AspidAnimatedLogoPreset SetColorCycleIntervalMs(long value)
        {
            ColorCycleIntervalMs = value;
            return this;
        }

        public AspidAnimatedLogoPreset SetPulseSpeed(float value)
        {
            PulseSpeed = value;
            return this;
        }

        public AspidAnimatedLogoPreset SetPulseHoverAmplitude(float value)
        {
            PulseHoverAmplitude = value;
            return this;
        }

        public AspidAnimatedLogoPreset SetImage1(Texture2D value)
        {
            Image1 = value;
            return this;
        }

        public AspidAnimatedLogoPreset SetImage2(Texture2D value)
        {
            Image2 = value;
            return this;
        }

        public AspidAnimatedLogoPreset SetImage3(Texture2D value)
        {
            Image3 = value;
            return this;
        }
    }
}
