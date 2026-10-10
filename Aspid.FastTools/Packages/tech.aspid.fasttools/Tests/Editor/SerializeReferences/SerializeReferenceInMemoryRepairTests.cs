using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using Aspid.FastTools.Tests;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the in-memory Fix of a missing type in an open saved scene. Unity's Undo snapshot does not hold
    /// missing-type data, so the replaced entry must survive until save: Ctrl+Z has to bring back the original
    /// reference with its payload, and only a save may drop the entry.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceInMemoryRepairTests
    {
        private const string ScenePath = "Assets/AspidInMemoryRepairTest.unity";
        private const string MissingClass = "InMemoryRepairGone";

        private Scene _scene;
        private InMemoryRepairTestComponent _component;

        [SetUp]
        public void SetUp() =>
            OpenSceneWithMissingType(component => component.value = new InMemoryRepairPayload { x = 3 });

        private void OpenSceneWithMissingType(Action<InMemoryRepairTestComponent> populate)
        {
            // Untitled scenes block additive creation, so the fixture replaces the open scene set.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("Holder");
            populate(go.AddComponent<InMemoryRepairTestComponent>());

            Assert.IsTrue(EditorSceneManager.SaveScene(scene, ScenePath));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var text = File.ReadAllText(ScenePath);
            Assert.That(text, Does.Contain($"class: {nameof(InMemoryRepairPayload)},"));
            File.WriteAllText(ScenePath, text.Replace($"class: {nameof(InMemoryRepairPayload)},", $"class: {MissingClass},"));
            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceUpdate);

            _scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            _component = _scene.GetRootGameObjects()[0].GetComponent<InMemoryRepairTestComponent>();
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(_component), "The fixture must load as a missing type.");
        }

        [TearDown]
        public void TearDown()
        {
            if (_component != null) Undo.ClearUndo(_component);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AssetDatabase.DeleteAsset(ScenePath);
        }

        [Test]
        public void FixInMemory_ThenUndo_RestoresMissingReferenceAndPayload()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            // SerializedProperty reports a missing-type pointer as null (-2), so the rid comes from the scene file.
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Assert.IsInstanceOf<InMemoryRepairReplacement>(_component.value);

            Undo.PerformUndo();
            serializedObject.Update();

            Assert.IsNull(_component.value, "Undo must point the field back at the missing reference.");
            var entry = SerializationUtility.GetManagedReferencesWithMissingTypes(_component)
                .FirstOrDefault(candidate => candidate.referenceId == referenceId);
            Assert.AreEqual(MissingClass, entry.className, "The missing-type entry must still exist after Undo.");
            Assert.That(entry.serializedData, Does.Contain("x: 3"), "The stored payload must survive Undo.");
        }

        [Test]
        public void FixInMemory_ThenSave_DropsReplacedEntryFromFile()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.AreEqual(3, ((InMemoryRepairReplacement)_component.value).x);

            var text = File.ReadAllText(ScenePath);
            Assert.That(text, Does.Not.Contain(MissingClass));
            Assert.That(text, Does.Contain($"class: {nameof(InMemoryRepairReplacement)},"));
        }

        [Test]
        public void FixInMemory_ThenUndoAndSave_KeepsMissingEntryInFile()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.PerformUndo();
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Contain($"value:\n    rid: {referenceId}\n"), "The field must still point at the missing rid.");
            Assert.That(text, Does.Contain($"- rid: {referenceId}\n      type: {{class: {MissingClass},"));
            Assert.That(text, Does.Contain("x: 3"));
        }

        [Test]
        public void FixInMemory_ThenUndoRedoAndSave_DropsReplacedEntryFromFile()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.PerformUndo();
            Undo.PerformRedo();
            Assert.IsInstanceOf<InMemoryRepairReplacement>(_component.value);
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.That(File.ReadAllText(ScenePath), Does.Not.Contain(MissingClass));
        }

        // Unity writes a null field over a live missing entry back as a pointer to that entry, so the entry of a fix
        // that was overwritten rather than undone must still go on save.
        [Test]
        public void FixInMemory_ThenSetNoneAndSave_WritesEmptyField()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.IncrementCurrentGroup();
            SetValue(serializedObject, null);
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            AssertSavedAsEmptyField();
        }

        // An undo step that does not reach the fix leaves it applied, even with the repaired instance gone.
        [Test]
        public void FixInMemory_ThenSetNoneEditAndUndoEditAndSave_WritesEmptyField()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.IncrementCurrentGroup();
            SetValue(serializedObject, null);
            Undo.IncrementCurrentGroup();
            AddListElement(serializedObject);
            Undo.PerformUndo();
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            AssertSavedAsEmptyField();
        }

        [Test]
        public void FixInMemory_ThenRetypeAndSave_DropsReplacedEntry()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.IncrementCurrentGroup();
            SetValue(serializedObject, new InMemoryRepairPayload { x = 9 });
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.That(File.ReadAllText(ScenePath), Does.Not.Contain(MissingClass));
        }

        // A later edit cannot redo an undone fix, so undoing or redoing that edit leaves the fix undone.
        [Test]
        public void FixInMemory_ThenUndoEditUndoRedoAndSave_KeepsMissingEntryInFile()
        {
            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.PerformUndo();
            serializedObject.Update();
            Undo.IncrementCurrentGroup();
            AddListElement(serializedObject);
            Undo.PerformUndo();
            Undo.PerformRedo();
            Assert.AreEqual(1, _component.list.Count);
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Contain($"value:\n    rid: {referenceId}\n"), "The field must still point at the missing rid.");
            Assert.That(text, Does.Contain($"- rid: {referenceId}\n      type: {{class: {MissingClass},"));
        }

        [Test]
        public void FixListElement_ThenDeleteElementAndSave_DropsReplacedEntry()
        {
            TearDown();
            OpenSceneWithMissingType(component => component.list.AddRange(new object[]
            {
                new InMemoryRepairReplacement { x = 1 },
                new InMemoryRepairPayload { x = 3 },
            }));

            using var serializedObject = new SerializedObject(_component);
            var list = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.list));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(list.GetArrayElementAtIndex(1), typeof(InMemoryRepairReplacement)));
            serializedObject.Update();
            Undo.IncrementCurrentGroup();
            list.DeleteArrayElementAtIndex(1);
            serializedObject.ApplyModifiedProperties();
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.AreEqual(1, _component.list.Count);
            Assert.That(File.ReadAllText(ScenePath), Does.Not.Contain(MissingClass));
        }

        // No scene or prefab save hook fires for an asset, so a dirty asset repaired in memory must drop the replaced
        // entry right away, or the next save writes it back.
        [Test]
        public void FixDirtyAssetInMemory_ThenSave_DropsReplacedEntryFromFile()
        {
            const string assetPath = "Assets/AspidInMemoryRepairTest.asset";
            var asset = UnityEngine.ScriptableObject.CreateInstance<InMemoryRepairTestObject>();
            asset.value = new InMemoryRepairPayload { x = 3 };
            AssetDatabase.CreateAsset(asset, assetPath);

            try
            {
                var text = File.ReadAllText(assetPath);
                File.WriteAllText(assetPath, text.Replace($"class: {nameof(InMemoryRepairPayload)},", $"class: {MissingClass},"));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                asset = AssetDatabase.LoadAssetAtPath<InMemoryRepairTestObject>(assetPath);
                Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(asset), "The fixture must load as a missing type.");
                EditorUtility.SetDirty(asset);

                using (var serializedObject = new SerializedObject(asset))
                {
                    var property = serializedObject.FindProperty(nameof(InMemoryRepairTestObject.value));
                    Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));
                    Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingTypeInMemory(property, typeof(InMemoryRepairReplacement), referenceId));
                }

                AssetDatabase.SaveAssetIfDirty(asset);

                Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(asset));
                Assert.AreEqual(3, ((InMemoryRepairReplacement)asset.value).x);
                Assert.That(File.ReadAllText(assetPath), Does.Not.Contain(MissingClass));
            }
            finally
            {
                if (asset != null) Undo.ClearUndo(asset);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static void SetValue(SerializedObject serializedObject, object value)
        {
            serializedObject.Update();
            serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value)).managedReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void AddListElement(SerializedObject serializedObject)
        {
            serializedObject.Update();
            serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.list)).arraySize++;
            serializedObject.ApplyModifiedProperties();
        }

        private void AssertSavedAsEmptyField()
        {
            Assert.IsNull(_component.value);
            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Not.Contain(MissingClass));
            Assert.That(text, Does.Contain("value:\n    rid: -2\n"));
        }

        // The undone fix is not tracked by its path: after the list shifts, the fixed index holds another element,
        // and clearing the entry on save would leave Undo of the shift pointing at nothing.
        // The shift itself stores the missing element as null (Unity's behaviour), so only Undo brings it back.
        [Test]
        public void FixListElement_ThenUndoAndShiftListAndSave_KeepsMissingEntryForUndo()
        {
            TearDown();
            OpenSceneWithMissingType(component => component.list.AddRange(new object[]
            {
                new InMemoryRepairReplacement { x = 1 },
                new InMemoryRepairPayload { x = 3 },
                new InMemoryRepairReplacement { x = 5 },
            }));

            using var serializedObject = new SerializedObject(_component);
            var list = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.list));
            var property = list.GetArrayElementAtIndex(1);
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.PerformUndo();
            serializedObject.Update();

            Undo.IncrementCurrentGroup();
            list.DeleteArrayElementAtIndex(0);
            serializedObject.ApplyModifiedProperties();
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            var entry = SerializationUtility.GetManagedReferencesWithMissingTypes(_component)
                .FirstOrDefault(candidate => candidate.referenceId == referenceId);
            Assert.AreEqual(MissingClass, entry.className, "Saving must not clear the entry of an undone fix.");

            Undo.PerformUndo();
            Assert.AreEqual(3, _component.list.Count);
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Match($@"list:\n  - rid: -?\d+\n  - rid: {referenceId}\n"), "The list must point at the missing rid again.");
            Assert.That(text, Does.Contain($"- rid: {referenceId}\n      type: {{class: {MissingClass},"));
            Assert.That(text, Does.Contain("x: 3"));
        }

        // Unity reports every field on a missing type as empty, so the save that drops the replaced entry would
        // write a field left on it as empty.
        [Test]
        public void FixInMemory_SharedMissingReference_RepairsEveryFieldAndKeepsThemShared()
        {
            TearDown();
            OpenSceneWithMissingType(component =>
            {
                var payload = new InMemoryRepairPayload { x = 3 };
                component.value = payload;
                component.other = payload;
            });

            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Assert.IsInstanceOf<InMemoryRepairReplacement>(_component.other, "The other field on the missing rid must be repaired too.");
            Assert.AreSame(_component.value, _component.other, "Both fields must keep sharing one instance.");

            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.AreEqual(3, ((InMemoryRepairReplacement)_component.other).x);

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Not.Contain(MissingClass));
            Assert.That(text, Does.Not.Contain("other:\n    rid: -2\n"), "The other field must not be saved empty.");
        }

        // The saved file still puts the missing rid in a field set to <None> since the save; the Fix must leave it empty.
        [Test]
        public void FixInMemory_SharedMissingReference_LeavesFieldSetToNoneEmpty()
        {
            TearDown();
            OpenSceneWithMissingType(component =>
            {
                var payload = new InMemoryRepairPayload { x = 3 };
                component.value = payload;
                component.other = payload;
            });

            using var serializedObject = new SerializedObject(_component);
            serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.other)).managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();

            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));

            Assert.IsInstanceOf<InMemoryRepairReplacement>(_component.value);
            Assert.IsNull(_component.other, "A field set to <None> before the Fix must stay empty.");
        }

        // An unsaved insert shifts the list, so the saved file puts the missing rid at the wrong index.
        [Test]
        public void FixInMemory_SharedMissingReference_FollowsListShiftedSinceSave()
        {
            TearDown();
            OpenSceneWithMissingType(component =>
            {
                var payload = new InMemoryRepairPayload { x = 3 };
                component.value = payload;
                component.list.Add(payload);
            });

            using var serializedObject = new SerializedObject(_component);
            var list = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.list));
            list.InsertArrayElementAtIndex(0);
            list.GetArrayElementAtIndex(0).managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();

            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));

            Assert.AreEqual(2, _component.list.Count);
            Assert.IsNull(_component.list[0], "The inserted empty element must stay empty.");
            Assert.AreSame(_component.value, _component.list[1], "The shifted element must share the repaired instance.");
        }

        [Test]
        public void FixInMemory_SharedMissingReference_RefusesTypeAnotherFieldCannotHold()
        {
            TearDown();
            OpenSceneWithMissingType(component =>
            {
                var payload = new InMemoryRepairPayload { x = 3 };
                component.value = payload;
                component.shape = payload;
            });

            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));

            LogAssert.Expect(LogType.Warning, new Regex("does not fit 'shape'"));
            Assert.IsFalse(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Assert.IsNull(_component.value);
            Assert.IsFalse(_scene.isDirty, "A refused fix must not touch the scene.");

            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairShapedReplacement)));
            Assert.IsInstanceOf<InMemoryRepairShapedReplacement>(_component.shape);
            Assert.AreSame(_component.value, _component.shape);
            Assert.AreEqual(3, ((InMemoryRepairShapedReplacement)_component.shape).x);
        }

        // One Undo step must bring every repaired field back to the missing rid, and the save must keep the entry.
        [Test]
        public void FixInMemory_SharedMissingReference_ThenUndoAndSave_KeepsEveryFieldOnMissingEntry()
        {
            TearDown();
            OpenSceneWithMissingType(component =>
            {
                var payload = new InMemoryRepairPayload { x = 3 };
                component.value = payload;
                component.other = payload;
            });

            using var serializedObject = new SerializedObject(_component);
            var property = serializedObject.FindProperty(nameof(InMemoryRepairTestComponent.value));
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
            Undo.PerformUndo();

            Assert.IsNull(_component.value);
            Assert.IsNull(_component.other);
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(_component));
            Assert.IsTrue(EditorSceneManager.SaveScene(_scene));

            var text = File.ReadAllText(ScenePath).Replace("\r\n", "\n");
            Assert.That(text, Does.Contain($"value:\n    rid: {referenceId}\n"), "The field must still point at the missing rid.");
            Assert.That(text, Does.Contain($"other:\n    rid: {referenceId}\n"), "The other field must still point at the missing rid.");
            Assert.That(text, Does.Contain($"- rid: {referenceId}\n      type: {{class: {MissingClass},"));
            Assert.That(text, Does.Contain("x: 3"));
        }

        // The YAML route retypes the shared entry itself, so a field that cannot hold the new type would load empty.
        [Test]
        public void FixAsset_SharedMissingReference_RefusesTypeAnotherFieldCannotHold()
        {
            const string assetPath = "Assets/AspidSharedRepairTest.asset";
            var asset = ScriptableObject.CreateInstance<InMemoryRepairTestObject>();
            var payload = new InMemoryRepairPayload { x = 3 };
            asset.value = payload;
            asset.shape = payload;
            AssetDatabase.CreateAsset(asset, assetPath);

            try
            {
                var broken = File.ReadAllText(assetPath)
                    .Replace($"class: {nameof(InMemoryRepairPayload)},", $"class: {MissingClass},");
                File.WriteAllText(assetPath, broken);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                asset = AssetDatabase.LoadAssetAtPath<InMemoryRepairTestObject>(assetPath);
                Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(asset), "The fixture must load as a missing type.");

                using (var serializedObject = new SerializedObject(asset))
                {
                    var property = serializedObject.FindProperty(nameof(InMemoryRepairTestObject.value));
                    LogAssert.Expect(LogType.Warning, new Regex("does not fit 'shape'"));
                    Assert.IsFalse(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairReplacement)));
                }

                Assert.AreEqual(broken, File.ReadAllText(assetPath), "A refused fix must leave the file unchanged.");

                using (var serializedObject = new SerializedObject(asset))
                {
                    var property = serializedObject.FindProperty(nameof(InMemoryRepairTestObject.value));
                    Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingType(property, typeof(InMemoryRepairShapedReplacement)));
                }

                asset = AssetDatabase.LoadAssetAtPath<InMemoryRepairTestObject>(assetPath);
                Assert.IsInstanceOf<InMemoryRepairShapedReplacement>(asset.shape);
                Assert.AreSame(asset.value, asset.shape, "Both fields must keep sharing one instance.");
                Assert.AreEqual(3, ((InMemoryRepairShapedReplacement)asset.shape).x);
            }
            finally
            {
                if (asset != null) Undo.ClearUndo(asset);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        // Unity hands the payload back as YAML: floats in exponent form, non-ASCII text in escaped double quotes and
        // long text wrapped onto several lines must all reach the replacement.
        [Test]
        public void FixDirtyAssetInMemory_RecoversQuotedWrappedAndExponentScalars()
        {
            const string assetPath = "Assets/AspidInMemoryRecoveryTest.asset";
            var asset = ScriptableObject.CreateInstance<InMemoryRepairTestObject>();
            asset.value = new InMemoryRecoveryPayload();
            AssetDatabase.CreateAsset(asset, assetPath);

            try
            {
                var text = File.ReadAllText(assetPath);
                File.WriteAllText(assetPath, text.Replace($"class: {nameof(InMemoryRecoveryPayload)},", $"class: {MissingClass},"));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                asset = AssetDatabase.LoadAssetAtPath<InMemoryRepairTestObject>(assetPath);
                Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(asset), "The fixture must load as a missing type.");
                EditorUtility.SetDirty(asset);

                using (var serializedObject = new SerializedObject(asset))
                {
                    var property = serializedObject.FindProperty(nameof(InMemoryRepairTestObject.value));
                    Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var referenceId));
                    Assert.IsTrue(SerializeReferenceHelpers.TryFixMissingTypeInMemory(property, typeof(InMemoryRecoveryReplacement), referenceId));
                }

                var expected = new InMemoryRecoveryPayload();
                var recovered = (InMemoryRecoveryReplacement)asset.value;

                Assert.AreEqual(expected.unicode, recovered.unicode);
                Assert.AreEqual(expected.wrapped, recovered.wrapped);
                Assert.AreEqual(expected.lines, recovered.lines);
                Assert.AreEqual(expected.number, recovered.number, "A number-like string must stay text.");
                Assert.AreEqual(expected.tiny, recovered.tiny);
                Assert.AreEqual(expected.huge, recovered.huge);
                Assert.AreEqual(expected.infinity, recovered.infinity);
                Assert.AreEqual(expected.negativeInfinity, recovered.negativeInfinity);
                Assert.IsNaN(recovered.nan);
                Assert.AreEqual(expected.precise, recovered.precise);
                Assert.AreEqual(expected.count, recovered.count);
            }
            finally
            {
                if (asset != null) Undo.ClearUndo(asset);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
    }

    [Serializable]
    public sealed class InMemoryRecoveryPayload
    {
        public string unicode = "Привет, мир";
        public string wrapped = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore.";
        public string lines = "line one\nline two";
        public string number = "1.5";
        public float tiny = 9.99e-05f;
        public float huge = 3.4e38f;
        public float infinity = float.PositiveInfinity;
        public float negativeInfinity = float.NegativeInfinity;
        public float nan = float.NaN;
        public double precise = 1e300;
        public int count = 7;
    }

    [Serializable]
    public sealed class InMemoryRecoveryReplacement
    {
        public string unicode;
        public string wrapped;
        public string lines;
        public string number;
        public float tiny;
        public float huge;
        public float infinity;
        public float negativeInfinity;
        public float nan;
        public double precise;
        public int count;
    }
}
