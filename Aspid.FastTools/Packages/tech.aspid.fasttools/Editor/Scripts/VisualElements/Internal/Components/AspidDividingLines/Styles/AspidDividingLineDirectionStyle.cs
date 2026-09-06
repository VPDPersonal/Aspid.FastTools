using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the orientation of an AspidDividingLine. The direction can be inherited from the StyleProperty USS
    // custom property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidDividingLineDirectionStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-prop-line_direction");
        public const string HorizontalClass = "aspid-fasttools-dividing-line--horizontal";
        public const string VerticalClass = "aspid-fasttools-dividing-line--vertical";

        private readonly InlineStyle<Type> _value;

        public AspidDividingLineDirectionStyle(AspidDividingLine element, Type value)
        {
            _value = new InlineStyle<Type>(value, (oldValue, newValue) =>
            {
                element
                    .RemoveClass(GetClass(oldValue))
                    .AddClass(GetClass(newValue));
            });

            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public Type Value => _value;

        public void SetValue(Type value) =>
            _value.SetInlineValue(value);

        public void SetDefaultValue(Type value) =>
            _value.SetDefaultValue(value);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetByEnum<Type>(StyleProperty, out var value))
                SetDefaultValue(value);
        }

        public static string GetClass(Type type) => type switch
        {
            Type.Horizontal => HorizontalClass,
            Type.Vertical => VerticalClass,
            _ => string.Empty,
        };

        public enum Type
        {
            Horizontal,
            Vertical,
        }
    }
}
