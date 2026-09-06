// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidDividingLineExtensions
    {
        public static AspidDividingLine SetTheme(this AspidDividingLine element, ThemeStyle.Type value)
        {
            element.Theme = value;
            return element;
        }

        public static AspidDividingLine SetStatus(this AspidDividingLine element, StatusStyle.Type value)
        {
            element.Status = value;
            return element;
        }

        public static AspidDividingLine SetSize(this AspidDividingLine element, AspidDividingLineSizeStyle.Type value)
        {
            element.Size = value;
            return element;
        }

        public static AspidDividingLine SetDirection(this AspidDividingLine element, AspidDividingLineDirectionStyle.Type value)
        {
            element.Direction = value;
            return element;
        }
    }
}
