using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// Editor window that displays a hierarchical type selector dropdown, allowing the user to browse and select a <see cref="System.Type"/> from a filtered list.
    /// </summary>
    public sealed class TypeSelectorWindow : EditorWindow
    {
        private const string NoneOption = "<None>";
        private const string GlobalNamespace = "<Global>";
        
        private const string StylesheetPath = "Styles/Aspid-FastTools-TypeSelectorWindow";
        private const string ContainerClass = "aspid-fasttools-type-selector-container";
        private const string HeaderClass = "aspid-fasttools-type-selector-header";

        private const string ItemClass = "aspid-fasttools-type-selector-item";
        private const string ItemTitleClass = "aspid-fasttools-type-selector-item-title";
        private const string ItemArrowClass = "aspid-fasttools-type-selector-item-arrow";
        
        private Label _titleLabel;
        private Button _backButton;
        private ListView _listView;
        private ToolbarSearchField _searchField;
        private NavigationController _navigation;
        
        private Action<string> _onSelected;
        private string _currentAqn = string.Empty;
        
        /// <summary>
        /// Opens the type selector window as a dropdown anchored to <paramref name="screenRect"/>.
        /// </summary>
        /// <param name="screenRect">The screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="types">Base types used to filter which concrete types are shown. Only types assignable to all entries are listed.</param>
        /// <param name="currentAqn">Assembly-qualified name of the currently selected type, used to pre-navigate to that type's location. Pass <c>null</c> or empty to start at the root.</param>
        /// <param name="allowAbstract">Whether abstract types are included in the list. Defaults to <c>false</c>.</param>
        /// <param name="allowInterface">Whether interface types are included in the list. Defaults to <c>false</c>.</param>
        /// <param name="onSelected">Callback invoked with the assembly-qualified name of the selected type, or <c>null</c> if the user chose <c>&lt;None&gt;</c>.</param>
        public static void Show(
            Rect screenRect,
            Type[] types = null,
            string currentAqn = "",
            bool allowAbstract = false,
            bool allowInterface = false,
            Action<string> onSelected = null)
        {
            var window = CreateInstance<TypeSelectorWindow>();
            window.Initialize(
                screenRect,
                types,
                currentAqn,
                allowAbstract,
                allowInterface,
                onSelected);
        }

        #region Initialization
        private void Initialize(
            Rect screenRect, 
            Type[] types,
            string currentAqn, 
            bool allowAbstract,
            bool allowInterface,
            Action<string> onSelected)
        {
            _onSelected = onSelected;
            _currentAqn = currentAqn ?? string.Empty;

            BuildUI();

            var hierarchy = HierarchyBuilder.Build(types, allowAbstract, allowInterface);
            InitializeNavigation(hierarchy, _currentAqn);
            
            RefreshView();
            
            var size = new Vector2(Mathf.Max(350, screenRect.width), 320);
            ShowAsDropDown(screenRect, size);
            
            _searchField.Focus();
        }

        private void BuildUI()
        {
            _searchField = CreateSearchField();
            _listView = CreateListView();
                
            rootVisualElement
                .AddStyleSheetsFromResource(StylesheetPath)
                .AddClass(ContainerClass)
                .AddChild(CreateHeader())
                .AddChild(_searchField)
                .AddChild(_listView);
                
            rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
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
                field.RegisterCallback<NavigationMoveEvent>(e =>
                {
                    if (e.move == Vector2.down)
                        _listView?.Focus();
                }, TrickleDown.TrickleDown);

                return field;
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
                    .AddClass(ItemClass)
                    .AddChild(label)
                    .AddChild(arrow);
            }

            void BindListItem(VisualElement element, int index)
            {
                var items = _navigation?.CurrentItems;
                
                if (items is null) return;
                if (index < 0 || index >= items.Count) return;

                var node = items[index];
                element.Q<Label>(className: ItemTitleClass)
                    .SetText(node.DisplayName)
                    .SetTooltip(node.Tooltip);

                element.Q<Label>(className: ItemArrowClass)
                    .SetDisplay(node.HasChildren && !_navigation.IsSearching
                        ? DisplayStyle.Flex
                        : DisplayStyle.None);
            }
        }
        
        private void InitializeNavigation(TreeNode hierarchy, string currentAqn)
        {
            _navigation = new NavigationController(hierarchy);
            
            if (!string.IsNullOrWhiteSpace(currentAqn))
                _navigation.NavigateToAssemblyQualifiedName(currentAqn);
        }
        #endregion
        
        #region KeyDown Hadnlers
        private void HandleKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    if (_listView.selectedIndex is 0)
                        _searchField.Focus();
                    
                    evt.StopPropagation();
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

        private void HandleEscapeKey()
        {
            if (_navigation.IsSearching && !string.IsNullOrWhiteSpace(_searchField.value))
                _searchField.value = string.Empty;
            else Close();
        }

        private void HandleRightArrow()
        {
            var items = _navigation.CurrentItems;
            var index = _listView.selectedIndex;
            
            if (index >= 0 && index < items.Count && items[index].HasChildren)
                NavigateInto(items[index]);
        }

        private void HandleSearchChanged(string query)
        {
            _navigation.ApplySearch(query);
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
            if (node.HasChildren && !_navigation.IsSearching) NavigateInto(node);
            else if (node.IsSelectable) SelectNode(node);
        }

        private void NavigateInto(TreeNode node)
        {
            _navigation.NavigateInto(node);
            RefreshView();

            _listView.selectedIndex = 0;
            _listView.ScrollToItem(0);
        }

        private void NavigateBack()
        {
            if (!_navigation.CanNavigateBack) return;

            var previousNode = _navigation.NavigateBack();
            RefreshView();

            var index = _navigation.CurrentItems.IndexOf(previousNode);
            _listView.selectedIndex = index >= 0 ? index : 0;
            _listView.ScrollToItem(_listView.selectedIndex);
        }

        private void SelectNode(TreeNode node)
        {
            _onSelected?.Invoke(node.AssemblyQualifiedName);
            Close();
        }
        #endregion
        
        private void RefreshView()
        {
            _titleLabel.text = _navigation.GetCurrentTitle();
            _backButton.SetEnabled(_navigation.CanNavigateBack);
            
            _listView.itemsSource = _navigation.CurrentItems;
            _listView.Rebuild();
        }
        
        private static class HierarchyBuilder
        {
            public static TreeNode Build(Type[] types, bool allowAbstract, bool allowInterface)
            {
                // TODO Aspid.FastTools – Get base type from attribute.
                var allTypes = TypeInfoScanner.GetAllTypeInfos(types, allowAbstract, allowInterface);
                
                var root = new TreeNode("/");
                root.Children.Add(new TreeNode(NoneOption, null, NoneOption));

                AddGlobalNamespaceGroup(root, allTypes);
                AddNamespaceHierarchy(root, allTypes);

                return root;
            }

            private static void AddGlobalNamespaceGroup(TreeNode root, List<TypeInfo> types)
            {
                var globals = types
                    .Where(type => type.Namespace == GlobalNamespace)
                    .OrderBy(type => type.Name)
                    .ToList();

                if (globals.Count is 0) return;
                var globalGroup = new TreeNode(GlobalNamespace);
                
                AddTypesWithDisambiguation(globalGroup, globals, includeNamespace: false);
                root.Children.Add(globalGroup);
            }

            private static void AddNamespaceHierarchy(TreeNode root, List<TypeInfo> types)
            {
                var namespacedTypes = types
                    .Where(type => type.Namespace != GlobalNamespace)
                    .ToList();

                var trie = BuildNamespaceTrie(namespacedTypes);
                CompressNamespaceTrie(trie);

                var nsToTypes = namespacedTypes
                    .GroupBy(type => type.Namespace)
                    .ToDictionary(group => group.Key, group => group.ToList());

                foreach (var child in trie.Children.Values.OrderBy(n => n.Segment))
                {
                    var node = BuildNamespaceNode(child, string.Empty, string.Empty, nsToTypes);
                    root.Children.Add(node);
                }
            }

            private static NamespaceNode BuildNamespaceTrie(List<TypeInfo> types)
            {
                var root = new NamespaceNode(string.Empty);

                foreach (var type in types)
                {
                    var current = root;
                    
                    foreach (var segment in type.Namespace.Split('.'))
                        current = current.GetOrCreateChild(segment);
                    
                    current.IsTerminal = true;
                }

                return root;
            }

            private static TreeNode BuildNamespaceNode(
                NamespaceNode trieNode,
                string displayPrefix,
                string fullNamespace,
                Dictionary<string, List<TypeInfo>> nsToTypes)
            {
                var nextDisplay = string.IsNullOrEmpty(displayPrefix) 
                    ? trieNode.Segment 
                    : $"{displayPrefix}.{trieNode.Segment}";
                
                var nextNamespace = string.IsNullOrEmpty(fullNamespace)
                    ? trieNode.Segment
                    : $"{fullNamespace}.{trieNode.Segment}";

                var node = new TreeNode(trieNode.Segment, null, nextDisplay);

                // Add types at this namespace level
                if (trieNode.IsTerminal && nsToTypes.TryGetValue(nextNamespace, out var types))
                {
                    AddTypesWithDisambiguation(node, types, includeNamespace: true, nextNamespace);
                }

                // Add child namespaces
                foreach (var child in trieNode.Children.Values.OrderBy(n => n.Segment))
                {
                    node.Children.Add(BuildNamespaceNode(child, nextDisplay, nextNamespace, nsToTypes));
                }

                // Flatten single-child chains
                return FlattenSingleChildChain(node);
            }

            private static TreeNode FlattenSingleChildChain(TreeNode node)
            {
                if (node.Children.Count != 1) return node;

                var onlyChild = node.Children[0];
                
                if (onlyChild.AssemblyQualifiedName == null)
                {
                    node.DisplayName = $"{node.DisplayName}.{onlyChild.DisplayName}";
                    node.Caption = $"{node.Caption}.{onlyChild.Caption}";
                    node.Children.Clear();
                    node.Children.AddRange(onlyChild.Children);
                }
                else
                {
                    node.DisplayName = $"{node.DisplayName}.{onlyChild.DisplayName}";
                    node.AssemblyQualifiedName = onlyChild.AssemblyQualifiedName;
                    node.Caption = onlyChild.Caption;
                    node.Tooltip = onlyChild.Tooltip;
                    node.Children.Clear();
                }

                return node;
            }

            private static void AddTypesWithDisambiguation(
                TreeNode parent,
                List<TypeInfo> types,
                bool includeNamespace,
                string namespacePath = "")
            {
                var nameCounts = types
                    .GroupBy(type => type.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var type in types.OrderBy(type => type.Name))
                {
                    var needsAssembly = nameCounts[type.Name] > 1;
                    var displayName = needsAssembly ? $"{type.Name} ({type.Assembly})" : type.Name;
                    
                    var caption = includeNamespace
                        ? $"{namespacePath}.{displayName}"
                        : displayName;

                    var leaf = new TreeNode(displayName, type.AssemblyQualifiedName, caption)
                    {
                        Tooltip = type.FullName
                    };
                    
                    parent.Children.Add(leaf);
                }
            }

            private static void CompressNamespaceTrie(NamespaceNode node)
            {
                // Recursively compress children first
                foreach (var child in node.Children.Values.ToList())
                {
                    CompressNamespaceTrie(child);
                }

                // Compress chains at this level
                foreach (var key in node.Children.Keys.ToList())
                {
                    if (!node.Children.TryGetValue(key, out var child)) continue;

                    // Merge non-terminal nodes with single child
                    while (!child.IsTerminal && child.Children.Count == 1)
                    {
                        var grandchild = child.Children.Values.First();
                        child.Segment = $"{child.Segment}.{grandchild.Segment}";
                        child.IsTerminal = grandchild.IsTerminal;
                        child.Children.Clear();
                        
                        foreach (var kv in grandchild.Children)
                            child.Children[kv.Key] = kv.Value;
                    }

                    // Update dictionary if segment changed
                    if (child.Segment != key)
                    {
                        node.Children.Remove(key);
                        node.Children[child.Segment] = child;
                    }
                }
            }
        }

        private class NamespaceNode
        {
            public string Segment { get; set; }
            
            public bool IsTerminal { get; set; }
            
            public Dictionary<string, NamespaceNode> Children { get; }

            public NamespaceNode(string segment)
            {
                Segment = segment;
                Children = new Dictionary<string, NamespaceNode>(StringComparer.Ordinal);
            }

            public NamespaceNode GetOrCreateChild(string segment)
            {
                if (!Children.TryGetValue(segment, out var child))
                {
                    child = new NamespaceNode(segment);
                    Children[segment] = child;
                }
                
                return child;
            }
        }
        
        private sealed class TypeInfo
        {
            public readonly string Name;
            public readonly string Assembly;
            public readonly string FullName;
            public readonly string Namespace;
            public readonly string AssemblyQualifiedName;
            
            public TypeInfo(Type type)
            {
                Name = type.Name;
                FullName = type.FullName;
                Assembly = type.Assembly.GetName().Name;
                AssemblyQualifiedName = type.AssemblyQualifiedName;
                Namespace = string.IsNullOrEmpty(type.Namespace) ? GlobalNamespace : type.Namespace;
            }
        }
        
        private static class TypeInfoScanner
        {
            public static List<TypeInfo> GetAllTypeInfos(Type[] baseTypes, bool allowAbstract, bool allowInterface)
            {
                var result = new List<TypeInfo>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        result.AddRange(assembly.GetTypes()
                            .Where(t => baseTypes.All(baseType => baseType.IsAssignableFrom(t)) &&
                                !t.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                                !t.Name.Contains("<") &&
                                !t.Name.Contains(">") &&
                                (allowAbstract || !t.IsAbstract) &&
                                (allowInterface || !t.IsInterface))
                            .Select(type => new TypeInfo(type)));
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return result;
            }
        }
        
        private class TreeNode
        {
            public string Caption { get; set; }
            
            public string Tooltip { get; set; }
            
            public List<TreeNode> Children { get; }
            
            public string DisplayName { get; set; }
            
            public string AssemblyQualifiedName { get; set; }

            public bool HasChildren => Children.Count > 0;
            
            public bool IsSelectable => AssemblyQualifiedName is not null || DisplayName == NoneOption;

            public TreeNode(string displayName, string assemblyQualifiedName = null, string caption = null)
            {
                DisplayName = displayName;
                AssemblyQualifiedName = assemblyQualifiedName;
                Caption = caption ?? displayName;
                Tooltip = string.Empty;
                Children = new List<TreeNode>();
            }

            public bool MatchesFilter(string filter)
            {
                if (string.IsNullOrWhiteSpace(filter))
                    return true;

                if (DisplayName?.ToLowerInvariant().Contains(filter) == true)
                    return true;
                
                if (Caption?.ToLowerInvariant().Contains(filter) == true)
                    return true;
                
                if (AssemblyQualifiedName?.ToLowerInvariant().Contains(filter) == true)
                    return true;

                return false;
            }
        }
        
        private sealed class NavigationController
        {
            private TreeNode _currentNode;
            private readonly TreeNode _rootNode;
            
            private readonly List<TreeNode> _breadcrumbs = new();
            private readonly List<TreeNode> _searchResults = new();

            public bool IsSearching { get; private set; }
            
            public bool CanNavigateBack =>
                _breadcrumbs.Count > 0;
            
            public List<TreeNode> CurrentItems =>
                IsSearching ? _searchResults : _currentNode.Children;

            public NavigationController(TreeNode root)
            {
                _rootNode = root;
                _currentNode = root;
                _breadcrumbs.Clear();
            }

            public string GetCurrentTitle()
            {
                if (IsSearching) return "Search";
                if (_breadcrumbs.Count is 0) return "Select Type";

                var parts = _breadcrumbs
                    .Select(node => node.DisplayName)
                    .Append(_currentNode.DisplayName)
                    .Where(node => node is not "/")
                    .ToList();

                return string.Join("/", parts);
            }
            
            public void ApplySearch(string query)
            {
                IsSearching = !string.IsNullOrWhiteSpace(query);
                
                if (IsSearching)
                {
                    _searchResults.Clear();
                    var filter = query?.Trim().ToLowerInvariant();
                    
                    foreach (var node in EnumerateLeaves(_rootNode))
                    {
                        if (node.MatchesFilter(filter))
                            _searchResults.Add(new TreeNode(
                                displayName: node.Caption, 
                                node.AssemblyQualifiedName, 
                                node.Caption));
                    }
                }
            }

            public void NavigateInto(TreeNode node)
            {
                _breadcrumbs.Add(_currentNode);
                _currentNode = node;
            }

            public TreeNode NavigateBack()
            {
                if (!CanNavigateBack) return null;

                var previousNode = _currentNode;
                _currentNode = _breadcrumbs[^1];
                _breadcrumbs.RemoveAt(_breadcrumbs.Count - 1);
                return previousNode;
            }

            public void NavigateToAssemblyQualifiedName(string aqn)
            {
                var path = new List<TreeNode>();
                if (!FindPathToAssemblyQualifiedName(_rootNode, aqn, path) || path.Count < 2) return;
                
                // Navigate to the parent of the target
                for (var i = path.Count - 2; i >= 1; i--)
                {
                    _breadcrumbs.Add(_currentNode);
                    _currentNode = path[i];
                }
            }

            private static IEnumerable<TreeNode> EnumerateLeaves(TreeNode node)
            {
                if (!node.HasChildren && node.AssemblyQualifiedName is not null)
                {
                    yield return node;
                }
                else
                {
                    foreach (var leaf in node.Children.SelectMany(EnumerateLeaves))
                    {
                        yield return leaf;
                    }
                }
            }
            
            private static bool FindPathToAssemblyQualifiedName(TreeNode node, string assemblyQualifiedName, List<TreeNode> path)
            {
                if (node.AssemblyQualifiedName == assemblyQualifiedName
                    || node.Children.Any(child => FindPathToAssemblyQualifiedName(child, assemblyQualifiedName, path)))
                {
                    path.Add(node);
                    return true;
                }

                return false;
            }
        }
    }
}