using System.IO;
using NUnit.Framework;

namespace Aspid.FastTools.Editors.Tests
{
    // Unity refuses to import a sample while a copy from another package version exists, so the window has to find
    // those copies with the same rule as Unity: Samples/<package>/<other version>/<sample>.
    [TestFixture]
    internal sealed class WelcomeSampleImportTests
    {
        private string _root;
        private string _package;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AspidFastToolsWelcome-" + Path.GetRandomFileName());
            _package = Path.Combine(_root, "Samples", "Aspid.FastTools");
            Directory.CreateDirectory(_package);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }

        [Test]
        public void FindPreviousImports_ListsCopiesOfTheSameSampleInOtherVersions()
        {
            var older = Make("1.0.0", "EnumValues");
            var oldest = Make("0.9.0", "EnumValues");
            Make("1.1.0", "EnumValues");

            var found = WelcomeView.FindPreviousImports(Path.Combine(_package, "1.1.0", "EnumValues"));

            Assert.AreEqual(new[] { oldest, older }, found);
        }

        [Test]
        public void FindPreviousImports_SkipsTheCurrentVersionAndOtherSamples()
        {
            Make("1.1.0", "EnumValues");
            Make("1.0.0", "Types");

            var found = WelcomeView.FindPreviousImports(Path.Combine(_package, "1.1.0", "EnumValues"));

            Assert.IsEmpty(found);
        }

        [Test]
        public void FindPreviousImports_WorksBeforeTheCurrentVersionFolderExists()
        {
            var older = Make("1.0.0", "Types");

            var found = WelcomeView.FindPreviousImports(Path.Combine(_package, "1.1.0", "Types"));

            Assert.AreEqual(new[] { older }, found);
        }

        [Test]
        public void FindPreviousImports_IgnoresATrailingSeparator()
        {
            var older = Make("1.0.0", "Types");

            var found = WelcomeView.FindPreviousImports(Path.Combine(_package, "1.1.0", "Types") + Path.DirectorySeparatorChar);

            Assert.AreEqual(new[] { older }, found);
        }

        [Test]
        public void FindPreviousImports_WithoutSamplesFolder_ReturnsEmpty()
        {
            Assert.IsEmpty(WelcomeView.FindPreviousImports(Path.Combine(_root, "Missing", "1.0.0", "Types")));
            Assert.IsEmpty(WelcomeView.FindPreviousImports(null));
            Assert.IsEmpty(WelcomeView.FindPreviousImports(string.Empty));
        }

        private string Make(string version, string sample)
        {
            var path = Path.Combine(_package, version, sample);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
