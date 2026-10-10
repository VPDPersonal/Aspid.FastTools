using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class PackageVersion
    {
        public const string Unknown = "?";

        private const string PackageName = "tech.aspid.fasttools";
        private const string PackageManifestPath = "Assets/Aspid/FastTools/package.json";

        // Unity does not register a copy in Assets (Asset Store or .unitypackage import) as a package,
        // so its version comes from package.json.
        public static string Current
        {
            get
            {
                var package = PackageInfo.FindForPackageName(PackageName);
                if (package is not null && !string.IsNullOrEmpty(package.version))
                    return package.version;

                var manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(PackageManifestPath);
                return manifest is null ? Unknown : ParseManifest(manifest.text);
            }
        }

        public static string ParseManifest(string json)
        {
            if (string.IsNullOrEmpty(json)) return Unknown;

            var match = Regex.Match(
                input: json,
                pattern: "\"version\"\\s*:\\s*\"([^\"]+)\"");

            return match.Success ? match.Groups[1].Value : Unknown;
        }
    }
}
