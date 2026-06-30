using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Builds the SerializeReference settings controls bound to <see cref="SerializeReferenceSettings"/>. Shared by the
    /// Project Settings page (<see cref="SerializeReferenceSettingsProvider"/>) and the in-window Settings tab
    /// (<see cref="SettingsView"/>) so both surfaces render the same controls from one definition — no duplicated UI.
    /// </summary>
    internal static class SerializeReferenceSettingsUI
    {
        /// <summary>
        /// Appends the rid-colour, auto-de-alias, breakage-detection, build-gate and excluded-folder controls to
        /// <paramref name="container"/>, each wired straight to <see cref="SerializeReferenceSettings"/>.
        /// </summary>
        public static void BuildControls(VisualElement container)
        {
            var ridColors = new AspidSwitch("Rid colours")
            {
                value = SerializeReferenceSettings.RidColorsEnabled,
                tooltip = "Colour-code shared managed references by rid in the inspector stripe/chip and the graph window.",
            };
            ridColors.RegisterValueChangedCallback(evt => SerializeReferenceSettings.RidColorsEnabled = evt.newValue);
            SyncFromSettings(ridColors, () => SerializeReferenceSettings.RidColorsEnabled);
            container.Add(ridColors);

            var autoDeAlias = new AspidSwitch("Auto de-alias duplicated list elements")
            {
                value = SerializeReferenceSettings.AutoDeAliasEnabled,
                tooltip = "Give a duplicated list element its own independent instance instead of sharing the original's rid.\n"
                    + "Stored in a committed ProjectSettings asset, so every teammate (and CI) sees the same behaviour.",
            };
            autoDeAlias.RegisterValueChangedCallback(evt => SerializeReferenceSettings.AutoDeAliasEnabled = evt.newValue);
            SyncFromSettings(autoDeAlias, () => SerializeReferenceSettings.AutoDeAliasEnabled);
            container.Add(autoDeAlias);

            var breakageDetection = new AspidSwitch("Breakage detection")
            {
                value = SerializeReferenceSettings.BreakageDetectionEnabled,
                tooltip = "Watch for managed references that just became missing (renamed/deleted scripts) and surface a " +
                          "toast pointing at Repair. Turn off to silence the domain-reload / import-time detection entirely.",
            };
            breakageDetection.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BreakageDetectionEnabled = evt.newValue);
            SyncFromSettings(breakageDetection, () => SerializeReferenceSettings.BreakageDetectionEnabled);
            container.Add(breakageDetection);

            var severity = new EnumField("Build / CI gate", SerializeReferenceSettings.BuildSeverity)
            {
                tooltip = "Off: never check. Warn: log missing / unset-required references. Fail: abort the build / fail the CI job.\n"
                    + "Stored in a committed ProjectSettings asset, so it travels to a clean CI runner. "
                    + "CLI flags -srGateWarnOnly / -srGateFail override it per run.",
            };
            severity.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BuildSeverity = (GateSeverity)evt.newValue);
            SyncFromSettings<EnumField, Enum>(severity, () => SerializeReferenceSettings.BuildSeverity);
            container.Add(severity);

            // The excluded-folders panel carries its own "Excluded scan folders" header inside its frame.
            container.Add(new SerializeReferenceExcludedFoldersField());
        }

        /// <summary>
        /// Keeps <paramref name="control"/> in lock-step with the shared <see cref="SerializeReferenceSettings"/> store so
        /// the in-window Settings tab and the Project Settings page reflect each other live: on every
        /// <see cref="SerializeReferenceSettings.Changed"/> the control re-reads its backing value through
        /// <paramref name="read"/> <i>without notifying</i>, so it never writes back or loops. The control the user is
        /// actively editing is skipped, so an in-progress edit (e.g. typing a path into the multiline folders field) is
        /// never normalized out from under the cursor. The subscription is released on
        /// <see cref="DetachFromPanelEvent"/> so a closed surface leaks nothing.
        /// </summary>
        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            void Handler()
            {
                // Don't clobber the surface the user is typing into; the other (unfocused) surface still syncs, and the
                // focused one re-reads the (already-correct) value on its next change or re-attach.
                if (control.focusController?.focusedElement == control) return;
                control.SetValueWithoutNotify(read());
            }

            SerializeReferenceSettings.Changed += Handler;
            control.RegisterCallback<DetachFromPanelEvent>(_ => SerializeReferenceSettings.Changed -= Handler);
        }
    }
}
