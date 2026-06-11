using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed class NavigationController
    {
        private TreeNode _currentNode;
        private readonly TreeNode _rootNode;
        private readonly bool _composeSections;

        private readonly List<TreeNode> _breadcrumbs = new();
        private readonly List<TreeNode> _searchResults = new();

        // Cached composition of the root page (None + Favorites/Recents sections + root children).
        private readonly List<TreeNode> _rootItems = new();

        // All pickable type leaves in the current candidate set, keyed by assembly-qualified name.
        private readonly Dictionary<string, TreeNode> _typesByAqn = new();

        public bool IsSearching { get; private set; }

        public bool CanNavigateBack =>
            _breadcrumbs.Count > 0;

        /// <summary>
        /// Whether the picker is showing its root page (not searching, no breadcrumbs). The Favorites
        /// and Recents sections are only composed here, and only when the controller was created with
        /// section composition enabled.
        /// </summary>
        public bool IsAtRoot =>
            !IsSearching && _breadcrumbs.Count is 0;

        public List<TreeNode> CurrentItems
        {
            get
            {
                if (IsSearching) return _searchResults;
                if (_composeSections && IsAtRoot) return _rootItems;
                return _currentNode.Children;
            }
        }

        /// <param name="root">The root of the candidate hierarchy.</param>
        /// <param name="composeSections">
        /// When <see langword="true"/>, the root page is augmented with <c>★ Favorites</c> and
        /// <c>Recent</c> sections drawn from <see cref="TypeSelectorPreferences"/>; only the base
        /// (root) page of the picker enables this — generic-argument pages do not.
        /// </param>
        public NavigationController(TreeNode root, bool composeSections = false)
        {
            _rootNode = root;
            _currentNode = root;
            _composeSections = composeSections;
            _breadcrumbs.Clear();

            if (!_composeSections) return;

            IndexTypeLeaves(root);
            RebuildRootItems();
        }

        public string GetCurrentTitle()
        {
            if (IsSearching) return "Search";
            if (_breadcrumbs.Count is 0) return "Select Type";

            return string.Join("/", _breadcrumbs
                .Select(node => node.DisplayName)
                .Append(_currentNode.DisplayName)
                .Where(name => name is not "/"));
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
                            node.Caption)
                        {
                            Tooltip = node.Tooltip,
                            Order = node.Order,
                            Icon = node.Icon,
                            SearchName = node.SearchName,
                        });
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

            // FindPathToAssemblyQualifiedName builds path leaf-to-root; reverse for root-to-leaf traversal
            path.Reverse();

            // Navigate into each node from root's child down to the target's parent
            for (var i = 1; i < path.Count - 1; i++)
            {
                _breadcrumbs.Add(_currentNode);
                _currentNode = path[i];
            }
        }

        /// <summary>
        /// Re-composes the root page after the favorites set changes (e.g. a star was toggled), so the
        /// Favorites section reflects the new state on the next refresh. No-op when this controller does
        /// not compose sections.
        /// </summary>
        public void RefreshFavoritesSection()
        {
            if (_composeSections) RebuildRootItems();
        }

        private void RebuildRootItems()
        {
            _rootItems.Clear();

            // Pin the <None> option (always the first root child) to the very top.
            var noneOption = _rootNode.Children
                .FirstOrDefault(child => child.DisplayName == TypeSelectorHelpers.NoneOption);

            if (noneOption is not null)
                _rootItems.Add(noneOption);

            AppendSection("★ Favorites", TypeSelectorPreferences.LoadFavorites());
            AppendSection("Recent", TypeSelectorPreferences.LoadRecents());

            foreach (var child in _rootNode.Children)
                if (child != noneOption)
                    _rootItems.Add(child);
        }

        private void AppendSection(string title, IReadOnlyList<string> assemblyQualifiedNames)
        {
            var rows = new List<TreeNode>();

            foreach (var aqn in assemblyQualifiedNames)
            {
                // Only surface types that are part of the current candidate set.
                if (!_typesByAqn.TryGetValue(aqn, out var source)) continue;

                rows.Add(new TreeNode(source.DisplayName, source.AssemblyQualifiedName, source.Caption)
                {
                    Tooltip = source.Tooltip,
                    Order = source.Order,
                    Icon = source.Icon,
                    SearchName = source.SearchName,
                });
            }

            if (rows.Count is 0) return;

            _rootItems.Add(new TreeNode(title) { Kind = TreeNodeKind.SectionTitle });
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
