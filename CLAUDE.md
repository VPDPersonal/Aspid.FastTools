## Repository Overview

**Aspid.FastTools** is a Unity package (`tech.aspid.fasttools`) that minimizes routine boilerplate code. Three components:

1. **`Aspid.FastTools/`** — Unity project with the package source (Runtime + Editor)
2. **`Aspid.FastTools.Generators/`** — standalone .NET solution with Roslyn source generators; pipeline patterns and per-generator details in `Aspid.FastTools.Generators/CLAUDE.md`
3. **`Aspid.FastTools.Analyzers/`** — standalone .NET solution with Roslyn analyzers validating package-attribute usage (`AFT*` diagnostics)

Repo-internal working documents (roadmap, release checklist, `QA-CHECKLIST.md`/`QA-CHECKLIST_RU.md`, `DESIGN.md`) live in `docs/` — distinct from the package's user-facing `Documentation/`. A new feature must add its QA-checklist item in **both** languages before its branch merges.

### Building

The Unity package itself has no CLI build — Unity compiles it when the project is open. Both Roslyn DLLs ship prebuilt inside the package; the `build-generator` / `build-analyzer` skills hold the exact build/test/deploy commands (PostToolUse hooks also rebuild them automatically on edit — see *Local Claude Code automation*).

- **Generator** (`Aspid.FastTools.Generators/`): on build, `ILRepack.targets` merges the `Aspid.Generators.Helper*` dependencies into a single-file DLL and `Directory.Build.targets` auto-copies it into the Unity package. Never reference `SourceGenerator.Foundations` — its injected `Console` logging deadlocks Unity's compiler server.
- **Analyzer** (`Aspid.FastTools.Analyzers/`): `Directory.Build.targets` auto-copies the DLL into the Unity package. The copy is **Release-only** on purpose — the Tests and Sample projects reference the analyzer, so a Debug `dotnet test` run would otherwise overwrite the shipped Release DLL.
- The committed `*.dll.meta` files carry the `RoslynAnalyzer` label with every platform excluded. Diagnostic ID prefixes: analyzer `AFT*`, generator `AFID*`.

## Architecture

### Assemblies (package root: `Aspid.FastTools/Packages/tech.aspid.fasttools/`)

| Assembly | Location | Purpose |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Pure C#, no Unity dependency |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Ships with player builds |
| `Aspid.FastTools.Unity.VisualElements.Math` | `Unity/Runtime/VisualElements/Extensions/INotifyValueChanged/Math/` | Satellite: `INotifyValueChanged` for `float2/3/4` etc. |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | Editor-only, excluded from builds |
| `Aspid.FastTools.Unity.Editor.SerializeReferences.Yaml` | `Unity/Editor/Scripts/SerializeReferences/Yaml/` | Asset-YAML parsing, isolated on purpose |

Plus: `Tests/Editor/` (Unity Test Runner), `Samples~/` (UPM tilde convention — imported via Package Manager), `Unity/Editor/Resources/UI|Icons/`.

**Assembly boundary rule:** `Unity/Runtime/` code must NOT reference `UnityEditor` — it ships with player builds.

**Optional Mathematics integration:** new Mathematics-dependent code goes in the satellite `Aspid.FastTools.Unity.VisualElements.Math` assembly, compiled only when `com.unity.mathematics` is installed (via `versionDefines` declaring `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`). Only the satellite asmdef declares that symbol — the main runtime asmdef does not.

### Feature map

Feature folders under `Unity/Runtime/` and `Unity/Editor/Scripts/` are named after the feature (`Enums`, `Ids`, `ProfilerMarkers`, `Types`, `VisualElements`, `IMGUI`, `SerializedProperties`, `Settings`, `Welcome`, `SerializeReferences`, `Extensions`) — `ls` finds a feature faster than this file can list it. Only what the layout does *not* tell you:

