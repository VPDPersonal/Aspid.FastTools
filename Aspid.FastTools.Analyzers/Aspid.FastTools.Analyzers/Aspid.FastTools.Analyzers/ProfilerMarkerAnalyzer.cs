using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Analyzers.Descriptions;

namespace Aspid.FastTools.Analyzers;

/// <summary>
/// Reports <c>this.Marker()</c> calls the profiler-marker source generator skips. The generator emits the marker
/// overload into a top-level class, so it cannot serve a type that class cannot name; such calls bind to the
/// <c>object</c> fallback and silently open no marker.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProfilerMarkerAnalyzer : DiagnosticAnalyzer
{
    private const string TargetClassName = "ProfilerMarkerExtensionsForGenerator";

    private const string InaccessibleReason =
        "is private or protected, or nested in such a type, so the generated overload cannot see it — make it internal or public";

    private const string ShadowedTypeParameterReason =
        "reuses a type parameter name of a containing type, which the generated overload cannot declare twice — rename it";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier.ValueText: "Marker" } name })
            return;

        // Same gate as the generator: only Marker() declared on the global-namespace marker class.
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method) return;
        if (method.ContainingType is not { Name: TargetClassName } owner || !owner.ContainingNamespace.IsGlobalNamespace) return;

        if (FindEnclosingType(context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart, context.CancellationToken)) is not { } type)
            return;

        var reason = !IsVisibleToGeneratedClass(type) ? InaccessibleReason
            : !HasUniqueTypeParameterNames(type) ? ShadowedTypeParameterReason
            : null;

        if (reason is null) return;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticRules.ProfilerMarkerUnsupportedTypeRule, name.GetLocation(), type.ToDisplayString(), reason));
    }

    // The type owning the member the call sits in — lambdas and local functions report the type they are written in.
    private static INamedTypeSymbol? FindEnclosingType(ISymbol? enclosing)
    {
        for (var symbol = enclosing; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol is IMethodSymbol or IFieldSymbol or IPropertySymbol)
                return symbol.ContainingType;
        }

        return null;
    }

    // Mirrors the generator: its top-level class can only name a type the whole assembly sees.
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

    // Mirrors the generator: the generated Marker<...> takes the type parameters of the whole containing chain.
    private static bool HasUniqueTypeParameterNames(INamedTypeSymbol type)
    {
        var names = new HashSet<string>();
        for (var t = type; t is not null; t = t.ContainingType)
        {
            foreach (var tp in t.TypeParameters)
            {
                if (!names.Add(tp.Name))
                    return false;
            }
        }

        return true;
    }
}
