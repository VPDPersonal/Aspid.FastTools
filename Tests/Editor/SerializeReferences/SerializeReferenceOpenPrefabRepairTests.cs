using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.Serialization;
using UnityEditor.SceneManagement;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for the Inspector Fix on a prefab asset selected in the Project window while that prefab is open in
    // Prefab Mode: the stage saves over the asset file, so a file rewrite under it must be refused. A MonoBehaviour
    // from this editor-only assembly cannot be attached, so the broken reference lives on a ScriptableObject embedded
    // in the prefab file; the guard keys on the file path either way.
    [TestFixture]
    internal sealed class SerializeReferenceOpenPrefabRepairTests
    {
        private const string ProbePrefabPath = "Assets/__AspidOpenPrefabRepairProbe__.prefab";

        private static readonly ManagedTypeName _goneType = new(
            ManagedTypeName.FromType(typeof(TestSword)).Assembly,
            ManagedTypeName.FromType(typeof(TestSword)).Namespace,
            "TestSwordGone");

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("OpenPrefabRepairProbe");
            PrefabUtility.SaveAsPrefabAsset(go, ProbePrefabPath);
            Object.DestroyImmediate(go);

            var probe = ScriptableObject.CreateInstance<UnsavedRepairTestObject>();
            probe.a = new TestSword { damage = 1 };
            AssetDatabase.AddObjectToAsset(probe, ProbePrefabPath);
            AssetDatabase.SaveAssets();

            // A sub-asset gets a random local id, often negative, which the YAML tooling does not read yet; pin a
            // positive one so the file route under test finds the document.
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);
            if (fileId < 0)
            {
                File.WriteAllText(ProbePrefabPath, File.ReadAllText(ProbePrefabPath).Replace($"&{fileId}", $"&{-fileId}"));
                AssetDatabase.ImportAsset(ProbePrefabPath, ImportAssetOptions.ForceUpdate);
                probe = LoadProbe();
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out fileId);
            }
            Assert.Greater(fileId, 0, "Precondition: the sub-asset must have a positive local id.");

            // Break field a on disk: its stored class no longer resolves.
            var rid = ManagedReferenceUtility.GetManagedReferenceIdForObject(probe, probe.a);
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(ProbePrefabPath, fileId, rid, _goneType));
            AssetDatabase.ImportAsset(ProbePrefabPath, ImportAssetOptions.ForceUpdate);

            Assert.IsNull(LoadProbe().a, "Precondition: field a must be a missing reference.");
        }

        [TearDown]
        public void TearDown()
        {
            StageUtility.GoToMainStage();
            AssetDatabase.DeleteAsset(ProbePrefabPath);
        }

        [Test]
        public void TryFixMissingType_AssetOpenInPrefabMode_LeavesFileUntouched()
        {
            // Outside batch mode the refusal dialog would wait for a click; in batch mode Unity cancels it with a log.
            if (!Application.isBatchMode) Assert.Ignore("Runs in batch mode only.");
            LogAssert.Expect(LogType.Assert, new Regex("Cancelling DisplayDialog: Fix Missing Type"));

            Assert.IsNotNull(PrefabStageUtility.OpenPrefab(ProbePrefabPath), "Precondition: the prefab must open.");
            var before = File.ReadAllText(ProbePrefabPath);

            using (var serializedObject = new SerializedObject(LoadProbe()))
            {
                Assert.IsFalse(SerializeReferenceHelpers.TryFixMissingType(serializedObject.FindProperty("a"), typeof(TestSword)),
                    "The open stage would overwrite a file rewrite on its next save.");
            }

            Assert.AreEqual(before, File.ReadAllText(ProbePrefabPath));
        }

        private static UnsavedRepairTestObject LoadProbe() =>
            AssetDatabase.LoadAssetAtPath<UnsavedRepairTestObject>(ProbePrefabPath);
    }
}
