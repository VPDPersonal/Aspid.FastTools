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
        public void SetRootItemsSelf_Null_ClearsTree()
        {
            var tree = new TreeView().SetRootItemsSelf(CreateRootItems());

            tree.SetRootItemsSelf<TreeView, string>(null);

            CollectionAssert.IsEmpty(tree.GetRootIds());
        }

        [Test]
        public void SetItemsSource_Null_OnTreeView_ClearsTree()
        {
            var tree = new TreeView().SetRootItemsSelf(CreateRootItems());

            Assert.AreSame(tree, tree.SetItemsSource(null));
            CollectionAssert.IsEmpty(tree.GetRootIds());
        }

        [Test]
        public void SetItemsSource_MatchingItems_OnTreeView_FillsTree()
        {
            var items = new List<TreeViewItemData<object>> { new(1, "Weapons"), new(2, "Armor") };

            var tree = new TreeView().SetItemsSource(items);

            CollectionAssert.AreEqual(new[] { 1, 2 }, tree.GetRootIds().ToArray());
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
