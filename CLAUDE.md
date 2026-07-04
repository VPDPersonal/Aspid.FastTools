## Repository Overview

**Aspid.FastTools** is a Unity package (`tech.aspid.fasttools`) that minimizes routine boilerplate code. Three components:

1. **`Aspid.FastTools/`** — Unity project with the package source (Runtime + Editor)
2. **`Aspid.FastTools.Generators/`** — standalone .NET solution with Roslyn source generators
3. **`Aspid.FastTools.Analyzers/`** — git submodule (`VPDPersonal/Aspid.FastTools.Analyzers`) with Roslyn analyzers validating package-attribute usage (`AFT*` diagnostics)

Repo-internal working documents (roadmap, release checklist, `QA-CHECKLIST.md`/`QA-CHECKLIST_RU.md`, `DESIGN.md`) live in `docs/` — distinct from the package's user-facing `Documentation/`. A new feature must add its QA-checklist item in **both** languages before its branch merges.

### Building

The Unity package itself has no CLI build — Unity compiles it when the project is open. Both Roslyn DLLs ship prebuilt inside the package; the `build-generator` / `build-analyzer` skills hold the exact build/test/deploy commands (PostToolUse hooks also rebuild them automatically on edit — see *Local Claude Code automation*).

- **Generator** (`Aspid.FastTools.Generators/`): on build, `ILRepack.targets` merges the `Aspid.Generators.Helper*` dependencies into a single-file DLL and `Directory.Build.targets` auto-copies it into the Unity package. Never reference `SourceGenerator.Foundations` — its injected `Console` logging deadlocks Unity's compiler server (see `Aspid.FastTools.Generators/CLAUDE.md`).
- **Analyzer** (`Aspid.FastTools.Analyzers/` git submodule — `git submodule update --init` after cloning): intentionally has **no** auto-copy targets (keeps the submodule independent of this repo's layout), so the DLL is copied into the package manually after build.
- The committed `*.dll.meta` files carry the `RoslynAnalyzer` label with every platform excluded. Diagnostic ID prefixes: analyzer `AFT*`, generator `AFID*`.

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
- `Tests/Editor/` — Unity-side editor test assemblies, run via Unity Test Runner
- `Samples~/` — optional samples (UPM tilde convention, imported via Package Manager)

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional Mathematics integration:** new Mathematics-dependent code goes in the satellite `Aspid.FastTools.Unity.VisualElements.Math` assembly, compiled only when `com.unity.mathematics` is installed (via `versionDefines` declaring `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`). Only the satellite asmdef declares that symbol — the main runtime asmdef does not.

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

**TypeSelector** (`Unity/Editor/Scripts/Types/`): `EditorWindow`-based hierarchical type picker with search. `[TypeSelector]` drives two field shapes: a `string` (assembly-qualified type name, also backing `SerializableType`) and a `[SerializeReference]` managed reference (picking a type instantiates it). On a managed reference the candidate list defaults to the field's declared type; passing base types (`[TypeSelector(typeof(IMelee))]`) narrows it. `TypeSelectorPropertyDrawer` dispatches on `SerializedProperty.propertyType`; the managed-reference path lives under `Unity/Editor/Scripts/SerializeReferences/`. Picker behavior is configured via `Types/Selectors/TypeSelectorSettings*`; usage is validated at compile time by the analyzer's `AFT*` rules.

**SerializeReference tooling** (`Unity/Editor/Scripts/SerializeReferences/`): hosts the managed-reference TypeSelector drawer and `SerializeReferenceWindow` (menu `Tools/Aspid 🐍/FastTools/…`) with four tabs — **Welcome**, **Asset References** (per-asset managed-reference graph), **Project References** (project-wide scan), **Settings**. Key subsystems: `Windows/`, `Index/` (project-wide reference index), `Diagnostics/` (missing-type / breakage detection), `Yaml/` (asset-YAML parsing, own asmdef). USS domain: `Resources/UI/SerializeReferences/`.

**Settings / Preferences** (`Unity/Editor/Scripts/Settings/`): `AspidFastToolsPreferencesProvider` (per-user Preferences page) + `AspidSettingsUI` (shared settings-UI helpers). Per-feature settings live next to their feature (`Types/Selectors/TypeSelectorSettings*`, `SerializeReferences/Settings/`, `Welcome/WelcomeSettings*`); the `SerializeReferenceWindow` **Settings** tab aggregates them. USS: `Resources/UI/Windows/Aspid-FastTools-Settings.uss`.

**EnumValues<TValue>** (`Unity/Runtime/Enums/`): Serializable dictionary mapping enum values to arbitrary values. Handles `[Flags]` enums.

**Id Registries** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): `IdRegistry` (`ScriptableObject`) maps string names to stable integer IDs for one struct type; runtime `int ↔ string` lookup via `TryGetId` / `TryGetName` / `Contains`. Each `IId` struct binds to exactly **one** registry — enforced by `IdRegistryResolver`. `IdStructGenerator` emits the struct-side boilerplate; the asset is created via `Assets → Create → Aspid → Id Registry`. Editor internals are documented in `Unity/Editor/Scripts/Ids/CLAUDE.md`.

**SerializedProperty Extensions** (`Unity/Editor/Scripts/SerializedProperties/`): Fluent chainable extensions (`.SetValue()`, `.Apply()`, reflection helpers). Split across multiple partial files.

