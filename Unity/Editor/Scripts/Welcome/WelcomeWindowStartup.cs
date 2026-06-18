using System.IO;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// First-run auto-show for the Welcome panel. The Welcome content now lives as the "home" tab of the
    /// Managed References window (<see cref="SerializeReferenceWindow"/>), so on first import we open that window
    /// on its home tab instead of a standalone window. Owns the per-project "seen" flag that gates the auto-show.
    /// </summary>
    [InitializeOnLoad]
    internal static class WelcomeWindowStartup
    {
        private const string SessionKey = "Aspid.FastTools.WelcomeWindow.StartupHandled";

        private static string SeenKey => $"Aspid.FastTools.WelcomeWindow.Seen::{ProjectPath}";

        public static bool HasBeenSeen
        {
            get => EditorPrefs.GetBool(SeenKey, false);
            private set => EditorPrefs.SetBool(SeenKey, value);
        }

        private static string ProjectPath
        {
            get
            {
                var projectDirectory = Directory.GetParent(Application.dataPath);
                return projectDirectory?.FullName ?? Application.dataPath;
            }
        }

        static WelcomeWindowStartup()
        {
            EditorApplication.delayCall += TryShowOnStartup;
        }

        /// <summary>Records that the Welcome panel has been opened, so the first-run auto-show won't fire again.</summary>
        public static void MarkSeen() => HasBeenSeen = true;

        private static void TryShowOnStartup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            if (Application.isBatchMode) return;
            if (HasBeenSeen) return;
            if (HasOpenWindow()) return;

            SerializeReferenceWindow.OpenWelcome();
        }

        private static bool HasOpenWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<SerializeReferenceWindow>();
            return windows is { Length: > 0 };
        }
    }
}
