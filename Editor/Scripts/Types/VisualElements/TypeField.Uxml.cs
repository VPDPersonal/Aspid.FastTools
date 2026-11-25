#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    public partial class TypeField
    {
        internal new sealed class UxmlFactory : UxmlFactory<TypeField, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new() { name = "label" };
            private readonly UxmlEnumAttributeDescription<TypeAllow> _allow = new() { name = "allow" };
            private readonly UxmlBoolAttributeDescription _isReadOnly = new() { name = "is-read-only" };
            private readonly UxmlBoolAttributeDescription _hideNoneOption = new() { name = "hide-none-option" };
            private readonly UxmlBoolAttributeDescription _excludeEditorOnlyTypes = new() { name = "exclude-editor-only-types" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (TypeField)ve;

                var label = element.label;
                if (_label.TryGetValueFromBag(bag, cc: cc, value: ref label))
                    element.label = label;

                var allow = element.Allow;
                if (_allow.TryGetValueFromBag(bag, cc: cc, value: ref allow))
                    element.Allow = allow;

                var hideNoneOption = element.HideNoneOption;
                if (_hideNoneOption.TryGetValueFromBag(bag, cc: cc, value: ref hideNoneOption))
                    element.HideNoneOption = hideNoneOption;

                var excludeEditorOnlyTypes = element.ExcludeEditorOnlyTypes;
                if (_excludeEditorOnlyTypes.TryGetValueFromBag(bag, cc: cc, value: ref excludeEditorOnlyTypes))
                    element.ExcludeEditorOnlyTypes = excludeEditorOnlyTypes;

                var isReadOnly = element.IsReadOnly;
                if (_isReadOnly.TryGetValueFromBag(bag, cc: cc, value: ref isReadOnly))
                    element.IsReadOnly = isReadOnly;
            }
        }
    }
}
#endif
