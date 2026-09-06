using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidHelpBoxExtensions
    {
        public static AspidHelpBox SetTitle(this AspidHelpBox element, string value)
        {
            element.Title = value;
            return element;
        }

        public static AspidHelpBox SetMessage(this AspidHelpBox element, string value)
        {
            element.Message = value;
            return element;
        }

        public static AspidHelpBox SetStatus(this AspidHelpBox element, StatusStyle.Type value)
        {
            element.Status = value;
            return element;
        }

        public static AspidHelpBox SetMessageType(this AspidHelpBox element, HelpBoxMessageType value)
        {
            element.MessageType = value;
            return element;
        }
    }
}
