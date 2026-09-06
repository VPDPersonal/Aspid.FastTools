// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidDividingLinePreset
    {
        public static AspidDividingLinePreset Default => new AspidDividingLinePreset()
            .SetTheme(ThemeStyle.Type.Light)
            .SetStatus(StatusStyle.Type.None)
            .SetSize(AspidDividingLineSizeStyle.Type.Medium)
            .SetDirection(AspidDividingLineDirectionStyle.Type.Horizontal);

        public ThemeStyle.Type Theme;

        public StatusStyle.Type Status;

        public AspidDividingLineSizeStyle.Type Size;

        public AspidDividingLineDirectionStyle.Type Direction;

        public AspidDividingLinePreset SetTheme(ThemeStyle.Type value)
        {
            Theme = value;
            return this;
        }

        public AspidDividingLinePreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            return this;
        }

        public AspidDividingLinePreset SetSize(AspidDividingLineSizeStyle.Type value)
        {
            Size = value;
            return this;
        }

        public AspidDividingLinePreset SetDirection(AspidDividingLineDirectionStyle.Type value)
        {
            Direction = value;
            return this;
        }
    }
}
