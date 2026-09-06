using System;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal class TreeNode
    {
        private int? _typeCount;

        internal string Caption { get; set; }

        internal string Tooltip { get; set; }

        internal List<TreeNode> Children { get; }

        internal string DisplayName { get; set; }

        internal string AssemblyQualifiedName { get; set; }

        // Raw editor icon identifier sourced from TypeSelectorDisplayAttribute.Icon;
        // null when the node has no icon.
        internal string Icon { get; set; }

        // The real (short) type name, kept separately from DisplayName so search keeps
        // matching the original type name even when the displayed label is disambiguated with its
        // assembly. null for non-type nodes.
        internal string SearchName { get; set; }

        // The node's presentation role. Section titles are non-interactive separators inserted by the
        // Favorites/Recents rendering; everything else is Default.
        internal TreeNodeKind Kind { get; set; }

        // The Favorites or Recents section this row belongs to, set on the header and its item rows alike, or null
        // for a row outside any composed section. Drives which section a row collapses under and its styling.
        internal string SectionKey { get; set; }

        // How many pickable types the row stands for, shown as the dim counter on container and section rows. A
        // container counts its descendant leaves lazily, which is safe because the hierarchy is immutable once
        // built; a section title assigns its count explicitly.
        internal int TypeCount
        {
            get => _typeCount ??= CountTypes(this);
            set => _typeCount = value;
        }

        internal bool HasChildren => Children.Count > 0;

        internal bool IsSectionTitle => Kind == TreeNodeKind.SectionTitle;

        // Whether this node represents a concrete pickable type (has an assembly-qualified name and is
        // not a section header). Used to gate the favorite star toggle.
        internal bool IsType => Kind == TreeNodeKind.Default && AssemblyQualifiedName is not null;

        internal bool IsSelectable =>
            Kind == TreeNodeKind.Default && (IsType || IsNoneOption);

        internal bool IsNoneOption =>
            AssemblyQualifiedName is null && DisplayName == TypeSelectorHelpers.NoneOption;

        internal TreeNode(string displayName, string assemblyQualifiedName = null, string caption = null)
        {
            DisplayName = displayName;
            AssemblyQualifiedName = assemblyQualifiedName;
            Caption = caption ?? displayName;
            Tooltip = string.Empty;
            Icon = null;
            SearchName = null;
            Kind = TreeNodeKind.Default;
            Children = new List<TreeNode>();
        }

        internal bool MatchesFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return Contains(DisplayName, filter)
                || Contains(Caption, filter)
                // Keep matching the real type name even when the displayed label is disambiguated.
                || Contains(SearchName, filter)
                || Contains(AssemblyQualifiedName, filter);
        }

        private static bool Contains(string text, string filter) =>
            text is not null && text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

        private static int CountTypes(TreeNode node) =>
            (node.IsType ? 1 : 0) + node.Children.Sum(CountTypes);
    }
}
