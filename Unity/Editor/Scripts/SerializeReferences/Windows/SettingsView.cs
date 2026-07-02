using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The Settings tab (the rightmost square tab): the package's settings surfaced inside the window so the toolset is
    /// self-contained. A thin host over the shared settings surface — <see cref="AspidSettingsUI.BuildSurfaceContent"/>
    /// composes the scope legend and every package area's section from one definition per area (the same composition
    /// the Preferences page renders), and <see cref="AspidSettingsUI.BuildResetFooter"/> pins the per-scope reset
    /// under the scroll. The window's dotted canvas already sits behind this tab, so unlike the Preferences page it
    /// brings no canvas of its own.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        public SettingsView()
        {
            // Repaint the plain Unity fields (EnumField popup / sliders / object fields) into the window's dark
            // palette so they don't read as Unity's bright default inputs floating over the black dotted canvas.
            this.AsSurface();

            // No in-content "Settings" heading: the active gear tab already names the mode.
            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            AspidSettingsUI.BuildSurfaceContent(scroll.contentContainer);
            Add(scroll);

            // Pinned under the scroll, so the reset affordance stays reachable however long the tab grows.
            Add(AspidSettingsUI.BuildResetFooter());
        }
    }
}
