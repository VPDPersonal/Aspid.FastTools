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
    /// Selecting an open generic definition (injected via <c>additionalTypes</c>) is not a final selection;
    /// instead it drills into an argument-selection flow — one hierarchical page per type parameter,
    /// reusing the same search/keyboard/navigation — and emits the constructed closed type once every argument
    /// is resolved. The argument flow stays dormant unless open generics are present, so the ordinary
    /// type-selection contract is unchanged.
    /// </remarks>
    internal sealed class TypeSelectorView : VisualElement
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-TypeSelector";

        private const string BlockClass = "aspid-fasttools-type-selector";
        private const string HeaderClass = BlockClass + "__header";
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
        private const string ErrorClass = BlockClass + "__error";
        private const string EmptyHintClass = BlockClass + "__empty-hint";
        private const string FooterHintClass = BlockClass + "__footer-hint";
        private const string FavoriteToggleClass = BlockClass + "__favorite-toggle";
        private const string FavoriteToggleOnModifier = FavoriteToggleClass + "--favorite-on";

        private const string StarFilledGlyph = "★";
        private const string StarEmptyGlyph = "☆";

        // A type leaf with no explicit [TypeSelectorItem] icon falls back to the C# script glyph; a namespace /
        // category container falls back to a folder glyph, so every row carries an icon and the list reads as a rhythm.
        private const string TypeFallbackIcon = "d_cs Script Icon";
        private const string ContainerFallbackIcon = "d_Folder Icon";

        // <None> draws a crisp hollow-circle text glyph in the leading slot — it is vertically symmetric, so it centres
        // cleanly on the caption.
        private const string NoneGlyph = "○";

        // Section headers use Unity's native foldout-arrow image (collapsed points right, expanded points down) in the
        // icon slot. It is the solid triangle look that fit best, and being an image like the type/folder icons it
        // centres exactly on the caption — where the text-glyph triangle rode off because its ink sits skewed in the
        // font's em box (unlike the symmetric circle). Both arrows are light grey, so they read on the dark surface.
        private const string SectionCollapsedIcon = "IN foldout";
        private const string SectionExpandedIcon = "IN foldout on";

        private VisualElement _breadcrumbBar;
        private ListView _listView;
        private Label _errorLabel;
        private Label _emptyHint;
        private Label _footerHint;
        private ToolbarSearchField _searchField;

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
        /// <param name="types">Base types used to filter which concrete types are shown. Only types assignable to all entries are listed.</param>
        /// <param name="currentAqn">Assembly-qualified name of the currently selected type, used to pre-navigate to that type's location. Pass <c>null</c> or empty to start at the root.</param>
        /// <param name="allow">Which type kinds are included in the list.</param>
        /// <param name="onSelected">Callback invoked with the assembly-qualified name of the selected type, or <c>null</c> if the user chose <c>&lt;None&gt;</c>. When an open generic is resolved, the assembly-qualified name of the constructed closed type is passed.</param>
        /// <param name="filter">Optional predicate applied to each candidate type after the base-type and <paramref name="allow"/> checks. Return <c>false</c> to hide a type. Pass <c>null</c> to keep every matching type.</param>
        /// <param name="additionalTypes">Optional extra types appended to the list verbatim, bypassing the base-type and <paramref name="allow"/> checks — used to inject entries the assignability scan cannot match, such as open generic definitions.</param>
        /// <param name="argumentFilter">Optional predicate applied to candidate types offered for an open generic's type arguments (in addition to the parameter's own constraints). Pass <c>null</c> to accept any constraint-satisfying type.</param>
        /// <param name="onDismiss">Invoked when the selector is done — after a selection is emitted, or when the user cancels with Escape. The host closes its window or collapses the inline panel here.</param>
        public TypeSelectorView(
            Type[] types = null,
            string currentAqn = "",
            TypeAllow allow = TypeAllow.None,
            Action<string> onSelected = null,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            Func<Type, bool> argumentFilter = null,
            Action onDismiss = null)
        {
            types ??= new[] { typeof(object) };

            _onDismiss = onDismiss;
            _onSelected = onSelected;
            _argumentFilter = argumentFilter;
            _currentAqn = currentAqn ?? string.Empty;
            _fieldTypes = types;

            BuildUI();

            var hierarchy = HierarchyBuilder.Build(types, allow, filter, additionalTypes);

            // Only the base page composes the Favorites/Recents sections; generic-argument pages do not.
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
        /// Moves keyboard focus into the search field. Call after the view is attached to a panel.
        /// </summary>
        public void FocusSearch() => _searchField.Focus();

        #region Initialization
        private void BuildUI()
        {
            _searchField = CreateSearchField();
            _listView = CreateListView();
            _errorLabel = CreateErrorLabel();
            _emptyHint = CreateEmptyHint();

            this.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(BlockClass)
                .AddChild(CreateHeader())
                .AddChild(_searchField)
                .AddChild(_errorLabel)
                .AddChild(_listView)
                .AddChild(_emptyHint)
                .AddChild(CreateFooterHint());

            RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);

            // The ListView drives its own selection from NavigationMoveEvent — a separate event from the KeyDownEvent
            // handled above. Left to fire it would advance the selection a second time on top of ours and skip a row,
            // so the directional moves are swallowed here and our KeyDown handler stays the single arrow navigator.
            RegisterCallback<NavigationMoveEvent>(SuppressDirectionalNavigation, TrickleDown.TrickleDown);

            // The footer hint drops its directional affordances while the search field holds focus, so re-render it
            // whenever focus moves between the search field and the list.
            RegisterCallback<FocusInEvent>(_ => UpdateFooterHint());
            return;

            // The breadcrumb trail is rebuilt per refresh (RebuildBreadcrumbs); the bar is just its container.
            VisualElement CreateHeader() =>
                _breadcrumbBar = new VisualElement().AddClass(HeaderClass);

            Label CreateEmptyHint()
            {
                var label = new Label(string.Empty).AddClass(EmptyHintClass);
                label.style.display = DisplayStyle.None;
                return label;
            }

            // Text is filled per refresh by UpdateFooterHint, which adapts the hint to the current selection.
            Label CreateFooterHint() =>
                _footerHint = new Label(string.Empty)
                    .AddClass(FooterHintClass)
                    .SetPickingMode(PickingMode.Ignore);

            ToolbarSearchField CreateSearchField()
            {
                var field = new ToolbarSearchField();

                field.RegisterValueChangedCallback(e => HandleSearchChanged(e.newValue ?? string.Empty));

                return field;
            }

            Label CreateErrorLabel()
            {
                var label = new Label(string.Empty).AddClass(ErrorClass);

                // Visibility is toggled in code (ShowError / HideError); the error palette and spacing live in USS.
                label.style.display = DisplayStyle.None;

                return label;
            }

            ListView CreateListView()
            {
                var list = new ListView
                {
                    selectionType = SelectionType.Single,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                };

                list.SetMakeItem(CreateListItem);
                list.SetBindItem(BindListItem);
                list.itemsChosen += HandleItemChosen;
                list.selectedIndicesChanged += _ => UpdateFooterHint();

                return list;
            }

            VisualElement CreateListItem()
            {
                var icon = new Image()
                    .AddClass(ItemIconClass)
                    .SetPickingMode(PickingMode.Ignore);

                // Text-glyph alternative to the image icon, occupying the same leading slot (shown for <None> / section
                // headers, hidden when an image icon is shown).
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

                // Clicking a section header toggles its collapse; the bound node rides on userData (set per rebind).
                // Normal rows fall through to the ListView's own selection / choose handling untouched.
                row.RegisterCallback<ClickEvent>(OnRowClicked);

                return row;
            }

            void BindListItem(VisualElement element, int index)
            {
                var items = _pages.Count > 0 ? Nav.CurrentItems : null;

                if (items is null) return;
                if (index < 0 || index >= items.Count) return;

                var node = items[index];
                var isSectionTitle = node.IsSectionTitle;

                // OnRowClicked reads this to know which node it operates on after row recycling.
                element.userData = node;

                element.EnableInClass(SectionTitleClass, isSectionTitle);
                // Favorites/Recents item rows take the indented grouping style.
                element.EnableInClass(ItemInSectionClass, !isSectionTitle && node.SectionKey is not null);

                // Every row is pickable: type/namespace rows pick or drill, and section headers are navigable too —
                // selecting one highlights the group and the collapse toggle runs on click / Right-Left / Enter.
                element.SetPickingMode(PickingMode.Position);

                // The section's collapse chevron now rides in the leading glyph slot, so the caption is just the name.
                element.Q<Label>(className: ItemTitleClass)
                    .SetText(node.DisplayName)
                    .SetTooltip(node.Tooltip);

                BindLeading(element.Q<Image>(className: ItemIconClass), element.Q<Label>(className: ItemGlyphClass), node);
                BindFavorite(element.Q<Button>(className: FavoriteToggleClass), node);

                element.Q<Label>(className: ItemArrowClass)
                    .SetDisplay(node.HasChildren && !Nav.IsSearching
                        ? DisplayStyle.Flex
                        : DisplayStyle.None);
            }

            // Fills the leading slot: a crisp text glyph for the special rows (<None> circle, section chevron) or the
            // image icon for everything else. The two are mutually exclusive — whichever is used, the other is hidden —
            // so the slot stays a uniform width across the list.
            void BindLeading(Image icon, Label glyph, TreeNode node)
            {
                // <None> fills the leading slot with a crisp text-glyph circle; every other row uses the image icon
                // slot. The two are mutually exclusive so the leading column stays a uniform width across the list.
                if (node.DisplayName == TypeSelectorHelpers.NoneOption)
                {
                    icon.SetDisplay(DisplayStyle.None);
                    glyph.SetText(NoneGlyph).SetDisplay(DisplayStyle.Flex);
                    return;
                }

                glyph.SetDisplay(DisplayStyle.None);

                // Section headers show the native foldout arrow (collapsed/expanded); types/containers their explicit
                // [TypeSelectorItem] icon or a uniform fallback. All go through the image slot so the column aligns.
                var texture = node.IsSectionTitle
                    ? TypeSelectorIconResolver.Resolve(Nav.IsSectionCollapsed(node.SectionKey) ? SectionCollapsedIcon : SectionExpandedIcon)
                    : TypeSelectorIconResolver.Resolve(node.Icon) ?? TypeSelectorIconResolver.Resolve(FallbackIconName(node));

                icon
                    .SetImage(texture)
                    .SetDisplay(texture is not null ? DisplayStyle.Flex : DisplayStyle.None);
            }

            static string FallbackIconName(TreeNode node)
            {
                if (node.IsType) return TypeFallbackIcon;
                return node.HasChildren ? ContainerFallbackIcon : null;
            }

            void BindFavorite(Button favorite, TreeNode node)
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
        }
        #endregion

        #region Section rows
        // A click on a section header (Favorites/Recents) toggles its collapsed state and keeps the header selected so
        // it stays highlighted; every other row falls through untouched to the ListView's own selection/choose handling.
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

        // The TreeNode under the current list selection, or null when nothing is selected / out of range.
        private TreeNode SelectedNode()
        {
            var items = Nav.CurrentItems;
            var index = _listView.selectedIndex;
            return index >= 0 && index < items.Count ? items[index] : null;
        }
        #endregion

        #region KeyDown Handlers
        // Swallows the directional NavigationMoveEvents the ListView would otherwise act on, so our KeyDown handler is
        // the only thing that moves the selection (Tab's Next/Previous is left alone for focus traversal).
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

        private void HandleKeyDown(KeyDownEvent evt)
        {
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

        private bool HandleUpArrow()
        {
            var focused = focusController?.focusedElement;

            // In the search field, Up only walks the caret to the line start; there is nothing above the field to
            // focus now that the breadcrumb trail is mouse-only.
            if (IsSearchFocused(focused))
                return TryMoveSearchCursorToStart();

            if (!IsListFocused(focused)) return false;

            // Step up one row; jump to the search field when nothing precedes.
            var target = FindSelectableIndex(_listView.selectedIndex - 1, step: -1);

            if (target < 0)
            {
                _searchField.Focus();
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

                // Land on the first navigable row.
                var first = FindSelectableIndex(0, step: 1);
                if (first >= 0) SetSelectedIndex(first);

                return true;
            }

            if (!IsListFocused(focused)) return false;

            // Step down one row.
            var target = FindSelectableIndex(_listView.selectedIndex + 1, step: 1);
            if (target >= 0) SetSelectedIndex(target);

            return true;
        }

        private bool IsSearchFocused(Focusable focused) =>
            focused == _searchField || IsDescendantOf(focused as VisualElement, _searchField);

        private bool IsListFocused(Focusable focused) =>
            focused == _listView || IsDescendantOf(focused as VisualElement, _listView);

        /// <summary>
        /// Returns the nearest navigable item index starting at <paramref name="start"/> and walking by
        /// <paramref name="step"/>. Every visible row is navigable — type leaves, namespace/category containers and
        /// collapsible section headers alike — so the arrow keys step one row at a time without skipping. Returns
        /// <c>-1</c> when none is found.
        /// </summary>
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
            if (Nav.IsSearching && !string.IsNullOrWhiteSpace(_searchField.value))
                _searchField.value = string.Empty;
            else _onDismiss?.Invoke();
        }

        private bool HandleRightArrow()
        {
            var node = SelectedNode();
            if (node is null) return false;

            // On a collapsed section header Right expands it; on a namespace/category container it drills in.
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
            // On an expanded section header Left collapses it instead of navigating up; everywhere else Left walks back
            // up the hierarchy (or pops a generic-argument page).
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

        private void HandleSearchChanged(string query)
        {
            Nav.ApplySearch(query);
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
            // Enter / double-click on a section header toggles it (the same as clicking it or the Right/Left keys).
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

            // Preserve the raw assembly-qualified-name contract for non-generic base selections
            // (concrete type, <None> → empty, or an unresolved/missing name).
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
            // Single choke point for every finalized pick: concrete base selections and constructed
            // closed generics both flow through here, so the recorded recent is always the closed type.
            TypeSelectorPreferences.RecordRecent(assemblyQualifiedName);

            _onSelected?.Invoke(assemblyQualifiedName);
            _onDismiss?.Invoke();
        }

        private void ToggleFavorite(TreeNode node)
        {
            if (!node.IsType) return;

            TypeSelectorPreferences.ToggleFavorite(node.AssemblyQualifiedName);

            // Re-compose the root page's Favorites section regardless of which page the star was toggled on, so it is
            // up to date once the user navigates back to root. A no-op for non-composing (generic-argument) controllers.
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
            // Modifier<T> for T) — picking one resolves its own arguments before it is used here.
            var nested = GenericTypeResolver.GetAssignableGenericDefinitions(constraintType);
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
            RefreshView();
            SelectFirstItem();
            _searchField.Focus();
        }

        private void PopPage()
        {
            if (_pages.Count <= 1) return;

            _pages.RemoveAt(_pages.Count - 1);
            HideError();
            ResetSearchField();
            RefreshView();
            SelectFirstItem();
            _searchField.Focus();
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

            var name = openDefinition.Name;
            var tick = name.IndexOf('`');
            var baseName = tick >= 0 ? name[..tick] : name;

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

            // An empty list (most often a search miss) swaps the ListView for a centred hint so the picker never
            // shows a blank void.
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

        // The footer hint mirrors what the current selection affords, so the available keys are never a guess: ↑↓
        // navigation is always shown; the action key (Open / Expand / Collapse / Select) and the back/close keys adapt
        // to the selected row and the navigation state.
        private void UpdateFooterHint()
        {
            if (_footerHint is null) return;

            // When the search field holds focus the arrow keys drive the caret, so the directional ← Back / → Open
            // affordances do not apply and are dropped — only the always-true keys are shown.
            var searchFocused = IsSearchFocused(focusController?.focusedElement);

            var parts = new List<string> { "↑↓ Navigate" };
            var node = SelectedNode();

            if (!searchFocused && node is { IsSectionTitle: true })
                parts.Add(Nav.IsSectionCollapsed(node.SectionKey) ? "→ Expand" : "← Collapse");
            else if (!searchFocused && node is { HasChildren: true } && !Nav.IsSearching)
                parts.Add("→ Open");
            else if (node is { IsSelectable: true })
                parts.Add("Enter Select");

            if (Nav.IsSearching)
            {
                parts.Add("Esc Clear");
            }
            else
            {
                if (!searchFocused && (Nav.CanNavigateBack || _pages.Count > 1)) parts.Add("← Back");
                parts.Add("Esc Close");
            }

            _footerHint.text = string.Join("  ·  ", parts);
        }

        // Rebuilds the clickable breadcrumb trail in the header. It mirrors the navigation path: a root crumb, one
        // crumb per opened ancestor, then the current level as a bright, non-clickable tail. Generic-argument pages
        // prepend a context crumb (the partially-built type) that escapes back to where the user opened it.
        private void RebuildBreadcrumbs()
        {
            _breadcrumbBar.Clear();

            // While searching the trail is moot — the list is a flat result set — so show a single quiet marker.
            if (Nav.IsSearching)
            {
                AddCrumb("Search", isCurrent: true, action: null);
                return;
            }

            var page = _pages[^1];

            // Base page sitting at the root: render a plain title rather than a one-item trail.
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
                // Full text on the tooltip recovers any crumb that ellipsised to fit the header strip.
                .SetTooltip(text);

            // Clickability tracks the action, not the current flag: the generic-context crumb is both "here" and an
            // escape hatch, so it can be the current crumb yet still navigate.
            if (action is not null)
            {
                crumb.AddClass(CrumbLinkModifier);
                crumb.RegisterCallback<ClickEvent>(_ => action());
            }

            _breadcrumbBar.AddChild(crumb);
        }

        private void AddSeparator() =>
            _breadcrumbBar.AddChild(new Label("›")
                .AddClass(CrumbSeparatorClass)
                .SetPickingMode(PickingMode.Ignore));

        // Jumps the breadcrumb trail up to a given depth (0 = root) and resets the list selection to the top.
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

        /// <summary>
        /// One step of the selector: a navigable candidate hierarchy plus what to do when a concrete type is
        /// chosen on it. The base page emits the selection through <see cref="_onSelected"/>; argument pages
        /// record the chosen type and advance to the next parameter (or construct the closed type).
        /// </summary>
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
