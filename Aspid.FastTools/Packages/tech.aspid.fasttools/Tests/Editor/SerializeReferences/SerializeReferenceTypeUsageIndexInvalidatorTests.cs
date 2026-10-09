using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for SerializeReferenceTypeUsageIndexInvalidator: a delete, a move or a small import
    // patches the warm SerializeReferenceTypeUsageIndex in place, and only a script change or a big
    // import drops it.
    [TestFixture]
    internal sealed class SerializeReferenceTypeUsageIndexInvalidatorTests
    {
        // Every shared setter saves this file, so restoring the values alone would still leave a new or rewritten file.
        private const string SharedSettingsPath = "ProjectSettings/SerializeReferenceSharedSettings.asset";

        private const string ScannedFolder = "Assets/__AspidInvalidatorScanned__";
        private const string MovedToFolder = "Assets/__AspidInvalidatorMoved__";
        private const string ExcludedFolder = "Assets/__AspidInvalidatorExcluded__";

        private static readonly string[] _none = Array.Empty<string>();

        private string[] _excludedFolders;
        private byte[] _sharedSettingsFile;
        private BreakageBaselineSnapshot _baselines;

        [SetUp]
        public void SetUp()
        {
            _baselines = new BreakageBaselineSnapshot();
            _excludedFolders = SerializeReferenceSettings.ExcludedFolders;
            _sharedSettingsFile = File.Exists(SharedSettingsPath) ? File.ReadAllBytes(SharedSettingsPath) : null;

            foreach (var folder in new[] { ScannedFolder, MovedToFolder, ExcludedFolder })
            {
                AssetDatabase.DeleteAsset(folder);
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            }
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                foreach (var folder in new[] { ScannedFolder, MovedToFolder, ExcludedFolder })
                    AssetDatabase.DeleteAsset(folder);

                SerializeReferenceSettings.ExcludedFolders = _excludedFolders;
                SerializeReferenceTypeUsageIndex.Reset();
            }
            finally
            {
                // Changing the excluded folders resets the detector baselines, so they are restored after the last change.
                _baselines.Restore();

                // The values above already match the snapshot in memory; this puts the file back byte for byte.
                if (_sharedSettingsFile is null) File.Delete(SharedSettingsPath);
                else File.WriteAllBytes(SharedSettingsPath, _sharedSettingsFile);
            }
        }

        [Test]
        public void Apply_ScannedAssetDeleted_DropsOnlyItsUsages()
        {
            var deleted = CreateProbe($"{ScannedFolder}/Deleted.asset");
            var kept = CreateProbe($"{ScannedFolder}/Kept.asset");
            WarmIndex();

            AssetDatabase.DeleteAsset($"{ScannedFolder}/Deleted.asset");
            SerializeReferenceTypeUsageIndexInvalidator.Apply(_none, new[] { $"{ScannedFolder}/Deleted.asset" }, _none, _none);

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm, "A delete must not drop the warm index.");
            Assert.AreEqual(0, UsageCount(deleted));
            Assert.AreEqual(1, UsageCount(kept));
        }

        [Test]
        public void Apply_ScannedAssetMovedBetweenScannedFolders_KeepsItsUsages()
        {
            var guid = CreateProbe($"{ScannedFolder}/Moved.asset");
            WarmIndex();

            Assert.IsEmpty(AssetDatabase.MoveAsset($"{ScannedFolder}/Moved.asset", $"{MovedToFolder}/Moved.asset"));
            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                _none, _none, new[] { $"{MovedToFolder}/Moved.asset" }, new[] { $"{ScannedFolder}/Moved.asset" });

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm, "A move between scanned folders must not drop the warm index.");
            Assert.AreEqual(1, UsageCount(guid));
        }

        [Test]
        public void Apply_ScannedAssetMovedIntoExcludedFolder_DropsItsUsages()
        {
            var guid = CreateProbe($"{ScannedFolder}/Moved.asset");
            SerializeReferenceSettings.ExcludedFolders = new[] { ExcludedFolder + "/" };
            WarmIndex();
            Assert.AreEqual(1, UsageCount(guid));

            Assert.IsEmpty(AssetDatabase.MoveAsset($"{ScannedFolder}/Moved.asset", $"{ExcludedFolder}/Moved.asset"));
            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                _none, _none, new[] { $"{ExcludedFolder}/Moved.asset" }, new[] { $"{ScannedFolder}/Moved.asset" });

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm);
            Assert.AreEqual(0, UsageCount(guid));
        }

        [Test]
        public void Apply_ExcludedAssetMovedIntoScannedFolder_AddsItsUsages()
        {
            var guid = CreateProbe($"{ExcludedFolder}/Moved.asset");
            SerializeReferenceSettings.ExcludedFolders = new[] { ExcludedFolder + "/" };
            WarmIndex();
            Assert.AreEqual(0, UsageCount(guid));

            Assert.IsEmpty(AssetDatabase.MoveAsset($"{ExcludedFolder}/Moved.asset", $"{ScannedFolder}/Moved.asset"));
            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                _none, _none, new[] { $"{ScannedFolder}/Moved.asset" }, new[] { $"{ExcludedFolder}/Moved.asset" });

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm);
            Assert.AreEqual(1, UsageCount(guid));
        }

        [Test]
        public void Apply_ScannedAssetImported_ReplacesItsUsages()
        {
            var guid = CreateProbe($"{ScannedFolder}/Imported.asset");
            WarmIndex();

            var probe = AssetDatabase.LoadAssetAtPath<LinkerTestObject>($"{ScannedFolder}/Imported.asset");
            probe.b = new DeleteGuardPistol();
            EditorUtility.SetDirty(probe);
            AssetDatabase.SaveAssets();
            SerializeReferenceTypeUsageIndexInvalidator.Apply(new[] { $"{ScannedFolder}/Imported.asset" }, _none, _none, _none);

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm);
            Assert.AreEqual(2, SerializeReferenceTypeUsageIndex.AllUsages().Count(usage => usage.Guid == guid));
        }

        [Test]
        public void Apply_SeveralAssetsImported_ReplacesTheirUsagesOnly()
        {
            var first = CreateProbe($"{ScannedFolder}/First.asset");
            var second = CreateProbe($"{ScannedFolder}/Second.asset");
            var untouched = CreateProbe($"{ScannedFolder}/Untouched.asset");
            WarmIndex();

            foreach (var name in new[] { "First", "Second" })
            {
                var probe = AssetDatabase.LoadAssetAtPath<LinkerTestObject>($"{ScannedFolder}/{name}.asset");
                probe.b = new DeleteGuardPistol();
                EditorUtility.SetDirty(probe);
            }

            AssetDatabase.SaveAssets();
            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                new[] { $"{ScannedFolder}/First.asset", $"{ScannedFolder}/Second.asset" }, _none, _none, _none);

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm);
            Assert.AreEqual(2, UsageCount(first));
            Assert.AreEqual(2, UsageCount(second));
            Assert.AreEqual(1, UsageCount(untouched));
        }

        [Test]
        public void Apply_SmallBatch_KeepsIndexWarm()
        {
            WarmIndex();

            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                Enumerable.Range(0, 5).Select(i => $"{ScannedFolder}/Unknown{i}.asset").ToArray(), _none, _none, _none);

            Assert.IsTrue(SerializeReferenceTypeUsageIndex.IsWarm);
        }

        [Test]
        public void Apply_BigBatch_ResetsIndex()
        {
            WarmIndex();

            SerializeReferenceTypeUsageIndexInvalidator.Apply(
                Enumerable.Range(0, 1000).Select(i => $"{ScannedFolder}/Unknown{i}.asset").ToArray(), _none, _none, _none);

            Assert.IsFalse(SerializeReferenceTypeUsageIndex.IsWarm);
        }

        [Test]
        public void Apply_ScriptImported_ResetsIndex()
        {
            WarmIndex();

            SerializeReferenceTypeUsageIndexInvalidator.Apply(new[] { $"{ScannedFolder}/Spear.cs" }, _none, _none, _none);

            Assert.IsFalse(SerializeReferenceTypeUsageIndex.IsWarm);
        }

        private static void WarmIndex() => SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

        // The usages of one asset, found by guid so a probe of another test cannot change the count.
        private static int UsageCount(string guid) =>
            SerializeReferenceTypeUsageIndex.AllUsages().Count(usage => usage.Guid == guid);

        private static string CreateProbe(string path)
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = new TestSword();

            AssetDatabase.CreateAsset(probe, path);
            AssetDatabase.SaveAssets();

            // Creating the asset patches a warm index in place; each case starts from a cold one.
            SerializeReferenceTypeUsageIndex.Reset();
            return AssetDatabase.AssetPathToGUID(path);
        }
    }
}
