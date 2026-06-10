# Changelog

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `[SerializeReferenceSelector]` attribute and property drawer — a hierarchical type-selector dropdown for `[SerializeReference]` fields (and arrays / `List<T>` of them). Picking a concrete implementation instantiates it, `<None>` clears the reference, the assigned instance's nested properties are drawn inline under a foldout, and a stored type that no longer resolves is surfaced as a missing-type warning. Works in both IMGUI and UIToolkit inspectors and reuses the existing `TypeSelectorWindow`.
- `[SerializeReferenceSelector]` now also offers open generic implementations (e.g. `Modifier<T>`). When the type arguments can be inferred from a closed-generic field (`Modifier<float>`) the closed type is created directly; otherwise a follow-up window lets you pick each argument (honouring the parameter's constraints) and validates the result against the field type before instantiating. Works in both IMGUI and UIToolkit paths.
- `TypeSelectorWindow.Show` gained an optional `filter` predicate that further narrows the candidate list after the base-type and `TypeAllow` checks (used by the SerializeReference drawer to exclude `UnityEngine.Object`, strings and delegates), plus an optional `additionalTypes` parameter for injecting entries the assignability scan cannot match (such as open generic definitions).
- `[SerializeReferenceSelector]` now preserves matching data when switching types — fields shared by the old and new implementation (by name and serialized shape) carry over instead of resetting to defaults.
- `[SerializeReferenceSelector]` header gained a Copy/Paste context menu (right-click) that copies the managed-reference value and pastes it as an independent instance into any compatible field; paste is disabled when the clipboard type is not assignable to the target. Works in both IMGUI and UIToolkit paths.
- `[SerializeReferenceSelector]` can now repair a missing managed-reference type. The warning is a compact yellow notice whose underlined **Fix** word opens the hierarchical type picker; the chosen type re-points the reference while keeping its stored data, and hovering the notice shows the full missing-type detail. Detection reads the orphaned type straight from the asset YAML, so it works even though Unity neither exposes a missing type through the serialization API nor keeps it on the live object for prefabs/GameObjects (UUM-129100). Repair applies to saved assets (ScriptableObjects and prefab assets) selected in the Project **and to objects open in Prefab Mode**: saved assets are rewritten in their YAML, while a Prefab Mode object is repaired on the live instance — reassigning the reference and recovering the field data Unity still holds for the missing type — because its open stage would discard a file rewrite on save. The repair resolves the reference at any depth — through nested managed references and through plain `[Serializable]` containers (a struct/class field or a `List<T>` of them), so a missing type buried in a slot or list element is fixed inline too; and when the repaired reference itself carried missing nested references, those now-orphaned entries are cleared as well so the object's missing-types banner clears. Works in both IMGUI and UIToolkit paths.
- `[SerializeReferenceSelector]` now flags an aliased managed reference (two fields backed by the same instance, e.g. after duplicating an array element) with the same compact notice, whose underlined **Make unique** word (also a right-click → **Make Unique Reference** action) clones it into an independent instance. Works in both IMGUI and UIToolkit paths.
- Added a **Repair Missing References** window (`Tools → Aspid 🐍 → Repair Missing References FastTools`) that scans a selected prefab/ScriptableObject's YAML and lists *every* orphaned managed reference — at any nesting depth and on any child object — each with a **Fix** picker. This reaches missing references the per-field drawer cannot surface in the moment — components on child objects when the asset is not open in Prefab Mode, plus bulk repair and orphaned entries no field points at — and never requires Prefab Mode.

## [1.0.0-rc.5] — 2026-06-06

Packaging-only release. No functional or API changes versus `1.0.0-rc.4`.

### Changed
- The package is now developed as an embedded UPM package under `Packages/tech.aspid.fasttools` (previously kept inside the Unity project's `Assets/`), aligning the repository layout with the published `upm` / `upm-preview` subtree. No effect on the published package contents. ([#44])

## [1.0.0-rc.4] — 2026-06-05

### Fixed
- `EnumValues<TValue>` `[Flags]` enum keys no longer reset to `None` while editing an entry or right after adding a new one. The per-element drawer now refreshes the `EnumFlagsField` without notifying (so the programmatic reset no longer fires the value-changed callback that wiped the stored key), and both drawers skip redundant `_key` / `_enumType` writes that re-entered the row drawer mid-edit. ([#43])

### Documentation
- Split the `upm` and `upm-preview` installation URLs into separate code blocks across all four READMEs. ([#38])

## [1.0.0-rc.3] — 2026-05-25

### Added
- Per-type `SetLabel` overloads for `BaseField<T>` covering 29 Unity types (`Quaternion`, `AnimationCurve`, `Bounds`, `BoundsInt`, `Color`, `Color32`, `Gradient`, `Hash128`, `Rect`, `Vector2/3/4`, `Vector2Int/3Int`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `short`, `ushort`, `byte`, `sbyte`, `char`, `string`, `Enum`, `Object`). ([#33])
- Raw-enum overloads for all `StyleEnum<T>` setters on `IStyleExtensions` and `VisualElementExtensions.Style` (e.g. `SetFlexWrap(Wrap)` alongside `SetFlexWrap(StyleEnum<Wrap>)`). ([#33])
- Conditional `*If` variants (`AddChildIf`, `InsertChildIf`, `AddChildrenIf`, `InsertChildrenIf`) for all child management methods across `VisualElement`, `IEnumerable`, array and `List` sources. ([#33])
- `BindTo(SerializedObject)` and `UnbindFrom()` editor-side fluent extensions for `VisualElement`. ([#33])
- `SetLabel` for `PropertyField`, `SetBindingPath` for `IBindable` (editor-side). ([#33])
- Extended `INotifyValueChanged` `ValueChanged` with per-type overloads for common Unity types. ([#33])
- Extended `Button` (`SetText`), `Focusable`, `Manipulators` (`RemoveManipulator`), `ProgressBar` (`SetTitle`), `BaseListView`, `MultiColumnListView`, `MultiColumnTreeView` with new fluent methods. ([#33])
- Math-assembly `SetValue` extensions for `float2/3/4`, `int2/3/4` types. ([#33])
- UPM preview branch: prerelease versions now publish to `upm-preview` / `upm-preview/<version>` tags, keeping `upm` clean for stable releases. ([#31])
- Preview-branch documentation and installation examples added to all four READMEs. ([#32])

### Changed
- Reorganized `BaseFieldExtensions` into `BaseFields/` subdirectory. ([#33])
- Renamed `Unbind<T>` extension to `UnbindFrom<T>` for consistency with `BindTo`. ([#33])
- Tightened `BindPropertyTo` generic constraint from `where T : IBindable` to `where T : VisualElement, IBindable`. ([#33])

### Fixed
- `SetMinSize` parameter naming bug (`maxHeight` → `minHeight`). ([#33])
- Null-safety in `FocusableExtensions.IsFocus` — `focusController` is now null-checked to prevent `NullReferenceException`. ([#33])
- `AddStyleSheetsFromResource` / `RemoveStyleSheetsFromResource` now log a warning and return gracefully instead of throwing when the stylesheet path is not found. ([#33])

## [1.0.0-rc.2] — 2026-05-18

Release-workflow validation build. No functional changes versus `1.0.0-rc.1`.

### Changed
- Integration URL in all four READMEs now points at the dedicated `upm` branch / `upm/<version>` tag published by the release workflow (no `?path=` query needed). ([#30])

## [1.0.0-rc.1] — 2026-05-18

First release candidate for `1.0.0`. Marketed as a preview while the **ID System** is finalised — its public API, generated boilerplate and editor workflow may still change before the final `1.0.0` release.

### Added

#### ProfilerMarkers
- `this.Marker()` extension method that resolves to a `ProfilerMarker` unique to the call-site (enclosing type + method/field/property + line number). ([#8])
- `ProfilerMarkersGenerator` (Roslyn incremental source generator) that emits one `ProfilerMarker` field per call-site and a per-type dispatcher. Walks through lambdas and local functions; supports `.WithName(literal)` and plain `$"..."` interpolated names; deduplicates fields when several call-sites share a line. ([#8])
- Semantic gating: only `ProfilerMarkerExtensionsForGenerator.Marker` is rewritten, user-defined `Marker()` extensions are left untouched. ([#8])
- The generated dispatcher is wrapped in `#if ENABLE_PROFILER` and falls back to `return default;`, so non-development builds carry no per-call cost. ([#8])

#### Serializable Type System
- `SerializableType` — `[Serializable]` wrapper around `System.Type` that stores the assembly-qualified name and resolves the type lazily on first access; implicit conversion to `Type`. ([#8])
- `SerializableType<T>` — generic variant with a base-type constraint enforced both at compile time and in the editor picker. ([#8])
- `TypeSelectorAttribute` — `PropertyAttribute` (editor-only via `[Conditional("UNITY_EDITOR")]`) that drives the type picker on `string` fields and lets you constrain the picker to one or more base types. ([#8])
- `TypeAllow` — `[Flags]` enum that opts the picker into abstract classes (`Abstract`), interfaces (`Interface`) or both (`All`); defaults to concrete classes only. ([#8])
- `ComponentTypeSelector` — `[Serializable]` helper that surfaces a `Component`-typed sibling on the same `GameObject` through the inspector. ([#8])
- `TypeSelectorWindow` — `EditorWindow`-based hierarchical type picker with namespace tree, fuzzy search, keyboard navigation and a public `Show(...)` API for invoking it from custom editors. ([#8])
- Property drawers for `SerializableType`, `SerializableType<T>`, `ComponentTypeSelector` and `[TypeSelector]` strings (IMGUI + UI Toolkit). UI Toolkit drawer renders a reusable `TypeField` / `InspectorTypeField` element. ([#8])

#### Enum System
- `EnumValues<TValue>` — `[Serializable]` enum-keyed dictionary that survives Unity serialization and handles `[Flags]` enums. ([#8])
- `EnumValue<TKey, TValue>` — single-entry building block used by the dictionary and exposed for standalone use. ([#8])
- Custom property drawers for both types with inline editing in the inspector. ([#8])

#### ID System (Beta)
- `IId` marker interface and `[UniqueId]` attribute for ID-struct types (one struct ↔ one `IdRegistry`). ([#8])
- `IdRegistry` (`ScriptableObject`) holding the canonical `int ↔ string` map; runtime lookups via `TryGetId`, `TryGetName`, `Contains(int)`, `Contains(string)`. ([#8])
- `IdRegistryResolver` — lazily builds a `Type AQN → registry` index on first lookup and keeps it incrementally up to date through an `AssetPostprocessor`; `IdRegistry.OnEnable` marks the cache dirty so re-imports are picked up. ([#8])
- `UniqueIdIndex` — sibling index used by the editor to detect `[UniqueId]` field-value collisions across registries. ([#8])
- `IdStructGenerator` (Roslyn incremental source generator) emits the struct-side boilerplate (`_id`, `Id`, `__stringId`, equality, conversions) and supports generic target structs as well as generic containing types. ([#8])
- Analyzer diagnostics: `AFID001` (the target `IId` struct must be `partial`) and `AFID002` (one of the generated members is already declared by the user). ([#8])
- Editor UI driven by `RegistryEditorCore`: C#-identifier name validation, full Undo, explicit clean-up flow for invalid/duplicate entries, Sort/Group toolbar, manual next-id entry with backward-step warning, Open-Registry shortcut from the `IdStruct` property drawer. ([#8])
- `Assets → Create → Aspid/Id Registry/Id Registry` menu entry for creating registry assets. ([#8])

#### VisualElement fluent extensions
- Extensive UI Toolkit fluent API on `VisualElement` and friends — layout, sizing, style, borders, colors, transitions, callbacks, USS classes/sheets, child management. ([#8])
- Per-element helper sets: `Button`, `Field`, `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `List`, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`, `CallbackEventHandler`, `ICustomStyle`. ([#8])
- Style preset helpers via `VisualElementExtensions.Style.Preset.cs` and reusable `ICustomStyle.TryGetByEnum<T>` extension for USS-driven enum bindings. ([#8])
- Editor-side `VisualElement` command extensions in `Unity.Editor/Scripts/VisualElements/Extensions/`. ([#8])

#### Optional Mathematics integration
- Satellite assembly `Aspid.FastTools.Unity.VisualElements.Math` adds `INotifyValueChanged` extensions (`SetValue`, `ValueChanged`) for `Unity.Mathematics` types (`float2/3/4`, `int2/3/4`, etc.). ([#8])
- Compiled only when `com.unity.mathematics` is installed (`versionDefines` gate, define symbol `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`). ([#8])

#### Internal editor components
Shared UI Toolkit elements used across the package's editor surfaces, all built on the base palette `Aspid-FastTools-Default-Dark.uss`:

- `AspidLabel`, `AspidBox`, `AspidGradientButton`, `AspidHelpBox`, `AspidInspectorHeader`, `AspidDividingLine`, `AspidAnimatedLogo`, `AspidAnimatedTitle`, `AspidAnimatedDotsBackground`, `AspidHoverGradientOverlay`. ([#8])
- USS-driven style structs (`AspidLabelSizeStyle`, `AspidLabelFontStyle`, `AspidDividingLineSizeStyle`, `AspidDividingLineDirectionStyle`, `AspidAnimatedLogoPulseSpeedStyle`, `AspidAnimatedLogoPulseHoverAmplitudeStyle`, `AspidAnimatedLogoLayerImageStyle`, `StatusStyle`, `ThemeStyle`, …). ([#8])
- Shared helpers: `AspidStyles` (single source of truth for USS class/property names), `InlineStyle<T>` (USS-vs-code precedence helper), `DoubleClickTracker`. ([#8])

#### SerializedProperty extensions
- Fluent chainable helpers in `SerializePropertyExtensions` (`SetValue`, `Apply`, `Persistent`) and a `Reflection` partial that exposes the backing field/value behind a `SerializedProperty`. ([#8])

#### IMGUI scopes
- Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers that expose the layout `Rect` for hit-testing. ([#8])

#### Editor helper extensions
- `MonoScript.GetScriptName()` and `MonoScript.GetScriptNameWithIndex()` — respect `[AddComponentMenu]` and append an index suffix when several copies of the same component live on one `GameObject`. ([#8])

#### Welcome window
- `WelcomeWindow` editor window (menu `Tools/Aspid FastTools/Welcome`) listing the package's installable samples by parsing `package.json`. ([#8])
- `WelcomeWindowStartup` shows the window automatically on first import. ([#8])

#### Samples
Five installable samples shipped under `Samples~/` (UPM convention, imported via Package Manager):

- `Types`, `EnumValues`, `Ids`, `ProfilerMarkers`, `VisualElements`. ([#8])

#### Documentation
- EN and RU READMEs at the package root and at `Documentation/EN/` and `Documentation/RU/`, mirroring the same content with language-appropriate image paths. ([#8])
- Per-feature reference documents next to each README: `SerializedPropertyExtensions.md`, `VisualElementExtensions.md`. ([#8])

[#8]: https://github.com/VPDPersonal/Aspid.FastTools/pull/8
[#30]: https://github.com/VPDPersonal/Aspid.FastTools/pull/30
[#31]: https://github.com/VPDPersonal/Aspid.FastTools/pull/31
[#32]: https://github.com/VPDPersonal/Aspid.FastTools/pull/32
[#33]: https://github.com/VPDPersonal/Aspid.FastTools/pull/33
[#38]: https://github.com/VPDPersonal/Aspid.FastTools/pull/38
[#43]: https://github.com/VPDPersonal/Aspid.FastTools/pull/43
[#44]: https://github.com/VPDPersonal/Aspid.FastTools/pull/44
[Unreleased]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.5...HEAD
[1.0.0-rc.5]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.4...v1.0.0-rc.5
[1.0.0-rc.4]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.3...v1.0.0-rc.4
[1.0.0-rc.3]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.2...v1.0.0-rc.3
[1.0.0-rc.2]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.1...v1.0.0-rc.2
[1.0.0-rc.1]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.1
