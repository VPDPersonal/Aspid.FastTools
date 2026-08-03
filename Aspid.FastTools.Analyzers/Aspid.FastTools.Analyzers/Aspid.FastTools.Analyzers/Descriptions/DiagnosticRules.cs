using Severity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using Descriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;

namespace Aspid.FastTools.Analyzers.Descriptions;

public static class DiagnosticRules
{
    private const string UsageCategory = "Usage";

    public static readonly Descriptor TypeSelectorFieldTypeRule = new(
        id: "AFT0001",
        title: "[TypeSelector] applied to an unsupported field",
        messageFormat: "[TypeSelector] on '{0}' has no effect: apply it to a string field or to a [SerializeReference] managed-reference field",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorAllowRule = new(
        id: "AFT0002",
        title: "[TypeSelector] Allow has no effect on a managed reference",
        messageFormat: "[TypeSelector] on '{0}' sets Allow, but abstract classes and interfaces cannot be instantiated for a [SerializeReference] field — Allow is ignored here",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorBaseTypeRule = new(
        id: "AFT0003",
        title: "[TypeSelector] base type shares no concrete type with the field",
        messageFormat: "[TypeSelector] base type '{0}' shares no concrete type with the field type '{1}' — the selector will be empty",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorObjectDerivedRule = new(
        id: "AFT0004",
        title: "[TypeSelector] managed reference targets a UnityEngine.Object-derived type",
        messageFormat: "[TypeSelector] with [SerializeReference] on '{0}': {1} derives from UnityEngine.Object, which Unity does not serialize as a managed reference — use a plain object field instead",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorNoConcreteImplementationRule = new(
        id: "AFT0005",
        title: "[TypeSelector] base type has no visible concrete implementation",
        messageFormat: "[TypeSelector] with [SerializeReference] on '{0}': no concrete, non-UnityEngine.Object class implementing '{1}' is visible in the compilation — the selector may be empty (implementations in downstream assemblies are not checked)",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorMemberNotFoundRule = new(
        id: "AFT0006",
        title: "[TypeSelector] string argument resolves to nothing",
        messageFormat: "[TypeSelector] on '{0}': '{1}' is neither a member of '{2}' nor an assembly-qualified type name — declare the member, or qualify the type with its assembly (\"{1}, MyAssembly\")",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorMemberUnsuitableRule = new(
        id: "AFT0007",
        title: "[TypeSelector] member reference cannot supply base types",
        messageFormat: "[TypeSelector] on '{0}': member '{1}' cannot supply base types — it must be an instance field or property of type Type, string, SerializableType (or an array of these)",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true);

    public static readonly Descriptor TypeSelectorTypeNameSyntaxRule = new(
        id: "AFT0008",
        title: "[TypeSelector] string argument is not a valid type name",
        messageFormat: "[TypeSelector] on '{0}': '{1}' is not a valid assembly-qualified type name",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true);
}
