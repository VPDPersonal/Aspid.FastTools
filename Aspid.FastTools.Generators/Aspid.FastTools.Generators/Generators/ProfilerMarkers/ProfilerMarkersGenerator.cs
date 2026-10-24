using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Aspid.FastTools.ProfilerMarkers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.ProfilerMarkers.Data;
using Aspid.FastTools.Generators.ProfilerMarkers.Bodies;

namespace Aspid.FastTools.Generators.ProfilerMarkers;

[Generator(LanguageNames.CSharp)]
internal sealed class ProfilerMarkersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var callsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static markerCall => markerCall.HasValue)
            .Select(static (markerCall, _) => markerCall!.Value);

        var collected = callsProvider.Collect();
        context.RegisterSourceOutput(collected, GenerateCode);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken _) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax
                {
                    Identifier.ValueText: "Marker"
                }
            }
        };

    private static MarkerCall? Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } invocation)
            return null;

        var model = context.SemanticModel;
        var receiverType = model.GetTypeInfo(access.Expression, ct).Type;

        if (GetMarker(model.GetSymbolInfo(invocation, ct), receiverType) is null)
            return null;

        if (MarkerCallRules.FindEnclosingMember(model.GetEnclosingSymbol(invocation.SpanStart, ct)) is not { ContainingType: { } type } member)
            return null;

        // Unsupported calls get no overload: they bind to the fallback and compile, and AFT0010 reports them.
        if (MarkerCallRules.GetUnsupportedReason(invocation, access, type, model, ct) is not null)
            return null;

        // A WithName() name known only at run time keeps the member name, and AFT0014 reports it.
        var markerName = ResolveMarkerName(member);

        var label = MarkerCallRules.FindWithNameArgument(invocation, model, ct) is { } nameArgument
            ? MarkerCallRules.GetConstantName(nameArgument, model, ct) ?? markerName
            : markerName;

        var line = GetCallerLine(invocation);

        return new MarkerCall(
            type: BuildTypeData(type),
            line: line,
            fieldName: $"{markerName}_Marker_Line_{line}",
            label: label,
            filePath: invocation.SyntaxTree.FilePath,
            column: GetCallerColumn(invocation));
    }

    // The Marker() the call binds to: the package fallback, or an overload another assembly generated —
    // extension lookup stops at the nearest namespace, so a base type's overload in the caller's namespace
    // hides the global fallback. A ref struct cannot be a type argument of the generic fallback, so its call
    // only has the fallback as a candidate; the generated overload is what makes it compile.
    private static IMethodSymbol? GetMarker(SymbolInfo symbolInfo, ITypeSymbol? receiverType)
    {
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            return MarkerCallRules.IsMarker(method)
                ? method
                : null;
        }

        if (receiverType is not { IsRefLikeType: true })
            return null;

        return symbolInfo.CandidateSymbols
            .OfType<IMethodSymbol>()
            .FirstOrDefault(MarkerCallRules.IsMarker);
    }

    // The line [CallerLineNumber] passes for an invocation: the mapped line of its opening parenthesis.
    private static int GetCallerLine(InvocationExpressionSyntax invocation) =>
        invocation.SyntaxTree.GetMappedLineSpan(invocation.ArgumentList.OpenParenToken.Span).StartLinePosition.Line + 1;

    private static int GetCallerColumn(InvocationExpressionSyntax invocation) =>
        invocation.SyntaxTree.GetMappedLineSpan(invocation.ArgumentList.OpenParenToken.Span).StartLinePosition.Character;

    private static TypeData BuildTypeData(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        // Arity is part of every type name so Foo, Foo<T> and Outer<T>.Inner stay distinct.
        var containing = new Stack<INamedTypeSymbol>();

        for (var t = symbol.ContainingType; t is not null; t = t.ContainingType)
            containing.Push(t);

        var nameBuilder = new StringBuilder("__");

        foreach (var t in containing)
            AppendNameWithArity(nameBuilder, t).Append('_');

        AppendNameWithArity(nameBuilder, symbol)
            .Append("ProfilerMarkerExtensions");

        var typeParameters = MarkerCallRules.GetChainTypeParameters(symbol);
        var typeParamList = typeParameters.Count > 0
            ? "<" + string.Join(", ", typeParameters.Select(static p => EscapeIdentifier(p.Name))) + ">"
            : string.Empty;

        return new TypeData(
            typeKey: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeName: symbol.Name,
            ns: ns,
            className: nameBuilder.ToString(),
            fullyQualifiedDisplay: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeParamList: typeParamList,
            ownTypeParameters: string.Join(",", symbol.TypeParameters.Select(static p => p.Name)),
            constraintsClause: BuildConstraintsClause(typeParameters),
            arity: symbol.Arity,
            isValueType: symbol.IsValueType,
            isObsolete: IsObsolete(symbol) || typeParameters.Any(static p => p.ConstraintTypes.Any(IsObsolete)));
    }

    // A type is obsolete when it, a containing type or a type in its name (type argument, array element) is.
    private static bool IsObsolete(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                return IsObsolete(array.ElementType);

            case IPointerTypeSymbol pointer:
                return IsObsolete(pointer.PointedAtType);

            case INamedTypeSymbol named:
                for (var t = named; t is not null; t = t.ContainingType)
                {
                    if (HasObsoleteAttribute(t) || t.TypeArguments.Any(IsObsolete))
                        return true;
                }

                return false;

            default:
                return false;
        }
    }

    private static bool HasObsoleteAttribute(ISymbol symbol) =>
        symbol
            .GetAttributes()
            .Any(static attribute =>
                attribute.AttributeClass is { Name: "ObsoleteAttribute" } attributeClass
                && attributeClass.ContainingNamespace.ToDisplayString() == "System");

    private static StringBuilder AppendNameWithArity(StringBuilder builder, INamedTypeSymbol type)
    {
        builder.Append(type.Name);

        if (type.Arity > 0)
        {
            builder
                .Append('_')
                .Append(type.Arity);
        }

        return builder;
    }

    private static string ResolveMarkerName(ISymbol member)
    {
        var name = member switch
        {
            IMethodSymbol method => ResolveMethodName(method),
            IFieldSymbol { AssociatedSymbol: IPropertySymbol property } => ResolvePropertyName(property),
            IFieldSymbol { AssociatedSymbol: IEventSymbol @event } => ResolveEventName(@event),
            IPropertySymbol property => ResolvePropertyName(property),
            IEventSymbol @event => ResolveEventName(@event),
            _ => member.Name,
        };

        return ToIdentifier(name);
    }

    private static string ResolveMethodName(IMethodSymbol method) =>
        method.AssociatedSymbol switch
        {
            IPropertySymbol property => ResolvePropertyName(property),
            IEventSymbol @event => ResolveEventName(@event),
            _ => method.MethodKind switch
            {
                MethodKind.Constructor => "Ctor",
                MethodKind.StaticConstructor => "StaticCtor",
                _ => method.ExplicitInterfaceImplementations.Length > 0
                    ? method.ExplicitInterfaceImplementations[0].Name
                    : method.Name
            }
        };

    private static string ResolvePropertyName(IPropertySymbol property)
    {
        // An indexer's name is "this[]", which is not a valid identifier for the generated field.
        if (property.IsIndexer)
            return "Indexer";

        return property.ExplicitInterfaceImplementations.Length > 0
            ? property.ExplicitInterfaceImplementations[0].Name
            : property.Name;
    }

    private static string ResolveEventName(IEventSymbol @event) =>
        @event.ExplicitInterfaceImplementations.Length > 0
            ? @event.ExplicitInterfaceImplementations[0].Name
            : @event.Name;

    // The name starts the generated field name, so anything that is not an identifier character becomes '_'.
    private static string ToIdentifier(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var c in name)
        {
            builder.Append(SyntaxFacts.IsIdentifierPartCharacter(c)
                ? c
                : '_');
        }

        if (builder.Length is 0 || !SyntaxFacts.IsIdentifierStartCharacter(builder[0]))
            builder.Insert(0, '_');

        return builder.ToString();
    }

    // A type parameter may be named with a keyword (class Foo<@event>); the generated code must escape it again.
    internal static string EscapeIdentifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) is SyntaxKind.None
            ? name
            : "@" + name;

    private static string BuildConstraintsClause(IReadOnlyList<ITypeParameterSymbol> typeParameters)
    {
        if (typeParameters.Count is 0)
            return string.Empty;

        var clauses = new List<string>();

        foreach (var tp in typeParameters)
        {
            var constraints = new List<string>();

            if (tp.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (tp.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");
            else if (tp.HasValueTypeConstraint)
                constraints.Add("struct");

            if (tp.HasNotNullConstraint)
                constraints.Add("notnull");

            foreach (var ct in tp.ConstraintTypes)
                constraints.Add(ct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            if (tp.HasConstructorConstraint)
                constraints.Add("new()");

            if (constraints.Count > 0)
                clauses.Add($"where {EscapeIdentifier(tp.Name)} : {string.Join(", ", constraints)}");
        }

        return clauses.Count is 0
            ? string.Empty
            : " " + string.Join(" ", clauses);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<MarkerCall> markerCalls)
    {
        if (markerCalls.Length is 0)
            return;

        ExtensionClassBody.GenerateCode(context, markerCalls);
    }
}
