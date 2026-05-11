# Aspid.FastTools.Generators

Standalone .NET solution containing Roslyn source generators for the `com.aspid.fasttools` Unity package.

## Commands

```bash
# Build and auto-deploy DLL into Unity package
dotnet build -c Release

# Run generator unit tests
dotnet test
```

`Directory.Build.targets` copies the compiled DLL to `../Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll` on build.

A repo-level PostToolUse hook (`.claude/hooks/rebuild-generators-on-change.sh`) also runs `dotnet build` automatically after any `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`. The hook intentionally **does not** trigger for tests, the Sample project, or Unity-side edits — keep that scope if you modify it.

## Solution Structure

```
Aspid.FastTools.Generators/        ← generator implementation
Aspid.FastTools.Generators.Tests/  ← unit tests + GeneratorTestHost helper
Aspid.FastTools.Generators.Sample/ ← manual smoke-test project
Aspid.FastTools.Generators.sln
Directory.Build.targets
```

## Target Framework

`netstandard2.0` — required by Roslyn. No Unity assemblies, no runtime packages.

## Dependencies

| Package | Role |
|---|---|
| `Microsoft.CodeAnalysis.CSharp` 4.3.0 | Roslyn semantic model and syntax |
| `Aspid.Generators.Helper` | `CodeWriter` utility for emitting source |
| `Aspid.Generators.Helper.Unity` | Unity-specific analysis helpers |
| `SourceGenerator.Foundations` 2.0.13 | Incremental generator infrastructure |

## Generator Implementation Pattern

All generators implement `IIncrementalGenerator` (never the deprecated `ISourceGenerator`).

### Three-Stage Pipeline

```
Predicate (SyntaxNode) → Transform (SemanticModel) → GenerateCode (SourceProductionContext)
```

**1. Predicate** — cheap syntax-only filter, no semantic model:
```csharp
private static bool Predicate(SyntaxNode node, CancellationToken _)
{
    if (node is not StructDeclarationSyntax s) return false;
    return s.BaseList is { Types.Count: > 0 };
}
```

Keep predicates broad enough that diagnostics for malformed declarations (e.g. `IId` struct without `partial`) are still reachable in Transform — they need the semantic model to decide whether to emit a diagnostic vs. silently skip.

**2. Transform** — semantic extraction. Returns a value-equatable `readonly struct` (data and/or diagnostic) or `default` to skip:
```csharp
private static IdStructResult Transform(GeneratorSyntaxContext ctx, CancellationToken ct)
{
    var symbol = ctx.SemanticModel.GetDeclaredSymbol(structDecl, ct) as INamedTypeSymbol;
    // resolve IId by metadata name, check AllInterfaces
    // emit diagnostic if precondition fails, otherwise wrap data
    return new IdStructResult(new IdStructData(symbol), diagnostic: null);
}
```

Always pass `CancellationToken` through to semantic-model APIs that accept it (`GetDeclaredSymbol`, `GetSymbolInfo`, `GetEnclosingSymbol`).

**3. GenerateCode** — emit source using `CodeWriter`, report diagnostics:
```csharp
private static void Emit(SourceProductionContext context, IdStructResult result)
{
    if (result.Diagnostic is { } d) context.ReportDiagnostic(d.ToDiagnostic());
    if (result.Data is { } data) IdStructBody.GenerateCode(context, data);
}
```

### Data Structures — value-equality is mandatory

Pipeline data passed between stages **must be value-equatable**. Roslyn caches results by comparing them with `Equals`; if equality is reference-based (the default for `readonly struct` containing reference fields), the cache never hits and the generator re-runs on every keystroke. Worse, holding `ISymbol` keeps the source `Compilation` alive.

**Forbidden in data structs:** any `ISymbol`/`INamedTypeSymbol`/`IMethodSymbol`/`SyntaxNode` field. Extract everything you need at Transform time as primitives/strings.

**Required for every data struct:**
- `readonly struct` with explicit `IEquatable<T>` implementation
- `Equals` and `GetHashCode` over every field
- For nested arrays use `ImmutableArray<T>` with element-wise comparison (or `EquatableArray<T>` if introduced)

