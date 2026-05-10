using Xunit;
using System.Linq;
using Microsoft.CodeAnalysis;
using Aspid.FastTools.Generators.Tests.Helpers;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Generators.Tests;

public class IdStructGeneratorTests
{
    [Fact]
    public void Generator_DoesNotCrash_OnEmptySource()
    {
        var run = GeneratorTestHost.RunIdStruct("namespace Test { }");

        Assert.Empty(run.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
    }

    [Fact]
    public void Struct_WithIId_InNamespace_GeneratesIdFieldAndProperty()
    {
        const string source = """
            namespace Sample
            {
                public partial struct Foo : global::Aspid.FastTools.Ids.IId { }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        Assert.EndsWith(".IId.g.cs", generated[0].HintName);

        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial struct Foo", text);
        Assert.Contains("private int _id;", text);
        Assert.Contains("public int Id =>", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_WithoutPartial_EmitsAFID001AndDoesNotGenerate()
    {
        const string source = """
            namespace Sample
            {
                public struct Foo : global::Aspid.FastTools.Ids.IId
                {
                    public int Id => 0;
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);

        Assert.Empty(run.RunResult.Results[0].GeneratedSources);

        var diags = run.RunResult.Diagnostics;
        Assert.Contains(diags, d => d.Id == "AFID001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Struct_WithExistingId_EmitsAFID002AndDoesNotGenerate()
    {
        const string source = """
            namespace Sample
            {
                public partial struct Foo : global::Aspid.FastTools.Ids.IId
                {
                    private int _id;
                    public int Id => _id;
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);

        Assert.Empty(run.RunResult.Results[0].GeneratedSources);

        var afid002 = run.RunResult.Diagnostics.FirstOrDefault(d => d.Id == "AFID002");
        Assert.NotNull(afid002);
        Assert.Equal(DiagnosticSeverity.Error, afid002!.Severity);
        var msg = afid002.GetMessage();
        Assert.Contains("_id", msg);
        Assert.Contains("Id", msg);
    }

    [Fact]
    public void Struct_WithoutIId_NotGenerated()
    {
        const string source = """
            namespace Sample
            {
                public interface IMarker { }
                public partial struct Foo : IMarker { }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);

        Assert.Empty(run.RunResult.Results[0].GeneratedSources);
    }

    [Fact]
    public void TwoStructsSameName_DifferentNamespaces_NoHintNameCollision()
    {
        const string source = """
            namespace SampleA
            {
                public partial struct Foo : global::Aspid.FastTools.Ids.IId { }
            }

            namespace SampleB
            {
                public partial struct Foo : global::Aspid.FastTools.Ids.IId { }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Equal(2, generated.Length);

        var hintNames = generated.Select(s => s.HintName).ToArray();
        Assert.NotEqual(hintNames[0], hintNames[1]);
        Assert.Contains(hintNames, h => h.Contains("SampleA"));
        Assert.Contains(hintNames, h => h.Contains("SampleB"));

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void NestedStruct_WrappedInPartialOuterClass()
    {
        const string source = """
            namespace Sample
            {
                public partial class Outer
                {
                    public partial struct Inner : global::Aspid.FastTools.Ids.IId { }
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial class Outer", text);
        Assert.Contains("partial struct Inner", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void NestedStruct_InGenericOuter_EmitsTypeParameters()
    {
        const string source = """
            namespace Sample
            {
                public partial class Outer<T>
                {
                    public partial struct Inner : global::Aspid.FastTools.Ids.IId { }
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial class Outer<T>", text);
        Assert.Contains("partial struct Inner", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_InGlobalNamespace_GeneratesWithoutNamespaceBlock()
    {
        const string source = "public partial struct GlobalFoo : global::Aspid.FastTools.Ids.IId { }";

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial struct GlobalFoo", text);
        Assert.DoesNotContain("namespace ", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_FileScopedNamespace_Generates()
    {
        const string source = """
            namespace Sample;

            public partial struct Foo : global::Aspid.FastTools.Ids.IId { }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("namespace Sample", text);
        Assert.Contains("partial struct Foo", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_ThreeLevelNesting_WrappedInCorrectOrder()
    {
        const string source = """
            namespace Sample
            {
                public partial class Outer
                {
                    public partial class Middle
                    {
                        public partial struct Inner : global::Aspid.FastTools.Ids.IId { }
                    }
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial class Outer", text);
        Assert.Contains("partial class Middle", text);
        Assert.Contains("partial struct Inner", text);

        // Verify Outer wraps Middle wraps Inner.
        var outerIdx = text.IndexOf("partial class Outer", System.StringComparison.Ordinal);
        var middleIdx = text.IndexOf("partial class Middle", System.StringComparison.Ordinal);
        var innerIdx = text.IndexOf("partial struct Inner", System.StringComparison.Ordinal);
        Assert.True(outerIdx < middleIdx && middleIdx < innerIdx, "Wrappers must be ordered Outer → Middle → Inner");

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_TransitiveIId_Generates()
    {
        const string source = """
            namespace Sample
            {
                public interface IMyId : global::Aspid.FastTools.Ids.IId { }
                public partial struct Foo : IMyId { }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial struct Foo", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_GenericTarget_EmitsTypeParameters()
    {
        const string source = """
            namespace Sample
            {
                public partial struct MyId<T> : global::Aspid.FastTools.Ids.IId { }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial struct MyId<T>", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void Struct_RecordStructContaining_GeneratesRecordWrapper()
    {
        const string source = """
            namespace Sample
            {
                public partial record struct Outer
                {
                    public partial struct Inner : global::Aspid.FastTools.Ids.IId { }
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Single(generated);
        var text = generated[0].SourceText.ToString();
        Assert.Contains("partial record struct Outer", text);
        Assert.Contains("partial struct Inner", text);

        GeneratorTestHost.AssertNoErrors(run);
    }

    [Fact]
    public void TwoStructs_InGenericOuters_DifferentArity_NoHintCollision()
    {
        const string source = """
            namespace Sample
            {
                public partial class Outer<T>
                {
                    public partial struct MyId : global::Aspid.FastTools.Ids.IId { }
                }

                public partial class Outer<T, U>
                {
                    public partial struct MyId : global::Aspid.FastTools.Ids.IId { }
                }
            }
            """;

        var run = GeneratorTestHost.RunIdStruct(source);
        var generated = run.RunResult.Results[0].GeneratedSources;

        Assert.Equal(2, generated.Length);

        var hintNames = generated.Select(s => s.HintName).ToArray();
        Assert.NotEqual(hintNames[0], hintNames[1]);

        var combined = string.Concat(generated.Select(s => s.SourceText.ToString()));
        Assert.Contains("partial class Outer<T>", combined);
        Assert.Contains("partial class Outer<T, U>", combined);

        GeneratorTestHost.AssertNoErrors(run);
    }
}
