using Xunit;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Generators.ProfilerMarkers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests.Helpers;

internal static class GeneratorTestHost
{
    public const string StubsPath = "Stubs.cs";

    // Mirrors the runtime signatures of Unity.Profiling and ProfilerMarkerExtensionsForGenerator.
    // The ProfilerMarker stub records every marker it opens, so Execute can show which one a call hit.
    public const string ProfilerMarkerStubs = """
        namespace Unity.Profiling
        {
            public struct ProfilerMarker
            {
                public static readonly System.Collections.Generic.List<string> Opened = new System.Collections.Generic.List<string>();

                private readonly string _name;

                public ProfilerMarker(string name) { _name = name; }

                public AutoScope Auto() { Opened.Add(_name); return default; }

                public struct AutoScope : System.IDisposable { public void Dispose() { } }
            }
        }

        public static class ProfilerMarkerExtensionsForGenerator
        {
            public static Unity.Profiling.ProfilerMarker.AutoScope Marker<T>(this T instance, [System.Runtime.CompilerServices.CallerLineNumber] int line = -1) => default;
            public static Unity.Profiling.ProfilerMarker.AutoScope WithName(this in Unity.Profiling.ProfilerMarker.AutoScope marker, string name) => marker;
        }
        """;

    // Unity compiles user code as C# 9 with ENABLE_PROFILER in the Editor and development builds,
    // so that is the default; without the symbol the compile checks would skip the marker fields entirely.
    public static GeneratorRun RunProfilerMarkers(string userSource, bool enableProfiler = true) =>
        RunProfilerMarkers(new[] { userSource }, enableProfiler);

    // Without stubs the package types must come from references (see EmitStubsReference).
    public static GeneratorRun RunProfilerMarkers(
        string[] userSources,
        bool enableProfiler = true,
        MetadataReference[]? references = null,
        bool includeStubs = true)
    {
        var parseOptions = ParseOptions(enableProfiler);
        var compilation = BuildCompilation(userSources, parseOptions, "TestCompilation", references ?? System.Array.Empty<MetadataReference>(), includeStubs);
        return Run(compilation, new ProfilerMarkersGenerator(), parseOptions);
    }

