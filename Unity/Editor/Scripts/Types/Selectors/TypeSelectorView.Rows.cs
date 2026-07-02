using System;
using System.Linq;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Row factory and per-row binding — leading icon/glyph, title, favorite toggle and the drill-in arrow —
    // plus the section-header collapse/expand interaction wired onto each row.
    internal sealed partial class TypeSelectorView
    {
        private const string ItemClass = BlockClass + "__item";
        private const string ItemInSectionClass = ItemClass + "--in-section";
        private const string ItemCurrentModifier = ItemClass + "--current";
        private const string ItemIconClass = BlockClass + "__item-icon";
        private const string ItemGlyphClass = BlockClass + "__item-glyph";
        private const string ItemTitleClass = BlockClass + "__item-title";
        private const string ItemCheckClass = BlockClass + "__item-check";
        private const string ItemCountClass = BlockClass + "__item-count";
        private const string ItemArrowClass = BlockClass + "__item-arrow";
        private const string RowAfterPinnedModifier = BlockClass + "__item--after-pinned";
        private const string SectionTitleClass = BlockClass + "__section-title";
        private const string FavoriteToggleClass = BlockClass + "__favorite-toggle";
        private const string FavoriteToggleOnModifier = FavoriteToggleClass + "--favorite-on";
        private const string ItemIconCollapsedModifier = ItemIconClass + "--collapsed";

        private const string TypeFallbackIcon = "d_cs Script Icon";
        private const string ContainerFallbackIcon = "d_Folder Icon";
        private const string ContainerOpenFallbackIcon = "d_FolderOpened Icon";
        private const string FavoritesCollapsedIcon = "d_Favorite";
        private const string FavoritesExpandedIcon = "d_Favorite Icon";
        private const string RecentCollapsedIcon = "d_UnityEditor.HistoryWindow";
        private const string RecentExpandedIcon = "d_UnityEditor.HistoryWindow";

        private VisualElement CreateListItem()
        {
            var icon = new Image()
                .AddClass(ItemIconClass)
                .SetPickingMode(PickingMode.Ignore);

            var glyph = new Label()
                .AddClass(ItemGlyphClass)
                .SetPickingMode(PickingMode.Ignore);

            var label = new Label()
                .AddClass(ItemTitleClass);

            var check = new Label(TypeSelectorGlyphs.Check)
                .AddClass(ItemCheckClass)
                .SetPickingMode(PickingMode.Ignore);

            var count = new Label()
                .AddClass(ItemCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var favorite = new Button()
                .AddClass(FavoriteToggleClass)
                .SetText(TypeSelectorGlyphs.StarEmpty);

            var arrow = new Label("›")
                .AddClass(ItemArrowClass);

            var row = new VisualElement()
                .AddClass(ItemClass)
                .AddChild(icon)
                .AddChild(glyph)
                .AddChild(label)
                .AddChild(check)
                .AddChild(count)
                .AddChild(favorite)
                .AddChild(arrow);

            row.RegisterCallback<ClickEvent>(OnRowClicked);

            return row;
        }

        private void BindListItem(VisualElement element, int index)
        {
            var items = _pages.Count > 0 ? Nav.CurrentItems : null;

            if (items is null) return;
            if (index < 0 || index >= items.Count) return;

            var node = items[index];
            var isSectionTitle = node.IsSectionTitle;

            // OnRowClicked reads this to know which node it operates on after row recycling.
            element.userData = node;

            var isCurrent = IsCurrentValue(node);

            element.EnableInClass(SectionTitleClass, isSectionTitle);
            element.EnableInClass(ItemInSectionClass, !isSectionTitle && node.SectionKey is not null);
            element.EnableInClass(ItemCurrentModifier, isCurrent);

            // The row root itself is the collection-view item (a non-reorderable ListView adds the class
            // straight onto the makeItem element, no per-row wrapper), so the divider goes here; the parent
            // is the content container shared by every row.
            element.EnableInClass(RowAfterPinnedModifier, IsFirstRowAfterPinnedBlock(items, index));

            element.SetPickingMode(PickingMode.Position);

            element.Q<Label>(className: ItemTitleClass)
                .SetText(node.DisplayName)
                .SetTooltip(node.Tooltip);

            BindLeading(element.Q<Image>(className: ItemIconClass), element.Q<Label>(className: ItemGlyphClass), node, index == _listView.selectedIndex);
            BindFavorite(element.Q<Button>(className: FavoriteToggleClass), node);

            element.Q<Label>(className: ItemCheckClass)
                .SetDisplay(isCurrent ? DisplayStyle.Flex : DisplayStyle.None);

            var typeCount = TypeCountFor(node);
            element.Q<Label>(className: ItemCountClass)
                .SetText(typeCount > 0 ? typeCount.ToString() : string.Empty)
                .SetDisplay(typeCount > 0 ? DisplayStyle.Flex : DisplayStyle.None);

            element.Q<Label>(className: ItemArrowClass)
                .SetDisplay(node.HasChildren && !Nav.IsSearching
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);
        }

        // The row is "current" when it names the value the field already holds — the concrete type by its
        // assembly-qualified name, or the <None> row when the field is empty — so the stored value reads
        // apart from the keyboard highlight while browsing. Base page only: a coincidental match on a
        // generic-argument page is not the field's value.
        private bool IsCurrentValue(TreeNode node)
        {
            if (!_pages[^1].IsBase) return false;

            // Null = the host has no current-value concept at all (a list "+" append, a missing-type Fix, the bulk
            // project picker) — nothing wears the check there, least of all <None>, which only an EMPTY STRING (a
            // field genuinely holding None) rightly marks.
            if (_currentAqn is null) return false;

            return _currentAqn.Length > 0
                ? node.IsType && node.AssemblyQualifiedName == _currentAqn
                : node.IsSelectable && node.DisplayName == TypeSelectorHelpers.NoneOption;
        }

        // Containers surface how many pickable types they hold; section titles carry their composed row
        // count. Type leaves show no counter, and search results are a flat type list with none either.
        private int TypeCountFor(TreeNode node)
        {
            if (Nav.IsSearching) return 0;
            return node.IsSectionTitle || node.HasChildren ? node.TypeCount : 0;
        }

        // True for the first ordinary root category after the pinned block (<None> plus the Favorites/Recent
        // sections) — the row that carries the divider separating the pinned rows from the namespace
        // hierarchy. Only the base root page composes a pinned block, so everywhere else this stays false.
        private bool IsFirstRowAfterPinnedBlock(List<TreeNode> items, int index)
        {
            if (!Nav.IsAtRoot || index <= 0 || index >= items.Count) return false;
            if (IsPinnedRow(items[index])) return false;

            return IsPinnedRow(items[index - 1]);
        }

        private static bool IsPinnedRow(TreeNode node) =>
            node.IsSectionTitle || node.SectionKey is not null || node.DisplayName == TypeSelectorHelpers.NoneOption;

        private void BindLeading(Image icon, Label glyph, TreeNode node, bool isSelected)
        {
            if (node.DisplayName == TypeSelectorHelpers.NoneOption)
            {
                icon.SetDisplay(DisplayStyle.None);
                glyph.SetText(TypeSelectorGlyphs.None).SetDisplay(DisplayStyle.Flex);
                return;
            }

            glyph.SetDisplay(DisplayStyle.None);

            var sectionCollapsed = false;
            Texture texture;

            if (node.IsSectionTitle)
            {
                sectionCollapsed = Nav.IsSectionCollapsed(node.SectionKey);
                texture = TypeSelectorIconResolver.Resolve(SectionIcon(node.SectionKey, sectionCollapsed));
            }
            else
            {
                texture = TypeSelectorIconResolver.Resolve(node.Icon);

                if (texture is null)
                {
                    var fallback = FallbackIconName(node);
                    if (fallback == ContainerFallbackIcon && isSelected) fallback = ContainerOpenFallbackIcon;
                    texture = TypeSelectorIconResolver.Resolve(fallback);
                }
            }

            icon
                .EnableInClass(ItemIconCollapsedModifier, sectionCollapsed)
                .SetImage(texture)
                .SetDisplay(texture is not null ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private static string FallbackIconName(TreeNode node)
        {
            if (node.IsType) return TypeFallbackIcon;
            return node.HasChildren ? ContainerFallbackIcon : null;
        }

        private static string SectionIcon(string sectionKey, bool collapsed)
        {
            if (sectionKey == NavigationController.RecentSection)
                return collapsed ? RecentCollapsedIcon : RecentExpandedIcon;

            return collapsed ? FavoritesCollapsedIcon : FavoritesExpandedIcon;
        }

        private void BindFavorite(Button favorite, TreeNode node)
        {
            // Replace any handler bound to a previously recycled row.
            favorite.clickable = new Clickable(() => ToggleFavorite(node));

            if (!node.IsType)
            {
                favorite.SetDisplay(DisplayStyle.None);
                return;
            }

            var isFavorite = TypeSelectorPreferences.IsFavorite(node.AssemblyQualifiedName);

            favorite
                .SetDisplay(DisplayStyle.Flex)
                .SetText(isFavorite ? TypeSelectorGlyphs.StarFilled : TypeSelectorGlyphs.StarEmpty)
                .EnableInClass(FavoriteToggleOnModifier, isFavorite);
        }

        private void OnRowClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not VisualElement row) return;
            if (row.userData is not TreeNode node || !node.IsSectionTitle) return;

            ToggleSectionKeepSelection(node);
            evt.StopPropagation();
        }

        // Collapses/expands a section and re-selects its header at the new index, so a keyboard or mouse toggle leaves
        // the highlight on the section the user is acting on (the list rebuilds, so the index can shift).
        private void ToggleSectionKeepSelection(TreeNode sectionTitle)
        {
            Nav.ToggleSection(sectionTitle.SectionKey);
            RefreshView();

            var index = Nav.CurrentItems.IndexOf(sectionTitle);
            if (index >= 0) SetSelectedIndex(index);

            UpdateFooterHint();
        }
    }
}
