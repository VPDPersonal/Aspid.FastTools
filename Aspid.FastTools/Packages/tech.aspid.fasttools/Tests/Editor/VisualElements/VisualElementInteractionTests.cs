using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    internal sealed class VisualElementInteractionTests
    {
        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator Navigation_DoesNotActOnSelectionInsideCollapsedContainer()
        {
            var host = _panel.Root;
            var container = new VisualElement();
            var target = new VisualElement();
            container.Add(target);
            host.Add(container);

            var actions = new List<string>();
            var ring = new NavRing(host, "nav-target", "nav-focused");
            ring.Register(target,
                () => actions.Add("activate"),
                delta => actions.Add(delta < 0 ? "decrease" : "increase"),
                () => actions.Add("remove"));
            yield return null;

            host.Focus();
            SendKey(host, KeyCode.DownArrow);
            Assert.IsTrue(target.ClassListContains("nav-focused"));

            var keys = new[] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Return, KeyCode.Delete, KeyCode.Backspace };
            foreach (var key in keys) SendKey(host, key);
            CollectionAssert.AreEqual(new[] { "decrease", "increase", "activate", "remove", "remove" }, actions);
            actions.Clear();

            container.style.display = DisplayStyle.None;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, container.resolvedStyle.display);

            foreach (var key in keys) SendKey(host, key);
            Assert.IsEmpty(actions, "Collapsing a selected row's ancestor must block every action on that row.");
        }

        [Test]
        public void NavRing_FocusElement_FocusesARegisteredTargetAndIgnoresOthers()
        {
            var host = new VisualElement();
            var first = new VisualElement();
            var second = new VisualElement();
            var scrolled = new List<VisualElement>();

            var ring = new NavRing(host, "nav-target", "nav-focused", scrollTo: scrolled.Add);
            ring.Register(first, () => { });
            ring.Register(second, () => { });

            ring.Focus(second, scrollTo: false);
            Assert.IsTrue(second.ClassListContains("nav-focused"));
            Assert.IsFalse(first.ClassListContains("nav-focused"));
            Assert.IsEmpty(scrolled, "scrollTo: false must not scroll.");

            ring.Focus(new VisualElement(), scrollTo: false);
            Assert.IsTrue(second.ClassListContains("nav-focused"), "An element that is not a target keeps the focus where it was.");

            ring.Focus(first);
            Assert.IsTrue(first.ClassListContains("nav-focused"));
            Assert.IsFalse(second.ClassListContains("nav-focused"));
            CollectionAssert.AreEqual(new[] { first }, scrolled);
        }

        [UnityTest]
        public IEnumerator InspectorHeader_LeavingAfterStatusClearedResetsHoverAccents()
        {
            var header = new AspidInspectorHeader();
            _panel.Root.Add(header);
            yield return null;

            var icon = header.Q<Image>(className: "aspid-fasttools-inspector-header__icon");
            var container = header.Q<AspidBox>();
            using (var enter = MouseEnterEvent.GetPooled())
            {
                enter.target = icon;
                icon.SendEvent(enter);
            }

            Assert.AreEqual(StatusStyle.Type.Success, container.Status);
            Assert.AreEqual(ThemeStyle.Type.Darkness, container.Theme);

            header.Status = StatusStyle.Type.None;
            using (var leave = MouseLeaveEvent.GetPooled())
            {
                leave.target = icon;
                icon.SendEvent(leave);
            }

            Assert.AreEqual(StatusStyle.Type.None, container.Status);
            Assert.AreEqual(ThemeStyle.Type.Dark, container.Theme);
            foreach (var label in header.Query<AspidLabel>().ToList())
                Assert.AreEqual(StatusStyle.Type.None, label.LabelStatus);
        }

        private static void SendKey(VisualElement host, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled('\0', key, EventModifiers.None);
            evt.target = host;
            host.SendEvent(evt);
        }
    }
}
