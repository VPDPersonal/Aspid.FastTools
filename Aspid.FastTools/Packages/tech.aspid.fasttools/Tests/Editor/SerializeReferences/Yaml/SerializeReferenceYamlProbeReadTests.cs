using System;
using System.IO;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for the reads of the missing-type probe: a path read inside a reference by its rid, the element ids of a
    // list, the stored type of an entry, and the reads kept until the file changes.
    [TestFixture]
    internal sealed class SerializeReferenceYamlProbeReadTests
    {
        private const long NoAnchor = SerializeReferenceYamlEditor.NoAnchor;

        private string _path;

        [SetUp]
        public void SetUp() =>
            SerializeReferenceYamlProbeCache.ClearCache();

        [TearDown]
        public void TearDown()
        {
            SerializeReferenceYamlProbeCache.ClearCache();
            YamlFixtures.Delete(_path);
        }

        [Test]
        public void TryReadStoredType_FromAnchor_ReadsTheListInsideTheReference()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NestedChildrenPrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, YamlFixtures.NestedChildrenFileId,
                YamlFixtures.NestedChildrenSequenceRid, "_children.Array.data[1]", out var rid, out var type));

            Assert.AreEqual(YamlFixtures.NestedChildrenGhostRid, rid);
            Assert.AreEqual("GhostChild", type.Class);
        }

        [Test]
        public void TryReadStoredType_FromAnchor_ReadsANestedField()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, YamlFixtures.MonoBehaviourFileId,
                YamlFixtures.RailgunRid, "_chargeEffect", out var rid, out var type));

            Assert.AreEqual(YamlFixtures.BurnEffectRid, rid);
            Assert.AreEqual("BurnEffect", type.Class);
        }

        [Test]
        public void TryReadStoredType_FromAnUnknownAnchor_ReturnsFalse()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadStoredType(_path, YamlFixtures.MonoBehaviourFileId,
                anchorRid: 9999, "_chargeEffect", out _, out _));
        }

        [Test]
        public void TryReadListIds_TopLevelList_ReadsEveryElement()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.MonoBehaviourFileId, NoAnchor,
                "_sidearms", out var rids));

            CollectionAssert.AreEqual(new[] { YamlFixtures.GhostPistolRid, YamlFixtures.ShotgunRid }, rids);
        }

        [Test]
        public void TryReadListIds_ListInsideAReference_ReadsItsElements()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.NestedChildrenPrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.NestedChildrenFileId,
                YamlFixtures.NestedChildrenSequenceRid, "_children", out var rids));

            CollectionAssert.AreEqual(new[] { YamlFixtures.NestedChildrenChildRid, YamlFixtures.NestedChildrenGhostRid }, rids);
        }

        [Test]
        public void TryReadListIds_SameNamedListInAnEarlierField_ReadsTheTopLevelOne()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.ShadowedListPrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.ShadowedListFileId, NoAnchor,
                "_weapons", out var topLevel));
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.ShadowedListFileId, NoAnchor,
                "_presets.Array.data[0]._weapons", out var nested));

            CollectionAssert.AreEqual(new long[] { -2, -2, 300 }, topLevel);
            CollectionAssert.AreEqual(new long[] { -2, 300 }, nested);
        }

        [Test]
        public void TryReadListIds_EmptyList_ReadsNoElement()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab.Replace(
                "  _sidearms:\n  - rid: 1002\n  - rid: 1003\n", "  _sidearms: []\n"));

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.MonoBehaviourFileId, NoAnchor,
                "_sidearms", out var rids));

            CollectionAssert.IsEmpty(rids);
        }

        [Test]
        public void TryReadListIds_NotAListOfReferences_ReturnsFalse()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.ListOfStructAsset);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.ListOfStructFileId, NoAnchor,
                "_slots", out _), "A list of plain structs holds no element ids.");
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadListIds(_path, YamlFixtures.ListOfStructFileId, NoAnchor,
                "_slots.Array.data[0]._weapon", out _), "A single reference is not a list.");
        }

        [Test]
        public void TryReadEntryType_ReadsTheStoredType()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadEntryType(_path, YamlFixtures.MonoBehaviourFileId,
                YamlFixtures.GhostPistolRid, out var type));
            Assert.AreEqual("GhostPistol", type.Class);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadEntryType(_path, YamlFixtures.MonoBehaviourFileId, rid: 9999, out _));
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadEntryType(_path, fileId: 42, YamlFixtures.GhostPistolRid, out _),
                "An entry of another object is not read.");
        }

        [Test]
        public void Reads_AreKeptUntilTheFileChanges()
        {
            // The same pinned write time as in SerializeReferenceYamlProbeCacheTests: only a newer one or ClearCache
            // drops the reads.
            var pinned = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var swapped = YamlFixtures.MissingTypePrefab.Replace(
                "  _sidearms:\n  - rid: 1002\n  - rid: 1003\n", "  _sidearms:\n  - rid: 1003\n  - rid: 1002\n");

            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            File.SetLastWriteTimeUtc(_path, pinned);
            Assert.AreEqual(YamlFixtures.GhostPistolRid, ReadFirstSidearm());

            File.WriteAllText(_path, swapped);
            File.SetLastWriteTimeUtc(_path, pinned);
            Assert.AreEqual(YamlFixtures.GhostPistolRid, ReadFirstSidearm(), "An unchanged file version must serve the kept read.");

            File.SetLastWriteTimeUtc(_path, pinned.AddSeconds(5));
            Assert.AreEqual(YamlFixtures.ShotgunRid, ReadFirstSidearm(), "A newer write time must read the file again.");
        }

        [Test]
        public void TryReadReferenceId_SeveralDocuments_ReadsEachByItsAnchor()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab + YamlFixtures.MissingTypePrefab
                .Replace("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n", string.Empty)
                .Replace("&6500000000000000001", "&6600000000000000001")
                .Replace("&6500000000000000003", "&6600000000000000003")
                .Replace("rid: 1002", "rid: 2002"));

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(_path, YamlFixtures.MonoBehaviourFileId,
                "_sidearms.Array.data[0]", out var first));
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(_path, fileId: 6600000000000000003L,
                "_sidearms.Array.data[0]", out var second));
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadReferenceId(_path, fileId: 42, "_sidearms.Array.data[0]", out _),
                "A file of several objects has no fallback document.");

            Assert.AreEqual(YamlFixtures.GhostPistolRid, first);
            Assert.AreEqual(2002, second);
        }

        private long ReadFirstSidearm()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, YamlFixtures.MonoBehaviourFileId,
                "_sidearms.Array.data[0]", out var rid, out _));

            return rid;
        }
    }
}
