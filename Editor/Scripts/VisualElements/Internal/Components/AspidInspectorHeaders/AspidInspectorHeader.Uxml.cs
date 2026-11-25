#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidInspectorHeader
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidInspectorHeader, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _text = new() { name = "text" };
            private readonly UxmlStringAttributeDescription _subtext = new() { name = "subtext" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _status = new() { name = "status" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidInspectorHeader)ve;

                var text = element.Text;
                if (_text.TryGetValueFromBag(bag, cc: cc, value: ref text))
                    element.Text = text;

                var subtext = element.Subtext;
                if (_subtext.TryGetValueFromBag(bag, cc: cc, value: ref subtext))
                    element.Subtext = subtext;

                var status = element.Status;
                if (_status.TryGetValueFromBag(bag, cc: cc, value: ref status))
                    element.Status = status;
            }
        }
    }
}
#endif
