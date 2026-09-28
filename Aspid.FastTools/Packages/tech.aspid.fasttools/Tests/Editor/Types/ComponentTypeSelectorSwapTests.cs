using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using Aspid.FastTools.Types.Tests;
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
        private GameObject _gameObject;

        [SetUp]
        public void SetUp() => _gameObject = new GameObject(nameof(ComponentTypeSelectorSwapTests));

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
            Undo.ClearAll();
        }

        [Test]
        public void SwapScript_AddsTheComponentsTheNewTypeRequires()
        {
            var component = _gameObject.AddComponent<ComponentSwapPlain>();

            Swap(component, typeof(ComponentSwapNeedsRequirement));

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

            Swap(component, typeof(ComponentSwapNeedsSpriteRenderer));

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

        private static void Swap(Component component, Type newType)
        {
            using var serializedObject = new SerializedObject(component);
            ComponentTypeSelectorPropertyDrawer.SwapScript(serializedObject, newType.FindMonoScript(), newType);
        }

        private static bool Replace(Component component, Type newType)
        {
            using var serializedObject = new SerializedObject(component);
            return ComponentTypeSelectorPropertyDrawer.ReplaceComponentScript(
                serializedObject.FindProperty("m_Script"), component.GetType(), newType);
        }
    }
}
