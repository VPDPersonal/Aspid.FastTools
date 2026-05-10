using System;

namespace Aspid.FastTools.Generators.IdStruct.Data;

internal readonly struct ContainingTypeInfo : IEquatable<ContainingTypeInfo>
{
    public readonly string Name;
    public readonly string Keyword;
    public readonly string TypeParameters;
    public readonly int Arity;

    public ContainingTypeInfo(string name, string keyword, string typeParameters, int arity)
    {
        Name = name;
        Keyword = keyword;
        TypeParameters = typeParameters;
        Arity = arity;
    }

    public bool Equals(ContainingTypeInfo other) =>
        Name == other.Name
        && Keyword == other.Keyword
        && TypeParameters == other.TypeParameters
        && Arity == other.Arity;

    public override bool Equals(object? obj) =>
        obj is ContainingTypeInfo other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Name.GetHashCode();
            hash = (hash * 397) ^ Keyword.GetHashCode();
            hash = (hash * 397) ^ TypeParameters.GetHashCode();
            hash = (hash * 397) ^ Arity;
            return hash;
        }
    }
}
