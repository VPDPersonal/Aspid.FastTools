using System;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

internal readonly struct MarkerCall(
    TypeData type,
    int line,
    string fieldName,
    string label,
    string filePath,
    int column)
    : IEquatable<MarkerCall>
{
    public readonly TypeData Type = type;
    public readonly int Line = line;
    public readonly string FieldName = fieldName;
    public readonly string Label = label;

    // Where the call is, so calls sharing a line are ordered the same way on every run. The column,
    // not the offset: an edit above the call must not change the model while the output stays the same.
    public readonly string FilePath = filePath;
    public readonly int Column = column;

    public bool Equals(MarkerCall other) =>
        Type.Equals(other.Type)
        && Line == other.Line
        && FieldName == other.FieldName
        && Label == other.Label
        && FilePath == other.FilePath
        && Column == other.Column;

    public override bool Equals(object? obj) =>
        obj is MarkerCall other
        && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Type.GetHashCode();
            hash = (hash * 397) ^ Line;
            hash = (hash * 397) ^ FieldName.GetHashCode();
            hash = (hash * 397) ^ Label.GetHashCode();
            hash = (hash * 397) ^ FilePath.GetHashCode();
            hash = (hash * 397) ^ Column;
            return hash;
        }
    }
}
