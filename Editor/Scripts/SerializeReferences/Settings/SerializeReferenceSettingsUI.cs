using System;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The SerializeReference settings controls, defined once and rendered by the window's Settings tab, the
    // Preferences page and the Project Settings page, so every surface shows the same controls and mirrors the
    // others live.
    internal static class SerializeReferenceSettingsUI
    {
        // Breakage detection belongs to the per-user scope; auto de-alias, the build gate and the excluded folders
        // to the shared one. Each row is tagged with the scope it persists in, which paints its stripe.
        public static void BuildControls(VisualElement container, AspidSettingsScope scope = AspidSettingsScope.All)
        {
            if ((scope & AspidSettingsScope.User) != 0)
            {
                container.Add(CreateBreakageDetectionSwitch());
                container.Add(AspidSettingsUI.CreateRowNote(
                    "Watches for references broken by script renames / deletes and points at Repair."));
            }

            if ((scope & AspidSettingsScope.Shared) == 0) return;

            var autoDeAlias = new AspidSwitch("Auto de-alias duplicated list elements")
            {
                value = SerializeReferenceSettings.AutoDeAliasEnabled,
                tooltip = "Give a duplicated list element its own independent instance instead of sharing the original's rid.\n"
                    + "Stored in a committed ProjectSettings asset, so every teammate (and CI) sees the same behavior.",
            };
            autoDeAlias.WithScopeStripe(AspidSettingsUI.SharedScopeClass);
            autoDeAlias.RegisterValueChangedCallback(evt => SerializeReferenceSettings.AutoDeAliasEnabled = evt.newValue);
            SyncFromSettings(autoDeAlias, () => SerializeReferenceSettings.AutoDeAliasEnabled);
            container.Add(autoDeAlias);
            container.Add(AspidSettingsUI.CreateRowNote(
                "A duplicated list element gets its own instance instead of sharing the original's reference."));

            var severity = new EnumField("Build / CI gate", SerializeReferenceSettings.BuildSeverity)
            {
                tooltip = "Off: never check. Warn: log missing / unset-required references. Fail: abort the build / fail the CI job.\n"
                    + "Stored in a committed ProjectSettings asset, so it travels to a clean CI runner. "
                    + "CLI flags -srGateWarnOnly / -srGateFail override it per run.",
            };
            severity.WithScopeStripe(AspidSettingsUI.SharedScopeClass);
            severity.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BuildSeverity = (GateSeverity)evt.newValue);
            SyncFromSettings<EnumField, Enum>(severity, () => SerializeReferenceSettings.BuildSeverity);
            container.Add(severity);
            container.Add(AspidSettingsUI.CreateRowNote(
                "Off — never check · Warn — log missing / unset-required references · Fail — abort the build / CI job."));

            // The excluded-folders control carries its own "Excluded scan folders" header row.
            container.Add(new SerializeReferenceExcludedFoldersField().WithScopeStripe(AspidSettingsUI.SharedScopeClass));
        }

        // Defined once so the window tab and the Preferences page render and live-sync the same switch.
        private static AspidSwitch CreateBreakageDetectionSwitch()
        {
            var breakageDetection = new AspidSwitch("Breakage detection")
            {
                value = SerializeReferenceSettings.BreakageDetectionEnabled,
                tooltip = "Watch for managed references that just became missing (renamed/deleted scripts) and surface a "
                    + "toast pointing at Repair. Turn off to silence the domain-reload / import-time detection entirely.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            breakageDetection.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            breakageDetection.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BreakageDetectionEnabled = evt.newValue);
            SyncFromSettings(breakageDetection, () => SerializeReferenceSettings.BreakageDetectionEnabled);
            return breakageDetection;
        }

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
