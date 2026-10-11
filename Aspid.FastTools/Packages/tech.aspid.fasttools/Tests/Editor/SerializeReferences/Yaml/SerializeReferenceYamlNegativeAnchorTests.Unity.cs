using System.Linq;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.YamlTests runs
    // the rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class SerializeReferenceYamlNegativeAnchorTests
    {
        [Test]
        public void GraphScanner_SubAssetBeforeMain_BuildsBothDocuments()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeSubAssetBeforeMainAsset);

            var fileIds = SerializeReferenceGraphScanner.Build(_path, resolveTypeNames: false).Select(document => document.FileId);

            CollectionAssert.AreEqual(
                new[] { YamlFixtures.NegativeSubAssetFileId, YamlFixtures.NegativeMainFileId }, fileIds.ToArray());
        }

        [Test]
        public void GraphScanner_NegativeComponentAfterPositive_KeepsDocumentsApart()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeComponentAfterPositivePrefab);

            var documents = SerializeReferenceGraphScanner.Build(_path, resolveTypeNames: false);
            Assert.AreEqual(2, documents.Count);

            var first = documents.Single(document => document.FileId == YamlFixtures.NegativePrefabFirstFileId);
            CollectionAssert.AreEqual(new[] { 1000L }, first.Nodes.Select(node => node.Rid).ToArray());
            CollectionAssert.IsEmpty(first.Orphans, "The second component's entries must not look orphaned here.");

            var second = documents.Single(document => document.FileId == YamlFixtures.NegativePrefabSecondFileId);
            CollectionAssert.AreEqual(new[] { YamlFixtures.NegativePrefabPayloadRid, YamlFixtures.NegativePrefabGhostRid },
                second.Nodes.Select(node => node.Rid).ToArray());
            CollectionAssert.IsEmpty(second.Orphans);
        }
    }
}
