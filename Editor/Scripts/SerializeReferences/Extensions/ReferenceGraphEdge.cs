// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A parent -> child edge of a document's nested graph. Label is the field path relative to the PARENT's data
    // block; the view joins it onto the parent's path to show where a nested reference lives. A null child slot is
    // kept as an empty edge that points at no node and never recurses.
    internal readonly struct ReferenceGraphEdge
    {
        public readonly long Rid;
        public readonly string Label;

        public ReferenceGraphEdge(long rid, string label)
        {
            Rid = rid;
            Label = label;
        }

        public bool IsEmpty => Rid < 0;
    }
}
