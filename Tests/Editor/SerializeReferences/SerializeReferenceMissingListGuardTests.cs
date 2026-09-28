using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
            // saves the same file, so the guard cannot tell the two apart and keeps the <None>.
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
        public void SaveWithoutResize_RestoresInPlace()
        {
            // A prefab drops every missing element on save, even when the list keeps its size.
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 1003);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void NotedClear_WithoutResize_DoesNotRestore()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
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
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            Save(-2, 1003, -2);

            Assert.AreEqual(0, snapshots.Count);
            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
        }

        [Test]
        public void NoteReplaced_OnElementProperty_KeepsItOutOfTheSnapshot()
        {
            const string assetPath = "Assets/__AspidMissingListGuardClearProbe__.asset";
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            probe.weapons.Add(new TestSword());

            try
            {
                AssetDatabase.CreateAsset(probe, assetPath);

                // The file now stores a missing type while the loaded element reads as null, as a missing one does.
                File.WriteAllText(assetPath, File.ReadAllText(assetPath).Replace("class: TestSword,", "class: GhostSword,"));
                probe.weapons[0] = null;

                using var serializedObject = new SerializedObject(probe);
                var element = serializedObject.FindProperty($"{nameof(ReferenceListTestObject.weapons)}.Array.data[0]");

                Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(assetPath, Resolves).Count);

                SerializeReferenceMissingListGuard.NoteReplaced(element);

                Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(assetPath, Resolves).Count);
            }
            finally
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        [Test]
        public void NotedClear_IsConsumedByOneSave()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            var next = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);

            Assert.AreEqual(1, next.Count);
        }

        [Test]
        public void NotedClear_IsDroppedByUndo()
        {
            // An Undo after a <None> pick is taken as undoing it, so the element comes back after the save.
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            PerformUndo();

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            Save(-2, 1003);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void RetypedSiblingAndDelete_RestoresInPlace()
        {
            // [GhostPistol, Shotgun, 1005]: Shotgun retyped, the last element deleted.
            var snapshots = Snapshot(1002, 1003, 1005);
            Save(-2, 5000);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void RetypedSiblingAndAppend_RestoresInPlace()
        {
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 5000, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots));
            AssertGhostPistolAt(0);
        }

        [Test]
        public void TryResolveRestoreIndex_RetypedMissingAndAppend_RestoresTheOtherInPlace()
        {
            // [Ghost1, Ghost2]: Ghost1 retyped through the picker (noted), then "+".
            const long fileId = YamlFixtures.MonoBehaviourFileId;
            var before = SerializeReferenceMissingListGuard.ArrayState.Build(new List<long> { 1002, 1004 }, fileId,
                new HashSet<(long, long)> { (fileId, 1002), (fileId, 1004) }, new HashSet<(long, long)> { (fileId, 1002) });
            var after = new long[] { 5000, -2, -2 };

            Assert.IsFalse(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 0, out _));
            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 1, out var target));
            Assert.AreEqual(1, target);
        }

        [Test]
        public void TryResolveRestoreIndex_DuplicatedSibling_FollowsTheShiftedElement()
        {
            // [Shotgun, GhostPistol], Shotgun duplicated and de-aliased: the copy has a new id.
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { 1003, 1002 }, new[] { false, true });

            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, new long[] { 1003, 5000, -2 }, 1, out var target));
            Assert.AreEqual(2, target);
        }

        [Test]
        public void TryResolveRestoreIndex_NoAlignment_FallsBackToTheOldSlot()
        {
            // [GhostPistol, Shotgun, 1005]: Shotgun deleted and 1005 set to <None> in one save.
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { 1002, 1003, 1005 }, new[] { true, false, false });

            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, new long[] { -2, -2 }, 0, out var target));
            Assert.AreEqual(0, target);
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
        public void TryResolveRestoreIndex_AllMissingShrunk_KeepsTheFirstInOrder()
        {
            // Three missing elements, one deleted: the saved nulls do not say which, so the first two come back in order.
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { 1002, 1004, 1006 }, new[] { true, true, true });
            var after = new long[] { -2, -2 };

            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 0, out var first));
            Assert.AreEqual(0, first);
            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 1, out var second));
            Assert.AreEqual(1, second);
            Assert.IsFalse(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 2, out _));
        }

        [Test]
        public void TryResolveRestoreIndex_MissingAmongNulls_KeepsTheNulls()
        {
            // [<None>, GhostPistol] with one element deleted saves [-2] either way; the guard keeps the <None>.
            var before = new SerializeReferenceMissingListGuard.ArrayState(new long[] { -2, 1002 }, new[] { false, true });

            Assert.IsFalse(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, new long[] { -2 }, 1, out _));
        }

        [Test]
        public void TryResolveRestoreIndex_MixedRunShrunk_RestoresTheSurvivors()
        {
            // [Ghost, Ghost, Ghost, <None>, Shotgun] with the <None> or one Ghost deleted: two Ghosts still come back.
            var before = new SerializeReferenceMissingListGuard.ArrayState(
                new long[] { 1002, 1004, 1006, -2, 1003 }, new[] { true, true, true, false, false });
            var after = new long[] { -2, -2, -2, 1003 };

            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 0, out var first));
            Assert.AreEqual(0, first);
            Assert.IsTrue(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 1, out var second));
            Assert.AreEqual(1, second);
            Assert.IsFalse(SerializeReferenceMissingListGuard.TryResolveRestoreIndex(before, after, 2, out _));
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

        [Test]
        public void PrefabInPrefabMode_IsNotGuarded()
        {
            const string prefabPath = "Assets/AspidMissingListGuardTests.prefab";
            var root = new GameObject("AspidMissingListGuardTests");
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            try
            {
                Assert.IsTrue(SerializeReferenceMissingListGuard.IsGuarded(prefabPath), "A prefab closed in Prefab Mode is guarded.");

                PrefabStageUtility.OpenPrefab(prefabPath);
                try
                {
                    Assert.IsFalse(SerializeReferenceMissingListGuard.IsGuarded(prefabPath),
                        "The prefab open in Prefab Mode must not be rewritten under the editor.");
                }
                finally
                {
                    StageUtility.GoToMainStage();
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
        }

        // Records and undoes a change of its own, so the editor's Undo stack is left as it was.
        private static void PerformUndo()
        {
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            try
            {
                Undo.IncrementCurrentGroup();
                Undo.RecordObject(probe, "Missing list guard test");
                probe.name = "Changed";
                Undo.FlushUndoRecordObjects();
                Undo.PerformUndo();
            }
            finally
            {
                Undo.ClearUndo(probe);
                UnityEngine.Object.DestroyImmediate(probe);
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
