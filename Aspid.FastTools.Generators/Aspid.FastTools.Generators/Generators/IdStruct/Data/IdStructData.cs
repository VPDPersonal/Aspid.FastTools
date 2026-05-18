using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Aspid.FastTools.Generators.IdStruct.Data;

internal readonly struct IdStructData : IEquatable<IdStructData>
{
    public readonly string StructName;
    public readonly string TypeParameters;
    public readonly int Arity;
    public readonly string? Namespace;
    public readonly ImmutableArray<ContainingTypeInfo> ContainingTypes;

    public IdStructData(INamedTypeSymbol symbol)
    {
        StructName = symbol.Name;

        var typeParams = symbol.TypeParameters;
        Arity = typeParams.Length;
        TypeParameters = Arity > 0
            ? "<" + string.Join(", ", typeParams.Select(p => p.Name)) + ">"
            : string.Empty;

        Namespace = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        if (symbol.ContainingType is null)
        {
            ContainingTypes = ImmutableArray<ContainingTypeInfo>.Empty;
            return;
        }

        var builder = ImmutableArray.CreateBuilder<ContainingTypeInfo>();
        var current = symbol.ContainingType;
        while (current is not null)
        {
            builder.Add(MakeContainingInfo(current));
            current = current.ContainingType;
        }

        builder.Reverse();
        ContainingTypes = builder.ToImmutable();
    }

    private static ContainingTypeInfo MakeContainingInfo(INamedTypeSymbol symbol)
    {
        var typeParams = symbol.TypeParameters;
        var typeParamList = typeParams.Length > 0
            ? "<" + string.Join(", ", typeParams.Select(p => p.Name)) + ">"
            : string.Empty;
        return new ContainingTypeInfo(symbol.Name, GetKeyword(symbol), typeParamList, typeParams.Length);
    }

    private static string GetKeyword(INamedTypeSymbol symbol)
    {
        if (symbol.IsRecord)
            return symbol.IsValueType ? "record struct" : "record";

        return symbol.TypeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            _ => "class",
        };
    }

    public bool Equals(IdStructData other)
    {
        if (StructName != other.StructName) return false;
        if (TypeParameters != other.TypeParameters) return false;
        if (Arity != other.Arity) return false;
        if (Namespace != other.Namespace) return false;
        if (ContainingTypes.Length != other.ContainingTypes.Length) return false;

        for (var i = 0; i < ContainingTypes.Length; i++)
        {
            if (!ContainingTypes[i].Equals(other.ContainingTypes[i])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) =>
        obj is IdStructData other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = StructName.GetHashCode();
            hash = (hash * 397) ^ TypeParameters.GetHashCode();
            hash = (hash * 397) ^ Arity;
            hash = (hash * 397) ^ (Namespace?.GetHashCode() ?? 0);

            foreach (var ct in ContainingTypes)
                hash = (hash * 397) ^ ct.GetHashCode();

            return hash;
        }
    }
}
