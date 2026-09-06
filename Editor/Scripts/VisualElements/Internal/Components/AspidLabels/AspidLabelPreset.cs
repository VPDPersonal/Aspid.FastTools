using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidLabelPreset
    {
        public static AspidLabelPreset Default => new AspidLabelPreset()
            .SetLabelTheme(ThemeStyle.Type.Light)
            .SetLabelSize(AspidLabelSizeStyle.Type.H5)
            .SetLine(AspidDividingLinePreset.Default)
            .SetFontStyle(UnityEngine.FontStyle.Bold);

        public bool Selectable;

        public ThemeStyle.Type Theme;

        public StatusStyle.Type Status;

        public AspidLabelSizeStyle.Type Size;

        public StyleEnum<FontStyle> FontStyle;

        public AspidDividingLinePreset Line;

        public AspidLabelPreset SetTheme(ThemeStyle.Type value)
        {
            Theme = value;
            Line.SetTheme(value);
            return this;
        }

        public AspidLabelPreset SetLineTheme(ThemeStyle.Type value)
        {
            Line.SetTheme(value);
            return this;
        }

        public AspidLabelPreset SetLabelTheme(ThemeStyle.Type value)
        {
            Theme = value;
            return this;
        }

        public AspidLabelPreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            Line.SetStatus(value);
            return this;
        }

        public AspidLabelPreset SetLineStatus(StatusStyle.Type value)
        {
            Line.SetStatus(value);
            return this;
        }

        public AspidLabelPreset SetLabelStatus(StatusStyle.Type value)
        {
            Status = value;
            return this;
        }

        public AspidLabelPreset SetLabelSize(AspidLabelSizeStyle.Type value)
        {
            Size = value;
            return this;
        }

        public AspidLabelPreset SetLine(AspidDividingLinePreset value)
        {
            Line = value;
            return this;
        }

        public AspidLabelPreset SetLineSize(AspidDividingLineSizeStyle.Type value)
        {
            Line.SetSize(value);
            return this;
        }

        public AspidLabelPreset SetLineDirection(AspidDividingLineDirectionStyle.Type value)
        {
            Line.SetDirection(value);
            return this;
        }

        public AspidLabelPreset SetFontStyle(StyleEnum<FontStyle> value)
        {
            FontStyle = value;
            return this;
        }

        public AspidLabelPreset SetSelectable(bool value = true)
        {
            Selectable = value;
            return this;
        }
    }
}
