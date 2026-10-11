using System.IO;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // The package folder is found by the GUID of its package.json, so it works wherever the folder sits in the project.
    internal static class AspidPackage
    {
        private const string ManifestGuid = "2343114218c7047ce9dfa4b163d203c2";

        // Empty when the project has no such asset.
        public static string ManifestPath => AssetDatabase.GUIDToAssetPath(ManifestGuid);

        // Empty when the project has no such asset.
        public static string RootPath
        {
            get
            {
                var manifestPath = ManifestPath;

                return string.IsNullOrEmpty(manifestPath)
                    ? string.Empty
                    : Path.GetDirectoryName(manifestPath)?.Replace('\\', '/') ?? string.Empty;
            }
        }
    }
}
