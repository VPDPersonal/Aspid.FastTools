// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
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
