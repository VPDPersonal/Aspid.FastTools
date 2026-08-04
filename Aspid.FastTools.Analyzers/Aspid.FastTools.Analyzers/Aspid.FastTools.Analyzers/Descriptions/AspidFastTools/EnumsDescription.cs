namespace Aspid.FastTools.Analyzers.Descriptions.AspidFastTools;

public static class EnumsDescription
{
    public const string TypeAllow = nameof(TypeAllow);
    public const string TypeAllowFull = $"{NamespacesDescription.AspidFastToolsTypes}.{TypeAllow}";

    // The named argument these flags are passed through on [TypeSelector].
    public const string AllowArgument = "Allow";

    // TypeAllow.None — the only value whose narrowing is meaningful on a managed reference. Every other value
    // opts abstract classes and/or interfaces in, which cannot be instantiated for a [SerializeReference] field.
    public const int TypeAllowNone = 0;
}
