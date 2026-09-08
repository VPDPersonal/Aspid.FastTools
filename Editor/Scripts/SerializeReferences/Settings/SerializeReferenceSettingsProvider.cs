using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceSettingsProvider
    {
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
