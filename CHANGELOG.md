# Changelog

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### VisualElement fluent extensions
- Per-type `SetLabel` overloads for `BaseField<T>` covering 25 Unity types (`Quaternion`, `AnimationCurve`, `Bounds`, `BoundsInt`, `Color`, `Color32`, `Gradient`, `Hash128`, `Rect`, `Vector2/3/4`, `Vector2Int/3Int`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `short`, `ushort`, `byte`, `sbyte`, `char`, `string`, `Enum`, `Object`).
- Raw-enum overloads for all `StyleEnum<T>` setters on `IStyleExtensions` and `VisualElementExtensions.Style` (e.g. `SetFlexWrap(Wrap)` alongside `SetFlexWrap(StyleEnum<Wrap>)`).
- Conditional `*If` variants (`AddChildIf`, `InsertChildIf`, `AddChildrenIf`, `InsertChildrenIf`) for all child management methods across `VisualElement`, `IEnumerable`, array and `List` sources.
- `BindTo(SerializedObject)` and `UnbindFrom()` editor-side fluent extensions for `VisualElement`.
- `SetLabel` for `PropertyField`, `SetBindingPath` for `IBindable` (editor-side).
- Extended `INotifyValueChanged` `ValueChanged` with per-type overloads for common Unity types.
- Extended `Button` (`SetText`), `Focusable`, `Manipulators` (`RemoveManipulator`), `ProgressBar` (`SetTitle`), `BaseListView`, `MultiColumnListView`, `MultiColumnTreeView` with new fluent methods.
- Math-assembly `SetValue` extensions for `float2/3/4`, `int2/3/4` types.

### Changed
- Reorganized `BaseFieldExtensions` into `BaseFields/` subdirectory.
- Renamed `Unbind<T>` extension to `UnbindFrom<T>` for consistency with `BindTo`.
- Tightened `BindPropertyTo` generic constraint from `where T : IBindable` to `where T : VisualElement, IBindable`.

### Fixed
- `SetMinSize` parameter naming bug (`maxHeight` → `minHeight`).

## [1.0.0-rc.2] — 2026-05-18

Release-workflow validation build. No functional changes versus `1.0.0-rc.1`.

### Changed
- Integration URL in all four READMEs now points at the dedicated `upm` branch / `upm/<version>` tag published by the release workflow (no `?path=` query needed).

## [1.0.0-rc.1] — 2026-05-18

First release candidate for `1.0.0`. Marketed as a preview while the **ID System** is finalised — its public API, generated boilerplate and editor workflow may still change before the final `1.0.0` release.

### Added

#### ProfilerMarkers
- `this.Marker()` extension method that resolves to a `ProfilerMarker` unique to the call-site (enclosing type + method/field/property + line number).
- `ProfilerMarkersGenerator` (Roslyn incremental source generator) that emits one `ProfilerMarker` field per call-site and a per-type dispatcher. Walks through lambdas and local functions; supports `.WithName(literal)` and plain `$"..."` interpolated names; deduplicates fields when several call-sites share a line.
- Semantic gating: only `ProfilerMarkerExtensionsForGenerator.Marker` is rewritten, user-defined `Marker()` extensions are left untouched.
- The generated dispatcher is wrapped in `#if ENABLE_PROFILER` and falls back to `return default;`, so non-development builds carry no per-call cost.

#### Serializable Type System
- `SerializableType` — `[Serializable]` wrapper around `System.Type` that stores the assembly-qualified name and resolves the type lazily on first access; implicit conversion to `Type`.
- `SerializableType<T>` — generic variant with a base-type constraint enforced both at compile time and in the editor picker.
- `TypeSelectorAttribute` — `PropertyAttribute` (editor-only via `[Conditional("UNITY_EDITOR")]`) that drives the type picker on `string` fields and lets you constrain the picker to one or more base types.
- `TypeAllow` — `[Flags]` enum that opts the picker into abstract classes (`Abstract`), interfaces (`Interface`) or both (`All`); defaults to concrete classes only.
- `ComponentTypeSelector` — `[Serializable]` helper that surfaces a `Component`-typed sibling on the same `GameObject` through the inspector.
- `TypeSelectorWindow` — `EditorWindow`-based hierarchical type picker with namespace tree, fuzzy search, keyboard navigation and a public `Show(...)` API for invoking it from custom editors.
- Property drawers for `SerializableType`, `SerializableType<T>`, `ComponentTypeSelector` and `[TypeSelector]` strings (IMGUI + UI Toolkit). UI Toolkit drawer renders a reusable `TypeField` / `InspectorTypeField` element.

#### Enum System
- `EnumValues<TValue>` — `[Serializable]` enum-keyed dictionary that survives Unity serialization and handles `[Flags]` enums.
- `EnumValue<TKey, TValue>` — single-entry building block used by the dictionary and exposed for standalone use.
- Custom property drawers for both types with inline editing in the inspector.