    public static CSharpParseOptions ParseOptions(bool enableProfiler = true) =>
        CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.CSharp9)
            .WithPreprocessorSymbols(enableProfiler ? new[] { "ENABLE_PROFILER" } : System.Array.Empty<string>());

    public static void AssertNoErrors(GeneratorRun run)
    {
        var generatorErrors = run.RunResult.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.True(
            generatorErrors.Length == 0,
            "Generator emitted errors: " + string.Join("; ", generatorErrors.Select(d => d.ToString())));

        var diagnostics = run.OutputCompilation.GetDiagnostics();

        var compileErrors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.True(
            compileErrors.Length == 0,
            "Compilation has errors: " + string.Join("; ", compileErrors.Select(d => d.ToString())));

        // Unity projects may treat warnings as errors, so generated code must not raise any.
        var generatedTrees = run.RunResult.GeneratedTrees.ToHashSet();
        var generatedWarnings = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Warning && generatedTrees.Contains(d.Location.SourceTree!))
            .ToArray();
        Assert.True(
            generatedWarnings.Length == 0,
            "Generated source has warnings: " + string.Join("; ", generatedWarnings.Select(d => d.ToString())));
    }

    // Every Marker() call in the user sources must bind to the overload generated for the type it is written in;
    // one left on the fallback or on another type's overload compiles but opens no marker or the wrong one.
    public static void AssertCallsBindToGenerated(GeneratorRun run)
    {
        var calls = GetUserMarkerCalls(run).ToArray();
        Assert.NotEmpty(calls);

        foreach (var (call, model) in calls)
        {
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            var enclosingType = model.GetEnclosingSymbol(call.SpanStart)?.ContainingType;
            for (var t = model.GetEnclosingSymbol(call.SpanStart); t is not null; t = t.ContainingSymbol)
            {
                if (t is INamedTypeSymbol named) { enclosingType = named; break; }
            }

            var receiver = method?.ReducedFrom is not null ? method.ReceiverType : method?.Parameters.FirstOrDefault()?.Type;
            Assert.True(
                method is not null
                && method.ContainingType.Name.StartsWith("__") && method.ContainingType.Name.Contains("ProfilerMarkerExtensions")
                && SymbolEqualityComparer.Default.Equals(receiver?.OriginalDefinition, enclosingType?.OriginalDefinition),
                $"'{call}' binds to '{method?.ContainingType.Name ?? "nothing"}' for '{receiver}', not to the overload generated for '{enclosingType}'");
        }
    }

    // Every Marker() call in the user sources must bind to the package fallback: it compiles and opens no marker.
    public static void AssertCallsBindToFallback(GeneratorRun run)
    {
        var calls = GetUserMarkerCalls(run).ToArray();
        Assert.NotEmpty(calls);

        foreach (var (call, model) in calls)
        {
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            Assert.True(
                method?.ContainingType.Name == "ProfilerMarkerExtensionsForGenerator",
                $"'{call}' binds to '{method?.ContainingType.Name ?? "nothing"}', not to the fallback");
        }
    }

    // Emits the output compilation, runs the static method and returns the names of the markers it opened.
    public static IReadOnlyList<string> Execute(GeneratorRun run, string typeName, string methodName = "Run")
    {
        AssertNoErrors(run);

        using var stream = new MemoryStream();
        var emit = run.OutputCompilation.Emit(stream);
        Assert.True(emit.Success, string.Join("; ", emit.Diagnostics));

        var context = new AssemblyLoadContext("ProfilerMarkers", isCollectible: true);
        try
        {
            stream.Position = 0;
            var assembly = context.LoadFromStream(stream);

            var opened = (List<string>)assembly.GetType("Unity.Profiling.ProfilerMarker")!.GetField("Opened")!.GetValue(null)!;
            opened.Clear();

            var type = assembly.GetType(typeName) ?? throw new System.InvalidOperationException($"No type {typeName}");
            var method = type.GetMethod(methodName) ?? throw new System.InvalidOperationException($"No method {methodName}");
            method.Invoke(null, null);

            return opened.ToArray();
        }
        finally
        {
            context.Unload();
        }
    }

    public static string GeneratedText(GeneratorRun run) =>
        string.Join("\n", run.RunResult.Results[0].GeneratedSources.Select(s => s.SourceText.ToString()));

    // The stubs as their own assembly, the way the package's runtime assembly is referenced in Unity.
    public static MetadataReference EmitStubsReference() =>
        Emit(BuildCompilation(System.Array.Empty<string>(), ParseOptions(), "Stubs", System.Array.Empty<MetadataReference>(), includeStubs: true));

    // A library compiled with the generator, referencing the given assemblies instead of its own stubs.
    public static MetadataReference EmitReference(string source, string assemblyName, params MetadataReference[] references)
    {
        var parseOptions = ParseOptions();
        var compilation = BuildCompilation(new[] { source }, parseOptions, assemblyName, references, includeStubs: false);
        var run = Run(compilation, new ProfilerMarkersGenerator(), parseOptions);
        AssertNoErrors(run);
        return Emit(run.OutputCompilation);
    }

    private static MetadataReference Emit(Compilation compilation)
    {
        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        Assert.True(emit.Success, string.Join("; ", emit.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static IEnumerable<(InvocationExpressionSyntax Call, SemanticModel Model)> GetUserMarkerCalls(GeneratorRun run)
    {
        var generatedTrees = run.RunResult.GeneratedTrees.ToHashSet();
        foreach (var tree in run.OutputCompilation.SyntaxTrees)
        {
            if (generatedTrees.Contains(tree) || tree.FilePath == StubsPath) continue;

            var model = run.OutputCompilation.GetSemanticModel(tree);
            foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (call.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Marker" })
                    yield return (call, model);
            }
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

    private static CSharpCompilation BuildCompilation(
        IEnumerable<string> sources,
        CSharpParseOptions parseOptions,
        string assemblyName,
        MetadataReference[] extraReferences,
        bool includeStubs = true)
    {
        var trees = sources
            .Select((s, i) => CSharpSyntaxTree.ParseText(s, parseOptions, path: $"User{i}.cs"));

        if (includeStubs)
            trees = trees.Append(CSharpSyntaxTree.ParseText(ProfilerMarkerStubs, parseOptions, path: StubsPath));

        var trustedAssemblies = ((string)System.AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var references = trustedAssemblies
            .Where(path => Path.GetFileName(path) is "System.Private.CoreLib.dll" or "System.Runtime.dll"
                or "System.Collections.dll" or "System.Linq.dll" or "System.Linq.Expressions.dll"
                or "System.Linq.Queryable.dll" or "netstandard.dll")
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .Concat(extraReferences);

        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: trees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
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
