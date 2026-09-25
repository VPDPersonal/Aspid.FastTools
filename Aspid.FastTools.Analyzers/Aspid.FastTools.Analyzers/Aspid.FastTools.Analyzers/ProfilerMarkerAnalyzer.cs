using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.ProfilerMarkers;
using Aspid.FastTools.Analyzers.Descriptions;

namespace Aspid.FastTools.Analyzers;

/// <summary>
/// Reports <c>this.Marker()</c> calls the profiler-marker source generator skips (AFT0010) — they bind to the
/// fallback and silently open no marker — and supported calls whose scope is discarded, so the sample never ends (AFT0011).
/// </summary>
/// <remarks>
/// Which calls are supported is decided by <see cref="MarkerCallRules"/>, the file the generator itself compiles.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProfilerMarkerAnalyzer : DiagnosticAnalyzer
{
    private const string MethodGroupReason = "it is used as a method group, so no call passes its line";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule, DiagnosticRules.ProfilerMarkerScopeDiscardedRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeMethodGroup, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier.ValueText: "Marker" } name } access)
            return;

        var model = context.SemanticModel;
        var ct = context.CancellationToken;

        // The package fallback or a generated overload; a call with the wrong arguments has it only as a candidate.
        var symbolInfo = model.GetSymbolInfo(invocation, ct);
        var method = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault(IsMarker);
        if (!IsMarker(method)) return;

        if (MarkerCallRules.FindEnclosingMember(model.GetEnclosingSymbol(invocation.SpanStart, ct)) is not { ContainingType: { } type })
            return;

        if (MarkerCallRules.GetUnsupportedReason(invocation, access, type, model, ct) is { } reason)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule, name.GetLocation(), reason));
            return;
        }

        if (IsDiscarded(WithNameChain(invocation), model, context))
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.ProfilerMarkerScopeDiscardedRule, name.GetLocation()));
    }

    // `Func<AutoScope> f = this.Marker` passes no line, so it reaches the fallback or another line's marker.
    private static void AnalyzeMethodGroup(SyntaxNodeAnalysisContext context)
    {
        var access = (MemberAccessExpressionSyntax)context.Node;
        if (access.Name is not IdentifierNameSyntax { Identifier.ValueText: "Marker" } name) return;
        if (access.Parent is InvocationExpressionSyntax invocation && invocation.Expression == access) return;

        if (context.SemanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol is not IMethodSymbol method || !IsMarker(method))
            return;

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule, name.GetLocation(), MethodGroupReason));
    }

    private static bool IsMarker(IMethodSymbol? method) =>
        method is { Name: "Marker" } && (MarkerCallRules.IsPackageMethod(method) || MarkerCallRules.IsGeneratedOverload(method));

    // this.Marker(), or this.Marker().WithName("...") when the name is chained on it.
    private static SyntaxNode WithNameChain(InvocationExpressionSyntax invocation)
    {
        SyntaxNode outer = invocation;
        while (outer.Parent is ParenthesizedExpressionSyntax paren)
            outer = paren;

        return outer.Parent is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "WithName", Parent: InvocationExpressionSyntax withName }
            ? withName
            : outer;
    }

    // A scope used as a statement or assigned to a discard is never disposed.
    private static bool IsDiscarded(SyntaxNode scope, SemanticModel model, SyntaxNodeAnalysisContext context)
    {
        var parent = model.GetOperation(scope, context.CancellationToken)?.Parent;
        while (parent is IConversionOperation or IParenthesizedOperation)
            parent = parent.Parent;

        return parent is IExpressionStatementOperation
            or ISimpleAssignmentOperation { Target: IDiscardOperation };
    }
}
