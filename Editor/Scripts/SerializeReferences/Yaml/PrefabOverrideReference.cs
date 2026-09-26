// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A managed reference whose type is set by a prefab instance override (a variant, a nested prefab or an instance in
    // a scene). Unity stores it in the PrefabInstance document's m_Modifications, not in a RefIds block.
    internal readonly struct PrefabOverrideReference
    {
        public readonly long Rid;

        // The anchor of the "--- !u!1001" PrefabInstance document that holds the modification.
        public readonly long FileId;

        public readonly ManagedTypeName StoredType;

        // The overridden object in the source prefab.
        public readonly long TargetFileId;
        public readonly string TargetGuid;

        // The property path of the modification that points at Rid ("weapon", "list.Array.data[0]"), or empty when
        // no modification in the document does.
        public readonly string FieldPath;

        public PrefabOverrideReference(
            long fileId,
            long rid,
            ManagedTypeName storedType,
            long targetFileId,
            string targetGuid,
            string fieldPath)
        {
            Rid = rid;
            FileId = fileId;
            StoredType = storedType;
            TargetFileId = targetFileId;
            TargetGuid = targetGuid ?? string.Empty;
            FieldPath = fieldPath ?? string.Empty;
        }
    }
}
