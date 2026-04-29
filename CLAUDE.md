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
  - `ProfilerMarkersGenerator` — generates per-call-site `ProfilerMarker` registrations based on method name and line number
  - `IdStructGenerator` — generates boilerplate for ID struct types
- Dependencies: `Aspid.Generators.Helper`, `Microsoft.CodeAnalysis.CSharp`, `SourceGenerator.Foundations`

**Unity package** (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/`):
- `Unity/Runtime/` — shipped with player builds
- `Unity/Editor/Scripts/` — editor-only code, excluded from builds
- `Unity/Editor/Resources/Styles/` — editor USS stylesheets, named `Aspid-FastTools-{Feature}.uss`
- `Source/` — pure C# extensions with no Unity dependency
- `Samples~/` — optional samples (UPM convention, tilde hides from Unity importer, imported via Package Manager)

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional integration define:** `Aspid.FastTools.Unity.asmdef` declares a `versionDefines` entry that activates `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` when `com.unity.mathematics` is installed. Gate Mathematics-dependent runtime code with this symbol.

### Assembly Definitions

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, Ids, ProfilerMarkers, VisualElements |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | Editor: Enums, Extensions, IMGUI, Ids, SerializedProperties, Types, VisualElements |

### Key Features and Their Locations

**ProfilerMarkers** (`Unity/Runtime/ProfilerMarkers/`): Extension method `this.Marker()` returns a `ProfilerMarker` unique to the call site. The source generator handles creating unique markers per (class, method, line).

**SerializableType** (`Unity/Runtime/Types/`): Wraps `System.Type` for Unity Inspector serialization using assembly-qualified names with lazy resolution. `SerializableType<T>` adds generic constraint support.

**TypeSelector** (`Unity/Editor/Scripts/Types/`): `EditorWindow`-based hierarchical type picker with search, used as a property drawer for `SerializableType`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**Id Registries** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): Two ScriptableObject types with different runtime contracts:

- `StringIdRegistry` — full `int ↔ string` mapping at runtime; `GetId(name)`, `GetNameId(id)`, `Contains(name)`.
- `IdRegistry` — int-only at runtime; names are stored in an editor-only partial and stripped from player builds.

Each struct type decorated with `[UniqueId]` / implementing `IId` should be bound to exactly **one** registry of either kind — uniqueness is enforced at lookup time by `IdRegistryResolver`, which searches both types.

Editor UI is shared through `RegistryEditorCore` + `IRegistryAccessor` (two implementations). Features: C#-identifier name validation, full Undo, explicit Clean-up flow for invalid entries, Sort/Group toolbar, manual Next ID with backward-step warning, Open-Registry shortcut on the `IdStruct` drawer.

The `IdStructGenerator` generates boilerplate for the struct side; the registry picks for that struct are made via `Assets → Create → Aspid/FastTools/Id Registry` (int-only) or `.../String Id Registry`.

**SerializedProperty Extensions** (`Unity/Editor/Scripts/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/Extensions/`): Extensive fluent API for UIToolkit — layout, sizing, style, borders, colors, transitions, callbacks, USS, child management, etc. Organized into subdirectories by element type (`Button/`, `CallbackEventHandler/`, `Field/`, `Focusable/`, `Foldout/`, `HelpBox/`, `IMGUIContainer/`, `IMixedValueSupport/`, `INotifyValueChanged/`, `IStyle/`, `Image/`, `List/`, `ProgressBar/`, `Slider/`, `TextElement/`) plus top-level partial files (`VisualElementExtensions.cs`, `.Style.cs`, `.Style.Preset.cs`, `.Uss.cs`, `.Child.cs`). Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/`.

**Internal Editor VisualElement Components** (`Unity/Editor/Scripts/VisualElements/Internal/`): Custom UIToolkit elements for editor UI. Layout:

- `Components/` — concrete elements, each in its own subfolder:
  - `AspidAnimatedDotsBackground/`, `AspidAnimatedTitle/` — decorative animated elements.
  - `AspidAnimatedLogo/` — `AspidAnimatedLogo`, `AspidAnimatedLogoPreset`, fluent extensions, plus `Styles/` with `AspidAnimatedLogoPulseSpeedStyle`, `AspidAnimatedLogoPulseHoverAmplitudeStyle` and `AspidAnimatedLogoLayerImageStyle` (USS-driven float and Texture2D bindings).
  - `AspidDividingLines/` — `AspidDividingLine`, `AspidDividingLinePreset`, fluent extensions, plus `Styles/` with `AspidDividingLineSizeStyle` and `AspidDividingLineDirectionStyle` (USS-driven enum bindings).
  - `AspidLabels/` — `AspidLabel`, `AspidLabelPreset`, fluent extensions, plus `Styles/` with `AspidLabelSizeStyle` and `AspidLabelFontStyle`.
  - `Containers/` — `AspidBox`.
  - `GradientButton/` — `GradientButton`.
  - `HelpBoxes/` — `AspidHelpBox` and `AspidHelpBoxPreset`.
  - `InspectorHeaders/` — `AspidInspectorHeader` and fluent extensions.
- `Styles/` — shared helpers used across components: `AspidStyles` (USS class/property registry), `StatusStyle`, `ThemeStyle`, `InlineStyle<T>` (USS-vs-code precedence helper). The companion `ICustomStyleExtensions` (extension methods on `ICustomStyle`, including `TryGetByEnum<T>`) lives in `Unity/Runtime/VisualElements/Extensions/ICustomStyle/` since it ships with player builds and is consumed by both runtime and editor styles.

All components use `Aspid-FastTools-Default-Dark.uss` as the base stylesheet (loaded via `AspidStyles.DefaultStyleSheet`) and follow the same `.AddClass()` pattern. Theme/status/size/direction enums are exposed as nested `Type` enums on their respective `Style` structs (e.g. `ThemeStyle.Type`, `AspidLabelSizeStyle.Type`).

**IMGUI Scopes** (`Unity/Editor/Scripts/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Scripts/Extensions/`): `GetScriptName()` and `GetScriptNameWithIndex()` on `MonoScript` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

### Editor Code Conventions

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**USS stylesheets:** Loaded via `.AddStyleSheetsFromResource("Styles/Aspid-FastTools-{Feature}")`. Styling goes in USS; code only applies `.AddClass()`. `Aspid-FastTools-Default-Dark.uss` serves as a shared base stylesheet for internal editor components — add it first when creating new editor components.

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

**README files:** 4 files to keep in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Documentation/README.md`/`README_RU.md`. Image paths differ between them.

### Submodule

`Aspid.Internal.Unity` is a git submodule providing internal Unity helpers. It is referenced but not part of the main package distribution.
