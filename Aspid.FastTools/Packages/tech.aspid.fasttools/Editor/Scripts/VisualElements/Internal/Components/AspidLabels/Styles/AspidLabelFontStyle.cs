using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the font style and weight of an AspidLabel. The value can be inherited from the StyleProperty USS custom
    // property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidLabelFontStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-metrics-label_font_style");

        private readonly InlineStyle<StyleEnum<FontStyle>> _value;

        public AspidLabelFontStyle(AspidLabel element, StyleEnum<FontStyle> value)
        {
            _value = new InlineStyle<StyleEnum<FontStyle>>(value, (_, newValue) =>
            {
                element.style.SetUnityFontStyleAndWeight(newValue);
            });

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public StyleEnum<FontStyle> Value => _value;

        public void SetValue(StyleEnum<FontStyle> value) =>
            _value.SetInlineValue(value);

        public void SetDefaultValue(StyleEnum<FontStyle> value) =>
            _value.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetByEnum<FontStyle>(StyleProperty, out var value))
                SetDefaultValue(value);
        }
    }
}
