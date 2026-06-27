// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// A serialized field that opts into the <c>[TypeSelector(Required = true)]</c> check, captured for the pure-YAML
    /// scene scan: the YAML field key and whether it is a <c>string</c> type field (vs a <c>[SerializeReference]</c>
    /// managed reference). Produced by reflection in <see cref="SerializeReferenceRequiredGate.GetRequiredFields"/> and
    /// consumed by <see cref="SerializeReferenceYamlEditor.FindUnsetRequiredFields"/>, which stays reflection-free.
    /// </summary>
    internal readonly struct RequiredFieldDescriptor
    {
        public readonly bool IsString;
        public readonly string FieldName;

        public RequiredFieldDescriptor(string fieldName, bool isString)
        {
            IsString = isString;
            FieldName = fieldName;
        }
    }
}
