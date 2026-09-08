using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    internal sealed class VisualElementInteractionTests
    {
        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator Navigation_DoesNotActOnSelectionInsideCollapsedContainer()
        {
            var host = _window.rootVisualElement;
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

        [UnityTest]
        public IEnumerator InspectorHeader_LeavingAfterStatusClearedResetsHoverAccents()
        {
            var header = new AspidInspectorHeader();
            _window.rootVisualElement.Add(header);
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
