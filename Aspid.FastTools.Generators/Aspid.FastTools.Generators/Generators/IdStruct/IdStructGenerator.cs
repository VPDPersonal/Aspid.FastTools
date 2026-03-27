using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.IdStruct.Data;
using Aspid.FastTools.Generators.IdStruct.Bodies;

namespace Aspid.FastTools.Generators.IdStruct;

[Generator(LanguageNames.CSharp)]
public class IdStructGenerator : IIncrementalGenerator
{
    private const string IIdFullName = "Aspid.FastTools.IId";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static d => d.HasValue)
            .Select(static (d, _) => d!.Value);

        context.RegisterSourceOutput(provider, static (ctx, data) => IdStructBody.GenerateCode(ctx, data));
    }

    private static bool Predicate(SyntaxNode node, CancellationToken _)
    {
        if (node is not StructDeclarationSyntax structDecl) return false;
        if (!structDecl.Modifiers.Any(SyntaxKind.PartialKeyword)) return false;
        return structDecl.BaseList is { Types.Count: > 0 };
    }

    private static IdStructData? Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var structDecl = (StructDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(structDecl, ct) is not INamedTypeSymbol symbol)
            return null;

        var iidInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(IIdFullName);
        if (iidInterface == null) return null;

        foreach (var iface in symbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, iidInterface))
                return new IdStructData(symbol);
        }

        return null;
    }
}
