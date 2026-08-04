namespace Aspid.FastTools.Analyzers.Descriptions.UnityEngine;

public static class AttributesDescription
{
    // Unity declares its serialization attributes without the conventional "Attribute" suffix
    // (class SerializeReference : Attribute), so the symbol display string carries none either.
    public const string SerializeReference = nameof(SerializeReference);
    public const string SerializeReferenceFull = $"{NamespacesDescription.UnityEngine}.{SerializeReference}";
}
