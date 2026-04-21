using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Aspid.FastTools.Generators.IdStruct;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests
{
    public class IdStructGeneratorTests
    {
        private const string IIdDefinition = "namespace Aspid.FastTools { public interface IId { int Id { get; } } }";

        private static GeneratorDriverRunResult RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var iidTree = CSharpSyntaxTree.ParseText(IIdDefinition);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create("TestCompilation",
                syntaxTrees: new[] { syntaxTree, iidTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new IdStructGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

            return driver.GetRunResult();
        }

        [Fact]
        public void Test_Generator_DoesNotCrash_OnEmptySource()
        {
            var result = RunGenerator("namespace Test { }");

            Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Empty(result.Results[0].GeneratedSources);
        }

        [Fact]
        public void Struct_WithIId_InNamespace_GeneratesIdFieldAndProperty()
        {
            const string source = @"
namespace Sample
{
    public partial struct Foo : global::Aspid.FastTools.IId { }
}
";
            var result = RunGenerator(source);
            var generated = result.Results[0].GeneratedSources;

            Assert.Single(generated);
            var source0 = generated[0];

            Assert.EndsWith(".IId.g.cs", source0.HintName);
            var text = source0.SourceText.ToString();
            Assert.Contains("partial struct Foo", text);
            Assert.Contains("private int _id;", text);
            Assert.Contains("public int Id =>", text);
        }

        [Fact]
        public void Struct_WithoutPartial_NotGenerated()
        {
            const string source = @"
namespace Sample
{
    public struct Foo : global::Aspid.FastTools.IId
    {
        public int Id => 0;
    }
}
";
            var result = RunGenerator(source);
            var generated = result.Results[0].GeneratedSources;

            Assert.Empty(generated);
        }

        [Fact]
        public void Struct_WithoutIId_NotGenerated()
        {
            const string source = @"
namespace Sample
{
    public interface IMarker { }
    public partial struct Foo : IMarker { }
}
";
            var result = RunGenerator(source);
            var generated = result.Results[0].GeneratedSources;

            Assert.Empty(generated);
        }

        [Fact]
        public void TwoStructsSameName_DifferentNamespaces_NoHintNameCollision()
        {
            const string source = @"
namespace SampleA
{
    public partial struct Foo : global::Aspid.FastTools.IId { }
}

namespace SampleB
{
    public partial struct Foo : global::Aspid.FastTools.IId { }
}
";
            var result = RunGenerator(source);
            var generated = result.Results[0].GeneratedSources;

            Assert.Equal(2, generated.Length);

            var hintNames = generated.Select(s => s.HintName).ToArray();
            Assert.NotEqual(hintNames[0], hintNames[1]);
            Assert.Contains(hintNames, h => h.Contains("SampleA"));
            Assert.Contains(hintNames, h => h.Contains("SampleB"));
        }

        [Fact]
        public void NestedStruct_WrappedInPartialOuterClass()
        {
            const string source = @"
namespace Sample
{
    public partial class Outer
    {
        public partial struct Inner : global::Aspid.FastTools.IId { }
    }
}
";
            var result = RunGenerator(source);
            var generated = result.Results[0].GeneratedSources;

            Assert.Single(generated);
            var text = generated[0].SourceText.ToString();
            Assert.Contains("partial class Outer", text);
            Assert.Contains("partial struct Inner", text);
        }
    }
}
