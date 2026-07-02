# Changelog

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `[TypeSelector]` now also drives `[SerializeReference]` managed-reference fields (and arrays / `List<T>` of them) in addition to `string` type-name fields — the Inspector replaces the default managed-reference UI with a hierarchical type-selector dropdown. Picking a concrete implementation instantiates it, `<None>` clears the reference, the assigned instance's nested properties are drawn inline under a foldout, and a stored type that no longer resolves is surfaced as a missing-type warning. The candidate set defaults to the field's declared type; passing base types (e.g. `[TypeSelector(typeof(IMelee))]`) narrows it further. Works in both IMGUI and UIToolkit inspectors and reuses the existing `TypeSelectorWindow`.
- `[TypeSelector]` now also offers open generic implementations (e.g. `Modifier<T>`). When the type arguments can be inferred from a closed-generic field (`Modifier<float>`) the closed type is created directly; otherwise a second page inside the same picker lets you pick each argument (honouring the parameter's constraints) and validates the result against the field type before instantiating. Works in both IMGUI and UIToolkit paths.
- `TypeSelectorWindow.Show` gained an optional `filter` predicate that further narrows the candidate list after the base-type and `TypeAllow` checks (used by the SerializeReference drawer to exclude `UnityEngine.Object`, strings and delegates), plus an optional `additionalTypes` parameter for injecting entries the assignability scan cannot match (such as open generic definitions).
- `[TypeSelector]` now preserves matching data when switching types — fields shared by the old and new implementation (by name and serialized shape) carry over instead of resetting to defaults.
- `[TypeSelector]` header gained a Copy/Paste context menu (right-click) that copies the managed-reference value and pastes it as an independent instance into any compatible field; paste is disabled when the clipboard type is not assignable to the target. Works in both IMGUI and UIToolkit paths.
- `[TypeSelector]` can now repair a missing managed-reference type. The warning is a compact yellow notice whose underlined **Fix** word opens the hierarchical type picker; the chosen type re-points the reference while keeping its stored data, and hovering the notice shows the full missing-type detail. Detection reads the orphaned type straight from the asset YAML, so it works even though Unity neither exposes a missing type through the serialization API nor keeps it on the live object for prefabs/GameObjects (UUM-129100). Repair applies to saved assets (ScriptableObjects and prefab assets) selected in the Project **and to objects open in Prefab Mode**: saved assets are rewritten in their YAML, while a Prefab Mode object is repaired on the live instance — reassigning the reference and recovering the field data Unity still holds for the missing type — because its open stage would discard a file rewrite on save. The repair resolves the reference at any depth — through nested managed references and through plain `[Serializable]` containers (a struct/class field or a `List<T>` of them), so a missing type buried in a slot or list element is fixed inline too; and when the repaired reference itself carried missing nested references, those now-orphaned entries are cleared as well so the object's missing-types banner clears. Works in both IMGUI and UIToolkit paths.
- `[TypeSelector]` now flags an aliased managed reference (two fields backed by the same instance, e.g. after duplicating an array element) with the same compact notice, whose underlined **Make unique** word (also a right-click → **Make Unique Reference** action) clones it into an independent instance. Works in both IMGUI and UIToolkit paths.
- Added a **Repair Missing References** window that scans a selected prefab/ScriptableObject's YAML and lists *every* orphaned managed reference — at any nesting depth and on any child object — each with a **Fix** picker. This reaches missing references the per-field drawer cannot surface in the moment — components on child objects when the asset is not open in Prefab Mode, plus bulk repair and orphaned entries no field points at — and never requires Prefab Mode.
- The missing-type notice now offers a **Smart Fix** suggestion: a second clickable segment (e.g. `· → Pistol?`) next to **Fix** that ranks the most likely replacement and applies it in one click. Candidates are ranked by a declared `[MovedFrom]` rename (highest), the same simple class name in a different namespace/assembly, a casing-only rename, and near-miss names lifted over the threshold by a serialized-field-shape match. The suggestion is drawn only from the types the picker itself would offer, and is never auto-applied — you always click.
- The **Repair Missing References** window gained a project-wide mode: a `Scan Project` button sweeps every `.prefab` / `.asset` / `.unity` file under `Assets/`, groups the broken references by their stored (now unloadable) type, and offers a single `Fix all (N)` per group — one type pick plus a confirmation re-points every entry across every affected file (the rewrite edits the asset files directly and cannot be undone). Each group also surfaces a one-click Smart Fix quick-apply. Entries that live in currently open scenes are skipped during a bulk apply (close the scene and rescan to include them).
- Added a **Managed References** window that maps an asset's whole `[SerializeReference]` graph straight from the YAML: a per-component tree of field-pointer roots, nested children, shared (aliased) references and orphaned payloads, with `MISSING` / `SHARED` badges, an `Orphaned` group, deterministic per-rid colours, and a constrained inline **Fix** picker for missing entries. Each nested reference is labelled with its full field path (e.g. `_primaryWeapon._chargeEffect`), and unassigned `[SerializeReference]` fields are surfaced as dim `<None>` slots so a cleared or never-set reference stays visible rather than dropping out of the graph. Read-only except for that Fix; it surfaces references at any nesting depth and the orphans the Inspector cannot navigate to. The active tab and the inspected asset persist across tab switches and domain reloads, so picking an asset and switching away no longer reopens the tab on the previous asset, and a script recompile no longer snaps the window back to the Asset References tab.
- Added the `[TypeSelectorDisplay]` attribute (`Aspid.FastTools.Types`, editor-only `[Conditional("UNITY_EDITOR")]`) for tuning how a type appears in the picker: a `Tooltip` shown on the type's row and an `Icon` (an `EditorGUIUtility.IconContent` name, a project-relative asset path with extension loaded via `AssetDatabase`, or a `Resources` texture path without extension).
- The type picker's root page now shows **Favorites** and **Recent** sections. A hover-revealed ★ toggle on each row pins a type to Favorites; the last picked types (8 by default, configurable) are kept in MRU order under Recent. Both are persisted per project in `EditorPrefs`, pruned of types that no longer resolve, surface only types in the current candidate set, and are hidden while searching.
- The type picker's rows now carry more context: **Favorites**/**Recent** headers and namespace rows show a dim right-aligned count of the types they hold (recursive for namespaces — visible while a section is collapsed, too), the field's current value is marked with a green ✓ and a bold caption (`<None>` gets it when the field is empty) so the stored value reads apart from the keyboard highlight, and a divider line separates the pinned block (`<None>`, Favorites, Recent) from the namespace hierarchy on the root page.
- The window's **Settings** tab gained a **Type Selector** section with the picker's per-user preferences: a toggle that hides the root page's **Favorites** section (the stored list survives and comes back when re-enabled), a **Recent items** capacity slider (0–20, default 8) whose 0 doubles as the Recent section's off switch — it hides the section and pauses recording without wiping the collected history — and a **Saved lists** maintenance row that clears the stored Favorites / Recent lists behind a count-naming confirmation. And since the tab now mixes team-wide and individual settings, every row is marked with a storage-scope stripe — green for settings in the committed `ProjectSettings` asset, blue for per-user `EditorPrefs` ones — decoded by a compact legend at the top of the tab, and a footer pinned under the scroll offers **Reset to defaults** separately per scope (**Shared** / **Per-user**), each behind a confirmation naming the exact defaults it restores (the saved Favorites / Recent lists are data, not settings, and survive a reset).
- `[TypeSelector]` managed-reference fields now support multi-object editing: a mixed selection shows a mixed-type dropdown state, and picking a type (or pasting) applies an independent instance to each selected object inside one Undo group. Per-asset notices (missing / shared) are suppressed under a multi-object selection.
- Duplicating a `[SerializeReference]` list element (context-menu Duplicate / Ctrl+D, or `+`-appending a copy of the last element) no longer aliases the managed reference: the copy silently becomes an independent instance in a single Undo step, so editing one element no longer edits the other. Pre-existing aliases are left alone (the shared-reference notice still covers intentional cross-field sharing).
- Fields that share one managed reference now get a deterministic per-rid colour stripe, chip and notice text, matching the colours used by the **Managed References** window so the same shared instance reads the same colour across both surfaces. Always on — there is nothing to opt out of, since matching colours is the whole point of spotting a shared instance at a glance.
- Added analyzer diagnostics for `[TypeSelector]` usage (shipped in the prebuilt `Aspid.FastTools.Analyzers` Roslyn DLL): `AFT0004` (error) — `[SerializeReference]` + `[TypeSelector]` on a type deriving from `UnityEngine.Object`, which Unity does not serialize as a managed reference; `AFT0005` (warning) — no visible concrete, Unity-serializable type satisfies both the `typeof(...)` base and the field's element type, so the picker would be empty.
- Added a project-wide **managed-reference usage index** that maps every `[SerializeReference]` stored type to the assets, documents and rids using it (built incrementally on import, modeled on the Id system's index). It powers **Find Usages** — a `Find Usages of <Type>` entry on a field's context menu and an `sr:` Quick Search provider (`sr:IWeapon`) that lists every use site and pings its asset — and makes the **Repair** window's `Scan Project` near-instant on repeat scans.
- Deleting a C# script that is used as a `[SerializeReference]` managed reference anywhere in the project now warns first (with the usage count and a sample of affected assets) and lets you cancel — Unity does this for components but never for managed references.
- **Proactive breakage detection**: after a script rename/delete (or asset reimport), references that *just* became missing raise a single non-intrusive notification (`N managed references became missing — open Repair`) that deep-links into the Repair window with Smart-Fix-ranked suggestions. Strictly observational — pre-existing breakages never re-alarm, and nothing is auto-applied. Can be switched off in Project Settings (a **Breakage detection** toggle) for projects that find the domain-reload / import-time detection intrusive; turning it back on silently re-baselines on the next change so a pre-existing miss never alarms.
- Authoring affordances on `[TypeSelector]` managed-reference fields: **drag a MonoScript** onto the field to assign its type; **durable named templates** (`Save as Template…` / `Paste Template ▸`) that persist a configured instance per project; **Link to Existing** to deliberately share one instance across fields of the same object (the inverse of Make Unique); a **picker-backed list `+`** that opens the type picker instead of duplicating the last element; and **Create New Script…** which generates a subclass stub of the field's type and assigns it once it compiles. Works in both IMGUI and UIToolkit paths — the UIToolkit ListView manages the list `+` automatically, while an IMGUI inspector opts in per list by drawing it with `SerializeReferenceIMGUIList.Draw` (Unity applies the drawer per element, so it cannot reach the list's own `+`).
- Closed repair gaps: missing types on objects in **saved scenes** are now detected and repaired in memory (via `GlobalObjectId`); the **Managed References** window offers **Open Source Prefab** to descend into a nested prefab instance's source where its data lives; and orphaned `RefIds` entries (no field points at them) gained a per-entry **Clear** button.
- Added a `Required` flag on `[TypeSelector]` (`Aspid.FastTools.Types`) for both field shapes: an unset required `[SerializeReference]` managed reference (empty == null) or `string` type field (empty == no type) shows an inline warning in the inspector (IMGUI and UIToolkit) and counts as a violation for the build/CI gate.
- Added a **Project Settings** page (`Project Settings → Aspid FastTools → SerializeReference`) to toggle auto de-alias, breakage detection, the build/CI gate severity, and excluded scan folders. Breakage detection is per-machine `EditorPrefs`; auto de-alias, the build/CI gate severity and excluded scan folders must behave the same for every teammate and for CI, so they are stored in a committed `ProjectSettings/SerializeReferenceSharedSettings.asset` instead.
- Added a **build / CI gate**: an `IPreprocessBuildWithReport` that warns or fails a player build on missing managed references, and a headless `-executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck` entry point that scans the project, writes a report, and exits non-zero for CI. The gate severity (`Off` / `Warn` / `Fail`, Warn by default) is stored in the committed `ProjectSettings/SerializeReferenceSharedSettings.asset` — not per-machine `EditorPrefs` — so the developer's choice travels to a clean CI runner: `Off` skips the check, `Warn` logs but exits 0, `Fail` exits 1 on violations. The `-srGateRequired` check covers prefabs, ScriptableObjects **and scenes** — a `.unity` is read through a pure-YAML pass (resolving each MonoBehaviour's required fields by its `m_Script` guid) for top-level required fields, since scene objects cannot be loaded for inspection. Flags: `-srGateReport <path>`, `-srGateRequired`, `-srGateWarnOnly` (force exit 0) and `-srGateFail` (force exit 1 on violations) override the committed severity per run.
- The bulk `Fix all` confirmation now shows a **diff preview** of the exact YAML lines that will change (old → new) before the irreversible rewrite.
- A new installable **SerializeReferences** sample (imported via Package Manager) demonstrating the `[TypeSelector]` managed-reference picker across single fields, `List<T>`, abstract bases, narrowing, nested references, generics and `Required`, in both UIToolkit and IMGUI inspectors, with a step-by-step `TUTORIAL` and a guided `TypeSelectorTutorial` scene.

