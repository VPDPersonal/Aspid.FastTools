using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Extensions that apply the Aspid editor theme (default palette plus the optional user override) to a
    // VisualElement.
    internal static class AspidThemeStyleSheetExtensions
    {
        public static T AddAspidThemeStyleSheets<T>(this T element)
            where T : VisualElement
        {
            element.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet);

            var applied = AspidThemeSettings.OverrideStyleSheet;
            if (applied != null) element.AddStyleSheets(applied);

            // Subscribed on attach and dropped on detach, so live updates survive a re-parent and an element that
            // never attaches leaks nothing.
            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                OnThemeChanged();
                AspidThemeSettings.Changed += OnThemeChanged;
            });
            element.RegisterCallback<DetachFromPanelEvent>(_ => AspidThemeSettings.Changed -= OnThemeChanged);

            return element;

            void OnThemeChanged()
            {
                if (applied != null) element.RemoveStyleSheets(applied);

                applied = AspidThemeSettings.OverrideStyleSheet;
                if (applied != null) element.AddStyleSheets(applied);
            }
        }
    }
}
