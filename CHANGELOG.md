# Changelog

> Русская версия: [CHANGELOG.ru.md](CHANGELOG.ru.md).

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

First release. Unity **6000.0**, assemblies `Aspid.FastTools` / `Aspid.FastTools.Editor`, prebuilt Roslyn DLLs `Aspid.FastTools.Generators` / `Aspid.FastTools.Analyzers`. Every inspector feature works in both IMGUI and UI Toolkit.

### Added

#### Serializable Type System

- `SerializableType` / `SerializableType<T>` — `[Serializable]` wrapper over `System.Type`, lazy resolution, implicit conversion to `Type`, `Type`-taking constructor, `AssemblyQualifiedName`.
- `SerializableMonoScript` / `SerializableMonoScript<T>` — the same, referenced through the script asset so renames and moves do not break it; the player serializes the name alone.
- `SerializableTypeBase` and `ISerializableType` for polymorphic access to any wrapper.
- `[TypeSelector]` — a hierarchical type picker on `string`, `SerializableType` / `SerializableMonoScript` and `[SerializeReference]` fields, and on arrays / lists of them. Base-type constraints, `Allow` (`TypeAllow`), `Required`, member references in string arguments (`Type`, `string`, `SerializableType`, arrays of these, resolved live).
- `[TypeSelectorDisplay]` — `Name`, `Group`, `Tooltip`, `Icon`, `Hidden` for a type's row in the picker.
- `ComponentTypeSelector` — swaps a sibling `Component` in place through a type dropdown.
- `TypeSelectorWindow` with a public `Show(...)` API: namespace tree, search, keyboard navigation, Favorites and Recent, `TypeSelectorFilter` (`Predicate`, `AdditionalTypes`, `HideNoneOption`).
- `TypeField` / `InspectorTypeField` UI Toolkit elements behind the drawers.

#### SerializeReference Selector

- Type dropdown on `[SerializeReference]` fields, nested references included (8 levels deep); custom drawers and `[Header]` / `[Space]` / `[Tooltip]` are honored.
- Open generic implementations: arguments inferred from the field's type arguments and interfaces, otherwise collected on a second picker page.
- Data carry-over on type switch, Copy / Paste, multi-object editing, duplicate de-aliasing.
- Shared-reference notices with **Make unique**, color-coded groups and member navigation.
- Drag a `MonoScript` to assign, named templates, **Link to Existing**, picker-backed list `+`, **Create New Script…**, **Find Usages**.

#### Missing-reference repair

- Inline **Fix** on a missing type, keeping the stored data; works for saved assets, Prefab Mode and scene objects.
- **Smart Fix** ranks the likely replacement (`[MovedFrom]`, same name elsewhere, casing, field-shape match).
- `[MovedFrom]` renames are shown as pending migrations with one-click **Migrate all**, never as violations.
- Breakage notification after a script rename / delete or reimport; delete guard for scripts used as managed references; YAML diff preview before every bulk rewrite.

#### Workbench window (`Tools → Aspid 🐍 → FastTools`)

- **Welcome** — samples with install markers; auto-opens once per package version.
- **Asset References** — the asset's whole `[SerializeReference]` graph from YAML with `MISSING` / `SHARED` badges, inline Fix, Clear for orphans, Open Source Prefab.
- **Project References** — `Scan Project` over `Assets/`, **Fix all** per type with Undo, Smart Fix, Migrate all, Required violations.
- **Settings** — all package settings with shared / per-user scope stripes and per-scope reset.
- Keyboard navigation, legends, row context menus on every tab.
- Project-wide usage index, `sr:` Quick Search provider.
- Build / CI gate: `IPreprocessBuildWithReport` plus headless `SerializeReferenceCiGate.RunCheck` with `-srGateReport`, `-srGateRequired`, `-srGateWarnOnly`, `-srGateFail`; severity `Off` / `Warn` / `Fail` and excluded folders in the committed `ProjectSettings/SerializeReferenceSharedSettings.asset`.

#### Settings

- **Project Settings → Aspid.FastTools → SerializeReference** — auto de-alias, breakage detection, gate severity, excluded folders.
- **Preferences → Aspid.FastTools** — mirror of the Settings tab: References, Type Selector (Favorites, Recent capacity), Welcome, theme.

#### Analyzer diagnostics

- `AFT0001` (error) — `[TypeSelector]` on an unsupported field.
- `AFT0002` (warning) — `Allow` on a managed reference is ignored.
- `AFT0003` (warning) — base type shares no concrete type with the field.
- `AFT0004` (error) — `[SerializeReference]` on a `UnityEngine.Object` type.
- `AFT0005` (warning) — no concrete serializable type satisfies the constraints.
- `AFT0006` (error) — string argument is neither a member nor a type name.
- `AFT0007` (error) — referenced member cannot supply base types.
- `AFT0008` (warning) — non-identifier string is not a valid type name.

#### ProfilerMarkers

- `this.Marker()` — a `ProfilerMarker` unique to the call site.
- `ProfilerMarkersGenerator` — emits one marker field per call site; supports lambdas, local functions, `.WithName(...)` and `$"..."` names; compiled out without `ENABLE_PROFILER`.

#### EnumValues

- `EnumValues<TValue>` — serializable enum-keyed dictionary with a default value and `[Flags]` support.
- `EnumValues<TEnum, TValue>` — typed variant, boxing-free lookups, struct enumerator.
- Drawers with inline editing and **Populate Missing Enum Members**.

#### VisualElement fluent extensions

- Fluent API on `VisualElement`: layout, style, borders, colors, transitions, callbacks, USS, child management with `*If` variants, style presets.
- Helpers for `Button`, `BaseField<T>` (`SetLabel` for 29 types), `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `ICustomStyle`, list views, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`, `CallbackEventHandler`.
- Editor: `BindTo` / `UnbindFrom`, `BindPropertyTo`, `SetBindingPath`, `SetLabel` for `PropertyField`, `AddOpenScriptCommand`, `GetOwnerWindow`.
- `Aspid.FastTools.VisualElements.Math` — `SetValue` / `ValueChanged` for `Unity.Mathematics` types, compiled only with `com.unity.mathematics`.

#### SerializedProperty extensions

- Typed `Set*` / `Set*AndApply` setters, `Update`, `ApplyModifiedProperties`, `Persistent`, path helpers, `GetPropertyType` / `GetFieldInfo` / `GetDeclaringInstance`.

#### Editor helpers

- `GetScriptName()` / `GetScriptNameWithIndex()`.
- Open-script command that handles interfaces in differently named files and nested types.
- `InspectorNotice` / `InspectorNoticeGUI` and the branded `Aspid*` UI Toolkit components.

#### Samples

- **Types**, **SerializeReferences**, **EnumValues**, **ProfilerMarkers**, **EditorTools** — one working scene (or window) each with a `README.md`.

#### Documentation and tooling

- English and Russian docs in `Documentation/`, published at https://vpdpersonal.github.io/Aspid.FastTools/.
- `aspid-fasttools` Claude Code plugin in [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins).
- `upm` / `upm/<version>` for stable releases, `upm-preview` for prereleases.
- EditMode tests for the YAML editor and the CI-gate scan.

[Unreleased]: https://github.com/VPDPersonal/Aspid.FastTools/commits/main
