# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

**Aspid.FastTools** is a Unity package (`com.aspid.fasttools`) targeting Unity 2022.3+ that minimizes routine boilerplate code. It consists of two separate projects:

1. **`Aspid.FastTools/`** — The Unity project containing the package source (Runtime + Editor code)
2. **`Aspid.FastTools.Generators/`** — A standalone .NET solution containing Roslyn source generators

### Unity Package
Compilation is handled automatically by Unity's build system when the project is open. There are no CLI build scripts.

### Building & Deploying Generators

`Directory.Build.targets` auto-copies the compiled DLL into the Unity package on build:

```bash
# From Aspid.FastTools.Generators/
dotnet build -c Release
# Outputs DLL to: Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll
```

Run tests with:
```bash
# From Aspid.FastTools.Generators/
dotnet test
```

## Architecture

### Two-Project Separation

**Generators project** (`Aspid.FastTools.Generators/Aspid.FastTools.Generators/`):
- Contains Roslyn `IIncrementalGenerator` implementations
- `Aspid.FastTools.Generators.Tests/` — unit tests for generators
- `Aspid.FastTools.Generators.Sample/` — sample project for manual testing
- Two generators:
  - `ProfilerMarkersGenerator` — generates per-call-site `ProfilerMarker` registrations keyed by enclosing type + method/field/property + line number. Semantically gated to `ProfilerMarkerExtensionsForGenerator.Marker` only (user-defined `Marker()` extensions are ignored). Walks past lambdas/local-functions; supports `.WithName(literal)` and plain `$"..."` interpolated strings; deduplicates fields when multiple call sites share a line. The generated dispatcher is wrapped in `#if ENABLE_PROFILER` and falls back to `return default;`, so non-development builds carry no per-call dispatch cost.
  - `IdStructGenerator` — generates boilerplate for ID struct types. Reports `AFID001` when an `IId` struct lacks `partial`, and `AFID002` when the user already declares `_id`/`Id`/`__stringId`. Supports generic target structs and generic containing types (the wrapping `partial` is emitted with `<T,U>`).
