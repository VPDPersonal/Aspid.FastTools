using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for SerializeReferenceMissingListGuard: the pre-save snapshot, the post-save restore and the notes of
    // replaced elements. Each test snapshots a YamlFixtures.MissingTypePrefab variant, overwrites the file with what Unity
    // would save after the user's edit, and checks whether the missing GhostPistol comes back. The matching itself is
    // covered by MissingListAlignmentTests.
    [TestFixture]
    internal sealed class SerializeReferenceMissingListGuardTests
    {
        private const string PristineSidearms = "  _sidearms:\n  - rid: 1002\n  - rid: 1003\n";

        private const string ShotgunType =
            "{class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}";

        // delayCall can wait many ticks in the batch-mode test runner.
        private const double DelayCallTimeoutSeconds = 10;

        private static readonly Func<ManagedTypeName, bool> Resolves = type =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        private string _path;

        [TearDown]
        public void TearDown()
        {
            if (_path is not null) SerializeReferenceMissingListGuard.ForgetNotes(_path);
            YamlFixtures.Delete(_path);
        }

        [Test]
        public void DeletingTheMissingElement_DoesNotRestoreIt()
        {
            // [GhostPistol, <None>], element 0 deleted: the saved [-2] is the surviving <None>. Deleting element 1
            // saves the same file, so the guard cannot tell the two apart and keeps the <None>.
            var snapshots = Snapshot(1002, -2);
            Save(-2);

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertElements(-2);
        }

        [Test]
        public void DeletingASibling_RestoresTheMissingElementAtItsNewIndex()
        {
            // [Shotgun, GhostPistol], Shotgun deleted: GhostPistol moved to index 0 and collapsed there.
            var snapshots = Snapshot(1003, 1002);
            Save(-2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(0);
        }

        [Test]
        public void SaveWithoutResize_RestoresInPlace()
        {
            // A prefab drops every missing element on save, even when the list keeps its size.
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 1003);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(0);
        }

        [Test]
        public void NotedClear_WithoutResize_DoesNotRestore()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            Save(-2, 1003);

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertElements(-2, 1003);
        }

        [Test]
        public void Append_RestoresTheCollapsedElement()
        {
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 1003, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
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
            Assert.AreEqual(0, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
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
                SerializeReferenceMissingListGuard.ForgetNotes(assetPath);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        [Test]
        public void NotedClear_IsConsumedByASaveThatWrites()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var pending = SerializeReferenceMissingListGuard.BeginSave(_path, Resolves);
            File.WriteAllText(_path, WithSidearms(new long[] { 1002, 1003 }, keepGhostEntry: true) + "\n");

            Assert.IsTrue(SerializeReferenceMissingListGuard.ConsumeIfWritten(_path, pending));
            Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
        }

        [Test]
        public void NotedClear_OutlivesASaveThatWroteNothing()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            var pending = SerializeReferenceMissingListGuard.BeginSave(_path, Resolves);

            Assert.IsFalse(SerializeReferenceMissingListGuard.ConsumeIfWritten(_path, pending));
            Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
        }

        [Test]
        public void NotedClear_SurvivesADomainReload()
        {
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            SerializeReferenceMissingListGuard.ReloadNotes();

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
        }

        [Test]
        public void NotedClear_SurvivesAnUndoOfALaterEdit()
        {
            Write(1002, 1003);
            Undo.IncrementCurrentGroup();
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            PerformUndo();

            Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
        }

        [Test]
        public void NotedClear_IsDroppedByUndoingItsGroup_AndBackOnRedo()
        {
            // An Undo of the <None> pick brings the element back, so the save keeps it; a Redo lets it go again.
            Write(1002, 1003);
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();

            try
            {
                Undo.IncrementCurrentGroup();
                SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);
                Undo.RecordObject(probe, "Missing list guard test");
                probe.name = "Changed";
                Undo.FlushUndoRecordObjects();

                Undo.PerformUndo();
                Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);

                Undo.PerformRedo();
                Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
            }
            finally
            {
                Undo.ClearUndo(probe);
                UnityEngine.Object.DestroyImmediate(probe);
            }
        }

        [UnityTest]
        public IEnumerator NotedClear_IsBackOnRedoOfAGroupBeforeTheLastOne()
        {
            // The replace records into the group after the note's own, and a focus change moves the current group on
            // before the next tick: a Redo of the replace's group restores the note.
            Write(1002, 1003);
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();

            try
            {
                Undo.IncrementCurrentGroup();
                SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);
                Undo.IncrementCurrentGroup();
                Undo.RecordObject(probe, "Missing list guard test");
                probe.name = "Changed";
                Undo.FlushUndoRecordObjects();
                Undo.IncrementCurrentGroup();

                yield return FlushDelayCalls();

                Undo.PerformUndo();
                Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);

                Undo.PerformRedo();
                Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
            }
            finally
            {
                Undo.ClearUndo(probe);
                UnityEngine.Object.DestroyImmediate(probe);
            }
        }

        [Test]
        public void NotedClear_IsDroppedByAChangeOutsideASave()
        {
            // A revert in version control brings the element back: the old note must not let the next save drop it.
            Write(1002, 1003);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

            File.WriteAllText(_path, WithSidearms(new long[] { 1002, 1003 }, keepGhostEntry: true) + "\n");

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves).Count);
        }

        [Test]
        public void NotedSlot_KeepsTheOtherSlotOfASharedElementGuarded()
        {
            // [GhostPistol, Shotgun, GhostPistol] share one entry; slot 0 set to <None>.
            Write(1002, 1003, 1002);
            SerializeReferenceMissingListGuard.NoteReplaced(_path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid,
                "_sidearms", index: 0);

            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path, Resolves);
            Save(-2, 1003, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(2);
            AssertElements(-2, 1003, YamlFixtures.GhostPistolRid);
        }

        [Test]
        public void MovedFromSibling_IsNotTakenForMissing()
        {
            // [Shotgun renamed with [MovedFrom], GhostPistol]: Unity loads the renamed element and keeps its id, while
            // the GhostPistol it dropped comes back.
            var movedFrom = $"{{class: {nameof(MovedNamespacePistol)}, ns: {typeof(MovedNamespacePistol).Namespace}.Legacy, " +
                $"asm: {typeof(MovedNamespacePistol).Assembly.GetName().Name}}}";

            _path = YamlFixtures.WriteTemp(WithSidearms(new long[] { 1003, 1002 }, keepGhostEntry: true).Replace(ShotgunType, movedFrom));
            var snapshots = SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(_path,
                SerializeReferenceMissingListGuard.StoredTypeLoads);

            File.WriteAllText(_path, WithSidearms(new long[] { 1003, -2 }, keepGhostEntry: false).Replace(ShotgunType, movedFrom));

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(1);
        }

        [Test]
        public void Reorder_RestoresTheMovedElement()
        {
            // [GhostPistol, Shotgun] reordered: the old index 0 now holds Shotgun.
            var snapshots = Snapshot(1002, 1003);
            Save(1003, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(1);
        }

        [Test]
        public void UnsavedEditAfterTheSave_IsKept()
        {
            const string assetPath = "Assets/__AspidMissingListGuardUnsavedEditProbe__.asset";
            const long fileId = 11400000;
            const string weapons = nameof(ReferenceListTestObject.weapons);

            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            probe.weapons.Add(new TestSword { damage = 7 });

            try
            {
                AssetDatabase.CreateAsset(probe, assetPath);

                // The file stores a missing type while the loaded element reads as null, as a missing one does.
                File.WriteAllText(assetPath, File.ReadAllText(assetPath).Replace("class: TestSword,", "class: GhostSword,"));
                probe.weapons[0] = null;

                var pending = SerializeReferenceMissingListGuard.BeginSave(assetPath, Resolves);
                Assert.AreEqual(1, pending.Snapshots.Count);

                // What the save wrote: the element as a null id, its entry gone.
                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(assetPath, fileId, $"{weapons}.Array.data[0]", out var rid));
                var lines = File.ReadAllLines(assetPath).ToList();
                lines[lines.IndexOf($"  - rid: {rid}")] = "  - rid: -2";
                lines.RemoveRange(lines.IndexOf($"    - rid: {rid}"), lines.Count - lines.IndexOf($"    - rid: {rid}"));
                lines.Add("    - rid: -2");
                lines.Add("      type: {class: , ns: , asm: }");
                File.WriteAllText(assetPath, string.Join("\n", lines) + "\n");

                // An edit made after the save, still only in memory.
                probe.weapons.Add(new TestSword { damage = 9 });
                EditorUtility.SetDirty(probe);

                SerializeReferenceMissingListGuard.CompleteSave(assetPath, pending);

                var text = File.ReadAllText(assetPath);
                StringAssert.Contains("class: GhostSword,", text, "The missing element must come back.");
                StringAssert.Contains("damage: 9", text, "The edit made after the save must not be lost.");
                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadTopLevelArrayRids(assetPath, fileId, weapons, out var rids));
                Assert.AreEqual(2, rids.Count);
            }
            finally
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        [Test]
        public void RetypedSiblingAndDelete_RestoresInPlace()
        {
            // [GhostPistol, Shotgun, 1005]: Shotgun retyped, the last element deleted.
            var snapshots = Snapshot(1002, 1003, 1005);
            Save(-2, 5000);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(0);
        }

        [Test]
        public void RetypedSiblingAndAppend_RestoresInPlace()
        {
            var snapshots = Snapshot(1002, 1003);
            Save(-2, 5000, -2);

            Assert.AreEqual(1, SerializeReferenceMissingListGuard.RestoreSnapshots(_path, snapshots).Restored.Count);
            AssertGhostPistolAt(0);
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

        // delayCall runs in registration order, so once this marker has run, the guard's queued calls have too.
        private static IEnumerator FlushDelayCalls()
        {
            var flushed = false;
            EditorApplication.delayCall += () => flushed = true;

            var deadline = EditorApplication.timeSinceStartup + DelayCallTimeoutSeconds;
            while (!flushed && EditorApplication.timeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue(flushed, "delayCall did not run in time.");
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

        private List<MissingListSnapshot> Snapshot(params long[] sidearms)
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
