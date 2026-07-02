using System;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.SerializeReferences.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// The shared vocabulary and composition of the package's settings surfaces — the SerializeReference window's
    /// Settings tab and the <c>Preferences → Aspid FastTools</c> page. Owns the surface's stylesheet path and USS
    /// class names (sections, legend, row/action primitives, and the storage-scope markers), the one definition of
    /// the surface's content (<see cref="BuildSurfaceContent"/>: the legend and every package area's section) and its
    /// per-scope reset footer (<see cref="BuildResetFooter"/>), and the live-sync helper every per-area settings
    /// builder wires its controls with. Keeping these here — rather than on one view — is what keeps the two surfaces
    /// identical and lets each package area define its controls once.
    /// </summary>
    internal static class AspidSettingsUI
    {
        /// <summary>The settings surface stylesheet — dark-branded cards, scoped under <see cref="RootClass"/>.</summary>
        public const string StyleSheetPath = "UI/Windows/Aspid-FastTools-Settings";

        public const string RootClass = "aspid-fasttools-settings";

        // The standalone canvas pair: a page that hosts the surface outside the SerializeReference window (whose
        // window owns its own dotted canvas) wraps it in a host carrying these — the host fills the page and the
        // dots component gets the black base the dots paint over.
        public const string CanvasClass = "aspid-fasttools-settings-canvas";
        public const string CanvasBackgroundClass = "aspid-fasttools-settings-canvas__background";
        public const string SectionTitleClass = "aspid-fasttools-settings__section-title";
        public const string SectionContentClass = "aspid-fasttools-settings__section-content";
        public const string LegendClass = "aspid-fasttools-settings__legend";
        public const string LegendItemClass = "aspid-fasttools-settings__legend-item";
        public const string LegendSwatchClass = "aspid-fasttools-settings__legend-swatch";
        public const string LegendTextClass = "aspid-fasttools-settings__legend-text";
        public const string FooterClass = "aspid-fasttools-settings__footer";

        // Storage-scope markers, applied by the per-area builders to every settings row. A utility block of its own,
        // like the status/theme classes, since the scope of a setting is not a part of any one surface.
        public const string SharedScopeClass = "aspid-fasttools-settings-scope--shared";
        public const string UserScopeClass = "aspid-fasttools-settings-scope--user";

        // Row primitives for builders that need a non-BaseField settings row (e.g. an action row of buttons): the row
        // reads as the same card as the field rows, the caption as the same left-hand label, and the action as a small
        // framed button. The danger modifier gives a destructive action the red hover family; the info modifier gives
        // an action tied to the per-user scope the matching blue one.
        public const string RowClass = "aspid-fasttools-settings__row";
        public const string RowCaptionClass = "aspid-fasttools-settings__row-caption";
        public const string ActionClass = "aspid-fasttools-settings__action";
        public const string ActionDangerClass = "aspid-fasttools-settings__action--danger";
        public const string ActionInfoClass = "aspid-fasttools-settings__action--info";

        /// <summary>
        /// Dresses <paramref name="element"/> as a settings surface: loads the shared stylesheet and applies
        /// <see cref="RootClass"/>, under which every rule in the sheet is scoped.
        /// </summary>
        public static T AsSurface<T>(this T element) where T : VisualElement =>
            element.AddStyleSheetsFromResource(StyleSheetPath).AddClass(RootClass);

        /// <summary>
        /// Fills <paramref name="container"/> with the settings surface's whole content — the scope legend followed by
        /// every package area's section, one definition per area. Both surfaces call this, so the window tab and the
        /// Preferences page always list the same controls and mirror each other live.
        /// </summary>
        public static void BuildSurfaceContent(VisualElement container)
        {
            container.Add(BuildScopeLegend());
            AddSection(container, "References", SerializeReferenceSettingsUI.BuildControls);
            AddSection(container, "Type Selector", TypeSelectorSettingsUI.BuildControls);
            AddSection(container, "Appearance", AspidThemeSettingsUI.BuildControls);
            AddSection(container, "Welcome", WelcomeSettingsUI.BuildControls);
        }

        /// <summary>
        /// Appends a titled settings section: a header label over a content container that
        /// <paramref name="buildContent"/> fills. Each package area (References, Type Selector, Appearance, Welcome)
        /// gets its own section so a surface reads as one grouped composition from a single definition per area.
        /// </summary>
        public static void AddSection(VisualElement container, string title, Action<VisualElement> buildContent)
        {
            container.Add(new Label(title).AddClass(SectionTitleClass));

            var content = new VisualElement().AddClass(SectionContentClass);
            buildContent(content);
            container.Add(content);
        }

        /// <summary>
        /// The one-line key to the rows' scope stripes: each item pairs a swatch painted by the same scope class the
        /// rows wear with a caption naming what that colour means for persistence.
        /// </summary>
        public static VisualElement BuildScopeLegend()
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

        /// <summary>
        /// The surface's footer: one row with a reset button per storage scope, each wearing its scope's stripe (the
        /// same classes the settings rows wear) and confirming with the exact default values before touching anything.
        /// Two separate buttons — not one — because the scopes have different blast radii: the shared reset changes
        /// the committed asset for the whole team, the per-user one only this machine's EditorPrefs. Pinned by the
        /// caller under its scroll, so the affordance stays reachable however long the surface grows.
        /// </summary>
        public static VisualElement BuildResetFooter()
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
                tooltip = "Reset your per-user settings to defaults: Breakage detection on, dropdown without [TypeSelector] off, "
                    + $"Favorites section on, Recent items {TypeSelectorSettings.DefaultRecentsCapacity}, no theme override, auto-show Welcome on.\n"
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
                + "• Dropdown without [TypeSelector]: Off\n"
                + "• Favorites section: On\n"
                + $"• Recent items: {TypeSelectorSettings.DefaultRecentsCapacity}\n"
                + "• Theme override: none\n"
                + "• Auto-show Welcome: On\n\n"
                + "Only this machine is affected; the saved Favorites / Recent lists are kept.",
                "Reset",
                "Cancel");

            if (!confirmed) return;

            SerializeReferenceSettings.ResetUserToDefaults();
            TypeSelectorSettings.ResetToDefaults();
            AspidThemeSettings.OverrideStyleSheet = null;
            WelcomeSettings.ResetToDefaults();
        }

        /// <summary>
        /// Keeps <paramref name="control"/> in lock-step with a settings store so every surface rendering the same
        /// control mirrors the others live: on each store change signal the control re-reads its backing value through
        /// <paramref name="read"/> <i>without notifying</i>, so it never writes back or loops. The control the user is
        /// actively editing is skipped, so an in-progress edit is never normalized out from under the cursor. The
        /// store's change event is passed as a <paramref name="subscribe"/> / <paramref name="unsubscribe"/> pair
        /// (events can't travel as values); the subscription follows the panel lifecycle so a closed surface leaks
        /// nothing and a re-parented one keeps mirroring.
        /// </summary>
        public static void SyncFromSettings<TControl, TValue>(
            TControl control,
            Func<TValue> read,
            Action<Action> subscribe,
            Action<Action> unsubscribe)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            void Handler()
            {
                // Only an in-progress TEXT edit must survive a store change. Two subtleties: composite fields (a
                // slider's inline text box) focus an inner element, never the registered control itself, so the
                // check is containment-based; and a clicked switch/enum keeps its panel's focus indefinitely (a
                // panel holds focus even without OS window focus), so skipping ANY focused control would silently
                // freeze that surface's mirror — a non-text control has no in-progress edit to clobber.
                if (control.focusController?.focusedElement is VisualElement focused &&
                    (focused == control || control.Contains(focused)) &&
                    focused is ITextEdition or TextElement)
                    return;

                control.SetValueWithoutNotify(read());
            }

            // Docking / undocking the host window re-parents the visual tree to another panel — a detach followed by
            // an attach WITHOUT a rebuild — so a build-time-only subscription would silently die on the first dock
            // move. The sync is armed from build time (the control must mirror even before it reaches a panel — the
            // settings tests pin that contract) and then follows the attach/detach pair, the guard keeping the
            // subscription single through any number of moves; re-arming also re-reads, so a change made while the
            // surface was detached is not missed.
            var subscribed = false;

            void Arm()
            {
                if (subscribed) return;
                subscribed = true;
                subscribe(Handler);
                control.SetValueWithoutNotify(read());
            }

            void Disarm()
            {
                if (!subscribed) return;
                subscribed = false;
                unsubscribe(Handler);
            }

            control.RegisterCallback<AttachToPanelEvent>(_ => Arm());
            control.RegisterCallback<DetachFromPanelEvent>(_ => Disarm());
            Arm();
        }
    }
}