| Feature | Non-obvious bits |
|---|---|
| ProfilerMarkers | `this.Marker()` returns a call-site-unique `ProfilerMarker` — the source generator emits one per (class, method, line) |
| TypeSelector | One attribute, two field shapes — a `string` (AQN) and a `[SerializeReference]` managed reference. **The managed-reference path lives under `SerializeReferences/`, not `Types/`.** Details: `Unity/Editor/Scripts/Types/CLAUDE.md` |
| SerializeReference tooling | `SerializeReferenceWindow` (menu `Tools/Aspid 🐍/FastTools/…`), tabs Welcome / Asset References / Project References / Settings; subsystems `Windows/`, `Index/`, `Diagnostics/`, `Yaml/` (own asmdef) |
| Id Registries | Spans `Unity/Runtime/Ids/` + `Unity/Editor/Scripts/Ids/`. `IdRegistry` (ScriptableObject) maps names to stable int IDs; each `IId` struct binds to exactly **one** registry (enforced by `IdRegistryResolver`); `IdStructGenerator` emits the struct boilerplate. Editor internals: `Unity/Editor/Scripts/Ids/CLAUDE.md` |
| Settings / Preferences | Per-feature settings live next to their feature; `AspidFastToolsPreferencesProvider` + `AspidSettingsUI` and the window's **Settings** tab only aggregate them |
| Internal editor components | Strict four-part layout per component (element + `{Name}Preset` + fluent extensions + `Styles/`) — follow it when adding one. Conventions: `Unity/Editor/Scripts/VisualElements/Internal/CLAUDE.md` |
| VisualElement extensions | Runtime fluent API in `Unity/Runtime/VisualElements/Extensions/`; editor-side command extensions in `Unity/Editor/Scripts/VisualElements/Extensions/` |
| Welcome view | Not its own window — a tab of `SerializeReferenceWindow`, plus `WelcomeWindowStartup` (auto-show on first import); lists installable samples from `package.json` |

### Editor Code Conventions

**Member accessibility:** in an `internal` class, members must be declared `internal` (or narrower), never `public` — the member's own modifier should show its real accessibility without checking the containing class.

**PropertyDrawers:** Always `internal sealed class`. Complex drawers split into a static helper `{Feature}Drawer` with `DrawIMGUI()` and `DrawUIToolkit()` methods — see `SerializableTypeDrawer.cs` as reference.

**XML doc comments:** `<summary>` — 1–2 sentences, what/why, no implementation details. `<remarks>` — only for non-obvious behavior, invariants, or gotchas; omit if it would just restate the summary or the code. `<example>` — only for non-trivial usage patterns where the shape of usage isn't obvious from the signature. Follow Microsoft's Framework Design Guidelines conventions.

**USS:** styling goes in USS, code only applies `.AddClass()`. Naming (BEM classes + variable grammar) and loading conventions: `Aspid.FastTools/Packages/tech.aspid.fasttools/Unity/Editor/Resources/UI/CLAUDE.md` — read it before touching any `.uss` file or USS class names / `--aspid-*` variables in code.

**README files:** keep 4 in sync: root `README.md`/`README_RU.md` and `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN|RU/README.md`. Image paths differ: root files use `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/...`, inner ones `../Images/...`. Per-feature references live alongside each README inside `EN/`/`RU/`.

### Local Claude Code automation

PostToolUse hooks (wired in `.claude/settings.json`):

- `.claude/hooks/rebuild-generators-on-change.sh` — on `Edit`/`Write` to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, rebuilds the generator and redeploys the DLL into the Unity package. Tests and Sample are skipped — keep that scope when changing the hook.
- `.claude/hooks/rebuild-analyzers-on-change.sh` — same for the analyzer (Tests/Sample skipped): rebuilds it, and `Directory.Build.targets` deploys the DLL.

Skills in `.claude/skills/`: `build-generator` / `build-analyzer` (build + deploy the Roslyn DLLs), `sync-readmes`, `unity-pipeline` (drive the live Editor via the `unity` CLI + `com.unity.pipeline` — recompile/test loop, `eval`, `sr_gate`, authoring `[CliCommand]`s), `editor-media-capture` (docs screenshots/GIFs of editor windows).

**Driving the Editor:** several Editors run at once (main checkout + `.claude/worktrees/shared-*`), so every `unity command` needs `--project-path`. Use `unity status` for liveness, not `unity pipeline list`. Details and gotchas live in the `unity-pipeline` skill.
