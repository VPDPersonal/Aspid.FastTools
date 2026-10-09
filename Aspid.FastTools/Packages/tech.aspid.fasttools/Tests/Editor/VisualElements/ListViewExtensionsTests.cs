using System;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Anchors for the list and tree view families: the Set, Add and Remove templates keep their meaning,
    /// every event pair registers and unregisters on the right event, and the property setters write their own property.
    /// </summary>
    [TestFixture]
    internal sealed class ListViewExtensionsTests
    {
        #region Set, Add and Remove
        [Test]
        public void OnAdd_SetAddRemove_ManageTheCallbacks()
        {
            Action<BaseListView> first = _ => { };
            Action<BaseListView> second = _ => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.onAdd,
                (list, value) => list.SetOnAdd(value),
                (list, value) => list.AddOnAdd(value),
                (list, value) => list.RemoveOnAdd(value),
                first,
                second);
        }

        [Test]
        public void OnRemove_SetAddRemove_ManageTheCallbacks()
        {
            Action<BaseListView> first = _ => { };
            Action<BaseListView> second = _ => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.onRemove,
                (list, value) => list.SetOnRemove(value),
                (list, value) => list.AddOnRemove(value),
                (list, value) => list.RemoveOnRemove(value),
                first,
                second);
        }

        [Test]
        public void OverridingAddButtonBehavior_SetAddRemove_ManageTheCallbacks()
        {
            Action<BaseListView, Button> first = (_, _) => { };
            Action<BaseListView, Button> second = (_, _) => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.overridingAddButtonBehavior,
                (list, value) => list.SetOverridingAddButtonBehavior(value),
                (list, value) => list.AddOverridingAddButtonBehavior(value),
                (list, value) => list.RemoveOverridingAddButtonBehavior(value),
                first,
                second);
        }

        [Test]
        public void ListView_BindItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement, int> first = (_, _) => { };
            Action<VisualElement, int> second = (_, _) => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.bindItem,
                (list, value) => list.SetBindItem(value),
                (list, value) => list.AddBindItem(value),
                (list, value) => list.RemoveBindItem(value),
                first,
                second);
        }

        [Test]
        public void ListView_UnbindItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement, int> first = (_, _) => { };
            Action<VisualElement, int> second = (_, _) => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.unbindItem,
                (list, value) => list.SetUnbindItem(value),
                (list, value) => list.AddUnbindItem(value),
                (list, value) => list.RemoveUnbindItem(value),
                first,
                second);
        }

        [Test]
        public void ListView_DestroyItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement> first = _ => { };
            Action<VisualElement> second = _ => { };

            AssertSetAddRemove(
                new ListView(),
                list => list.destroyItem,
                (list, value) => list.SetDestroyItem(value),
                (list, value) => list.AddDestroyItem(value),
                (list, value) => list.RemoveDestroyItem(value),
                first,
                second);
        }

        [Test]
        public void TreeView_BindItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement, int> first = (_, _) => { };
            Action<VisualElement, int> second = (_, _) => { };

            AssertSetAddRemove(
                new TreeView(),
                tree => tree.bindItem,
                (tree, value) => tree.SetBindItem(value),
                (tree, value) => tree.AddBindItem(value),
                (tree, value) => tree.RemoveBindItem(value),
                first,
                second);
        }

        [Test]
        public void TreeView_UnbindItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement, int> first = (_, _) => { };
            Action<VisualElement, int> second = (_, _) => { };

            AssertSetAddRemove(
                new TreeView(),
                tree => tree.unbindItem,
                (tree, value) => tree.SetUnbindItem(value),
                (tree, value) => tree.AddUnbindItem(value),
                (tree, value) => tree.RemoveUnbindItem(value),
                first,
                second);
        }

        [Test]
        public void TreeView_DestroyItem_SetAddRemove_ManageTheCallbacks()
        {
            Action<VisualElement> first = _ => { };
            Action<VisualElement> second = _ => { };

            AssertSetAddRemove(
                new TreeView(),
                tree => tree.destroyItem,
                (tree, value) => tree.SetDestroyItem(value),
                (tree, value) => tree.AddDestroyItem(value),
                (tree, value) => tree.RemoveDestroyItem(value),
                first,
                second);
        }

        private static void AssertSetAddRemove<TElement, TDelegate>(
            TElement element,
            Func<TElement, TDelegate> get,
            Func<TElement, TDelegate, TElement> set,
            Func<TElement, TDelegate, TElement> add,
            Func<TElement, TDelegate, TElement> remove,
            TDelegate first,
            TDelegate second)
            where TDelegate : Delegate
        {
            Assert.AreSame(element, set(element, first));
            CollectionAssert.AreEqual(new Delegate[] { first }, get(element).GetInvocationList());

            Assert.AreSame(element, add(element, second));
            CollectionAssert.AreEqual(new Delegate[] { first, second }, get(element).GetInvocationList());

            Assert.AreSame(element, remove(element, first));
            CollectionAssert.AreEqual(new Delegate[] { second }, get(element).GetInvocationList());

            set(element, first);
            CollectionAssert.AreEqual(new Delegate[] { first }, get(element).GetInvocationList(), "Set must replace the callbacks added before.");
        }
        #endregion

        #region Events
        [Test]
        public void SelectionChanged_AddAndRemove_RaiseOnlyWhileRegistered()
        {
            var calls = 0;
            void OnChanged(IEnumerable<object> _) => calls++;
            var list = CreateList().AddSelectionChanged(OnChanged);

            list.SetSelection(0);
            list.RemoveSelectionChanged(OnChanged);
            list.SetSelection(1);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void SelectedIndicesChanged_AddAndRemove_RaiseOnlyWhileRegistered()
        {
            var calls = 0;
            void OnChanged(IEnumerable<int> _) => calls++;
            var list = CreateList().AddSelectedIndicesChanged(OnChanged);

            list.SetSelection(0);
            list.RemoveSelectedIndicesChanged(OnChanged);
            list.SetSelection(1);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void ItemsSourceChanged_AddAndRemove_RaiseOnlyWhileRegistered()
        {
            var calls = 0;
            void OnChanged() => calls++;
            var list = CreateList().AddItemsSourceChanged(OnChanged);

            list.itemsSource = new List<string> { "Sword" };
            var afterAdd = calls;
            list.RemoveItemsSourceChanged(OnChanged);
            list.itemsSource = new List<string> { "Armor" };

            Assert.Greater(afterAdd, 0, "The registered callback must run when the items source changes.");
            Assert.AreEqual(afterAdd, calls, "The removed callback must not run.");
        }

        [Test]
        public void ItemExpandedChanged_AddAndRemove_RaiseOnlyWhileRegistered()
        {
            var calls = 0;
            void OnChanged(TreeViewExpansionChangedArgs _) => calls++;
            var tree = new TreeView()
                .SetMakeItem(() => new Label())
                .SetBindItem((_, _) => { })
                .SetRootItemsSelf(new List<TreeViewItemData<string>>
                {
                    new(1, "Weapons", new List<TreeViewItemData<string>> { new(2, "Sword") }),
                    new(3, "Armor", new List<TreeViewItemData<string>> { new(4, "Helmet") }),
                })
                .AddItemExpandedChanged(OnChanged);

            tree.ExpandItem(1);
            tree.RemoveItemExpandedChanged(OnChanged);
            tree.ExpandItem(3);

            Assert.AreEqual(1, calls);
        }

        private static ListView CreateList() =>
            new ListView()
                .SetMakeItem(() => new Label())
                .SetBindItem((_, _) => { })
                .SetItemsSource(new List<string> { "Sword", "Armor" });
        #endregion

        #region Setters
        [Test]
        public void BaseListView_Setters_ReturnListAndSetProperties()
        {
            Func<VisualElement> makeNoneElement = () => new Label("None");
            var source = new ListView();

            ListView list = source
                .SetAllowAdd(false)
                .SetAllowRemove(false)
                .SetShowAddRemoveFooter(true)
                .SetShowBoundCollectionSize(false)
                .SetReorderMode(ListViewReorderMode.Animated)
                .SetBindingSourceSelectionMode(BindingSourceSelectionMode.AutoAssign)
                .SetMakeNoneElement(makeNoneElement);

            Assert.AreSame(source, list);
            Assert.IsFalse(list.allowAdd);
            Assert.IsFalse(list.allowRemove);
            Assert.IsTrue(list.showAddRemoveFooter);
            Assert.IsFalse(list.showBoundCollectionSize);
            Assert.AreEqual(ListViewReorderMode.Animated, list.reorderMode);
            Assert.AreEqual(BindingSourceSelectionMode.AutoAssign, list.bindingSourceSelectionMode);
            Assert.AreSame(makeNoneElement, list.makeNoneElement);
        }

        [Test]
        public void BaseListView_SetMakeFooter_SetsProperty()
        {
            Func<VisualElement> makeFooter = () => new Label("Footer");

            var list = new ListView().SetMakeFooter(makeFooter);

            Assert.AreSame(makeFooter, list.makeFooter);
        }

        [Test]
        public void BaseListView_FoldoutHeaderSetters_SetProperties()
        {
            var list = new ListView().SetHeaderTitle("Items").SetShowFoldoutHeader(true);

            Assert.AreEqual("Items", list.headerTitle);
            Assert.IsTrue(list.showFoldoutHeader);
        }

        [Test]
        public void BaseListView_SetMakeHeader_SetsProperty()
        {
            Func<VisualElement> makeHeader = () => new Label("Header");

            var list = new ListView().SetMakeHeader(makeHeader);

            Assert.AreSame(makeHeader, list.makeHeader);
        }

        [Test]
        public void BaseVerticalCollectionView_Setters_ReturnListAndSetProperties()
        {
            var source = new ListView();

            ListView list = source
                .SetReorderable(true)
                .SetFixedItemHeight(40f)
                .SetSelectionType(SelectionType.Multiple)
                .SetHorizontalScrollingEnabled(true)
                .SetVirtualizationMethod(CollectionVirtualizationMethod.DynamicHeight)
                .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.All);

            Assert.AreSame(source, list);
            Assert.IsTrue(list.reorderable);
            Assert.AreEqual(40f, list.fixedItemHeight);
            Assert.AreEqual(SelectionType.Multiple, list.selectionType);
            Assert.IsTrue(list.horizontalScrollingEnabled);
            Assert.AreEqual(CollectionVirtualizationMethod.DynamicHeight, list.virtualizationMethod);
            Assert.AreEqual(AlternatingRowBackground.All, list.showAlternatingRowBackgrounds);
        }

        [Test]
        public void SetSelectedIndex_SelectsTheItem()
        {
            var list = CreateList().SetSelectedIndex(1);

            Assert.AreEqual(1, list.selectedIndex);
        }

        [Test]
        public void ListView_SetMakeItemAndItemTemplate_SetProperties()
        {
            Func<VisualElement> makeItem = () => new Label();
            var template = ScriptableObject.CreateInstance<VisualTreeAsset>();

            var list = new ListView().SetMakeItem(makeItem);
            Assert.AreSame(makeItem, list.makeItem);

            list.SetItemTemplate(template);
            Assert.AreSame(template, list.itemTemplate);

            UnityEngine.Object.DestroyImmediate(template);
        }

        [Test]
        public void TreeView_Setters_ReturnTreeAndSetProperties()
        {
            Func<VisualElement> makeItem = () => new Label();
            var template = ScriptableObject.CreateInstance<VisualTreeAsset>();
            var source = new TreeView();

            TreeView tree = source
                .SetAutoExpand(true)
                .SetMakeItem(makeItem)
                .SetItemTemplate(template);

            Assert.AreSame(source, tree);
            Assert.IsTrue(tree.autoExpand);
            Assert.AreSame(template, tree.itemTemplate);

            UnityEngine.Object.DestroyImmediate(template);
        }

        [Test]
        public void MultiColumnViews_SetSortingMode_SetsProperty()
        {
            var list = new MultiColumnListView().SetSortingMode(ColumnSortingMode.Custom);
            var tree = new MultiColumnTreeView().SetSortingMode(ColumnSortingMode.Default);

            Assert.AreEqual(ColumnSortingMode.Custom, list.sortingMode);
            Assert.AreEqual(ColumnSortingMode.Default, tree.sortingMode);
        }
        #endregion
    }
}
