using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTools.Editors
{
    /// <summary>
    /// Dev-only menu that links the manual fixtures of <c>Assets/DevTests~</c> into the project as <c>Assets/DevTests</c>
    /// and unlinks them. Unity does not import a folder whose name ends with "~", so the fixtures that are broken on
    /// purpose stay out of the console until a manual test needs them. Through the link, Unity writes every change back
    /// into <c>Assets/DevTests~</c>.
    /// </summary>
    internal static class DevTestsMenu
    {
        private const string MenuPath = "Tools/Aspid 🐍/FastTools/Import DevTests";
        private const string Source = "DevTests~";
        private const string Link = "DevTests";
        // The guid the folder had before the fixtures moved to DevTests~, so a reference to the folder still resolves.
        private const string Guid = "6b9501a56f3149f38f66f517f00527d0";

        private static string AssetsPath => Path.GetFullPath(path: "Assets");

        private static string LinkPath => Path.Combine(path1: AssetsPath, path2: Link);

        private static bool IsImported =>
            Directory.Exists(path: LinkPath) && File.GetAttributes(path: LinkPath).HasFlag(flag: FileAttributes.ReparsePoint);

        [MenuItem(MenuPath, priority = 60)]
        private static void Toggle()
        {
            if (IsImported) Remove();
            else Import();

            AssetDatabase.Refresh();
        }

        // A real Assets/DevTests folder (not the link) disables the item: unlinking must never touch its files.
        [MenuItem(MenuPath, validate = true)]
        private static bool CanToggle()
        {
            var isImported = IsImported;
            Menu.SetChecked(menuPath: MenuPath, isChecked: isImported);
            return isImported || (Directory.Exists(path: Path.Combine(path1: AssetsPath, path2: Source)) && !Directory.Exists(path: LinkPath));
        }

        private static void Import()
        {
            var isLinked = Application.platform is RuntimePlatform.WindowsEditor
                ? Run(fileName: "cmd", arguments: $"/c mklink /J {Link} {Source}")
                : Run(fileName: "ln", arguments: $"-s {Source} {Link}");

            // The .meta goes in only next to the link: alone, Unity would delete it with a warning.
            if (!isLinked) return;

            File.WriteAllText(
                path: LinkPath + ".meta",
                contents: $"fileFormatVersion: 2\nguid: {Guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n" +
                    "  userData: \n  assetBundleName: \n  assetBundleVariant: \n");
        }

        // Only the link goes: "rm" without -r and "rmdir" without /s never follow it into Assets/DevTests~.
        private static void Remove()
        {
            var isUnlinked = Application.platform is RuntimePlatform.WindowsEditor
                ? Run(fileName: "cmd", arguments: $"/c rmdir {Link}")
                : Run(fileName: "rm", arguments: Link);

            // A link that stays keeps its .meta, so the refresh does not import it under a new guid.
            if (isUnlinked) File.Delete(path: LinkPath + ".meta");
        }

        private static bool Run(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo(fileName: fileName, arguments: arguments)
            {
                WorkingDirectory = AssetsPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(startInfo: startInfo)!;
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode is 0) return true;

            Debug.LogError(message: $"Aspid.FastTools DevTests: \"{fileName} {arguments}\" failed: {error}");
            return false;
        }
    }
}
