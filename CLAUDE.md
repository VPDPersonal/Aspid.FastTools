## Repository Overview

**Aspid.FastTools** is a Unity package (`tech.aspid.fasttools`) that minimizes routine boilerplate code. Three components:

1. **`Aspid.FastTools/`** — Unity project with the package source (Runtime + Editor)
2. **`Aspid.FastTools.Generators/`** — standalone .NET solution with Roslyn source generators; pipeline patterns and per-generator details in `Aspid.FastTools.Generators/CLAUDE.md`
3. **`Aspid.FastTools.Analyzers/`** — git submodule (`VPDPersonal/Aspid.FastTools.Analyzers`) with Roslyn analyzers validating package-attribute usage (`AFT*` diagnostics)

Repo-internal working documents (roadmap, release checklist, `QA-CHECKLIST.md`/`QA-CHECKLIST_RU.md`, `DESIGN.md`) live in `docs/` — distinct from the package's user-facing `Documentation/`. A new feature must add its QA-checklist item in **both** languages before its branch merges.

### Building

The Unity package itself has no CLI build — Unity compiles it when the project is open. Both Roslyn DLLs ship prebuilt inside the package; the `build-generator` / `build-analyzer` skills hold the exact build/test/deploy commands (PostToolUse hooks also rebuild them automatically on edit — see *Local Claude Code automation*).

