using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// The role a <see cref="TreeNode"/> plays in the rendered list, used to style and gate
    /// interaction (section headers are not selectable and never show a star toggle).
    /// </summary>
    internal enum TreeNodeKind
    {
        /// <summary>A regular hierarchy node (type leaf, namespace or category container).</summary>
        Default,

        /// <summary>A non-selectable header that introduces the Favorites or Recents section.</summary>
        SectionTitle,
    }

    internal class TreeNode
    {
        public string Caption { get; set; }

        public string Tooltip { get; set; }

        public List<TreeNode> Children { get; }

        public string DisplayName { get; set; }

        public string AssemblyQualifiedName { get; set; }

        /// <summary>
        /// Raw editor icon identifier sourced from <see cref="TypeSelectorItemAttribute.Icon"/>;
        /// <see langword="null"/> when the node has no icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The real (short) type name, kept separately from <see cref="DisplayName"/> so search keeps
        /// matching the original type name even when the displayed label is disambiguated with its
        /// assembly. <see langword="null"/> for non-type nodes.
        /// </summary>
        public string SearchName { get; set; }

        /// <summary>
        /// The node's presentation role. Section titles are non-interactive separators inserted by the
        /// Favorites/Recents rendering; everything else is <see cref="TreeNodeKind.Default"/>.
        /// </summary>
        public TreeNodeKind Kind { get; set; }

        /// <summary>
        /// Title of the Favorites/Recents section this row belongs to (set on both the section header and its item
        /// rows by <see cref="NavigationController"/>), or <see langword="null"/> for rows outside any composed section
        /// (the &lt;None&gt; option, root categories, search results). Drives which section a row collapses under and
        /// the indented, left-lined styling of section items.
        /// </summary>
        public string SectionKey { get; set; }

        public bool HasChildren => Children.Count > 0;

        public bool IsSectionTitle => Kind == TreeNodeKind.SectionTitle;

        /// <summary>
        /// Whether this node represents a concrete pickable type (has an assembly-qualified name and is
        /// not a section header). Used to gate the favorite star toggle.
        /// </summary>
        public bool IsType => Kind == TreeNodeKind.Default && AssemblyQualifiedName is not null;

        public bool IsSelectable =>
            Kind == TreeNodeKind.Default &&
            (AssemblyQualifiedName is not null || DisplayName == TypeSelectorHelpers.NoneOption);

        public TreeNode(string displayName, string assemblyQualifiedName = null, string caption = null)
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

        public bool MatchesFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (DisplayName?.ToLowerInvariant().Contains(filter) == true)
                return true;

            if (Caption?.ToLowerInvariant().Contains(filter) == true)
                return true;

            // Keep matching the real type name even when the displayed label is disambiguated.
            if (SearchName?.ToLowerInvariant().Contains(filter) == true)
                return true;

            if (AssemblyQualifiedName?.ToLowerInvariant().Contains(filter) == true)
                return true;

            return false;
        }
    }
}
