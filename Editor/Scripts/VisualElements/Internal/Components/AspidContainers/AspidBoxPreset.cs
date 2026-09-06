// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidBoxPreset
    {
        public static AspidBoxPreset Default => new AspidBoxPreset()
            .SetTheme(ThemeStyle.Type.Light)
            .SetStatus(StatusStyle.Type.None);

        public ThemeStyle.Type Theme;

        public StatusStyle.Type Status;

        public AspidBoxPreset SetTheme(ThemeStyle.Type value)
        {
            Theme = value;
            return this;
        }

        public AspidBoxPreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            return this;
        }
    }
}
