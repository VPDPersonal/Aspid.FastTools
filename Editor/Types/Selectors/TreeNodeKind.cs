// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // The role a TreeNode plays in the rendered list, used to style and gate
    // interaction (section headers are not selectable and never show a star toggle).
    internal enum TreeNodeKind
    {
        // A regular hierarchy node (type leaf, namespace or category container).
        Default,

        // A non-selectable header that introduces the Favorites or Recents section.
        SectionTitle,
    }
}
