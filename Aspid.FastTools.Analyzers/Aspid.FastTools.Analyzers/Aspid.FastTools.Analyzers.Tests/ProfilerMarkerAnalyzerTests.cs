using System.Threading.Tasks;
using Xunit;
using Aspid.FastTools.Analyzers.Descriptions;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Aspid.FastTools.Analyzers.ProfilerMarkerAnalyzer,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace Aspid.FastTools.Analyzers.Tests;

public class ProfilerMarkerAnalyzerTests
{
    // Mirrors the global-namespace class the analyzer matches by name, so the tests need no package reference.
    private const string Stubs = @"
public static class ProfilerMarkerExtensionsForGenerator
{
    public static int Marker(this object instance) => 0;
}";

    private const string InaccessibleReason =
        "is private or protected, or nested in such a type, so the generated overload cannot see it — make it internal or public";

    private const string ShadowedTypeParameterReason =
        "reuses a type parameter name of a containing type, which the generated overload cannot declare twice — rename it";

    private static Task Verify(string code, params Microsoft.CodeAnalysis.Testing.DiagnosticResult[] expected) =>
        VerifyCS.VerifyAnalyzerAsync(code + "\n" + Stubs, expected);

    [Fact]
    public Task PublicType_NoDiagnostic() => Verify(@"
class C { void Run() { this.Marker(); } }");

    [Fact]
    public Task InternalNestedType_NoDiagnostic() => Verify(@"
class Outer { internal class Inner { void Run() { this.Marker(); } } }");

    [Fact]
    public Task ProtectedInternalNestedType_NoDiagnostic() => Verify(@"
class Outer { protected internal class Inner { void Run() { this.Marker(); } } }");

    [Fact]
    public Task NonGenericTypeNestedInGenericType_NoDiagnostic() => Verify(@"
class Outer<T> { public class Inner { void Run() { this.Marker(); } } }");

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public Task InaccessibleNestedType_Reports(string accessibility) => Verify(
        "class Outer { " + accessibility + " class Inner { void Run() { this.{|#0:Marker|}(); } } }",
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule)
            .WithLocation(0)
            .WithArguments("Outer.Inner", InaccessibleReason));

    [Fact]
    public Task PublicTypeNestedInPrivateType_Reports() => Verify(@"
class Outer { private class Middle { public class Inner { void Run() { System.Action a = () => this.{|#0:Marker|}(); } } } }",
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule)
            .WithLocation(0)
            .WithArguments("Outer.Middle.Inner", InaccessibleReason));

    [Fact]
    public Task NestedTypeParameterShadowingOuter_Reports() => Verify(@"
class Outer<T>
{
#pragma warning disable CS0693
    public class Inner<T> { void Run() { this.{|#0:Marker|}(); } }
#pragma warning restore CS0693
}",
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule)
            .WithLocation(0)
            .WithArguments("Outer<T>.Inner<T>", ShadowedTypeParameterReason));

    [Fact]
    public Task UnrelatedMarkerMethod_NoDiagnostic() => Verify(@"
class Outer
{
    private class Inner
    {
        int Marker() => 0;
        void Run() { this.Marker(); }
    }
}");
}
