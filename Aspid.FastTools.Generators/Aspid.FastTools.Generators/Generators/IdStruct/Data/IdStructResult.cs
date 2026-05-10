using System;

namespace Aspid.FastTools.Generators.IdStruct.Data;

internal readonly struct IdStructResult : IEquatable<IdStructResult>
{
    public readonly IdStructData? Data;
    public readonly DiagnosticInfo? Diagnostic;

    public IdStructResult(IdStructData? data, DiagnosticInfo? diagnostic)
    {
        Data = data;
        Diagnostic = diagnostic;
    }

    public bool IsEmpty => Data is null && Diagnostic is null;

    public bool Equals(IdStructResult other) =>
        Nullable.Equals(Data, other.Data) && Nullable.Equals(Diagnostic, other.Diagnostic);

    public override bool Equals(object? obj) => obj is IdStructResult other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Data?.GetHashCode() ?? 0;
            hash = (hash * 397) ^ (Diagnostic?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
