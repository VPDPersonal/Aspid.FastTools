// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct ReferenceGraphNode
    {
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;
        public readonly bool Resolves;

        public ReferenceGraphNode(long rid, ManagedTypeName storedType, bool resolves)
        {
            Rid = rid;
            StoredType = storedType;
            Resolves = resolves;
        }

        public string ShortName =>
            string.IsNullOrEmpty(StoredType.Class) ? $"rid {Rid}" : StoredType.Class;

        public string FullName => StoredType.FullName;
    }
}