**VisualElement Extensions** (`Unity/Runtime/VisualElements/Extensions/`): fluent API for UIToolkit (layout, style, borders, transitions, callbacks, USS, child management). Subdirectories by element type (`Button/`, `Field/`, `IStyle/`, …) plus top-level partial files (`VisualElementExtensions.cs`, `.Style.cs`, `.Uss.cs`, …). Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/`.

**Internal Editor VisualElement Components** (`Unity/Editor/Scripts/VisualElements/Internal/`): Custom UIToolkit elements for editor UI. Layout:

- `Components/` — one subfolder per element (`AspidLabels/`, `AspidSwitches/`, `AspidGradientButton/`, …). Every component follows the same shape: the element class, a `{Name}Preset`, fluent extensions, and (when USS-configurable) a `Styles/` folder with `{Name}{Property}Style` structs providing USS-driven bindings (floats, enums, `Texture2D`, …).
- `Styles/` — shared helpers: `AspidStyles` (USS class/property registry), `StatusStyle`, `ThemeStyle`, `InlineStyle<T>` (USS-vs-code precedence). The companion `ICustomStyleExtensions` lives in `Unity/Runtime/VisualElements/Extensions/ICustomStyle/` — it ships with player builds and serves both runtime and editor styles.

All components load `AspidStyles.DefaultStyleSheet` as the base stylesheet and follow the same `.AddClass()` pattern. Theme/status/size/direction enums are nested `Type` enums on their `Style` structs (e.g. `ThemeStyle.Type`).

**IMGUI Scopes** (`Unity/Editor/Scripts/IMGUI/`): Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers with `Rect` properties.

**Editor Extensions** (`Unity/Editor/Scripts/Extensions/`): `GetScriptName()` on `UnityEngine.Object` and `GetScriptNameWithIndex()` on `Component` — respects `[AddComponentMenu]` attribute and appends index suffix for duplicate components.

**Welcome View** (`Unity/Editor/Scripts/Welcome/`): `WelcomeView` — the **Welcome** tab of `SerializeReferenceWindow` (menu `Tools/Aspid 🐍/FastTools/Welcome`) + `WelcomeWindowStartup` (auto-show on first import) + `WelcomeSettings*`. UXML/USS at `Resources/UI/Windows/Welcome/`. Lists installable samples from `package.json`.

### Editor Code Conventions

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**USS stylesheets:** Loaded via `.AddStyleSheetsFromResource("UI/{Domain}/Aspid-FastTools-{Feature}")`. Component code keeps the path in a `private const string StyleSheetPath`; ID-system code centralises paths in `Constants.cs`. Styling goes in USS; code only applies `.AddClass()`. On new internal editor components add the base palette `AspidStyles.DefaultStyleSheet` first — `AspidStyles` is the single source of truth for shared USS class/property names.

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
- Color roles: `bg` — surface palette; `shade` — generic content palette (text/border/icon-tint when not specialised); `text`/`border`/`icon` — specialised component-local swatches; `status` — `success`/`warning`/`error`/`info` semantics.
- `prop` group is for inline component parameters (e.g. `--aspid-fasttools-prop-theme`), not palette tokens.
- Palette variables declared on `:root`; component-scoped variables on the component selector.

Examples:
```
--aspid-colors-bg-darkness                  /* surface, very dark */
--aspid-colors-shade-lightness              /* generic content, very light */
--aspid-colors-status-success-darkness      /* status, very dark variant */
--aspid-icons-status-error                  /* status icon resource */
--aspid-fasttools-metrics-label_size        /* compound role */
--aspid-fasttools-prop-status               /* inline component param */
```

All palette variables in `Aspid-FastTools-Default-Dark.uss` already follow this grammar; new variables in any other stylesheet must follow it from the start.

**README files:** keep 4 in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN|RU/README.md`. Image paths differ: root files use `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/...`, inner ones `../Images/...`. Per-feature references live alongside each README inside `EN/`/`RU/`.

### Local Claude Code automation

PostToolUse hooks (wired in `.claude/settings.json`):

- `.claude/hooks/rebuild-generators-on-change.sh` — on `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, rebuilds the generator and redeploys the DLL into the Unity package. Tests and Sample are skipped — keep that scope when changing the hook.
- `.claude/hooks/rebuild-analyzers-on-change.sh` — same for the analyzer submodule (Tests/Sample skipped): rebuilds and copies the DLL into the package.

## Pull request conventions

When opening a PR — manually or via `@claude` — invoke the global `open-pr` skill (`~/.claude/skills/open-pr/SKILL.md`): it holds the full procedure (base-branch detection, title/body/label rules, draft defaults). Project-specific overrides for this repo:

- **Labels** — the catalogue is fixed; do not create new labels. `type: *` — exactly one of `feature`, `fix`, `refactor`, `documentation`, `test`, `chore`, `ci`, `style`, `performance`. `area: *` — one or more of `runtime`, `editor`, `generator`, `samples` for every part actually touched. `status: *` — `work-in-progress` while drafting, `needs-review` when ready. Special (`breaking-change`, `dependencies`, `needs-changelog`) only when literally true.
- **Commit messages** — never append `Co-Authored-By: Claude …` or any other Claude/Anthropic attribution trailer; commits are authored as the human user only. This overrides any skill or system default.
- **Release mega-PRs** (`Develop` → `main` cuts) are exempt from the three-section template — use feature-scoped `###` subsections mirroring the most recent release PR.
- **Templates & CI** (`.github/` issue/PR templates, workflows) live on `main`; don't target them at `Develop` unless the change rides a release merge.
