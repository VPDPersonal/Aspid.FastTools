using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// The package's settings page at <c>Preferences → Aspid FastTools</c> — a full mirror of the window's Settings
    /// tab, rendered over its own dotted canvas (the same backdrop as the window, in the Settings tab's calm idle
    /// tone). <see cref="AspidSettingsUI.BuildSurfaceContent"/> composes the identical legend, sections and controls
    /// (the scope stripes — not the page — say what is committed and what is local, and the SerializeReference
    /// Project Settings page still exposes the References controls in Unity's native look), and the per-scope reset
    /// footer is pinned under the scroll just like in the window. Every surface mirrors the others live from one
    /// definition per control. Supersedes the old theme-only provider at this path; its controls now form the
    /// Appearance section.
    /// </summary>
    internal static class AspidFastToolsPreferencesProvider
    {
        private const string SettingsPath = "Preferences/Aspid FastTools";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                label = "Aspid FastTools",
                activateHandler = static (_, root) => BuildUI(root),
                keywords = new[]
                {
                    "Aspid", "FastTools", "Theme", "Style", "USS", "Color", "Palette", "Override",
                    "Type Selector", "Favorites", "Recent", "Breakage", "Welcome",
                },
            };
        }

        private static void BuildUI(VisualElement root)
        {
            // The theme sheets make a user override apply to this page too.
            root.AddAspidThemeStyleSheets();
            root.style.flexGrow = 1;

            // The branded cards read wrong over Unity's native grey panel, so the page brings its own backdrop (the
            // window's dotted canvas). The host loads the surface sheet so canvas and card rules reach the subtree.
            var host = new VisualElement()
                .AddStyleSheetsFromResource(AspidSettingsUI.StyleSheetPath)
                .AddClass(AspidSettingsUI.CanvasClass);

            var canvas = new AspidAnimatedDotsBackground()
                .AddClass(AspidSettingsUI.CanvasBackgroundClass)
                .SetPickingMode(PickingMode.Ignore);
            canvas.SetTone(SerializeReferenceCanvasStyle.Info);

            var surface = new VisualElement().AddClass(AspidSettingsUI.RootClass);
            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            AspidSettingsUI.BuildSurfaceContent(scroll.contentContainer);

            surface.Add(scroll);
            surface.Add(AspidSettingsUI.BuildResetFooter());
            host.AddChild(canvas).AddChild(surface);
            root.Add(host);
        }
    }
}
