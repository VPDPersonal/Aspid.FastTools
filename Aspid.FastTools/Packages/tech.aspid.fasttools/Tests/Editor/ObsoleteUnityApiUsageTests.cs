using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Text.RegularExpressions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Aspid.FastTools.Editors.Tests
{
    // package.json promises 6000.0+, but Unity 6000.5 turns Object.GetInstanceID into a compile error, and a package in
    // PackageCache is never touched by the API Updater. Earlier editors at most warn, so the source is checked directly.
    internal sealed class ObsoleteUnityApiUsageTests
    {
        private const string PackageName = "tech.aspid.fasttools";

        private static readonly Regex GetInstanceIdCall = new(@"\.GetInstanceID\s*\(");

        [Test]
        public void PackageSource_DoesNotCallGetInstanceID()
        {
            var package = PackageInfo.FindForAssetPath($"Packages/{PackageName}");
            Assert.IsNotNull(package, $"Package {PackageName} is not resolved.");

            var offenders = Directory
                .EnumerateFiles(package.resolvedPath, "*.cs", SearchOption.AllDirectories)
                .Where(path => GetInstanceIdCall.IsMatch(File.ReadAllText(path)))
                .Select(path => Path.GetRelativePath(package.resolvedPath, path))
                .ToArray();

            CollectionAssert.IsEmpty(offenders, "Object.GetInstanceID is a compile error on Unity 6000.5+.");
        }
    }
}
