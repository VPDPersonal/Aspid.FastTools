using Xunit;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        var fieldDeclarations = Regex.Matches(
            text, @"static\s+readonly\s+global::Unity\.Profiling\.ProfilerMarker\s+(\w+)\s*=");
        Assert.Equal(2, fieldDeclarations.Count);

        var name1 = fieldDeclarations[0].Groups[1].Value;
        var name2 = fieldDeclarations[1].Groups[1].Value;
        Assert.NotEqual(name1, name2);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void TwoCallsOnSameLine_ShareTheFirstMarker()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); this.Marker(); }
                }

                public static class Probe { public static void Run() => new Foo().Run(); }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = GeneratorTestHost.GeneratedText(run);

        // [CallerLineNumber] cannot tell the calls apart, so the second one gets no field of its own.
        var fieldDeclarations = Regex.Matches(
            text, @"static\s+readonly\s+global::Unity\.Profiling\.ProfilerMarker\s+(\w+)\s*=");
        Assert.Single(fieldDeclarations);

        Assert.Equal(new[] { "Foo.Run (5)", "Foo.Run (5)" }, GeneratorTestHost.Execute(run, "Sample.Probe"));
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
        // yet the type name must still be resolved per closed instantiation via typeof(T).
        Assert.Contains("Brace", text);
        Assert.Contains("__TypeName(typeof(T))", text);

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
    public void EventAccessors_UseEventNameAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public event System.Action Changed
                    {
                        add { this.Marker(); }
                        remove { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        // add and remove are on different lines, so they get distinct fields and markers.
        var fields = Regex.Matches(text, @"\bChanged_Marker_Line_\d+\b")
            .Select(m => m.Value).Distinct().ToArray();
        var names = Regex.Matches(text, @"""Foo\.Changed \(\d+\)""")
            .Select(m => m.Value).Distinct().ToArray();
        Assert.Equal(2, fields.Length);
        Assert.Equal(2, names.Length);
        Assert.DoesNotContain("add_Changed", text);
        Assert.DoesNotContain("remove_Changed", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void ExplicitInterfaceEvent_UsesEventNameAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public interface INotifier { event System.Action Changed; }
                public class Foo : INotifier
                {
                    event System.Action INotifier.Changed
                    {
                        add { this.Marker(); }
                        remove { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Changed_Marker_Line_", text);
        Assert.Contains("\"Foo.Changed (", text);
        Assert.DoesNotContain("INotifier.Changed (", text);
        Assert.DoesNotContain("add_Changed", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void MarkerFields_AreOnlyCompiledWithEnableProfiler()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); }
                }

                public class Bar<T>
                {
                    public void Run() { this.Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source, enableProfiler: false);

        foreach (var generated in run.RunResult.Results[0].GeneratedSources)
        {
            var root = generated.SyntaxTree.GetRoot();

            // Without ENABLE_PROFILER the fields and Markers<T> are disabled text, not declarations.
            Assert.Empty(root.DescendantNodes().OfType<FieldDeclarationSyntax>());
            Assert.DoesNotContain(
                root.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                c => c.Identifier.ValueText == "Markers");
        }

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
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
                    public static readonly int _count = Count(new Foo().Marker());

                    private static int Count(Unity.Profiling.ProfilerMarker.AutoScope scope) => 1;
                }

                public static class Probe { public static int Run() => Foo._count; }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        Assert.Contains("_count_Marker_Line_", generated[0].SourceText.ToString());
        GeneratorTestHost.AssertCallsBindToGenerated(run);
        Assert.Equal(new[] { "Foo._count (5)" }, GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("private protected")]
    public void InaccessibleNestedType_IsSkipped_AndCompiles(string accessibility)
    {
        var source = $$"""
            namespace Sample
            {
                public class Outer
                {
                    {{accessibility}} class Inner
                    {
                        public void Run() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        // The generated class is top-level and cannot name the nested type, so no overload is emitted:
        // the call binds to the object fallback, and AFT0010 reports the lost marker.
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void PublicTypeNestedInPrivateType_IsSkipped_AndCompiles()
    {
        const string source = """
            namespace Sample
            {
                public class Outer
                {
                    private class Middle
                    {
                        public class Inner
                        {
                            public void Run() { this.Marker(); }
                        }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
        GeneratorTestHost.AssertNoErrors(run);
    }

    [Theory]
    [InlineData("internal")]
    [InlineData("protected internal")]
    public void AssemblyVisibleNestedType_GeneratesExtensionClass(string accessibility)
    {
        var source = $$"""
            namespace Sample
            {
                public class Outer
                {
                    {{accessibility}} class Inner
                    {
                        public void Run() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        Assert.Single(run.RunResult.Results[0].GeneratedSources);
        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void NonGenericTypeNestedInGenericType_CarriesOuterTypeParameters()
    {
        const string source = """
            namespace Sample
            {
                public class Outer<T>
                {
                    public class Inner
                    {
                        public void Run() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Marker<T>(this global::Sample.Outer<T>.Inner", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void GenericTypeNestedInGenericType_CarriesAllTypeParametersAndConstraints()
    {
        const string source = """
            namespace Sample
            {
                public class Outer<T> where T : class
                {
                    public class Inner<U> where U : T
                    {
                        public void Run() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Marker<T, U>(this global::Sample.Outer<T>.Inner<U>", text);
        // Only the nested type's own parameters appear in the label.
        Assert.Contains("__TypeName(typeof(U))", text);
        Assert.DoesNotContain("__TypeName(typeof(T))", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void SameNestedName_UnderGenericAndNonGenericOuter_NoCollision()
    {
        const string source = """
            namespace Sample
            {
                public class Outer { public class Inner { public void Run() { this.Marker(); } } }
                public class Outer<T> { public class Inner { public void Run() { this.Marker(); } } }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Equal(2, generated.Length);
        Assert.NotEqual(generated[0].HintName, generated[1].HintName);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void NestedTypeParameterShadowingOuter_IsSkipped_AndCompiles()
    {
        const string source = """
            namespace Sample
            {
                public class Outer<T>
                {
            #pragma warning disable CS0693
                    public class Inner<T>
            #pragma warning restore CS0693
                    {
                        public void Run() { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);

        // Marker<T, T> would be a duplicate type parameter — the type is skipped and AFT0010 reports it.
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void IndexerAccessors_UseIndexerAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    public int this[int index]
                    {
                        get { this.Marker(); return 0; }
                        set { this.Marker(); }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Indexer_Marker_Line_", text);
        Assert.Contains("\"Foo.Indexer (", text);
        Assert.DoesNotContain("this[]", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void ExplicitInterfaceIndexer_UsesIndexerAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public interface IList { int this[int index] { get; } }
                public class Foo : IList
                {
                    int IList.this[int index] { get { this.Marker(); return 0; } }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("Indexer_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
    }

    [Fact]
    public void StaticConstructor_UsesStaticCtorAsMarkerName()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                    static Foo() { new Foo().Marker(); }
                }
            }
            """;

        var run = GeneratorTestHost.RunProfilerMarkers(source);
        var text = run.RunResult.Results[0].GeneratedSources[0].SourceText.ToString();

        Assert.Contains("StaticCtor_Marker_Line_", text);

        GeneratorTestHost.AssertNoErrors(run);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
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
