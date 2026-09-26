using System.IO;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using System.Collections;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Guards the package stylesheets against syntax the minimum supported Unity cannot parse. Before 6000.3 a single
    /// <c>#RGBA</c> or <c>#RRGGBBAA</c> value makes the importer drop the whole sheet, so every rule silently stops applying.
    /// </summary>
    [TestFixture]
    internal sealed class StyleSheetImportTests
    {
        private const string PackagePath = "Packages/tech.aspid.fasttools";

        private static readonly Regex Comment = new(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex Declarations = new(@"\{([^{}]*)\}");
        private static readonly Regex AlphaHexColor = new(@"#(?:[0-9a-fA-F]{8}|[0-9a-fA-F]{4})\b");

        [Test]
        public void PackageStyleSheets_ImportWithoutErrors()
        {
            // A UXML also yields a StyleSheet (its inline styles), often empty, so only .uss files are checked.
            var paths = AssetDatabase.FindAssets("t:StyleSheet", new[] { PackagePath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".uss"))
                .ToArray();

            Assert.IsNotEmpty(paths, "No stylesheets found in the package.");

            var rulesProperty = typeof(StyleSheet).GetProperty("rules", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(rulesProperty, "StyleSheet.rules was not found; update this test for the current Unity.");

            foreach (var path in paths)
            {
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                Assert.IsNotNull(styleSheet, $"{path} did not load as a StyleSheet.");
                Assert.IsFalse(styleSheet.importedWithErrors, $"{path} was imported with errors.");

                var rules = rulesProperty.GetValue(styleSheet) as ICollection;
                Assert.IsNotNull(rules, $"{path} has no rules collection.");
                Assert.Greater(rules.Count, 0, $"{path} was imported without rules.");
            }
        }

        [Test]
        public void PackageStyleSheets_DoNotUseAlphaHexColors()
        {
            // Walks the folder on disk rather than the AssetDatabase, so Samples~ is covered too.
            var files = Directory.GetFiles(Path.GetFullPath(PackagePath), "*.uss", SearchOption.AllDirectories);
            Assert.IsNotEmpty(files, "No stylesheets found in the package.");

            foreach (var file in files)
            {
                var content = Comment.Replace(File.ReadAllText(file), string.Empty);

                foreach (Match block in Declarations.Matches(content))
                {
                    var color = AlphaHexColor.Match(block.Groups[1].Value);
                    Assert.IsFalse(color.Success, $"{file} uses {color.Value}; write it as rgba(), " +
                        "Unity before 6000.3 cannot parse #RGBA and #RRGGBBAA and drops the whole sheet.");
                }
            }
        }
    }
}
