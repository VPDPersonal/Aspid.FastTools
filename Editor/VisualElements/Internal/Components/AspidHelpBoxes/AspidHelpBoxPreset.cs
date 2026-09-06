using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidHelpBoxPreset
    {
        public static AspidHelpBoxPreset Default => new AspidHelpBoxPreset()
            .SetTitle(new AspidLabelPreset()
                .SetSelectable()
                .SetLineTheme(ThemeStyle.Type.Light)
                .SetLabelTheme(ThemeStyle.Type.Light)
                .SetLineSize(AspidDividingLineSizeStyle.Type.Thin)
                .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                .SetFontStyle(FontStyle.Bold))
            .SetMessage(new AspidLabelPreset()
                .SetSelectable()
                .SetLabelTheme(ThemeStyle.Type.Dark)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None)
                .SetLabelSize(AspidLabelSizeStyle.Type.H7));

        public StatusStyle.Type Status;

        public AspidLabelPreset TitlePreset;

        public AspidLabelPreset MessagePreset;

        public HelpBoxMessageType MessageType;

        public AspidHelpBoxPreset SetTitle(AspidLabelPreset value)
        {
            TitlePreset = value;
            return this;
        }

        public AspidHelpBoxPreset SetMessage(AspidLabelPreset value)
        {
            MessagePreset = value;
            return this;
        }

        public AspidHelpBoxPreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            TitlePreset = TitlePreset.SetStatus(value);
            MessagePreset = MessagePreset.SetStatus(value);
            return this;
        }

        public AspidHelpBoxPreset SetMessageType(HelpBoxMessageType value)
        {
            MessageType = value;
            if (Status == StatusStyle.Type.None) SetStatus(MapToStatus(value));
            return this;
        }

        private static StatusStyle.Type MapToStatus(HelpBoxMessageType type) => type switch
        {
            HelpBoxMessageType.Info => StatusStyle.Type.Info,
            HelpBoxMessageType.Warning => StatusStyle.Type.Warning,
            HelpBoxMessageType.Error => StatusStyle.Type.Error,
            _ => StatusStyle.Type.None,
        };
    }
}
