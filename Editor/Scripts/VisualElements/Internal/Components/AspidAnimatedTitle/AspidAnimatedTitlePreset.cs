using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidAnimatedTitlePreset
    {
        public static AspidAnimatedTitlePreset Default => new AspidAnimatedTitlePreset()
            .SetColorStride(0.12f)
            .SetColorSpeed(0.4f)
            .SetWaveStride(0.55f)
            .SetWaveSpeed(1.6f)
            .SetWaveAmplitude(3f);

        public float ColorStride;

        public float ColorSpeed;

        public float WaveStride;

        public float WaveSpeed;

        public float WaveAmplitude;

        public Color Color1;

        public Color Color2;

        public Color Color3;

        public AspidAnimatedTitlePreset SetColorStride(float value)
        {
            ColorStride = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetColorSpeed(float value)
        {
            ColorSpeed = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetWaveStride(float value)
        {
            WaveStride = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetWaveSpeed(float value)
        {
            WaveSpeed = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetWaveAmplitude(float value)
        {
            WaveAmplitude = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetColor1(Color value)
        {
            Color1 = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetColor2(Color value)
        {
            Color2 = value;
            return this;
        }

        public AspidAnimatedTitlePreset SetColor3(Color value)
        {
            Color3 = value;
            return this;
        }
    }
}
