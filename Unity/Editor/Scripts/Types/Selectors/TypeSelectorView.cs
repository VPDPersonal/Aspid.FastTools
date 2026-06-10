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
        private const string ItemTitleClass = BlockClass + "__item-title";
        private const string ItemArrowClass = BlockClass + "__item-arrow";

        private Label _titleLabel;
        private Button _backButton;
        private ListView _listView;
        private Label _errorLabel;
        private ToolbarSearchField _searchField;

        private readonly List<PickerPage> _pages = new();

        private readonly Action _onDismiss;
        private readonly Action<string> _onSelected;
        private readonly Func<Type, bool> _argumentFilter;
        private readonly Type[] _fieldTypes;
        private readonly string _currentAqn;

        private NavigationController Nav => _pages[^1].Navigation;
        private bool CanGoBack => Nav.CanNavigateBack || _pages.Count > 1;

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
            var navigation = new NavigationController(hierarchy);

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

            this.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(BlockClass)
                .AddChild(CreateHeader())
                .AddChild(_searchField)
                .AddChild(_errorLabel)
                .AddChild(_listView);

            RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
            return;

            VisualElement CreateHeader()
            {
                _titleLabel = new Label(string.Empty);
                _backButton = new Button(NavigateBack).SetText("←");

                return new VisualElement()
                    .AddClass(HeaderClass)
                    .AddChild(_backButton)
                    .AddChild(_titleLabel);
            }

            ToolbarSearchField CreateSearchField()
            {
                var field = new ToolbarSearchField();

                field.RegisterValueChangedCallback(e => HandleSearchChanged(e.newValue ?? string.Empty));

                return field;
            }

            Label CreateErrorLabel()
            {
                var label = new Label(string.Empty);

                label.style.display = DisplayStyle.None;
                label.style.color = new Color(0.9f, 0.35f, 0.35f);
                label.style.whiteSpace = WhiteSpace.Normal;
                label.style.marginLeft = 4;
                label.style.marginRight = 4;
                label.style.marginTop = 2;
                label.style.marginBottom = 2;

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

                return list;
            }

            VisualElement CreateListItem()
            {
                var label = new Label()
                    .AddClass(ItemTitleClass);

                var arrow = new Label("›")
                    .AddClass(ItemArrowClass);

                return new VisualElement()
                    .AddChild(label)
                    .AddChild(arrow);
            }

            void BindListItem(VisualElement element, int index)
            {
                var items = _pages.Count > 0 ? Nav.CurrentItems : null;

                if (items is null) return;
                if (index < 0 || index >= items.Count) return;

                var node = items[index];
                element.Q<Label>(className: ItemTitleClass)
                    .SetText(node.DisplayName)
                    .SetTooltip(node.Tooltip);

                element.Q<Label>(className: ItemArrowClass)
                    .SetDisplay(node.HasChildren && !Nav.IsSearching
                        ? DisplayStyle.Flex
                        : DisplayStyle.None);
            }
        }
        #endregion

        #region KeyDown Handlers
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
                    HandleRightArrow();
                    evt.StopPropagation();
                    break;

                case KeyCode.LeftArrow:
                    if (_searchField.focusController.focusedElement != _searchField)
                    {
                        NavigateBack();
                        evt.StopPropagation();
                    }
                    break;
            }
        }

        private bool HandleUpArrow()
        {
            var focused = focusController?.focusedElement;

            if (IsSearchFocused(focused))
            {
                if (TryMoveSearchCursorToStart()) return true;
                if (!CanGoBack || Nav.IsSearching) return false;

                _backButton.Focus();
                return true;
            }

            if (_listView.selectedIndex is 0)
            {
                _searchField.Focus();
                return true;
            }

            return false;
        }

        private bool HandleDownArrow()
        {
            var focused = focusController?.focusedElement;

            if (focused == _backButton)
            {
                _searchField.Focus();
                return true;
            }

            if (IsSearchFocused(focused))
            {
                if (TryMoveSearchCursorToEnd()) return true;
                if (Nav.CurrentItems is not { Count: > 0 }) return false;

                _listView.Focus();
                return true;
            }

            return false;
        }

        private bool IsSearchFocused(Focusable focused) =>
            focused == _searchField || IsDescendantOf(focused as VisualElement, _searchField);

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

        private void HandleRightArrow()
        {
            var items = Nav.CurrentItems;
            var index = _listView.selectedIndex;

            if (index >= 0 && index < items.Count && items[index].HasChildren)
                NavigateInto(items[index]);
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
            if (node.HasChildren && !Nav.IsSearching) NavigateInto(node);
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
                var backButtonWasFocused = focusController?.focusedElement == _backButton;
                var previousNode = Nav.NavigateBack();
                RefreshView();

                var index = Nav.CurrentItems.IndexOf(previousNode);
                _listView.selectedIndex = index >= 0 ? index : 0;
                _listView.ScrollToItem(_listView.selectedIndex);

                if (backButtonWasFocused && !CanGoBack)
                    _searchField.Focus();
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
            _onSelected?.Invoke(assemblyQualifiedName);
            _onDismiss?.Invoke();
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
            _titleLabel.text = GetTitle();
            _backButton.SetEnabled(CanGoBack);
            _backButton.SetDisplay(Nav.IsSearching ? DisplayStyle.None : DisplayStyle.Flex);

            _listView.itemsSource = Nav.CurrentItems;
            _listView.Rebuild();
        }

        private string GetTitle()
        {
            var page = _pages[^1];

            if (Nav.IsSearching || page.IsBase) return Nav.GetCurrentTitle();

            return Nav.CanNavigateBack ? $"{page.TitlePrefix} / {Nav.GetCurrentTitle()}" : page.TitlePrefix;
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
