using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Types;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    [Serializable]
    internal class DrawnBaseEffect
    {
        public int power;
    }

    [Serializable]
    internal sealed class DrawnDerivedEffect : DrawnBaseEffect { }

    [Serializable]
    internal sealed class DrawnGenericEffect<T>
    {
        public T value;
    }

    internal interface IDrawnEffect { }

    internal interface IDrawnChildEffect : IDrawnEffect { }

    [Serializable]
    internal sealed class DrawnInterfaceEffect : IDrawnEffect
    {
        public int power;
    }

    [Serializable]
    internal sealed class UndrawnEffect
    {
        public int power;
    }

    [Serializable]
    internal sealed class PipelineDrawnEffect { }

    [Serializable]
    internal sealed class AnyPipelineDrawnEffect { }

    [Serializable]
    internal sealed class MixedPipelineDrawnEffect { }

    internal sealed class DrawnPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline() => null;
    }

    internal sealed class OtherPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline() => null;
    }

    internal class DrawnBaseInspectorAttribute : UnityEngine.PropertyAttribute { }

    internal sealed class DrawnDerivedInspectorAttribute : DrawnBaseInspectorAttribute { }

    internal sealed class DrawnCollectionInspectorAttribute : UnityEngine.PropertyAttribute
    {
        public DrawnCollectionInspectorAttribute() : base(applyToCollection: true) { }
    }

    internal sealed class DrawnPipelineInspectorAttribute : UnityEngine.PropertyAttribute { }

    // None of these sets useForChildren: Unity still applies them to managed references of derived types.
    [CustomPropertyDrawer(typeof(DrawnBaseEffect))]
    internal sealed class DrawnBaseEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(DrawnGenericEffect<>))]
    internal sealed class DrawnGenericEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(IDrawnEffect))]
    internal sealed class DrawnInterfaceEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(DrawnBaseInspectorAttribute))]
    internal sealed class DrawnBaseInspectorAttributeDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(DrawnCollectionInspectorAttribute))]
    internal sealed class DrawnCollectionInspectorAttributeDrawer : PropertyDrawer { }

    // Unity uses a drawer marked with [SupportedOnRenderPipeline] only under that pipeline, never in the Built-in one.
    [SupportedOnRenderPipeline(typeof(DrawnPipelineAsset))]
    [CustomPropertyDrawer(typeof(PipelineDrawnEffect))]
    internal sealed class PipelineDrawnEffectDrawer : PropertyDrawer { }

    [SupportedOnRenderPipeline(typeof(RenderPipelineAsset))]
    [CustomPropertyDrawer(typeof(AnyPipelineDrawnEffect))]
    internal sealed class AnyPipelineDrawnEffectDrawer : PropertyDrawer { }

    [SupportedOnRenderPipeline(typeof(DrawnPipelineAsset))]
    [CustomPropertyDrawer(typeof(MixedPipelineDrawnEffect))]
    internal sealed class MixedPipelineDrawnEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(MixedPipelineDrawnEffect))]
    internal sealed class MixedPipelineDrawnEffectFallbackDrawer : PropertyDrawer { }

    [SupportedOnRenderPipeline(typeof(DrawnPipelineAsset))]
    [CustomPropertyDrawer(typeof(DrawnPipelineInspectorAttribute))]
    internal sealed class DrawnPipelineInspectorAttributeDrawer : PropertyDrawer { }

    internal sealed class DrawnNestingHost : ScriptableObject
    {
        [SerializeReference] public object single;
        [SerializeReference] public DrawnDerivedEffect baseDrawn;
        [SerializeReference] public DrawnGenericEffect<int> genericDrawn;
        [SerializeReference] public IDrawnChildEffect interfaceDrawn;
        [SerializeReference] public UndrawnEffect undrawn;
        [SerializeReference, DrawnDerivedInspector] public object attributed;
        [SerializeReference, DrawnPipelineInspector] public object pipelineAttributed;
        [SerializeReference] public PipelineDrawnEffect pipelineDrawn;
        [SerializeReference, DrawnDerivedInspector] public List<UndrawnEffect> attributedList = new();
        [SerializeReference, DrawnCollectionInspector] public List<UndrawnEffect> collectionAttributedList = new();
        [SerializeReference] public List<DrawnBaseEffect> drawnList = new();
        [SerializeReference] public List<UndrawnEffect> undrawnList = new();
        [TypeSelector] [SerializeReference] public List<DrawnBaseEffect> selectorList = new();
    }

    // A nested managed reference goes back to Unity whenever Unity would pick a custom drawer for its declared type:
    // by generic definition, by a base class or an interface without useForChildren, and per element of a list.
    [TestFixture]
    internal sealed class SerializeReferenceNestingTests
    {
        private DrawnNestingHost _host;

        [SetUp]
        public void SetUp() => _host = ScriptableObject.CreateInstance<DrawnNestingHost>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_host);

        [Test]
        public void UnityDrawerInternals_StillExist()
        {
            // A miss is not an error at runtime: every custom drawer under [SerializeReference] would silently lose
            // to the package header, so the rename has to fail here instead.
            Assert.IsNotNull(CustomDrawerRegistry.TargetField,
                "Unity renamed CustomPropertyDrawer.m_Type; update CustomDrawerRegistry.");
            Assert.IsNotNull(CustomDrawerRegistry.UseForChildrenField,
                "Unity renamed CustomPropertyDrawer.m_UseForChildren; update CustomDrawerRegistry.");
            Assert.AreEqual(typeof(Type), CustomDrawerRegistry.TargetField.FieldType);
            Assert.AreEqual(typeof(bool), CustomDrawerRegistry.UseForChildrenField.FieldType);
        }

        [Test]
        public void HasDrawerFor_ClosedGeneric_MatchesOpenGenericDrawer() =>
            Assert.IsTrue(CustomDrawerRegistry.HasDrawerFor(typeof(DrawnGenericEffect<int>)));

        [Test]
        public void HasDrawerFor_BaseClassDrawer_AppliesOnlyToManagedReferences()
        {
            Assert.IsTrue(CustomDrawerRegistry.HasDrawerFor(typeof(DrawnDerivedEffect), isManagedReference: true));
            Assert.IsFalse(CustomDrawerRegistry.HasDrawerFor(typeof(DrawnDerivedEffect)),
                "Without useForChildren a base-class drawer does not apply to a plain derived field.");
        }

        [Test]
        public void HasDrawerFor_InterfaceDrawer_AppliesToManagedReferences() =>
            Assert.IsTrue(CustomDrawerRegistry.HasDrawerFor(typeof(DrawnInterfaceEffect), isManagedReference: true));

        [Test]
        public void HasDrawerFor_ParentInterfaceDrawer_AppliesOnlyToManagedReferences()
        {
            Assert.IsTrue(CustomDrawerRegistry.HasDrawerFor(typeof(IDrawnChildEffect), isManagedReference: true));
            Assert.IsFalse(CustomDrawerRegistry.HasDrawerFor(typeof(IDrawnChildEffect)),
                "Without useForChildren a parent-interface drawer does not apply to a plain child-interface field.");
        }

        [Test]
        public void HasDrawerFor_TypeWithoutDrawer_ReturnsFalse() =>
            Assert.IsFalse(CustomDrawerRegistry.HasDrawerFor(typeof(UndrawnEffect), isManagedReference: true));

        [Test]
        public void DeclaresDrawnAttribute_BaseAttributeDrawer_AppliesOnlyToManagedReferences()
        {
            var field = typeof(DrawnNestingHost).GetField(nameof(DrawnNestingHost.attributed));

            Assert.IsTrue(CustomDrawerRegistry.DeclaresDrawnAttribute(field, isManagedReference: true));
            Assert.IsFalse(CustomDrawerRegistry.DeclaresDrawnAttribute(field),
                "Without useForChildren a base-attribute drawer does not apply to a plain field.");
        }

        [TestCase(typeof(PipelineDrawnEffect), null, false)]
        [TestCase(typeof(PipelineDrawnEffect), typeof(OtherPipelineAsset), false)]
        [TestCase(typeof(PipelineDrawnEffect), typeof(DrawnPipelineAsset), true)]
        [TestCase(typeof(AnyPipelineDrawnEffect), null, false)]
        [TestCase(typeof(AnyPipelineDrawnEffect), typeof(OtherPipelineAsset), true)]
        [TestCase(typeof(MixedPipelineDrawnEffect), null, true)]
        [TestCase(typeof(MixedPipelineDrawnEffect), typeof(OtherPipelineAsset), true)]
        public void HasDrawerFor_FollowsTheRenderPipelineOfTheDrawer(Type type, Type pipelineType, bool expected) =>
            Assert.AreEqual(expected, CustomDrawerRegistry.HasDrawerFor(type, isManagedReference: true, pipelineType));

        [Test]
        public void DeclaresDrawnAttribute_DrawerMarkedForAnotherPipeline_IsIgnored()
        {
            var field = typeof(DrawnNestingHost).GetField(nameof(DrawnNestingHost.pipelineAttributed));

            Assert.IsFalse(CustomDrawerRegistry.DeclaresDrawnAttribute(field, isManagedReference: true));
        }

        [Test]
        public void DeclaresDrawnAttribute_ListAttribute_BelongsToTheListOrToItsElements()
        {
            var perElement = typeof(DrawnNestingHost).GetField(nameof(DrawnNestingHost.attributedList));
            var perList = typeof(DrawnNestingHost).GetField(nameof(DrawnNestingHost.collectionAttributedList));

            Assert.IsFalse(CustomDrawerRegistry.DeclaresDrawnAttribute(perElement, isManagedReference: true, isCollection: true));
            Assert.IsTrue(CustomDrawerRegistry.DeclaresDrawnAttribute(perElement, isManagedReference: true, isArrayElement: true));
            Assert.IsTrue(CustomDrawerRegistry.DeclaresDrawnAttribute(perList, isManagedReference: true, isCollection: true));
            Assert.IsFalse(CustomDrawerRegistry.DeclaresDrawnAttribute(perList, isManagedReference: true, isArrayElement: true));
        }

        [TestCase(nameof(DrawnNestingHost.baseDrawn), false)]
        [TestCase(nameof(DrawnNestingHost.genericDrawn), false)]
        [TestCase(nameof(DrawnNestingHost.interfaceDrawn), false)]
        [TestCase(nameof(DrawnNestingHost.attributed), false)]
        [TestCase(nameof(DrawnNestingHost.undrawn), true)]
        [TestCase(nameof(DrawnNestingHost.pipelineDrawn), true)]
        [TestCase(nameof(DrawnNestingHost.pipelineAttributed), true)]
        public void DrawsOwnHeader_FollowsTheDrawerOfTheDeclaredType(string fieldName, bool expected)
        {
            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(fieldName);

            Assert.AreEqual(expected, SerializeReferenceNesting.DrawsOwnHeader(property, depth: 0));
        }

        // Unity would draw these with the stored type's drawer, which has no type picker to change or clear it.
        [TestCase(typeof(DrawnDerivedEffect))]
        [TestCase(typeof(DrawnGenericEffect<int>))]
        [TestCase(typeof(DrawnInterfaceEffect))]
        public void DrawsOwnHeader_KeepsThePickerWhenOnlyTheStoredTypeHasADrawer(Type instanceType)
        {
            _host.single = Activator.CreateInstance(instanceType);

            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(DrawnNestingHost.single));

            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(property, depth: 0));
        }

        [Test]
        public void DrawsOwnHeader_ListKeepsItsHeaderAndHandsOverElementsOfADrawnType()
        {
            _host.drawnList.Add(new DrawnDerivedEffect());
            _host.undrawnList.Add(new UndrawnEffect());

            using var serializedObject = new SerializedObject(_host);
            var drawnList = serializedObject.FindProperty(nameof(DrawnNestingHost.drawnList));
            var undrawnList = serializedObject.FindProperty(nameof(DrawnNestingHost.undrawnList));

            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(drawnList, depth: 0),
                "Unity never applies a type drawer to the list itself, so the picker-backed add stays.");
            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(drawnList.GetArrayElementAtIndex(0), depth: 0));
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(undrawnList, depth: 0));
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(undrawnList.GetArrayElementAtIndex(0), depth: 0));
        }

        [Test]
        public void DrawsOwnHeader_ListWithAnElementAttribute_KeepsThePickerAndHandsOverElements()
        {
            _host.attributedList.Add(new UndrawnEffect());

            using var serializedObject = new SerializedObject(_host);
            var list = serializedObject.FindProperty(nameof(DrawnNestingHost.attributedList));

            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(list, depth: 0),
                "The attribute applies to each element, so Unity draws the list itself and the picker-backed add stays.");
            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(list.GetArrayElementAtIndex(0), depth: 0));
        }

        [Test]
        public void DrawsOwnHeader_ListWithACollectionAttribute_GoesToItsDrawerAndDrawsItsElements()
        {
            _host.collectionAttributedList.Add(new UndrawnEffect());

            using var serializedObject = new SerializedObject(_host);
            var list = serializedObject.FindProperty(nameof(DrawnNestingHost.collectionAttributedList));

            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(list, depth: 0),
                "The attribute applies to the collection, so Unity hands the list to its drawer.");
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(list.GetArrayElementAtIndex(0), depth: 0));
        }

        [Test]
        public void DrawsOwnHeader_SelectorListGoesToItsDrawerAndDrawsItsElements()
        {
            _host.selectorList.Add(new DrawnDerivedEffect());

            using var serializedObject = new SerializedObject(_host);
            var selectorList = serializedObject.FindProperty(nameof(DrawnNestingHost.selectorList));

            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(selectorList, depth: 0),
                "[TypeSelector] applies to the collection, so Unity hands the list to the attribute's drawer.");
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(selectorList.GetArrayElementAtIndex(0), depth: 0),
                "Unity no longer hands [TypeSelector] to an element, so the list draws it with the picker, ahead of the " +
                "element type's drawer.");
        }

        [TestCase(nameof(DrawnNestingHost.drawnList), typeof(PropertyField))]
        [TestCase(nameof(DrawnNestingHost.undrawnList), typeof(SerializeReferenceField))]
        [TestCase(nameof(DrawnNestingHost.selectorList), typeof(SerializeReferenceField))]
        public void ListField_BindItem_HandsOverTheSameElementsAsTheIMGUIList(string listName, Type expectedField)
        {
            _host.drawnList.Add(new DrawnDerivedEffect());
            _host.undrawnList.Add(new UndrawnEffect());
            _host.selectorList.Add(new DrawnDerivedEffect());

            using var serializedObject = new SerializedObject(_host);
            var list = serializedObject.FindProperty(listName);
            var listField = new SerializeReferenceListField(list.displayName, list,
                SerializeReferenceHelpers.GetArrayElementType(list), depth: 1);

            var item = new VisualElement();
            listField.Q<ListView>().bindItem(item, 0);

            Assert.AreEqual(1, item.childCount);
            Assert.IsInstanceOf(expectedField, item[0]);
        }
    }
}