- **Generator** (`Aspid.FastTools.Generators/`): on build, `ILRepack.targets` merges the `Aspid.Generators.Helper*` dependencies into a single-file DLL and `Directory.Build.targets` auto-copies it into the Unity package. Never reference `SourceGenerator.Foundations` — its injected `Console` logging deadlocks Unity's compiler server.
- **Analyzer** (`Aspid.FastTools.Analyzers/` git submodule — `git submodule update --init` after cloning): intentionally has **no** auto-copy targets (keeps the submodule independent of this repo's layout), so the DLL is copied into the package manually after build.
- The committed `*.dll.meta` files carry the `RoslynAnalyzer` label with every platform excluded. Diagnostic ID prefixes: analyzer `AFT*`, generator `AFID*`.

## Architecture

### Assemblies (package root: `Aspid.FastTools/Packages/tech.aspid.fasttools/`)

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C# type extensions, no Unity dependency |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Runtime: Types, Enums, Ids, ProfilerMarkers, VisualElements — ships with player builds |
| `Aspid.FastTools.Unity.VisualElements.Math` | `Unity/Runtime/VisualElements/Extensions/INotifyValueChanged/Math/` | Satellite: `INotifyValueChanged` extensions for `float2/3/4` etc. |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | All editor-only tooling, excluded from builds |
| `Aspid.FastTools.Unity.Editor.SerializeReferences.Yaml` | `Unity/Editor/Scripts/SerializeReferences/Yaml/` | Asset-YAML parsing, isolated in its own assembly |

Plus: `Tests/Editor/` — Unity-side editor tests (Unity Test Runner); `Samples~/` — optional samples (UPM tilde convention, imported via Package Manager); `Unity/Editor/Resources/UI|Icons/` — editor stylesheets and icon assets.

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional Mathematics integration:** new Mathematics-dependent code goes in the satellite `Aspid.FastTools.Unity.VisualElements.Math` assembly, compiled only when `com.unity.mathematics` is installed (via `versionDefines` declaring `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`). Only the satellite asmdef declares that symbol — the main runtime asmdef does not.

### Feature map

| Feature | Location | Non-obvious bits |
|---|---|---|
| ProfilerMarkers | `Unity/Runtime/ProfilerMarkers/` | `this.Marker()` returns a call-site-unique `ProfilerMarker`; the source generator creates one per (class, method, line) |
| SerializableType | `Unity/Runtime/Types/` | Wraps `System.Type` via assembly-qualified name, lazy resolution; `SerializableType<T>` adds generic constraints |
| TypeSelector | `Unity/Editor/Scripts/Types/` | `[TypeSelector]` drives two field shapes: a `string` (AQN, also backing `SerializableType`) and a `[SerializeReference]` managed reference (picking a type instantiates it; candidate list defaults to the field's declared type, base types like `[TypeSelector(typeof(IMelee))]` narrow it). `TypeSelectorPropertyDrawer` dispatches on `SerializedProperty.propertyType`; the managed-reference path lives under `SerializeReferences/`. Settings in `Types/Selectors/TypeSelectorSettings*`; usage validated by analyzer `AFT*` rules |
| SerializeReference tooling | `Unity/Editor/Scripts/SerializeReferences/` | `SerializeReferenceWindow` (menu `Tools/Aspid 🐍/FastTools/…`), tabs: Welcome / Asset References / Project References / Settings. Subsystems: `Windows/`, `Index/`, `Diagnostics/`, `Yaml/` (own asmdef) |
| Settings / Preferences | `Unity/Editor/Scripts/Settings/` | `AspidFastToolsPreferencesProvider` + `AspidSettingsUI`; per-feature settings live next to their feature, the window's **Settings** tab aggregates them |
| EnumValues\<TValue\> | `Unity/Runtime/Enums/` | Serializable enum→value dictionary; handles `[Flags]` |
| Id Registries | `Unity/Runtime/Ids/` + `Unity/Editor/Scripts/Ids/` | `IdRegistry` (ScriptableObject) maps names to stable int IDs; each `IId` struct binds to exactly **one** registry (enforced by `IdRegistryResolver`); `IdStructGenerator` emits struct boilerplate. Editor internals: `Unity/Editor/Scripts/Ids/CLAUDE.md` |
| SerializedProperty extensions | `Unity/Editor/Scripts/SerializedProperties/` | Fluent chainable (`.SetValue()`, `.Apply()`), split across partial files |
| VisualElement extensions | `Unity/Runtime/VisualElements/Extensions/` | Fluent UIToolkit API; subdirectories by element type plus top-level partials. Editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/` |
| Internal editor components | `Unity/Editor/Scripts/VisualElements/Internal/` | One subfolder per component: element class + `{Name}Preset` + fluent extensions + `Styles/` structs for USS bindings. Shared helpers in `Styles/` (`AspidStyles`, `StatusStyle`, `ThemeStyle`, `InlineStyle<T>`); `ICustomStyleExtensions` lives in runtime (`Extensions/ICustomStyle/`). All components load `AspidStyles.DefaultStyleSheet` first; enums are nested `Type` on their `Style` structs |
| IMGUI scopes | `Unity/Editor/Scripts/IMGUI/` | Disposable `VerticalScope`/`HorizontalScope`/`ScrollViewScope` with `Rect` properties |
| Editor extensions | `Unity/Editor/Scripts/Extensions/` | `GetScriptName()` / `GetScriptNameWithIndex()` — respects `[AddComponentMenu]`, index suffix for duplicates |
| Welcome view | `Unity/Editor/Scripts/Welcome/` | **Welcome** tab of `SerializeReferenceWindow` + `WelcomeWindowStartup` (auto-show on first import); lists installable samples from `package.json` |

### Editor Code Conventions

**Member accessibility:** in an `internal` class, members must be declared `internal` (or narrower), never `public` — the member's own modifier should show its real accessibility without checking the containing class.

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**XML doc comments:** `<summary>` — 1–2 sentences, what/why, no implementation details. `<remarks>` — only for non-obvious behavior, invariants, or gotchas; omit if it would just restate the summary or the code. `<example>` — only for non-trivial usage patterns where the shape of usage isn't obvious from the signature. Follow Microsoft's Framework Design Guidelines conventions.

**USS:** styling goes in USS, code only applies `.AddClass()`. Naming (BEM classes + variable grammar) and loading conventions: `Aspid.FastTools/Packages/tech.aspid.fasttools/Unity/Editor/Resources/UI/CLAUDE.md` — read it before touching any `.uss` file or USS class names / `--aspid-*` variables in code.

**README files:** keep 4 in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN|RU/README.md`. Image paths differ: root files use `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/...`, inner ones `../Images/...`. Per-feature references live alongside each README inside `EN/`/`RU/`.

### Local Claude Code automation

PostToolUse hooks (wired in `.claude/settings.json`):

- `.claude/hooks/rebuild-generators-on-change.sh` — on `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, rebuilds the generator and redeploys the DLL into the Unity package. Tests and Sample are skipped — keep that scope when changing the hook.
- `.claude/hooks/rebuild-analyzers-on-change.sh` — same for the analyzer submodule (Tests/Sample skipped): rebuilds and copies the DLL into the package.
