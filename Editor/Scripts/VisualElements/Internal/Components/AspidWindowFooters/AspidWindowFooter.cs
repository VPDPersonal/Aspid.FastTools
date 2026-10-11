using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
    internal sealed partial class AspidWindowFooter : VisualElement
    {
        private const string GitHubUrl = "https://github.com/VPDPersonal/Aspid.FastTools";
        private const string GitHubReleasesUrl = GitHubUrl + "/releases";
        private const string GitHubReleaseTagUrlFormat = GitHubReleasesUrl + "/tag/v{0}";
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-WindowFooter";
        private const string RootClass = "aspid-fasttools-window-footer";
        private const string RowClass = RootClass + "__row";
        private const string VersionClass = RootClass + "__version";
        private const string KeysClass = RootClass + "__keys";
        private const string LinkClass = RootClass + "__link";

        // The keys of a view with a keyboard ring; a view whose rows also step with the left and right arrows adds them.
        internal const string KeysHint = "↑↓ navigate   ⏎ activate   esc dismiss";
        internal const string AdjustableKeysHint = "↑↓ navigate   ←→ adjust   ⏎ activate   esc dismiss";

        private readonly Label _keys;

        public AspidWindowFooter() : this(showKeysHint: true) { }

        public AspidWindowFooter(bool showKeysHint)
        {
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(RootClass);

            var version = PackageVersion.Current;

            var releaseUrl = version == PackageVersion.Unknown
                ? GitHubReleasesUrl
                : string.Format(GitHubReleaseTagUrlFormat, version);

            var versionLabel = new Label("v" + version).AddClass(VersionClass);
            versionLabel.AddManipulator(new Clickable(() => Application.OpenURL(releaseUrl)));

            var githubLabel = new Label("GitHub").AddClass(LinkClass);
            githubLabel.AddManipulator(new Clickable(() => Application.OpenURL(GitHubUrl)));

            var row = new VisualElement().AddClass(RowClass);
            row.AddChild(versionLabel);

            if (showKeysHint)
            {
                _keys = new Label(KeysHint)
                    .AddClass(KeysClass)
                    .SetPickingMode(PickingMode.Ignore);

                row.AddChild(_keys);
            }

            row.AddChild(githubLabel);

            this.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default.SetTheme(ThemeStyle.Type.Darkness)))
                .AddChild(row);
        }

        // Does nothing in a footer built without the keys hint.
        internal void SetKeysHint(string text)
        {
            if (_keys is not null) _keys.text = text;
        }
    }
}
