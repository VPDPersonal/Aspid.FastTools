using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

// Type shapes and names that used to break the generated code or merge two types into one.
public class ProfilerMarkersTypeShapeTests
{
    private static GeneratorRun AssertCompilesAndBinds(params string[] sources)
    {
        var run = GeneratorTestHost.RunProfilerMarkers(sources);
        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
        return run;
    }

    [Fact]
    public void KeywordNamespace_Compiles() => AssertCompilesAndBinds("""
        namespace Game.@event
        {
            public class Foo { public void Run() { using var _ = this.Marker(); } }
        }
        """);

    [Fact]
    public void TypesDifferingOnlyInCase_GetDistinctFiles()
    {
        var run = AssertCompilesAndBinds("""
            namespace Sample
            {
                public class Foo { public void Run() { using var _ = this.Marker(); } }
                public class foo { public void Run() { using var _ = this.Marker(); } }
            }
            """, """
            namespace Sample.UI { public class View { public void Run() { using var _ = this.Marker(); } } }
            namespace Sample.Ui { public class View { public void Run() { using var _ = this.Marker(); } } }
            """);

        Assert.Equal(4, run.RunResult.Results[0].GeneratedSources.Length);
    }

    [Fact]
    public void FlattenedNamesThatCollide_GetDistinctClasses() => AssertCompilesAndBinds("""
        namespace Sample
        {
            public class Foo<T> { public void Run() { using var _ = this.Marker(); } }
            public class Foo_1 { public void Run() { using var _ = this.Marker(); } }

            public class Outer { public class Inner { public void Run() { using var _ = this.Marker(); } } }
            public class Outer_Inner { public void Run() { using var _ = this.Marker(); } }

            public class Box<T> { public class Inner { public void Run() { using var _ = this.Marker(); } } }
            public class Box_1 { public class Inner { public void Run() { using var _ = this.Marker(); } } }
        }
        """);

    [Fact]
    public void KeywordAndGeneratorNamedTypeParameters_Compile() => AssertCompilesAndBinds("""
        namespace Sample
        {
            public class Foo<@event> where @event : class { public void Run() { using var _ = this.Marker(); } }
            public class Bar<__Markers, __line, __instance> { public void Run() { using var _ = this.Marker(); } }
            public class Baz<Markers, line, _> { public void Run() { using var _ = this.Marker(); } }
        }
        """);

    [Fact]
    public void EveryConstraintKind_Compiles() => AssertCompilesAndBinds("""
        #nullable enable
        namespace Sample
        {
            public interface IUnit { }

            public class A<T> where T : class?, new() { public void Run() { using var _ = this.Marker(); } }
            public class B<T> where T : struct { public void Run() { using var _ = this.Marker(); } }
            public class C<T> where T : unmanaged { public void Run() { using var _ = this.Marker(); } }
            public class D<T> where T : notnull { public void Run() { using var _ = this.Marker(); } }
            public class E<T, U> where T : IUnit, U where U : class { public void Run() { using var _ = this.Marker(); } }
            public struct F<T> where T : System.IComparable<T> { public void Run() { using var _ = this.Marker(); } }
            public class G<T> where T : G<T> { public void Run() { using var _ = this.Marker(); } }
        }
        """);

    [Fact]
    public void StaticClass_GetsNoOverloadAndCompiles()
    {
        var run = GeneratorTestHost.RunProfilerMarkers("""
            namespace Sample
            {
                public class Foo { }

                public static class FooExtensions
                {
                    public static void Measure(this Foo foo) { using var _ = foo.Marker(); }
                }

                public static class Helpers<T>
                {
                    public static void Go(object value) { using var _ = value.Marker(); }
                }
            }
            """);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToFallback(run);
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
    }

