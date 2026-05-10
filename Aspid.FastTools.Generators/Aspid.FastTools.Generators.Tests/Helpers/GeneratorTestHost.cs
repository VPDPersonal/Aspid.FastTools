using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Aspid.FastTools.Generators.IdStruct;
using Aspid.FastTools.Generators.ProfilerMarkers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests.Helpers;

internal static class GeneratorTestHost
{
    public const string IIdDefinition = """
        namespace Aspid.FastTools.Ids
        {
            public interface IId { int Id { get; } }
        }
        """;

    public const string UnityEngineStubs = """
        namespace UnityEngine
        {
            [System.AttributeUsage(System.AttributeTargets.Field)]
            public sealed class SerializeField : System.Attribute { }
        }
        """;

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

    public static GeneratorRun RunIdStruct(string userSource)
    {
        var compilation = BuildCompilation(new[] { userSource, IIdDefinition, UnityEngineStubs });
        return Run(compilation, new IdStructGenerator());
    }

    public static GeneratorRun RunProfilerMarkers(string userSource)
    {
        var compilation = BuildCompilation(new[] { userSource, ProfilerMarkerStubs });
        return Run(compilation, new ProfilerMarkersGenerator());
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

    private static GeneratorRun Run(CSharpCompilation compilation, IIncrementalGenerator generator)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return new GeneratorRun(driver, driver.GetRunResult(), output);
    }

    private static CSharpCompilation BuildCompilation(IEnumerable<string> sources)
    {
        var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s));
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
