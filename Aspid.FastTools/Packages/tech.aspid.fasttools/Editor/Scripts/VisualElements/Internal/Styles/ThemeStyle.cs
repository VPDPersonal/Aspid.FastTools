using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the theme (background brightness variant) of a VisualElement. The theme can be inherited from the
    // StyleProperty USS custom property or set explicitly in code; once set explicitly it is no longer overridden by
    // the CustomStyleResolvedEvent.
    internal readonly struct ThemeStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-prop-theme");
        public const string DarknessClass = "aspid-fasttools-theme--darkness";
        public const string DarkClass = "aspid-fasttools-theme--dark";
        public const string LightClass = "aspid-fasttools-theme--light";
        public const string LightnessClass = "aspid-fasttools-theme--lightness";

        private readonly InlineStyle<Type> _value;

        public ThemeStyle(VisualElement element, Type type = Type.Light)
        {
            _value = new InlineStyle<Type>(type, (oldValue, newValue) =>
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

        public static string GetClass(Type theme) => theme switch
        {
            Type.Darkness => DarknessClass,
            Type.Dark => DarkClass,
            Type.Light => LightClass,
            Type.Lightness => LightnessClass,
            _ => string.Empty,
        };

        public enum Type
        {
            Darkness,
            Dark,
            Light,
            Lightness,
        }
    }
}