    [Fact]
    public void DefaultInterfaceMethod_GetsNoOverload_SoImplementersAreNotAmbiguous()
    {
        var run = GeneratorTestHost.RunProfilerMarkers("""
            namespace Sample
            {
                public interface ITickable { void Tick() { using var _ = this.Marker(); } }

                public class ViewBase { public void Show() { using var _ = this.Marker(); } }

                public class Host
                {
                    private sealed class Popup : ViewBase, ITickable
                    {
                        public void Open() { using var _ = this.Marker(); }
                    }
                }
            }
            """);

        GeneratorTestHost.AssertNoErrors(run);
        Assert.DoesNotContain("ITickable", GeneratorTestHost.GeneratedText(run));
    }

    [Fact]
    public void ExpressionTree_GetsNoOverloadAndCompiles()
    {
        var run = GeneratorTestHost.RunProfilerMarkers("""
            namespace Sample
            {
                public class Foo
                {
                    public System.Linq.Expressions.Expression<System.Func<Unity.Profiling.ProfilerMarker.AutoScope>> Get() => () => this.Marker();
                }
            }
            """);

        GeneratorTestHost.AssertNoErrors(run);
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
    }

    [Fact]
    public void ExplicitLineArgument_IsNotMarked()
    {
        // Alone, Marker(5) matches no overload (CS1501); next to a real call it binds to the generated one,
        // which only knows the real call's line — AFT0010 reports the argument.
        var run = GeneratorTestHost.RunProfilerMarkers("""
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { using var _ = this.Marker(); }
                    public void Other() { using var _ = this.Marker(5); }
                }
            }
            """);

        GeneratorTestHost.AssertNoErrors(run);
        var text = GeneratorTestHost.GeneratedText(run);
        Assert.Contains("Run_Marker_Line_5", text);
        Assert.DoesNotContain("Other_Marker", text);
    }

    [Fact]
    public void DerivedPrivateTypeInAnotherAssembly_BindsToFallback()
    {
        var stubs = GeneratorTestHost.EmitStubsReference();
        var library = GeneratorTestHost.EmitReference("""
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestCompilation")]

            namespace Lib
            {
                public class ViewBase { public void Show() { using var _ = this.Marker(); } }
            }
            """, "Lib", stubs);

        var run = GeneratorTestHost.RunProfilerMarkers(new[] { """
            namespace App
            {
                public class Screen : Lib.ViewBase { public void Open() { using var _ = this.Marker(); } }

                public class Host
                {
                    private sealed class Popup : Lib.ViewBase { public void Open() { using var _ = this.Marker(); } }
                }
            }
            """ }, references: new[] { stubs, library }, includeStubs: false);

        GeneratorTestHost.AssertNoErrors(run);

        var model = run.OutputCompilation.GetSemanticModel(run.OutputCompilation.SyntaxTrees.First());
        var calls = run.OutputCompilation.SyntaxTrees.First().GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .Select(call => ((IMethodSymbol)model.GetSymbolInfo(call).Symbol!).ContainingType.Name)
            .ToArray();

        Assert.Equal(new[] { "__ScreenProfilerMarkerExtensions", "ProfilerMarkerExtensionsForGenerator" }, calls);
    }

    [Fact]
    public void DerivedTypeInTheSameNamespaceOfAnotherAssembly_GetsItsOwnOverload()
    {
        var stubs = GeneratorTestHost.EmitStubsReference();
        var library = GeneratorTestHost.EmitReference("""
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestCompilation")]

            namespace Game
            {
                public class Base { public void Show() { using var _ = this.Marker(); } }
                public class GBase<T> { public void Show() { using var _ = this.Marker(); } }
            }
            """, "Lib", stubs);

        // Extension lookup meets Lib's overloads in namespace Game before the global fallback,
        // so these types must get overloads of their own.
        var run = GeneratorTestHost.RunProfilerMarkers(new[] { """
            namespace Game
            {
                public class Derived : Base { public void Open() { using var _ = this.Marker(); } }
                public class Closed : GBase<int> { public void Open() { using var _ = this.Marker(); } }
                public class Open<U> : GBase<U> { public void Run() { using var _ = this.Marker(); } }
            }
            """ }, references: new[] { stubs, library }, includeStubs: false);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }
}
