using Microsoft.CodeAnalysis;

namespace Aspid.FastTools.Generators.IdStruct.Data;

internal static class IdStructDiagnostics
{
    private const string Category = "Aspid.FastTools.IdStruct";

    public static readonly DiagnosticDescriptor NotPartial = new(
        id: "AFID001",
        title: "IId struct must be partial",
        messageFormat: "Struct '{0}' implements 'Aspid.FastTools.Ids.IId' but is not declared partial; the generator cannot emit the required '_id' field and 'Id' property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MemberConflict = new(
        id: "AFID002",
        title: "Generated IId members already declared",
        messageFormat: "Struct '{0}' already declares member(s) that the IId generator would emit: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor? GetDescriptor(string id) => id switch
    {
        "AFID001" => NotPartial,
        "AFID002" => MemberConflict,
        _ => null,
    };
}
