using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
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
        private const string ItemDividerClass = BlockClass + "__item-divider";
        private const string ItemContentClass = BlockClass + "__item-content";
        private const string RowAfterPinnedModifier = BlockClass + "__item--after-pinned";
        private const string SectionTitleClass = BlockClass + "__section-title";
        private const string FavoriteToggleClass = BlockClass + "__favorite-toggle";
        private const string FavoriteToggleOnModifier = FavoriteToggleClass + "--favorite-on";
        private const string ItemIconCollapsedModifier = ItemIconClass + "--collapsed";

        private const string ContainerFallbackIcon = "d_Folder Icon";
        private const string ContainerOpenFallbackIcon = "d_FolderOpened Icon";
        private const string FavoritesCollapsedIcon = "d_Favorite";
        private const string FavoritesExpandedIcon = "d_Favorite Icon";
        private const string RecentCollapsedIcon = "d_UnityEditor.HistoryWindow";
        private const string RecentExpandedIcon = "d_UnityEditor.HistoryWindow";

        private VisualElement CreateListItem()
        {
            // The divider rides inside the row shell rather than being a border, which would curve along the
            // content's rounded corners. Every visual lives on the content wrapper, so the shell itself stays bare.
            var divider = new VisualElement()
                .AddClass(ItemDividerClass)
                .SetPickingMode(PickingMode.Ignore);

            var icon = new Image()
                .AddClass(ItemIconClass)
                .SetPickingMode(PickingMode.Ignore);

            var glyph = new Label()
                .AddClass(ItemGlyphClass)
                .SetPickingMode(PickingMode.Ignore);

            var label = new Label()
                .AddClass(ItemTitleClass);

            var check = new Label(TypeSelectorHelpers.Check)
                .AddClass(ItemCheckClass)
                .SetPickingMode(PickingMode.Ignore);

            var count = new Label()
                .AddClass(ItemCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var favorite = new Button()
                .AddClass(FavoriteToggleClass)
                .SetText(TypeSelectorHelpers.StarEmpty);

            var arrow = new Label("›")
                .AddClass(ItemArrowClass);

            var content = new VisualElement()
                .AddClass(ItemContentClass)
                .AddChild(icon)
                .AddChild(glyph)
                .AddChild(label)
                .AddChild(check)
                .AddChild(count)
                .AddChild(favorite)
                .AddChild(arrow);

            var row = new VisualElement()
                .AddClass(ItemClass)
                .AddChild(divider)
                .AddChild(content);

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

            element.userData = node;

            var isCurrent = IsCurrentValue(node);

            element.EnableClass(SectionTitleClass, isSectionTitle);
            element.EnableClass(ItemInSectionClass, !isSectionTitle && node.SectionKey is not null);
            element.EnableClass(ItemCurrentModifier, isCurrent);

            element.EnableClass(RowAfterPinnedModifier, IsFirstRowAfterPinnedBlock(items, index));

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

        private bool IsCurrentValue(TreeNode node)
        {
            if (!_pages[^1].IsBase) return false;

            if (_currentAqn is null) return false;

            return _currentAqn.Length > 0
                ? node.IsType && node.AssemblyQualifiedName == _currentAqn
                : node.IsNoneOption;
        }

        private int TypeCountFor(TreeNode node)
        {
            if (Nav.IsSearching) return 0;
            return node.IsSectionTitle || node.HasChildren ? node.TypeCount : 0;
        }

        private bool IsFirstRowAfterPinnedBlock(List<TreeNode> items, int index)
        {
            if (!Nav.IsAtRoot || index <= 0 || index >= items.Count) return false;
            if (IsPinnedRow(items[index])) return false;

            return IsPinnedRow(items[index - 1]);
        }

        private static bool IsPinnedRow(TreeNode node) =>
            node.IsSectionTitle || node.SectionKey is not null || node.IsNoneOption;

        private void BindLeading(Image icon, Label glyph, TreeNode node, bool isSelected)
        {
            if (node.IsNoneOption)
            {
                icon.SetDisplay(DisplayStyle.None);
                glyph.SetText(TypeSelectorHelpers.None).SetDisplay(DisplayStyle.Flex);
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
                    if (node.IsType)
                    {
                        texture = TypeSelectorIconResolver.ResolveForType(node.AssemblyQualifiedName);
                    }
                    else
                    {
                        var fallback = node.HasChildren
                            ? (isSelected ? ContainerOpenFallbackIcon : ContainerFallbackIcon)
                            : null;
                        texture = TypeSelectorIconResolver.Resolve(fallback);
                    }
                }
            }

            icon
                .EnableClass(ItemIconCollapsedModifier, sectionCollapsed)
                .SetImage(texture)
                .SetDisplay(texture is not null ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private static string SectionIcon(string sectionKey, bool collapsed)
        {
            if (sectionKey == NavigationController.RecentSection)
                return collapsed ? RecentCollapsedIcon : RecentExpandedIcon;

            return collapsed ? FavoritesCollapsedIcon : FavoritesExpandedIcon;
        }

        private void BindFavorite(Button favorite, TreeNode node)
        {
            favorite.clickable = new Clickable(() => ToggleFavorite(node));

            if (!node.IsType)
            {
                favorite.SetDisplay(DisplayStyle.None);
                return;
            }

            var isFavorite = TypeSelectorPreferences.IsFavorite(node.AssemblyQualifiedName);

            favorite
                .SetDisplay(DisplayStyle.Flex)
                .SetText(isFavorite ? TypeSelectorHelpers.StarFilled : TypeSelectorHelpers.StarEmpty)
                .EnableClass(FavoriteToggleOnModifier, isFavorite);
        }

        private void OnRowClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not VisualElement row) return;
            if (row.userData is not TreeNode node || !node.IsSectionTitle) return;

            ToggleSectionKeepSelection(node);
            evt.StopPropagation();
        }

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
