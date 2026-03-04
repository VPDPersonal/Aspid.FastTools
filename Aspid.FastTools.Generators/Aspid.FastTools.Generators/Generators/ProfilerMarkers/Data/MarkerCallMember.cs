using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

public readonly struct MarkerCallMember(
    IMethodSymbol methodSymbol,
    ImmutableArray<MarkerCall> markerCalls)
{
    public readonly IMethodSymbol MethodSymbol = methodSymbol;
    public readonly ImmutableArray<MarkerCall> MarkerCalls = markerCalls;
}