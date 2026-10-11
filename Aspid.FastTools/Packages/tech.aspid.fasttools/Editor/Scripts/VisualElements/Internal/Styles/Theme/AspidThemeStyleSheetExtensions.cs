using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeStyleSheetExtensions
    {
        public static T AddAspidThemeStyleSheets<T>(this T element)
            where T : VisualElement
        {
            element.AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet);
            element.AddStyleSheetFromResources(AspidStyles.LightStyleSheet);
            UpdateSkinClass(element);

            // A switch of the editor skin restyles the panel, so the class follows it without a rebuild.
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            var applied = AspidThemeSettings.OverrideStyleSheet;
            if (applied != null) element.AddStyleSheet(applied);

            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                UpdateSkinClass(element);
                OnThemeChanged();
                Subscribe();
            });
            element.RegisterCallback<DetachFromPanelEvent>(_ => AspidThemeSettings.Changed -= OnThemeChanged);

            // An element already in a panel gets no attach event, so it subscribes now.
            if (element.panel is not null) Subscribe();

            return element;

            void Subscribe()
            {
                AspidThemeSettings.Changed -= OnThemeChanged;
                AspidThemeSettings.Changed += OnThemeChanged;
            }

            void OnThemeChanged()
            {
                if (applied != null) element.RemoveStyleSheet(applied);

                applied = AspidThemeSettings.OverrideStyleSheet;
                if (applied != null) element.AddStyleSheet(applied);
            }
        }

        private static void OnCustomStyleResolved(CustomStyleResolvedEvent evt) =>
            UpdateSkinClass((VisualElement)evt.currentTarget);

        // Every element that declares the palette takes the light one, not only the window root: each redeclares
        // Default-Dark on itself, and an inherited value would lose to it.
        private static void UpdateSkinClass(VisualElement element)
        {
            var lightSkin = !EditorGUIUtility.isProSkin;
            element.EnableInClassList(AspidStyles.SkinLightClass, lightSkin);
            element.EnableInClassList(AspidStyles.PaletteLightClass, lightSkin && IsInWindow(element));
        }

        private static bool IsInWindow(VisualElement element)
        {
            for (var current = element; current is not null; current = current.hierarchy.parent)
            {
                if (current.ClassListContains(AspidStyles.WindowClass)) return true;
            }

            return false;
        }
    }
}
