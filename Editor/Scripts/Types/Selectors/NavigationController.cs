using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed class NavigationController
    {
        internal const string FavoritesSection = "Favorites";
        internal const string RecentSection = "Recent";

        private TreeNode _currentNode;
        private readonly TreeNode _rootNode;
        private readonly bool _composeSections;

        private readonly List<TreeNode> _breadcrumbs = new();
        private readonly List<TreeNode> _searchResults = new();

        private readonly List<TreeNode> _rootItems = new();

        // Sections (by title) the user has collapsed; their item rows are hidden from _visibleRootItems until expanded.
        private readonly HashSet<string> _collapsedSections = new();

        private readonly List<TreeNode> _visibleRootItems = new();

        private readonly Dictionary<string, TreeNode> _typesByAqn = new();

        internal bool IsSearching { get; private set; }

        internal bool CanNavigateBack =>
            _breadcrumbs.Count > 0;

        // The Favorites and Recents sections are composed only on this page, and only when the controller was
        // created with section composition enabled.
        internal bool IsAtRoot =>
            !IsSearching && _breadcrumbs.Count is 0;

        internal IReadOnlyList<TreeNode> Breadcrumbs => _breadcrumbs;

        internal TreeNode CurrentNode => _currentNode;

        internal List<TreeNode> CurrentItems
        {
            get
            {
                if (IsSearching) return _searchResults;
                if (_composeSections && IsAtRoot) return _visibleRootItems;
                return _currentNode.Children;
            }
        }

        internal NavigationController(TreeNode root, bool composeSections = false)
        {
            _rootNode = root;
            _currentNode = root;
            _composeSections = composeSections;
            _breadcrumbs.Clear();

            if (!_composeSections) return;

            // Both open collapsed, so the root lands on the full type hierarchy. Seeding the keys here rather than
            // per section also collapses a section that appears only later, once its first entry is recorded, while
            // a user-driven expand survives, since the rebuild never re-adds them.
            _collapsedSections.Add(FavoritesSection);
            _collapsedSections.Add(RecentSection);

            IndexTypeLeaves(root);
            RebuildRootItems();
        }

        internal void ApplySearch(string query)
        {
            IsSearching = !string.IsNullOrWhiteSpace(query);

            if (IsSearching)
            {
                _searchResults.Clear();
                var filter = query.Trim();

                foreach (var node in EnumerateLeaves(_rootNode))
                {
                    if (node.MatchesFilter(filter))
                        _searchResults.Add(new TreeNode(
                            displayName: node.Caption,
                            node.AssemblyQualifiedName,
                            node.Caption)
                        {
                            Tooltip = node.Tooltip,
                            Icon = node.Icon,
                            SearchName = node.SearchName,
                        });
                }
            }
        }

        internal void NavigateInto(TreeNode node)
        {
            _breadcrumbs.Add(_currentNode);
            _currentNode = node;
        }

        // Pops breadcrumbs until keep remain, making that ancestor current — the breadcrumb bar's jump up several
        // levels. Zero returns to the root page, and a value at or above the current depth is a no-op.
        internal void NavigateToDepth(int keep)
        {
            while (_breadcrumbs.Count > keep && CanNavigateBack)
                NavigateBack();
        }

        internal TreeNode NavigateBack()
        {
            if (!CanNavigateBack) return null;

            var previousNode = _currentNode;
            _currentNode = _breadcrumbs[^1];
            _breadcrumbs.RemoveAt(_breadcrumbs.Count - 1);
            return previousNode;
        }

        internal void NavigateToAssemblyQualifiedName(string aqn)
        {
            var path = new List<TreeNode>();
            if (!FindPathToAssemblyQualifiedName(_rootNode, aqn, path) || path.Count < 2) return;

            path.Reverse();

            for (var i = 1; i < path.Count - 1; i++)
            {
                _breadcrumbs.Add(_currentNode);
                _currentNode = path[i];
            }
        }

        internal void RefreshFavoritesSection()
        {
            if (_composeSections) RebuildRootItems();
        }

        internal bool IsSectionCollapsed(string sectionKey) =>
            sectionKey is not null && _collapsedSections.Contains(sectionKey);

        internal void ToggleSection(string sectionKey)
        {
            if (!_composeSections || string.IsNullOrEmpty(sectionKey)) return;

            if (!_collapsedSections.Remove(sectionKey))
                _collapsedSections.Add(sectionKey);

            RebuildVisibleRootItems();
        }

        private void RebuildRootItems()
        {
            _rootItems.Clear();

            var noneOption = _rootNode.Children.FirstOrDefault(child => child.IsNoneOption);

            if (noneOption is not null)
                _rootItems.Add(noneOption);

            if (TypeSelectorSettings.ShowFavorites)
                AppendSection(FavoritesSection, TypeSelectorPreferences.LoadFavorites());

            AppendSection(RecentSection, TypeSelectorPreferences.LoadRecents());

            foreach (var child in _rootNode.Children)
            {
                if (child != noneOption)
                    _rootItems.Add(child);
            }

            RebuildVisibleRootItems();
        }

        private void RebuildVisibleRootItems()
        {
            _visibleRootItems.Clear();

            foreach (var node in _rootItems)
            {
                if (!node.IsSectionTitle && node.SectionKey is not null && _collapsedSections.Contains(node.SectionKey))
                    continue;

                _visibleRootItems.Add(node);
            }
        }

        private void AppendSection(string title, IReadOnlyList<string> assemblyQualifiedNames)
        {
            var rows = new List<TreeNode>();

            foreach (var aqn in assemblyQualifiedNames)
            {
                if (!_typesByAqn.TryGetValue(aqn, out var source)) continue;

                rows.Add(new TreeNode(source.DisplayName, source.AssemblyQualifiedName, source.Caption)
                {
                    Tooltip = source.Tooltip,
                    Icon = source.Icon,
                    SearchName = source.SearchName,
                    SectionKey = title,
                });
            }

            if (rows.Count is 0) return;

            _rootItems.Add(new TreeNode(title) { Kind = TreeNodeKind.SectionTitle, SectionKey = title, TypeCount = rows.Count });
            _rootItems.AddRange(rows);
        }

        private void IndexTypeLeaves(TreeNode node)
        {
            if (node.AssemblyQualifiedName is not null)
                _typesByAqn[node.AssemblyQualifiedName] = node;

            foreach (var child in node.Children)
                IndexTypeLeaves(child);
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
                    yield return leaf;
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
