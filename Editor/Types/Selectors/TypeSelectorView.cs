using System;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // The hierarchical type selector as a host-agnostic element: search, keyboard navigation, namespace drill-down
    // and the generic-argument flow. TypeSelectorWindow hosts it as a dropdown; an embedding host adds it inline and
    // collapses it through the dismiss callback.
    //
    // Selecting an open generic definition is not a final selection: it drills into one hierarchical page per type
    // parameter, reusing the same search and navigation, and emits the constructed closed type once every argument
    // is resolved. That flow stays dormant unless open generics are present.
    //
    // Split across partial files: this one owns construction and shared state, .Rows binds list rows, .Input handles
    // the search chrome and keyboard, .Navigation drives drill-down and selection, .Generics hosts the argument
    // pages and .View renders breadcrumbs, the footer hint and errors.
    internal sealed partial class TypeSelectorView : VisualElement
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

        private const string HeaderName = "type-selector-header";
        private const string BreadcrumbBarName = "type-selector-breadcrumb-bar";
        private const string SearchButtonName = "type-selector-search-button";
        private const string SearchFieldName = "type-selector-search-field";
        private const string ErrorName = "type-selector-error";
        private const string ListName = "type-selector-list";
        private const string EmptyHintName = "type-selector-empty-hint";
        private const string FooterHintName = "type-selector-footer-hint";
        private const string SettingsButtonName = "type-selector-settings-button";

        private VisualElement _header;
        private VisualElement _breadcrumbBar;
        private Button _searchButton;
        private ListView _listView;
        private Label _errorLabel;
        private Label _emptyHint;
        private Label _footerHint;
        private Button _settingsButton;
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
        private readonly GenericArgumentFilter _inferredArgumentFilter;
        private readonly Type[] _fieldTypes;
        private readonly string _currentAqn;
        private readonly bool _includeHidden;

        private NavigationController Nav => _pages[^1].Navigation;

        // currentAqn pre-navigates to that type's location; empty starts at the root. onSelected receives the
        // selected type's assembly-qualified name — the constructed closed type for a resolved open generic — or
        // null for <None>. onDismiss fires once the selector is done, whether it emitted a selection or was
        // canceled, and is where the host closes its window or collapses its inline panel.
        internal TypeSelectorView(
            TypeSelectorFilter filter = default,
            string currentAqn = "",
            Action<string> onSelected = null,
            Action onDismiss = null)
        {
            var types = filter.Types ?? new[] { typeof(object) };

            _onDismiss = onDismiss;
            _onSelected = onSelected;
            _argumentFilter = filter.ArgumentFilter;
            _inferredArgumentFilter = filter.InferredArgumentFilter;
            // Null and "" mean DIFFERENT things and both flow through unchanged: null = the host has no current-value
            // concept at all (a list "+" append, a missing-type Fix, the bulk project picker), "" = the field exists
            // and currently holds <None>. Only the latter may put the current-value check on the <None> row.
            _currentAqn = currentAqn;
            _fieldTypes = types;
            _includeHidden = filter.IncludeHidden;

            BuildUI();

            var hierarchy = HierarchyBuilder.Build(types, filter.Allow, filter.Predicate, filter.AdditionalTypes,
                includeNoneOption: !filter.HideNoneOption, includeHidden: _includeHidden);
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
            PreselectCurrent();
        }

        // Highlights the current type's row on open, so an immediate Enter re-confirms the same value. A current
        // value of <None>, or one whose type is absent, selects the pinned <None> row instead, so Enter re-confirms
        // or clears rather than committing an arbitrary first row. Only a null current value — a host with no
        // current-value concept — leaves the selection empty and Enter inert.
        private void PreselectCurrent()
        {
            if (_currentAqn is null) return;

            var items = Nav.CurrentItems;

            if (!string.IsNullOrEmpty(_currentAqn))
            {
                for (var i = 0; i < items.Count; i++)
                {
                    if (items[i].IsType && items[i].AssemblyQualifiedName == _currentAqn)
                    {
                        _listView.selectedIndex = i;
                        return;
                    }
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].IsNoneOption)
                {
                    _listView.selectedIndex = i;
                    return;
                }
            }
        }

        // Gives the picker keyboard focus so the arrow keys navigate and any printable key starts a search (the search
        // field stays collapsed until then). Call after the view is attached to a panel.
        internal void FocusPicker()
        {
            if (Nav.CurrentItems.Count > 0)
            {
                // A just-shown ListView silently refuses Focus() until its display resolves on the next layout pass
                // (the same constraint OpenSearch documents for the search field), so defer the focus to that pass.
                // The current value's row is pre-selected in PreselectCurrent (no arbitrary first-row selection that an
                // immediate Enter could commit); scroll it into view once the list is laid out.
                _listView.schedule.Execute(() =>
                {
                    if (_listView.panel is null) return;

                    _listView.Focus();
                    if (_listView.selectedIndex >= 0) _listView.ScrollToItem(_listView.selectedIndex);
                });

                return;
            }

            Focus();
        }

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
            _settingsButton = this.Q<Button>(SettingsButtonName);

            _searchButton.clicked += () => OpenSearch();

            // Settings live outside the picker, so the selector is done: dismiss first (the dropdown host would lose
            // focus and close anyway; embedded hosts collapse), then land the user on the window's Settings tab.
            _settingsButton.clicked += () =>
            {
                _onDismiss?.Invoke();
                TabWindow.OpenSettings();
            };
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

        private TreeNode SelectedNode()
        {
            var items = Nav.CurrentItems;
            var index = _listView.selectedIndex;
            return index >= 0 && index < items.Count ? items[index] : null;
        }

        private int FindSelectableIndex(int start, int step)
        {
            var items = _pages.Count > 0 ? Nav.CurrentItems : null;
            if (items is null) return -1;

            for (var i = start; i >= 0 && i < items.Count; i += step)
            {
                if (items[i].IsSelectable || items[i].HasChildren || items[i].IsSectionTitle)
                    return i;
            }

            return -1;
        }

        private void SetSelectedIndex(int index)
        {
            _listView.selectedIndex = index;
            _listView.ScrollToItem(index);
        }

        private static bool IsDescendantOf(VisualElement element, VisualElement ancestor)
        {
            for (var current = element; current is not null; current = current.parent)
                if (current == ancestor) return true;

            return false;
        }
    }
}
