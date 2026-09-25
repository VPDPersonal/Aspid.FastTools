using System;
using NUnit.Framework;
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

        [TestCase(typeof(DisplayNameTestComponent), "Ability")]
        [TestCase(typeof(DerivedDisplayNameTestComponent), "Derived Display Name Test Component")]
        [TestCase(typeof(EmptyMenuDisplayNameTestComponent), "Empty Menu Display Name Test Component")]
        [TestCase(typeof(TrailingSlashDisplayNameTestComponent), "Trailing Slash Display Name Test Component")]
#pragma warning disable CS0618
        [TestCase(typeof(ObsoleteDisplayNameTestComponent), "Obsolete Ability")]
#pragma warning restore CS0618
        public void ComponentMenu_UsesOwnMenuTitleWithoutInspectorSuffixes(Type componentType, string expected)
        {
            var component = _gameObject.AddComponent(componentType);

            Assert.AreEqual(expected, component.GetDisplayName());
        }

        [AddComponentMenu("Gameplay/Ability")]
        private class DisplayNameTestComponent : MonoBehaviour { }

        private sealed class DerivedDisplayNameTestComponent : DisplayNameTestComponent { }

        [AddComponentMenu("")]
        private sealed class EmptyMenuDisplayNameTestComponent : MonoBehaviour { }

        [AddComponentMenu("Gameplay/")]
        private sealed class TrailingSlashDisplayNameTestComponent : MonoBehaviour { }

        [Obsolete("Display name test.")]
        [AddComponentMenu("Gameplay/Obsolete Ability")]
        private sealed class ObsoleteDisplayNameTestComponent : MonoBehaviour { }
    }
}
