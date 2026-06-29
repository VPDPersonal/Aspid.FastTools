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
    /// <summary>
    /// The hierarchical type selector as a host-agnostic <see cref="VisualElement"/>: search, keyboard
    /// navigation, namespace drill-down and the generic-argument resolution flow all live here.
    /// <see cref="TypeSelectorWindow"/> hosts it as a dropdown; embedding hosts (e.g. the Repair References
    /// window) add it inline and collapse it through the dismiss callback.
    /// </summary>
    /// <remarks>
    /// Selecting an open generic definition (injected via <see cref="TypeSelectorFilter.AdditionalTypes"/>) is not a final selection;
    /// instead it drills into an argument-selection flow — one hierarchical page per type parameter,
    /// reusing the same search/keyboard/navigation — and emits the constructed closed type once every argument
    /// is resolved. The argument flow stays dormant unless open generics are present, so the ordinary
    /// type-selection contract is unchanged.
    /// </remarks>
    internal sealed class TypeSelectorView : VisualElement
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-TypeSelector";

        // The static skeleton lives in a UXML cloned in BuildUI. It keeps a distinct base name from the stylesheet so
        // AddStyleSheetsFromResource's Resources.Load<StyleSheet> on StyleSheetPath stays unambiguous (a same-named
        // VisualTreeAsset would shadow the StyleSheet). The code keeps only the classes it toggles/queries at runtime;
        // the skeleton's own classes live in the UXML.
        private const string UxmlResourcePath = "UI/Types/Aspid-FastTools-TypeSelector-View";

        private const string BlockClass = "aspid-fasttools-type-selector";
        private const string HeaderClass = BlockClass + "__header";
        private const string HeaderSearchFocusedModifier = HeaderClass + "--search-focused";
        private const string CrumbClass = BlockClass + "__breadcrumb";
        private const string CrumbCurrentModifier = CrumbClass + "--current";
        private const string CrumbLinkModifier = CrumbClass + "--link";
        private const string CrumbSeparatorClass = BlockClass + "__breadcrumb-separator";
        private const string ItemClass = BlockClass + "__item";
        private const string ItemInSectionClass = ItemClass + "--in-section";
        private const string ItemIconClass = BlockClass + "__item-icon";
        private const string ItemGlyphClass = BlockClass + "__item-glyph";
        private const string ItemTitleClass = BlockClass + "__item-title";
        private const string ItemArrowClass = BlockClass + "__item-arrow";
        private const string SectionTitleClass = BlockClass + "__section-title";
        private const string FavoriteToggleClass = BlockClass + "__favorite-toggle";
        private const string FavoriteToggleOnModifier = FavoriteToggleClass + "--favorite-on";
        private const string ItemIconCollapsedModifier = ItemIconClass + "--collapsed";

        private const string StarFilledGlyph = "★";
        private const string StarEmptyGlyph = "☆";
        private const string NoneGlyph = "○";

        private const string TypeFallbackIcon = "d_cs Script Icon";
        private const string ContainerFallbackIcon = "d_Folder Icon";
        private const string ContainerOpenFallbackIcon = "d_FolderOpened Icon";
        private const string FavoritesCollapsedIcon = "d_Favorite";
        private const string FavoritesExpandedIcon = "d_Favorite Icon";
        private const string RecentCollapsedIcon = "d_UnityEditor.HistoryWindow";
        private const string RecentExpandedIcon = "d_UnityEditor.HistoryWindow";

        private const string HeaderName = "type-selector-header";
        private const string BreadcrumbBarName = "type-selector-breadcrumb-bar";
        private const string SearchButtonName = "type-selector-search-button";
        private const string SearchFieldName = "type-selector-search-field";
        private const string ErrorName = "type-selector-error";
        private const string ListName = "type-selector-list";
        private const string EmptyHintName = "type-selector-empty-hint";
        private const string FooterHintName = "type-selector-footer-hint";

        private VisualElement _header;
        private VisualElement _breadcrumbBar;
        private Button _searchButton;
        private ListView _listView;
        private Label _errorLabel;
        private Label _emptyHint;
        private Label _footerHint;
        private ToolbarSearchField _searchField;

        private bool _searchFieldFocused;
        private bool _searchChromeOpen;

        // Space toggles a favorite, but it is also a navigation submit key — so the same press raises a NavigationSubmit
        // that would choose (and close on) the row. This arms the submit suppressor for that one event.
        private bool _suppressNextSubmit;

        private readonly List<PickerPage> _pages = new();

        private readonly Action _onDismiss;
        private readonly Action<string> _onSelected;
        private readonly Func<Type, bool> _argumentFilter;
        private readonly Type[] _fieldTypes;
        private readonly string _currentAqn;

        private NavigationController Nav => _pages[^1].Navigation;

        /// <summary>
        /// Creates a type selector view.
        /// </summary>
        /// <param name="filter">Defines which types the selector offers: base types, kind constraints, the per-type predicate, extra entries and the open-generic argument predicate. See <see cref="TypeSelectorFilter"/>.</param>
        /// <param name="currentAqn">Assembly-qualified name of the currently selected type, used to pre-navigate to that type's location. Pass <c>null</c> or empty to start at the root.</param>
        /// <param name="onSelected">Callback invoked with the assembly-qualified name of the selected type, or <c>null</c> if the user chose <c>&lt;None&gt;</c>. When an open generic is resolved, the assembly-qualified name of the constructed closed type is passed.</param>
        /// <param name="onDismiss">Invoked when the selector is done — after a selection is emitted, or when the user cancels with Escape. The host closes its window or collapses the inline panel here.</param>
        public TypeSelectorView(
            TypeSelectorFilter filter = default,
            string currentAqn = "",
            Action<string> onSelected = null,
            Action onDismiss = null)
        {
            var types = filter.Types ?? new[] { typeof(object) };

            _onDismiss = onDismiss;
            _onSelected = onSelected;
            _argumentFilter = filter.ArgumentFilter;
            _currentAqn = currentAqn ?? string.Empty;
            _fieldTypes = types;

            BuildUI();

            var hierarchy = HierarchyBuilder.Build(types, filter.Allow, filter.Predicate, filter.AdditionalTypes);

            var navigation = new NavigationController(hierarchy, composeSections: true);

            if (!string.IsNullOrWhiteSpace(_currentAqn))
                navigation.NavigateToAssemblyQualifiedName(_currentAqn);

            _pages.Add(new PickerPage
            {
                Navigation = navigation,
                TitlePrefix = null,
                ConstraintType = types.Length > 0 ? types[0] : typeof(object),
                OnPicked = closed => Emit(closed?.AssemblyQualifiedName),
                IsBase = true,
            });

            RefreshView();
        }

        /// <summary>
        /// Gives the picker keyboard focus so the arrow keys navigate and any printable key starts a search (the search
        /// field stays collapsed until then). Call after the view is attached to a panel.
        /// </summary>
        public void FocusPicker()
        {
            if (Nav.CurrentItems.Count > 0)
            {
                _listView.Focus();

                if (_listView.selectedIndex < 0)
                {
                    var first = FindSelectableIndex(0, step: 1);
                    if (first >= 0) SetSelectedIndex(first);
                }

                return;
            }

            Focus();
        }

        #region Initialization
        private void BuildUI()
        {
            focusable = true;

            this.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(BlockClass);

            Resources.Load<VisualTreeAsset>(UxmlResourcePath).CloneTree(this);

            _header = this.Q<VisualElement>(HeaderName);
            _breadcrumbBar = this.Q<VisualElement>(BreadcrumbBarName);
            _searchButton = this.Q<Button>(SearchButtonName);
            _searchField = this.Q<ToolbarSearchField>(SearchFieldName);
            _errorLabel = this.Q<Label>(ErrorName);
            _listView = this.Q<ListView>(ListName);
            _emptyHint = this.Q<Label>(EmptyHintName);
            _footerHint = this.Q<Label>(FooterHintName);

            _searchButton.clicked += () => OpenSearch();
            _breadcrumbBar.RegisterCallback<ClickEvent>(_ => OpenSearch());

            WireSearchField();
            WireListView();

            UpdateSearchChrome();

            RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);

            // The ListView drives its own selection from NavigationMoveEvent — a separate event from the KeyDownEvent
            // handled above. Left to fire it would advance the selection a second time on top of ours and skip a row,
            // so the directional moves are swallowed here and our KeyDown handler stays the single arrow navigator.
            RegisterCallback<NavigationMoveEvent>(SuppressDirectionalNavigation, TrickleDown.TrickleDown);

            RegisterCallback<NavigationSubmitEvent>(SuppressFavoriteSubmit, TrickleDown.TrickleDown);

            RegisterCallback<FocusInEvent>(_ => UpdateFooterHint());
        }

        private void WireSearchField()
        {
            _searchField.RegisterValueChangedCallback(e => HandleSearchChanged(e.newValue ?? string.Empty));

            _searchField.RegisterCallback<FocusInEvent>(_ =>
            {
                _searchFieldFocused = true;
                _header.EnableInClass(HeaderSearchFocusedModifier, true);

                _listView.ClearSelection();

                UpdateSearchChrome();
                UpdateFooterHint();
            });

            _searchField.RegisterCallback<FocusOutEvent>(evt =>
            {
                // Focus moving within the field (text input ↔ its clear button) is not a real blur — keep it open.
                if (evt.relatedTarget is VisualElement next && IsDescendantOf(next, _searchField)) return;

                _searchFieldFocused = false;
                _header.EnableInClass(HeaderSearchFocusedModifier, false);
                UpdateSearchChrome();
                UpdateFooterHint();
            });
        }

        private void WireListView()
        {
            _listView.SetMakeItem(CreateListItem);
            _listView.SetBindItem(BindListItem);
            _listView.itemsChosen += HandleItemChosen;

            // Re-bind the visible rows on every selection change so the selected folder can swap to its opened icon
            // (selection only toggles a USS class otherwise; the leading image is set in code, not USS).
            _listView.selectedIndicesChanged += _ =>
            {
                _listView.RefreshItems();
                UpdateFooterHint();
            };
        }

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

            var favorite = new Button()
                .AddClass(FavoriteToggleClass)
                .SetText(StarEmptyGlyph);

            var arrow = new Label("›")
                .AddClass(ItemArrowClass);

            var row = new VisualElement()
                .AddClass(ItemClass)
                .AddChild(icon)
                .AddChild(glyph)
                .AddChild(label)
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

            element.EnableInClass(SectionTitleClass, isSectionTitle);
            element.EnableInClass(ItemInSectionClass, !isSectionTitle && node.SectionKey is not null);

            element.SetPickingMode(PickingMode.Position);

            element.Q<Label>(className: ItemTitleClass)
                .SetText(node.DisplayName)
                .SetTooltip(node.Tooltip);

            BindLeading(element.Q<Image>(className: ItemIconClass), element.Q<Label>(className: ItemGlyphClass), node, index == _listView.selectedIndex);
            BindFavorite(element.Q<Button>(className: FavoriteToggleClass), node);

            element.Q<Label>(className: ItemArrowClass)
                .SetDisplay(node.HasChildren && !Nav.IsSearching
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);
        }

        private void BindLeading(Image icon, Label glyph, TreeNode node, bool isSelected)
        {
            if (node.DisplayName == TypeSelectorHelpers.NoneOption)
            {
                icon.SetDisplay(DisplayStyle.None);
                glyph.SetText(NoneGlyph).SetDisplay(DisplayStyle.Flex);
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
                .SetText(isFavorite ? StarFilledGlyph : StarEmptyGlyph)
                .EnableInClass(FavoriteToggleOnModifier, isFavorite);
        }
        #endregion

        #region Section rows
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

        private TreeNode SelectedNode()
        {
            var items = Nav.CurrentItems;
            var index = _listView.selectedIndex;
            return index >= 0 && index < items.Count ? items[index] : null;
        }
        #endregion

        #region Search chrome
        private void UpdateSearchChrome()
        {
            _searchChromeOpen = _searchFieldFocused || !string.IsNullOrEmpty(_searchField.value);

            _breadcrumbBar.SetDisplay(_searchChromeOpen ? DisplayStyle.None : DisplayStyle.Flex);
            _searchButton.SetDisplay(_searchChromeOpen ? DisplayStyle.None : DisplayStyle.Flex);
            _searchField.SetDisplay(_searchChromeOpen ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OpenSearch(string initialText = null)
        {
            _searchChromeOpen = true;
            _breadcrumbBar.SetDisplay(DisplayStyle.None);
            _searchButton.SetDisplay(DisplayStyle.None);
            _searchField.SetDisplay(DisplayStyle.Flex);

            // Seeding the value works immediately, but the field was just un-hidden — its display only resolves on the
            // next layout pass, and a display:none element silently refuses Focus(). Defer the focus (and caret-to-end)
            // to that pass so the field reliably takes focus and any further typing lands in it.
            if (!string.IsNullOrEmpty(initialText))
                _searchField.value = initialText;

            _searchField.schedule.Execute(() =>
            {
                // The deferred tick can land after the embedding host collapsed the panel; bail if we are detached.
                if (_searchField.panel is null) return;

                _searchField.Focus();
                TryMoveSearchCursorToEnd();
            });

            UpdateFooterHint();
        }

        private void CollapseSearch()
        {
            ResetSearchField();
            FocusPicker();
        }
        #endregion

        #region KeyDown Handlers
        private static void SuppressDirectionalNavigation(NavigationMoveEvent evt)
        {
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                case NavigationMoveEvent.Direction.Down:
                case NavigationMoveEvent.Direction.Left:
                case NavigationMoveEvent.Direction.Right:
                    evt.StopPropagation();
                    break;
            }
        }

        private void SuppressFavoriteSubmit(NavigationSubmitEvent evt)
        {
            if (!_suppressNextSubmit) return;

            _suppressNextSubmit = false;
            evt.StopPropagation();
        }

        private void HandleKeyDown(KeyDownEvent evt)
        {
            // Any real key press other than the favorite Space (and other than the paired character event, keyCode None)
            // cancels a pending favorite-submit suppression, so a later Enter still chooses normally.
            if (evt.keyCode != KeyCode.Space && evt.keyCode != KeyCode.None)
                _suppressNextSubmit = false;

            if (!_searchChromeOpen && IsTypingCharacter(evt))
            {
                OpenSearch(evt.character.ToString());
                evt.StopPropagation();
                return;
            }

            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    if (HandleUpArrow()) evt.StopPropagation();
                    break;

                case KeyCode.DownArrow:
                    if (HandleDownArrow()) evt.StopPropagation();
                    break;

                case KeyCode.Escape:
                    HandleEscapeKey();
                    evt.StopPropagation();
                    break;

                case KeyCode.Space:
                    if (!IsSearchFocused(focusController?.focusedElement) && HandleToggleFavoriteKey())
                    {
                        // Disarm the submit this same Space raises, so the row toggles favorite without being chosen.
                        _suppressNextSubmit = true;
                        evt.StopPropagation();
                    }
                    break;

                case KeyCode.RightArrow:
                    if (!IsSearchFocused(focusController?.focusedElement) && HandleRightArrow())
                        evt.StopPropagation();
                    break;

                case KeyCode.LeftArrow:
                    if (!IsSearchFocused(focusController?.focusedElement) && HandleLeftArrow())
                        evt.StopPropagation();
                    break;
            }
        }

        // A KeyDownEvent that represents a printable character typed without a command/control/alt chord. Unity raises a
        // separate character event (keyCode == None, character set) alongside the keyCode event, so keying off the
        // character keeps arrows/Esc/Enter — which carry no printable character — on the navigation path below.
        private static bool IsTypingCharacter(KeyDownEvent evt)
        {
            var c = evt.character;

            if (c == '\0' || char.IsControl(c) || char.IsWhiteSpace(c)) return false;
            if (evt.ctrlKey || evt.commandKey || evt.altKey) return false;

            return true;
        }

        private bool HandleUpArrow()
        {
            var focused = focusController?.focusedElement;

            if (IsSearchFocused(focused))
                return TryMoveSearchCursorToStart();

            if (!IsListFocused(focused)) return false;

            var target = FindSelectableIndex(_listView.selectedIndex - 1, step: -1);

            if (target < 0)
            {
                OpenSearch();
                return true;
            }

            SetSelectedIndex(target);
            return true;
        }

        private bool HandleDownArrow()
        {
            var focused = focusController?.focusedElement;

            if (IsSearchFocused(focused))
            {
                if (TryMoveSearchCursorToEnd()) return true;
                if (Nav.CurrentItems is not { Count: > 0 }) return false;

                _listView.Focus();

                var first = FindSelectableIndex(0, step: 1);
                if (first >= 0) SetSelectedIndex(first);

                return true;
            }

            if (!IsListFocused(focused)) return false;

            var target = FindSelectableIndex(_listView.selectedIndex + 1, step: 1);
            if (target >= 0) SetSelectedIndex(target);

            return true;
        }

        private bool IsSearchFocused(Focusable focused) =>
            focused == _searchField || IsDescendantOf(focused as VisualElement, _searchField);

        private bool IsListFocused(Focusable focused) =>
            focused == _listView || IsDescendantOf(focused as VisualElement, _listView);

        private int FindSelectableIndex(int start, int step)
        {
            var items = _pages.Count > 0 ? Nav.CurrentItems : null;
            if (items is null) return -1;

            for (var i = start; i >= 0 && i < items.Count; i += step)
                if (items[i].IsSelectable || items[i].HasChildren || items[i].IsSectionTitle)
                    return i;

            return -1;
        }

        private void SetSelectedIndex(int index)
        {
            _listView.selectedIndex = index;
            _listView.ScrollToItem(index);
        }

        private bool TryMoveSearchCursorToStart()
        {
            var input = _searchField.Q<TextField>();
            if (input is null) return false;

            if (input.cursorIndex == 0 && input.selectIndex == 0) return false;

            input.SelectRange(0, 0);
            return true;
        }

        private bool TryMoveSearchCursorToEnd()
        {
            var input = _searchField.Q<TextField>();
            if (input is null) return false;

            var length = input.value?.Length ?? 0;
            if (input.cursorIndex == length && input.selectIndex == length) return false;

            input.SelectRange(length, length);
            return true;
        }

        private static bool IsDescendantOf(VisualElement element, VisualElement ancestor)
        {
            for (var current = element; current is not null; current = current.parent)
                if (current == ancestor) return true;

            return false;
        }

        private void HandleEscapeKey()
        {
            // A three-step ladder: the first Esc clears a running query and keeps the field open and focused for a retype
            // (re-asserted here so the rung behaves the same whether focus sits in the field or has moved to the results);
            // a second (empty field) collapses the header back to the trail; a third (trail showing) dismisses the picker.
            if (!string.IsNullOrEmpty(_searchField.value))
            {
                _searchField.value = string.Empty;
                OpenSearch();
                return;
            }

            if (_searchChromeOpen)
            {
                CollapseSearch();
                return;
            }

            _onDismiss?.Invoke();
        }

        private bool HandleRightArrow()
        {
            var node = SelectedNode();
            if (node is null) return false;

            if (node.IsSectionTitle)
            {
                if (!Nav.IsSectionCollapsed(node.SectionKey)) return false;
                ToggleSectionKeepSelection(node);
                return true;
            }

            if (node.HasChildren && !Nav.IsSearching)
            {
                NavigateInto(node);
                return true;
            }

            return false;
        }

        private bool HandleLeftArrow()
        {
            if (SelectedNode() is { IsSectionTitle: true } section)
            {
                if (Nav.IsSectionCollapsed(section.SectionKey)) return false;
                ToggleSectionKeepSelection(section);
                return true;
            }

            if (!Nav.CanNavigateBack && _pages.Count <= 1) return false;

            NavigateBack();
            return true;
        }

        private bool HandleToggleFavoriteKey()
        {
            if (SelectedNode() is not { IsType: true } node) return false;

            ToggleFavorite(node);

            // Favoriting at the root recomposes the list (the Favorites section moves), so keep the highlight on the row
            // the user acted on where it survives the rebuild.
            var index = Nav.CurrentItems.IndexOf(node);
            if (index >= 0) SetSelectedIndex(index);

            return true;
        }

        private void HandleSearchChanged(string query)
        {
            Nav.ApplySearch(query);
            UpdateSearchChrome();
            RefreshView();
        }

        private void HandleItemChosen(IEnumerable<object> items)
        {
            var node = items.OfType<TreeNode>().FirstOrDefault();

            if (node is not null)
                ActivateNode(node);
        }
        #endregion

        #region Navigation
        private void ActivateNode(TreeNode node)
        {
            if (node.IsSectionTitle) ToggleSectionKeepSelection(node);
            else if (node.HasChildren && !Nav.IsSearching) NavigateInto(node);
            else if (node.IsSelectable) SelectNode(node);
        }

        private void NavigateInto(TreeNode node)
        {
            HideError();
            Nav.NavigateInto(node);
            RefreshView();

            _listView.selectedIndex = 0;
            _listView.ScrollToItem(0);
        }

        private void NavigateBack()
        {
            HideError();

            if (Nav.CanNavigateBack)
            {
                var previousNode = Nav.NavigateBack();
                RefreshView();

                var index = Nav.CurrentItems.IndexOf(previousNode);
                _listView.selectedIndex = index >= 0 ? index : 0;
                _listView.ScrollToItem(_listView.selectedIndex);
                return;
            }

            if (_pages.Count > 1)
                PopPage();
        }

        private void SelectNode(TreeNode node)
        {
            HideError();

            var page = _pages[^1];
            var aqn = node.AssemblyQualifiedName;
            var type = string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false);

            // An open generic definition is not a final selection — drill into its argument flow instead.
            if (type is { IsGenericTypeDefinition: true })
            {
                var validationFieldTypes = page.IsBase ? _fieldTypes : new[] { page.ConstraintType };
                BeginResolveGeneric(type, page.ConstraintType, validationFieldTypes, page.OnPicked);
                return;
            }

            if (page.IsBase)
            {
                Emit(aqn);
                return;
            }

            if (type is not null)
                page.OnPicked(type);
        }

        private void Emit(string assemblyQualifiedName)
        {
            TypeSelectorPreferences.RecordRecent(assemblyQualifiedName);

            _onSelected?.Invoke(assemblyQualifiedName);
            _onDismiss?.Invoke();
        }

        private void ToggleFavorite(TreeNode node)
        {
            if (!node.IsType) return;

            TypeSelectorPreferences.ToggleFavorite(node.AssemblyQualifiedName);

            Nav.RefreshFavoritesSection();

            // On the root page the recomposed section must be re-rendered; on a search/namespace page only the row's
            // own star glyph changed, so a lighter item refresh suffices.
            if (Nav.IsAtRoot) RefreshView();
            else _listView.RefreshItems();
        }
        #endregion

        #region Generic argument resolution
        private void BeginResolveGeneric(Type openDefinition, Type primaryFieldType, Type[] validationFieldTypes, Action<Type> onClosed)
        {
            // A closed-generic field already fixes the arguments — skip the picker and construct directly.
            if (GenericTypeResolver.TryInferFromFieldType(primaryFieldType, openDefinition, out var inferred))
            {
                onClosed(inferred);
                return;
            }

            PickParam(openDefinition, validationFieldTypes, Array.Empty<Type>(), _pages.Count, onClosed);
        }

        private void PickParam(Type openDefinition, Type[] validationFieldTypes, Type[] argsSoFar, int startDepth, Action<Type> onClosed)
        {
            var parameters = openDefinition.GetGenericArguments();

            if (argsSoFar.Length == parameters.Length)
            {
                if (GenericTypeResolver.TryConstruct(openDefinition, argsSoFar, validationFieldTypes, out var closed, out var error))
                {
                    PopToDepth(startDepth);
                    onClosed(closed);
                }
                else
                {
                    ShowError(error);
                }

                return;
            }

            var index = argsSoFar.Length;
            var parameter = parameters[index];

            var page = BuildParamPage(openDefinition, argsSoFar, index, parameter, picked =>
                PickParam(openDefinition, validationFieldTypes, Append(argsSoFar, picked), startDepth, onClosed));

            PushPage(page);
        }

        private PickerPage BuildParamPage(Type openDefinition, Type[] argsSoFar, int index, Type parameter, Action<Type> onPicked)
        {
            var baseTypes = GenericTypeResolver.GetConstraintBaseTypes(parameter);
            var constraintType = baseTypes.Length == 1 ? baseTypes[0] : typeof(object);

            Func<Type, bool> filter = candidate =>
                GenericTypeResolver.SatisfiesSpecialConstraints(parameter, candidate) &&
                (_argumentFilter?.Invoke(candidate) ?? true);

            // Offer open generic definitions as arguments too, so the user can nest generics (e.g. choose
            // Modifier<T> for T) — picking one resolves its own arguments before it is used here. Pass every
            // constraint base type (not just the collapsed single one) so a multi-constraint parameter narrows
            // the nested definitions by all of them up front, instead of offering defs that fail every later pick.
            var nested = GenericTypeResolver.GetAssignableGenericDefinitions(baseTypes[0], baseTypes);
            var hierarchy = HierarchyBuilder.Build(baseTypes, TypeAllow.None, filter, nested, includeNoneOption: false);

            return new PickerPage
            {
                Navigation = new NavigationController(hierarchy),
                TitlePrefix = $"{FormatBuilding(openDefinition, argsSoFar, index)}  ▸  {parameter.Name}",
                ConstraintType = constraintType,
                OnPicked = onPicked,
                IsBase = false,
            };
        }

        private void PushPage(PickerPage page)
        {
            _pages.Add(page);
            ResetSearchField();
            UpdateSearchChrome();
            RefreshView();
            SelectFirstItem();
            FocusPicker();
        }

        private void PopPage()
        {
            if (_pages.Count <= 1) return;

            _pages.RemoveAt(_pages.Count - 1);
            HideError();
            ResetSearchField();
            UpdateSearchChrome();
            RefreshView();
            SelectFirstItem();
            FocusPicker();
        }

        private void PopToDepth(int depth)
        {
            while (_pages.Count > depth)
                _pages.RemoveAt(_pages.Count - 1);
        }

        private void ResetSearchField()
        {
            _searchField.SetValueWithoutNotify(string.Empty);
            Nav.ApplySearch(string.Empty);
        }

        private void SelectFirstItem()
        {
            _listView.selectedIndex = Nav.CurrentItems.Count > 0 ? 0 : -1;
            _listView.ScrollToItem(0);
        }

        private static string FormatBuilding(Type openDefinition, Type[] argsSoFar, int currentIndex)
        {
            var parameters = openDefinition.GetGenericArguments();

            var baseName = TypeExtensions.StripArity(openDefinition.Name);

            var parts = new string[parameters.Length];
            for (var k = 0; k < parameters.Length; k++)
            {
                if (k < argsSoFar.Length) parts[k] = TypeSelectorHelpers.GetTypeSelectorTitle(argsSoFar[k]);
                else if (k == currentIndex) parts[k] = "?";
                else parts[k] = parameters[k].Name;
            }

            return $"{baseName}<{string.Join(", ", parts)}>";
        }

        private static Type[] Append(Type[] array, Type value)
        {
            var result = new Type[array.Length + 1];
            Array.Copy(array, result, array.Length);
            result[^1] = value;
            return result;
        }
        #endregion

        #region View
        private void RefreshView()
        {
            RebuildBreadcrumbs();

            var items = Nav.CurrentItems;
            _listView.itemsSource = items;
            _listView.Rebuild();

            var hasItems = items.Count > 0;
            _listView.SetDisplay(hasItems ? DisplayStyle.Flex : DisplayStyle.None);
            _emptyHint.text = hasItems ? string.Empty : BuildEmptyHintText();
            _emptyHint.SetDisplay(hasItems ? DisplayStyle.None : DisplayStyle.Flex);

            UpdateFooterHint();
        }

        private string BuildEmptyHintText() =>
            Nav.IsSearching
                ? $"No types match '{_searchField.value}'."
                : "Nothing here.";

        private void UpdateFooterHint()
        {
            if (_footerHint is null) return;

            var searchFocused = IsSearchFocused(focusController?.focusedElement);

            var parts = new List<string> { "↑↓ Navigate" };
            var node = SelectedNode();

            if (!searchFocused && node is { IsSectionTitle: true })
                parts.Add(Nav.IsSectionCollapsed(node.SectionKey) ? "→ Expand" : "← Collapse");
            else if (!searchFocused && node is { HasChildren: true } && !Nav.IsSearching)
                parts.Add("→ Open");
            else if (node is { IsSelectable: true })
                parts.Add("Enter Select");

            if (!searchFocused && node is { IsType: true })
                parts.Add(TypeSelectorPreferences.IsFavorite(node.AssemblyQualifiedName)
                    ? StarFilledGlyph + " Space Unfavorite"
                    : StarEmptyGlyph + " Space Favorite");

            if (Nav.IsSearching)
            {
                parts.Add("Esc Clear");
            }
            else if (_searchChromeOpen)
            {
                parts.Add("Esc Cancel");
            }
            else
            {
                if (!searchFocused && (Nav.CanNavigateBack || _pages.Count > 1)) parts.Add("← Back");

                parts.Add("Type to search");
                parts.Add("Esc Close");
            }

            _footerHint.text = string.Join("  ·  ", parts);
        }

        private void RebuildBreadcrumbs()
        {
            _breadcrumbBar.Clear();

            if (Nav.IsSearching)
            {
                AddCrumb("Search", isCurrent: true, action: null);
                return;
            }

            var page = _pages[^1];

            if (page.IsBase && !Nav.CanNavigateBack)
            {
                AddCrumb("Select Type", isCurrent: true, action: null);
                return;
            }

            var crumbs = new List<(string Label, Action Action, bool IsCurrent)>();
            var atContextRoot = !Nav.CanNavigateBack;

            if (page.IsBase)
            {
                crumbs.Add(("Types", () => JumpToDepth(0), false));
            }
            else
            {
                // Generic-argument context (e.g. "Modifier<?>"): from the page's own root it pops back to the previous
                // page; once drilled into a namespace it jumps back to this page's root listing instead. It reads as
                // the current location only while nothing deeper is open.
                crumbs.Add((page.TitlePrefix, atContextRoot ? PopPage : () => JumpToDepth(0), atContextRoot));
            }

            // Real ancestors live at Breadcrumbs[1..] (index 0 is the hidden "/" root).
            var trail = Nav.Breadcrumbs;
            for (var i = 1; i < trail.Count; i++)
            {
                var depth = i;
                crumbs.Add((trail[i].DisplayName, () => JumpToDepth(depth), false));
            }

            if (Nav.CanNavigateBack)
                crumbs.Add((Nav.CurrentNode.DisplayName, null, true));

            for (var i = 0; i < crumbs.Count; i++)
            {
                AddCrumb(crumbs[i].Label, crumbs[i].IsCurrent, crumbs[i].Action);
                if (i < crumbs.Count - 1) AddSeparator();
            }
        }

        private void AddCrumb(string text, bool isCurrent, Action action)
        {
            var crumb = new Label(text)
                .AddClass(CrumbClass)
                .EnableInClass(CrumbCurrentModifier, isCurrent)
                .SetTooltip(text);

            // Clickability tracks the action, not the current flag: the generic-context crumb is both "here" and an
            // escape hatch, so it can be the current crumb yet still navigate.
            if (action is not null)
            {
                crumb.AddClass(CrumbLinkModifier);
                crumb.RegisterCallback<ClickEvent>(evt =>
                {
                    action();

                    // Keep the click off the breadcrumb bar's open-search handler — a navigable crumb navigates only.
                    evt.StopPropagation();
                });
            }

            _breadcrumbBar.AddChild(crumb);
        }

        private void AddSeparator() =>
            _breadcrumbBar.AddChild(new Label("›")
                .AddClass(CrumbSeparatorClass)
                .SetPickingMode(PickingMode.Ignore));

        private void JumpToDepth(int keep)
        {
            HideError();
            Nav.NavigateToDepth(keep);
            RefreshView();
            SelectFirstItem();
        }

        private void ShowError(string message)
        {
            if (_errorLabel is null) return;

            _errorLabel.text = message;
            _errorLabel.style.display = DisplayStyle.Flex;
        }

        private void HideError()
        {
            if (_errorLabel is not null)
                _errorLabel.style.display = DisplayStyle.None;
        }
        #endregion

        private sealed class PickerPage
        {
            public NavigationController Navigation;
            public string TitlePrefix;
            public Type ConstraintType;
            public Action<Type> OnPicked;
            public bool IsBase;
        }
    }
}
