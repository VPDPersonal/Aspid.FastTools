// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct MissingReferenceEntry
    {
        public readonly long Rid;
        public readonly long FileId;
        public readonly ManagedTypeName StoredType;

        // True for a type set by a prefab instance override: FileId is then the PrefabInstance document, and the
        // YAML repair (which edits RefIds blocks) cannot reach the entry.
        public readonly bool IsOverride;

        // The overridden field that points at the reference; empty for a RefIds entry.
        public readonly string FieldPath;

        public MissingReferenceEntry(long fileId, long rid, ManagedTypeName storedType, bool isOverride = false, string fieldPath = null)
        {
            Rid = rid;
            FileId = fileId;
            StoredType = storedType;
            IsOverride = isOverride;
            FieldPath = fieldPath ?? string.Empty;
        }
    }
}
