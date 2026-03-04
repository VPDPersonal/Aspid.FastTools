using Microsoft.CodeAnalysis;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

public readonly struct MarkerCall(
    INamedTypeSymbol namedTypeSymbol,
    IMethodSymbol methodSymbol,
    int line,
    string markerName,
    string markerValue)
{
    public readonly int Line = line;
    public readonly IMethodSymbol MethodSymbol = methodSymbol;
    public readonly INamedTypeSymbol NamedTypeSymbol = namedTypeSymbol;
    
    public readonly string MarkerName = markerName + "_line_" + line;
    public readonly string MarkerValue = $"{namedTypeSymbol.Name}.{markerValue} ({line})";
}