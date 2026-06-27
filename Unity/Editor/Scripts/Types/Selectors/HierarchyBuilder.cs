using System;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class HierarchyBuilder
    {
        public static TreeNode Build(
            Type[] types,
            TypeAllow allow,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            bool includeNoneOption = true)
        {
            var allTypes = TypeInfo.GetAllTypeInfos(types, allow, filter, additionalTypes);

            var root = new TreeNode("/");

            // Generic-argument pages must yield a concrete type, so they omit the <None> entry.
            if (includeNoneOption)
                root.Children.Add(new TreeNode(TypeSelectorHelpers.NoneOption, null, TypeSelectorHelpers.NoneOption));

            // Types carrying a [TypeSelectorItem("Cat/Sub/...")] path are re-homed under explicit category
            // nodes instead of their namespace hierarchy; everything else keeps its namespace placement.
            var categorized = allTypes.Where(type => type.HasCategoryPath).ToList();
            var uncategorized = allTypes.Where(type => !type.HasCategoryPath).ToList();

            AddCategoryHierarchy(root, categorized);
            AddGlobalNamespaceGroup(root, uncategorized);
            AddNamespaceHierarchy(root, uncategorized);

            SortNode(root);

            return root;
        }

        private static void AddCategoryHierarchy(TreeNode root, List<TypeInfo> types)
        {
            foreach (var type in types)
            {
                var parent = root;

                // Walk/create the category nodes; categories with the same name merge.
                foreach (var segment in type.CategoryPath)
                    parent = GetOrCreateCategory(parent, segment);

                parent.Children.Add(CreateLeaf(type, type.DisplayName));
            }
        }

        private static TreeNode GetOrCreateCategory(TreeNode parent, string segment)
        {
            var existing = parent.Children.FirstOrDefault(child =>
                child.AssemblyQualifiedName is null &&
                child.DisplayName == segment &&
                child.DisplayName != TypeSelectorHelpers.NoneOption);

            if (existing is not null) return existing;

            var node = new TreeNode(segment, null, segment);
            parent.Children.Add(node);
            return node;
        }

        private static void AddGlobalNamespaceGroup(TreeNode root, List<TypeInfo> types)
        {
            var globals = types
                .Where(type => type.Namespace == TypeSelectorHelpers.GlobalNamespace)
                .ToList();

            if (globals.Count is 0) return;
            var globalGroup = new TreeNode(TypeSelectorHelpers.GlobalNamespace);

            AddTypesWithDisambiguation(globalGroup, globals, includeNamespace: false);
            root.Children.Add(globalGroup);
        }

        private static void AddNamespaceHierarchy(TreeNode root, List<TypeInfo> types)
        {
            var namespacedTypes = types
                .Where(type => type.Namespace != TypeSelectorHelpers.GlobalNamespace)
                .ToList();

            var trie = BuildNamespaceTrie(namespacedTypes);

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
            if (trieNode.IsTerminal && nsToTypes.TryGetValue(nextNamespace, out var typeInfos))
                AddTypesWithDisambiguation(node, typeInfos, includeNamespace: true, nextNamespace);

            // Add child namespaces
            foreach (var child in trieNode.Children.Values.OrderBy(n => n.Segment))
                node.Children.Add(BuildNamespaceNode(child, nextDisplay, nextNamespace, nsToTypes));

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
                node.Caption = onlyChild.Caption;
                node.Children.Clear();
                node.Children.AddRange(onlyChild.Children);
            }
            else
            {
                node.DisplayName = $"{node.DisplayName}.{onlyChild.DisplayName}";
                node.AssemblyQualifiedName = onlyChild.AssemblyQualifiedName;
                node.Caption = onlyChild.Caption;
                node.Tooltip = onlyChild.Tooltip;
                node.Order = onlyChild.Order;
                node.Icon = onlyChild.Icon;
                node.SearchName = onlyChild.SearchName;
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
                .GroupBy(type => type.DisplayName)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var type in types)
            {
                var needsAssembly = nameCounts[type.DisplayName] > 1;
                var displayName = needsAssembly ? $"{type.DisplayName} ({type.Assembly})" : type.DisplayName;

                var caption = includeNamespace
                    ? $"{namespacePath}.{displayName}"
                    : displayName;

                parent.Children.Add(CreateLeaf(type, displayName, caption));
            }
        }

        private static TreeNode CreateLeaf(TypeInfo type, string displayName, string caption = null)
        {
            return new TreeNode(displayName, type.AssemblyQualifiedName, caption ?? displayName)
            {
                Tooltip = type.Tooltip,
                Order = type.Order,
                Icon = type.Icon,
                SearchName = type.Name,
            };
        }

        /// <summary>
        /// Sorts each node's children by <see cref="TreeNode.Order"/> then alphabetically by display
        /// name, while keeping the <c>&lt;None&gt;</c> option pinned to the top. Applied recursively so
        /// every hierarchy level honours the same ordering.
        /// </summary>
        private static void SortNode(TreeNode node)
        {
            node.Children.Sort(CompareNodes);

            foreach (var child in node.Children)
                SortNode(child);
        }

        private static int CompareNodes(TreeNode left, TreeNode right)
        {
            // Keep <None> pinned to the top of the root list.
            var leftNone = left.DisplayName == TypeSelectorHelpers.NoneOption;
            var rightNone = right.DisplayName == TypeSelectorHelpers.NoneOption;

            if (leftNone != rightNone) return leftNone ? -1 : 1;

            if (left.Order != right.Order)
                return left.Order.CompareTo(right.Order);

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
