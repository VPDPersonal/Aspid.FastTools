using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Aspid.FastTools.Analyzers.Descriptions;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Aspid.FastTools.Analyzers.ProfilerMarkerAnalyzer,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace Aspid.FastTools.Analyzers.Tests;

public class ProfilerMarkerAnalyzerTests
{
    // Mirrors the runtime signatures of the global-namespace class the analyzer matches, so the tests need no package reference.
    private const string Stubs = @"
namespace Unity.Profiling
{
    public struct ProfilerMarker
    {
        public struct AutoScope : System.IDisposable { public void Dispose() { } }
    }
}

public static class ProfilerMarkerExtensionsForGenerator
{
    public static Unity.Profiling.ProfilerMarker.AutoScope Marker<T>(this T instance, [System.Runtime.CompilerServices.CallerLineNumber] int line = -1) => default;
    public static Unity.Profiling.ProfilerMarker.AutoScope WithName(this in Unity.Profiling.ProfilerMarker.AutoScope marker, string name) => marker;
}";

    private static string Inaccessible(string type) =>
        $"'{type}' is private or protected, or nested in such a type, so the generated overload cannot see it — make it internal or public";

    private static string Shadowed(string type) =>
        $"'{type}' reuses a type parameter name of a containing type, which the generated overload cannot declare twice — rename it";

    private static string Receiver(string receiver, string type) =>
        $"it is called on '{receiver}', not on '{type}' — only calls on an instance of the type they are written in get a marker";

    private const string Argument = "it passes an argument, but the line must come from [CallerLineNumber] — call Marker() without arguments";

    private static Task Verify(string code, params DiagnosticResult[] expected) =>
        VerifyCS.VerifyAnalyzerAsync(code + "\n" + Stubs, expected);

    private static Task Verify(LanguageVersion languageVersion, string code, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ProfilerMarkerAnalyzer, XUnitVerifier> { TestCode = code + "\n" + Stubs };
        test.SolutionTransforms.Add((solution, projectId) =>
            solution.WithProjectParseOptions(projectId, new CSharpParseOptions(languageVersion)));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    private static DiagnosticResult Unsupported(int location, string reason) =>
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerUnsupportedTypeRule).WithLocation(location).WithArguments(reason);

