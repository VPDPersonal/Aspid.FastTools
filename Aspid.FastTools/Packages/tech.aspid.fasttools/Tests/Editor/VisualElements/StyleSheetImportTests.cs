using System.Linq;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// Every style sheet of the package must import cleanly: a USS parsing error silently drops rules of the sheet.
    /// </summary>
    [TestFixture]
    internal sealed class StyleSheetImportTests
    {
        private const string EditorFolder = "Packages/tech.aspid.fasttools/Editor";

        [Test]
        public void PackageStyleSheets_ImportWithoutErrors()
        {
            var paths = AssetDatabase.FindAssets("t:StyleSheet", new[] { EditorFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            Assert.IsNotEmpty(paths, "The package style sheets must be found.");

            var failed = paths
                .Where(path => AssetDatabase.LoadAssetAtPath<StyleSheet>(path).importedWithErrors)
                .ToArray();

            Assert.IsEmpty(failed, $"Style sheets imported with errors: {string.Join(", ", failed)}");
        }
    }
}
