using System;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

internal readonly struct TypeData : IEquatable<TypeData>
{
    public readonly string TypeKey;
    public readonly string TypeName;
    public readonly string? Namespace;
    public readonly string ContainingTypeChain;
    public readonly string FullyQualifiedDisplay;
    public readonly string TypeParamList;
    public readonly string ConstraintsClause;
    public readonly int Arity;

    public TypeData(
        string typeKey,
        string typeName,
        string? @namespace,
        string containingTypeChain,
        string fullyQualifiedDisplay,
        string typeParamList,
        string constraintsClause,
        int arity)
    {
        TypeKey = typeKey;
        TypeName = typeName;
        Namespace = @namespace;
        ContainingTypeChain = containingTypeChain;
        FullyQualifiedDisplay = fullyQualifiedDisplay;
        TypeParamList = typeParamList;
        ConstraintsClause = constraintsClause;
        Arity = arity;
    }

    public bool Equals(TypeData other) =>
        TypeKey == other.TypeKey
        && TypeName == other.TypeName
        && Namespace == other.Namespace
        && ContainingTypeChain == other.ContainingTypeChain
        && FullyQualifiedDisplay == other.FullyQualifiedDisplay
        && TypeParamList == other.TypeParamList
        && ConstraintsClause == other.ConstraintsClause
        && Arity == other.Arity;

    public override bool Equals(object? obj) => obj is TypeData other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = TypeKey.GetHashCode();
            hash = (hash * 397) ^ TypeName.GetHashCode();
            hash = (hash * 397) ^ (Namespace?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ ContainingTypeChain.GetHashCode();
            hash = (hash * 397) ^ FullyQualifiedDisplay.GetHashCode();
            hash = (hash * 397) ^ TypeParamList.GetHashCode();
            hash = (hash * 397) ^ ConstraintsClause.GetHashCode();
            hash = (hash * 397) ^ Arity;
            return hash;
        }
    }
}
