using Xunit;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

// The Unity compiler runs the DLLs committed into the package, not the project output the other tests use.
// If a DLL breaks (a bad ILRepack merge, a forbidden dependency), every other test stays green.
public class PackagedAnalyzerTests
{
    [Fact]
    public void CommittedGeneratorDll_LoadsAndOpensTheMarkersOfTheCalls()
    {
        const string source = """
            namespace Sample
            {
                public class Foo
                {
                #line 100
                    public void Run() { using var _ = this.Marker(); }
                #line 200
                    public void Named() { using var _ = this.Marker().WithName("Inner"); }
                }

                public class Box<T>
                {
                #line 300
                    public void Run() { using var _ = this.Marker(); }
                }

                public struct Job<T>
                {
                #line 400
                    public void Execute() { using var _ = this.Marker(); }
                }

                public static class Probe
                {
                    public static void Run()
                    {
                        new Foo().Run();
                        new Foo().Named();
                        new Box<int>().Run();
                        new Job<int>().Execute();
                    }
                }
            }
            """;

        using var analyzer = PackagedAnalyzer.Load(PackagedAnalyzer.GeneratorsDll);
        var generator = Assert.Single(analyzer.LoadGenerators());

        var run = GeneratorTestHost.RunProfilerMarkers(new[] { source }, generator: generator);

        Assert.Empty(run.RunResult.Diagnostics);
        GeneratorTestHost.AssertCallsBindToGenerated(run);
        Assert.Equal(
            new[] { "Foo.Run (100)", "Foo.Inner (200)", "Box<Int32>.Run (300)", "Job<T>.Execute (400)" },
            GeneratorTestHost.Execute(run, "Sample.Probe"));
    }

    [Theory]
    [InlineData(PackagedAnalyzer.GeneratorsDll)]
    [InlineData(PackagedAnalyzer.AnalyzersDll)]
    public void CommittedDll_NeedsNothingBeyondTheCompiler(string dll)
    {
        using var metadata = new PEReader(File.OpenRead(PackagedAnalyzer.FindDll(dll)));
        var reader = metadata.GetMetadataReader();

        var assemblies = reader.AssemblyReferences
            .Select(handle => reader.GetString(reader.GetAssemblyReference(handle).Name))
            .ToArray();

        // Aspid.Generators.Helper* are merged into the generator; Unity loads the DLL without its neighbours.
        Assert.DoesNotContain(assemblies, name => name.StartsWith("Aspid.Generators.Helper"));

        // SourceGenerator.Foundations injects a module initializer that writes to Console and deadlocks Unity's compiler.
        Assert.DoesNotContain(assemblies, name => name.Contains("Foundations"));

        var types = reader.TypeReferences
            .Select(handle => reader.GetTypeReference(handle))
            .Select(type => reader.GetString(type.Namespace) + "." + reader.GetString(type.Name))
            .ToArray();

        // Neither the analyzer code nor a merged dependency may write to Console.
        Assert.DoesNotContain("System.Console", types);
    }

    [Theory]
    [InlineData("Aspid.FastTools.Generators/Aspid.FastTools.Generators/Aspid.FastTools.Generators.csproj")]
    [InlineData("Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.csproj")]
    public void ProjectFile_DoesNotReferenceSourceGeneratorFoundations(string project)
    {
        var path = Path.Combine(PackagedAnalyzer.FindRepositoryRoot(), project);
        var packages = XDocument.Load(path)
            .Descendants("PackageReference")
            .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
            .ToArray();

        Assert.NotEmpty(packages);
        Assert.DoesNotContain(packages, package => package.Contains("Foundations"));
    }
}
