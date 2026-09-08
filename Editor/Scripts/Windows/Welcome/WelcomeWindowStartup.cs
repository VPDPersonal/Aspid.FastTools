using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [InitializeOnLoad]
    internal static class WelcomeWindowStartup
    {
        private const string SessionKey = "Aspid.FastTools.WelcomeWindow.StartupHandled";

        private static string SeenKey => $"Aspid.FastTools.WelcomeWindow.Seen::{PackageVersion}::{ProjectPath}";

        private static string PackageVersion =>
            PackageInfo.FindForAssembly(typeof(WelcomeWindowStartup).Assembly)?.version ?? "unknown";

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

        public static void MarkSeen() => HasBeenSeen = true;

        private static void TryShowOnStartup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            if (Application.isBatchMode) return;

            if (!WelcomeSettings.AutoShowEnabled) return;
            if (HasBeenSeen) return;
            if (HasOpenWindow()) return;

            TabWindow.OpenWelcome();
        }

        private static bool HasOpenWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<TabWindow>();
            return windows is { Length: > 0 };
        }
    }
}
