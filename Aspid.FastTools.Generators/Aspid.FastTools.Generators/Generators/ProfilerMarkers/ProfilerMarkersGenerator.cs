using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.ProfilerMarkers.Data;
using Aspid.FastTools.Generators.ProfilerMarkers.Bodies;

namespace Aspid.FastTools.Generators.ProfilerMarkers;

[Generator(LanguageNames.CSharp)]
internal sealed class ProfilerMarkersGenerator : IIncrementalGenerator
{
    private const string TargetClassName = "ProfilerMarkerExtensionsForGenerator";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var callsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static markerCall => markerCall.HasValue)
            .Select(static (markerCall, _) => markerCall!.Value);

        var collected = callsProvider.Collect();
        context.RegisterSourceOutput(collected, GenerateCode);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken _)
    {
        if (node is not InvocationExpressionSyntax invocation) return false;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression) return false;

        return memberAccessExpression.Name is IdentifierNameSyntax
        {
            Identifier.ValueText: "Marker"
        };
    }

    private static MarkerCall? Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var node = context.Node;
        if (node is not InvocationExpressionSyntax invocation) return null;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression) return null;
        if (memberAccessExpression.Name is not IdentifierNameSyntax idName || idName.Identifier.ValueText is not "Marker") return null;

        // Semantic gate: only match Marker() declared on the global-namespace ProfilerMarkerExtensionsForGenerator class.
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol invokedMethod) return null;
        var owningType = invokedMethod.ContainingType;
        if (owningType is null) return null;
        if (owningType.Name != TargetClassName) return null;
        if (!owningType.ContainingNamespace.IsGlobalNamespace) return null;

        var initialEnclosing = context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart, ct);
        if (ResolveEnclosingMember(initialEnclosing) is not { } enclosingInfo) return null;
        var (namedTypeSymbol, markerName, methodKey) = enclosingInfo;

        // Unsupported types get no overload: the call binds to the object fallback and compiles,
        // and the analyzer (AFT0010) reports the missing marker instead.
        if (!IsVisibleToGeneratedClass(namedTypeSymbol)) return null;
        if (!HasUniqueTypeParameterNames(namedTypeSymbol)) return null;

        var markerValue = markerName;

        // Walk past any parentheses so `(this.Marker()).WithName("x")` is still recognised.
        SyntaxNode outer = invocation;
        while (outer.Parent is ParenthesizedExpressionSyntax paren)
            outer = paren;

        if (outer.Parent is MemberAccessExpressionSyntax memberAccessExpressionWithName
            && memberAccessExpressionWithName.Name is IdentifierNameSyntax { Identifier.ValueText: "WithName" }
            && memberAccessExpressionWithName.Parent is InvocationExpressionSyntax invocationExpressionWithName
            && invocationExpressionWithName.ArgumentList.Arguments.FirstOrDefault()?.Expression is { } argExpr
            && TryExtractStringLiteral(argExpr) is { } extracted)
        {
            markerValue = extracted;
        }

        var lineSpan = invocation.GetLocation().GetLineSpan();
        var lineNumber = lineSpan.StartLinePosition.Line + 1;

        var typeData = BuildTypeData(namedTypeSymbol);

        return new MarkerCall(typeData, methodKey, lineNumber, markerName, markerValue);
    }

    private static (INamedTypeSymbol Type, string MarkerName, string MethodKey)? ResolveEnclosingMember(ISymbol? enclosing)
    {
        // Walk past synthesized symbols (lambdas, local functions, anonymous methods)
        // until we find a real declared member that owns the call site.
        while (enclosing is not null)
        {
            switch (enclosing)
            {
                case IMethodSymbol method:
                    if (method.MethodKind is MethodKind.LambdaMethod
                        or MethodKind.AnonymousFunction
                        or MethodKind.LocalFunction)
                    {
                        enclosing = method.ContainingSymbol;
                        continue;
                    }
                    if (method.ContainingType is null) return null;
                    return (method.ContainingType, ResolveMarkerName(method), method.ToDisplayString());

                case IFieldSymbol field:
                    if (field.ContainingType is null) return null;
                    return (field.ContainingType, field.Name, field.ToDisplayString());

                case IPropertySymbol property:
                    if (property.ContainingType is null) return null;
                    return (property.ContainingType, property.IsIndexer ? "Indexer" : property.Name, property.ToDisplayString());

                default:
                    enclosing = enclosing.ContainingSymbol;
                    continue;
            }
        }

        return null;
    }

    // The generated extension class is top-level, so it can only name a type the whole assembly sees.
    private static bool IsVisibleToGeneratedClass(INamedTypeSymbol symbol)
    {
        for (var t = symbol; t is not null; t = t.ContainingType)
        {
            if (t.DeclaredAccessibility is Accessibility.Private
                or Accessibility.Protected
                or Accessibility.ProtectedAndInternal)
                return false;
        }

        return true;
    }

    // The generated Marker<...> method takes the type parameters of the whole containing chain,
    // so a nested type parameter that shadows an outer one (CS0693) would be declared twice.
    private static bool HasUniqueTypeParameterNames(INamedTypeSymbol symbol)
    {
        var names = new HashSet<string>();
        foreach (var tp in GetChainTypeParameters(symbol))
        {
            if (!names.Add(tp.Name))
                return false;
        }

        return true;
    }

    // Type parameters of the type and all its containing types, outermost first.
    private static ImmutableArray<ITypeParameterSymbol> GetChainTypeParameters(INamedTypeSymbol symbol)
    {
        var stack = new Stack<INamedTypeSymbol>();
        for (var t = symbol; t is not null; t = t.ContainingType)
            stack.Push(t);

        return stack.SelectMany(t => t.TypeParameters).ToImmutableArray();
    }

    private static TypeData BuildTypeData(INamedTypeSymbol symbol)
    {
        var typeName = symbol.Name;
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        // Arity is part of each containing type's name so Outer.Inner and Outer<T>.Inner stay distinct.
        var containingChain = string.Empty;
        if (symbol.ContainingType is not null)
        {
            var stack = new Stack<INamedTypeSymbol>();
            for (var t = symbol.ContainingType; t is not null; t = t.ContainingType)
                stack.Push(t);

            var sb = new System.Text.StringBuilder();
            foreach (var t in stack)
            {
                sb.Append(t.Name);
                if (t.Arity > 0) sb.Append('_').Append(t.Arity);
                sb.Append('.');
            }
            containingChain = sb.ToString();
        }

        var typeKey = (ns is null ? string.Empty : ns + ".") + containingChain + typeName;

        var typeParameters = GetChainTypeParameters(symbol);
        var isGeneric = typeParameters.Length > 0;
        var typeParamList = isGeneric
            ? "<" + string.Join(", ", typeParameters.Select(p => p.Name)) + ">"
            : string.Empty;
        var constraintsClause = BuildConstraintsClause(typeParameters);

        var fullyQualifiedDisplay = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new TypeData(
            typeKey: typeKey,
            typeName: typeName,
            @namespace: ns,
            containingTypeChain: containingChain,
            fullyQualifiedDisplay: fullyQualifiedDisplay,
            typeParamList: typeParamList,
            constraintsClause: constraintsClause,
            arity: symbol.Arity);
    }

    private static string? TryExtractStringLiteral(ExpressionSyntax expr)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit when lit.Token.IsKind(SyntaxKind.StringLiteralToken):
                return lit.Token.ValueText;

            case InterpolatedStringExpressionSyntax interp:
            {
                var sb = new StringBuilder();
                foreach (var content in interp.Contents)
                {
                    if (content is not InterpolatedStringTextSyntax text) return null;
                    sb.Append(text.TextToken.ValueText);
                }
                return sb.ToString();
            }
        }

        return null;
    }

    private static string ResolveMarkerName(IMethodSymbol enclosing)
    {
        if (enclosing.AssociatedSymbol is IPropertySymbol property)
        {
            // An indexer's name is "this[]", which is not a valid identifier for the generated field.
            if (property.IsIndexer)
                return "Indexer";

            return property.ExplicitInterfaceImplementations.Length > 0
                ? property.ExplicitInterfaceImplementations[0].Name
                : property.Name;
        }

        if (enclosing.AssociatedSymbol is IEventSymbol @event)
        {
            return @event.ExplicitInterfaceImplementations.Length > 0
                ? @event.ExplicitInterfaceImplementations[0].Name
                : @event.Name;
        }

        if (enclosing.MethodKind is MethodKind.Constructor)
            return "Ctor";

        if (enclosing.MethodKind is MethodKind.StaticConstructor)
            return "StaticCtor";

        return enclosing.ExplicitInterfaceImplementations.Length > 0
            ? enclosing.ExplicitInterfaceImplementations[0].Name
            : enclosing.Name;
    }

    private static string BuildConstraintsClause(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        if (typeParameters.Length is 0) return string.Empty;

        var clauses = new List<string>();
        foreach (var tp in typeParameters)
        {
            var constraints = new List<string>();

            if (tp.HasReferenceTypeConstraint) constraints.Add("class");
            else if (tp.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
            else if (tp.HasValueTypeConstraint) constraints.Add("struct");

            if (tp.HasNotNullConstraint) constraints.Add("notnull");

            foreach (var ct in tp.ConstraintTypes)
                constraints.Add(ct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            if (tp.HasConstructorConstraint) constraints.Add("new()");

            if (constraints.Count > 0)
                clauses.Add($"where {tp.Name} : {string.Join(", ", constraints)}");
        }

        return clauses.Count is 0 ? string.Empty : " " + string.Join(" ", clauses);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<MarkerCall> markerCalls)
    {
        if (markerCalls.Length is 0) return;
        ExtensionClassBody.GenerateCode(context, markerCalls);
    }
}
