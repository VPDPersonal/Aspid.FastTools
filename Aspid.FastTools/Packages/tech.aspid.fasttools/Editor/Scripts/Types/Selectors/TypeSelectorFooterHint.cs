using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeSelectorFooterHint
    {
        // One line without wrapping: the longest hint, for a type row on a sub-page, must fit the footer of a picker
        // at TypeSelectorWindow.MinWidth, or the ellipsis cuts the keys at its end. So the arrows, which everyone
        // knows, stay out, and the favorite key is spelled by its star.
        internal static string Build(
            bool searchFocused,
            TreeNode selected,
            bool isSelectedSectionCollapsed,
            bool isSearching,
            bool searchChromeOpen,
            bool canNavigateBack,
            bool hasParentPage)
        {
            var parts = new List<string>();

            if (!searchFocused && selected is { IsSectionTitle: true })
                parts.Add(isSelectedSectionCollapsed ? "→ Expand" : "← Collapse");
            else if (!searchFocused && selected is { HasChildren: true } && !isSearching)
                parts.Add("→ Open");
            else if (selected is { IsSelectable: true })
                parts.Add("Enter Select");

            if (!searchFocused && selected is { IsType: true })
            {
                parts.Add(TypeSelectorPreferences.IsFavorite(selected.AssemblyQualifiedName)
                    ? "Space " + TypeSelectorHelpers.StarFilled
                    : "Space " + TypeSelectorHelpers.StarEmpty);
            }

            if (isSearching)
            {
                parts.Add("Esc Clear");
            }
            else if (searchChromeOpen)
            {
                parts.Add("Esc Cancel");
            }
            else
            {
                if (!searchFocused && (canNavigateBack || hasParentPage)) parts.Add("← Back");

                parts.Add("Type to search");
                parts.Add("Esc Close");
            }

            return string.Join(" · ", parts);
        }
    }
}
