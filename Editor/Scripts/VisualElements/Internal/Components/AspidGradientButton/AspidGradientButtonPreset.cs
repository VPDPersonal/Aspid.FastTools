using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidGradientButtonPreset
    {
        public static AspidGradientButtonPreset Default => new AspidGradientButtonPreset()
            .SetText(string.Empty);

        public string Text;

        public string TrailingText;

        public Action<EventBase> OnClick;

        public Color Gradient;

        public Color Accent;

        public AspidGradientButtonPreset SetText(string value)
        {
            Text = value;
            return this;
        }

        public AspidGradientButtonPreset SetTrailingText(string value)
        {
            TrailingText = value;
            return this;
        }

        public AspidGradientButtonPreset SetOnClick(Action<EventBase> value)
        {
            OnClick = value;
            return this;
        }

        public AspidGradientButtonPreset SetGradient(Color value)
        {
            Gradient = value;
            return this;
        }

        public AspidGradientButtonPreset SetAccent(Color value)
        {
            Accent = value;
            return this;
        }
    }
}
