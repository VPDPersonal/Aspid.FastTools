using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Aspid.FastTools.Generators.ProfilerMarkers;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

// Verifies that running the same generator twice over compilations that differ only
// in an *unrelated* file produces cached pipeline steps. Regression guard for the
// "ISymbol stored in data structures" anti-pattern that defeats the cache.
public class IncrementalCacheTests
{
    [Fact]
    public void ProfilerMarkers_UnrelatedSourceChange_PreservesCache()
    {
        const string targetSource = """
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { this.Marker(); }
                }
            }
            """;

        AssertCachedAfterUnrelatedEdit(targetSource, new ProfilerMarkersGenerator());
    }

    [Theory]
    [InlineData("this.Marker().WithName(\"A\")", "this.Marker().WithName(\"B\")", "Foo.B (5)")]
    [InlineData("this.Marker()", "\n        this.Marker()", "Foo.Run (6)")]
    public void ProfilerMarkers_RelevantEdit_ChangesOutput(string before, string after, string expectedLabel)
    {
        static string Source(string call) => $$"""
            namespace Sample
            {
                public class Foo
                {
                    public void Run() { using var _ = {{call}}; }
                }
            }
            """;

        var stubs = new[] { GeneratorTestHost.ProfilerMarkerStubs };
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ProfilerMarkersGenerator());

        driver = driver.RunGenerators(MakeCompilation(Source(before), "// unrelated", stubs));
        var first = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();

        driver = driver.RunGenerators(MakeCompilation(Source(after), "// unrelated", stubs));
        var second = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();

        Assert.DoesNotContain($"\"{expectedLabel}\"", first);
        Assert.Contains($"\"{expectedLabel}\"", second);
    }

    private static void AssertCachedAfterUnrelatedEdit(string targetSource, IIncrementalGenerator generator)
    {
        var stubs = new[] { GeneratorTestHost.ProfilerMarkerStubs };

        var driverOptions = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        var compilation1 = MakeCompilation(targetSource, "// version 1", stubs);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { generator.AsSourceGenerator() },
            additionalTexts: null,
            parseOptions: null,
            optionsProvider: null,
            driverOptions: driverOptions);

        driver = driver.RunGenerators(compilation1);

        // Second run with the unrelated tree edited; the target tree is unchanged.
        var compilation2 = MakeCompilation(targetSource, "// version 2", stubs);
        driver = driver.RunGenerators(compilation2);

        var result = driver.GetRunResult().Results.Single();

        // Inspect the steps from the *second* run. Steps tied to the target source
        // must show all-Cached / Unchanged outputs (the user code didn't change).
        var trackedSteps = result.TrackedOutputSteps.SelectMany(kvp => kvp.Value).ToArray();
        Assert.NotEmpty(trackedSteps);

        foreach (var step in trackedSteps)
        {
            foreach (var (_, reason) in step.Outputs)
            {
                Assert.True(
                    reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    $"Output step '{step.Name}' was '{reason}', expected Cached/Unchanged. " +
                    "This indicates a non-equatable value somewhere in the pipeline.");
            }
        }
    }

    private static CSharpCompilation MakeCompilation(string targetSource, string unrelatedSource, string[] stubs)
    {
        var trees = new[]
        {
            CSharpSyntaxTree.ParseText(targetSource, path: "Target.cs"),
            CSharpSyntaxTree.ParseText(unrelatedSource, path: "Unrelated.cs"),
        }.Concat(stubs.Select(s => CSharpSyntaxTree.ParseText(s)));

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CallerLineNumberAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly.Location),
        };

        return CSharpCompilation.Create(
            assemblyName: "TestCompilation",
            syntaxTrees: trees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
