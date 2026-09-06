// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidBoxExtensions
    {
        public static AspidBox SetTheme(this AspidBox element, ThemeStyle.Type value)
        {
            element.Theme = value;
            return element;
        }

        public static AspidBox SetStatus(this AspidBox element, StatusStyle.Type value)
        {
            element.Status = value;
            return element;
        }
    }
}
