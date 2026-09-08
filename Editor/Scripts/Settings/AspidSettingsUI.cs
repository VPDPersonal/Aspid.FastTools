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
    internal static class AspidSettingsUI
    {
        internal const string StyleSheetPath = "UI/Windows/Aspid-FastTools-Settings";

        internal const string RootClass = "aspid-fasttools-settings";

        internal const string CanvasClass = "aspid-fasttools-settings-canvas";
        internal const string CanvasBackgroundClass = "aspid-fasttools-settings-canvas__background";
        internal const string HeaderClass = "aspid-fasttools-settings__header";
        internal const string HeaderTitleClass = "aspid-fasttools-settings__header-title";
        internal const string HeaderDescriptionClass = "aspid-fasttools-settings__header-description";
        internal const string SectionClass = "aspid-fasttools-settings__section";
        internal const string SectionTitleClass = "aspid-fasttools-settings__section-title";
        internal const string SectionDividerClass = "aspid-fasttools-settings__section-divider";
        internal const string SectionContentClass = "aspid-fasttools-settings__section-content";
        internal const string LegendClass = "aspid-fasttools-settings__legend";
        internal const string LegendItemClass = "aspid-fasttools-settings__legend-item";
        internal const string LegendSwatchClass = "aspid-fasttools-settings__legend-swatch";
        internal const string LegendTextClass = "aspid-fasttools-settings__legend-text";
        internal const string FooterClass = "aspid-fasttools-settings__footer";

        internal const string SharedScopeClass = "aspid-fasttools-settings-scope--shared";
        internal const string UserScopeClass = "aspid-fasttools-settings-scope--user";

        internal const string ScopeStripeClass = "aspid-fasttools-settings__scope-stripe";
        internal const string RowBackplateClass = "aspid-fasttools-settings__row-backplate";

        internal const string NavTargetClass = "aspid-fasttools-settings__nav-target";
        internal const string NavTargetFocusedClass = "aspid-fasttools-settings__nav-target--focused";

        internal const string RowClass = "aspid-fasttools-settings__row";
        internal const string RowCaptionClass = "aspid-fasttools-settings__row-caption";
        internal const string RowNoteClass = "aspid-fasttools-settings__row-note";
        internal const string ActionClass = "aspid-fasttools-settings__action";
        internal const string ActionDangerClass = "aspid-fasttools-settings__action--danger";
        internal const string ActionInfoClass = "aspid-fasttools-settings__action--info";

        internal static T AsSurface<T>(this T element) where T : VisualElement =>
            element.AddStyleSheetFromResources(StyleSheetPath).AddClass(RootClass);

        internal static T WithScopeStripe<T>(this T element, string scopeClass) where T : VisualElement
        {
            element.AddClass(scopeClass);

            // Use hierarchy because BaseField.Add routes through a closed content container.
            element.hierarchy.Insert(0, new VisualElement()
                .AddClass(RowBackplateClass)
                .SetPickingMode(PickingMode.Ignore));

            element.hierarchy.Add(new VisualElement()
                .AddClass(ScopeStripeClass)
                .AddClass(scopeClass)
                .SetPickingMode(PickingMode.Ignore));
            return element;
        }

        internal static void BuildProviderPage(VisualElement root, AspidSettingsScope scope)
        {
            BuildProviderHost(root, surface =>
            {
                var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
                BuildSurfaceContent(scroll.contentContainer, scope);

                surface.Add(scroll);
                surface.Add(BuildResetFooter(scope));
            });
        }

        internal static void BuildAreaProviderPage(VisualElement root, string title, Action<VisualElement> buildControls)
        {
            BuildProviderHost(root, surface =>
            {
                var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
                var content = scroll.contentContainer;

                content.Add(BuildSurfaceHeader(AspidSettingsScope.User, title, description: null));
                AddSection(content, title: null, buildControls);
                surface.Add(scroll);
            });
        }

        private static void BuildProviderHost(VisualElement root, Action<VisualElement> fill)
        {
            root.AddAspidThemeStyleSheets();
            root.style.flexGrow = 1;

            var host = new VisualElement()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(CanvasClass);

            var canvas = new AspidAnimatedDotsBackground()
                .SetStatus(StatusStyle.Type.Info)
                .AddClass(CanvasBackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            var surface = new VisualElement().AddClass(RootClass);
            fill(surface);

            host.AddChild(canvas).AddChild(surface).AddChild(new AspidWindowFooter(showKeysHint: false));
            root.Add(host);
        }

        internal static void BuildSurfaceContent(VisualElement container, AspidSettingsScope scope = AspidSettingsScope.All)
        {
            container.Add(BuildSurfaceHeader(
                scope,
                title: "Settings",
                description: "Every FastTools setting in one place. The stripe on each row shows where the value is stored."));

            AddSection(container, "SerializeReference", content => SerializeReferenceSettingsUI.BuildControls(content, scope));

            if ((scope & AspidSettingsScope.User) == 0) return;

            AddSection(container, "Type Selector", TypeSelectorSettingsView.BuildControls);
            AddSection(container, "Welcome", WelcomeSettingsUI.BuildControls);
        }

        private static VisualElement BuildSurfaceHeader(AspidSettingsScope scope, string title, string description)
        {
            var heading = new AspidLabel(title, AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark))
                .AddClass(HeaderTitleClass);

            var header = new VisualElement().AddClass(HeaderClass)
                .AddChild(heading);

            if (description != null)
                header.AddChild(new Label(description).AddClass(HeaderDescriptionClass));

            return header.AddChild(BuildScopeLegend(scope));
        }

        internal static void AddSection(VisualElement container, string title, Action<VisualElement> buildContent)
        {
            var card = new VisualElement().AddClass(SectionClass);

            if (title != null)
                card.AddChild(new Label(title).AddClass(SectionTitleClass))
                    .AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                            .SetTheme(ThemeStyle.Type.Light)
                            .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                        .AddClass(SectionDividerClass));

            var content = new VisualElement().AddClass(SectionContentClass);
            buildContent(content);
            container.Add(card.AddChild(content));
        }

        internal static Label CreateRowNote(string text) => new Label(text).AddClass(RowNoteClass);

        internal static VisualElement BuildScopeLegend(AspidSettingsScope scope = AspidSettingsScope.All)
        {
            var legend = new VisualElement().AddClass(LegendClass);

            if ((scope & AspidSettingsScope.Shared) != 0)
                legend.Add(BuildLegendItem(SharedScopeClass, "Shared — committed, same for the whole team"));

            if ((scope & AspidSettingsScope.User) != 0)
                legend.Add(BuildLegendItem(UserScopeClass, "Per-user — stored locally, just for you"));

            return legend;
        }

        private static VisualElement BuildLegendItem(string scopeClass, string caption)
        {
            return new VisualElement().AddClass(LegendItemClass)
                .AddChild(new VisualElement().AddClass(LegendSwatchClass).AddClass(scopeClass))
                .AddChild(new Label(caption).AddClass(LegendTextClass));
        }

        internal static VisualElement BuildResetFooter(AspidSettingsScope scope = AspidSettingsScope.All)
        {
            var row = new VisualElement().AddClass(RowClass)
                .AddChild(new Label("Reset to defaults").AddClass(RowCaptionClass));

            if ((scope & AspidSettingsScope.Shared) != 0)
            {
                var shared = new Button(ResetSharedToDefaults)
                {
                    text = "Shared",
                    tooltip = "Reset the team-wide settings to defaults: Auto de-alias on, Build / CI gate Warn, no excluded scan folders.\n"
                        + "Changes the committed ProjectSettings asset — affects every teammate once committed.",
                };
                row.AddChild(shared.AddClass(ActionClass).AddClass(SharedScopeClass));
            }

            if ((scope & AspidSettingsScope.User) != 0)
            {
                var user = new Button(ResetUserToDefaults)
                {
                    text = "Per-user",
                    tooltip = "Reset your per-user settings to defaults: Breakage detection on, dropdown without [TypeSelector] off, "
                        + $"Favorites section on, Recent items {TypeSelectorSettings.DefaultRecentsCapacity}, no theme override, auto-show Welcome on.\n"
                        + "Only this machine; the saved Favorites / Recent lists are kept.",
                };
                row.AddChild(user.AddClass(ActionClass).AddClass(ActionInfoClass).AddClass(UserScopeClass));
            }

            return new VisualElement().AddClass(FooterClass)
                .AddChild(row);
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

        internal static void SyncFromSettings<TControl, TValue>(
            TControl control,
            Func<TValue> read,
            Action<Action> subscribe,
            Action<Action> unsubscribe)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {

            var subscribed = false;

            control.RegisterCallback<AttachToPanelEvent>(_ => Arm());
            control.RegisterCallback<DetachFromPanelEvent>(_ => Disarm());
            Arm();
            return;

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

            void Handler()
            {
                // Preserve only active text edits; focused switches and dropdowns must still mirror external changes.
                if (control.focusController?.focusedElement is VisualElement focused &&
                    (focused == control || control.Contains(focused)) &&
                    focused is ITextEdition or TextElement)
                    return;

                control.SetValueWithoutNotify(read());
            }
        }
    }
}
