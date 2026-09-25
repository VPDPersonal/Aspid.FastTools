using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.ProfilerMarkers;

// Decides which Marker() calls get a marker. The generator compiles this file and skips every other call;
// the analyzer links the same file (Aspid.FastTools.Analyzers.csproj) and reports those calls as AFT0010,
// so the two cannot disagree about a call.
internal static class MarkerCallRules
{
    public const string ExtensionsClassName = "ProfilerMarkerExtensionsForGenerator";
    public const string GeneratorName = "Aspid.FastTools.Generators.ProfilerMarkersGenerator";

    // Marker() or WithName() declared on the package's global-namespace class.
    public static bool IsPackageMethod(IMethodSymbol? method) =>
        method?.ContainingType is { Name: ExtensionsClassName } type && type.ContainingNamespace.IsGlobalNamespace;

    // A Marker() overload the generator emitted: its class carries [GeneratedCode] with the generator's name.
    public static bool IsGeneratedOverload(IMethodSymbol? method) =>
        method is { Name: "Marker" }
        && method.ContainingType.GetAttributes().Any(static attribute =>
            attribute.AttributeClass?.Name is "GeneratedCodeAttribute"
            && attribute.ConstructorArguments.Length > 0
            && attribute.ConstructorArguments[0].Value as string == GeneratorName);

    // The Marker() the call binds to: the package fallback, or an overload another assembly generated —
    // extension lookup stops at the nearest namespace, so a base type's overload in the caller's namespace
    // hides the global fallback. A ref struct cannot be a type argument of the generic fallback, so its call
    // only has the fallback as a candidate; the generated overload is what makes it compile.
    public static IMethodSymbol? GetMarker(SymbolInfo symbolInfo, ITypeSymbol? receiverType)
    {
        if (symbolInfo.Symbol is IMethodSymbol method)
            return IsMarker(method) ? method : null;

        if (receiverType is not { IsRefLikeType: true }) return null;

        return symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault(IsMarker);
    }

    public static bool IsMarker(IMethodSymbol? method) =>
        method is { Name: "Marker" } && (IsPackageMethod(method) || IsGeneratedOverload(method));

    // The member that owns the call: lambdas and local functions belong to the member they are written in.
    public static ISymbol? FindEnclosingMember(ISymbol? enclosing)
    {
        for (var symbol = enclosing; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            switch (symbol)
            {
                case IMethodSymbol { MethodKind: MethodKind.LambdaMethod or MethodKind.AnonymousFunction or MethodKind.LocalFunction }:
                    continue;

                case IMethodSymbol or IFieldSymbol or IPropertySymbol or IEventSymbol:
                    return symbol.ContainingType is null ? null : symbol;

                case ITypeSymbol or INamespaceSymbol:
                    return null;
            }
        }

        return null;
    }

    // The line [CallerLineNumber] passes for an invocation: the mapped line of its opening parenthesis.
    public static int GetCallerLine(InvocationExpressionSyntax invocation) =>
        invocation.SyntaxTree.GetMappedLineSpan(invocation.ArgumentList.OpenParenToken.Span).StartLinePosition.Line + 1;

    public static int GetCallerColumn(InvocationExpressionSyntax invocation) =>
        invocation.SyntaxTree.GetMappedLineSpan(invocation.ArgumentList.OpenParenToken.Span).StartLinePosition.Character;

    // Why a Marker() call written in a member of type gets no marker, or null when it gets one.
    public static string? GetUnsupportedReason(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax access,
        INamedTypeSymbol type,
        SemanticModel model,
        CancellationToken ct)
    {
        if (model.GetSymbolInfo(access.Expression, ct).Symbol is ITypeSymbol)
            return "it is called as a static method — call it as an extension method: this.Marker()";

        if (access.Name is GenericNameSyntax)
            return "it passes type arguments — call Marker() without them";

        if (invocation.ArgumentList.Arguments.Count > 0)
            return "it passes an argument, but the line must come from [CallerLineNumber] — call Marker() without arguments";

        var receiverType = model.GetTypeInfo(access.Expression, ct).Type;
        if (receiverType is null || !SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, type.OriginalDefinition))
        {
            return $"it is called on '{receiverType?.ToDisplayString() ?? access.Expression.ToString()}', not on '{type.ToDisplayString()}' "
                + "— only calls on an instance of the type they are written in get a marker";
        }

        if (IsInExpressionTree(invocation, model, ct))
            return "it is inside an expression tree";

        return GetUnsupportedTypeReason(type);
    }

    public static string? GetUnsupportedTypeReason(INamedTypeSymbol type)
    {
        if (type.TypeKind is TypeKind.Interface)
            return $"'{type.ToDisplayString()}' is an interface — call it from the implementing type";

        if (!IsVisibleToGeneratedClass(type))
        {
            return $"'{type.ToDisplayString()}' is private or protected, or nested in such a type, "
                + "so the generated overload cannot see it — make it internal or public";
        }

        if (!HasUniqueTypeParameterNames(type))
        {
            return $"'{type.ToDisplayString()}' reuses a type parameter name of a containing type, "
                + "which the generated overload cannot declare twice — rename it";
        }

        return null;
    }

    // Type parameters of the type and all its containing types, outermost first.
    public static IReadOnlyList<ITypeParameterSymbol> GetChainTypeParameters(INamedTypeSymbol type)
    {
        var chain = new Stack<INamedTypeSymbol>();
        for (var t = type; t is not null; t = t.ContainingType)
            chain.Push(t);

        return chain.SelectMany(static t => t.TypeParameters).ToArray();
    }

    // The generated extension class is top-level, so it can only name a type the whole assembly sees.
    private static bool IsVisibleToGeneratedClass(INamedTypeSymbol type)
    {
        for (var t = type; t is not null; t = t.ContainingType)
        {
            if (t.DeclaredAccessibility is Accessibility.Private
                or Accessibility.Protected
                or Accessibility.ProtectedAndInternal)
                return false;
        }

        return true;
    }

    // The generated Marker<...> takes the type parameters of the whole containing chain,
    // so a nested type parameter that shadows an outer one (CS0693) would be declared twice.
    private static bool HasUniqueTypeParameterNames(INamedTypeSymbol type)
    {
        var names = new HashSet<string>();
        return GetChainTypeParameters(type).All(tp => names.Add(tp.Name));
    }

    // An expression tree cannot hold the generated call: it has an optional argument (CS0854).
    private static bool IsInExpressionTree(SyntaxNode node, SemanticModel model, CancellationToken ct)
    {
        for (var operation = model.GetOperation(node, ct); operation is not null; operation = operation.Parent)
        {
            if (operation is IAnonymousFunctionOperation && IsExpressionTree(operation.Parent?.Type))
                return true;
        }

        return false;
    }

    private static bool IsExpressionTree(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "Expression", Arity: 1 } named
        && named.ContainingNamespace.ToDisplayString() == "System.Linq.Expressions";
}
