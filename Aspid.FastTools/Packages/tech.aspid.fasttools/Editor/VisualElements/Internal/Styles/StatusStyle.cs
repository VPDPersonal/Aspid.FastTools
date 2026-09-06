using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Manages the status accent (success / warning / error / info) of a VisualElement. The status can be inherited
    // from the StyleProperty USS custom property or set explicitly in code; once set explicitly it is no longer
    // overridden by the CustomStyleResolvedEvent.
    internal readonly struct StatusStyle
    {
        public static readonly CustomStyleProperty<string> StyleProperty = new("--aspid-fasttools-prop-status");
        public const string SuccessClass = "aspid-fasttools-status--success";
        public const string WarningClass = "aspid-fasttools-status--warning";
        public const string ErrorClass = "aspid-fasttools-status--error";
        public const string InfoClass = "aspid-fasttools-status--info";

        private readonly InlineStyle<Type> _value;

        public StatusStyle(VisualElement element, Type type = Type.None)
        {
            _value = new InlineStyle<Type>(type, (oldValue, newValue) =>
            {
                var oldClass = GetClass(oldValue);
                var newClass = GetClass(newValue);

                if (!string.IsNullOrWhiteSpace(oldClass)) element.RemoveClass(oldClass);
                if (!string.IsNullOrWhiteSpace(newClass)) element.AddClass(newClass);
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

        public static string GetClass(Type status) => status switch
        {
            Type.Success => SuccessClass,
            Type.Warning => WarningClass,
            Type.Error => ErrorClass,
            Type.Info => InfoClass,
            _ => string.Empty,
        };

        public enum Type
        {
            None,
            Info,
            Warning,
            Error,
            Success,
        }
    }
}
