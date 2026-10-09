using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
#pragma warning disable CS0649
    [Serializable]
    internal sealed class DecoratedParent
    {
        [Tooltip("Child tip")]
        [Header("First")]
        [Space(7)]
        [Header("Second")]
        [SerializeReference] public object child;

        [Header("Late", order = 2)]
        [Header("Early", order = 1)]
        [SerializeReference] public List<object> items = new();

        [SerializeReference] public object plain;
    }

    [Serializable]
    internal sealed class SpacedParent
    {
        [Space(7)] [SerializeReference] public object child;
        [Space(3)] [SerializeReference] public object other;
    }

    [Serializable]
    internal sealed class UnspacedParent
    {
        [SerializeReference] public object child;
        [SerializeReference] public object other;
    }

#pragma warning restore CS0649

    internal sealed class DecoratedHost : ScriptableObject
    {
        [SerializeReference] public object parent = new DecoratedParent();
        [SerializeReference] public object spaced = new SpacedParent();
        [SerializeReference] public object unspaced = new UnspacedParent();
    }

    // A nested reference is drawn by the package, which skips Unity's decorator drawers, so [Tooltip], [Header] and
    // [Space] are read from the field and re-emitted by both drawers.
    [TestFixture]
    internal sealed class SerializeReferenceDecoratorsTests
    {
        private const string HeaderClass = "unity-header-drawer__label";

        private DecoratedHost _host;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _host = ScriptableObject.CreateInstance<DecoratedHost>();
            _serializedObject = new SerializedObject(_host);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void For_SeveralHeadersAndSpaces_ReturnsAllOfThemInDeclarationOrder()
        {
            var decorators = SerializeReferenceDecorators.For(_serializedObject.FindProperty("parent.child"));

            Assert.AreEqual("Child tip", decorators.Tooltip);
            Assert.AreEqual(3, decorators.Items.Count);
            Assert.AreEqual("First", ((HeaderAttribute)decorators.Items[0]).header);
            Assert.AreEqual(7f, ((SpaceAttribute)decorators.Items[1]).height);
            Assert.AreEqual("Second", ((HeaderAttribute)decorators.Items[2]).header);
        }

        [Test]
        public void For_HeadersWithOrder_SortsThemByOrder()
        {
            var decorators = SerializeReferenceDecorators.For(_serializedObject.FindProperty("parent.items"));

            Assert.AreEqual(2, decorators.Items.Count);
            Assert.AreEqual("Early", ((HeaderAttribute)decorators.Items[0]).header);
            Assert.AreEqual("Late", ((HeaderAttribute)decorators.Items[1]).header);
        }

        [Test]
        public void For_FieldWithoutDecorators_ReturnsNone()
        {
            var decorators = SerializeReferenceDecorators.For(_serializedObject.FindProperty("parent.plain"));

            Assert.IsEmpty(decorators.Tooltip);
            Assert.IsEmpty(decorators.Items);
        }

        [Test]
        public void Field_NestedReferenceWithSeveralDecorators_BuildsThemAboveTheField()
        {
            var parent = _serializedObject.FindProperty("parent");
            parent.isExpanded = true;

            var field = new SerializeReferenceField("Parent", parent);
            var child = field.Query<SerializeReferenceField>().Where(candidate => candidate.tooltip == "Child tip").First();

            Assert.IsNotNull(child, "The [Tooltip] of the nested reference goes onto its field.");

            var group = child.hierarchy.parent;
            Assert.AreEqual(4, group.hierarchy.childCount);
            Assert.AreEqual("First", ((Label)group.hierarchy[0]).text);
            Assert.IsTrue(group.hierarchy[0].ClassListContains(HeaderClass));
            Assert.AreEqual(7f, group.hierarchy[1].style.height.value.value);
            Assert.AreEqual("Second", ((Label)group.hierarchy[2]).text);
            Assert.AreSame(child, group.hierarchy[3]);
        }

        [Test]
        public void Field_NestedListWithOrderedHeaders_BuildsThemInOrderAboveTheList()
        {
            var parent = _serializedObject.FindProperty("parent");
            parent.isExpanded = true;

            var field = new SerializeReferenceField("Parent", parent);
            var list = field.Q<SerializeReferenceListField>();

            var group = list.hierarchy.parent;
            Assert.AreEqual(3, group.hierarchy.childCount);
            Assert.AreEqual("Early", ((Label)group.hierarchy[0]).text);
            Assert.AreEqual("Late", ((Label)group.hierarchy[1]).text);
            Assert.AreSame(list, group.hierarchy[2]);
        }

        [Test]
        public void GetHeight_NestedReferencesWithSpace_ReservesTheSpaceAboveThem()
        {
            var spaced = _serializedObject.FindProperty("spaced");
            var unspaced = _serializedObject.FindProperty("unspaced");
            spaced.isExpanded = true;
            unspaced.isExpanded = true;

            var extra = SerializeReferenceIMGUIPropertyDrawer.GetHeight(spaced) -
                        SerializeReferenceIMGUIPropertyDrawer.GetHeight(unspaced);

            Assert.AreEqual(10f, extra, 0.001f, "Space(7) and Space(3) add their heights to the rows they precede.");
        }
    }
}