#### ID System (Beta)
- `IId` marker interface and `[UniqueId]` attribute for ID-struct types (one struct ↔ one `IdRegistry`).
- `IdRegistry` (`ScriptableObject`) holding the canonical `int ↔ string` map; runtime lookups via `TryGetId`, `TryGetName`, `Contains(int)`, `Contains(string)`.
- `IdRegistryResolver` — lazily builds a `Type AQN → registry` index on first lookup and keeps it incrementally up to date through an `AssetPostprocessor`; `IdRegistry.OnEnable` marks the cache dirty so re-imports are picked up.
- `UniqueIdIndex` — sibling index used by the editor to detect `[UniqueId]` field-value collisions across registries.
- `IdStructGenerator` (Roslyn incremental source generator) emits the struct-side boilerplate (`_id`, `Id`, `__stringId`, equality, conversions) and supports generic target structs as well as generic containing types.
- Analyzer diagnostics: `AFID001` (the target `IId` struct must be `partial`) and `AFID002` (one of the generated members is already declared by the user).
- Editor UI driven by `RegistryEditorCore`: C#-identifier name validation, full Undo, explicit clean-up flow for invalid/duplicate entries, Sort/Group toolbar, manual next-id entry with backward-step warning, Open-Registry shortcut from the `IdStruct` property drawer.
- `Assets → Create → Aspid/Id Registry/Id Registry` menu entry for creating registry assets.

#### VisualElement fluent extensions
- Extensive UI Toolkit fluent API on `VisualElement` and friends — layout, sizing, style, borders, colors, transitions, callbacks, USS classes/sheets, child management.
- Per-element helper sets: `Button`, `Field`, `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `List`, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`, `CallbackEventHandler`, `ICustomStyle`.
- Style preset helpers via `VisualElementExtensions.Style.Preset.cs` and reusable `ICustomStyle.TryGetByEnum<T>` extension for USS-driven enum bindings.
- Editor-side `VisualElement` command extensions in `Unity.Editor/Scripts/VisualElements/Extensions/`.

#### Optional Mathematics integration
- Satellite assembly `Aspid.FastTools.Unity.VisualElements.Math` adds `INotifyValueChanged` extensions (`SetValue`, `ValueChanged`) for `Unity.Mathematics` types (`float2/3/4`, `int2/3/4`, etc.).
- Compiled only when `com.unity.mathematics` is installed (`versionDefines` gate, define symbol `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`).

#### Internal editor components
Shared UI Toolkit elements used across the package's editor surfaces, all built on the base palette `Aspid-FastTools-Default-Dark.uss`:

- `AspidLabel`, `AspidBox`, `AspidGradientButton`, `AspidHelpBox`, `AspidInspectorHeader`, `AspidDividingLine`, `AspidAnimatedLogo`, `AspidAnimatedTitle`, `AspidAnimatedDotsBackground`, `AspidHoverGradientOverlay`.
- USS-driven style structs (`AspidLabelSizeStyle`, `AspidLabelFontStyle`, `AspidDividingLineSizeStyle`, `AspidDividingLineDirectionStyle`, `AspidAnimatedLogoPulseSpeedStyle`, `AspidAnimatedLogoPulseHoverAmplitudeStyle`, `AspidAnimatedLogoLayerImageStyle`, `StatusStyle`, `ThemeStyle`, …).
- Shared helpers: `AspidStyles` (single source of truth for USS class/property names), `InlineStyle<T>` (USS-vs-code precedence helper), `DoubleClickTracker`.

#### SerializedProperty extensions
- Fluent chainable helpers in `SerializePropertyExtensions` (`SetValue`, `Apply`, `Persistent`) and a `Reflection` partial that exposes the backing field/value behind a `SerializedProperty`.

#### IMGUI scopes
- Disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` wrappers that expose the layout `Rect` for hit-testing.

#### Editor helper extensions
- `MonoScript.GetScriptName()` and `MonoScript.GetScriptNameWithIndex()` — respect `[AddComponentMenu]` and append an index suffix when several copies of the same component live on one `GameObject`.

#### Welcome window
- `WelcomeWindow` editor window (menu `Tools/Aspid FastTools/Welcome`) listing the package's installable samples by parsing `package.json`.
- `WelcomeWindowStartup` shows the window automatically on first import.

#### Samples
Five installable samples shipped under `Samples~/` (UPM convention, imported via Package Manager):

- `Types`, `EnumValues`, `Ids`, `ProfilerMarkers`, `VisualElements`.

#### Documentation
- EN and RU READMEs at the package root and at `Documentation/EN/` and `Documentation/RU/`, mirroring the same content with language-appropriate image paths.
- Per-feature reference documents next to each README: `SerializedPropertyExtensions.md`, `VisualElementExtensions.md`.

[Unreleased]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.2...HEAD
[1.0.0-rc.2]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.1...v1.0.0-rc.2
[1.0.0-rc.1]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.1
