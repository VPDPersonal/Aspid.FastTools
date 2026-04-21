using System;

namespace Aspid.FastTools.Generators.IdStruct.Data;

public readonly struct ContainingTypeInfo(string name, string keyword) : IEquatable<ContainingTypeInfo>
{
    public readonly string Name = name;
    public readonly string Keyword = keyword;

    public bool Equals(ContainingTypeInfo other) =>
        Name == other.Name && Keyword == other.Keyword;

    public override bool Equals(object? obj) => 
        obj is ContainingTypeInfo other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Keyword.GetHashCode();
        }
    }
}
