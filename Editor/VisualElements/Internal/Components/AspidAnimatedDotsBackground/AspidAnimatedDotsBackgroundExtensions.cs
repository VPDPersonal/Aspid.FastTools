using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidAnimatedDotsBackgroundExtensions
    {
        public static AspidAnimatedDotsBackground SetStatus(this AspidAnimatedDotsBackground element, StatusStyle.Type value)
        {
            element.Status = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetColor1(this AspidAnimatedDotsBackground element, Color value)
        {
            element.Color1 = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetColor2(this AspidAnimatedDotsBackground element, Color value)
        {
            element.Color2 = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetColor3(this AspidAnimatedDotsBackground element, Color value)
        {
            element.Color3 = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetDotRadius(this AspidAnimatedDotsBackground element, float value)
        {
            element.DotRadius = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetDotSpacing(this AspidAnimatedDotsBackground element, float value)
        {
            element.DotSpacing = value;
            return element;
        }

        public static AspidAnimatedDotsBackground SetScaleReferenceSize(this AspidAnimatedDotsBackground element, float value)
        {
            element.ScaleReferenceSize = value;
            return element;
        }
    }
}
