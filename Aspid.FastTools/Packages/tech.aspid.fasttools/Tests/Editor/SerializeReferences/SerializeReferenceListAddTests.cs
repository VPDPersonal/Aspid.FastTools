using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A managed-reference list nested in a by-value array element, next to a top-level one.
    [Serializable]
    internal sealed class ListAddTestLoadout
    {
        [SerializeReference] public List<ITestWeapon> weapons = new();
    }

    internal sealed class ListAddTestObject : ScriptableObject
    {
        [SerializeReference] public List<ITestWeapon> sidearms = new();
        public List<ListAddTestLoadout> loadouts = new();
        public List<int> counts = new();
    }

    // Covers the picker-backed "+" of managed-reference lists: which array and objects an element's "+" appends to,
    // one independent instance per selected object, and the argument checks of the public IMGUI list.
    [TestFixture]
    internal sealed class SerializeReferenceListAddTests
    {
        private static readonly string SwordName = typeof(TestSword).AssemblyQualifiedName;

        [Test]
        public void TryGetArrayPath_ResolvesTheInnermostArray()
        {
            Assert.IsTrue(SerializeReferenceHelpers.TryGetArrayPath("loadouts.Array.data[0].weapons.Array.data[1]", out var nested));
            Assert.AreEqual("loadouts.Array.data[0].weapons", nested,
                "A list nested in another array's element must resolve to the inner list, not the outer array.");

            Assert.IsTrue(SerializeReferenceHelpers.TryGetArrayPath("sidearms.Array.data[2]", out var top));
            Assert.AreEqual("sidearms", top);
        }

        [Test]
        public void TryGetArrayPath_RejectsNonElementPaths()
        {
            Assert.IsFalse(SerializeReferenceHelpers.TryGetArrayPath("loadouts.Array.data[0].weapon", out _),
                "A sub-field of an array element is not itself an element.");
            Assert.IsFalse(SerializeReferenceHelpers.TryGetArrayPath("primary", out _));
            Assert.IsFalse(SerializeReferenceHelpers.TryGetArrayPath(null, out _));
        }

        [Test]
        public void TryResolveAppendTarget_NestedListElement_ResolvesTheInnerList()
        {
            var obj = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                obj.loadouts.Add(new ListAddTestLoadout { weapons = { new TestSword() } });
                var serialized = new SerializedObject(obj);
                var element = serialized.FindProperty("loadouts.Array.data[0].weapons.Array.data[0]");

                Assert.IsTrue(SerializeReferenceListAddBehavior.TryResolveAppendTarget(element, out var targets, out var arrayPath));
                Assert.AreEqual("loadouts.Array.data[0].weapons", arrayPath,
                    "The \"+\" of a list nested in another array's element must append to the inner list, not the outer array.");
                CollectionAssert.AreEqual(new Object[] { obj }, targets);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void TryResolveAppendTarget_MultipleObjects_ResolvesEveryTarget()
        {
            var first = ScriptableObject.CreateInstance<ListAddTestObject>();
            var second = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                first.sidearms.Add(new TestSword());
                second.sidearms.Add(new TestSword());
                var serialized = new SerializedObject(new Object[] { first, second });
                var element = serialized.FindProperty("sidearms.Array.data[0]");

                Assert.IsTrue(SerializeReferenceListAddBehavior.TryResolveAppendTarget(element, out var targets, out var arrayPath));
                Assert.AreEqual("sidearms", arrayPath);
                CollectionAssert.AreEqual(new Object[] { first, second }, targets,
                    "A multi-object selection must append to every selected object.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void CreateList_MultipleObjects_OverridesTheAddButton()
        {
            var first = ScriptableObject.CreateInstance<ListAddTestObject>();
            var second = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                var serialized = new SerializedObject(new Object[] { first, second });
                var field = SerializeReferenceEditorGUI.CreateList(serialized.FindProperty("sidearms"));

                Assert.IsNotNull(field.Q<ListView>().overridingAddButtonBehavior,
                    "A multi-object selection must get the picker-backed \"+\", not the native add that copies the last element's rid.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Append_NestedList_GrowsOnlyTheInnerList()
        {
            var obj = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                obj.loadouts.Add(new ListAddTestLoadout { weapons = { new TestSword { damage = 1 } } });
                obj.loadouts.Add(new ListAddTestLoadout());

                SerializeReferenceListAddBehavior.Append(new Object[] { obj }, "loadouts.Array.data[0].weapons", SwordName);

                Assert.AreEqual(2, obj.loadouts.Count, "The outer array must not grow.");
                Assert.AreEqual(2, obj.loadouts[0].weapons.Count);
                Assert.IsInstanceOf<TestSword>(obj.loadouts[0].weapons[1]);
                Assert.AreNotSame(obj.loadouts[0].weapons[0], obj.loadouts[0].weapons[1]);
                Assert.AreEqual(0, obj.loadouts[1].weapons.Count);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Append_MultipleTargets_AppendsAnIndependentInstanceToEach()
        {
            var first = ScriptableObject.CreateInstance<ListAddTestObject>();
            var second = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                first.sidearms.Add(new TestSword());
                second.sidearms.Add(new TestSword());

                SerializeReferenceListAddBehavior.Append(new Object[] { first, second }, "sidearms", SwordName);

                Assert.AreEqual(2, first.sidearms.Count);
                Assert.AreEqual(2, second.sidearms.Count, "Every selected object must receive the new element.");

                Assert.IsInstanceOf<TestSword>(first.sidearms[1]);
                Assert.IsInstanceOf<TestSword>(second.sidearms[1]);
                Assert.AreNotSame(first.sidearms[0], first.sidearms[1], "The new element must not alias the previous one.");
                Assert.AreNotSame(second.sidearms[0], second.sidearms[1], "The new element must not alias the previous one.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Append_None_AppendsAnEmptyEntryToEachTarget()
        {
            var first = ScriptableObject.CreateInstance<ListAddTestObject>();
            var second = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                first.sidearms.Add(new TestSword());
                second.sidearms.Add(new TestSword());

                SerializeReferenceListAddBehavior.Append(new Object[] { first, second }, "sidearms", null);

                Assert.AreEqual(2, first.sidearms.Count);
                Assert.AreEqual(2, second.sidearms.Count);
                Assert.IsNull(first.sidearms[1]);
                Assert.IsNull(second.sidearms[1]);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void IMGUIListDraw_NullProperty_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SerializeReferenceIMGUIList.Draw(null, GUIContent.none, typeof(ITestWeapon)));
        }

        [Test]
        public void IMGUIListDraw_NotAManagedReferenceArray_Throws()
        {
            var obj = ScriptableObject.CreateInstance<ListAddTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                Assert.Throws<ArgumentException>(() =>
                    SerializeReferenceIMGUIList.Draw(serialized.FindProperty("counts"), GUIContent.none, typeof(ITestWeapon)),
                    "A list of plain values must be rejected, not silently skipped.");

                Assert.Throws<ArgumentException>(() =>
                    SerializeReferenceIMGUIList.Draw(serialized.FindProperty("loadouts"), GUIContent.none, typeof(ITestWeapon)),
                    "A list of by-value structs must be rejected, not drawn with a type picker.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
