## Repository Overview

**Aspid.FastTools** is a Unity package (`tech.aspid.fasttools`) that minimizes routine boilerplate code. It consists of three components:

1. **`Aspid.FastTools/`** — The Unity project containing the package source (Runtime + Editor code)
2. **`Aspid.FastTools.Generators/`** — A standalone .NET solution containing Roslyn source generators
3. **`Aspid.FastTools.Analyzers/`** — A git submodule (repo `VPDPersonal/Aspid.FastTools.Analyzers`) with standalone Roslyn analyzers that validate package-attribute usage (`AFT*` diagnostic IDs)

Repo-internal working documents (roadmap, release checklist, the bilingual QA checklist `QA-CHECKLIST.md`/`QA-CHECKLIST_RU.md`, the `DESIGN.md` design-system spec) live in `docs/` — distinct from the package's user-facing `Documentation/`. The QA checklist is the standing pre-release verification protocol: a new feature must add its checklist item in **both** languages before its branch merges.

### Unity Package
Compilation is handled automatically by Unity's build system when the project is open. There are no CLI build scripts.

### Building & Deploying Generators

On build, `ILRepack.targets` merges the `Aspid.Generators.Helper*` dependencies into a single-file DLL and `Directory.Build.targets` auto-copies it into the Unity package (never reference `SourceGenerator.Foundations` — its injected `Console` logging deadlocks Unity's compiler server; see `Aspid.FastTools.Generators/CLAUDE.md`):

```bash
# From Aspid.FastTools.Generators/
dotnet build -c Release
# Outputs DLL to: Aspid.FastTools/Packages/tech.aspid.fasttools/Aspid.FastTools.Generators.dll
```

Run tests with:
```bash
# From Aspid.FastTools.Generators/
dotnet test
```

### Building & Deploying the Analyzer

The analyzer lives in the `Aspid.FastTools.Analyzers/` git submodule (run `git submodule update --init` after cloning). It ships into the package as a prebuilt Roslyn DLL. Unlike the generator there is **no** auto-copy `Directory.Build.targets` — keeping it out keeps the submodule independent of this repo's layout — so the DLL is rebuilt and copied manually:

```bash
# From the repo root, after editing the submodule:
dotnet build Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.sln -c Release
dotnet test  Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.sln -c Release
cp Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/bin/Release/netstandard2.0/Aspid.FastTools.Analyzers.dll \
   Aspid.FastTools/Packages/tech.aspid.fasttools/
```

The committed `Aspid.FastTools.Analyzers.dll.meta` carries the `RoslynAnalyzer` label with every platform excluded (mirrors `Aspid.FastTools.Generators.dll.meta`). Diagnostic IDs use the `AFT*` prefix; the generator's `IdStructGenerator` uses `AFID*`.

## Architecture

### Two-Project Separation

**Generators project** (`Aspid.FastTools.Generators/Aspid.FastTools.Generators/`):
- Roslyn `IIncrementalGenerator` implementations — pipeline patterns, data-struct rules, and per-generator details live in `Aspid.FastTools.Generators/CLAUDE.md`
- `Aspid.FastTools.Generators.Tests/` — unit tests; `Aspid.FastTools.Generators.Sample/` — manual smoke-test project

**Unity package** (`Aspid.FastTools/Packages/tech.aspid.fasttools/`):
- `Unity/Runtime/` — shipped with player builds
- `Unity/Editor/Scripts/` — editor-only code, excluded from builds
- `Unity/Editor/Resources/UI/` — editor USS stylesheets, organized by domain subfolders (`UI/Components/`, `UI/Ids/`, …); shared base palette at `UI/Aspid-FastTools-Default-Dark.uss`. Files follow `Aspid-FastTools-{Feature}.uss`.
- `Unity/Editor/Resources/Icons/` — editor icon assets referenced by USS via `--aspid-icons-*` variables.
- `Source/` — pure C# extensions with no Unity dependency
- `Tests/Editor/` — Unity-side editor test assemblies (`Aspid.FastTools.Unity.Editor.Tests`, `Aspid.FastTools.Unity.Editor.SerializeReferences.Tests`), run via Unity Test Runner
- `Samples~/` — optional samples (UPM convention, tilde hides from Unity importer, imported via Package Manager)

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional Mathematics integration:** Mathematics-dependent extensions live in the satellite `Aspid.FastTools.Unity.VisualElements.Math` assembly, which references `Unity.Mathematics` directly and is gated by a `versionDefines` entry that compiles it only when `com.unity.mathematics` is installed. New Mathematics-dependent code goes there. The same `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` symbol is also declared on the main runtime asmdef for the rare case when a single file in `Aspid.FastTools.Unity` needs to gate Mathematics-aware behavior.

### Assembly Definitions

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, Ids, ProfilerMarkers, VisualElements |
| `Aspid.FastTools.Unity.VisualElements.Math` | `Unity/Runtime/VisualElements/Extensions/INotifyValueChanged/Math/` | Satellite assembly compiled only when `com.unity.mathematics` is installed; hosts `INotifyValueChanged` extensions for `float2/3/4`, `int2/3/4`, etc. |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | All editor-only tooling |
| `Aspid.FastTools.Unity.Editor.SerializeReferences.Yaml` | `Unity/Editor/Scripts/SerializeReferences/Yaml/` | Asset-YAML parsing for the SerializeReference tooling, isolated in its own assembly |

### Key Features and Their Locations

**ProfilerMarkers** (`Unity/Runtime/ProfilerMarkers/`): Extension method `this.Marker()` returns a `ProfilerMarker` unique to the call site. The source generator handles creating unique markers per (class, method, line).

**SerializableType** (`Unity/Runtime/Types/`): Wraps `System.Type` for Unity Inspector serialization using assembly-qualified names with lazy resolution. `SerializableType<T>` adds generic constraint support.

**TypeSelector** (`Unity/Editor/Scripts/Types/`): `EditorWindow`-based hierarchical type picker with search. The `[TypeSelector]` attribute drives two field shapes: a `string` (storing an assembly-qualified type name, also backing `SerializableType`) and a `[SerializeReference]` managed reference (where picking a type instantiates it). On a managed reference the candidate list defaults to the field's declared type; passing base types (`[TypeSelector(typeof(IMelee))]`) narrows it. `TypeSelectorPropertyDrawer` dispatches on `SerializedProperty.propertyType`; the managed-reference path lives under `Unity/Editor/Scripts/SerializeReferences/`. Picker behavior (Favorites/Recent capacity, toggles) is configurable via `Types/Selectors/TypeSelectorSettings*`. Usage is validated at compile time by the `AFT*` rules in the analyzer submodule.

**SerializeReference tooling** (`Unity/Editor/Scripts/SerializeReferences/`): beyond the managed-reference TypeSelector drawer, hosts `SerializeReferenceWindow` (menu `Tools/Aspid 🐍/FastTools/…`) with four tabs — **Welcome**, **Asset References** (per-asset managed-reference graph via `SerializeReferenceGraphView`), **Project References** (project-wide scan), **Settings**. Key subsystems: `Windows/` (window + views), `Index/` (project-wide reference index), `Diagnostics/` (missing-type / breakage detection), `Yaml/` (asset-YAML parsing, own asmdef). USS domain: `Resources/UI/SerializeReferences/`.

**Settings / Preferences** (`Unity/Editor/Scripts/Settings/`): `AspidFastToolsPreferencesProvider` (per-user Preferences page) + `AspidSettingsUI` (shared settings-UI helpers). Per-feature settings live next to their feature (`Types/Selectors/TypeSelectorSettings*`, `SerializeReferences/Settings/`, `Welcome/WelcomeSettings*`); the `SerializeReferenceWindow` **Settings** tab aggregates them. USS: `Resources/UI/Windows/Aspid-FastTools-Settings.uss`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**Id Registries** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): A single `IdRegistry` (`ScriptableObject`) maps string names to stable integer IDs for a given struct type; full `int ↔ string` mapping at runtime via `TryGetId` / `TryGetName` / `Contains`. Each `IId` struct binds to exactly **one** registry — enforced at lookup time by `IdRegistryResolver`. `IdStructGenerator` emits the struct-side boilerplate; the registry asset is created via `Assets → Create → Aspid → Id Registry`. Editor internals (`RegistryEditorCore` mutation cycle, validation, resolver invariants, `UniqueIdIndex`) are documented in `Unity/Editor/Scripts/Ids/CLAUDE.md`.

