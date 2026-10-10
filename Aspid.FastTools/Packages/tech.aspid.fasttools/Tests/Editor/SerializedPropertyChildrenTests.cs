using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.Editors.Tests
{
    internal sealed class SerializedPropertyChildrenTests
    {
        private ChildrenTarget _target;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<ChildrenTarget>();
            _serializedObject = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Object.DestroyImmediate(_target);
        }

        private SerializedProperty Group => _serializedObject.FindProperty(nameof(ChildrenTarget.Group));

        [Test]
        public void VisibleChildren_YieldsDirectVisibleChildrenOfTheProperty()
        {
            var names = Group.VisibleChildren().Select(child => child.name).ToArray();

            CollectionAssert.AreEqual(new[] { "First", "Inner", "Last" }, names);
        }

        [Test]
        public void VisibleChildren_YieldsNothingForAPropertyWithoutChildren()
        {
            var after = _serializedObject.FindProperty(nameof(ChildrenTarget.After));

            Assert.IsEmpty(after.VisibleChildren());
        }

        [Test]
        public void VisibleChildren_LeavesTheSourcePropertyAtItsPlace()
        {
            var group = Group;

            foreach (var _ in group.VisibleChildren()) { }
            var again = group.VisibleChildren().Select(child => child.propertyPath).ToArray();

            Assert.AreEqual(nameof(ChildrenTarget.Group), group.propertyPath);
            Assert.AreEqual(3, again.Length);
        }

        [Test]
        public void VisibleChildren_CanStopEarly()
        {
            var group = Group;
            var first = string.Empty;

            foreach (var child in group.VisibleChildren())
            {
                first = child.name;
                break;
            }

            Assert.AreEqual("First", first);
            Assert.AreEqual(nameof(ChildrenTarget.Group), group.propertyPath);
        }

        [Test]
        public void VisibleChildren_YieldsOneIteratorThatCopyKeeps()
        {
            var kept = Group.VisibleChildren().Select(child => child.Copy()).ToArray();

            CollectionAssert.AreEqual(new[] { "First", "Inner", "Last" }, kept.Select(child => child.name).ToArray());
        }

        [Test]
        public void HasVisibleChildren_ReportsWhetherAnyChildIsVisible()
        {
            Assert.IsTrue(SerializeReferenceNesting.HasVisibleChildren(Group));
            Assert.IsFalse(SerializeReferenceNesting.HasVisibleChildren(_serializedObject.FindProperty(nameof(ChildrenTarget.After))));
            Assert.IsFalse(SerializeReferenceNesting.HasVisibleChildren(Group.FindPropertyRelative(nameof(ChildrenGroup.Hidden))));
        }

        private sealed class ChildrenTarget : ScriptableObject
        {
            public ChildrenGroup Group = new();
            public int After = 0;
        }

        [Serializable]
        private sealed class ChildrenGroup
        {
            public int First = 0;
            [HideInInspector] public int Hidden = 0;
            public ChildrenInner Inner = new();
            public int Last = 0;
        }

        [Serializable]
        private sealed class ChildrenInner
        {
            public int Deep = 0;
        }
    }
}
