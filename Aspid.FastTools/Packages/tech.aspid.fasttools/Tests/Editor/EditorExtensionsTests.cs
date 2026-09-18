using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    internal sealed class EditorExtensionsTests
    {
        private GameObject _gameObject;
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            _scene = EditorSceneManager.NewPreviewScene();
            _gameObject = new GameObject("Display name test");
            SceneManager.MoveGameObjectToScene(_gameObject, _scene);
        }

        [TearDown]
        public void TearDown() => EditorSceneManager.ClosePreviewScene(_scene);

        [Test]
        public void NullObjects_ReturnEmptyNames()
        {
            Assert.AreEqual(string.Empty, ((Object)null).GetDisplayName());
            Assert.AreEqual(string.Empty, ((Component)null).GetDisplayNameWithIndex());
        }

        [Test]
        public void DestroyedObjects_ReturnEmptyNames()
        {
            var component = _gameObject.AddComponent<AudioSource>();
            Object.DestroyImmediate(component);

            Assert.AreEqual(string.Empty, component.GetDisplayName());
            Assert.AreEqual(string.Empty, component.GetDisplayNameWithIndex());
        }

        [Test]
        public void DisplayName_UsesTypeInsteadOfObjectName()
        {
            var component = _gameObject.AddComponent<AudioSource>();

            Assert.AreEqual("Audio Source", component.GetDisplayName());
        }

        [Test]
        public void SingleComponent_HasNoIndex()
        {
            var component = _gameObject.AddComponent<AudioSource>();

            Assert.AreEqual("Audio Source", component.GetDisplayNameWithIndex());
        }

        [Test]
        public void DuplicateComponents_HaveOneBasedIndices()
        {
            var first = _gameObject.AddComponent<AudioSource>();
            var second = _gameObject.AddComponent<AudioSource>();

            Assert.AreEqual("Audio Source (1)", first.GetDisplayNameWithIndex());
            Assert.AreEqual("Audio Source (2)", second.GetDisplayNameWithIndex());
        }

        [Test]
        public void RemovingDuplicate_RemovesRemainingIndex()
        {
            var first = _gameObject.AddComponent<AudioSource>();
            var second = _gameObject.AddComponent<AudioSource>();
            Assert.AreEqual("Audio Source (2)", second.GetDisplayNameWithIndex());

            Object.DestroyImmediate(first);

            Assert.AreEqual("Audio Source", second.GetDisplayNameWithIndex());
        }

        [Test]
        public void DerivedComponents_DoNotCountAsExactTypeDuplicates()
        {
            var first = _gameObject.AddComponent<DisplayNameTestComponent>();
            var derived = _gameObject.AddComponent<DerivedDisplayNameTestComponent>();
            Assert.AreEqual(first.GetDisplayName(), first.GetDisplayNameWithIndex());

            var second = _gameObject.AddComponent<DisplayNameTestComponent>();

            Assert.AreEqual($"{first.GetDisplayName()} (1)", first.GetDisplayNameWithIndex());
            Assert.AreEqual($"{second.GetDisplayName()} (2)", second.GetDisplayNameWithIndex());
            Assert.AreEqual(derived.GetDisplayName(), derived.GetDisplayNameWithIndex());
        }

        [Test]
        public void InheritedComponentMenu_UsesInspectorTitle()
        {
            var component = _gameObject.AddComponent<DerivedDisplayNameTestComponent>();

            Assert.AreEqual(ObjectNames.GetInspectorTitle(component), component.GetDisplayName());
        }

        [AddComponentMenu("FastTools Tests/Display Name")]
        private class DisplayNameTestComponent : MonoBehaviour { }

        private sealed class DerivedDisplayNameTestComponent : DisplayNameTestComponent { }
    }
}
