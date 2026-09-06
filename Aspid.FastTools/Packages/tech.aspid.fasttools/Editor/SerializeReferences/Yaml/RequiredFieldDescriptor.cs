// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The serialized shape of a required field, so the pure-YAML scene scan can read its "unset" state without
    // reflection.
    internal enum RequiredFieldKind
    {
        // Unset means a null-or-empty scalar.
        String,

        // Unset means the null-id pointer.
        ManagedReference,

        // Unset means the wrapper's nested _assemblyQualifiedName scalar is null-or-empty.
        SerializableType,
    }

    // A field that opts into the required check, captured for the pure-YAML scene scan: its YAML key, its kind and,
    // for a field nested inside plain [Serializable] containers, the chain of container keys leading to it. Produced
    // by reflection in the required gate and consumed by the YAML scan, which stays reflection-free.
    internal readonly struct RequiredFieldDescriptor
    {
        public readonly RequiredFieldKind Kind;
        public readonly string FieldName;

        // Container keys from the document's top level down to the field's parent; empty for a top-level field.
        public readonly string[] Parents;

        // The dotted path, matching what SerializedProperty reports, so gate reports read alike either way.
        public string Path => Parents is { Length: > 0 } ? string.Join(".", Parents) + "." + FieldName : FieldName;

        public RequiredFieldDescriptor(string fieldName, RequiredFieldKind kind)
            : this(System.Array.Empty<string>(), fieldName, kind) { }

        public RequiredFieldDescriptor(string[] parents, string fieldName, RequiredFieldKind kind)
        {
            Kind = kind;
            Parents = parents;
            FieldName = fieldName;
        }
    }
}
