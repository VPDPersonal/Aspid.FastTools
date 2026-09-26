using System;
using System.IO;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using Aspid.FastTools.Tests;
using UnityEngine.SceneManagement;

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

        // The fix is recorded by the repaired instance's rid, not by its path: after the list shifts, the fixed index
        // holds another element, and clearing the entry on save would leave Undo of the shift pointing at nothing.
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
    }
}