- All pipeline data structures are value-equatable `readonly struct` with explicit `IEquatable<T>` — never store `ISymbol`/`SyntaxNode` in pipeline data (breaks Roslyn's incremental cache). `Aspid.FastTools.Generators.Tests/IncrementalCacheTests` regression-tests this.
- Dependencies: `Aspid.Generators.Helper`, `Microsoft.CodeAnalysis.CSharp`, `SourceGenerator.Foundations`

**Unity package** (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/`):
- `Unity/Runtime/` — shipped with player builds
- `Unity/Editor/Scripts/` — editor-only code, excluded from builds
- `Unity/Editor/Resources/UI/` — editor USS stylesheets, organized by domain (`UI/Components/`, `UI/Ids/`, `UI/Types/`, `UI/Enums/`, `UI/Windows/`); shared base palette at `UI/Aspid-FastTools-Default-Dark.uss`. Files follow `Aspid-FastTools-{Feature}.uss`.
- `Unity/Editor/Resources/Icons/` — editor icon assets referenced by USS via `--aspid-icons-*` variables.
- `Source/` — pure C# extensions with no Unity dependency
- `Samples~/` — optional samples (UPM convention, tilde hides from Unity importer, imported via Package Manager)

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional Mathematics integration:** Mathematics-dependent extensions live in the satellite `Aspid.FastTools.Unity.VisualElements.Math` assembly, which references `Unity.Mathematics` directly and is gated by a `versionDefines` entry that compiles it only when `com.unity.mathematics` is installed. New Mathematics-dependent code goes there. The same `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` symbol is also declared on the main runtime asmdef for the rare case when a single file in `Aspid.FastTools.Unity` needs to gate Mathematics-aware behavior.

### Assembly Definitions

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, Ids, ProfilerMarkers, VisualElements |
| `Aspid.FastTools.Unity.VisualElements.Math` | `Unity/Runtime/VisualElements/Extensions/INotifyValueChanged/Math/` | Satellite assembly compiled only when `com.unity.mathematics` is installed; hosts `INotifyValueChanged` extensions for `float2/3/4`, `int2/3/4`, etc. |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | Editor: Enums, Extensions, IMGUI, Ids, SerializedProperties, Types, VisualElements, Welcome |

### Key Features and Their Locations

**ProfilerMarkers** (`Unity/Runtime/ProfilerMarkers/`): Extension method `this.Marker()` returns a `ProfilerMarker` unique to the call site. The source generator handles creating unique markers per (class, method, line).

**SerializableType** (`Unity/Runtime/Types/`): Wraps `System.Type` for Unity Inspector serialization using assembly-qualified names with lazy resolution. `SerializableType<T>` adds generic constraint support.

**TypeSelector** (`Unity/Editor/Scripts/Types/`): `EditorWindow`-based hierarchical type picker with search, used as a property drawer for `SerializableType`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**Id Registries** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): A single `IdRegistry` (`ScriptableObject`) maps string names to stable integer IDs for a given struct type. Full `int ↔ string` mapping is available at runtime via `TryGetId` / `TryGetName` / `Contains(int)` / `Contains(string)`.

Each struct type decorated with `[UniqueId]` / implementing `IId` is bound to exactly **one** registry — uniqueness is enforced at lookup time by `IdRegistryResolver`. The resolver lazily builds a `Type AQN → registry` index on first lookup and updates it incrementally via `AssetPostprocessor`. `UniqueIdIndex` mirrors that strategy for `[UniqueId]`-field collision checks.

Editor UI lives in `RegistryEditorCore`, which talks directly to the registry's `SerializedObject`. Features: C#-identifier name validation, full Undo, explicit Clean-up flow for invalid entries (empty / duplicate names), Sort/Group toolbar, manual Next ID with backward-step warning, Open-Registry shortcut on the `IdStruct` drawer.

The `IdStructGenerator` generates the struct-side boilerplate (and reports `AFID001`/`AFID002` when the struct isn't `partial` or already declares the boilerplate members); the registry asset is created via `Assets → Create → Aspid/Id Registry/Id Registry`.

**SerializedProperty Extensions** (`Unity/Editor/Scripts/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/Extensions/`): Extensive fluent API for UIToolkit — layout, sizing, style, borders, colors, transitions, callbacks, USS, child management, etc. Organized into subdirectories by element type (`Button/`, `CallbackEventHandler/`, `Field/`, `Focusable/`, `Foldout/`, `HelpBox/`, `IMGUIContainer/`, `IMixedValueSupport/`, `INotifyValueChanged/`, `IStyle/`, `Image/`, `List/`, `ProgressBar/`, `Slider/`, `TextElement/`) plus top-level partial files (`VisualElementExtensions.cs`, `.Style.cs`, `.Style.Preset.cs`, `.Uss.cs`, `.Child.cs`). Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/`.

**Internal Editor VisualElement Components** (`Unity/Editor/Scripts/VisualElements/Internal/`): Custom UIToolkit elements for editor UI. Layout:

- `Components/` — concrete elements, each in its own subfolder:
  - `AspidAnimatedDotsBackground/`, `AspidAnimatedTitle/` — decorative animated elements.
  - `AspidAnimatedLogo/` — `AspidAnimatedLogo`, `AspidAnimatedLogoPreset`, fluent extensions, plus `Styles/` with `AspidAnimatedLogoPulseSpeedStyle`, `AspidAnimatedLogoPulseHoverAmplitudeStyle` and `AspidAnimatedLogoLayerImageStyle` (USS-driven float and Texture2D bindings).
  - `AspidDividingLines/` — `AspidDividingLine`, `AspidDividingLinePreset`, fluent extensions, plus `Styles/` with `AspidDividingLineSizeStyle` and `AspidDividingLineDirectionStyle` (USS-driven enum bindings).
  - `AspidLabels/` — `AspidLabel`, `AspidLabelPreset`, fluent extensions, plus `Styles/` with `AspidLabelSizeStyle` and `AspidLabelFontStyle`.
  - `AspidContainers/` — `AspidBox`, `AspidBoxPreset`, fluent extensions.
  - `AspidGradientButton/` — `AspidGradientButton`, `AspidGradientButtonPreset`, fluent extensions, plus `Styles/`.
  - `AspidHelpBoxes/` — `AspidHelpBox`, `AspidHelpBoxPreset`, fluent extensions.
  - `AspidHoverGradientOverlays/` — `AspidHoverGradientOverlay` and its `Styles/` (USS-driven hover-tracking overlay shared by other components).
  - `AspidInspectorHeaders/` — `AspidInspectorHeader`, `AspidInspectorHeaderPreset`, fluent extensions, plus `Styles/`.
- `Styles/` — shared helpers used across components: `AspidStyles` (USS class/property registry), `StatusStyle`, `ThemeStyle`, `InlineStyle<T>` (USS-vs-code precedence helper). The companion `ICustomStyleExtensions` (extension methods on `ICustomStyle`, including `TryGetByEnum<T>`) lives in `Unity/Runtime/VisualElements/Extensions/ICustomStyle/` since it ships with player builds and is consumed by both runtime and editor styles.

All components use `Aspid-FastTools-Default-Dark.uss` as the base stylesheet (loaded via `AspidStyles.DefaultStyleSheet`) and follow the same `.AddClass()` pattern. Theme/status/size/direction enums are exposed as nested `Type` enums on their respective `Style` structs (e.g. `ThemeStyle.Type`, `AspidLabelSizeStyle.Type`).

**IMGUI Scopes** (`Unity/Editor/Scripts/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Scripts/Extensions/`): `GetScriptName()` and `GetScriptNameWithIndex()` on `MonoScript` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

**Welcome Window** (`Unity/Editor/Scripts/Welcome/`): `WelcomeWindow` (`EditorWindow`, menu `Tools/Aspid FastTools/Welcome`) + `WelcomeWindowStartup` (auto-show on first import). UXML/USS at `Resources/UI/Windows/Welcome/Aspid-FastTools-Welcome.{uxml,uss}`. Lists installable samples by reading `package.json`.

### Editor Code Conventions

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**USS stylesheets:** Loaded via `.AddStyleSheetsFromResource("UI/{Domain}/Aspid-FastTools-{Feature}")` (e.g. `UI/Components/Aspid-FastTools-AspidLabel`, `UI/Ids/Aspid-FastTools-Id-Registry`). Component code keeps the path in a `private const string StyleSheetPath`; ID-system code centralises paths in `Constants.cs`. Styling goes in USS; code only applies `.AddClass()`. The base palette `AspidStyles.DefaultStyleSheet` (`UI/Aspid-FastTools-Default-Dark`) must be added first on new internal editor components — `AspidStyles` is the single source of truth for shared USS class/property names.

**USS class naming (BEM):** Follow Unity's recommended Block-Element-Modifier convention (see [UIE-USS-WritingStyleSheets](https://docs.unity3d.com/6000.4/Documentation/Manual/UIE-USS-WritingStyleSheets.html)).

Format: `aspid-fasttools-{block}[__{element}][--{modifier}]`

- **Prefix** `aspid-fasttools-` is mandatory and joined to the block with a single `-` (matches Unity's own `unity-foldout__toggle` style — the prefix is a namespace, not a BEM block).
- **Block** — feature/component name in kebab-case: `id-registry`, `id-drawer`, `enum-values`, `serializable-type`.
- **Element** — part of a block, joined with `__`: `aspid-fasttools-id-drawer__add-button`, `aspid-fasttools-id-registry__delete`.
- **Modifier** — state or variant, joined with `--`: `aspid-fasttools-id-registry__warning--visible`, `aspid-fasttools-status--error`.
- **kebab-case inside any segment** (`add-button`, never `addButton` or `add_button`).
- **Utility/state classes** (status, theme) are blocks of their own: `aspid-fasttools-status--error`, `aspid-fasttools-theme--dark`.

Pre-existing classes that use `-` instead of `__` between block and element (e.g. `aspid-fasttools-id-drawer-add-button`) are legacy. Migrate to BEM when touching the surrounding code; new classes must follow the rule from the start.

**USS variable naming:** USS custom properties are design tokens with a positional grammar — not BEM (variables have no block/element/modifier). Follow Unity's separator convention from built-in `--unity-*` variables: `-` between slots, `_` for compound words inside a single slot. See [UIE-USS-UnityVariables](https://docs.unity3d.com/6000.4/Documentation/Manual/UIE-USS-UnityVariables.html).

Format: `--{prefix}-{group}-{role}[-{state}][-{tone}]`

| Slot | Values | Required |
|---|---|---|
| `prefix` | `aspid` (palette shared between Aspid packages) / `aspid-fasttools` (product-specific) | yes |
| `group` | `colors` · `icons` · `metrics` · `prop` | yes |
| `role` | `bg`, `shade`, `text`, `border`, `icon`, `status`, `gradient`, `label_size`, `line_size`, `theme`, … | yes |
| `state` | `success`, `warning`, `error`, `info`, `hover`, `pressed`, … | optional |
| `tone` | `darkness`, `dark`, `light`, `lightness` | optional |

Rules:
- One word per slot, or one compound joined by `_` (`label_size`) — never two independent concepts in one slot.
- Order is `state` → `tone` (`success-darkness`, not `darkness-success`): "what is it" first, then "how bright".
- Color roles: `bg` is the surface palette; `shade` is the generic content palette (used when the role isn't specialised — text, border or icon-tint share the same shade swatch). `text`/`border`/`icon` are specialised roles for component-local variables that need their own swatch (see `Aspid-FastTools-Id-Registry-Entry-Field` style sheet for examples). `status` covers `success`/`warning`/`error`/`info` semantics.
- `prop` group is for inline component parameters (e.g. `--aspid-fasttools-prop-theme`), not palette tokens.
- Palette variables declared on `:root`; component-scoped variables on the component selector.

Examples:
```
--aspid-colors-bg-darkness                  /* surface, very dark */
--aspid-colors-shade-lightness              /* generic content, very light */
--aspid-colors-text-light                   /* component-specific text colour */
--aspid-colors-border-darkness              /* component-specific border colour */
--aspid-colors-status-success               /* status base */
--aspid-colors-status-success-darkness      /* status, very dark variant */
--aspid-icons-status-error                  /* status icon resource */
--aspid-fasttools-metrics-label_size        /* compound role */
--aspid-fasttools-prop-status               /* inline component param */
```

All palette variables in `Aspid-FastTools-Default-Dark.uss` already follow this grammar; new variables in any other stylesheet must follow it from the start.

**README files:** 4 files to keep in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Documentation/EN/README.md`/`RU/README.md`. Image paths differ between them — root files use `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Documentation/Images/...`, the inner READMEs use `../Images/...`. Per-feature references (`SerializedPropertyExtensions.md`, `VisualElementExtensions.md`) live alongside each README inside `EN/`/`RU/`.

### Local Claude Code automation

- **PostToolUse hook** (`.claude/hooks/rebuild-generators-on-change.sh`): on every `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, runs `dotnet build -c Release` for the generator project (which redeploys the DLL into the Unity package). Unity-side edits, tests, and the Sample project are explicitly skipped — keep that scope when changing the hook.
- **Project skills** (`.claude/skills/`): `build-generator` (manual generator build + DLL deploy), `sync-readmes` (verify README EN/RU + root/Documentation copies against the codebase), `open-pr` (project conventions for opening pull requests — see *Pull request conventions* below).
- **Project subagents** (`.claude/agents/`): `code-reviewer` (Unity/Editor boundary + generator + package convention review), `uss-bem-checker` (validates USS class names + `--aspid-*` variables against the BEM/positional grammars above).

### Submodule

`Aspid.Internal.Unity` is a git submodule providing internal Unity helpers. It is referenced but not part of the main package distribution.

## Pull request conventions

When opening a PR — manually or via `@claude` — invoke the `open-pr` skill (`.claude/skills/open-pr/SKILL.md`) for the full procedure. Quick reference:

- **Title** — short imperative sentence under 70 characters; no auto-generated branch-name strings.
- **Body** — fill out `.github/PULL_REQUEST_TEMPLATE.md` (Summary / Notes for review / Linked issues). Mega-PRs like the `Develop` → `main` release cut are exempt and use feature-scoped `###` subsections instead.
- **Labels** — exactly one `type: *`, plus matching `area: *` labels.
- **Commit messages** — short imperative sentences, **no** `Co-Authored-By: Claude …` trailer.
- **Scope** — one logical change per PR; flag any unrelated noise in *Notes for review*.
