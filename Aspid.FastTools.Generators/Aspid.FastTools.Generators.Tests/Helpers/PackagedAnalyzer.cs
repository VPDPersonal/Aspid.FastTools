using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests.Helpers;

// An analyzer DLL committed into the Unity package, loaded the way the compiler loads it: from its path, on its own.
// The test run does not see the DLL otherwise: it compiles against the project reference, not the merged file.
internal sealed class PackagedAnalyzer : AssemblyLoadContext, IAnalyzerAssemblyLoader, IDisposable
{
    public const string GeneratorsDll = "Aspid.FastTools.Generators.dll";
    public const string AnalyzersDll = "Aspid.FastTools.Analyzers.dll";

    private const string PackageFolder = "Aspid.FastTools/Packages/tech.aspid.fasttools";

    public string FilePath { get; }

    private PackagedAnalyzer(string filePath) : base($"Packaged {Path.GetFileName(filePath)}", isCollectible: true) =>
        FilePath = filePath;

    // The repository root, found from the test output upward: the folder that holds the Unity package.
    public static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, PackageFolder))) return directory.FullName;
        }

        throw new DirectoryNotFoundException($"{PackageFolder} is not above {AppContext.BaseDirectory}.");
    }

    public static string FindDll(string fileName)
    {
        var path = Path.Combine(FindRepositoryRoot(), PackageFolder, fileName);
        return File.Exists(path) ? path : throw new FileNotFoundException($"{fileName} is not committed into {PackageFolder}.", path);
    }

    public static PackagedAnalyzer Load(string fileName) => new(FindDll(fileName));

    // What the compiler does with the DLL; a failure to load it is reported through the event, not thrown.
    public ImmutableArray<ISourceGenerator> LoadGenerators()
    {
        var failures = new List<string>();
        var reference = new AnalyzerFileReference(FilePath, this);
        reference.AnalyzerLoadFailed += (_, e) => failures.Add(e.Message);

        var generators = reference.GetGenerators(LanguageNames.CSharp);
        if (failures.Count > 0)
            throw new InvalidOperationException($"{Path.GetFileName(FilePath)} failed to load: {string.Join("; ", failures)}");

        return generators;
    }

    public void Dispose() => Unload();

    void IAnalyzerAssemblyLoader.AddDependencyLocation(string fullPath) { }

    Assembly IAnalyzerAssemblyLoader.LoadFromPath(string fullPath) => LoadFromAssemblyPath(fullPath);

    // The compiler offers an analyzer the framework and Roslyn, nothing else.
    public static bool IsProvidedByCompiler(string assemblyName) =>
        assemblyName is "netstandard" or "mscorlib"
        || assemblyName.StartsWith("System", StringComparison.Ordinal)
        || assemblyName.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal);

    // The test output also holds the unmerged Aspid.Generators.Helper*, so falling back to it
    // would hide a DLL that lost its merged copy.
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var name = assemblyName.Name ?? string.Empty;

        return IsProvidedByCompiler(name)
            ? null
            : throw new FileNotFoundException($"{Path.GetFileName(FilePath)} needs {name}, which the compiler does not provide.");
    }
}
