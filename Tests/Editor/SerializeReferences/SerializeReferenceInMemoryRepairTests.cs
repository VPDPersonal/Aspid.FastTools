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
        public void SetUp()
        {
            // Untitled scenes block additive creation, so the fixture replaces the open scene set.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("Holder");
            go.AddComponent<InMemoryRepairTestComponent>().value = new InMemoryRepairPayload { x = 3 };

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
    }
}
