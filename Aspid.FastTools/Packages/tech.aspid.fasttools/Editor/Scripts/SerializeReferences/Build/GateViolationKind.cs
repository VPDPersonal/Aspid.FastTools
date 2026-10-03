// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal enum GateViolationKind
    {
        MissingType,
        RequiredUnset,

        // A SerializableType or SerializableMonoScript field whose stored name no longer resolves.
        MissingTypeName,
    }
}
