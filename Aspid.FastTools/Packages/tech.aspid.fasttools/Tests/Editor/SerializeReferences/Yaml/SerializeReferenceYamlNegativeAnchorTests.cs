using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Documents with a negative "&-N" anchor (sub-assets, prefab components) are documents of their own: scans report
    // them under their own fileID, and reads and writes addressed to one document never reach its neighbour.
    [TestFixture]
    internal sealed class SerializeReferenceYamlNegativeAnchorTests
    {
        private string _path;

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [Test]
        public void FindMissingReferences_SubAssetBeforeMain_ReportsSubAsset()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeSubAssetBeforeMainAsset);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, type => type.Class != "GhostNode");

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(YamlFixtures.NegativeSubAssetFileId, missing[0].FileId);
            Assert.AreEqual(YamlFixtures.NegativeSharedRid, missing[0].Rid);
        }

        [Test]
        public void TryReadStoredType_SubAssetBeforeMain_ReadsSubAssetDocument()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeSubAssetBeforeMainAsset);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, YamlFixtures.NegativeSubAssetFileId, "_payload", out _, out var subType));
            Assert.AreEqual("GhostNode", subType.Class);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, YamlFixtures.NegativeMainFileId, "_payload", out _, out var mainType));
            Assert.AreEqual("HealthyNode", mainType.Class);
        }

        [Test]
        public void TryRewriteType_SubAssetBeforeMain_RewritesOnlySubAsset()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeSubAssetBeforeMainAsset);
            var before = File.ReadAllLines(_path);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(_path, YamlFixtures.NegativeSubAssetFileId,
                YamlFixtures.NegativeSharedRid, new ManagedTypeName("Assembly-CSharp", "Game", "FixedNode")));

            var after = File.ReadAllLines(_path);
            var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToArray();

            Assert.AreEqual(1, changed.Length);
            StringAssert.Contains("GhostNode", before[changed[0]]);
            Assert.IsTrue(after.Any(line => line.Contains("class: HealthyNode")), "The main object keeps its type.");
        }

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

        [Test]
        public void FindMissingReferences_NegativeComponentAfterPositive_ReportsOwnFileId()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeComponentAfterPositivePrefab);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, type => type.Class != "GhostNode");

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(YamlFixtures.NegativePrefabSecondFileId, missing[0].FileId);
            Assert.AreEqual(YamlFixtures.NegativePrefabGhostRid, missing[0].Rid);
        }

        [Test]
        public void TryRemoveEntry_NegativeComponentAfterPositive_WrongDocument_NoOp()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeComponentAfterPositivePrefab);
            var before = File.ReadAllText(_path);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryRemoveEntry(
                _path, YamlFixtures.NegativePrefabFirstFileId, YamlFixtures.NegativePrefabPayloadRid));
            Assert.AreEqual(before, File.ReadAllText(_path));
        }

        [Test]
        public void TryNullReference_NegativeComponentAfterPositive_StaysInOwnDocument()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeComponentAfterPositivePrefab);
            var before = File.ReadAllText(_path);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryNullReference(
                _path, YamlFixtures.NegativePrefabFirstFileId, YamlFixtures.NegativePrefabGhostRid));
            Assert.AreEqual(before, File.ReadAllText(_path), "The first component holds no such reference.");

            Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                _path, YamlFixtures.NegativePrefabSecondFileId, YamlFixtures.NegativePrefabGhostRid));

            var after = File.ReadAllLines(_path);
            var secondStart = System.Array.IndexOf(after, "--- !u!114 &-4000000000000000002");

            Assert.IsFalse(after.Take(secondStart).Any(line => line.Contains("rid: -2")),
                "The first component must get neither a null pointer nor a sentinel.");

            var second = after.Skip(secondStart).ToArray();
            Assert.IsTrue(second.Contains("  - rid: -2"), "The list element is nulled.");
            Assert.IsTrue(second.Contains("    - rid: -2"), "The sentinel is added to the component's own RefIds.");
            Assert.IsFalse(second.Any(line => line.Contains("GhostNode")));
        }

        [Test]
        public void FindUnsetRequiredFields_NegativeComponent_IsScanned()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NegativeComponentAfterPositivePrefab);

            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, guid =>
                guid == YamlFixtures.NegativePrefabSecondScriptGuid
                    ? new List<RequiredFieldDescriptor> { new("requiredString", RequiredFieldKind.String) }
                    : null);

            Assert.AreEqual(1, violations.Count);
            Assert.AreEqual(YamlFixtures.NegativePrefabSecondFileId, violations[0].FileId);
            Assert.AreEqual("requiredString", violations[0].FieldName);
        }
    }
}
