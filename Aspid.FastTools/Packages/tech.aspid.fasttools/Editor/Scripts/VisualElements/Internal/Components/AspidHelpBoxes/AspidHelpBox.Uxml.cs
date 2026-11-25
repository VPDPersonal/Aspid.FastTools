#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidHelpBox
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidHelpBox, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _title = new() { name = "title" };
            private readonly UxmlStringAttributeDescription _message = new() { name = "message" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _status = new() { name = "status" };
            private readonly UxmlEnumAttributeDescription<HelpBoxMessageType> _messageType = new() { name = "message-type" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidHelpBox)ve;

                var title = element.Title;
                if (_title.TryGetValueFromBag(bag, cc: cc, value: ref title))
                    element.Title = title;

                var message = element.Message;
                if (_message.TryGetValueFromBag(bag, cc: cc, value: ref message))
                    element.Message = message;

                var status = element.Status;
                if (_status.TryGetValueFromBag(bag, cc: cc, value: ref status))
                    element.Status = status;

                var messageType = element.MessageType;
                if (_messageType.TryGetValueFromBag(bag, cc: cc, value: ref messageType))
                    element.MessageType = messageType;
            }
        }
    }
}
#endif
