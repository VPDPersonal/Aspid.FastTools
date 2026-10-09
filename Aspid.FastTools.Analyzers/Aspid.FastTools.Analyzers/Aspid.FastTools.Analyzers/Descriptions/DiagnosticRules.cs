using Severity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using Descriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;

namespace Aspid.FastTools.Analyzers.Descriptions;

public static class DiagnosticRules
{
    private const string UsageCategory = "Usage";

    // The IDE opens the help link from the diagnostic ID; without one it searches the web for the ID.
    private const string DocsUrl = "https://vpdpersonal.github.io/Aspid.FastTools/docs/";
    private const string TypeSelectorDocs = DocsUrl + "type-selector";
    private const string SerializeReferenceDocs = DocsUrl + "serialize-reference-selector";
    private const string ProfilerMarkersDocs = DocsUrl + "profiler-markers";

    public static readonly Descriptor TypeSelectorFieldTypeRule = new(
        id: "AFT0001",
        title: "[TypeSelector] applied to an unsupported field",
        messageFormat: "[TypeSelector] on '{0}' has no effect: apply it to a string field, a SerializableType / SerializableMonoScript field or a [SerializeReference] managed-reference field (a SerializableType subclass must be [Serializable], not abstract, and have a parameterless constructor)",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true,
        description: "[TypeSelector] draws a string field, a SerializableType or SerializableMonoScript field, or a [SerializeReference] field. Change the field type, add [SerializeReference], or remove the attribute.",
        helpLinkUri: TypeSelectorDocs + "#supported-fields");

    public static readonly Descriptor TypeSelectorAllowRule = new(
        id: "AFT0002",
        title: "[TypeSelector] Allow has no effect on a managed reference",
        messageFormat: "[TypeSelector] on '{0}' sets Allow, but abstract classes and interfaces cannot be instantiated for a [SerializeReference] field — Allow is ignored here",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "A [SerializeReference] field needs an instance, so its picker never offers abstract classes or interfaces. Remove the Allow argument.",
        helpLinkUri: TypeSelectorDocs + "#properties");

    public static readonly Descriptor TypeSelectorBaseTypeRule = new(
        id: "AFT0003",
        title: "[TypeSelector] base type shares no concrete type with the field",
        messageFormat: "[TypeSelector] base type '{0}' shares no concrete type with the field type '{1}' — the selector will be empty",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The picker offers only types assignable to the field type and to every base type of the attribute. Pass a base type that has a subtype in common with the field type, or change the field type.",
        helpLinkUri: TypeSelectorDocs + "#which-types-are-offered");

    public static readonly Descriptor TypeSelectorObjectDerivedRule = new(
        id: "AFT0004",
        title: "[TypeSelector] managed reference targets a UnityEngine.Object-derived type",
        messageFormat: "[TypeSelector] with [SerializeReference] on '{0}': '{1}' derives from UnityEngine.Object, which Unity does not serialize as a managed reference — use a plain object field instead",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true,
        description: "Unity does not store a UnityEngine.Object as a managed reference. Remove [SerializeReference] and [TypeSelector] and keep a plain object reference field.",
        helpLinkUri: SerializeReferenceDocs + "#which-classes-are-offered");

    public static readonly Descriptor TypeSelectorNoConcreteImplementationRule = new(
        id: "AFT0005",
        title: "[TypeSelector] base type has no visible concrete implementation",
        messageFormat: "[TypeSelector] with [SerializeReference] on '{0}': no concrete, non-UnityEngine.Object class implementing {1} is visible in the compilation — the selector may be empty (implementations in downstream assemblies are not checked)",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The picker of a [SerializeReference] field offers concrete classes that implement every base type and do not derive from UnityEngine.Object. Add such a class or loosen the base types; if the classes live in an assembly that references this one, suppress the warning.",
        helpLinkUri: SerializeReferenceDocs + "#which-classes-are-offered");

    public static readonly Descriptor TypeSelectorMemberNotFoundRule = new(
        id: "AFT0006",
        title: "[TypeSelector] string argument resolves to nothing",
        messageFormat: "[TypeSelector] on '{0}': '{1}' is neither a member of '{2}' nor an assembly-qualified type name — declare the member, or qualify the type with its assembly (\"{1}, MyAssembly\")",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true,
        description: "A string argument names an instance field or property of the declaring type, or a type as \"Namespace.Type, Assembly\". Declare the member, correct the name, or qualify the type with its assembly.",
        helpLinkUri: TypeSelectorDocs + "#constraint-from-another-field");

