# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

**Aspid.FastTools** is a Unity package (`com.aspid.fasttools`) targeting Unity 2022.3+ that minimizes routine boilerplate code. It consists of two separate projects:

1. **`Aspid.FastTools/`** — The Unity project containing the package source (Runtime + Editor code)
2. **`Aspid.FastTools.Generators/`** — A standalone .NET solution containing Roslyn source generators

### Unity Package
Compilation is handled automatically by Unity's build system when the project is open. There are no CLI build scripts.

## Architecture

### Two-Project Separation

**Generators project** (`Aspid.FastTools.Generators/Aspid.FastTools.Generators/`):
- Contains Roslyn `IIncrementalGenerator` implementations
- Two generators:
  - `ProfilerMarkersGenerator` — generates per-call-site `ProfilerMarker` registrations based on method name and line number
  - `IdStructGenerator` — generates boilerplate for ID struct types
- Dependencies: `Aspid.Generators.Helper`, `Microsoft.CodeAnalysis.CSharp`, `SourceGenerator.Foundations`

**Unity package** (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/`):
- `Unity/Runtime/` — shipped with player builds
- `Unity/Editor/Scripts/` — editor-only code, excluded from builds
- `Unity/Editor/Resources/Styles/` — editor USS stylesheets, named `Aspid-FastTools-{Feature}.uss`
- `Source/` — pure C# extensions with no Unity dependency
- `Samples/` — optional samples (UPM convention, imported via Package Manager)

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

### Assembly Definitions

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, ProfilerMarkers, VisualElements |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/` | Editor: SerializedProperties, IMGUI, TypeSelector, Enums |

### Key Features and Their Locations

**ProfilerMarkers** (`Unity/Runtime/ProfilerMarkers/`): Extension method `this.Marker()` returns a `ProfilerMarker` unique to the call site. The source generator handles creating unique markers per (class, method, line).

**SerializableType** (`Unity/Runtime/Types/`): Wraps `System.Type` for Unity Inspector serialization using assembly-qualified names with lazy resolution. `SerializableType<T>` adds generic constraint support.

**TypeSelector** (`Unity/Editor/Scripts/Types/`): `EditorWindow`-based hierarchical type picker with search, used as a property drawer for `SerializableType`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**StringIds** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): String-based ID system with `StringIdRegistry` (ScriptableObject), `[UniqueId]` attribute, and the `IId` interface. Editor side includes selector window, create/rename dialogs, registry editor, and property drawers. The `IdStructGenerator` generates boilerplate for ID struct types.

**SerializedProperty Extensions** (`Unity/Editor/Scripts/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/Extensions/`): Extensive fluent API for UIToolkit — layout, sizing, style, borders, colors, transitions, callbacks, USS, etc. Split into many partial files by category (`IStyleExtensions.cs`, `VisualElementExtensions.Style.cs`, `VisualElementExtensions.RegisterCallback.cs`, `VisualElementExtensions.Uss.cs`, plus element-specific files for `Field`, `Foldout`, `HelpBox`, `Image`, `ListView`, `TextElement`). Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/`.

**IMGUI Scopes** (`Unity/Editor/Scripts/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Scripts/Extensions/`): `GetScriptName()` and `GetScriptNameWithIndex()` on `MonoScript` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

### Editor Code Conventions

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**USS stylesheets:** Loaded via `.AddStyleSheetsFromResource("Styles/Aspid-FastTools-{Feature}")`. Classes named `aspid-fasttools-{feature}-{element}` (kebab-case). Styling goes in USS; code only applies `.AddClass()`.

### Submodule

`Aspid.Internal.Unity` is a git submodule providing internal Unity helpers. It is referenced but not part of the main package distribution.
