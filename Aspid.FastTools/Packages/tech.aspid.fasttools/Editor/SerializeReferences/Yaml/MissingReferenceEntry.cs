// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // One orphaned managed-reference entry found in an asset's YAML: its document, its RefIds id and the stored,
    // unresolvable type.
    internal readonly struct MissingReferenceEntry
    {
        public readonly long Rid;
        public readonly long FileId;
        public readonly ManagedTypeName StoredType;

        public MissingReferenceEntry(long fileId, long rid, ManagedTypeName storedType)
        {
            Rid = rid;
            FileId = fileId;
            StoredType = storedType;
        }
    }
}
