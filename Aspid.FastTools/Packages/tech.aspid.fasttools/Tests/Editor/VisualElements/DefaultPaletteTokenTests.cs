using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// Default-Light gives every colour token of Default-Dark a value for an Aspid window on the light skin. A token
    /// added only to Default-Dark keeps its dark colour there, and nothing warns about it.
    /// </summary>
    [TestFixture]
    internal sealed class DefaultPaletteTokenTests
    {
        private const string PackagePath = "Packages/tech.aspid.fasttools";
        private const string DarkSheet = "Editor/Resources/UI/Aspid-FastTools-Default-Dark.uss";
        private const string LightSheet = "Editor/Resources/UI/Aspid-FastTools-Default-Light.uss";

        // Only the type picker dropdown uses these, on the light skin outside an Aspid window.
        private static readonly string[] DarkOnlyTokens =
        {
            "--aspid-colors-status-info-tint-light",
            "--aspid-colors-status-info-tint-lightness",
            "--aspid-colors-light-skin-text-dark",
            "--aspid-colors-light-skin-text-darkness",
            "--aspid-colors-light-skin-status-warning-text",
        };

        private static readonly Regex Comment = new(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex ColorToken = new(@"(--aspid-colors-[\w-]+)\s*:");

        [Test]
        public void LightPalette_CoversDarkColorTokens()
        {
            var dark = ColorTokens(DarkSheet, selector: ":root");
            var light = ColorTokens(LightSheet, selector: ".aspid-fasttools-palette--light");

            var missing = dark.Except(light).Except(DarkOnlyTokens).ToArray();
            Assert.IsEmpty(missing, $"Default-Light has no value for {string.Join(", ", missing)}.");
        }

        [Test]
        public void LightPalette_DefinesOnlyDarkColorTokens()
        {
            var dark = ColorTokens(DarkSheet, selector: ":root");
            var light = ColorTokens(LightSheet, selector: ".aspid-fasttools-palette--light");

            var unknown = light.Except(dark).ToArray();
            Assert.IsEmpty(unknown, $"Default-Dark does not define {string.Join(", ", unknown)}.");
        }

        [Test]
        public void DarkOnlyTokens_StayOutOfLightPalette()
        {
            var dark = ColorTokens(DarkSheet, selector: ":root");
            var light = ColorTokens(LightSheet, selector: ".aspid-fasttools-palette--light");

            foreach (var token in DarkOnlyTokens)
            {
                CollectionAssert.Contains(dark, token, $"{token} is no longer in Default-Dark; drop it from the list.");
                CollectionAssert.DoesNotContain(light, token, $"{token} is in Default-Light; drop it from the list.");
            }
        }

        private static string[] ColorTokens(string sheet, string selector)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);
            Assert.IsNotNull(package, $"{PackagePath} is not a package.");

            var content = Comment.Replace(File.ReadAllText(Path.Combine(package.resolvedPath, sheet)), string.Empty);
            var block = Regex.Match(content, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
            Assert.IsTrue(block.Success, $"{sheet} has no {selector} rule.");

            return ColorToken.Matches(block.Groups[1].Value)
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .ToArray();
        }
    }
}
