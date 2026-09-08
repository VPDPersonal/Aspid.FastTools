// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal enum RequiredFieldKind
    {
        // Unset means a null-or-empty scalar.
        String,

        // Unset means the null-id pointer.
        ManagedReference,

        // Unset means the wrapper's nested _assemblyQualifiedName scalar is null-or-empty.
        SerializableType,
    }

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
