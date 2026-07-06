// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The serialized shape of a <c>[TypeSelector(Required = true)]</c> field, so the pure-YAML scene scan knows how to
    /// read its "unset" state without reflection.
    /// </summary>
    internal enum RequiredFieldKind
    {
        /// <summary>A <c>string</c> type-name field: unset == a null-or-empty scalar.</summary>
        String,

        /// <summary>A <c>[SerializeReference]</c> managed reference: unset == the null-id pointer.</summary>
        ManagedReference,

        /// <summary>A <see cref="Aspid.FastTools.Types.SerializableType"/> wrapper: unset == its nested
        /// <c>_assemblyQualifiedName</c> scalar is null-or-empty.</summary>
        SerializableType,
    }

    /// <summary>
    /// A serialized field that opts into the <c>[TypeSelector(Required = true)]</c> check, captured for the pure-YAML
    /// scene scan: the YAML field key and its <see cref="RequiredFieldKind"/>. Produced by reflection in
    /// <see cref="SerializeReferenceRequiredGate.GetRequiredFields"/> and consumed by
    /// <see cref="SerializeReferenceYamlEditor.FindUnsetRequiredFields"/>, which stays reflection-free.
    /// </summary>
    internal readonly struct RequiredFieldDescriptor
    {
        public readonly RequiredFieldKind Kind;
        public readonly string FieldName;

        public RequiredFieldDescriptor(string fieldName, RequiredFieldKind kind)
        {
            Kind = kind;
            FieldName = fieldName;
        }
    }
}
