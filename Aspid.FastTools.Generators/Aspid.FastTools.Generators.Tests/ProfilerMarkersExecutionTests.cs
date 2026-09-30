using Xunit;
using System;
using System.Linq;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

// Runs the generated code and checks which markers the calls actually open.
public class ProfilerMarkersExecutionTests
{
    // 1-based line of the source line that carries the tag comment.
    private static int LineOf(string source, string tag)
    {
        var lines = source.Split('\n');
        var index = Array.FindIndex(lines, l => l.Contains("/*" + tag + "*/"));
        Assert.True(index >= 0, $"No /*{tag}*/ in the source");
        return index + 1;
    }

    private static string[] Run(string source, bool enableProfiler = true)
    {
        var run = GeneratorTestHost.RunProfilerMarkers(source, enableProfiler);
        return GeneratorTestHost.Execute(run, "Sample.Probe").ToArray();
    }

    [Fact]
    public void EveryMemberKind_OpensItsOwnMarker()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public static readonly System.Func<Foo, int> Hook = f => { using var _ = f.Marker(); return 0; };
                    public System.Func<Foo, int> Selector { get; } = f => { using var _ = f.Marker(); return 0; };

                    public Foo() { using var _ = this.Marker(); }

                    public void Step()
                    {
                        using var _ = this.Marker();
                        void Local() { using var __ = this.Marker(); }
                        Local();
                        System.Action lambda = () => { using var __ = this.Marker(); };
                        lambda();
                    }

                    public int Speed { get { using var _ = this.Marker(); return 0; } }

                    public int this[int i] { get { using var _ = this.Marker(); return i; } }

                    public event System.Action Changed
                    {
                        add { using var _ = this.Marker(); }
                        remove { using var _ = this.Marker(); }
                    }

