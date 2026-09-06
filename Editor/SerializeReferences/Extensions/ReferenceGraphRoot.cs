// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A field pointer from a document's body into its RefIds block — a root of the reference tree. Label is the full
    // field path holding it, with list elements indexed (_config._slots[2]). A field holding nothing is kept as an
    // empty root, with no node behind it, so a cleared slot stays visible.
    internal readonly struct ReferenceGraphRoot
    {
        public readonly long Rid;
        public readonly string Label;

        public ReferenceGraphRoot(long rid, string label)
        {
            Rid = rid;
            Label = label;
        }

        public bool IsEmpty => Rid < 0;
    }
}
