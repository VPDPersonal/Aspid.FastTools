using System;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Builds the SerializeReference settings controls bound to <see cref="SerializeReferenceSettings"/>. Rendered —
    /// via <see cref="Aspid.FastTools.Editors.AspidSettingsUI.BuildSurfaceContent"/> — by the window's Settings tab
    /// (both scopes), the Preferences page (per-user controls) and the Project Settings page (shared controls), so
    /// every surface renders the same controls from one definition and mirrors the others live.
    /// </summary>
    internal static class SerializeReferenceSettingsUI
    {
        /// <summary>
        /// Appends the References controls that belong to <paramref name="scope"/> — breakage detection and the
        /// attribute-free dropdown for the per-user scope; auto de-alias, the build gate and the excluded folders for
        /// the shared scope — to <paramref name="container"/>, each wired straight to
        /// <see cref="SerializeReferenceSettings"/>. Rid colours are not configurable — they always identify a shared
        /// reference, so there is no control for them here. Each row is tagged with its storage scope
        /// (<see cref="AspidSettingsUI.SharedScopeClass"/> / <see cref="AspidSettingsUI.UserScopeClass"/>) matching
        /// where <see cref="SerializeReferenceSettings"/> persists it; the classes paint a scope stripe on the
        /// branded surfaces.
        /// </summary>
        public static void BuildControls(VisualElement container, AspidSettingsScope scope = AspidSettingsScope.All)
        {
            if ((scope & AspidSettingsScope.User) != 0)
            {
                container.Add(CreateBreakageDetectionSwitch());
                container.Add(CreateDropdownWithoutAttributeSwitch());
            }

            if ((scope & AspidSettingsScope.Shared) == 0) return;

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

        // Built by one definition so the window tab and the Preferences page both render (and live-sync) the same
        // switch.
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

        // The attribute-free dropdown opt-in (off by default), per user like the other purely-inspector behaviours.
        private static AspidSwitch CreateDropdownWithoutAttributeSwitch()
        {
            var dropdownWithoutAttribute = new AspidSwitch("Dropdown without [TypeSelector]")
            {
                value = SerializeReferenceSettings.DropdownWithoutAttributeEnabled,
                tooltip = "Draw [SerializeReference] fields and lists with the type dropdown even when they carry no "
                    + "[TypeSelector] attribute. Applies to components without their own custom editor and to the "
                    + "nested fields of an already-drawn reference; a [TypeSelector] field always keeps its attribute's "
                    + "base-type narrowing.\nPer-user setting — stored locally, never committed.",
            };
            dropdownWithoutAttribute.AddClass(AspidSettingsUI.UserScopeClass);
            dropdownWithoutAttribute.RegisterValueChangedCallback(evt => SerializeReferenceSettings.DropdownWithoutAttributeEnabled = evt.newValue);
            SyncFromSettings(dropdownWithoutAttribute, () => SerializeReferenceSettings.DropdownWithoutAttributeEnabled);
            return dropdownWithoutAttribute;
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
