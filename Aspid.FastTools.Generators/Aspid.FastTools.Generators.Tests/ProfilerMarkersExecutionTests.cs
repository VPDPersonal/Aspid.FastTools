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
                    public static readonly System.Func<Foo, int> Hook = f => { using var _ = f.Marker(); /*hook*/ return 0; };
                    public System.Func<Foo, int> Selector { get; } = f => { using var _ = f.Marker(); /*auto*/ return 0; };

                    public Foo() { using var _ = this.Marker(); /*ctor*/ }

                    public void Step()
                    {
                        using var _ = this.Marker(); /*step*/
                        void Local() { using var __ = this.Marker(); /*local*/ }
                        Local();
                        System.Action lambda = () => { using var __ = this.Marker(); /*lambda*/ };
                        lambda();
                    }

                    public int Speed { get { using var _ = this.Marker(); /*speed*/ return 0; } }

                    public int this[int i] { get { using var _ = this.Marker(); /*indexer*/ return i; } }

                    public event System.Action Changed
                    {
                        add { using var _ = this.Marker(); /*add*/ }
                        remove { using var _ = this.Marker(); /*remove*/ }
                    }

                    public static Foo operator +(Foo a, Foo b) { using var _ = a.Marker(); /*op*/ return a; }
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
            $"Foo.Ctor ({LineOf(source, "ctor")})",
            $"Foo.Hook ({LineOf(source, "hook")})",
            $"Foo.Selector ({LineOf(source, "auto")})",
            $"Foo.Step ({LineOf(source, "step")})",
            $"Foo.Step ({LineOf(source, "local")})",
            $"Foo.Step ({LineOf(source, "lambda")})",
            $"Foo.Speed ({LineOf(source, "speed")})",
            $"Foo.Indexer ({LineOf(source, "indexer")})",
            $"Foo.Changed ({LineOf(source, "add")})",
            $"Foo.Changed ({LineOf(source, "remove")})",
            $"Foo.op_Addition ({LineOf(source, "op")})",
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
                    public static System.Action<Box<T>> Hook { get; } = b => { using var _ = b.Marker(); /*box*/ };
                }

                public record Rec
                {
                    public System.Action<Rec> Hook { get; init; } = r => { using var _ = r.Marker(); /*rec*/ };
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
            $"Box<Int32>.Hook ({LineOf(source, "box")})",
            $"Rec.Hook ({LineOf(source, "rec")})",
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
                            .Marker() /*a*/
                            .WithName("A");
                    }

                    public void B()
                    {
                        using var _ = this.Marker
                        ( /*b*/
                        );
                    }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[]
        {
            $"Foo.A ({LineOf(source, "a")})",
            $"Foo.B ({LineOf(source, "b")})",
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
                    public void B() { using var _ = this.Marker(); /*b*/ }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[] { "Foo.A (100)", $"Foo.B ({LineOf(source, "b")})" }, Run(source));
    }

    [Fact]
    public void TypesDifferingOnlyInArity_EachOpenTheirOwnMarkers()
    {
        const string source = """
            namespace Sample
            {
                public class Pool<T> { public void Get() { using var _ = this.Marker(); /*one*/ } }
                public class Pool { public void Get() { using var _ = this.Marker(); /*zero*/ } }
                public class Pool<T, U> { public void Get() { using var _ = this.Marker(); /*two*/ } }

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
            $"Pool.Get ({LineOf(source, "zero")})",
            $"Pool<Int32>.Get ({LineOf(source, "one")})",
            $"Pool<Int32, String>.Get ({LineOf(source, "two")})",
        }, Run(source));
    }

    [Fact]
    public void GenericClass_NamesNestedTypeArguments()
    {
        const string source = """
            namespace Sample
            {
                public class Foo<T> { public void A() { using var _ = this.Marker(); /*a*/ } }

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

        var line = LineOf(source, "a");
        Assert.Equal(new[] { $"Foo<List<Int32>>.A ({line})", $"Foo<List<String>>.A ({line})" }, Run(source));
    }

    [Fact]
    public void GenericStruct_GetsOneLiteralName()
    {
        const string source = """
            namespace Sample
            {
                public struct Worker<T> { public void Execute() { using var _ = this.Marker(); /*a*/ } }

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

        var line = LineOf(source, "a");
        Assert.Equal(new[] { $"Worker<T>.Execute ({line})", $"Worker<T>.Execute ({line})" }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Fact]
    public void WithName_UnescapesBracesAndEscapesLineSeparators()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void A() { using var _ = this.Marker().WithName($"Br{{ace}}"); /*a*/ }
                    public void B() { using var _ = this.Marker().WithName("Line\u2028Sep"); /*b*/ }
                }

                public static class Probe { public static void Run() { new Foo().A(); new Foo().B(); } }
            }
            """;

        Assert.Equal(new[]
        {
            $"Foo.Br{{ace}} ({LineOf(source, "a")})",
            $"Foo.Line\u2028Sep ({LineOf(source, "b")})",
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
                    public void A() { using var _ = Other.WithName(this.Marker(), "Wrong"); /*a*/ }
                }

                public static class Probe { public static void Run() => new Foo().A(); }
            }
            """;

        Assert.Equal(new[] { $"Foo.A ({LineOf(source, "a")})" }, Run(source));
    }

    [Fact]
    public void ReceiverOfAnotherType_OpensNoMarkerAndEmitsNoDeadFields()
    {
        const string source = """
            namespace Sample
            {
                public class Bar { public void Run() { using var _ = this.Marker(); /*bar*/ } }

                public class Foo
                {
                    private readonly Bar _bar = new Bar();
                    private readonly Foo? _other;

                    public Foo(Foo? other) { _other = other; }

                    public void Run()
                    {
                        using var a = _bar.Marker(); /*foreign*/
                        using var b = ((object)this).Marker();
                        if (_other is not null) { using var c = _other.Marker(); /*same*/ }
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
            $"Foo.Run ({LineOf(source, "same")})",
            $"Bar.Run ({LineOf(source, "bar")})",
        }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Fact]
    public void PrivateNestedTypeDerivedFromMarkedBase_BindsToFallback()
    {
        const string source = """
            namespace Sample
            {
                public class ViewBase { public void Show() { using var _ = this.Marker(); /*base*/ } }

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

        // Popup gets no overload of its own; the generic fallback is a better match than ViewBase's,
        // so Popup's call opens nothing instead of whatever ViewBase marker sits on its line.
        Assert.Equal(new[] { $"ViewBase.Show ({LineOf(source, "base")})" }, Run(source));
    }

    [Fact]
    public void StructAndRefStruct_OpenTheirMarkers()
    {
        const string source = """
            namespace Sample
            {
                public struct Job { public void Execute() { using var _ = this.Marker(); /*job*/ } }
                public ref struct Span { public void Execute() { using var _ = this.Marker(); /*span*/ } }

                public static class Probe { public static void Run() { new Job().Execute(); new Span().Execute(); } }
            }
            """;

        Assert.Equal(new[]
        {
            $"Job.Execute ({LineOf(source, "job")})",
            $"Span.Execute ({LineOf(source, "span")})",
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
