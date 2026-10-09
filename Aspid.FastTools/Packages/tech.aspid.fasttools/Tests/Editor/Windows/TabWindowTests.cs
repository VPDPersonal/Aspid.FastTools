using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// Coverage for the FastTools window's state: the selected tab survives serialization (a domain reload keeps only
    /// serialized fields), and Asset References keeps its asset when the new selection is not one it can map.
    /// </summary>
    [TestFixture]
    internal sealed class TabWindowTests
    {
        private const string PrefabPath = "Assets/__AspidTabWindowProbe__.prefab";

        private TabWindow _window;
        private GameObject _prefab;
        private GameObject _sceneObject;
        private GameObject _prefabInstance;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<TabWindow>();

            var root = new GameObject("TabWindowProbe");
            try
            {
                _prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            _sceneObject = new GameObject("TabWindowSceneObject");
            _prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_window);
            Object.DestroyImmediate(_sceneObject);
            Object.DestroyImmediate(_prefabInstance);
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        [Test]
        public void CurrentTabType_DefaultsToWelcome() =>
            Assert.AreEqual(TabType.Welcome, _window.CurrentTabType);

        [TestCase(TabType.Welcome)]
        [TestCase(TabType.AssetReference)]
        [TestCase(TabType.ProjectReferences)]
        [TestCase(TabType.Settings)]
        public void SwitchMode_BeforeCreateGui_RemembersTab(TabType tab)
        {
            _window.SwitchMode(tab);

            Assert.AreEqual(tab, _window.CurrentTabType);
        }

        [TestCase(TabType.AssetReference)]
        [TestCase(TabType.ProjectReferences)]
        [TestCase(TabType.Settings)]
        public void CurrentTabType_SurvivesSerialization(TabType tab)
        {
            _window.SwitchMode(tab);

            // A domain reload rebuilds the window from its serialized fields only.
            var json = EditorJsonUtility.ToJson(_window);
            var reloaded = ScriptableObject.CreateInstance<TabWindow>();
            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, reloaded);

                Assert.AreEqual(tab, reloaded.CurrentTabType, "A reloaded window must reopen on the tab it was on.");
            }
            finally
            {
                Object.DestroyImmediate(reloaded);
            }
        }

        [Test]
        public void CanInspect_SavedAssetAndPrefabInstance_AreAccepted()
        {
            Assert.IsTrue(TabWindow.CanInspect(_prefab), "A saved asset can be mapped.");
            Assert.IsTrue(TabWindow.CanInspect(_prefabInstance), "A prefab instance maps through its source prefab.");
        }

        [Test]
        public void CanInspect_NullAndPlainSceneObject_AreRejected()
        {
            Assert.IsFalse(TabWindow.CanInspect(null), "An empty selection is not an asset.");
            Assert.IsFalse(TabWindow.CanInspect(_sceneObject), "A plain scene object has no saved asset.");
        }

        [Test]
        public void ResolvePendingTarget_ValidRequest_ReplacesCurrent() =>
            Assert.AreSame(_prefab, TabWindow.ResolvePendingTarget(current: _sceneObject, requested: _prefab));

        [Test]
        public void ResolvePendingTarget_EmptySelection_KeepsCurrent() =>
            Assert.AreSame(_prefab, TabWindow.ResolvePendingTarget(current: _prefab, requested: null));

        [Test]
        public void ResolvePendingTarget_PlainSceneObject_KeepsCurrent() =>
            Assert.AreSame(_prefab, TabWindow.ResolvePendingTarget(current: _prefab, requested: _sceneObject));

        [Test]
        public void ResolvePendingTarget_NothingToKeep_StaysEmpty() =>
            Assert.IsNull(TabWindow.ResolvePendingTarget(current: null, requested: null));
    }
}
