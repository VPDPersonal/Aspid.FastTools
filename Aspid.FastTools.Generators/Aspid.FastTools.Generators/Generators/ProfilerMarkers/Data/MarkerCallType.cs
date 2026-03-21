using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

public readonly struct MarkerCallType(
    INamedTypeSymbol symbol,
    ImmutableArray<MarkerCall> markerCalls)
{
    public readonly INamedTypeSymbol Symbol = symbol;
    public readonly ImmutableArray<MarkerCall> MarkerCalls = markerCalls;
}