using System.Linq;
using UnityEditor;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for the row counters shown by the type picker:
    /// <list type="bullet">
    /// <item><see cref="TreeNode.TypeCount"/> counts the pickable type leaves under a container recursively
    /// (and only the leaves — containers themselves are not counted);</item>
    /// <item><see cref="NavigationController"/> stamps each composed Favorites/Recent section title with the
    /// number of rows it actually surfaced, so the header's counter matches what expanding reveals.</item>
    /// </list>
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorCountTests
    {
        // The stored favorites / recents live as JSON under these project-scoped keys (the storage contract of
        // TypeSelectorPreferences); the fixture snapshots the raw JSON so the developer's real lists survive the tests.
        private static string FavoritesKey => "Aspid.FastTools.TypeSelector.Favorites." + PlayerSettings.productGUID;
        private static string RecentsKey => "Aspid.FastTools.TypeSelector.Recents." + PlayerSettings.productGUID;

        private bool _showFavorites;
        private string _favoritesJson;
        private string _recentsJson;

        [SetUp]
        public void SetUp()
        {
            _showFavorites = TypeSelectorSettings.ShowFavorites;
            _favoritesJson = EditorPrefs.GetString(FavoritesKey, string.Empty);
            _recentsJson = EditorPrefs.GetString(RecentsKey, string.Empty);

            // Start each test from an empty slate.
            TypeSelectorPreferences.ClearFavorites();
            TypeSelectorPreferences.ClearRecents();
        }

        [TearDown]
        public void TearDown()
        {
            TypeSelectorSettings.ShowFavorites = _showFavorites;

            // Clear first: it invalidates TypeSelectorPreferences' favorites cache, so the restored JSON is re-read
            // lazily instead of being shadowed by the stale in-session set.
            TypeSelectorPreferences.ClearFavorites();
            TypeSelectorPreferences.ClearRecents();
            if (!string.IsNullOrEmpty(_favoritesJson)) EditorPrefs.SetString(FavoritesKey, _favoritesJson);
            if (!string.IsNullOrEmpty(_recentsJson)) EditorPrefs.SetString(RecentsKey, _recentsJson);
        }

        [Test]
        public void TypeCount_CountsDescendantTypeLeavesRecursively()
        {
            var root = new TreeNode("/");
            var container = new TreeNode("Container");
            var nested = new TreeNode("Nested");

            nested.Children.Add(new TreeNode("A", typeof(int).AssemblyQualifiedName));
            container.Children.Add(nested);
            container.Children.Add(new TreeNode("B", typeof(string).AssemblyQualifiedName));
            root.Children.Add(container);
            root.Children.Add(new TreeNode("C", typeof(float).AssemblyQualifiedName));

            Assert.AreEqual(3, root.TypeCount, "The root must count every descendant type leaf, at any depth.");
            Assert.AreEqual(2, container.TypeCount, "A container must count only the leaves under itself.");
            Assert.AreEqual(0, new TreeNode("Empty").TypeCount, "A childless container holds no types.");
        }

        [Test]
        public void AppendedSectionTitle_CarriesItsSurfacedRowCount()
        {
            TypeSelectorSettings.ShowFavorites = true;
            TypeSelectorPreferences.ToggleFavorite(typeof(int).AssemblyQualifiedName);
            TypeSelectorPreferences.ToggleFavorite(typeof(string).AssemblyQualifiedName);

            // A favorite outside the candidate set is not surfaced — it must not inflate the counter either.
            TypeSelectorPreferences.ToggleFavorite(typeof(double).AssemblyQualifiedName);

            var root = new TreeNode("/");
            root.Children.Add(new TreeNode("Int32", typeof(int).AssemblyQualifiedName));
            root.Children.Add(new TreeNode("String", typeof(string).AssemblyQualifiedName));

            var controller = new NavigationController(root, composeSections: true);

            var title = controller.CurrentItems.Single(node =>
                node.IsSectionTitle && node.SectionKey == NavigationController.FavoritesSection);

            Assert.AreEqual(2, title.TypeCount,
                "The section title's counter must match the rows the section actually surfaced.");
        }
    }
}
