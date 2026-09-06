using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidAnimatedTitleExtensions
    {
        public static AspidAnimatedTitle SetText(this AspidAnimatedTitle element, string value)
        {
            element.Text = value;
            return element;
        }

        public static AspidAnimatedTitle SetColorStride(this AspidAnimatedTitle element, float value)
        {
            element.ColorStride = value;
            return element;
        }

        public static AspidAnimatedTitle SetColorSpeed(this AspidAnimatedTitle element, float value)
        {
            element.ColorSpeed = value;
            return element;
        }

        public static AspidAnimatedTitle SetWaveStride(this AspidAnimatedTitle element, float value)
        {
            element.WaveStride = value;
            return element;
        }

        public static AspidAnimatedTitle SetWaveSpeed(this AspidAnimatedTitle element, float value)
        {
            element.WaveSpeed = value;
            return element;
        }

        public static AspidAnimatedTitle SetWaveAmplitude(this AspidAnimatedTitle element, float value)
        {
            element.WaveAmplitude = value;
            return element;
        }

        public static AspidAnimatedTitle SetColor1(this AspidAnimatedTitle element, Color value)
        {
            element.Color1 = value;
            return element;
        }

        public static AspidAnimatedTitle SetColor2(this AspidAnimatedTitle element, Color value)
        {
            element.Color2 = value;
            return element;
        }

        public static AspidAnimatedTitle SetColor3(this AspidAnimatedTitle element, Color value)
        {
            element.Color3 = value;
            return element;
        }
    }
}
