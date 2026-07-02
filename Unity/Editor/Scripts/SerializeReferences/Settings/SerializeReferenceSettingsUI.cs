using System;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Builds the SerializeReference settings controls bound to <see cref="SerializeReferenceSettings"/>. Shared by the
    /// Project Settings page (<see cref="SerializeReferenceSettingsProvider"/>) and — via
    /// <see cref="Aspid.FastTools.Editors.AspidSettingsUI.BuildSurfaceContent"/> — the window's Settings tab and the
    /// Preferences page, so every surface renders the same controls from one definition and mirrors the others live.
    /// </summary>
    internal static class SerializeReferenceSettingsUI
    {
        /// <summary>
        /// Appends the breakage-detection, auto-de-alias, build-gate and excluded-folder controls to
        /// <paramref name="container"/>, each wired straight to <see cref="SerializeReferenceSettings"/>. Rid colours
        /// are not configurable — they always identify a shared reference, so there is no control for them here.
        /// Each row is tagged with its storage scope (<see cref="AspidSettingsUI.SharedScopeClass"/> /
        /// <see cref="AspidSettingsUI.UserScopeClass"/>) matching where <see cref="SerializeReferenceSettings"/>
        /// persists it; the classes paint a scope stripe on the branded surfaces and are inert on the Project Settings
        /// page, which never loads that sheet.
        /// </summary>
        public static void BuildControls(VisualElement container)
        {
            container.Add(CreateBreakageDetectionSwitch());

            var autoDeAlias = new AspidSwitch("Auto de-alias duplicated list elements")
            {
                value = SerializeReferenceSettings.AutoDeAliasEnabled,
                tooltip = "Give a duplicated list element its own independent instance instead of sharing the original's rid.\n"
                    + "Stored in a committed ProjectSettings asset, so every teammate (and CI) sees the same behaviour.",
            };
            autoDeAlias.AddClass(AspidSettingsUI.SharedScopeClass);
            autoDeAlias.RegisterValueChangedCallback(evt => SerializeReferenceSettings.AutoDeAliasEnabled = evt.newValue);
            SyncFromSettings(autoDeAlias, () => SerializeReferenceSettings.AutoDeAliasEnabled);
            container.Add(autoDeAlias);

            var severity = new EnumField("Build / CI gate", SerializeReferenceSettings.BuildSeverity)
            {
                tooltip = "Off: never check. Warn: log missing / unset-required references. Fail: abort the build / fail the CI job.\n"
                    + "Stored in a committed ProjectSettings asset, so it travels to a clean CI runner. "
                    + "CLI flags -srGateWarnOnly / -srGateFail override it per run.",
            };
            severity.AddClass(AspidSettingsUI.SharedScopeClass);
            severity.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BuildSeverity = (GateSeverity)evt.newValue);
            SyncFromSettings<EnumField, Enum>(severity, () => SerializeReferenceSettings.BuildSeverity);
            container.Add(severity);

            // The excluded-folders panel carries its own "Excluded scan folders" header inside its frame.
            container.Add(new SerializeReferenceExcludedFoldersField().AddClass(AspidSettingsUI.SharedScopeClass));
        }

        // The one References control persisted per user; built by one definition so the window tab, the Project
        // Settings page and the Preferences page all render (and live-sync) the same switch.
        private static AspidSwitch CreateBreakageDetectionSwitch()
        {
            var breakageDetection = new AspidSwitch("Breakage detection")
            {
                value = SerializeReferenceSettings.BreakageDetectionEnabled,
                tooltip = "Watch for managed references that just became missing (renamed/deleted scripts) and surface a "
                    + "toast pointing at Repair. Turn off to silence the domain-reload / import-time detection entirely.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            breakageDetection.AddClass(AspidSettingsUI.UserScopeClass);
            breakageDetection.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BreakageDetectionEnabled = evt.newValue);
            SyncFromSettings(breakageDetection, () => SerializeReferenceSettings.BreakageDetectionEnabled);
            return breakageDetection;
        }

        // Shorthand over the shared live-sync helper, binding to this store's Changed signal.
        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            AspidSettingsUI.SyncFromSettings(
                control,
                read,
                handler => SerializeReferenceSettings.Changed += handler,
                handler => SerializeReferenceSettings.Changed -= handler);
        }
    }
}
