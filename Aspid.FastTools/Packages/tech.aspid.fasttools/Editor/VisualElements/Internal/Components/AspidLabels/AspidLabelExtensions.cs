// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidLabelExtensions
    {
        public static AspidLabel SetText(this AspidLabel element, string value)
        {
            element.Text = value;
            return element;
        }
        
        public static AspidLabel SetSelectable(this AspidLabel element, bool value)
        {
            element.Selectable = value;
            return element;
        }

        public static AspidLabel SetLabelTheme(this AspidLabel element, ThemeStyle.Type value)
        {
            element.LabelTheme = value;
            return element;
        }

        public static AspidLabel SetLabelStatus(this AspidLabel element, StatusStyle.Type value)
        {
            element.LabelStatus = value;
            return element;
        }

        public static AspidLabel SetLabelSize(this AspidLabel element, AspidLabelSizeStyle.Type value)
        {
            element.LabelSize = value;
            return element;
        }

        public static AspidLabel SetLineTheme(this AspidLabel element, ThemeStyle.Type value)
        {
            element.LineTheme = value;
            return element;
        }

        public static AspidLabel SetLineStatus(this AspidLabel element, StatusStyle.Type value)
        {
            element.LineStatus = value;
            return element;
        }

        public static AspidLabel SetLineSize(this AspidLabel element, AspidDividingLineSizeStyle.Type value)
        {
            element.LineSize = value;
            return element;
        }
    }
}
