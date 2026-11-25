#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidBox
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidBox, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlEnumAttributeDescription<ThemeStyle.Type> _theme = new() { name = "theme" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _status = new() { name = "status" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidBox)ve;

                var theme = element.Theme;
                if (_theme.TryGetValueFromBag(bag, cc: cc, value: ref theme))
                    element.Theme = theme;

                var status = element.Status;
                if (_status.TryGetValueFromBag(bag, cc: cc, value: ref status))
                    element.Status = status;
            }
        }
    }
}
#endif
