namespace Aspid.FastTools.Analyzers.Descriptions.AspidFastTools;

public static class ClassesDescription
{
    public const string SerializableType = nameof(SerializableType);

    // The non-generic wrapper. Compared against ITypeSymbol.OriginalDefinition.ToDisplayString().
    public const string SerializableTypeFull = $"{NamespacesDescription.AspidFastToolsTypes}.{SerializableType}";

    // The generic wrapper's open definition renders its type parameter as "<T>" in a display string.
    public const string SerializableTypeGenericFull = $"{SerializableTypeFull}<T>";
}