    private static DiagnosticResult Discarded(int location) =>
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerScopeDiscardedRule).WithLocation(location);

    private static DiagnosticResult Suspended(int location, string keyword) =>
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerScopeSuspendedRule).WithLocation(location).WithArguments(keyword);

    private static DiagnosticResult NotConstant(int location) =>
        VerifyCS.Diagnostic(DiagnosticRules.ProfilerMarkerNameNotConstantRule).WithLocation(location);

    [Fact]
    public Task PublicType_NoDiagnostic() => Verify(@"
class C { void Run() { using var _ = this.Marker(); } }");

    [Fact]
    public Task SupportedShapes_NoDiagnostic() => Verify(@"
class Outer { internal class Inner { void Run() { using var _ = this.Marker(); } } }
class Outer2 { protected internal class Inner { void Run() { using var _ = this.Marker(); } } }
class Box<T> { public class Inner { void Run() { using var _ = this.Marker(); } } }
struct S { void Run() { using var _ = this.Marker(); } }
class F
{
    static readonly int Count = Use(new F().Marker());
    static int Use(Unity.Profiling.ProfilerMarker.AutoScope scope) => 0;
    void Other(F other) { using var _ = other.Marker(); }
    void Scoped() { using (this.Marker()) { } }
    Unity.Profiling.ProfilerMarker.AutoScope Returned() => this.Marker().WithName(""X"");
}");

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public Task InaccessibleNestedType_Reports(string accessibility) => Verify(
        "class Outer { " + accessibility + " class Inner { void Run() { using var _ = this.{|#0:Marker|}(); } } }",
        Unsupported(0, Inaccessible("Outer.Inner")));

    [Fact]
    public Task PublicTypeNestedInPrivateType_Reports() => Verify(@"
class Outer { private class Middle { public class Inner { void Run() { System.Action a = () => { using var _ = this.{|#0:Marker|}(); }; } } } }",
        Unsupported(0, Inaccessible("Outer.Middle.Inner")));

    [Fact]
    public Task NestedTypeParameterShadowingOuter_Reports() => Verify(@"
class Outer<T>
{
#pragma warning disable CS0693
    public class Inner<T> { void Run() { using var _ = this.{|#0:Marker|}(); } }
#pragma warning restore CS0693
}",
        Unsupported(0, Shadowed("Outer<T>.Inner<T>")));

    [Fact]
    public Task DefaultInterfaceMethod_Reports() => Verify(@"
interface ITickable { void Tick() { using var _ = this.{|#0:Marker|}(); } }",
        Unsupported(0, "'ITickable' is an interface — call it from the implementing type"));

    [Fact]
    public Task ReceiverOfAnotherType_Reports() => Verify(@"
class Bar { }
class Foo
{
    void Run(Bar bar)
    {
        using var a = bar.{|#0:Marker|}();
        using var b = ((object)this).{|#1:Marker|}();
    }
}
static class FooExtensions
{
    static void Measure(this Foo foo) { using var _ = foo.{|#2:Marker|}(); }
}",
        Unsupported(0, Receiver("Bar", "Foo")),
        Unsupported(1, Receiver("object", "Foo")),
        Unsupported(2, Receiver("Foo", "FooExtensions")));

    [Fact]
    public Task ExpressionTree_Reports() => Verify(@"
class Foo
{
    System.Linq.Expressions.Expression<System.Func<Unity.Profiling.ProfilerMarker.AutoScope>> Get() => () => {|#1:this.{|#0:Marker|}()|};
}",
        Unsupported(0, "it is inside an expression tree"),
        DiagnosticResult.CompilerError("CS0854").WithLocation(1));

    [Fact]
    public Task Argument_OnFallback_Reports() => Verify(@"
class Foo { void Run() { using var _ = this.{|#0:Marker|}(5); } }",
        Unsupported(0, Argument));

    [Fact]
    public Task Argument_OnGeneratedOverload_Reports() => Verify(@"
[System.CodeDom.Compiler.GeneratedCode(""Aspid.FastTools.Generators.ProfilerMarkersGenerator"", ""1.0.0"")]
static class __FooProfilerMarkerExtensions
{
    public static Unity.Profiling.ProfilerMarker.AutoScope Marker(this Foo __instance, [System.Runtime.CompilerServices.CallerLineNumber] int __line = -1) => default;
}
class Foo
{
    void Run() { using var _ = this.Marker(); }
    void Other() { using var _ = this.{|#0:Marker|}(line: 5); }
}".Replace("(line: 5)", "(__line: 5)"),
        Unsupported(0, Argument));

    // The generator takes a struct by in, so the call does not copy it.
    [Fact]
    public Task StructGeneratedOverload_IsRecognised() => Verify(@"
[System.CodeDom.Compiler.GeneratedCode(""Aspid.FastTools.Generators.ProfilerMarkersGenerator"", ""1.0.0"")]
static class __JobProfilerMarkerExtensions
{
    public static Unity.Profiling.ProfilerMarker.AutoScope Marker(this in Job __instance, [System.Runtime.CompilerServices.CallerLineNumber] int __line = -1) => default;
}
[System.CodeDom.Compiler.GeneratedCode(""Aspid.FastTools.Generators.ProfilerMarkersGenerator"", ""1.0.0"")]
static class __SpanProfilerMarkerExtensions
{
    public static Unity.Profiling.ProfilerMarker.AutoScope Marker(this in Span __instance, [System.Runtime.CompilerServices.CallerLineNumber] int __line = -1) => default;
}
struct Job
{
    void Run() { using var _ = this.Marker(); }
    void Statement() { this.{|#0:Marker|}(); }
    void Other() { using var _ = this.{|#1:Marker|}(__line: 5); }
}
ref struct Span
{
    void Run() { using var _ = this.Marker(); }
    void Statement() { this.{|#2:Marker|}(); }
}",
        Discarded(0), Unsupported(1, Argument), Discarded(2));

    [Fact]
    public Task MethodGroup_Reports() => Verify(@"
class Foo
{
    void Run()
    {
        System.Func<int, Unity.Profiling.ProfilerMarker.AutoScope> f = this.{|#0:Marker|};
    }
}",
        Unsupported(0, "it is used as a method group, so no call passes its line"));

    [Fact]
    public Task DiscardedScope_Reports() => Verify(@"
class Foo
{
    void Statement() { this.{|#0:Marker|}(); }
    void Discard() { _ = this.{|#1:Marker|}(); }
    void ExpressionBody() => this.{|#2:Marker|}();
    void Named() { this.{|#3:Marker|}().WithName(""X""); }
    void Lambda() { System.Action a = () => this.{|#4:Marker|}(); }
}",
        Discarded(0), Discarded(1), Discarded(2), Discarded(3), Discarded(4));

    [Fact]
    public Task ScopeAcrossYieldOrAwait_Reports() => Verify(@"
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
class Foo
{
    IEnumerator Statement() { using (this.{|#0:Marker|}()) { Step(); yield return null; } }
    IEnumerator Declaration() { using var _ = this.{|#1:Marker|}(); Step(); if (true) yield return null; }
    IEnumerator Named() { using (var scope = (this.{|#2:Marker|}()).WithName(""Load"")) yield return null; }
    async Task Awaited() { using (this.{|#3:Marker|}()) await Task.Yield(); }
    async Task AwaitForeach(IAsyncEnumerable<int> items) { using var _ = this.{|#4:Marker|}(); await foreach (var item in items) Step(); }
    void Step() { }
}",
        Suspended(0, "yield return"), Suspended(1, "yield return"), Suspended(2, "yield return"), Suspended(3, "await"), Suspended(4, "await"));

    [Fact]
    public Task ScopeEndingBeforeYieldOrAwait_NoDiagnostic() => Verify(@"
using System.Collections;
using System.Threading.Tasks;
class Foo
{
    IEnumerator Coroutine()
    {
        using (this.Marker()) Step();
        yield return null;
        {
            using var _ = this.Marker();
            Step();
        }
        yield return null;
    }

    async Task Run()
    {
        using (this.Marker()) Step();
        await Task.Yield();
        using var _ = this.Marker();
        System.Func<Task> later = async () => await Task.Yield();
        Step();
    }

    IEnumerator Nested()
    {
        using var _ = this.Marker();
        IEnumerator Local() { yield return null; }
        async Task LocalAsync() { await Task.Yield(); }
        Step();
        yield break;
    }

    void Step() { }
}");

    // Unity compiles C# 9, where an interpolated string is never a constant, so its parts are read one by one.
    [Theory]
    [InlineData(LanguageVersion.CSharp9)]
    [InlineData(LanguageVersion.Latest)]
    public Task ConstantName_NoDiagnostic(LanguageVersion languageVersion) => Verify(languageVersion, @"
class Foo
{
    const string Prefix = ""Load"";
    void Literal() { using var _ = this.Marker().WithName(""X""); }
    void Const() { using var _ = this.Marker().WithName(Prefix); }
    void NameOf() { using var _ = this.Marker().WithName(nameof(Literal)); }
    void Concatenation() { using var _ = this.Marker().WithName(Prefix + "".Assets""); }
    void Interpolated() { using var _ = this.Marker().WithName($""{Prefix}.{nameof(Const)}""); }
}");

    [Fact]
    public Task NameKnownOnlyAtRunTime_Reports() => Verify(@"
class Foo
{
    static readonly string Field = ""X"";
    void Variable(string name) { using var _ = this.Marker().WithName({|#0:name|}); }
    void ReadonlyField() { using var _ = this.Marker().WithName({|#1:Field|}); }
    void Hole(int i) { using var _ = this.Marker().WithName({|#2:$""X{i}""|}); }
    void Aligned() { using var _ = this.Marker().WithName({|#3:$""{nameof(Aligned),8}""|}); }
    void Null() { using var _ = this.Marker().WithName({|#4:null|}); }
}",
        NotConstant(0), NotConstant(1), NotConstant(2), NotConstant(3), NotConstant(4));

    [Fact]
    public Task DiscardedScopeOfUnsupportedCall_ReportsOnlyUnsupported() => Verify(@"
class Outer { private class Inner { void Run() { this.{|#0:Marker|}(); } } }",
        Unsupported(0, Inaccessible("Outer.Inner")));

    // Stands in for the generator's output, so calls can bind to a generated overload without running it.
    private const string GeneratedBase = @"
namespace N
{
    [System.CodeDom.Compiler.GeneratedCode(""Aspid.FastTools.Generators.ProfilerMarkersGenerator"", ""1.0.0"")]
    static class __BaseProfilerMarkerExtensions
    {
        public static Unity.Profiling.ProfilerMarker.AutoScope Marker(this Base __instance, [System.Runtime.CompilerServices.CallerLineNumber] int __line = -1) => default;
    }

    public class Base { void Show() { using var _ = this.Marker(); } }
}";

    [Fact]
    public Task PrivateNestedTypeBindingToBaseOverload_Reports() => Verify(GeneratedBase + @"
namespace N
{
    class Host { private class Popup : Base { void Open() { using var _ = this.{|#0:Marker|}(); } } }
}",
        Unsupported(0, Inaccessible("N.Host.Popup")));

    [Fact]
    public Task MethodGroupOfGeneratedOverload_Reports() => Verify(GeneratedBase + @"
namespace N
{
    class Derived : Base
    {
        void Run() { System.Func<int, Unity.Profiling.ProfilerMarker.AutoScope> f = this.{|#0:Marker|}; }
    }
}",
        Unsupported(0, "it is used as a method group, so no call passes its line"));

    [Fact]
    public Task ConditionalAccess_Reports() => Verify(@"
class Foo { void Run() { using var _ = this?.{|#0:Marker|}(); } }",
        Unsupported(0, "it uses '?.' — call this.Marker() directly"));

    [Fact]
    public Task TypeArguments_Reports() => Verify(@"
class Foo { void Run() { using var _ = this.{|#0:Marker<Foo>|}(); } }",
        Unsupported(0, "it passes type arguments — call Marker() without them"));

    [Fact]
    public Task StaticCallForm_Reports() => Verify(@"
class Foo { void Run() { using var _ = ProfilerMarkerExtensionsForGenerator.{|#0:Marker|}(this); } }",
        Unsupported(0, "it is called as a static method — call it as an extension method: this.Marker()"));

    [Fact]
    public Task InitializersOfPrivateNestedType_Report() => Verify(@"
class Outer
{
    private class Inner
    {
        static readonly int Field = Use(new Inner().{|#0:Marker|}());
        static int Property { get; } = Use(new Inner().{|#1:Marker|}());
        static int Use(Unity.Profiling.ProfilerMarker.AutoScope scope) => 0;
    }
}",
        Unsupported(0, Inaccessible("Outer.Inner")),
        Unsupported(1, Inaccessible("Outer.Inner")));

    [Fact]
    public Task DeeplyNestedShadowedTypeParameter_Reports() => Verify(@"
public class A<T>
{
    public class B<U>
    {
#pragma warning disable CS0693
        public class C<T> { void Run() { using var _ = this.{|#0:Marker|}(); } }
#pragma warning restore CS0693
    }
}",
        Unsupported(0, Shadowed("A<T>.B<U>.C<T>")));

    [Fact]
    public Task LookalikeClassInANamespace_NoDiagnostic() => Verify(@"
namespace Other
{
    static class ProfilerMarkerExtensionsForGenerator { public static int Marker(this object instance) => 0; }

    class Outer { private class Inner { void Run() { this.Marker(); } } }
}");

    [Fact]
    public Task UnreadLocalScope_Reports() => Verify(@"
class Foo
{
    void Unread() { var _ = this.{|#0:Marker|}(); }
    void Disposed() { var scope = this.Marker(); scope.Dispose(); }
    void Passed() { var scope = this.Marker(); Use(scope); }
    static void Use(Unity.Profiling.ProfilerMarker.AutoScope scope) { }
}",
        Discarded(0));

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
