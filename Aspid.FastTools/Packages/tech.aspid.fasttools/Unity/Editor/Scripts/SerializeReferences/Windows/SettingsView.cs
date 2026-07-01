using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The Settings tab (the rightmost square tab): the package's project-wide settings surfaced inside the window so
    /// the toolset is self-contained. It groups the controls under titled sections — the first, "References", renders
    /// the very same SerializeReference controls as the Project Settings page (both call
    /// <see cref="SerializeReferenceSettingsUI.BuildControls"/>, bound to <see cref="SerializeReferenceSettings"/>, so
    /// there is one definition and no duplicated UI). Further package areas (e.g. TypeSelector) add their own section.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        // Scopes the dark-theme overrides to this in-window surface only — the Project Settings page that shares
        // SerializeReferenceSettingsUI.BuildControls never adds the class, so its native Unity look is preserved.
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Settings";
        private const string RootClass = "aspid-fasttools-serialize-reference-settings";
        private const string SectionTitleClass = "aspid-fasttools-serialize-reference-settings__section-title";
        private const string SectionContentClass = "aspid-fasttools-serialize-reference-settings__section-content";

        public SettingsView()
        {
            // Repaint the plain Unity fields (EnumField popup / Toggles) into the window's dark palette so they don't
            // read as Unity's bright default inputs floating over the black dotted canvas.
            this.AddStyleSheetsFromResource(StyleSheetPath).AddClass(RootClass);

            // No in-content "Settings" heading: the active gear tab already names the mode. Instead the controls are
            // grouped under per-area section titles, so the tab scales as more package areas add their settings here.
            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            AddSection(scroll.contentContainer, "References", SerializeReferenceSettingsUI.BuildControls);
            Add(scroll);
        }

        // Appends a titled settings section: a header label over a content container that <paramref name="buildContent"/>
        // fills. Each package area (References now, TypeSelector and others later) gets its own section here so the tab
        // reads as one grouped surface from a single definition per area.
        private static void AddSection(VisualElement container, string title, Action<VisualElement> buildContent)
        {
            container.Add(new Label(title).AddClass(SectionTitleClass));

            var content = new VisualElement().AddClass(SectionContentClass);
            buildContent(content);
            container.Add(content);
        }
    }
}
