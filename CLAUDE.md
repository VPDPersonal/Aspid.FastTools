# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

**Aspid.FastTools** is a Unity package (`com.aspid.fasttools`) targeting Unity 2022.3+ that minimizes routine boilerplate code. It consists of two separate projects:

1. **`Aspid.FastTools/`** — The Unity project containing the package source (Runtime + Editor code)
2. **`Aspid.FastTools.Generators/`** — A standalone .NET solution containing Roslyn source generators

## Build Commands

### Source Generators (.NET)
```bash
# Build the generator project
cd Aspid.FastTools.Generators
dotnet build

# Run generator tests
dotnet test Aspid.FastTools.Generators/Aspid.FastTools.Generators.Tests/

# Run a single test class
dotnet test --filter "ClassName=<TestClassName>"
```

After building the generator, the resulting DLL must be copied into the Unity package's plugins folder for Unity to pick it up.

### Unity Package
Compilation is handled automatically by Unity's build system when the project is open. There are no CLI build scripts.

## Architecture

### Two-Project Separation

**Generators project** (`Aspid.FastTools.Generators/Aspid.FastTools.Generators/`):
- Contains Roslyn `IIncrementalGenerator` implementations
- Currently one generator: `ProfilerMarkersGenerator` — generates per-call-site `ProfilerMarker` registrations based on method name and line number
- Dependencies: `Aspid.Generators.Helper`, `Microsoft.CodeAnalysis.CSharp`, `SourceGenerator.Foundations`
- Test project uses XUnit + `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit`

**Unity package** (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/`):
- `Unity/Runtime/` — shipped with player builds
- `Unity/Editor/` — editor-only code, excluded from builds
- `Source/` — pure C# extensions with no Unity dependency
- `Samples~/` — optional sample packages (UPM convention: tilde prevents auto-import)

### Assembly Definitions

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, ProfilerMarkers, VisualElements |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/` | Editor: SerializedProperties, IMGUI, TypeSelector, Enums |

### Key Features and Their Locations

**ProfilerMarkers** (`Unity/Runtime/ProfilerMarkers/`): Extension method `this.Marker()` returns a `ProfilerMarker` unique to the call site. The source generator handles creating unique markers per (class, method, line).

**SerializableType** (`Unity/Runtime/Types/`): Wraps `System.Type` for Unity Inspector serialization using assembly-qualified names with lazy resolution. `SerializableType<T>` adds generic constraint support.

**TypeSelector** (`Unity/Editor/Types/`): `EditorWindow`-based hierarchical type picker with search, used as a property drawer for `SerializableType`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**SerializedProperty Extensions** (`Unity/Editor/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/`): Extensive fluent API for UIToolkit — layout, sizing, style, borders, colors, transitions, etc. Split into many partial files by category (`IStyleExtensions.cs`, `VisualElementExtensions.Style.cs`, etc.).

**IMGUI Scopes** (`Unity/Editor/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Extensions/`): `GetScriptName()` and `GetScriptNameWithIndex()` on `MonoScript` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

### Submodule

`Aspid.Internal.Unity` is a git submodule providing internal Unity helpers. It is referenced but not part of the main package distribution.
