using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the thickness of an AspidDividingLine. The size can be inherited from the StyleProperty USS custom
    // property or set explicitly in code; once set explicitly it is no longer overridden by USS resolution.
    internal readonly struct AspidDividingLineSizeStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-metrics-line_size");
        public const string ThinClass = "aspid-fasttools-dividing-line--thin";
        public const string MediumClass = "aspid-fasttools-dividing-line--medium";
        public const string BoldClass = "aspid-fasttools-dividing-line--bold";
        
        private readonly InlineStyle<Type> _value;

        public AspidDividingLineSizeStyle(AspidDividingLine element, Type value)
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
            Type.Thin => ThinClass,
            Type.Medium => MediumClass,
            Type.Bold => BoldClass,
            _ => string.Empty,
        };

        public enum Type
        {
            None,
            Thin,
            Medium,
            Bold,
        }
    }
}
