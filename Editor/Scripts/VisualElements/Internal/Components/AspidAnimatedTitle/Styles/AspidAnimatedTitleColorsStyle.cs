using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the three palette colors of an AspidAnimatedTitle. Each color can be inherited from its USS custom
    // property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidAnimatedTitleColorsStyle
    {
        public static readonly CustomStyleProperty<Color> Color1Property =
            new("--aspid-fasttools-colors-animated_title-color_1");

        public static readonly CustomStyleProperty<Color> Color2Property =
            new("--aspid-fasttools-colors-animated_title-color_2");

        public static readonly CustomStyleProperty<Color> Color3Property =
            new("--aspid-fasttools-colors-animated_title-color_3");

        private readonly InlineStyle<Color> _color1;
        private readonly InlineStyle<Color> _color2;
        private readonly InlineStyle<Color> _color3;

        public Color Color1 => _color1.Value;
        public Color Color2 => _color2.Value;
        public Color Color3 => _color3.Value;

        public Color this[int index] => index switch
        {
            0 => _color1.Value,
            1 => _color2.Value,
            _ => _color3.Value,
        };

        public AspidAnimatedTitleColorsStyle(
            VisualElement element,
            Color color1,
            Color color2,
            Color color3,
            Action onChanged)
        {
            _color1 = new InlineStyle<Color>(color1, (_, _) => onChanged?.Invoke());
            _color2 = new InlineStyle<Color>(color2, (_, _) => onChanged?.Invoke());
            _color3 = new InlineStyle<Color>(color3, (_, _) => onChanged?.Invoke());

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public void SetColor1(Color value) => _color1.SetInlineValue(value);

        public void SetColor2(Color value) => _color2.SetInlineValue(value);

        public void SetColor3(Color value) => _color3.SetInlineValue(value);

        public void SetDefaultColor1(Color value) => _color1.SetDefaultValue(value);

        public void SetDefaultColor2(Color value) => _color2.SetDefaultValue(value);

        public void SetDefaultColor3(Color value) => _color3.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(Color1Property, out var c1)) SetDefaultColor1(c1);
            if (evt.customStyle.TryGetValue(Color2Property, out var c2)) SetDefaultColor2(c2);
            if (evt.customStyle.TryGetValue(Color3Property, out var c3)) SetDefaultColor3(c3);
        }
    }
}
