using System;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The Settings tab (the rightmost square tab): the package's settings surfaced inside the window so the toolset is
    /// self-contained. It groups the controls under titled per-area sections — "References" renders the very same
    /// SerializeReference controls as the Project Settings page (both call
    /// <see cref="SerializeReferenceSettingsUI.BuildControls"/>, bound to <see cref="SerializeReferenceSettings"/>, so
    /// there is one definition and no duplicated UI), and "Type Selector" hosts the picker's per-user preferences
    /// (<see cref="TypeSelectorSettingsUI"/>). Because the tab mixes team-wide and individual settings, every row is
    /// striped by its storage scope (<see cref="SharedScopeClass"/> / <see cref="UserScopeClass"/>) and a compact
    /// legend at the top decodes the two colours.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        // Scopes the dark-theme overrides to this in-window surface only — the Project Settings page that shares
        // SerializeReferenceSettingsUI.BuildControls never adds the class, so its native Unity look is preserved.
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Settings";
        private const string RootClass = "aspid-fasttools-serialize-reference-settings";
        private const string SectionTitleClass = "aspid-fasttools-serialize-reference-settings__section-title";
        private const string SectionContentClass = "aspid-fasttools-serialize-reference-settings__section-content";
        private const string LegendClass = "aspid-fasttools-serialize-reference-settings__legend";
        private const string LegendItemClass = "aspid-fasttools-serialize-reference-settings__legend-item";
        private const string LegendSwatchClass = "aspid-fasttools-serialize-reference-settings__legend-swatch";
        private const string LegendTextClass = "aspid-fasttools-serialize-reference-settings__legend-text";
        private const string FooterClass = "aspid-fasttools-serialize-reference-settings__footer";

        // Storage-scope markers, applied by the per-area builders to every settings row (and styled only inside this
        // window — on the Project Settings page they are inert). A utility block of its own, like the status/theme
        // classes, since the scope of a setting is not a part of this view.
        public const string SharedScopeClass = "aspid-fasttools-settings-scope--shared";
        public const string UserScopeClass = "aspid-fasttools-settings-scope--user";

        // Row primitives for builders that need a non-BaseField settings row (e.g. an action row of buttons): the row
        // reads as the same card as the field rows, the caption as the same left-hand label, and the action as a small
        // framed button. The danger modifier gives a destructive action the red hover family; the info modifier gives
        // an action tied to the per-user scope the matching blue one.
        public const string RowClass = "aspid-fasttools-serialize-reference-settings__row";
        public const string RowCaptionClass = "aspid-fasttools-serialize-reference-settings__row-caption";
        public const string ActionClass = "aspid-fasttools-serialize-reference-settings__action";
        public const string ActionDangerClass = "aspid-fasttools-serialize-reference-settings__action--danger";
        public const string ActionInfoClass = "aspid-fasttools-serialize-reference-settings__action--info";

        public SettingsView()
        {
            // Repaint the plain Unity fields (EnumField popup / Toggles) into the window's dark palette so they don't
            // read as Unity's bright default inputs floating over the black dotted canvas.
            this.AddStyleSheetsFromResource(StyleSheetPath).AddClass(RootClass);

            // No in-content "Settings" heading: the active gear tab already names the mode. Instead the controls are
            // grouped under per-area section titles, so the tab scales as more package areas add their settings here.
            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            scroll.contentContainer.Add(BuildScopeLegend());
            AddSection(scroll.contentContainer, "References", SerializeReferenceSettingsUI.BuildControls);
            AddSection(scroll.contentContainer, "Type Selector", TypeSelectorSettingsUI.BuildControls);
            Add(scroll);

            // Pinned under the scroll, so the reset affordance stays reachable however long the tab grows.
            Add(BuildResetFooter());
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

        // The one-line key to the rows' scope stripes: each item pairs a swatch painted by the same scope class the
        // rows wear with a caption naming what that colour means for persistence.
        private static VisualElement BuildScopeLegend()
        {
            var legend = new VisualElement().AddClass(LegendClass);
            legend.Add(BuildLegendItem(SharedScopeClass, "Shared — committed, same for the whole team"));
            legend.Add(BuildLegendItem(UserScopeClass, "Per-user — stored locally, just for you"));
            return legend;
        }

        private static VisualElement BuildLegendItem(string scopeClass, string caption)
        {
            return new VisualElement().AddClass(LegendItemClass)
                .AddChild(new VisualElement().AddClass(LegendSwatchClass).AddClass(scopeClass))
                .AddChild(new Label(caption).AddClass(LegendTextClass));
        }

        // The tab's footer: one row with a reset button per storage scope, each wearing its scope's stripe (the same
        // classes the settings rows wear) and confirming with the exact default values before touching anything. Two
        // separate buttons — not one — because the scopes have different blast radii: the shared reset changes the
        // committed asset for the whole team, the per-user one only this machine's EditorPrefs.
        private static VisualElement BuildResetFooter()
        {
            var caption = new Label("Reset to defaults").AddClass(RowCaptionClass);

            var shared = new Button(ResetSharedToDefaults)
            {
                text = "Shared",
                tooltip = "Reset the team-wide settings to defaults: Auto de-alias on, Build / CI gate Warn, no excluded scan folders.\n"
                    + "Changes the committed ProjectSettings asset — affects every teammate once committed.",
            };
            shared.AddClass(ActionClass).AddClass(SharedScopeClass);

            var user = new Button(ResetUserToDefaults)
            {
                text = "Per-user",
                tooltip = "Reset your per-user settings to defaults: Breakage detection on, Favorites section on, Recent items 8.\n"
                    + "Only this machine; the saved Favorites / Recent lists are kept.",
            };
            user.AddClass(ActionClass).AddClass(ActionInfoClass).AddClass(UserScopeClass);

            var row = new VisualElement().AddClass(RowClass)
                .AddChild(caption)
                .AddChild(shared)
                .AddChild(user);

            return new VisualElement().AddClass(FooterClass).AddChild(row);
        }

        private static void ResetSharedToDefaults()
        {
            var confirmed = EditorUtility.DisplayDialog(
                "Reset shared settings",
                "Reset the team-wide settings to defaults?\n\n"
                + "• Auto de-alias duplicated list elements: On\n"
                + "• Build / CI gate: Warn\n"
                + "• Excluded scan folders: none\n\n"
                + "This edits the committed ProjectSettings asset, so it affects the whole team once committed.",
                "Reset",
                "Cancel");

            if (confirmed) SerializeReferenceSettings.ResetSharedToDefaults();
        }

        private static void ResetUserToDefaults()
        {
            var confirmed = EditorUtility.DisplayDialog(
                "Reset per-user settings",
                "Reset your per-user settings to defaults?\n\n"
                + "• Breakage detection: On\n"
                + "• Favorites section: On\n"
                + "• Recent items: 8\n\n"
                + "Only this machine is affected; the saved Favorites / Recent lists are kept.",
                "Reset",
                "Cancel");

            if (!confirmed) return;

            SerializeReferenceSettings.ResetUserToDefaults();
            TypeSelectorSettings.ResetToDefaults();
        }
    }
}
