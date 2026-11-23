#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidDividingLine
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidDividingLine, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlEnumAttributeDescription<ThemeStyle.Type> _theme = new() { name = "theme" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _status = new() { name = "status" };
            private readonly UxmlEnumAttributeDescription<AspidDividingLineSizeStyle.Type> _size = new() { name = "size" };
            private readonly UxmlEnumAttributeDescription<AspidDividingLineDirectionStyle.Type> _direction = new() { name = "direction" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidDividingLine)ve;

                var theme = element.Theme;
                if (_theme.TryGetValueFromBag(bag, cc: cc, value: ref theme))
                    element.Theme = theme;

                var status = element.Status;
                if (_status.TryGetValueFromBag(bag, cc: cc, value: ref status))
                    element.Status = status;

                var size = element.Size;
                if (_size.TryGetValueFromBag(bag, cc: cc, value: ref size))
                    element.Size = size;

                var direction = element.Direction;
                if (_direction.TryGetValueFromBag(bag, cc: cc, value: ref direction))
                    element.Direction = direction;
            }
        }
    }
}
#endif
