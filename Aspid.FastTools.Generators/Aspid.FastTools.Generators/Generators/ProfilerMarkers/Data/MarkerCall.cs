using System;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

internal readonly struct MarkerCall : IEquatable<MarkerCall>
{
    public readonly TypeData Type;
    public readonly int Line;
    public readonly string FieldName;
    public readonly string Label;

    // Where the call is, so calls sharing a line are ordered the same way on every run.
    public readonly string FilePath;
    public readonly int Position;

    public MarkerCall(
        TypeData type,
        int line,
        string fieldName,
        string label,
        string filePath,
        int position)
    {
        Type = type;
        Line = line;
        FieldName = fieldName;
        Label = label;
        FilePath = filePath;
        Position = position;
    }

    public bool Equals(MarkerCall other) =>
        Type.Equals(other.Type)
        && Line == other.Line
        && FieldName == other.FieldName
        && Label == other.Label
        && FilePath == other.FilePath
        && Position == other.Position;

    public override bool Equals(object? obj) => obj is MarkerCall other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Type.GetHashCode();
            hash = (hash * 397) ^ Line;
            hash = (hash * 397) ^ FieldName.GetHashCode();
            hash = (hash * 397) ^ Label.GetHashCode();
            hash = (hash * 397) ^ FilePath.GetHashCode();
            hash = (hash * 397) ^ Position;
            return hash;
        }
    }
}
