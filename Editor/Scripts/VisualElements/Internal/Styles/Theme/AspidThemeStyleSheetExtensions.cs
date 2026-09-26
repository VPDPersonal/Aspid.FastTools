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
            UpdateSkinClass(element);

            // A switch of the editor skin restyles the panel, so the class follows it without a rebuild.
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            var applied = AspidThemeSettings.OverrideStyleSheet;
            if (applied != null) element.AddStyleSheet(applied);

            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                OnThemeChanged();
                AspidThemeSettings.Changed += OnThemeChanged;
            });
            element.RegisterCallback<DetachFromPanelEvent>(_ => AspidThemeSettings.Changed -= OnThemeChanged);

            return element;

            void OnThemeChanged()
            {
                if (applied != null) element.RemoveStyleSheet(applied);

                applied = AspidThemeSettings.OverrideStyleSheet;
                if (applied != null) element.AddStyleSheet(applied);
            }
        }

        private static void OnCustomStyleResolved(CustomStyleResolvedEvent evt) =>
            UpdateSkinClass((VisualElement)evt.currentTarget);

        private static void UpdateSkinClass(VisualElement element) =>
            element.EnableInClassList(AspidStyles.SkinLightClass, !EditorGUIUtility.isProSkin);
    }
}
