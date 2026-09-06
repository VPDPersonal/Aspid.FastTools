using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the font size of an AspidLabel. The size can be inherited from the StyleProperty USS custom property or
    // set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidLabelSizeStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-metrics-label_size");
        public const string H1Class = "aspid-fasttools-label-size--h1";
        public const string H2Class = "aspid-fasttools-label-size--h2";
        public const string H3Class = "aspid-fasttools-label-size--h3";
        public const string H4Class = "aspid-fasttools-label-size--h4";
        public const string H5Class = "aspid-fasttools-label-size--h5";
        public const string H6Class = "aspid-fasttools-label-size--h6";
        public const string H7Class = "aspid-fasttools-label-size--h7";

        private readonly InlineStyle<Type> _value;

        public AspidLabelSizeStyle(AspidLabel element, Type value)
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
            Type.H1 => H1Class,
            Type.H2 => H2Class,
            Type.H3 => H3Class,
            Type.H4 => H4Class,
            Type.H5 => H5Class,
            Type.H6 => H6Class,
            Type.H7 => H7Class,
            _ => string.Empty,
        };

        public enum Type
        {
            None = 0,
            H1 = 36,
            H2 = 24,
            H3 = 18,
            H4 = 16,
            H5 = 14,
            H6 = 13,
            H7 = 12,
        }
    }
}
