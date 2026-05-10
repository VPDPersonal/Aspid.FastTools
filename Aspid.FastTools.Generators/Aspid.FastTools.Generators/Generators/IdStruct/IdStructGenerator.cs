using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.IdStruct.Data;
using Aspid.FastTools.Generators.IdStruct.Bodies;

namespace Aspid.FastTools.Generators.IdStruct;

[Generator(LanguageNames.CSharp)]
internal sealed class IdStructGenerator : IIncrementalGenerator
{
    private const string IIdFullName = "Aspid.FastTools.Ids.IId";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static r => !r.IsEmpty);

        context.RegisterSourceOutput(provider, static (ctx, result) => Emit(ctx, result));
    }

    private static bool Predicate(SyntaxNode node, CancellationToken _)
    {
        if (node is not StructDeclarationSyntax structDecl) return false;
        return structDecl.BaseList is { Types.Count: > 0 };
    }

    private static IdStructResult Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var structDecl = (StructDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(structDecl, ct) is not INamedTypeSymbol symbol)
            return default;

        var iidInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(IIdFullName);
        if (iidInterface is null) return default;

        var implementsIId = false;
        foreach (var iface in symbol.AllInterfaces)
        {
            if (!SymbolEqualityComparer.Default.Equals(iface, iidInterface)) continue;
            implementsIId = true;
            break;
        }

        if (!implementsIId) return default;

        if (!structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return new IdStructResult(
                data: null,
                diagnostic: new DiagnosticInfo(
                    IdStructDiagnostics.NotPartial,
                    structDecl.Identifier.GetLocation(),
                    symbol.Name));
        }

        var conflicts = CollectMemberConflicts(symbol);
        if (conflicts.Count > 0)
        {
            return new IdStructResult(
                data: null,
                diagnostic: new DiagnosticInfo(
                    IdStructDiagnostics.MemberConflict,
                    structDecl.Identifier.GetLocation(),
                    symbol.Name,
                    string.Join(", ", conflicts)));
        }

        return new IdStructResult(new IdStructData(symbol), diagnostic: null);
    }

    private static List<string> CollectMemberConflicts(INamedTypeSymbol symbol)
    {
        var result = new List<string>();
        AddIfDeclared(symbol, "_id", result);
        AddIfDeclared(symbol, "Id", result);
        AddIfDeclared(symbol, "__stringId", result);
        return result;
    }

    private static void AddIfDeclared(INamedTypeSymbol symbol, string memberName, List<string> output)
    {
        foreach (var member in symbol.GetMembers(memberName))
        {
            if (member.Kind is not (SymbolKind.Field or SymbolKind.Property)) continue;
            output.Add(memberName);
            return;
        }
    }

    private static void Emit(SourceProductionContext context, IdStructResult result)
    {
        if (result.Diagnostic is { } diag)
            context.ReportDiagnostic(diag.ToDiagnostic());

        if (result.Data is { } data)
            IdStructBody.GenerateCode(context, data);
    }
}