    public static readonly Descriptor TypeSelectorMemberUnsuitableRule = new(
        id: "AFT0007",
        title: "[TypeSelector] member reference cannot supply base types",
        messageFormat: "[TypeSelector] on '{0}': member '{1}' cannot supply base types — it must be an instance field or readable property of type Type, string, SerializableType or SerializableMonoScript (or an array of these)",
        category: UsageCategory,
        defaultSeverity: Severity.Error,
        isEnabledByDefault: true,
        description: "The Inspector reads base types from an instance field or a readable property of type Type, string, SerializableType or SerializableMonoScript, or from an array of these. Change the member, or reference another one.",
        helpLinkUri: TypeSelectorDocs + "#constraint-from-another-field");

    public static readonly Descriptor TypeSelectorTypeNameSyntaxRule = new(
        id: "AFT0008",
        title: "[TypeSelector] string argument is not a valid type name",
        messageFormat: "[TypeSelector] on '{0}': '{1}' is not a valid assembly-qualified type name — {2}",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The Inspector resolves the name with Type.GetType, which finds a type without an assembly only in mscorlib. Write the name as \"Namespace.Type, Assembly\".",
        helpLinkUri: TypeSelectorDocs + "#errors-in-string-arguments");

    public static readonly Descriptor TypeSelectorDisjointBaseTypesRule = new(
        id: "AFT0009",
        title: "[TypeSelector] base types have no type in common",
        messageFormat: "[TypeSelector] base types '{0}' and '{1}' have no type in common — a candidate must be assignable to every base type, so the selector will be empty",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The picker offers only types assignable to every base type of the attribute. Give the allowed classes a common base class or interface and pass that instead.",
        helpLinkUri: TypeSelectorDocs + "#which-types-are-offered");

    public static readonly Descriptor TypeSelectorNotSerializedRule = new(
        id: "AFT0012",
        title: "[TypeSelector] on a field Unity does not serialize",
        messageFormat: "[TypeSelector] on '{0}' has no effect: Unity does not serialize the field, so the Inspector does not show it — {1}",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The Inspector shows only fields Unity serializes. Make the field public or add [SerializeField] or [SerializeReference]; a static, const, readonly or [NonSerialized] field is never serialized.",
        helpLinkUri: TypeSelectorDocs + "#supported-fields");

    public static readonly Descriptor ProfilerMarkerUnsupportedTypeRule = new(
        id: "AFT0010",
        title: "this.Marker() call the profiler-marker generator cannot support",
        messageFormat: "this.Marker() opens no profiler marker: {0}",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The generator creates a marker only for a plain this.Marker() call on an instance of the internal or public type it is written in. Change the call as the message says, or use a hand-written ProfilerMarker.",
        helpLinkUri: ProfilerMarkersDocs + "#limitations");

    public static readonly Descriptor ProfilerMarkerScopeDiscardedRule = new(
        id: "AFT0011",
        title: "this.Marker() scope is never disposed",
        messageFormat: "The scope of this.Marker() is discarded, so the sample it begins never ends — wrap the measured code in 'using (this.Marker()) { … }', or declare 'using var scope = this.Marker();' under a name this scope does not use yet",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The scope ends the sample when it is disposed. Dispose it with a using statement or a using declaration.",
        helpLinkUri: ProfilerMarkersDocs + "#limitations");

    public static readonly Descriptor ProfilerMarkerScopeSuspendedRule = new(
        id: "AFT0013",
        title: "this.Marker() scope stays open across yield return or await",
        messageFormat: "The scope of this.Marker() stays open across '{0}': the method resumes later, possibly in another frame or on another thread, so the profiler sample is left unbalanced — end the scope before '{0}'",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "A coroutine or an async method resumes after yield return or await in a later frame or on another thread, where the sample cannot end. Close the using before yield return or await, and open a new marker after it.",
        helpLinkUri: ProfilerMarkersDocs + "#limitations");

    public static readonly Descriptor ProfilerMarkerNameNotConstantRule = new(
        id: "AFT0014",
        title: "WithName() name is not a compile-time constant",
        messageFormat: "WithName() is read at compile time, but this name is known only at run time, so the marker keeps the member name — pass a string literal, a const or nameof(...)",
        category: UsageCategory,
        defaultSeverity: Severity.Warning,
        isEnabledByDefault: true,
        description: "The generator reads the WithName() argument from the source. Pass a string literal, a const, nameof(...) or a concatenation of them; for a name computed at run time, use a hand-written ProfilerMarker.",
        helpLinkUri: ProfilerMarkersDocs + "#withname");
}
