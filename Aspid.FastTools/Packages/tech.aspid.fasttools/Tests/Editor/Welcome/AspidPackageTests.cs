using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.UIElements.Editors.Internal;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// The package folder and its version come from the GUID of <c>package.json</c>, so the Welcome tab and the
    /// footer keep working when the folder sits somewhere other than a fixed path.
    /// </summary>
    [TestFixture]
    internal sealed class AspidPackageTests
    {
        [Serializable]
        private sealed class Manifest
        {
            public string name = string.Empty;
            public string version = string.Empty;
        }

        [Test]
        public void ManifestPath_PointsAtThePackageJson()
        {
            var manifest = LoadManifest();

            Assert.AreEqual("tech.aspid.fasttools", manifest.name);
            Assert.IsNotEmpty(manifest.version);
        }

        [Test]
        public void RootPath_IsTheFolderOfThePackageJson()
        {
            var rootPath = AspidPackage.RootPath;

            Assert.IsNotEmpty(rootPath, "The package folder must be found by the GUID of its package.json.");
            Assert.IsTrue(AssetDatabase.IsValidFolder(rootPath));
            Assert.AreEqual(AspidPackage.ManifestPath, rootPath + "/package.json");
        }

        [Test]
        public void ReadManifestVersion_ReturnsTheVersionOfThePackageJson()
        {
            var version = AspidWindowFooter.ReadManifestVersion();

            Assert.AreNotEqual("?", version, "A package.json found by its GUID must give a version.");
            Assert.AreEqual(LoadManifest().version, version);
        }

        private static Manifest LoadManifest()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AspidPackage.ManifestPath);
            Assert.IsNotNull(asset, "The GUID of package.json must resolve to an asset.");

            return JsonUtility.FromJson<Manifest>(asset.text);
        }
    }
}