### Changed
- The **Repair Missing References** and **Managed References** windows are merged into a single workbench whose tabs are individual menu entries under `Tools → Aspid 🐍 → FastTools` — **Welcome**, **Asset References** (the reference graph with inline Fix / Clear / Open Source Prefab — which subsumes the old per-asset repair list), **Project References** (the project-wide grouped bulk fix), and **Settings**. A result in Project References links straight to that asset's Asset References graph.
- The per-property missing-type probe now caches the asset YAML by path + write-time, eliminating a `File.ReadAllLines` on every IMGUI repaint.
- The confirm dialog for clearing a missing managed reference now names how many fields it will null, so an aliased reference shared across several slots makes its all-pointer clear explicit before the irreversible YAML edit (the clear still nulls every aliased field — only the wording changed).

### Fixed
- A `null` element in a `Type[]` member referenced by a `string` / `SerializableType` `[TypeSelector]` field no longer throws `NullReferenceException` and aborts the type picker — null entries are now filtered out before building the candidate list. ([#51])
- Hardened the asset-YAML managed-reference editor (`SerializeReferenceYamlEditor`) against non-Unity / oddly-indented files. Its indent measure now counts tabs so it stays aligned with the `- rid:` entry-bounding regexes (previously a tab read as indent 0 here but as a real indent in the regex, so an entry block could be mis-bounded), and the destructive writes (type rewrite, entry removal, reference null-out) now (a) refuse to touch a file that is not a Unity-serialized YAML asset — one lacking the `%TAG !u!` directive — and (b) bail before any write when the target `RefIds` entry uses unexpected tab / mixed indentation, rather than risk a mis-bounded, non-undoable edit. Unity always writes space-indented YAML with the directive preamble, so well-formed assets are unaffected.
- The UIToolkit `[TypeSelector]` managed-reference drawer now renders its missing / shared / required notices **above** the assigned instance's child fields when the foldout is expanded, matching the IMGUI drawer (the notices previously appeared *below* the children because they were appended to the field root after the foldout). They are now hosted between the foldout toggle and its content, so collapsed-state behaviour is unchanged.

### Added (internal)
- First package test coverage: an `Aspid.FastTools.Unity.Editor.Tests` EditMode assembly exercising the `SerializeReferenceYamlEditor` parser (missing-type discovery, propertyPath → rid resolution incl. nested/list, stored-type reading, round-trip rewrite, entry removal, aliased-pointer nulling + the dialog pointer-count helper, diff-preview consistency) plus the YAML probe cache, and the write-path hardening (non-Unity-file refusal, tab-indent bail).
- `AspidSwitch` — a branded iOS-style on/off toggle (`BaseField<bool>`) internal editor component, used by the SerializeReference **Settings** tab.

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
[#51]: https://github.com/VPDPersonal/Aspid.FastTools/pull/51
[Unreleased]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.5...HEAD
[1.0.0-rc.5]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.4...v1.0.0-rc.5
[1.0.0-rc.4]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.3...v1.0.0-rc.4
[1.0.0-rc.3]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.2...v1.0.0-rc.3
[1.0.0-rc.2]: https://github.com/VPDPersonal/Aspid.FastTools/compare/v1.0.0-rc.1...v1.0.0-rc.2
[1.0.0-rc.1]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.1
