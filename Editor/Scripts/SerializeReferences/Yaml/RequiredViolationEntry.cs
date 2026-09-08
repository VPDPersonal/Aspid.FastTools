// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct RequiredViolationEntry
    {
        public readonly long Rid;
        public readonly long FileId;
        public readonly string FieldName;

        public RequiredViolationEntry(long fileId, string fieldName, long rid)
        {
            Rid = rid;
            FileId = fileId;
            FieldName = fieldName;
        }
    }
}
