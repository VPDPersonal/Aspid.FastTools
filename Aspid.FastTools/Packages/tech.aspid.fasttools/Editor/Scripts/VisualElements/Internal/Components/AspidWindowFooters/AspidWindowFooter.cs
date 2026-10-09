using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Text.RegularExpressions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidWindowFooter : VisualElement
    {
        private const string PackageName = "tech.aspid.fasttools";
        private const string GitHubUrl = "https://github.com/VPDPersonal/Aspid.FastTools";
        private const string GitHubReleasesUrl = GitHubUrl + "/releases";
        private const string GitHubReleaseTagUrlFormat = GitHubReleasesUrl + "/tag/v{0}";
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-WindowFooter";
        private const string RootClass = "aspid-fasttools-window-footer";
        private const string RowClass = RootClass + "__row";
        private const string VersionClass = RootClass + "__version";
        private const string KeysClass = RootClass + "__keys";
        private const string LinkClass = RootClass + "__link";

        public AspidWindowFooter() : this(showKeysHint: true) { }

        public AspidWindowFooter(bool showKeysHint)
        {
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(RootClass);

            var version = ReadPackageVersion();

            var releaseUrl = version is "?"
                ? GitHubReleasesUrl
                : string.Format(GitHubReleaseTagUrlFormat, version);

            var versionLabel = new Label("v" + version).AddClass(VersionClass);
            versionLabel.AddManipulator(new Clickable(() => Application.OpenURL(releaseUrl)));

            var githubLabel = new Label("GitHub").AddClass(LinkClass);
            githubLabel.AddManipulator(new Clickable(() => Application.OpenURL(GitHubUrl)));

            var row = new VisualElement().AddClass(RowClass);
            row.AddChild(versionLabel);

            if (showKeysHint)
                row.AddChild(new Label("↑↓ navigate   ⏎ activate   esc dismiss")
                    .AddClass(KeysClass)
                    .SetPickingMode(PickingMode.Ignore));

            row.AddChild(githubLabel);

            this.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default.SetTheme(ThemeStyle.Type.Darkness)))
                .AddChild(row);
        }

        private static string ReadPackageVersion()
        {
            var package = PackageInfo.FindForPackageName(PackageName);
            if (package is not null && !string.IsNullOrEmpty(package.version))
                return package.version;

            return ReadManifestVersion();
        }

        // The fallback for a package that sits in Assets: UPM does not list it, but its package.json is still an asset.
        internal static string ReadManifestVersion()
        {
            var manifestPath = AspidPackage.ManifestPath;
            if (string.IsNullOrEmpty(manifestPath)) return "?";

            var manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);
            if (manifest is null) return "?";

            var match = Regex.Match(
                input: manifest.text,
                pattern: "\"version\"\\s*:\\s*\"([^\"]+)\"");

            return match.Success ? match.Groups[1].Value : "?";
        }
    }
}
