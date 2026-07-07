using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

public class ProfilerMarkersGeneratorTests
{
    [Fact]
    public void Generator_DoesNotCrash_OnEmptySource()
    {
        var run = GeneratorTestHost.RunProfilerMarkers("namespace Test { }");

        Assert.Empty(run.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
    }

    [Fact]
    public void SingleMarkerCall_GeneratesExtensionClass()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("internal static class __FooProfilerMarkerExtensions", text);
        Assert.Contains("Run_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void TwoCallsOnDifferentLines_GenerateTwoFields()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run()
                    {
                        this.Marker();
                        this.Marker();
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Two distinct fields, named after method + their line numbers.
        var fieldDeclarations = System.Text.RegularExpressions.Regex.Matches(
            text, @"static\s+readonly\s+global::Unity\.Profiling\.ProfilerMarker\s+(\w+)\s*=");
        Assert.Equal(2, fieldDeclarations.Count);

        var name1 = fieldDeclarations[0].Groups[1].Value;
        var name2 = fieldDeclarations[1].Groups[1].Value;
        Assert.NotEqual(name1, name2);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void TwoCallsOnSameLine_DedupesFieldNames()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Two distinct fields even though they're on the same source line.
        var fieldDeclarations = System.Text.RegularExpressions.Regex.Matches(
            text, @"static\s+readonly\s+global::Unity\.Profiling\.ProfilerMarker\s+(\w+)\s*=");
        Assert.Equal(2, fieldDeclarations.Count);

        var name1 = fieldDeclarations[0].Groups[1].Value;
        var name2 = fieldDeclarations[1].Groups[1].Value;
        Assert.NotEqual(name1, name2);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameLiteral_OverridesMarkerLabel()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker().WithName("CustomLabel"); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("CustomLabel", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameInterpolated_PlainText_OverridesMarkerLabel()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker().WithName($"MyMarker"); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("MyMarker", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameAfterParenthesis_OverridesMarkerLabel()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { (this.Marker()).WithName("ParenLabel"); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("ParenLabel", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameInterpolatedWithVariable_FallsBackToMethodName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { var x = 42; this.Marker().WithName($"X{x}"); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Variable interpolation can't be evaluated at compile time → label is the method name.
        Assert.Contains("Run_Marker_Line_", text);
        Assert.DoesNotContain("\"X{x}\"", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameWithSpecialCharacters_EscapesGeneratedLiteral()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker().WithName("A \"quoted\" \\ label"); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Quotes and backslashes in the label must be escaped; AssertNoErrors proves the emitted
        // marker literal is valid C# rather than a broken string that fails to compile.
        Assert.Contains("quoted", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void WithNameWithBraces_OnGenericType_EscapesGeneratedLiteral()
    {
        const string source = """
            namespace Sample
            {
                public class Foo<T>
                {
                    public void Run() { this.Marker().WithName("Brace{x} and \"q\""); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Braces in the label must not be treated as interpolation holes on the generic path,
        // yet the type name must still be resolved per closed instantiation via typeof(T).Name.
        Assert.Contains("Brace", text);
        Assert.Contains("typeof(T).Name", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Constructor_UsesCtorAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public Foo() { this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Ctor_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void PropertyGetter_UsesPropertyNameAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public int Value
                    {
                        get { this.Marker(); return 0; }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Value_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void GenericClass_EmitsTypeParameters()
    {
        const string source = """
            namespace Sample
            {
                public class Foo<T>
                {
                    public void Run() { this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("__Foo_1ProfilerMarkerExtensions", text);
        Assert.Contains("Markers<T>", text);
        Assert.Contains("Marker<T>(this global::Sample.Foo<T>", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void GlobalNamespace_GeneratesWithoutNamespaceBlock()
    {
        const string source = """
            public class Foo
            {
                public void Run() { this.Marker(); }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.DoesNotContain("namespace ", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void TwoTypesSameName_DifferentNamespaces_NoHintCollision()
    {
        const string source = """
            namespace SampleA
            {
                public class Foo { public void Run() { this.Marker(); } }
            }
            namespace SampleB
            {
                public class Foo { public void Run() { this.Marker(); } }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Equal(2, generated.Length);
        var hintNames = generated.Select(s => s.HintName).ToArray();
        Assert.NotEqual(hintNames[0], hintNames[1]);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void NestedTypesSameName_DifferentOuters_NoHintCollision()
    {
        const string source = """
            namespace Sample
            {
                public class OuterA { public class Inner { public void Run() { this.Marker(); } } }
                public class OuterB { public class Inner { public void Run() { this.Marker(); } } }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Equal(2, generated.Length);
        var hintNames = generated.Select(s => s.HintName).ToArray();
        Assert.NotEqual(hintNames[0], hintNames[1]);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Lambda_UsesContainingMethodAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run()
                    {
                        System.Action a = () => this.Marker();
                        a();
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // Marker name must be the enclosing real method, not the synthesized lambda symbol.
        Assert.Contains("Run_Marker_Line_", text);
        Assert.DoesNotContain("<>", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void LocalFunction_UsesContainingMethodAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run()
                    {
                        Local();
                        void Local() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Run_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void FieldInitializer_UsesFieldNameAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    private static readonly int _count = ((object)null).Marker() is var _ ? 1 : 0;
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        GeneratorTestHost.AssertNoErrors(run);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("_count_Marker_Line_", text);
    }

    [Fact]
    public void UnrelatedMarkerExtension_IsNotProcessed()
    {
        // User defines their own Marker() extension on a custom type — the generator
        // must NOT mistake it for ProfilerMarkerExtensionsForGenerator.Marker.
        const string source = """
            namespace Sample
            {
                public class Foo { }
                public static class FooExtensions
                {
                    public static int Marker(this Foo _) => 0;
                }
                public class Caller
                {
                    public void Run() { var x = new Foo().Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Dispatcher_IsGatedByEnableProfiler_AndFallsBackToDefault()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("#if ENABLE_PROFILER", text);
        Assert.Contains("#endif", text);
        Assert.Contains("return default;", text);
        Assert.DoesNotContain("throw new", text);

        GeneratorTestHost.AssertNoErrors(run);
    }
}
