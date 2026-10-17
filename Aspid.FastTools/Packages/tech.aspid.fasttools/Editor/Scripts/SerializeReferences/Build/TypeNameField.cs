// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A [TypeSelector] string, SerializableType or SerializableMonoScript field. FieldPath is a property path, as in
    // GateViolation; TypeName is empty while the field holds no type.
    internal readonly struct TypeNameField
    {
        public readonly long FileId;
        public readonly string FieldPath;
        public readonly string ComponentName;
        public readonly string TypeName;
        public readonly bool Required;

        public TypeNameField(long fileId, string fieldPath, string componentName, string typeName, bool required)
        {
            FileId = fileId;
            FieldPath = fieldPath;
            ComponentName = componentName;
            TypeName = typeName;
            Required = required;
        }
    }
}
