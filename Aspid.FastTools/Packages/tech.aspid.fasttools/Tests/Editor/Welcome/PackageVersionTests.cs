using System.IO;
using NUnit.Framework;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Aspid.FastTools.Editors.Tests
{
    // The version keys the Welcome "already shown" flag and fills the window footer. A copy in Assets is not a
    // registered package, so its version comes from package.json.
    [TestFixture]
    internal sealed class PackageVersionTests
    {
        [Test]
        public void ParseManifest_ReadsVersion()
        {
            const string json = "{\n  \"name\": \"tech.aspid.fasttools\",\n  \"version\": \"1.2.3-rc.4\",\n  \"unity\": \"6000.0\"\n}";

            Assert.AreEqual("1.2.3-rc.4", PackageVersion.ParseManifest(json));
        }

        [Test]
        public void ParseManifest_ToleratesSpacing()
        {
            Assert.AreEqual("2.0.0", PackageVersion.ParseManifest("{\"version\"   :\t\"2.0.0\"}"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{}")]
        [TestCase("{\"name\": \"tech.aspid.fasttools\"}")]
        public void ParseManifest_WithoutVersion_ReturnsUnknown(string json)
        {
            Assert.AreEqual(PackageVersion.Unknown, PackageVersion.ParseManifest(json));
        }

        [Test]
        public void ParseManifest_ReadsTheShippedManifest()
        {
            var package = PackageInfo.FindForAssembly(typeof(PackageVersion).Assembly);
            Assert.IsNotNull(package, "The tests run against the package installed through UPM.");

            var json = File.ReadAllText(Path.Combine(package.resolvedPath, "package.json"));

            Assert.AreEqual(package.version, PackageVersion.ParseManifest(json));
        }

        [Test]
        public void Current_IsTheInstalledPackageVersion()
        {
            var package = PackageInfo.FindForAssembly(typeof(PackageVersion).Assembly);

            Assert.AreEqual(package.version, PackageVersion.Current);
        }

        [Test]
        public void SeenKey_CarriesThePackageVersion()
        {
            // A version bump must change the key, or an updated package never shows the Welcome window again.
            Assert.That(WelcomeWindowStartup.SeenKey, Does.Contain($"::{PackageVersion.Current}::"));
            Assert.AreNotEqual(PackageVersion.Unknown, PackageVersion.Current);
        }
    }
}