**SerializedProperty Extensions** (`Unity/Editor/Scripts/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/Extensions/`): Extensive fluent API for UIToolkit — layout, sizing, style, borders, colors, transitions, callbacks, USS, child management, etc. Organized into subdirectories by element type (`Button/`, `Field/`, `INotifyValueChanged/`, `IStyle/`, …) plus top-level partial files (`VisualElementExtensions.cs`, `.Style.cs`, `.Uss.cs`, …). Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/`.

**Internal Editor VisualElement Components** (`Unity/Editor/Scripts/VisualElements/Internal/`): Custom UIToolkit elements for editor UI. Layout:

- `Components/` — one subfolder per element (`AspidLabels/`, `AspidSwitches/`, `AspidGradientButton/`, …). Every component follows the same shape: the element class, a `{Name}Preset`, fluent extensions, and (when USS-configurable) a `Styles/` folder with `{Name}{Property}Style` structs providing USS-driven bindings (floats, enums, `Texture2D`, …).
- `Styles/` — shared helpers used across components: `AspidStyles` (USS class/property registry), `StatusStyle`, `ThemeStyle`, `InlineStyle<T>` (USS-vs-code precedence helper). The companion `ICustomStyleExtensions` (extension methods on `ICustomStyle`, including `TryGetByEnum<T>`) lives in `Unity/Runtime/VisualElements/Extensions/ICustomStyle/` since it ships with player builds and is consumed by both runtime and editor styles.

All components use `Aspid-FastTools-Default-Dark.uss` as the base stylesheet (loaded via `AspidStyles.DefaultStyleSheet`) and follow the same `.AddClass()` pattern. Theme/status/size/direction enums are exposed as nested `Type` enums on their respective `Style` structs (e.g. `ThemeStyle.Type`, `AspidLabelSizeStyle.Type`).

**IMGUI Scopes** (`Unity/Editor/Scripts/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Scripts/Extensions/`): `GetScriptName()` and `GetScriptNameWithIndex()` on `MonoScript` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

**Welcome View** (`Unity/Editor/Scripts/Welcome/`): `WelcomeView` (a `VisualElement` hosted as the **Welcome** tab of `SerializeReferenceWindow`, menu `Tools/Aspid 🐍/FastTools/Welcome`) + `WelcomeWindowStartup` (auto-show on first import) + `WelcomeSettings`/`WelcomeSettingsUI`. UXML/USS at `Resources/UI/Windows/Welcome/Aspid-FastTools-Welcome.{uxml,uss}`. Lists installable samples by reading `package.json`.

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

**README files:** 4 files to keep in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/README.md`/`RU/README.md`. Image paths differ between them — root files use `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/...`, the inner READMEs use `../Images/...`. Per-feature references (`SerializedPropertyExtensions.md`, `VisualElementExtensions.md`) live alongside each README inside `EN/`/`RU/`.

### Local Claude Code automation

- **PostToolUse hook** (`.claude/hooks/rebuild-generators-on-change.sh`): on every `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, runs `dotnet build -c Release` for the generator project (which redeploys the DLL into the Unity package). Unity-side edits, tests, and the Sample project are explicitly skipped — keep that scope when changing the hook.
- **PostToolUse hook** (`.claude/hooks/rebuild-analyzers-on-change.sh`): same pattern for the analyzer submodule — on edits to `*.cs` under the analyzer project (Tests/Sample skipped), rebuilds it and copies the DLL into the Unity package. Wired in `.claude/settings.json` `hooks.PostToolUse` next to the generator hook.

## Pull request conventions

When opening a PR — manually or via `@claude` — invoke the `open-pr` skill (`.claude/skills/open-pr/SKILL.md`): it holds the full procedure (title/body/label/commit-message/scope rules).
