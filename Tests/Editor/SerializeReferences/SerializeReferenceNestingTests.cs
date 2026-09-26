using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
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

    // None of these sets useForChildren: Unity still applies them to managed references of derived types.
    [CustomPropertyDrawer(typeof(DrawnBaseEffect))]
    internal sealed class DrawnBaseEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(DrawnGenericEffect<>))]
    internal sealed class DrawnGenericEffectDrawer : PropertyDrawer { }

    [CustomPropertyDrawer(typeof(IDrawnEffect))]
    internal sealed class DrawnInterfaceEffectDrawer : PropertyDrawer { }

    internal sealed class DrawnNestingHost : ScriptableObject
    {
        [SerializeReference] public object single;
        [SerializeReference] public List<DrawnBaseEffect> drawnList = new();
        [SerializeReference] public List<UndrawnEffect> undrawnList = new();
    }

    // A nested managed reference goes back to Unity whenever Unity would pick a custom drawer for it: by generic
    // definition, by a base class or an interface without useForChildren, and by the instance or list element type.
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
        public void HasDrawerFor_TypeWithoutDrawer_ReturnsFalse() =>
            Assert.IsFalse(CustomDrawerRegistry.HasDrawerFor(typeof(UndrawnEffect), isManagedReference: true));

        [TestCase(typeof(DrawnDerivedEffect), false)]
        [TestCase(typeof(DrawnGenericEffect<int>), false)]
        [TestCase(typeof(DrawnInterfaceEffect), false)]
        [TestCase(typeof(UndrawnEffect), true)]
        public void DrawsOwnHeader_FollowsTheDrawerOfTheInstanceType(Type instanceType, bool expected)
        {
            _host.single = Activator.CreateInstance(instanceType);

            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(DrawnNestingHost.single));

            Assert.AreEqual(expected, SerializeReferenceNesting.DrawsOwnHeader(property, depth: 0));
        }

        [Test]
        public void DrawsOwnHeader_ListFollowsTheDrawerOfItsElementType()
        {
            _host.drawnList.Add(new DrawnDerivedEffect());
            _host.undrawnList.Add(new UndrawnEffect());

            using var serializedObject = new SerializedObject(_host);
            var drawnList = serializedObject.FindProperty(nameof(DrawnNestingHost.drawnList));
            var undrawnList = serializedObject.FindProperty(nameof(DrawnNestingHost.undrawnList));

            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(drawnList, depth: 0));
            Assert.IsFalse(SerializeReferenceNesting.DrawsOwnHeader(drawnList.GetArrayElementAtIndex(0), depth: 0));
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(undrawnList, depth: 0));
            Assert.IsTrue(SerializeReferenceNesting.DrawsOwnHeader(undrawnList.GetArrayElementAtIndex(0), depth: 0));
        }
    }
}
