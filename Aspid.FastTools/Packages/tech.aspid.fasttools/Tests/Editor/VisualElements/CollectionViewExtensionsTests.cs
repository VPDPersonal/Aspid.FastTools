using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class CollectionViewExtensionsTests
    {
        private static List<TreeViewItemData<string>> CreateRootItems() => new()
        {
            new TreeViewItemData<string>(1, "Weapons", new List<TreeViewItemData<string>>
            {
                new(2, "Sword"),
            }),
            new TreeViewItemData<string>(3, "Armor"),
        };

        [Test]
        public void SetRootItemsSelf_FillsTreeViewAndReturnsIt()
        {
            var source = new TreeView();

            TreeView tree = source.SetRootItemsSelf(CreateRootItems());

            Assert.AreSame(source, tree);
            CollectionAssert.AreEqual(new[] { 1, 3 }, tree.GetRootIds().ToArray());
            Assert.AreEqual("Sword", tree.GetItemDataForId<string>(2));
        }

        [Test]
        public void SetRootItemsSelf_FillsMultiColumnTreeView()
        {
            var tree = new MultiColumnTreeView().SetRootItemsSelf(CreateRootItems());

            CollectionAssert.AreEqual(new[] { 1, 3 }, tree.GetRootIds().ToArray());
        }

        [Test]
        public void SetItemsSource_OnTreeView_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new TreeView().SetItemsSource(CreateRootItems()));
            Assert.Throws<InvalidOperationException>(() => new MultiColumnTreeView().SetItemsSource(CreateRootItems()));
        }

        [Test]
        public void SetItemsSource_OnListView_SetsSource()
        {
            var items = new List<string> { "Sword", "Armor" };

            var list = new ListView().SetItemsSource(items);

            Assert.AreSame(items, list.itemsSource);
        }
    }
}
