using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Argument checks of the public helpers for custom inspectors.
    // They throw before a control is drawn, so a wrong property fails at once, in UI Toolkit and in IMGUI.
    [TestFixture]
    internal sealed class SerializeReferenceEditorGUITests
    {
        [Test]
        public void CreateField_ManagedReference_ReturnsTheField()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var property = new SerializedObject(obj).FindProperty("a");

                Assert.IsInstanceOf<SerializeReferenceField>(SerializeReferenceEditorGUI.CreateField(property, label: "Weapon"));
                Assert.IsInstanceOf<SerializeReferenceField>(SerializeReferenceEditorGUI.CreateField(property),
                    "A null label falls back to the display name.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void CreateField_NullProperty_Throws() =>
            Assert.Throws<ArgumentNullException>(() => SerializeReferenceEditorGUI.CreateField(null));

        [Test]
        public void CreateField_NotAManagedReference_Throws()
        {
            var obj = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                Assert.Throws<ArgumentException>(() =>
                        SerializeReferenceEditorGUI.CreateField(serialized.FindProperty("requiredString")),
                    "A plain field has no managed reference to pick a type for.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void CreateList_ManagedReferenceList_ReturnsAList()
        {
            var obj = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            try
            {
                var property = new SerializedObject(obj).FindProperty("weapons");

                Assert.IsNotNull(SerializeReferenceEditorGUI.CreateList(property, label: "Weapons").Q<ListView>());
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void CreateList_NullProperty_Throws() =>
            Assert.Throws<ArgumentNullException>(() => SerializeReferenceEditorGUI.CreateList(null));

        [Test]
        public void CreateList_NotAManagedReferenceList_Throws()
        {
            var obj = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                Assert.Throws<ArgumentException>(() =>
                        SerializeReferenceEditorGUI.CreateList(serialized.FindProperty(nameof(ListAddTestObject.counts))),
                    "A list of plain values has no managed references.");
                Assert.Throws<ArgumentException>(() =>
                        SerializeReferenceEditorGUI.CreateList(serialized.FindProperty(nameof(ListAddTestObject.loadouts))),
                    "A list of by-value structs has no managed references.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void DrawFieldLayout_NullProperty_Throws() =>
            Assert.Throws<ArgumentNullException>(() => SerializeReferenceEditorGUI.DrawFieldLayout(null));

        [Test]
        public void DrawFieldLayout_NotAManagedReference_Throws()
        {
            var obj = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                Assert.Throws<ArgumentException>(() =>
                        SerializeReferenceEditorGUI.DrawFieldLayout(serialized.FindProperty("weapons")),
                    "A list belongs to SerializeReferenceIMGUIList.Draw, not to the single-field helper.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
