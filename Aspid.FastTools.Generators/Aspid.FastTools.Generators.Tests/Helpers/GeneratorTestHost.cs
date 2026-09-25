using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.ProfilerMarkers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests.Helpers;

internal static class GeneratorTestHost
{
    public const string ProfilerMarkerStubs = """
        namespace Unity.Profiling
        {
            public struct ProfilerMarker
            {
                public ProfilerMarker(string _) { }
                public AutoScope Auto() => default;
                public struct AutoScope : System.IDisposable { public void Dispose() { } }
            }
        }

        public static class ProfilerMarkerExtensionsForGenerator
        {
            public static Unity.Profiling.ProfilerMarker.AutoScope Marker(this object _) => default;
            public static Unity.Profiling.ProfilerMarker.AutoScope WithName(this in Unity.Profiling.ProfilerMarker.AutoScope marker, string _) => marker;
        }
        """;

    // The generated code keeps its markers under ENABLE_PROFILER, which the Editor defines,
    // so it is on by default: otherwise the compile checks would skip the marker fields entirely.
    public static GeneratorRun RunProfilerMarkers(string userSource, bool enableProfiler = true)
    {
        var parseOptions = CSharpParseOptions.Default
            .WithPreprocessorSymbols(enableProfiler ? new[] { "ENABLE_PROFILER" } : System.Array.Empty<string>());

        var compilation = BuildCompilation(new[] { userSource, ProfilerMarkerStubs }, parseOptions);
        return Run(compilation, new ProfilerMarkersGenerator(), parseOptions);
    }

    public static void AssertNoErrors(GeneratorRun run)
    {
        var generatorErrors = run.RunResult.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.True(
            generatorErrors.Length == 0,
            "Generator emitted errors: " + string.Join("; ", generatorErrors.Select(d => d.ToString())));

        var compileErrors = run.OutputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.True(
            compileErrors.Length == 0,
            "Generated source has compile errors: " + string.Join("; ", compileErrors.Select(d => d.ToString())));
    }

    // Every Marker() call in the user source must bind to a generated overload; one left on the
    // ProfilerMarkerExtensionsForGenerator fallback compiles but never opens a marker.
    public static void AssertCallsBindToGenerated(GeneratorRun run)
    {
        var userTree = run.OutputCompilation.SyntaxTrees.First();
        var model = run.OutputCompilation.GetSemanticModel(userTree);
        var calls = userTree.GetRoot().DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(i => i.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Marker" })
            .ToArray();

        Assert.NotEmpty(calls);
        foreach (var call in calls)
        {
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            Assert.True(
                method is not null && method.ContainingType.Name.EndsWith("ProfilerMarkerExtensions"),
                $"'{call}' binds to '{method?.ContainingType.Name ?? "nothing"}', not to a generated overload");
        }
    }

    private static GeneratorRun Run(CSharpCompilation compilation, IIncrementalGenerator generator, CSharpParseOptions parseOptions)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return new GeneratorRun(driver, driver.GetRunResult(), output);
    }

    private static CSharpCompilation BuildCompilation(IEnumerable<string> sources, CSharpParseOptions parseOptions)
    {
        var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s, parseOptions));
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

internal readonly struct GeneratorRun
{
    public readonly GeneratorDriver Driver;
    public readonly GeneratorDriverRunResult RunResult;
    public readonly Compilation OutputCompilation;

    public GeneratorRun(GeneratorDriver driver, GeneratorDriverRunResult runResult, Compilation outputCompilation)
    {
        Driver = driver;
        RunResult = runResult;
        OutputCompilation = outputCompilation;
    }
}
