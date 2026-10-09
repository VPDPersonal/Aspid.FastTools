using UnityEditor;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Fix all, Clear and Undo of Project References on real assets. They edit the files directly and cannot be undone
    // with Ctrl+Z, so each test checks what Unity loads back after the batch reimport.
    [TestFixture]
    internal sealed class SerializeReferenceBatchEditorTests
    {
        private const string FirstProbePath = "Assets/__AspidBatchEditorProbeFirst__.asset";
        private const string SecondProbePath = "Assets/__AspidBatchEditorProbeSecond__.asset";
        private const float DelayA = 1.5f;
        private const float DelayB = 4f;

        private static readonly string[] _probePaths = { FirstProbePath, SecondProbePath };
        private static readonly int[] _wavesA = { 1, 2, 3 };
        private static readonly int[] _wavesB = { 7 };

        private static readonly ManagedTypeName _storedType = ManagedTypeName.FromType(typeof(UnsavedRepairSpawnAction));
        private static readonly ManagedTypeName _otherType = ManagedTypeName.FromType(typeof(TestSword));
        private static readonly ManagedTypeName _goneType = new(_storedType.Assembly, _storedType.Namespace, "UnsavedRepairSpawnActionGone");

        // Two missing references (fields a and b) in each probe.
        private List<MissingReferenceLocation> _locations;

        [SetUp]
        public void SetUp()
        {
            _locations = new List<MissingReferenceLocation>();
            foreach (var path in _probePaths)
                _locations.AddRange(CreateBrokenProbe(path));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var path in _probePaths)
                AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void Rewrite_EntriesInSeveralFiles_RepairsEveryEntry_AndKeepsItsData()
        {
            Assert.AreEqual(4, SerializeReferenceBatchEditor.Rewrite(_locations, _storedType, progressTitle: "Test"));

            foreach (var path in _probePaths)
                AssertRepaired(path);
        }

        [Test]
        public void Rewrite_StaleEntry_IsSkipped_AndTheOthersAreRepaired()
        {
            var stale = new MissingReferenceLocation(FirstProbePath, new MissingReferenceEntry(_locations[0].Entry.FileId, rid: 999999, _goneType));
            var entries = _locations.Prepend(stale).ToList();

            Assert.AreEqual(4, SerializeReferenceBatchEditor.Rewrite(entries, _storedType, progressTitle: "Test"));

            foreach (var path in _probePaths)
                AssertRepaired(path);
        }

        [Test]
        public void Null_EntriesInSeveralFiles_ClearsEveryEntry()
        {
            Assert.AreEqual(4, SerializeReferenceBatchEditor.Null(_locations, progressTitle: "Test"));

            foreach (var path in _probePaths)
            {
                var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(path);
                Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(probe), path);
                Assert.IsNull(probe.a, path);
                Assert.IsNull(probe.b, path);
            }
        }

        [Test]
        public void FilterStillHolding_AfterRewrite_KeepsEveryEntry_AndUndoRestoresTheMissingType()
        {
            Assert.AreEqual(4, SerializeReferenceBatchEditor.Rewrite(_locations, _storedType, progressTitle: "Test"));

            var holding = SerializeReferenceBatchEditor.FilterStillHolding(_locations, _storedType, out var diverged);
            Assert.AreEqual(0, diverged);
            Assert.AreEqual(4, holding.Count);

            // What Undo on the summary does.
            Assert.AreEqual(4, SerializeReferenceBatchEditor.Rewrite(holding, _goneType, progressTitle: "Test"));

            foreach (var path in _probePaths)
            {
                var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(path);
                Assert.AreEqual(2, SerializationUtility.GetManagedReferencesWithMissingTypes(probe).Length, path);
            }
        }

        [Test]
        public void FilterStillHolding_EntryRepointedSinceRewrite_CountsItAsDiverged()
        {
            Assert.AreEqual(4, SerializeReferenceBatchEditor.Rewrite(_locations, _storedType, progressTitle: "Test"));

            // A later fix re-pointed one entry to another type; an undo must not destroy that newer fix.
            var repointed = _locations[1];
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(repointed.AssetPath, repointed.Entry.FileId, repointed.Entry.Rid, _otherType));

            var holding = SerializeReferenceBatchEditor.FilterStillHolding(_locations, _storedType, out var diverged);

            Assert.AreEqual(1, diverged);
            CollectionAssert.AreEqual(
                _locations.Where((_, index) => index != 1).Select(Describe),
                holding.Select(Describe));
        }

        [Test]
        public void FilterWritable_AssetWithUnsavedChanges_HoldsBackEveryEntryOfIt()
        {
            var dirty = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(FirstProbePath);
            EditorUtility.SetDirty(dirty);
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.HasUnsavedChanges(FirstProbePath), "Precondition: the first probe must be dirty.");

            var writable = SerializeReferenceBatchEditor.FilterWritable(_locations, out var skipped);

            Assert.AreEqual(2, skipped);
            CollectionAssert.AreEqual(new[] { SecondProbePath, SecondProbePath }, writable.Select(location => location.AssetPath));

            SerializeReferenceBatchEditor.SplitWritable(_locations, out var onDisk, out var inMemory);

            CollectionAssert.AreEqual(new[] { SecondProbePath, SecondProbePath }, onDisk.Select(location => location.AssetPath));
            CollectionAssert.AreEqual(new[] { FirstProbePath, FirstProbePath }, inMemory.Select(location => location.AssetPath));
        }

        // Creates a probe with two healthy references and breaks both on disk: their stored class no longer resolves.
        private static IEnumerable<MissingReferenceLocation> CreateBrokenProbe(string path)
        {
            var probe = ScriptableObject.CreateInstance<UnsavedRepairTestObject>();
            probe.a = new UnsavedRepairSpawnAction { delay = DelayA, waves = _wavesA.ToList() };
            probe.b = new UnsavedRepairSpawnAction { delay = DelayB, waves = _wavesB.ToList() };
            AssetDatabase.CreateAsset(probe, path);

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);
            var entries = new[]
            {
                new MissingReferenceEntry(fileId, ManagedReferenceUtility.GetManagedReferenceIdForObject(probe, probe.a), _goneType),
                new MissingReferenceEntry(fileId, ManagedReferenceUtility.GetManagedReferenceIdForObject(probe, probe.b), _goneType),
            };

            foreach (var entry in entries)
                Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(path, entry.FileId, entry.Rid, _goneType));

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var broken = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(path);
            Assert.AreEqual(2, SerializationUtility.GetManagedReferencesWithMissingTypes(broken).Length,
                "Precondition: both fields must be missing references.");

            return entries.Select(entry => new MissingReferenceLocation(path, entry));
        }

        private static void AssertRepaired(string path)
        {
            var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(path);
            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(probe), path);

            var a = probe.a as UnsavedRepairSpawnAction;
            var b = probe.b as UnsavedRepairSpawnAction;
            Assert.IsNotNull(a, path);
            Assert.IsNotNull(b, path);

            Assert.AreEqual(DelayA, a.delay, path);
            Assert.AreEqual(DelayB, b.delay, path);
            CollectionAssert.AreEqual(_wavesA, a.waves, "The repair must keep the reference's nested data.");
            CollectionAssert.AreEqual(_wavesB, b.waves, "The repair must keep the reference's nested data.");
        }

        private static string Describe(MissingReferenceLocation location) =>
            $"{location.AssetPath}:{location.Entry.FileId}:{location.Entry.Rid}";
    }
}
