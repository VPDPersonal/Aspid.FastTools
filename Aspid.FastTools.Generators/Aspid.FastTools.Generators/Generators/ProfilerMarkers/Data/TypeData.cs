using System;

namespace Aspid.FastTools.Generators.ProfilerMarkers.Data;

internal readonly struct TypeData : IEquatable<TypeData>
{
    // Fully qualified name with type parameters: unique per type, including its arity.
    public readonly string TypeKey;
    public readonly string TypeName;
    public readonly string? Namespace;

    // Generated class name before de-duplication; containing types and arities are joined with '_'.
    public readonly string ClassName;
    public readonly string FullyQualifiedDisplay;
    public readonly string TypeParamList;

    // The type's own type parameter names, comma-separated and unescaped.
    public readonly string OwnTypeParameters;
    public readonly string ConstraintsClause;
    public readonly int Arity;
    public readonly bool IsValueType;

    // The generated code names the type, its containing types and its constraints, so it is marked
    // [Obsolete] when one of them is: only an obsolete context silences CS0612, CS0618 and CS0619.
    public readonly bool IsObsolete;

    public TypeData(
        string typeKey,
        string typeName,
        string? @namespace,
        string className,
        string fullyQualifiedDisplay,
        string typeParamList,
        string ownTypeParameters,
        string constraintsClause,
        int arity,
        bool isValueType,
        bool isObsolete)
    {
        TypeKey = typeKey;
        TypeName = typeName;
        Namespace = @namespace;
        ClassName = className;
        FullyQualifiedDisplay = fullyQualifiedDisplay;
        TypeParamList = typeParamList;
        OwnTypeParameters = ownTypeParameters;
        ConstraintsClause = constraintsClause;
        Arity = arity;
        IsValueType = isValueType;
        IsObsolete = isObsolete;
    }

    public bool Equals(TypeData other) =>
        TypeKey == other.TypeKey
        && TypeName == other.TypeName
        && Namespace == other.Namespace
        && ClassName == other.ClassName
        && FullyQualifiedDisplay == other.FullyQualifiedDisplay
        && TypeParamList == other.TypeParamList
        && OwnTypeParameters == other.OwnTypeParameters
        && ConstraintsClause == other.ConstraintsClause
        && Arity == other.Arity
        && IsValueType == other.IsValueType
        && IsObsolete == other.IsObsolete;

    public override bool Equals(object? obj) => obj is TypeData other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = TypeKey.GetHashCode();
            hash = (hash * 397) ^ TypeName.GetHashCode();
            hash = (hash * 397) ^ (Namespace?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ ClassName.GetHashCode();
            hash = (hash * 397) ^ FullyQualifiedDisplay.GetHashCode();
            hash = (hash * 397) ^ TypeParamList.GetHashCode();
            hash = (hash * 397) ^ OwnTypeParameters.GetHashCode();
            hash = (hash * 397) ^ ConstraintsClause.GetHashCode();
            hash = (hash * 397) ^ Arity;
            hash = (hash * 397) ^ (IsValueType ? 1 : 0);
            hash = (hash * 397) ^ (IsObsolete ? 1 : 0);
            return hash;
        }
    }
}
