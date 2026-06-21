using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
        /// Appends the rid-colour, auto-de-alias, build-gate and excluded-folder controls to <paramref name="container"/>,
        /// each wired straight to <see cref="SerializeReferenceSettings"/>.
        /// </summary>
        public static void BuildControls(VisualElement container)
        {
            var ridColors = new Toggle("Rid colours")
            {
                value = SerializeReferenceSettings.RidColorsEnabled,
                tooltip = "Colour-code shared managed references by rid in the inspector stripe/chip and the graph window.",
            };
            ridColors.RegisterValueChangedCallback(evt => SerializeReferenceSettings.RidColorsEnabled = evt.newValue);
            container.Add(ridColors);

            var autoDeAlias = new Toggle("Auto de-alias duplicated list elements")
            {
                value = SerializeReferenceSettings.AutoDeAliasEnabled,
                tooltip = "Give a duplicated list element its own independent instance instead of sharing the original's rid.",
            };
            autoDeAlias.RegisterValueChangedCallback(evt => SerializeReferenceSettings.AutoDeAliasEnabled = evt.newValue);
            container.Add(autoDeAlias);

            var severity = new EnumField("Build / CI gate", SerializeReferenceSettings.BuildSeverity)
            {
                tooltip = "Off: never check. Warn: log missing / unset-required references. Fail: abort the build / fail the CI job.\n"
                    + "Stored in a committed ProjectSettings asset, so it travels to a clean CI runner. "
                    + "CLI flags -srGateWarnOnly / -srGateFail override it per run.",
            };
            severity.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BuildSeverity = (GateSeverity)evt.newValue);
            container.Add(severity);

            container.Add(new Label("Excluded scan folders (one path per line)")
            {
                style = { marginTop = 10, marginBottom = 2, unityFontStyleAndWeight = FontStyle.Bold },
            });

            var folders = new TextField { multiline = true, value = string.Join("\n", SerializeReferenceSettings.ExcludedFolders) };
            folders.style.minHeight = 64;
            folders.RegisterValueChangedCallback(evt => SerializeReferenceSettings.ExcludedFolders = evt.newValue
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToArray());
            container.Add(folders);
        }
    }
}
