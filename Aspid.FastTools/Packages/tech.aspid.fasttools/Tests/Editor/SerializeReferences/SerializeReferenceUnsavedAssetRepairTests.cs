using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for repairs on a loaded asset with unsaved changes. A file rewrite reimports the asset, which reloads
    /// it from disk and silently drops those changes, so the Inspector Fix, the in-memory clear and the open-copy guard
    /// must treat such an asset as an open copy.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceUnsavedAssetRepairTests
    {
        private const string ProbeAssetPath = "Assets/__AspidUnsavedAssetRepairProbe__.asset";
        private const int StoredDamage = 1;
        private const int SavedDamage = 5;
        private const int UnsavedDamage = 42;

        private static readonly ManagedTypeName _goneType = new(
            ManagedTypeName.FromType(typeof(TestSword)).Assembly,
            ManagedTypeName.FromType(typeof(TestSword)).Namespace,
            "TestSwordGone");

        private UnsavedRepairTestObject _probe;

        [SetUp]
        public void SetUp()
        {
            var probe = ScriptableObject.CreateInstance<UnsavedRepairTestObject>();
            probe.a = new TestSword { damage = StoredDamage };
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
        public void Guard_DirtyLoadedAsset_IsNotWritableUntilSaved()
        {
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.HasUnsavedChanges(ProbeAssetPath));
            Assert.IsFalse(SerializeReferenceOpenCopyGuard.IsWritable(ProbeAssetPath, null),
                "A file rewrite would reload the asset and discard its unsaved changes.");

            AssetDatabase.SaveAssetIfDirty(_probe);

            Assert.IsFalse(SerializeReferenceOpenCopyGuard.HasUnsavedChanges(ProbeAssetPath));
            Assert.IsTrue(SerializeReferenceOpenCopyGuard.IsWritable(ProbeAssetPath, null));
        }

        [Test]
        public void TryFixMissingType_DirtyAsset_KeepsUnsavedChanges()
        {
            using (var serializedObject = new SerializedObject(_probe))
            {
                Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(serializedObject.FindProperty("a"), typeof(TestSword)));
            }

            var probe = AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(ProbeAssetPath);
            Assert.AreEqual(UnsavedDamage, ((TestSword)probe.b).damage, "The repair must not discard the unsaved edit.");
            Assert.IsInstanceOf<TestSword>(probe.a);
            Assert.AreEqual(StoredDamage, ((TestSword)probe.a).damage, "The repair must keep the reference's stored data.");
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
