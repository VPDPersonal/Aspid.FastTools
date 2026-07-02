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
                    "dropdown", "typeselector", "attribute",
                }),
                activateHandler = (_, root) => Build(root),
            };

        private static void Build(VisualElement root)
        {
            root.style.marginLeft = 9;
            root.style.marginTop = 6;
            root.style.marginRight = 9;

            root.Add(Header("SerializeReference"));
            SerializeReferenceSettingsUI.BuildControls(root);
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
