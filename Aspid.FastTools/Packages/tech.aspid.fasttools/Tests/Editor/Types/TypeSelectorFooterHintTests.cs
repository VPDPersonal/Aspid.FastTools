using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="TypeSelectorFooterHint"/>: the hint is one line that never wraps, so its longest
    /// form has to fit the footer of a picker at <see cref="TypeSelectorWindow.MinWidth"/>.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorFooterHintTests
    {
        // The footer line is about 355 pt wide at the minimum picker width. The 60-character hint for a type row on
        // a sub-page measures 344 pt in 12 px Inter, so any longer form risks the ellipsis eating the last keys.
        private const int MaxLength = 60;

        private static IEnumerable<TreeNode> Nodes()
        {
            yield return null;
            yield return new TreeNode(TypeSelectorHelpers.NoneOption, null, TypeSelectorHelpers.NoneOption);
            yield return new TreeNode("Game", null, "Game") { Children = { new TreeNode("Sword", "Game.Sword, Game") } };
            yield return new TreeNode("Favorites") { Kind = TreeNodeKind.SectionTitle };
            yield return new TreeNode("Sword", "Game.Sword, Game");
        }

        [Test]
        public void EveryHint_FitsTheFooterAtTheMinimumWidth()
        {
            var bools = new[] { false, true };

            foreach (var node in Nodes())
            foreach (var searchFocused in bools)
            foreach (var collapsed in bools)
            foreach (var searching in bools)
            foreach (var chromeOpen in bools)
            foreach (var canNavigateBack in bools)
            foreach (var hasParentPage in bools)
            {
                var hint = TypeSelectorFooterHint.Build(
                    searchFocused: searchFocused,
                    selected: node,
                    isSelectedSectionCollapsed: collapsed,
                    isSearching: searching,
                    searchChromeOpen: chromeOpen,
                    canNavigateBack: canNavigateBack,
                    hasParentPage: hasParentPage);

                Assert.LessOrEqual(hint.Length, MaxLength, $"'{hint}' is too long for the footer.");
            }
        }

        [Test]
        public void ReadyPicker_ShowsTheSearchAndCloseKeys_NextToTheLongestRowHints()
        {
            var type = Nodes().Last();

            var hint = TypeSelectorFooterHint.Build(
                searchFocused: false,
                selected: type,
                isSelectedSectionCollapsed: false,
                isSearching: false,
                searchChromeOpen: false,
                canNavigateBack: true,
                hasParentPage: false);

            StringAssert.Contains("Enter Select", hint);
            StringAssert.Contains("Space", hint);
            StringAssert.Contains("← Back", hint);
            StringAssert.Contains("Type to search", hint);
            StringAssert.EndsWith("Esc Close", hint);
        }
    }
}
