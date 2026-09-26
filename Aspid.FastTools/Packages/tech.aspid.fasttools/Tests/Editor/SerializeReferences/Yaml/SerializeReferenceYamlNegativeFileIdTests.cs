using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for documents under a negative anchor (<c>--- !u!114 &amp;-3200400644298251397</c>), the local file id
    /// Unity gives about half of all sub-assets and prefab objects. The fixture pairs a main object with such a
    /// sub-asset, both holding rid 1000, so a reader that skips the negative header does not just miss the sub-asset:
    /// it folds it into the main document and reads or edits the main object's entry instead.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlNegativeFileIdTests
    {
        private string _path;

        private static readonly ManagedTypeName Shotgun = new(
            "Aspid.FastTools.Samples.SerializeReferences",
            "Aspid.FastTools.Samples.SerializeReferences",
            "Shotgun");

        private static bool Resolves(ManagedTypeName type) =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        [SetUp]
        public void SetUp() =>
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeSubAssetAsset);

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [Test]
        public void FindMissingReferences_NegativeAnchor_ReportsTheSubAsset()
        {
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, Resolves);

            Assert.AreEqual(1, missing.Count, "Only the sub-asset's GhostPistol is missing.");
            Assert.AreEqual(YamlFixtures.NegativeFileId, missing[0].FileId, "The entry belongs to the sub-asset, sign included.");
            Assert.AreEqual(YamlFixtures.SubAssetRid, missing[0].Rid);
            Assert.AreEqual("GhostPistol", missing[0].StoredType.Class);
        }

        [Test]
        public void TryReadStoredType_NegativeAnchor_ReadsTheSubAsset()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, YamlFixtures.NegativeFileId, "_weapon", out var rid, out var type));
            Assert.AreEqual(YamlFixtures.SubAssetRid, rid);
            Assert.AreEqual("GhostPistol", type.Class, "The main object's Pistol must not be read for the sub-asset.");
        }

        [Test]
        public void TryRewriteType_NegativeAnchor_RewritesOnlyTheSubAsset()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.NegativeFileId, YamlFixtures.SubAssetRid, Shotgun));

            AssertStoredType(YamlFixtures.NegativeFileId, "Shotgun");
            AssertStoredType(YamlFixtures.SubAssetMainFileId, "Pistol");
        }

        [Test]
        public void TryRemoveEntry_NegativeAnchor_RemovesOnlyTheSubAssetEntry()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRemoveEntry(
                _path, YamlFixtures.NegativeFileId, YamlFixtures.SubAssetRid));

            var text = File.ReadAllText(_path);
            StringAssert.DoesNotContain("GhostPistol", text, "The sub-asset's entry must be removed.");
            AssertStoredType(YamlFixtures.SubAssetMainFileId, "Pistol");
        }

        [Test]
        public void TryNullReference_NegativeAnchor_NullsOnlyTheSubAsset()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                _path, YamlFixtures.NegativeFileId, YamlFixtures.SubAssetRid));

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(_path, YamlFixtures.NegativeFileId, "_weapon", out var nulled));
            Assert.AreEqual(-2L, nulled, "The sub-asset's pointer must read the null id.");
            StringAssert.DoesNotContain("GhostPistol", File.ReadAllText(_path), "The sub-asset's entry must be removed.");
            AssertStoredType(YamlFixtures.SubAssetMainFileId, "Pistol");
        }

        [Test]
        public void GraphScannerBuild_NegativeAnchor_KeepsTheSubAssetAsItsOwnDocument()
        {
            var documents = SerializeReferenceGraphScanner.Build(_path, resolveTypeNames: false);

            CollectionAssert.AreEqual(
                new[] { YamlFixtures.SubAssetMainFileId, YamlFixtures.NegativeFileId },
                documents.Select(document => document.FileId).ToArray(),
                "The sub-asset must be a separate document, not folded into the main one.");
            Assert.AreEqual("GhostPistol", documents[1].Nodes.Single().StoredType.Class);
        }

        private void AssertStoredType(long fileId, string expectedClass)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, fileId, "_weapon", out _, out var type),
                $"The _weapon pointer of document {fileId} must still resolve.");
            Assert.AreEqual(expectedClass, type.Class, $"Document {fileId} stored type.");
        }
    }
}
