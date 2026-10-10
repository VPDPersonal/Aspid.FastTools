using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for SerializeReferenceYamlEditor.SnapshotMissingLists and RestoreMissingLists, the file side of the
    // missing-list guard. Each test snapshots an object with two lists, overwrites the file with what Unity saves after
    // the edit (the GhostPistol entry gone, its slots null) and restores.
    [TestFixture]
    internal sealed class SerializeReferenceYamlMissingListTests
    {
        private const long FileId = 11400000;
        private const long GhostRid = 1002;
        private const long ShotgunRid = 1003;
        private const long N = MissingListState.NullRid;

        private const string GhostEntry =
            "    - rid: 1002\n" +
            "      type: {class: GhostPistol, ns: Game, asm: Game}\n" +
            "      data:\n" +
            "        _damage: 15\n";

        private static readonly Func<ManagedTypeName, bool> Resolves = type =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        private string _path;

        [TearDown]
        public void TearDown() => YamlFixtures.Delete(_path);

        [Test]
        public void Snapshot_FileWithoutRefIds_ReadsNothing()
        {
            _path = YamlFixtures.WriteTemp("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n  _sidearms:\n  - rid: 1002\n");

            Assert.IsEmpty(SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null));
        }

        [Test]
        public void Snapshot_BinaryFile_ReadsNothing()
        {
            _path = YamlFixtures.WriteTemp(string.Empty);
            File.WriteAllBytes(_path, new byte[] { 0, 1, 2, 3, 0x52, 0x65, 0x66 });

            Assert.IsEmpty(SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null));
        }

        [Test]
        public void Snapshot_CapturesEachListWithTheMissingEntry()
        {
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid }, backups: new[] { N, GhostRid }, ghostEntry: true));

            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null);

            CollectionAssert.AreEquivalent(new[] { "_sidearms", "_backups" }, snapshots.Select(snapshot => snapshot.Field));
            foreach (var snapshot in snapshots)
            {
                Assert.AreEqual("GhostPistol", snapshot.Entries[GhostRid].storedType.Class);
                StringAssert.Contains("_damage: 15", string.Join("\n", snapshot.Entries[GhostRid].entryLines));
            }
        }

        [Test]
        public void Snapshot_ReplacedElement_IsLeftOut()
        {
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid }, backups: new[] { N }, ghostEntry: true));

            var replaced = new HashSet<(long, long)> { (FileId, GhostRid) };
            Assert.IsEmpty(SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced));
        }

        [Test]
        public void Restore_ReplacedSlotOfASharedElement_RestoresTheOtherSlot()
        {
            // [Ghost, Shotgun, Ghost] share one entry, and slot 0 was set to <None>: only slot 2 comes back.
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid, GhostRid }, backups: new[] { N }, ghostEntry: true));
            var replacedSlots = new[] { (FileId, GhostRid, "_sidearms", 0) };
            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null, replacedSlots);
            File.WriteAllText(_path, Asset(sidearms: new[] { N, ShotgunRid, N }, backups: new[] { N }, ghostEntry: false));

            var report = SerializeReferenceYamlEditor.RestoreMissingLists(_path, snapshots);

            Assert.AreEqual(1, report.Restored.Count);
            AssertList("_sidearms", N, ShotgunRid, GhostRid);
        }

        [Test]
        public void Snapshot_ReplacedSlotThatNoLongerHoldsTheElement_LeavesTheNearestSlotOut()
        {
            // The list was edited before the pick, so the noted slot holds Shotgun now: the note covers the nearest Ghost slot.
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid }, backups: new[] { N }, ghostEntry: true));
            var replacedSlots = new[] { (FileId, GhostRid, "_sidearms", 1) };

            Assert.IsEmpty(SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null, replacedSlots));
        }

        [Test]
        public void Restore_ReplacedSlotShiftedByAnUnsavedDelete_RestoresTheOtherSlot()
        {
            // The file holds [Shotgun, Ghost, Ghost]. Shotgun was deleted in memory, then slot 0 of [Ghost, Ghost] was set to
            // <None>: the note's slot 0 holds Shotgun in the file, so only the nearest Ghost slot counts as replaced.
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { ShotgunRid, GhostRid, GhostRid }, backups: new[] { N }, ghostEntry: true));
            var replacedSlots = new[] { (FileId, GhostRid, "_sidearms", 0) };
            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null, replacedSlots);
            File.WriteAllText(_path, Asset(sidearms: new[] { N, N }, backups: new[] { N }, ghostEntry: false));

            var report = SerializeReferenceYamlEditor.RestoreMissingLists(_path, snapshots);

            Assert.AreEqual(1, report.Restored.Count);
            Assert.AreEqual(1, CountEntries(GhostRid));
            AssertList("_sidearms", N, GhostRid);
        }

        [Test]
        public void Restore_Reorder_RestoresTheMovedElement()
        {
            var report = SaveAndRestore(new[] { GhostRid, ShotgunRid }, new[] { ShotgunRid, N });

            Assert.AreEqual(1, report.Restored.Count);
            Assert.AreEqual(1, report.Restored[0].Index);
            AssertGhostAt("_sidearms", 1);
            AssertList("_sidearms", ShotgunRid, GhostRid);
        }

        [Test]
        public void Restore_SharedMissingElement_PointsBothSlotsAtOneEntry()
        {
            var report = SaveAndRestore(new[] { GhostRid, ShotgunRid, GhostRid }, new[] { N, ShotgunRid, N });

            Assert.AreEqual(2, report.Restored.Count);
            AssertList("_sidearms", GhostRid, ShotgunRid, GhostRid);
            Assert.AreEqual(1, CountEntries(GhostRid), "One entry serves both slots.");
        }

        [Test]
        public void Restore_MissingElementSharedByTwoLists_PointsBothAtOneEntry()
        {
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid }, backups: new[] { GhostRid }, ghostEntry: true));
            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null);
            File.WriteAllText(_path, Asset(sidearms: new[] { N, ShotgunRid, N }, backups: new[] { N }, ghostEntry: false));

            var report = SerializeReferenceYamlEditor.RestoreMissingLists(_path, snapshots);

            Assert.AreEqual(2, report.Restored.Count);
            AssertList("_sidearms", GhostRid, ShotgunRid, N);
            AssertList("_backups", GhostRid);
            Assert.AreEqual(1, CountEntries(GhostRid));
        }

        [Test]
        public void Restore_DeletedAmongNulls_ReportsTheDroppedElement()
        {
            // [Ghost, <None>] with one element deleted saves [N] either way: the guard keeps the <None> and reports Ghost.
            var report = SaveAndRestore(new[] { GhostRid, N }, new[] { N });

            Assert.IsEmpty(report.Restored);
            Assert.AreEqual(1, report.Dropped.Count);
            Assert.AreEqual(GhostRid, report.Dropped[0].Rid);
            Assert.AreEqual(0, report.Dropped[0].Index);
            Assert.IsTrue(report.Dropped[0].Guessed, "A null slot is left that Ghost may have collapsed into.");
            StringAssert.Contains("_sidearms[0] (GhostPistol, rid 1002)", MissingListReport.Describe(report.Dropped, markGuessed: false));
        }

        [Test]
        public void Restore_DeletedWithoutANullLeft_IsNotInDoubt()
        {
            var report = SaveAndRestore(new[] { GhostRid, ShotgunRid }, new[] { ShotgunRid });

            Assert.AreEqual(1, report.Dropped.Count);
            Assert.IsFalse(report.Dropped[0].Guessed);
        }

        [Test]
        public void Restore_AssignedSlot_IsNotOverwritten()
        {
            _path = YamlFixtures.WriteTemp(Asset(sidearms: new[] { GhostRid, ShotgunRid }, backups: new[] { N }, ghostEntry: true));
            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null);
            File.WriteAllText(_path, Asset(sidearms: new[] { ShotgunRid, ShotgunRid }, backups: new[] { N }, ghostEntry: false));

            var report = SerializeReferenceYamlEditor.RestoreMissingLists(_path, snapshots);

            Assert.IsEmpty(report.Restored);
            AssertList("_sidearms", ShotgunRid, ShotgunRid);
        }

        private MissingListReport SaveAndRestore(long[] before, long[] after)
        {
            _path = YamlFixtures.WriteTemp(Asset(before, backups: new[] { N }, ghostEntry: true));
            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(_path, Resolves, replaced: null);

            File.WriteAllText(_path, Asset(after, backups: new[] { N }, ghostEntry: false));
            return SerializeReferenceYamlEditor.RestoreMissingLists(_path, snapshots);
        }

        private static string Asset(long[] sidearms, long[] backups, bool ghostEntry) =>
            "%YAML 1.1\n" +
            "%TAG !u! tag:unity3d.com,2011:\n" +
            $"--- !u!114 &{FileId}\n" +
            "MonoBehaviour:\n" +
            "  m_GameObject: {fileID: 0}\n" +
            "  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}\n" +
            "  m_Name:\n" +
            "  _sidearms:\n" + Items(sidearms) +
            "  _backups:\n" + Items(backups) +
            "  references:\n" +
            "    version: 2\n" +
            "    RefIds:\n" +
            "    - rid: -2\n" +
            "      type: {class: , ns: , asm: }\n" +
            (ghostEntry ? GhostEntry : string.Empty) +
            "    - rid: 1003\n" +
            "      type: {class: Shotgun, ns: Game, asm: Game}\n" +
            "      data:\n" +
            "        _pellets: 8\n";

        private static string Items(long[] rids) => string.Concat(rids.Select(rid => $"  - rid: {rid}\n"));

        private void AssertList(string field, params long[] expected)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadTopLevelArrayRids(_path, FileId, field, out var rids));
            CollectionAssert.AreEqual(expected, rids);
        }

        private void AssertGhostAt(string field, int index)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, FileId, $"{field}.Array.data[{index}]", out var rid, out var type));
            Assert.AreEqual(GhostRid, rid);
            Assert.AreEqual("GhostPistol", type.Class);
        }

        private int CountEntries(long rid) =>
            File.ReadAllLines(_path).Count(line => line == $"    - rid: {rid}");
    }
}
