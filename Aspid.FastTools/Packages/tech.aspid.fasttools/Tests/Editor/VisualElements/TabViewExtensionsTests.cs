using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class TabViewExtensionsTests
    {
        private static void SendPointerDown(VisualElement target)
        {
            using var evt = PointerDownEvent.GetPooled();
            evt.target = target;
            target.SendEvent(evt);
        }

        [Test]
        public void Tab_Setters_ReturnTab()
        {
            var source = new Tab();
            var icon = Background.FromTexture2D(Texture2D.whiteTexture);

            Tab tab = source
                .SetLabel("Stats")
                .SetIconImage(icon)
                .SetCloseable(true);

            Assert.AreSame(source, tab);
            Assert.AreEqual("Stats", tab.label);
            Assert.AreEqual(icon, tab.iconImage);
            Assert.IsTrue(tab.closeable);
        }

        [Test]
        public void TabView_Setters_ReturnTabView()
        {
            var first = new Tab("First");
            var second = new Tab("Second");
            var source = new TabView();

            TabView view = source
                .AddChild(first)
                .AddChild(second)
                .SetReorderable(true)
                .SetSelectedTabIndex(1);

            Assert.AreSame(source, view);
            Assert.IsTrue(view.reorderable);
            Assert.AreSame(second, view.activeTab);

            view.SetActiveTab(first);

            Assert.AreSame(first, view.activeTab);
            Assert.AreEqual(0, view.selectedTabIndex);
        }

        [Test]
        public void TabView_SetSelectedTabIndex_IgnoresIndexOutOfRange()
        {
            var first = new Tab("First");
            var view = new TabView()
                .AddChild(first)
                .AddChild(new Tab("Second"))
                .SetSelectedTabIndex(5)
                .SetSelectedTabIndex(-1);

            Assert.AreSame(first, view.activeTab);
        }

        [Test]
        public void TabView_SetActiveTab_ThrowsForTabOutsideView()
        {
            var view = new TabView().AddChild(new Tab("First"));

            Assert.Catch(() => view.SetActiveTab(new Tab("Foreign")));
        }

        [Test]
        public void TabView_ActiveTabChanged_AddAndRemove()
        {
            var first = new Tab("First");
            var second = new Tab("Second");
            var calls = new List<(Tab previous, Tab current)>();
            void OnChanged(Tab previous, Tab current) => calls.Add((previous, current));

            var view = new TabView()
                .AddChild(first)
                .AddChild(second)
                .AddActiveTabChanged(OnChanged)
                .SetSelectedTabIndex(1);

            CollectionAssert.AreEqual(new[] { (first, second) }, calls);

            view.RemoveActiveTabChanged(OnChanged).SetSelectedTabIndex(0);

            Assert.AreEqual(1, calls.Count);
        }

        [Test]
        public void TabView_TabReordered_AddAndRemove()
        {
            var calls = new List<(int from, int to)>();
            void OnReordered(int from, int to) => calls.Add((from, to));

            var view = new TabView()
                .SetReorderable(true)
                .AddChild(new Tab("First"))
                .AddChild(new Tab("Second"))
                .AddTabReordered(OnReordered);

            view.ReorderTab(0, 1);
            CollectionAssert.AreEqual(new[] { (0, 1) }, calls);

            view.RemoveTabReordered(OnReordered).ReorderTab(1, 0);
            Assert.AreEqual(1, calls.Count);
        }

        [UnityTest]
        public IEnumerator Tab_Selected_AddAndRemove()
        {
            using var panel = new TestPanel();
            var selected = new List<Tab>();
            void OnSelected(Tab tab) => selected.Add(tab);

            var tab = new Tab("First").AddSelected(OnSelected);
            panel.Root.Add(new TabView().AddChild(tab));
            yield return null;

            SendPointerDown(tab.tabHeader);
            CollectionAssert.AreEqual(new[] { tab }, selected);

            tab.RemoveSelected(OnSelected);
            SendPointerDown(tab.tabHeader);
            Assert.AreEqual(1, selected.Count);
        }

        [UnityTest]
        public IEnumerator Tab_Closing_CanCancelClose()
        {
            using var panel = new TestPanel();
            var allowClose = false;
            var closed = new List<Tab>();
            bool OnClosing() => allowClose;
            void OnClosed(Tab tab) => closed.Add(tab);

            var tab = new Tab("First")
                .SetCloseable(true)
                .AddClosing(OnClosing)
                .AddClosed(OnClosed);
            var view = new TabView().AddChild(tab);
            panel.Root.Add(view);
            yield return null;

            var closeButton = tab.tabHeader.Q(className: Tab.closeButtonUssClassName);
            SendPointerDown(closeButton);

            Assert.IsEmpty(closed);
            Assert.IsTrue(view.Contains(tab));

            tab.RemoveClosing(OnClosing);
            SendPointerDown(closeButton);

            CollectionAssert.AreEqual(new[] { tab }, closed);
            Assert.IsFalse(view.Contains(tab));
        }

        [UnityTest]
        public IEnumerator TabView_TabClosed_AddAndRemove()
        {
            using var panel = new TestPanel();
            var calls = new List<(Tab tab, int index)>();
            void OnTabClosed(Tab tab, int index) => calls.Add((tab, index));

            var first = new Tab("First").SetCloseable(true);
            var second = new Tab("Second").SetCloseable(true);
            var view = new TabView()
                .AddChild(first)
                .AddChild(second)
                .AddTabClosed(OnTabClosed);
            panel.Root.Add(view);
            yield return null;

            SendPointerDown(second.tabHeader.Q(className: Tab.closeButtonUssClassName));
            CollectionAssert.AreEqual(new[] { (second, 1) }, calls);

            view.RemoveTabClosed(OnTabClosed);
            SendPointerDown(first.tabHeader.Q(className: Tab.closeButtonUssClassName));
            Assert.AreEqual(1, calls.Count);
        }
    }
}
