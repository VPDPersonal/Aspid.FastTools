// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
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
