// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // One unset required field found by the pure-YAML scene scan. Rid is the null id read from a managed reference,
    // and 0 for a string field.
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
