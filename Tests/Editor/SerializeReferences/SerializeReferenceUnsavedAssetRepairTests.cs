using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for repairs on a loaded asset with unsaved changes. A file rewrite reimports the asset, which reloads
    /// it from disk and silently drops those changes, so the Inspector Fix must refuse until the asset is saved, the
    /// batch filters must hold it back and the in-memory clear must reach it.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceUnsavedAssetRepairTests
    {
        private const string ProbeAssetPath = "Assets/__AspidUnsavedAssetRepairProbe__.asset";
        private const float StoredDelay = 1.5f;
        private const int SavedDamage = 5;
        private const int UnsavedDamage = 42;

        private static readonly List<int> _storedWaves = new() { 1, 2, 3 };

        private static readonly ManagedTypeName _goneType = new(
            ManagedTypeName.FromType(typeof(UnsavedRepairSpawnAction)).Assembly,
            ManagedTypeName.FromType(typeof(UnsavedRepairSpawnAction)).Namespace,
            "UnsavedRepairSpawnActionGone");

        private UnsavedRepairTestObject _probe;

        [SetUp]
        public void SetUp()
        {
            var probe = ScriptableObject.CreateInstance<UnsavedRepairTestObject>();
            probe.a = new UnsavedRepairSpawnAction { delay = StoredDelay, waves = new List<int>(_storedWaves) };
            probe.b = new TestSword { damage = SavedDamage };
            AssetDatabase.CreateAsset(probe, ProbeAssetPath);

            // Break field a on disk: its stored class no longer resolves.
            var rid = ManagedReferenceUtility.GetManagedReferenceIdForObject(probe, probe.a);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(ProbeAssetPath, fileId, rid, _goneType));
            AssetDatabase.ImportAsset(ProbeAssetPath, ImportAssetOptions.ForceUpdate);

            _probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(ProbeAssetPath);
            Assert.IsNull(_probe.a, "Precondition: field a must be a missing reference.");

            // An unsaved Inspector edit on another field.
            using (var serializedObject = new SerializedObject(_probe))
            {
                serializedObject.FindProperty("b.damage").intValue = UnsavedDamage;
                serializedObject.ApplyModifiedProperties();
            }

            Assert.IsTrue(EditorUtility.IsDirty(_probe), "Precondition: the asset must have unsaved changes.");
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(ProbeAssetPath);

        [Test]
        public void Guard_DirtyLoadedAsset_IsNotRewriteSafeUntilSaved()
        {
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.HasUnsavedChanges(ProbeAssetPath));
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.IsWritable(ProbeAssetPath, null),
                "IsWritable is an open-copy check: an asset being saved is still dirty and must stay guarded.");
            Assert.IsFalse(SerializeReferenceOpenCopyGuard.IsRewriteSafe(ProbeAssetPath, null),
                "A file rewrite would reload the asset and discard its unsaved changes.");

            AssetDatabase.SaveAssetIfDirty(_probe);

            Assert.IsFalse(SerializeReferenceOpenCopyGuard.HasUnsavedChanges(ProbeAssetPath));
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.IsRewriteSafe(ProbeAssetPath, null));
        }

        [Test]
        public void TryFixMissingType_DirtyAsset_RefusesAndKeepsUnsavedChanges()
        {
            // Outside batch mode the Save and Continue prompt would wait for a click.
            if (!Application.isBatchMode) Assert.Ignore("Runs in batch mode only.");

            var before = File.ReadAllText(ProbeAssetPath);

            using (var serializedObject = new SerializedObject(_probe))
            {
                Assert.IsFalse(SerializeReferenceHelpers.TryFixMissingType(serializedObject.FindProperty("a"), typeof(UnsavedRepairSpawnAction)));
            }

            Assert.AreEqual(before, File.ReadAllText(ProbeAssetPath));
            Assert.AreEqual(UnsavedDamage, ((TestSword)_probe.b).damage, "The refused fix must not discard the unsaved edit.");
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(_probe),
                "The refused fix must not repair the reference in memory, where its nested data would be lost.");
        }

        [Test]
        public void TryFixMissingType_AssetSavedFirst_KeepsNestedDataAndUnsavedChanges()
        {
            // What Save and Continue does before the file rewrite.
            AssetDatabase.SaveAssetIfDirty(_probe);

            using (var serializedObject = new SerializedObject(_probe))
            {
                Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(serializedObject.FindProperty("a"), typeof(UnsavedRepairSpawnAction)));
            }

            var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(ProbeAssetPath);
            Assert.AreEqual(UnsavedDamage, ((TestSword)probe.b).damage, "The repair must not discard the saved edit.");
            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(probe));

            var action = probe.a as UnsavedRepairSpawnAction;
            Assert.IsNotNull(action);
            Assert.AreEqual(StoredDelay, action.delay);
            CollectionAssert.AreEqual(_storedWaves, action.waves, "The repair must keep the reference's nested data.");
        }

        [Test]
        public void ApplyFix_DirtyAsset_RefusesAndKeepsUnsavedChanges()
        {
            // Outside batch mode the Save and Continue prompt would wait for a click.
            if (!Application.isBatchMode) Assert.Ignore("Runs in batch mode only.");

            var rid = SerializationUtility.GetManagedReferencesWithMissingTypes(_probe)[0].referenceId;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_probe, out _, out long fileId);
            var before = File.ReadAllText(ProbeAssetPath);

            Assert.IsFalse(SerializeReferenceGraphEditor.ApplyFix(ProbeAssetPath, fileId, rid, typeof(UnsavedRepairSpawnAction).AssemblyQualifiedName));

            Assert.AreEqual(before, File.ReadAllText(ProbeAssetPath));
            var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(ProbeAssetPath);
            Assert.AreEqual(UnsavedDamage, ((TestSword)probe.b).damage, "The refused fix must not discard the unsaved edit.");
        }

        [Test]
        public void TryClearMissingReferenceInMemory_DirtyAsset_ClearsOnTheLoadedObject()
        {
            var rid = SerializationUtility.GetManagedReferencesWithMissingTypes(_probe)[0].referenceId;

            Assert.IsTrue(SerializeReferenceHelpers.TryClearMissingReferenceInMemory(ProbeAssetPath, rid, _goneType));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_probe));
            Assert.AreEqual(UnsavedDamage, ((TestSword)_probe.b).damage, "The clear must not discard the unsaved edit.");
        }
    }
}
