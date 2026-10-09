using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the constraint cache behind the Project References groups: every render of the window reads the
    /// constraint of each entry, so a group must reuse what an earlier group loaded, and a cache that fills from many
    /// prefabs must release them.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceConstraintCacheTests
    {
        private const string FirstPath = "Assets/__AspidConstraintCacheFirst__.asset";
        private const string SecondPath = "Assets/__AspidConstraintCacheSecond__.asset";

        private int _unloadBatch;

        [SetUp]
        public void SetUp()
        {
            _unloadBatch = SerializeReferenceGateScanner.UnloadEveryLoadedFiles;
            MissingReferenceGroup.ClearConstraintCache();
        }

        [TearDown]
        public void TearDown()
        {
            SerializeReferenceGateScanner.UnloadEveryLoadedFiles = _unloadBatch;
            MissingReferenceGroup.ClearConstraintCache();

            AssetDatabase.DeleteAsset(FirstPath);
            AssetDatabase.DeleteAsset(SecondPath);
        }

        // Two groups of one render, or the same group of the next render, are separate objects. The second one must
        // not load the asset again for a constraint the first one already read.
        [Test]
        public void ResolveConstraint_AnotherGroupOfTheSameAsset_ReusesTheCachedMap()
        {
            var probe = CreateUnloadedProbe(FirstPath);

            Assert.AreEqual(typeof(ITestWeapon), NewGroup(probe).ResolveConstraint());
            Assert.IsTrue(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath), "The first group reads the asset.");

            EditorUtility.UnloadUnusedAssetsImmediate();
            Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath), Is.False);

            Assert.AreEqual(typeof(ITestWeapon), NewGroup(probe).ResolveConstraint());
            Assert.IsFalse(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath),
                "A later group must take the constraint from the cache instead of loading the asset again.");
        }

        // An import or a Rescan clears the cache, so a changed asset is read again.
        [Test]
        public void ClearConstraintCache_ReadsTheAssetAgain()
        {
            var probe = CreateUnloadedProbe(FirstPath);

            NewGroup(probe).ResolveConstraint();
            EditorUtility.UnloadUnusedAssetsImmediate();
            Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath), Is.False);

            MissingReferenceGroup.ClearConstraintCache();
            NewGroup(probe).ResolveConstraint();

            Assert.IsTrue(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath));
        }

        // A group over hundreds of prefabs loads each one; they must be released in batches, not all kept at once.
        [Test]
        public void ResolveConstraint_ManyAssets_UnloadsTheOnesItLoadedInBatches()
        {
            var first = CreateUnloadedProbe(FirstPath);
            var second = CreateUnloadedProbe(SecondPath);

            SerializeReferenceGateScanner.UnloadEveryLoadedFiles = 1;

            var group = NewGroup(first);
            group.Add(second.Path, new MissingReferenceEntry(second.FileId, second.Rid, group.StoredType));

            Assert.AreEqual(typeof(ITestWeapon), group.ResolveConstraint());
            Assert.IsFalse(AssetDatabase.IsMainAssetAtPathLoaded(FirstPath),
                "An asset loaded earlier in the pass must be unloaded before the pass ends.");
            Assert.IsFalse(AssetDatabase.IsMainAssetAtPathLoaded(SecondPath));
        }

        private static MissingReferenceGroup NewGroup(Probe probe)
        {
            var stored = ManagedTypeName.FromType(typeof(TestSword));
            var group = new MissingReferenceGroup(stored);
            group.Add(probe.Path, new MissingReferenceEntry(probe.FileId, probe.Rid, stored));
            return group;
        }

        // A saved asset whose reference field holds a TestSword, so the field's declared type is the constraint, and
        // which is no longer in memory. UnsavedRepairTestObject is the fixture that reloads from disk.
        private static Probe CreateUnloadedProbe(string path)
        {
            var asset = ScriptableObject.CreateInstance<UnsavedRepairTestObject>();
            asset.a = new TestSword();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long fileId);

            // The id the file stores, which is the one a reload reads back.
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(path, fileId, nameof(UnsavedRepairTestObject.a), out var rid));

            Resources.UnloadAsset(asset);
            Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(path), Is.False);

            return new Probe(path, fileId, rid);
        }

        private readonly struct Probe
        {
            public readonly string Path;
            public readonly long FileId;
            public readonly long Rid;

            public Probe(string path, long fileId, long rid)
            {
                Path = path;
                FileId = fileId;
                Rid = rid;
            }
        }
    }
}
