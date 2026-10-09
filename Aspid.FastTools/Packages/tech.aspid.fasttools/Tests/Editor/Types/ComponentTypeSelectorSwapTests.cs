using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Aspid.FastTools.Types.Tests;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for the script swap behind <see cref="ComponentTypeSelector"/>. It writes <c>m_Script</c> instead of
    /// calling <c>AddComponent</c>, so without these checks Unity applies neither <see cref="RequireComponent"/> nor
    /// <see cref="DisallowMultipleComponent"/> of the new class, and a component other components depend on can vanish.
    /// </summary>
    [TestFixture]
    internal sealed class ComponentTypeSelectorSwapTests
    {
        private const string BasePrefabPath = "Assets/__AspidComponentSwapBase__.prefab";
        private const string VariantPrefabPath = "Assets/__AspidComponentSwapVariant__.prefab";

        private int _undoGroup;
        private GameObject _gameObject;
        private GameObject _prefabInstance;

        // Lives in an Editor assembly, so it stands for a class a player build does not contain.
        private sealed class EditorOnlySwapType : ComponentSwapBase { }

        private sealed class EditorOnlyOwner : ScriptableObject { }

        [SetUp]
        public void SetUp()
        {
            DeletePrefabs();

            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();
            _gameObject = new GameObject(nameof(ComponentTypeSelectorSwapTests));
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            Object.DestroyImmediate(_gameObject);
            if (_prefabInstance) Object.DestroyImmediate(_prefabInstance);
            Undo.IncrementCurrentGroup();

            DeletePrefabs();
        }

        [Test]
        public void SwapScript_AddsTheComponentsTheNewTypeRequires()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            Assert.IsTrue(Swap(component, typeof(ComponentSwapNeedsRequirement)));

            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapNeedsRequirement>(), "The swap must change the component's class.");
            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapRequirement>(), "The [RequireComponent] of the new class must be added.");
        }

        [Test]
        public void SwapScript_AddsTheRequiredComponentsBeforeTheNewTypeValidates()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            Swap(component, typeof(ComponentSwapNeedsRequirement));

            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapNeedsRequirement>().Requirement,
                "OnValidate of the new class must already find its [RequireComponent].");
        }

        [Test]
        public void SwapScript_RequirementThatCannotBeAdded_RevertsTheSwap()
        {
            _gameObject.AddComponent<MeshRenderer>();
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            LogAssert.Expect(LogType.Log, new Regex("conflicts with the existing 'MeshRenderer'"));
            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(SpriteRenderer)}, which cannot be added"));

            Assert.IsFalse(Swap(component, typeof(ComponentSwapNeedsSpriteRenderer)));

            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapPlain>(), "The swap must be reverted.");
            Assert.IsNull(_gameObject.GetComponent<ComponentSwapNeedsSpriteRenderer>());
            Assert.IsNull(_gameObject.GetComponent<SpriteRenderer>());
        }

        [Test]
        public void SwapScript_OneUndoRevertsTheSwapAndTheAddedComponents()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            Undo.IncrementCurrentGroup();

            Swap(component, typeof(ComponentSwapNeedsRequirement));
            Assert.AreEqual($"Change Type to {nameof(ComponentSwapNeedsRequirement)}", Undo.GetCurrentGroupName());
            Undo.PerformUndo();

            Assert.IsNull(_gameObject.GetComponent<ComponentSwapRequirement>());
            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapPlain>());
        }

        [Test]
        public void ReplaceComponentScript_SecondDisallowMultipleComponent_IsRefused()
        {
            _gameObject.AddComponent<ComponentSwapSingle>();
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            LogAssert.Expect(LogType.Warning, new Regex(nameof(DisallowMultipleComponent)));

            Assert.IsFalse(Replace(component, typeof(ComponentSwapSingle)));
        }

        [Test]
        public void ReplaceComponentScript_ComponentAnotherOneRequires_IsRefused()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            _gameObject.AddComponent<ComponentSwapPlainDependent>();

            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(ComponentSwapPlainDependent)} requires {nameof(ComponentSwapPlain)}"));

            Assert.IsFalse(Replace(component, typeof(ComponentSwapNeedsRequirement)));
        }

        [TestCase(typeof(ComponentSwapNeedsAbstractRequirement), nameof(ComponentSwapAbstractRequirement))]
        [TestCase(typeof(ComponentSwapNeedsRenderer), nameof(Renderer))]
        public void ReplaceComponentScript_AbstractRequirement_IsRefused(Type newType, string requiredName)
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            LogAssert.Expect(LogType.Warning, new Regex($"{newType.Name} requires {requiredName}, which is abstract"));

            Assert.IsFalse(Replace(component, newType));
        }

        [Test]
        public void FindSwapConflict_RequirementTheNewTypeStillMeets_IsNoConflict()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            _gameObject.AddComponent<ComponentSwapBaseDependent>();

            Assert.IsNull(ComponentTypeSelectorPropertyDrawer.FindSwapConflict(component, typeof(ComponentSwapNeedsRequirement)));
        }

        [Test]
        public void FindSwapConflict_RequirementAnotherComponentMeets_IsNoConflict()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            _gameObject.AddComponent<ComponentSwapPlain>();
            _gameObject.AddComponent<ComponentSwapPlainDependent>();

            Assert.IsNull(ComponentTypeSelectorPropertyDrawer.FindSwapConflict(component, typeof(ComponentSwapNeedsRequirement)));
        }

        [UnityTest]
        public IEnumerator ReplaceComponentScript_SwapRefusedOnTheNextTick_ReportsItToTheCaller()
        {
            _gameObject.AddComponent<MeshRenderer>();
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            var rejected = false;

            LogAssert.Expect(LogType.Log, new Regex("conflicts with the existing 'MeshRenderer'"));
            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(SpriteRenderer)}, which cannot be added"));

            using var serializedObject = new SerializedObject(component);
            var scheduled = ComponentTypeSelectorPropertyDrawer.ReplaceComponentScript(
                serializedObject.FindProperty("m_Script"), component.GetType(), typeof(ComponentSwapNeedsSpriteRenderer),
                onRejected: () => rejected = true);

            Assert.IsTrue(scheduled, "The pre-checks do not rule this swap out; it fails only when the component is added.");
            Assert.IsFalse(rejected, "The swap runs on the next editor tick, not inside the call.");

            yield return WaitUntil(() => rejected);

            Assert.IsTrue(rejected, "The caller must learn that the scheduled swap was refused.");
            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapPlain>());
        }

        [UnityTest]
        public IEnumerator ReplaceComponentScript_SwapApplied_DoesNotReportARejection()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            var rejected = false;

            using var serializedObject = new SerializedObject(component);
            Assert.IsTrue(ComponentTypeSelectorPropertyDrawer.ReplaceComponentScript(
                serializedObject.FindProperty("m_Script"), component.GetType(), typeof(ComponentSwapSingle),
                onRejected: () => rejected = true));

            yield return WaitUntil(() => _gameObject.GetComponent<ComponentSwapSingle>());

            Assert.IsNotNull(_gameObject.GetComponent<ComponentSwapSingle>(), "The scheduled swap must run.");
            Assert.IsFalse(rejected);
        }

        // Unity records no override for a changed m_Script, so on an inherited component the swap would look done
        // and be gone after the scene or the variant is saved and reloaded.
        [Test]
        public void ReplaceComponentScript_ComponentInheritedFromPrefab_IsRefused()
        {
            CreateBasePrefab(withComponent: true);
            _prefabInstance = InstantiateBasePrefab();

            LogAssert.Expect(LogType.Warning, new Regex("inherited from a prefab"));

            Assert.IsFalse(Replace(_prefabInstance.GetComponent<ComponentSwapPlain>(), typeof(ComponentSwapSingle)));
        }

        [Test]
        public void ReplaceComponentScript_ComponentInheritedByPrefabVariant_IsRefused()
        {
            CreateBasePrefab(withComponent: true);
            CreateVariantPrefab();
            var component = AssetDatabase.LoadAssetAtPath<GameObject>(VariantPrefabPath).GetComponent<ComponentSwapPlain>();

            LogAssert.Expect(LogType.Warning, new Regex("inherited from a prefab"));

            Assert.IsFalse(Replace(component, typeof(ComponentSwapSingle)));
        }

        [Test]
        public void FindSwapConflict_ComponentOfTheSourcePrefab_IsNoConflict()
        {
            CreateBasePrefab(withComponent: true);
            var component = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath).GetComponent<ComponentSwapPlain>();

            Assert.IsNull(ComponentTypeSelectorPropertyDrawer.FindSwapConflict(component, typeof(ComponentSwapSingle)),
                "The source prefab owns the component, so its class can change there.");
        }

        [Test]
        public void FindSwapConflict_ComponentAddedToPrefabInstance_IsNoConflict()
        {
            CreateBasePrefab(withComponent: false);
            _prefabInstance = InstantiateBasePrefab();
            var added = _prefabInstance.AddComponent<ComponentSwapPlain>();

            Assert.IsNull(ComponentTypeSelectorPropertyDrawer.FindSwapConflict(added, typeof(ComponentSwapSingle)),
                "A component the instance added has no source, and its script is saved with the scene.");
        }

        [Test]
        public void CreateFilter_ComponentOfARuntimeAssembly_LeavesOutEditorOnlyTypes()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();
            using var serializedObject = new SerializedObject(component);

            var filter = ComponentTypeSelectorPropertyDrawer.CreateFilter(
                declaringType: typeof(ComponentSwapBase), property: serializedObject.FindProperty("m_Script"));
            var offered = Scan(filter);

            Assert.IsTrue(filter.ExcludeEditorOnly);
            Assert.IsTrue(offered.Contains(typeof(ComponentSwapPlain)));
            Assert.IsFalse(offered.Contains(typeof(EditorOnlySwapType)),
                "A player build has no such class, so the swap would leave a missing script.");
        }

        [Test]
        public void CreateFilter_ObjectOfAnEditorAssembly_KeepsEditorOnlyTypes()
        {
            var owner = ScriptableObject.CreateInstance<EditorOnlyOwner>();

            try
            {
                using var serializedObject = new SerializedObject(owner);

                var filter = ComponentTypeSelectorPropertyDrawer.CreateFilter(
                    declaringType: typeof(ComponentSwapBase), property: serializedObject.FindProperty("m_Script"));

                Assert.IsFalse(filter.ExcludeEditorOnly);
                Assert.IsTrue(Scan(filter).Contains(typeof(EditorOnlySwapType)));
            }
            finally { Object.DestroyImmediate(owner); }
        }

        private static Type[] Scan(TypeSelectorFilter filter) =>
            TypeInfo.GetAllTypeInfos(filter.Types, filter.Allow, filter.Predicate, filter.AdditionalTypes,
                    filter.IncludeHidden, filter.ExcludeEditorOnly)
                .Select(info => TypeUtility.GetTypeOrNull(info.AssemblyQualifiedName))
                .ToArray();

        // A swap scheduled with delayCall runs in an editor update after the test method has returned.
        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            var deadline = EditorApplication.timeSinceStartup + 5d;
            while (!condition() && EditorApplication.timeSinceStartup < deadline)
                yield return null;
        }

        private static void CreateBasePrefab(bool withComponent)
        {
            var root = new GameObject(nameof(ComponentTypeSelectorSwapTests));

            try
            {
                if (withComponent) root.AddComponent<ComponentSwapPlain>();
                PrefabUtility.SaveAsPrefabAsset(root, BasePrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void CreateVariantPrefab()
        {
            var instance = InstantiateBasePrefab();

            try { PrefabUtility.SaveAsPrefabAsset(instance, VariantPrefabPath); }
            finally { Object.DestroyImmediate(instance); }
        }

        private static GameObject InstantiateBasePrefab() =>
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath));

        private static void DeletePrefabs()
        {
            AssetDatabase.DeleteAsset(VariantPrefabPath);
            AssetDatabase.DeleteAsset(BasePrefabPath);
        }

        private static bool Swap(Component component, Type newType)
        {
            using var serializedObject = new SerializedObject(component);
            return ComponentTypeSelectorPropertyDrawer.SwapScript(serializedObject, newType.FindMonoScript(), newType);
        }

        private static bool Replace(Component component, Type newType)
        {
            using var serializedObject = new SerializedObject(component);
            return ComponentTypeSelectorPropertyDrawer.ReplaceComponentScript(
                serializedObject.FindProperty("m_Script"), component.GetType(), newType);
        }
    }
}
