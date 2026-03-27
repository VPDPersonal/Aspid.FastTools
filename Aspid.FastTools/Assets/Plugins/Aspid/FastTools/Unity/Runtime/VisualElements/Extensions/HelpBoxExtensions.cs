using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools
{
    public static class HelpBoxExtensions
    {
        /// <summary>
        /// Sets the font size of the label inside the help box.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetHelpBoxFontSize<T>(this T helpBox, StyleLength value)
            where T : HelpBox
        {
            helpBox.Q<Label>().SetFontSize(value);
            return helpBox;
        }

        /// <summary>
        /// Sets the message type (icon) of the help box.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMessageType<T>(this T helpBox, int size, HelpBoxMessageType value)
            where T : HelpBox
        {
            helpBox.messageType = value;
            return helpBox;
        }
    }
}