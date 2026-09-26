using System;
using System.IO;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceMissingListGuard"/>: the pre-save snapshot and post-save restore of
    /// missing list elements. Each test snapshots a <see cref="YamlFixtures.MissingTypePrefab"/> variant, overwrites the
    /// file with what Unity would save after the user's edit, and checks whether the missing GhostPistol comes back.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceMissingListGuardTests
    {
        private const string PristineSidearms = "  _sidearms:\n  - rid: 1002\n  - rid: 1003\n";

        private static readonly Func<ManagedTypeName, bool> Resolves = type =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        private string _path;

        [TearDown]
        public void TearDown() => YamlFixtures.Delete(_path);

        [Test]
        public void DeletingTheMissingElement_DoesNotRestoreIt()
        {
            // [GhostPistol, <None>], element 0 deleted: the saved [-2] is the surviving <None>. Deleting element 1
            // saves the same file, so the guard cannot tell the two apart and leaves the deletion as it is.
            var snapshots = Snapshot(1002, -2);
            Save(-2);

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertElements(-2);
        }

        [Test]
        public void DeletingASibling_RestoresTheMissingElementAtItsNewIndex()
        {
            // [Shotgun, GhostPistol], Shotgun deleted: GhostPistol moved to index 0 and collapsed there.
            var snapshots = Snapshot(1003, 1002);
            Save(-2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void ClearingToNoneWithoutResize_DoesNotRestore()
        {
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 1003);

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertElements(-2, 1003);
        }

        [Test]
        public void Append_RestoresTheCollapsedElement()
        {
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 1003, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void NotedClear_WithAppend_DoesNotRestore()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteIntentionalClear(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            Save(-2, 1003, -2);

            Assert.AreEqual(0, snapshots.Count);
            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
        }

        [Test]
        public void NotedClear_IsConsumedByOneSave()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteIntentionalClear(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            var next = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);

            Assert.AreEqual(1, next.Count);
        }

        [Test]
        public void TryResolveRestoreIndex_InsertBefore_FollowsTheShiftedElement()
        {
            // [Shotgun, GhostPistol] with an element inserted at the front: index 1 now holds Shotgun.
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { 1003, 1002 }, new[] { false, true });

            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, new long[] { -2, 1003, -2 }, 1, out var target));
            Assert.AreEqual(2, target);
        }

        [Test]
        public void TryResolveRestoreIndex_ReassignedSlot_DoesNotRestore()
        {
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { 1002, 1003 }, new[] { true, false });

            Assert.IsFalse(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, new long[] { 1007, 1003, -2 }, 0, out _));
        }

        [Test]
        public void LoadedScene_IsNotGuarded()
        {
            const string scenePath = "Assets/AspidMissingListGuardTests.unity";
            File.WriteAllText(scenePath, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(scenePath);

            try
            {
                Assert.IsTrue(SerializeReferenceMissingListGuard.IsGuarded(scenePath), "A closed scene file is guarded.");

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    Assert.IsFalse(SerializeReferenceMissingListGuard.IsGuarded(scenePath),
                        "A loaded scene must not be rewritten under the editor.");
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(scenePath);
            }
        }

        private List<SerializeReferenceMissingListGuard.Snapshot> Snapshot(params long[] sidearms)
        {
            Write(sidearms);
            return SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
        }

        private void Write(params long[] sidearms) =>
            _path = YamlFixtures.WriteTemp(WithSidearms(sidearms, keepGhostEntry: true));

        // What Unity writes after the edit: the collapsed GhostPistol entry is gone from RefIds.
        private void Save(params long[] sidearms) =>
            File.WriteAllText(_path, WithSidearms(sidearms, keepGhostEntry: false));

        private static string WithSidearms(long[] sidearms, bool keepGhostEntry)
        {
            var yaml = YamlFixtures.MissingTypePrefab.Replace("\r\n", "\n");
            Assert.IsTrue(yaml.Contains(PristineSidearms));

            var block = "  _sidearms:\n" + string.Concat(sidearms.Select(rid => $"  - rid: {rid}\n"));
            yaml = yaml.Replace(PristineSidearms, block);
            if (keepGhostEntry) return yaml;

            var lines = yaml.Split('\n').ToList();
            var header = lines.IndexOf($"    - rid: {YamlFixtures.GhostPistolRid}");
            Assert.GreaterOrEqual(header, 0);

            var end = header + 1;
            while (end < lines.Count && !lines[end].StartsWith("    - rid:", StringComparison.Ordinal)) end++;
            lines.RemoveRange(header, end - header);

            return string.Join("\n", lines);
        }

        private void AssertElements(params long[] expected)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadTopLevelArrayRids(
                _path, YamlFixtures.MonoBehaviourFileId, "_sidearms", out var rids));
            CollectionAssert.AreEqual(expected, rids);
        }

        private void AssertGhostPistolAt(int index)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, YamlFixtures.MonoBehaviourFileId, $"_sidearms.Array.data[{index}]", out var rid, out var type));
            Assert.Greater(rid, 0);
            Assert.AreEqual("GhostPistol", type.Class);
        }
    }
}
