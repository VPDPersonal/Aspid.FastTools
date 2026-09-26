# Changelog

> Русская версия: [CHANGELOG.ru.md](CHANGELOG.ru.md).

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `ToggleButtonGroup` gets typed `SetValue`, `AddValueChanged`, `RemoveValueChanged` and `SetLabel` overloads for `ToggleButtonGroupState`, so calls such as `AddValueChanged(evt => …)` need no type arguments.
- Added `AndApplyWithoutUndo` counterparts for every `SerializedProperty` setter with immediate application, including `SetValue` overloads, object references, enums, and array size helpers.
- Analyzer `AFT0009` (warning) — two `[TypeSelector]` base types have no type in common, so the selector is empty.
- Analyzer `AFT0010` (warning) — a `this.Marker()` call opens no profiler marker because the generator cannot support its type: the type is `private` or `protected` (or nested in such a type), or it reuses a type parameter name of a containing type.
- Analyzer `AFT0011` (warning) — the scope of `this.Marker()` is discarded (`this.Marker();` as a statement, `_ = this.Marker();`, a local nothing reads), so the sample it begins never ends.

### Changed

- The Agent Skills for this package moved from the `aspid-fasttools` Claude Code plugin in [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) into this repository (`skills/`). Install them into Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI or another agent with `npx skills add VPDPersonal/Aspid.FastTools`; the plugin is no longer published.
- Renamed `GetScriptName()` to `GetDisplayName()` and `GetScriptNameWithIndex()` to `GetDisplayNameWithIndex()`; update existing calls to the new names. Both methods now return `string.Empty` for null or destroyed objects. Component indexing uses a pooled list instead of temporary arrays and LINQ.
- `[TypeSelector]` on a `[SerializeReference]` field now offers only types assignable to every attribute type — the rule `string` and `SerializableType` fields already follow; it used to offer types matching any one of them. The same applies to `baseTypes` of `SerializeReferenceEditorGUI.CreateField`, `CreateList` and `DrawFieldLayout`. A list of alternatives such as `typeof(Pistol), typeof(Rifle)` now leaves the selector empty and triggers `AFT0009`: give the allowed classes a common interface or base class and pass that instead. `AFT0005` now checks all attribute types together.
- The generated `this.Marker()` code now declares its marker fields only under `ENABLE_PROFILER`, like the `Marker()` body that reads them: builds without the profiler no longer create every marker in a static constructor.
- `ProfilerMarkerExtensionsForGenerator.Marker(this object)` is now `Marker<T>(this T)`: a struct call site the generator cannot mark is no longer boxed, so a Burst job still compiles.
- `AFT0010` now reports every `this.Marker()` call that opens no marker, not only unsupported types: a receiver of another type (`other.Marker()`, calls in static classes), a default interface method, an explicit argument (`this.Marker(5)`) or type argument, `this?.Marker()`, the static call form, a method group and a call inside an expression tree. The generator no longer emits dead markers for such calls.
- A generic class names nested type arguments the way C# writes them (`Foo<List<Int32>>.Run (line)`, `Foo<Outer<Int32>.Inner>.Run (line)` instead of ``Foo<List`1>``, `Foo<Inner>`). A generic struct gets one marker per call site for all its closed types, named `Job<T>.Execute (line)`, because Burst cannot run the per-type label.
- The generated `Marker()` dispatches on the line with a `switch`, so its cost no longer grows with the number of call sites in a type.

### Removed

- Removed `SetExposedReferenceAndApply()` from `SerializedProperty` extensions; call `SetExposedReference()` instead. Without an `IExposedPropertyTable` context, Unity's `exposedReferenceValue` setter already applies the write and records Undo, so the extra apply did nothing, and a variant without Undo cannot be built on top of it.

### Fixed

- `GetDisplayName()` and `GetDisplayNameWithIndex()` no longer append " (Script)" when `[AddComponentMenu]` is inherited from a base class or its path is empty or ends with `/`; such types get the nicified type name. The title now comes from the attribute declared on the type itself, and an `[Obsolete]` type no longer gets " (Deprecated)".
- A `[TypeSelector(nameof(...))]` member reference on a field inside a `[Serializable]` class or a list element now resolves on the instance that declares the field, as analyzers `AFT0006`–`AFT0008` already check it; it used to be looked up on the inspected component or asset and showed a warning. The same applies to the picker of the Asset References window.
- On Unity 6000.0–6000.2 the `[SerializeReference]` field, the picker and the Project References and Asset References windows are styled again: two stylesheets used a colour syntax these versions cannot parse, so each was dropped whole and the import logged an error.
- `this.Marker()` inside a `private` or `protected` nested type no longer breaks compilation with CS0122. The generated overload cannot see such a type, so the generator now skips it: the call compiles, opens no marker, and `AFT0010` reports it — make the type `internal` or `public` to profile it.
- `this.Marker()` in a type nested in a generic type (`Outer<T>.Inner`) now compiles; the generated overload used to miss the outer type parameters (CS0246).
- `this.Marker()` in an indexer accessor or a static constructor now compiles; the generated field names were invalid. The markers are named `Type.Indexer (line)` and `Type.StaticCtor (line)`.
- `this.Marker()` in an event accessor is now named after the event, like a property accessor: `Type.Changed (line)` instead of `Type.add_Changed (line)` / `Type.remove_Changed (line)`, including explicit interface implementations.
- `this.Marker()` no longer breaks compilation in an auto-property initializer, a static class, a default interface method or an expression tree, in a namespace or type parameter named with a keyword (`Game.@event`, `Foo<@event>`), or when type names differ only in case or flatten to the same name (`Foo`/`foo`, `Foo<T>`/`Foo_1`, `Outer.Inner`/`Outer_Inner`); some of these made every marker of the assembly disappear.
- `Foo`, `Foo<T>` and `Foo<T1, T2>` in one namespace no longer share one generated class in which only the first type got markers; the package's own `EnumValues<TEnum, TValue>` markers now open.
- A `this.Marker()` call split over several lines or under a `#line` directive now opens its own marker; the generator used a different line than `[CallerLineNumber]`.
- A generic struct marked `[BurstCompile]` no longer fails the Burst build (BC1025/BC1360).
- A type deriving from a type of another assembly that shares its namespace through `InternalsVisibleTo` now gets markers of its own; its calls used to bind to the base type's overload and open nothing.
- `WithName($"Br{{ace}}")` gives `Br{ace}`, `WithName` text with U+2028, U+2029 or U+0085 compiles, and a user's own `WithName` extension no longer renames the marker.
- `Persistent()` no longer leaks the `SerializedObject` it creates when the property path no longer exists on the targets; it disposes that object before returning `null`.

## [1.0.0-rc.8] — 2026-09-06

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
- Helpers for `Button`, `BaseField<T>` (`SetLabel` for 29 types), `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `ICustomStyle`, list views, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`.
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
- `upm` / `upm/<version>` for stable releases, `upm-preview` for prereleases. Until the first stable release, the `upm` branch still holds the older `com.aspid.fasttools` package (`1.0.0-rc.2`).
- EditMode tests for the YAML editor and the CI-gate scan.

[1.0.0-rc.8]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8
