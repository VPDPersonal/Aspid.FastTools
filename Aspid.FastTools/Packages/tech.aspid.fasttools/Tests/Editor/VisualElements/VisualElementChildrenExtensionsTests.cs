using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class VisualElementChildrenExtensionsTests
    {
        private static VisualElement CreateParent(out VisualElement[] children)
        {
            children = new[]
            {
                new VisualElement { name = "a" },
                new VisualElement { name = "b" },
                new VisualElement { name = "c" },
            };

            var parent = new VisualElement();
            foreach (var child in children)
                parent.Add(child);

            return parent;
        }

        [Test]
        public void AddChildren_MovesEveryChildOfAnotherElement()
        {
            var source = CreateParent(out var children);
            var target = new VisualElement();

            target.AddChildren(source.Children());

            Assert.AreEqual(0, source.childCount);
            CollectionAssert.AreEqual(children, target.Children().ToArray());
        }

        [Test]
        public void AddChildrenIf_MovesEveryChildOfAnotherElement()
        {
            var source = CreateParent(out var children);
            var target = new VisualElement();

            target.AddChildrenIf(true, source.Children());

            Assert.AreEqual(0, source.childCount);
            CollectionAssert.AreEqual(children, target.Children().ToArray());
        }

        [Test]
        public void InsertChildren_MovesEveryChildOfAnotherElement()
        {
            var source = CreateParent(out var children);
            var existing = new VisualElement();
            var target = new VisualElement().AddChild(existing);

            target.InsertChildren(0, source.Children());

            Assert.AreEqual(0, source.childCount);
            CollectionAssert.AreEqual(children.Append(existing), target.Children().ToArray());
        }

        [Test]
        public void InsertChildrenIf_MovesEveryChildOfAnotherElement()
        {
            var source = CreateParent(out var children);
            var target = new VisualElement();

            target.InsertChildrenIf(true, 0, source.Children());

            Assert.AreEqual(0, source.childCount);
            CollectionAssert.AreEqual(children, target.Children().ToArray());
        }

        [Test]
        public void RemoveChildren_RemovesEveryOwnChild()
        {
            var parent = CreateParent(out _);

            parent.RemoveChildren(parent.Children());

            Assert.AreEqual(0, parent.childCount);
        }

        [Test]
        public void RemoveChildrenIf_RemovesEveryOwnChild()
        {
            var parent = CreateParent(out _);

            parent.RemoveChildrenIf(true, parent.Children());

            Assert.AreEqual(0, parent.childCount);
        }
    }
}