Reference shapes:

```csharp
internal readonly struct TypeData : IEquatable<TypeData>
{
    public readonly string TypeKey;            // "Foo.Outer.Inner"
    public readonly string TypeName;
    public readonly string? Namespace;
    public readonly string ContainingTypeChain; // "Outer.Middle." or ""
    public readonly string FullyQualifiedDisplay;
    public readonly string TypeParamList;       // "<T,U>" or ""
    public readonly string ConstraintsClause;
    public readonly int Arity;
    // ... ctor + IEquatable<TypeData> Equals/GetHashCode over all fields
}

internal readonly struct IdStructData : IEquatable<IdStructData>
{
    public readonly string StructName;
    public readonly string TypeParameters;    // "<T>" or ""
    public readonly int Arity;
    public readonly string? Namespace;
    public readonly ImmutableArray<ContainingTypeInfo> ContainingTypes;
    // ... IEquatable<IdStructData> walks ContainingTypes element-wise
}
```

Cache stability is regression-tested in `IncrementalCacheTests` — that test runs each generator twice over compilations differing only in an unrelated source file, and asserts every output step is `Cached`/`Unchanged`. Adding a non-equatable field to any pipeline struct will fail it.

### Diagnostic Delivery

Generators that need to report diagnostics use a value-equatable wrapper that survives the pipeline:

```csharp
internal readonly struct DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    public readonly string DescriptorId;        // "AFID001"
    public readonly string MessageArg0;
    public readonly string? MessageArg1;
    public readonly string? FilePath;           // location is rebuilt from path + spans
    public readonly TextSpan TextSpan;
    public readonly LinePositionSpan LineSpan;

    public Diagnostic ToDiagnostic() { ... }    // looks up the DiagnosticDescriptor by id
}

internal readonly struct IdStructResult : IEquatable<IdStructResult>
{
    public readonly IdStructData? Data;
    public readonly DiagnosticInfo? Diagnostic;
    public bool IsEmpty => Data is null && Diagnostic is null;
}
```

Transform returns the wrapper; the `RegisterSourceOutput` callback emits the diagnostic and/or generates the source. Storing `Location` directly in the wrapper would defeat caching (it holds a `SyntaxTree` reference); rebuild it from `FilePath` + spans inside `ToDiagnostic()`.

Descriptors live in `Generators/{Feature}/Data/{Feature}Diagnostics.cs` with IDs prefixed `AFID` (Aspid FastTools, IdStruct domain). The csproj suppresses `RS2008` (analyzer release tracking) — this is a private in-tree generator, not a published analyzer package.

### Generated File Naming

Every hint name must include enough qualifiers to be unique across types with the same short name in different namespaces or containing types. Prefer `{ns}.{containing-chain}{Name}{aritySuffix}.{tag}.g.cs`.

| Generator | Pattern |
|---|---|
| `IdStructGenerator` | `{Namespace}.{Containing.Chain.}{StructName}{_Arity?}.IId.g.cs` |
| `ProfilerMarkersGenerator` | `{Namespace}.{Containing.Chain.}{TypeName}{_Arity?}ProfilerMarkerExtensions.g.cs` |

`{_Arity?}` is `_2`, `_3`, … for generic types (omitted when arity is 0). The containing chain is dot-joined with a trailing dot, or empty for top-level types.

The emitted `partial`/extension class names follow the same uniqueness rule; for `ProfilerMarkersGenerator` the extension class is `__{Containing_}{TypeName}{_Arity?}ProfilerMarkerExtensions` (containing chain joined with `_`).

All generated files begin with `// <auto-generated>`.

## Existing Generators

### ProfilerMarkersGenerator

Finds every `.Marker()` call site, semantically verifies it resolves to `ProfilerMarkerExtensionsForGenerator.Marker` (global-namespace class on the Unity side — calls to user-defined extensions named `Marker` are ignored), and generates `private static readonly ProfilerMarker` fields unique to each call site.

