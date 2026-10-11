using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

// A Burst job is a struct, and this.Marker() inside it is compiled by Burst. It rejects managed code there, and the
// user sees the error (BC1025 for a managed static field, BC1360 for a managed array or Type) in their own job.
// Burst itself is checked by the Burst tests of the Unity package; these tests keep the generated shape it accepts.
public class ProfilerMarkersBurstShapeTests
{
    [Theory]
    [InlineData("public struct Job", "")]
    [InlineData("public readonly struct Job", "")]
    [InlineData("public struct Job<T> where T : unmanaged", "")]
    [InlineData("public struct Job<T, U> where T : unmanaged where U : unmanaged", "")]
    [InlineData("public static class Host { public struct Job", "}")]
    [InlineData("public class Outer<T> { public struct Job", "}")]
    public void StructType_GetsOnlyLiteralMarkersAndOneSwitch(string open, string close)
    {
        var run = GeneratorTestHost.RunProfilerMarkers($$"""
            namespace Sample
            {
                {{open}}
                {
                    public void Execute()
                    {
                        using var a = this.Marker();
                        using var b = this.Marker().WithName("Inner");
                    }
                }
                {{close}}
            }
            """);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);

        var root = Assert.Single(run.RunResult.GeneratedTrees).GetRoot();
        var generated = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        // The type-name helper and the per-closed-type marker class exist only for a generic class.
        Assert.Empty(root.DescendantNodes().OfType<TypeOfExpressionSyntax>());
        Assert.Empty(generated.Members.OfType<BaseTypeDeclarationSyntax>());

        // Every marker is a static readonly field built from one string literal, which Burst folds at compile time.
        var fields = generated.Members.OfType<FieldDeclarationSyntax>().ToArray();
        Assert.Equal(2, fields.Length);
        foreach (var field in fields)
        {
            Assert.True(field.Modifiers.Any(SyntaxKind.StaticKeyword) && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword));

            var creation = Assert.IsType<ImplicitObjectCreationExpressionSyntax>(field.Declaration.Variables.Single().Initializer!.Value);
            var argument = Assert.Single(creation.ArgumentList.Arguments);
            Assert.True(argument.Expression.IsKind(SyntaxKind.StringLiteralExpression), $"'{creation}' is not a string literal.");
        }

        // The only method is Marker: it takes the job by reference, switches on the line and calls Auto() on a field.
        var marker = Assert.Single(generated.Members.OfType<MethodDeclarationSyntax>());
        Assert.Equal("Marker", marker.Identifier.ValueText);
        Assert.True(marker.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.InKeyword), "The job is copied on every call.");

        var calls = marker.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
        Assert.NotEmpty(calls);
        Assert.All(calls, call => Assert.Matches(@"^\w+\.Auto\(\)$", call.ToString()));
    }
}
