using Microsoft.CodeAnalysis;

namespace Aspid.FastTools.Generators.IdStruct.Data;

public readonly struct IdStructData(INamedTypeSymbol symbol)
{
    public readonly INamedTypeSymbol Symbol = symbol;
    public readonly string StructName = symbol.Name;
    public readonly string? Namespace = symbol.ContainingNamespace.IsGlobalNamespace
        ? null
        : symbol.ContainingNamespace.ToDisplayString();
}
