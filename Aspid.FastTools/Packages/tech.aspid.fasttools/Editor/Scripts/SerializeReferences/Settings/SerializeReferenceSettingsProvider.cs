using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The package's Project Settings page: the team-wide half of the settings, matching the page's project scope.
    // The per-user controls live under Preferences instead, and the window's Settings tab shows both scopes.
    internal static class SerializeReferenceSettingsProvider
    {
        // Repaint open editor windows when a setting changes, so toggles like rid colors apply without reselection.
        [InitializeOnLoadMethod]
        private static void HookRepaint() => SerializeReferenceSettings.Changed += RepaintAll;

        private static void RepaintAll()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                if (window != null) window.Repaint();
        }

        [SettingsProvider]
        public static SettingsProvider Create() =>
            new("Project/Aspid.FastTools/SerializeReference", SettingsScope.Project)
            {
                label = "SerializeReference",
                keywords = new HashSet<string>(new[]
                {
                    "serialize", "reference", "managed", "aspid", "rid", "gate", "missing", "required",
                    "alias", "build", "ci", "excluded", "folders",
                }),
                activateHandler = (_, root) => AspidSettingsUI.BuildProviderPage(root, AspidSettingsScope.Shared),
            };
    }
}
