using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidAnimatedDotsBackgroundPreset
    {
        public static AspidAnimatedDotsBackgroundPreset Default => new AspidAnimatedDotsBackgroundPreset()
            .SetDotSpacing(18)
            .SetDotRadius(1.55f)
            .SetScaleReferenceSize(420)
            .SetStatus(StatusStyle.Type.None);

        public StatusStyle.Type Status;

        public Color Color1;

        public Color Color2;

        public Color Color3;

        public float DotRadius;

        public float DotSpacing;

        public float ScaleReferenceSize;

        public AspidAnimatedDotsBackgroundPreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetColor1(Color value)
        {
            Color1 = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetColor2(Color value)
        {
            Color2 = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetColor3(Color value)
        {
            Color3 = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetDotRadius(float value)
        {
            DotRadius = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetDotSpacing(float value)
        {
            DotSpacing = value;
            return this;
        }

        public AspidAnimatedDotsBackgroundPreset SetScaleReferenceSize(float value)
        {
            ScaleReferenceSize = value;
            return this;
        }
    }
}
