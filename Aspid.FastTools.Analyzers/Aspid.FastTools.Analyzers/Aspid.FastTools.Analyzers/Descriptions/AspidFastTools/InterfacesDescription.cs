namespace Aspid.FastTools.Analyzers.Descriptions.AspidFastTools;

public static class InterfacesDescription
{
    // The contract of every Type wrapper; the drawer reads a member reference's wrapper value through it.
    public const string ISerializableType = nameof(ISerializableType);
    public const string ISerializableTypeFull = $"{NamespacesDescription.AspidFastToolsTypes}.{ISerializableType}";
}
