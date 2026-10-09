using System.Linq;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Order and row text of the picker's search results.
    [TestFixture]
    internal sealed class TypeSelectorSearchTests
    {
        private interface ISingleProbe { }

        private interface IGroupedProbe { }

        private sealed class SingleSearchProbe : ISingleProbe { }

        [TypeSelectorDisplay(Group = "Probes/Search")]
        private sealed class GroupedResultProbe : IGroupedProbe { }

        [TestCase("Button", 0, TestName = "Exact name")]
        [TestCase("button", 0, TestName = "Exact name ignores case")]
        [TestCase("ButtonGroup", 1, TestName = "Name starts with the query")]
        [TestCase("SubButton", 2, TestName = "Name contains the query")]
        public void GetMatchRank_RanksTheName(string name, int expected)
        {
            Assert.AreEqual(expected, Leaf(name).GetMatchRank("Button"));
        }

        [Test]
        public void GetMatchRank_NamespaceOnlyMatch_IsLast()
        {
            Assert.AreEqual(3, Leaf("Other", @namespace: "Button.Tools").GetMatchRank("Button"),
                "A query found only in the namespace must rank below every name match.");
        }

        [Test]
        public void GetMatchRank_FullName_IsExact()
        {
            Assert.AreEqual(0, Leaf("Button").GetMatchRank("Game.Button"));
        }

        [TestCase("List", 0, TestName = "Generic type: the name without its arguments is exact")]
        [TestCase("List<T>", 0, TestName = "Generic type: the full generic name is exact")]
        [TestCase("Lis", 1, TestName = "Generic type: a prefix of the name")]
        [TestCase("Game.List", 0, TestName = "Generic type: the full name without its arguments is exact")]
        public void GetMatchRank_GenericType_IgnoresTheArguments(string filter, int expected)
        {
            Assert.AreEqual(expected, Leaf("List<T>").GetMatchRank(filter));
        }

        [Test]
        public void GetMatchRank_UsesTheTypeName_BehindALabelOrADisambiguation()
        {
            var disambiguated = Leaf("Button (Game)");
            disambiguated.SearchName = "Button";

            var labelled = Leaf("Longsword");
            labelled.SearchName = "Sword";

            Assert.AreEqual(0, disambiguated.GetMatchRank("Button"), "The assembly suffix must not hide an exact name.");
            Assert.AreEqual(0, labelled.GetMatchRank("Sword"), "The real type name must stay an exact match.");
            Assert.AreEqual(1, labelled.GetMatchRank("Long"), "The label must count as a name.");
        }

        [Test]
        public void ApplySearch_ListsTheBestNameMatchFirst_AndKeepsTheHierarchyOrderWithinARank()
        {
            var tools = new TreeNode("Button.Tools", null, "Button.Tools");
            tools.Children.Add(Leaf("Other", @namespace: "Button.Tools"));

            var game = new TreeNode("Game", null, "Game");
            game.Children.Add(Leaf("ButtonGroup"));
            game.Children.Add(Leaf("ButtonClicked"));
            game.Children.Add(Leaf("SubButton"));
            game.Children.Add(Leaf("Button"));

            var root = new TreeNode("/");
            root.Children.Add(tools);
            root.Children.Add(game);

            var navigation = new NavigationController(root);
            navigation.ApplySearch("Button");

            CollectionAssert.AreEqual(
                new[] { "Button", "ButtonGroup", "ButtonClicked", "SubButton", "Other" },
                navigation.CurrentItems.Select(node => node.DisplayName).ToArray(),
                "Exact name, then prefix, then part of the name, then namespace-only matches; equal ranks keep their order.");
        }

        [Test]
        public void ApplySearch_ShowsTheNameFirst_AndTheNamespaceAsContext()
        {
            var root = HierarchyBuilder.Build(new[] { typeof(ISingleProbe) }, TypeAllow.None, includeNoneOption: false);

            var navigation = new NavigationController(root);
            navigation.ApplySearch(nameof(SingleSearchProbe));

            var result = navigation.CurrentItems.Single();
            Assert.AreEqual(nameof(SingleSearchProbe), result.DisplayName,
                "The row text must be the type name, not the name behind its namespace.");
            Assert.AreEqual(typeof(SingleSearchProbe).Namespace, result.Context,
                "The namespace must move to the context, even when the only type collapsed its namespace chain.");
        }

        [Test]
        public void ApplySearch_ShowsTheGroupPathAsContext()
        {
            var root = HierarchyBuilder.Build(new[] { typeof(IGroupedProbe) }, TypeAllow.None, includeNoneOption: false);

            var navigation = new NavigationController(root);
            navigation.ApplySearch(nameof(GroupedResultProbe));

            var result = navigation.CurrentItems.Single();
            Assert.AreEqual(nameof(GroupedResultProbe), result.DisplayName);
            Assert.AreEqual("Probes/Search", result.Context, "A grouped type must show its group path, as the list does.");
        }

        [Test]
        public void ApplySearch_TypeWithoutContext_HasNone()
        {
            var root = new TreeNode("/");
            root.Children.Add(new TreeNode("Button", "Button, Game"));

            var navigation = new NavigationController(root);
            navigation.ApplySearch("Button");

            Assert.IsEmpty(navigation.CurrentItems.Single().Context ?? string.Empty);
        }

        private static TreeNode Leaf(string name, string @namespace = "Game")
        {
            var qualified = $"{@namespace}.{name}";

            return new TreeNode(name, $"{qualified}, Game", qualified)
            {
                Title = name,
                SearchName = name,
                QualifiedName = qualified,
            };
        }
    }
}
