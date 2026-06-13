using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The package's Project Settings page (<c>Project Settings → Aspid FastTools → SerializeReference</c>) for the
    /// SerializeReference toolset: rid colours, auto de-alias, the build/CI gate severity, and excluded scan folders.
    /// Backed by <see cref="SerializeReferenceSettings"/>.
    /// </summary>
    internal static class SerializeReferenceSettingsProvider
    {
        // Repaint open editor windows when a setting changes, so toggles like rid colours apply without reselection.
        [InitializeOnLoadMethod]
        private static void HookRepaint() => SerializeReferenceSettings.Changed += RepaintAll;

        private static void RepaintAll()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                if (window != null) window.Repaint();
        }

        [SettingsProvider]
        public static SettingsProvider Create() =>
            new("Project/Aspid FastTools/SerializeReference", SettingsScope.Project)
            {
                label = "SerializeReference",
                keywords = new HashSet<string>(new[]
                {
                    "serialize", "reference", "managed", "aspid", "rid", "gate", "missing", "required",
                }),
                activateHandler = (_, root) => Build(root),
            };

        private static void Build(VisualElement root)
        {
            root.style.marginLeft = 9;
            root.style.marginTop = 6;
            root.style.marginRight = 9;

            root.Add(Header("SerializeReference"));

            var ridColors = new Toggle("Rid colours")
            {
                value = SerializeReferenceSettings.RidColorsEnabled,
                tooltip = "Colour-code shared managed references by rid in the inspector stripe/chip and the graph window.",
            };
            ridColors.RegisterValueChangedCallback(evt => SerializeReferenceSettings.RidColorsEnabled = evt.newValue);
            root.Add(ridColors);

            var autoDeAlias = new Toggle("Auto de-alias duplicated list elements")
            {
                value = SerializeReferenceSettings.AutoDeAliasEnabled,
                tooltip = "Give a duplicated list element its own independent instance instead of sharing the original's rid.",
            };
            autoDeAlias.RegisterValueChangedCallback(evt => SerializeReferenceSettings.AutoDeAliasEnabled = evt.newValue);
            root.Add(autoDeAlias);

            var severity = new EnumField("Build / CI gate", SerializeReferenceSettings.BuildSeverity)
            {
                tooltip = "Off: never check. Warn: log missing / unset-required references at build time. Fail: abort the build.",
            };
            severity.RegisterValueChangedCallback(evt => SerializeReferenceSettings.BuildSeverity = (GateSeverity)evt.newValue);
            root.Add(severity);

            root.Add(new Label("Excluded scan folders (one path per line)")
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
            root.Add(folders);
        }

        private static Label Header(string text) => new(text)
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 8,
            },
        };
    }
}
