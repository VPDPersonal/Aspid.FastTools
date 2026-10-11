using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // CreateField must offer the types the same field offers through PropertyField: its [TypeSelector] constraints
    // join the caller's base types, as they already do for CreateList.
    [TestFixture]
    internal sealed class SerializeReferenceCreateFieldTests
    {
        private LinkFilterTestObject _target;
        private SerializedObject _serialized;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<LinkFilterTestObject>();
            _serialized = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serialized.Dispose();
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void CreateField_TypeSelectorOnTheField_NarrowsThePicker()
        {
            var field = SerializeReferenceEditorGUI.CreateField(_serialized.FindProperty(nameof(LinkFilterTestObject.meleeBackup)));

            CollectionAssert.AreEquivalent(new[] { typeof(ILinkMelee) }, GetBaseTypes(field));
        }

        [Test]
        public void CreateField_MemberReferencedTypeSelector_NarrowsThePicker()
        {
            var field = SerializeReferenceEditorGUI.CreateField(_serialized.FindProperty(nameof(LinkFilterTestObject.memberBackup)));

            CollectionAssert.AreEquivalent(new[] { typeof(ILinkMelee) }, GetBaseTypes(field));
        }

        [Test]
        public void CreateField_ExplicitBaseTypes_StayNextToTheAttribute()
        {
            var field = SerializeReferenceEditorGUI.CreateField(
                _serialized.FindProperty(nameof(LinkFilterTestObject.meleeBackup)), label: null, typeof(ILinkWeapon));

            CollectionAssert.AreEquivalent(new[] { typeof(ILinkWeapon), typeof(ILinkMelee) }, GetBaseTypes(field));
        }

        [Test]
        public void CreateField_NoAttribute_AddsNothingToTheCallersBaseTypes()
        {
            var property = _serialized.FindProperty(nameof(LinkFilterTestObject.spare));

            CollectionAssert.IsEmpty(GetBaseTypes(SerializeReferenceEditorGUI.CreateField(property)) ?? Array.Empty<Type>());
            CollectionAssert.AreEqual(new[] { typeof(ILinkMelee) },
                GetBaseTypes(SerializeReferenceEditorGUI.CreateField(property, label: null, typeof(ILinkMelee))));
        }

        private static Type[] GetBaseTypes(VisualElement field) =>
            (Type[])typeof(SerializeReferenceField)
                .GetField("_baseTypes", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(field);
    }
}
