using System.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.ProfilerMarkers.Data;
using Aspid.FastTools.Generators.ProfilerMarkers.Bodies;

namespace Aspid.FastTools.Generators.ProfilerMarkers;

[Generator(LanguageNames.CSharp)]
public class ProfilerMarkersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var callsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static markerCall => markerCall.HasValue)
            .Select(static (markerCall, _) => markerCall!.Value);

        var groupedByType = callsProvider.Collect();
        context.RegisterSourceOutput(groupedByType, GenerateCode);
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

    private static MarkerCall? Transform(GeneratorSyntaxContext context, CancellationToken _)
    {
        var node = context.Node;
        if (node is not InvocationExpressionSyntax invocation) return null;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression) return null;
        if (memberAccessExpression.Name is not IdentifierNameSyntax idName || idName.Identifier.ValueText is not "Marker") return null;
        if (context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart) is not IMethodSymbol enclosing) return null;

        var namedTypeSymbol = enclosing.ContainingType;
        if (namedTypeSymbol is null) return null;

        var markerName = ResolveMarkerName(enclosing);
        var markerValue = markerName;

        if (invocation.Parent is MemberAccessExpressionSyntax memberAccessExpressionWithName
            && memberAccessExpressionWithName.Name is IdentifierNameSyntax { Identifier.ValueText: "WithName" }
            && memberAccessExpressionWithName.Parent is InvocationExpressionSyntax invocationExpressionWithName
            && invocationExpressionWithName.ArgumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literalExpressionWithName)
        {
            markerValue = literalExpressionWithName.Token.ValueText;
        }

        var lineSpan = invocation.GetLocation().GetLineSpan();
        var lineNumber = lineSpan.StartLinePosition.Line + 1;

        return new MarkerCall(namedTypeSymbol, enclosing, lineNumber, markerName, markerValue);
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

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<MarkerCall> markerCalls)
    {
        if (markerCalls.Length is 0) return;
        ExtensionClassBody.GenerateCode(context, markerCalls);
    }
}
