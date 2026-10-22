using System.Text;
using NUnit.Framework;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    /// <summary>
    /// Coverage for the card titles of a missing type whose suggested replacement has the same class name: both are
    /// qualified just enough to tell them apart, and other names are left as they are.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceAuditUITests
    {
        private const string StoredAssembly = "Game.Legacy";

        [Test]
        public void TryQualifySameNames_DifferentClassNames_LeavesTheTitles()
        {
            var stored = new ManagedTypeName(StoredAssembly, "System.Text", "Builder");

            var qualified = SerializeReferenceAuditUI.TryQualifySameNames(stored, typeof(StringBuilder), out _, out _);

            Assert.IsFalse(qualified, "Names that already differ need no namespace.");
        }

        [TestCase("Legacy.Text", "Legacy.Text.StringBuilder", "System.Text.StringBuilder")]
        [TestCase("Legacy", "Legacy.StringBuilder", "Text.StringBuilder")]
        [TestCase("", "StringBuilder", "Text.StringBuilder")]
        public void TryQualifySameNames_SameClassName_AddsTheShortestDifferingNamespaceSuffix(
            string storedNamespace, string expectedStored, string expectedTarget)
        {
            var stored = new ManagedTypeName(StoredAssembly, storedNamespace, nameof(StringBuilder));

            var qualified = SerializeReferenceAuditUI.TryQualifySameNames(stored, typeof(StringBuilder),
                out var storedTitle, out var targetTitle);

            Assert.IsTrue(qualified);
            Assert.AreEqual(expectedStored, storedTitle);
            Assert.AreEqual(expectedTarget, targetTitle);
        }

        [Test]
        public void TryQualifySameNames_SameNamespace_NamesTheAssemblies()
        {
            var stored = new ManagedTypeName(StoredAssembly, "System.Text", nameof(StringBuilder));
            var targetAssembly = typeof(StringBuilder).Assembly.GetName().Name;

            SerializeReferenceAuditUI.TryQualifySameNames(stored, typeof(StringBuilder), out var storedTitle, out var targetTitle);

            Assert.AreEqual($"StringBuilder ({StoredAssembly})", storedTitle);
            Assert.AreEqual($"StringBuilder ({targetAssembly})", targetTitle);
        }
    }
}
