using System;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

internal readonly struct MarkerCall : IEquatable<MarkerCall>
{
    public readonly TypeData Type;
    public readonly string MethodKey;
    public readonly int Line;
    public readonly string MarkerName;
    public readonly string Label;

    public MarkerCall(
        TypeData type,
        string methodKey,
        int line,
        string markerName,
        string markerValue)
    {
        Type = type;
        MethodKey = methodKey;
        Line = line;
        MarkerName = markerName + "_Marker_Line_" + line;
        Label = markerValue;
    }

    public bool Equals(MarkerCall other) =>
        Type.Equals(other.Type)
        && MethodKey == other.MethodKey
        && Line == other.Line
        && MarkerName == other.MarkerName
        && Label == other.Label;

    public override bool Equals(object? obj) => obj is MarkerCall other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Type.GetHashCode();
            hash = (hash * 397) ^ MethodKey.GetHashCode();
            hash = (hash * 397) ^ Line;
            hash = (hash * 397) ^ MarkerName.GetHashCode();
            hash = (hash * 397) ^ Label.GetHashCode();
            return hash;
        }
    }
}