                    public static Foo operator +(Foo a, Foo b) { using var _ = a.Marker(); return a; }
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        var foo = new Foo();
                        Foo.Hook(foo);
                        foo.Selector(foo);
                        foo.Step();
                        _ = foo.Speed;
                        _ = foo[1];
                        foo.Changed += null;
                        foo.Changed -= null;
                        _ = foo + foo;
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo.Ctor",
            "Foo.Hook",
            "Foo.Selector",
            "Foo.Step",
            "Foo.Step #2",
            "Foo.Step #3",
            "Foo.Speed",
            "Foo.Indexer",
            "Foo.Changed",
            "Foo.Changed #2",
            "Foo.op_Addition",
        }, Run(source));
    }

    [Fact]
    public void AutoPropertyInitializer_OnGenericTypeAndRecord_Compiles()
    {
        const string source = """
            namespace Sample
            {
                public class Box<T>
                {
                    public static System.Action<Box<T>> Hook { get; } = b => { using var _ = b.Marker(); };
                }

                public record Rec
                {
                    public System.Action<Rec> Hook { get; init; } = r => { using var _ = r.Marker(); };
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        Box<int>.Hook(new Box<int>());
                        var rec = new Rec();
                        rec.Hook(rec);
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Box<Int32>.Hook",
            "Rec.Hook",
        }, Run(source));
    }

    [Fact]
    public void MultiLineCall_OpensTheMarkerOfTheLineCallerLineNumberPasses()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void A()
                    {
                        using var _ = this
                            .Marker()
                            .WithName("A");
                    }

                    public void B()
                    {
                        using var _ = this.Marker
                        (
                        );
                    }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo.A",
            "Foo.B",
        }, Run(source));
    }

    [Fact]
    public void LineDirective_UsesTheMappedLine()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                #line 100
                    public void A() { using var _ = this.Marker(); }
                #line default
                    public void B() { using var _ = this.Marker(); }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[] { "Foo.A", "Foo.B" }, Run(source));
    }

    [Fact]
    public void RepeatedName_GetsAnOrdinalInSourceOrder()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Step()
                    {
                        using var _ = this.Marker();
                        using (this.Marker()) { }
                    }

                    public void Update() { using var _ = this.Marker().WithName("Step"); }
                }

                public static class Probe { public static void Run() { new Foo().Update(); new Foo().Step(); } }
            }
            """;

        Assert.Equal(new[] { "Foo.Step #3", "Foo.Step", "Foo.Step #2" }, Run(source));
    }

    [Fact]
    public void OrdinalTakenByWithName_IsSkipped()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Step() { using var _ = this.Marker(); }
                    public void Update() { using var _ = this.Marker().WithName("Step"); }
                    public void Late() { using var _ = this.Marker().WithName("Step #2"); }
                }

                public static class Probe { public static void Run() { new Foo().Step(); new Foo().Update(); new Foo().Late(); } }
            }
            """;

        Assert.Equal(new[] { "Foo.Step", "Foo.Step #3", "Foo.Step #2" }, Run(source));
    }

    [Fact]
    public void EditAboveACall_KeepsItsName()
    {
        static string Source(string padding) => $$"""
            namespace Sample
            {
                public class Foo
                {{{padding}}
                    public void Run() { using var _ = this.Marker(); }
                    public void Walk() { using var _ = this.Marker().WithName("Run"); }
                }

                public static class Probe { public static void Run() { new Foo().Run(); new Foo().Walk(); } }
            }
            """;

        Assert.Equal(new[] { "Foo.Run", "Foo.Run #2" }, Run(Source(string.Empty)));
        Assert.Equal(new[] { "Foo.Run", "Foo.Run #2" }, Run(Source("\n\n\n")));
    }

    [Fact]
    public void PartialFiles_AreNumberedFileByFile()
    {
        // B sits lower in its file than A in its own, but User0.cs comes first.
        var run = GeneratorTestHost.RunProfilerMarkers(new[]
        {
            """
            namespace Sample
            {
                public partial class Foo
                {

                    public void B() { using var _ = this.Marker().WithName("Step"); }
                }
            }
            """,
            """
            namespace Sample
            {
                public partial class Foo
                {
                    public void A() { using var _ = this.Marker().WithName("Step"); }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """,
        });

        Assert.Equal(new[] { "Foo.Step #2", "Foo.Step" }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Fact]
    public void TypesDifferingOnlyInArity_EachOpenTheirOwnMarkers()
    {
        const string source = """
            namespace Sample
            {
                public class Pool<T> { public void Get() { using var _ = this.Marker(); } }
                public class Pool { public void Get() { using var _ = this.Marker(); } }
                public class Pool<T, U> { public void Get() { using var _ = this.Marker(); } }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Pool().Get();
                        new Pool<int>().Get();
                        new Pool<int, string>().Get();
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Pool.Get",
            "Pool<Int32>.Get",
            "Pool<Int32, String>.Get",
        }, Run(source));
    }

    [Fact]
    public void GenericClass_NamesNestedTypeArguments()
    {
        const string source = """
            namespace Sample
            {
                public class Foo<T> { public void A() { using var _ = this.Marker(); } }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Foo<System.Collections.Generic.List<int>>().A();
                        new Foo<System.Collections.Generic.List<string>>().A();
                    }
                }
            }
            """;

        Assert.Equal(new[] { "Foo<List<Int32>>.A", "Foo<List<String>>.A" }, Run(source));
    }

    [Fact]
    public void GenericStruct_GetsOneLiteralName()
    {
        const string source = """
            namespace Sample
            {
                public struct Worker<T> { public void Execute() { using var _ = this.Marker(); } }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Worker<int>().Execute();
                        new Worker<float>().Execute();
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        // Burst cannot run typeof(T), so the struct's markers are literals in the non-generic class.
        Assert.DoesNotContain("typeof", GeneratorTestHost.GeneratedText(run));

        Assert.Equal(new[] { "Worker<T>.Execute", "Worker<T>.Execute" }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Fact]
    public void WithName_UnescapesBracesAndEscapesLineSeparators()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void A() { using var _ = this.Marker().WithName($"Br{{ace}}"); }
                    public void B() { using var _ = this.Marker().WithName("Line\u2028Sep\u2029Par\u0085Next"); }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo.Br{ace}",
            "Foo.Line\u2028Sep\u2029Par\u0085Next",
        }, Run(source));
    }

    [Fact]
    public void UnrelatedWithName_DoesNotRenameTheMarker()
    {
        const string source = """
            namespace Sample
            {
                public static class Other
                {
                    public static Unity.Profiling.ProfilerMarker.AutoScope WithName(this Unity.Profiling.ProfilerMarker.AutoScope scope, string name, int _ = 0) => scope;
                }

                public class Foo
                {
                    public void A() { using var _ = Other.WithName(this.Marker(), "Wrong"); }
                }

                public static class Probe { public static void Run() => new Foo().A(); }
            }
            """;

        Assert.Equal(new[] { "Foo.A" }, Run(source));
    }

    [Fact]
    public void ReceiverOfAnotherType_OpensNoMarkerAndEmitsNoDeadFields()
    {
        const string source = """
            namespace Sample
            {
                public class Bar { public void Run() { using var _ = this.Marker(); } }

                public class Foo
                {
                    private readonly Bar _bar = new Bar();
                    private readonly Foo? _other;

                    public Foo(Foo? other) { _other = other; }

                    public void Run()
                    {
                        using var a = _bar.Marker(); /*foreign*/
                        using var b = ((object)this).Marker();
                        if (_other is not null) { using var c = _other.Marker(); }
                    }
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Foo(new Foo(null)).Run();
                        new Bar().Run();
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = GeneratorTestHost.GeneratedText(run);

        Assert.DoesNotContain($"Run_Marker_Line_{LineOf(source, "foreign")}", text);
        Assert.Equal(new[]
        {
            "Foo.Run",
            "Bar.Run",
        }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Fact]
    public void PrivateNestedTypeDerivedFromMarkedBase_OpensNothing()
    {
        const string source = """
            namespace Sample
            {
                public class ViewBase { public void Show() { using var _ = this.Marker(); } }

                public class Host
                {
                    private sealed class Popup : ViewBase
                    {
                        public void Open() { using var _ = this.Marker(); }
                    }

                    public static void Run() { var p = new Popup(); p.Open(); p.Show(); }
                }

                public static class Probe { public static void Run() => Host.Run(); }
            }
            """;

        // Popup gets no overload of its own, and extension lookup finds ViewBase's in the same namespace
        // before the global fallback: the call opens nothing unless a ViewBase call shares its line (AFT0010 warns).
        Assert.Equal(new[] { "ViewBase.Show" }, Run(source));
    }

    [Fact]
    public void GenericTypeArguments_AreNamedLikeCSharp()
    {
        const string source = """
            namespace Sample
            {
                public class O<T> { public class N { } public class J<U> { } }

                public class Foo<T> { public void A() { using var _ = this.Marker(); } }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Foo<O<int>.N>().A();
                        new Foo<O<string>.N>().A();
                        new Foo<O<int>.J<string>>().A();
                        new Foo<int[]>().A();
                        new Foo<System.Collections.Generic.Dictionary<int, string>.KeyCollection>().A();
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo<O<Int32>.N>.A",
            "Foo<O<String>.N>.A",
            "Foo<O<Int32>.J<String>>.A",
            "Foo<Int32[]>.A",
            "Foo<Dictionary<Int32, String>.KeyCollection>.A",
        }, Run(source));
    }

    [Fact]
    public void TypeNestedInGenericType_AndItsGenericSibling_OpenTheirOwnMarkers()
    {
        const string source = """
            namespace Sample
            {
                public class Outer<T>
                {
                    public class Inner { public void Get() { using var _ = this.Marker(); } }
                    public class Inner<U> { public void Get() { using var _ = this.Marker(); } }
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Outer<int>.Inner().Get();
                        new Outer<int>.Inner<string>().Get();
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Inner.Get",
            "Inner<String>.Get",
        }, Run(source));
    }

    [Fact]
    public void ExplicitInterfaceMembers_UseTheInterfaceMemberName()
    {
        const string source = """
            namespace Sample
            {
                public interface IFoo { void Run(); int Value { get; } event System.Action Changed; }

                public class Foo : IFoo
                {
                    void IFoo.Run() { using var _ = this.Marker(); }
                    int IFoo.Value { get { using var _ = this.Marker(); return 0; } }
                    event System.Action IFoo.Changed { add { using var _ = this.Marker(); } remove { } }
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        IFoo foo = new Foo();
                        foo.Run();
                        _ = foo.Value;
                        foo.Changed += null;
                    }
                }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo.Run",
            "Foo.Value",
            "Foo.Changed",
        }, Run(source));
    }

    [Fact]
    public void WithNameWithInterpolationHole_KeepsTheMemberName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run(int x) { using var _ = this.Marker().WithName($"X{x}"); }
                }

                public static class Probe { public static void Run() => new Foo().Run(1); }
            }
            """;

        Assert.Equal(new[] { "Foo.Run" }, Run(source));
    }

    [Fact]
    public void StructAndRefStruct_OpenTheirMarkers()
    {
        const string source = """
            namespace Sample
            {
                public struct Job { public void Execute() { using var _ = this.Marker(); } }
                public ref struct Span { public void Execute() { using var _ = this.Marker(); } }

                public static class Probe { public static void Run() { new Job().Execute(); new Span().Execute(); } }
            }
            """;

        Assert.Equal(new[]
        {
            "Job.Execute",
            "Span.Execute",
        }, Run(source));
    }

    [Fact]
    public void GlobalNamespaceTypes_OpenTheirMarkers()
    {
        // Unity's script template declares no namespace.
        const string source = """
            public class Foo { public void Run() { using var _ = this.Marker(); } }
            public class Box<T> { public void Run() { using var _ = this.Marker(); } }

            public static class Probe { public static void Run() { new Foo().Run(); new Box<int>().Run(); } }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        Assert.Equal(new[]
        {
            "Foo.Run",
            "Box<Int32>.Run",
        }, GeneratorTestHost.Execute(run, "Probe"));
    }

    [Fact]
    public void ObsoleteTypes_OpenTheirMarkers()
    {
        const string source = """
            namespace Sample
            {
                [System.Obsolete("x")] public class Foo { public void Run() { using var _ = this.Marker(); } }
                [System.Obsolete("x", true)] public class Bar { public void Run() { using var _ = this.Marker(); } }

                [System.Obsolete]
                public static class Probe { public static void Run() { new Foo().Run(); new Bar().Run(); } }
            }
            """;

        Assert.Equal(new[]
        {
            "Foo.Run",
            "Bar.Run",
        }, Run(source));
    }

    [Fact]
    public void WithoutEnableProfiler_CompilesAndOpensNothing()
    {
        const string source = """
            namespace Sample
            {
                public class Foo { public void Run() { using var _ = this.Marker(); } }
                public class Bar<T> { public void Run() { using var _ = this.Marker(); } }

                public static class Probe { public static void Run() { new Foo().Run(); new Bar<int>().Run(); } }
            }
            """;

        Assert.Empty(Run(source, enableProfiler: false));
    }
}