- **Field name:** `"{markerName}_Marker_Line_{line}"`. If multiple `.Marker()` calls share the same `(markerName, line)` (e.g. two calls on one source line), subsequent fields receive a `_2`/`_3` suffix and the dispatcher is updated in lockstep.
- **Marker display value:** `"{TypeName}.{member} ({line})"`. For generic enclosing types the marker value is interpolated with `typeof(T).Name` so each closed instantiation gets its own runtime label.
- **`.WithName(string)` override:** accepts string literals, plain interpolated strings without holes (`$"X"`), and survives a `(this.Marker()).WithName(...)` parenthesised receiver. Interpolated strings with substitutions are silently ignored — the generator falls back to the method name.
- **Enclosing resolution:** walks past lambdas, anonymous functions, and local functions to the nearest declared `IMethodSymbol`/`IFieldSymbol`/`IPropertySymbol`. Field initializers use the field name as the marker.
- **Release-build gating:** the dispatcher body (the `if (line is N) return …` chain) is wrapped in `#if ENABLE_PROFILER`. When the symbol is undefined (non-development player builds) the method falls through to `return default;`, so player builds pay no per-call lookup cost. The static `ProfilerMarker` field declarations are emitted unconditionally — their `Begin`/`End` calls already strip via Unity's `[Conditional("ENABLE_PROFILER")]`.

### IdStructGenerator

Finds `partial struct` types implementing `Aspid.FastTools.Ids.IId` (transitive interfaces are resolved through `INamedTypeSymbol.AllInterfaces`) and generates boilerplate: serialized `_id` field, `Id` property, editor-only `__stringId` field. All generated members carry `[GeneratedCode("Aspid.FastTools.Generators.IdStructGenerator", "1.0.0")]`.

**Supported shapes:**
- Nested types — generated code is wrapped in matching `partial class`/`partial struct`/`partial record`/`partial record struct` containing-type declarations, with full type-parameter lists (`partial class Outer<T>`).
- Generic target structs — `partial struct MyId<T> : IId` is supported; the wrapper is emitted with `<T>` and the hint name encodes the arity.
- Global namespace, file-scoped namespace, multi-level nesting, transitive `IId` implementations.

**Diagnostics:**

| Id | Title | Trigger |
|---|---|---|
| `AFID001` | IId struct must be partial | Struct implements `IId` but the declaration lacks `partial` |
| `AFID002` | Generated IId members already declared | User declares any of `_id`/`Id`/`__stringId` themselves |

When a diagnostic fires, the body is **not** emitted — the user gets a generator-level error pointing at the struct identifier instead of a CS compile error from inside the generated source.

## Conventions

- `[Generator(LanguageNames.CSharp)]` on every generator class
- Generators and their `Bodies/Data/` types are `internal sealed` / `internal`. The Tests project gets access via `<InternalsVisibleTo Include="Aspid.FastTools.Generators.Tests" />` in the csproj
- Generated members use the attribute string from `Descriptions/General.cs` (`ProfilerMarkerGeneratedCode` for ProfilerMarkers, `IdStructGeneratedCode` for IdStruct)
- Always check `IsGlobalNamespace` before emitting a `namespace` block
- Avoid LINQ in hot Predicate/Transform paths — allocations defeat incremental caching
- Diagnostic descriptor IDs use the `AFID` prefix (Aspid FastTools IdStruct); add new descriptor groups under their feature's `Data/` folder

## Test Infrastructure

`Aspid.FastTools.Generators.Tests/Helpers/GeneratorTestHost.cs` provides:

- `RunIdStruct(userSource)` / `RunProfilerMarkers(userSource)` — drive the generator over a synthesised compilation that includes the necessary stubs (`IIdDefinition`, `UnityEngineStubs`, `ProfilerMarkerStubs`)
- `AssertNoErrors(GeneratorRun)` — fails the test if either the generator emitted error-severity diagnostics **or** the resulting `Compilation.GetDiagnostics()` reports any compile error in the generated source

Use `AssertNoErrors` on every test that exercises a happy-path emission — it catches malformed C# (missing using directives, bad escapes, unbalanced blocks) for free.

`IncrementalCacheTests` verifies cache stability — when extending pipeline data, run it to confirm equality is intact.

The `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit` package is referenced but not currently used; the harness uses `CSharpGeneratorDriver` directly because the test surface is small.
