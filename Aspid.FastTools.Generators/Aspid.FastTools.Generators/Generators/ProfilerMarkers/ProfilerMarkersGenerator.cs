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
                    return (property.ContainingType, property.Name, property.ToDisplayString());

                default:
                    enclosing = enclosing.ContainingSymbol;
                    continue;
            }
        }

        return null;
    }

    private static TypeData BuildTypeData(INamedTypeSymbol symbol)
    {
        var typeName = symbol.Name;
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        var containingChain = string.Empty;
        if (symbol.ContainingType is not null)
        {
            var stack = new Stack<string>();
            for (var t = symbol.ContainingType; t is not null; t = t.ContainingType)
                stack.Push(t.Name);

            var sb = new System.Text.StringBuilder();
            foreach (var name in stack)
                sb.Append(name).Append('.');
            containingChain = sb.ToString();
        }

        var typeKey = (ns is null ? string.Empty : ns + ".") + containingChain + typeName;

        var typeParameters = symbol.TypeParameters;
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
            return property.ExplicitInterfaceImplementations.Length > 0
                ? property.ExplicitInterfaceImplementations[0].Name
                : property.Name;
        }

        if (enclosing.MethodKind is MethodKind.Constructor)
            return "Ctor";

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
