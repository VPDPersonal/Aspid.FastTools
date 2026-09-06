using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidGradientButtonExtensions
    {
        public static AspidGradientButton SetText(this AspidGradientButton element, string value)
        {
            element.Text = value;
            return element;
        }

        public static AspidGradientButton SetTrailingText(this AspidGradientButton element, string value)
        {
            element.TrailingText = value;
            return element;
        }

        public static AspidGradientButton SetGradient(this AspidGradientButton element, Color value)
        {
            element.Gradient = value;
            return element;
        }

        public static AspidGradientButton SetAccent(this AspidGradientButton element, Color value)
        {
            element.Accent = value;
            return element;
        }
    }
}
