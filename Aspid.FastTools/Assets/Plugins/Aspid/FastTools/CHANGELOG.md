# Changelog

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0]

Initial public release.

### Added
- **ProfilerMarkers** — `this.Marker()` extension and a Roslyn source generator (`ProfilerMarkersGenerator`) that emits a unique `ProfilerMarker` per call-site (class + method + line). Supports `.WithName(literal)` and interpolated `$"..."` names; the generated dispatcher is gated by `ENABLE_PROFILER` so non-development builds carry no per-call cost.
- **SerializableType / SerializableType\<T\>** — `System.Type` wrappers serialisable in the Unity Inspector with assembly-qualified names and lazy resolution; generic constraint support.
- **TypeSelector** — `EditorWindow`-based hierarchical type picker with search, used as the property drawer for `SerializableType` and as `ComponentTypeSelector`.
- **EnumValues\<TValue\>** — serializable dictionary mapping enum values to arbitrary values, with `[Flags]` support.
- **Id system** — `IId` / `[UniqueId]` structs paired with `IdRegistry` (`ScriptableObject`) for stable `int ↔ string` mappings. `IdStructGenerator` emits the struct boilerplate and reports diagnostics `AFID001` (missing `partial`) and `AFID002` (member already declared). Editor UI features name validation, full Undo, clean-up flow for invalid entries, sort/group toolbar, manual next-id with backward-step warning.
- **VisualElement fluent extensions** — extensive UIToolkit API covering layout, sizing, style, borders, colors, transitions, callbacks, USS, child management, and per-element helpers (`Button`, `Field`, `Foldout`, `HelpBox`, `Image`, `List`, `ProgressBar`, `Slider`, `TextElement`, etc.).
- **Optional Mathematics integration** — satellite assembly `Aspid.FastTools.Unity.VisualElements.Math` adds `INotifyValueChanged` extensions for `float2/3/4`, `int2/3/4`, etc., compiled only when `com.unity.mathematics` is installed.
- **SerializedProperty extensions** — fluent chainable helpers (`SetValue`, `Apply`, reflection helpers, `Persistent` extension).
- **IMGUI scopes** — disposable `VerticalScope`, `HorizontalScope`, `ScrollViewScope` with `Rect` properties.
- **Editor extensions** — `MonoScript.GetScriptName()` / `GetScriptNameWithIndex()` honouring `[AddComponentMenu]` and duplicate-component index suffixes.
- **Welcome window** — `Tools/Aspid FastTools/Welcome` editor window listing installable samples from `package.json`; shown automatically on first import.
- **Internal editor components** — `AspidLabel`, `AspidBox`, `AspidGradientButton`, `AspidHelpBox`, `AspidInspectorHeader`, `AspidDividingLine`, `AspidAnimatedLogo`, `AspidAnimatedTitle`, `AspidAnimatedDotsBackground`, `AspidHoverGradientOverlay` with USS-driven presets and a shared dark palette (`Aspid-FastTools-Default-Dark.uss`).
- **Samples** — `Types`, `EnumValues`, `Ids`, `ProfilerMarkers`, `VisualElements` (installable via Package Manager).
