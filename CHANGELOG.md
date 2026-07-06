# Changelog

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

This release is centred on the **SerializeReference toolchain**: `[TypeSelector]` now drives managed-reference and `SerializableType` fields, a repair/diagnostics workbench finds and fixes missing managed references project-wide, and a build/CI gate keeps them out of builds.

> Unless noted otherwise, every inspector feature below works in **both IMGUI and UIToolkit** inspectors.

### Added

#### `[TypeSelector]` on `SerializableType` fields
- `[TypeSelector]` can now annotate a `SerializableType` / `SerializableType<T>` field (and arrays / `List<T>` of them) to filter its type picker — the same assembly-qualified-name picker a raw `string` field gets. Previously the drawer threw on this field shape.
  - The attribute's base types are intersected with the generic argument `T` (a candidate must be assignable to both); `Allow` controls whether abstract classes / interfaces are offered.
  - `Required = true` is honoured: an unset `SerializableType` shows the inline "required" notice and counts as a build/CI-gate violation — in the inspector, in saved assets and in the pure-YAML scene scan.
  - The analyzer accepts `SerializableType` as a valid third field shape (no `AFT0001`).

#### `[TypeSelector]` on `[SerializeReference]` fields
- `[TypeSelector]` now also drives `[SerializeReference]` managed-reference fields (and arrays / `List<T>` of them) — the Inspector replaces the default managed-reference UI with a hierarchical type-selector dropdown (reusing `TypeSelectorWindow`).
  - Picking a concrete implementation instantiates it; `<None>` clears the reference; the assigned instance's nested properties draw inline under a foldout; hovering the dropdown shows the full `Namespace.Class, Assembly` identity.
  - The candidate set defaults to the field's declared type; base types (e.g. `[TypeSelector(typeof(IMelee))]`) narrow it further.
- **Open generic implementations** (e.g. `Modifier<T>`) are offered too. When the arguments can be inferred from a closed-generic field the closed type is created directly; otherwise a second picker page collects each argument (honouring its constraints) and validates the result before instantiating.
- **Data carry-over on type switch** — fields shared by the old and new implementation (by name and serialized shape) keep their values instead of resetting.
- **Copy/Paste** context menu on the field header: copies the managed-reference value and pastes it as an independent instance into any compatible field (paste is disabled when the clipboard type is not assignable).
- **`Required` flag** on `[TypeSelector]` for both field shapes: an unset required managed reference (null) or type-name field (empty) shows an inline inspector warning and counts as a build/CI-gate violation.
- **Multi-object editing**: a mixed selection shows a mixed-type dropdown state; picking a type (or pasting) applies an independent instance to each selected object in one Undo group. Per-asset notices (missing / shared) are suppressed under a multi-object selection.
- **Duplicate de-aliasing**: duplicating a `[SerializeReference]` list element (Duplicate / Ctrl+D / `+`-append) no longer aliases the reference — the copy silently becomes an independent instance in a single Undo step. Pre-existing aliases are left alone.
- **Shared-reference notices**: fields backed by one instance are flagged with a compact notice whose **Make unique** action (also right-click → **Make Unique Reference**) clones an independent instance.
  - Each shared group gets a deterministic colour stripe, chip and badge (`Shared reference #1`, `#2`, … — a group id, never a member count); the notice's tooltip lists the other members by inspector path (`Sidearms › Element 1`), and clicking it scrolls to the group's next member and briefly pulses every member in the group colour. Always on. (The **Managed References** window colours its per-rid chips independently, so the two surfaces don't necessarily agree on a colour.)
- **Authoring affordances**: drag a MonoScript onto the field to assign its type; durable named templates (`Save as Template…` / `Paste Template ▸`) persisting a configured instance per project; **Link to Existing** to deliberately share one instance across fields of the same object (the inverse of Make unique); a picker-backed list `+` that opens the type picker instead of duplicating the last element; and **Create New Script…** which generates a subclass stub of the field's type and assigns it once it compiles. (UIToolkit gets the list `+` automatically; an IMGUI inspector opts in per list via `SerializeReferenceIMGUIList.Draw`.)

#### Missing-reference detection & repair
- **Inline repair**: a missing managed-reference type shows a compact amber notice whose underlined **Fix** opens the type picker; the chosen type re-points the reference while keeping its stored data. Hovering the notice shows the full missing-type detail.
  - Detection reads the orphaned type straight from the asset YAML — Unity neither exposes a missing type through the serialization API nor keeps it on the live object for prefabs/GameObjects (UUM-129100).
  - Saved assets (ScriptableObjects, prefab assets) are rewritten in their YAML; an object open in Prefab Mode is repaired on the live instance instead (its open stage would discard a file rewrite on save) — recovering the field data Unity still holds and clearing now-orphaned nested references so the missing-types banner clears.
  - Repair resolves the reference at any depth — through nested managed references and plain `[Serializable]` containers (struct/class fields, `List<T>` elements).
  - The broken dropdown caption is tinted the same warning amber, truncates from the **left** so the class name of `<Missing Namespace.Class>` stays visible, and hovering shows the full stored identity, assembly included.
- **Smart Fix**: the notice ranks the most likely replacement and offers it as a second clickable segment (`· → Pistol?`). Ranking: a declared `[MovedFrom]` rename (highest), the same simple name in a different namespace/assembly, a casing-only rename, near-miss names lifted by a serialized-field-shape match. Drawn only from types the picker would offer; never auto-applied.
- **`[MovedFrom]` renames are told apart from real breakages.** A stored type that no longer loads but is claimed by exactly one type's declared `[MovedFrom]` (an ambiguous claim resolves to nothing) is not broken — Unity migrates it in memory at load; only the YAML keeps the old name.
  - Such entries render as calm info-tinted **pending migrations** instead of amber alarms, across every surface: the breakage toast names them auto-migratable, the Project References group card offers an authoritative one-click **Migrate all (N) → NewType** (same confirm + diff preview + Undo flow), and the Asset References graph shows an info pill, a neutral `Fix ▼` band, a one-click **Migrate → Type** row and a separate headline count.
  - The build/CI gate agrees: a pending migration is never a violation. Baking the rename into the files is what lets the `[MovedFrom]` attribute eventually be deleted.
  - A genuinely missing card surfaces the same one-click **Smart Fix** suggestion (`→ Name?`) on every missing-type surface.
- **Proactive breakage detection**: after a script rename/delete or asset reimport, references that *just* became missing raise a single notification (`N managed references became missing — open Repair`) deep-linking into the Repair window with Smart-Fix-ranked suggestions. Strictly observational — pre-existing breakages never re-alarm, nothing is auto-applied; a Project Settings toggle switches it off, and re-enabling silently re-baselines so a pre-existing miss never alarms.
- **Delete guard**: deleting a C# script used as a managed reference anywhere in the project warns first — with the usage count and a sample of affected assets — and lets you cancel. Unity does this for components but never for managed references.
- **Diff preview**: the bulk `Fix all` confirmation shows the exact YAML lines that will change (old → new) before the irreversible rewrite.
- **Closed repair gaps**: missing types on objects in **saved scenes** are detected and repaired in memory (via `GlobalObjectId`); the Managed References window offers **Open Source Prefab** to descend into a nested prefab instance's source where its data lives; orphaned `RefIds` entries (no field points at them) gained a per-entry **Clear** button.

#### SerializeReference workbench windows
- **Repair Missing References** window: scans a selected prefab/ScriptableObject's YAML and lists *every* orphaned managed reference — at any nesting depth and on any child object — each with a **Fix** picker. Reaches references the per-field drawer cannot surface (components on child objects outside Prefab Mode, bulk repair, orphaned entries) and never requires Prefab Mode.
  - **Project-wide mode**: `Scan Project` sweeps every `.prefab` / `.asset` / `.unity` under `Assets/`, groups broken references by stored type, and offers `Fix all (N)` per group — one type pick plus a confirmation re-points every entry across every affected file (direct file edit; a same-session Undo on the summary reverts it) — plus a one-click Smart Fix quick-apply per group. Entries open in a scene or Prefab Mode are skipped during a bulk apply (close and rescan to include them).
- **Managed References** window: maps an asset's whole `[SerializeReference]` graph straight from the YAML — a per-component tree of field-pointer roots, nested children, shared (aliased) references and orphaned payloads, with `MISSING` / `SHARED` badges, an `Orphaned` group, deterministic per-rid colours and a constrained inline **Fix** picker for missing entries. Read-only except for that Fix; it surfaces references at any depth and the orphans the Inspector cannot navigate to.
  - Nested references are labelled with their full field path (`_primaryWeapon._chargeEffect`); unassigned `[SerializeReference]` fields surface as dim `<None>` slots so a cleared or never-set reference stays visible.
  - The active tab and the inspected asset persist across tab switches and domain reloads.
- **Usage index & Find Usages**: a project-wide managed-reference usage index (built incrementally on import, modeled on the Id system's index) maps every stored `[SerializeReference]` type to the assets, documents and rids using it. Powers `Find Usages of <Type>` on the field context menu and an `sr:` Quick Search provider (`sr:IWeapon`) that lists every use site and pings its asset — and makes repeat `Scan Project` runs near-instant.
- **Build / CI gate**: an `IPreprocessBuildWithReport` warns or fails a player build on missing managed references, and a headless `-executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck` entry point scans the project, writes a report, and exits non-zero for CI.
  - Severity (`Off` / `Warn` / `Fail`, Warn by default) is stored in the committed `ProjectSettings/SerializeReferenceSharedSettings.asset` — not per-machine `EditorPrefs` — so it travels to a clean CI runner: `Off` skips, `Warn` logs but exits 0, `Fail` exits 1 on violations.
  - The `-srGateRequired` check covers prefabs, ScriptableObjects **and scenes** — a `.unity` is read through a pure-YAML pass (resolving each MonoBehaviour's required fields by its `m_Script` guid), since scene objects cannot be loaded for inspection.
  - Flags `-srGateReport <path>`, `-srGateWarnOnly` (force exit 0) and `-srGateFail` (force exit 1 on violations) override the committed severity per run.

#### Type picker UX
- **`[TypeSelectorDisplay]` attribute** (`Aspid.FastTools.Types`, editor-only `[Conditional("UNITY_EDITOR")]`) for tuning how a type appears in the picker: a `Name` display override for rows and the closed dropdown's caption (search still matches the real type name; the hover tooltip keeps the real `Namespace.Class, Assembly` identity), a `Group` path (e.g. `"Combat/Melee"`) placing the type under an explicit hierarchy **instead of** its namespace (path segments are shared between types, so related types meet under one node), a `Tooltip` on the type's row and an `Icon` (an `EditorGUIUtility.IconContent` name, a project-relative asset path with extension, or a `Resources` texture path without one).
- **Favorites & Recent** sections on the picker's root page: a hover-revealed ★ toggle pins a type to Favorites; the last picked types (5 by default, configurable) keep MRU order under Recent. Both persist per project in `EditorPrefs`, are pruned of types that no longer resolve, surface only types in the current candidate set, and hide while searching.
- **Richer rows**: Favorites/Recent headers and namespace rows show a dim right-aligned count of the types they hold (recursive for namespaces, visible while collapsed); the field's current value wears a green ✓ and a bold caption (`<None>` gets it when the field is empty) so the stored value reads apart from the keyboard highlight; a divider separates the pinned block (`<None>`, Favorites, Recent) from the namespace hierarchy.
- `TypeSelectorWindow.Show` gained an optional `TypeSelectorFilter` parameter: its `Predicate` further narrows the candidate list after the base-type and `TypeAllow` checks (the SerializeReference drawer uses it to exclude `UnityEngine.Object`, strings and delegates), and its `AdditionalTypes` injects entries the assignability scan cannot match (such as open generic definitions).

#### Settings & Preferences
- **Settings tab — Type Selector section**: a toggle that hides the root page's Favorites section (the stored list survives and comes back), a Recent-items capacity slider (0–20, default 5; 0 doubles as the off switch — hides the section and pauses recording without wiping history) and a Saved-lists maintenance row that clears the stored Favorites / Recent lists behind a count-naming confirmation.
- **Settings tab — Welcome section**: an **Auto-show Welcome** toggle that suppresses the Welcome tab's auto-open (the menu entry keeps working); covered by the per-user reset (auto-show → on).
- **Storage-scope stripes**: since the tab now mixes team-wide and individual settings, every row is marked green (committed `ProjectSettings` asset) or blue (per-user `EditorPrefs`), decoded by a compact legend; a footer pinned under the scroll offers **Reset to defaults** separately per scope (**Shared** / **Per-user**), each behind a confirmation naming the exact defaults it restores. The saved Favorites / Recent lists are data, not settings, and survive a reset.
- **Preferences → Aspid FastTools** (previously theme-only) is now a full mirror of the window's Settings tab — the same References / Type Selector / Welcome sections, scope legend and per-scope reset footer, rendered on the branded surface. Every surface is built from one control definition per area and mirrors the others live; the SerializeReference Project Settings page still exposes the References controls in Unity's native look.
- **Project Settings → Aspid FastTools → SerializeReference** page: auto de-alias, breakage detection, build/CI-gate severity and excluded scan folders. Breakage detection is per-machine `EditorPrefs`; the rest must behave the same for every teammate and CI, so they live in the committed `ProjectSettings/SerializeReferenceSharedSettings.asset`.

#### Analyzer diagnostics
Shipped in the prebuilt `Aspid.FastTools.Analyzers` Roslyn DLL:
- `AFT0004` (error) — `[SerializeReference]` + `[TypeSelector]` on a type deriving from `UnityEngine.Object`, which Unity does not serialize as a managed reference.
- `AFT0005` (warning) — no visible concrete, Unity-serializable type satisfies both the `typeof(...)` base and the field's element type, so the picker would be empty.

#### Samples
- New installable **SerializeReferences** sample (imported via Package Manager) demonstrating the managed-reference picker across single fields, `List<T>`, abstract bases, narrowing, nested references, generics and `Required`, in both inspectors, with a step-by-step `TUTORIAL` and a guided `TypeSelectorTutorial` scene.
  - Ships pre-broken repair assets covering the recovery paths: `Ghost*` types with no plausible successor exercise the manual **Fix** picker (inline, whole-asset and project-wide sweep); `MovedWeaponPreset.asset` — storing `Pistol` under an old namespace, as after a move without `[MovedFrom]` — surfaces the one-click **Smart Fix** (`→ Pistol?`); `RenamedWeaponPreset.asset` — storing the old class name of the `[MovedFrom]`-renamed `Crossbow` — surfaces the **Migrate all** migration flow.

### Changed
- The Welcome tab auto-opens once per package **version** instead of once per project — after installing *or updating* the package, the next editor launch shows it again. The **Auto-show Welcome** toggle still suppresses every auto-open; the menu entry is unaffected.
- The **Repair Missing References** and **Managed References** windows are merged into a single workbench whose tabs are individual menu entries under `Tools → Aspid 🐍 → FastTools`: **Welcome**, **Asset References** (the reference graph with inline Fix / Clear / Open Source Prefab — subsumes the old per-asset repair list), **Project References** (the project-wide grouped bulk fix) and **Settings**. A Project References result links straight to that asset's Asset References graph.
- The per-property missing-type probe now caches the asset YAML by path + write-time, eliminating a `File.ReadAllLines` on every IMGUI repaint.
- The confirm dialog for clearing a missing managed reference now names how many fields it will null, so an aliased reference shared across several slots makes its all-pointer clear explicit before the irreversible YAML edit (the clear still nulls every aliased field — only the wording changed).
- `[TypeSelector].Allow` now defaults to `TypeAllow.All` instead of `TypeAllow.None`: a type-name field (`string` / `string[]` / `SerializableType`) now lists abstract classes and interfaces in the picker by default — pass `Allow = TypeAllow.None` to restrict it to concrete types. This aligns the raw-`string` picker with `SerializableType`, which already offered them. A `[SerializeReference]` managed reference is unaffected: that path never offered abstract/interface types and ignores `Allow` entirely.

### Fixed
- A `null` element in a `Type[]` member referenced by a `string` / `SerializableType` `[TypeSelector]` field no longer throws `NullReferenceException` and aborts the type picker — null entries are filtered out before building the candidate list. ([#51])
- Hardened the asset-YAML managed-reference editor (`SerializeReferenceYamlEditor`) against non-Unity / oddly-indented files: the indent measure now counts tabs so it stays aligned with the `- rid:` entry-bounding regexes, and the destructive writes (type rewrite, entry removal, reference null-out) refuse a file lacking the `%TAG !u!` directive and bail on unexpected tab / mixed indentation rather than risk a mis-bounded, non-undoable edit. Unity always writes space-indented YAML with the directive preamble, so well-formed assets are unaffected.
- The UIToolkit and IMGUI managed-reference drawers no longer disagree on notice placement: missing / required / mixed-values notices render only when the field holds no value (so their position reads the same in both renderers), and the shared-reference notice deliberately sits at the very bottom — under the assigned instance's nested properties — in both renderers.
- **Make unique**, the silent list de-alias and a keep-data type switch no longer drop nested `[SerializeReference]` children: a type switch carries the shared children over by reference (the old parent is discarded, so reuse is what preserves them), and the unique-making flows deep-copy the whole reference graph — internal aliasing topology preserved, cyclic graphs safe.
- Cyclic managed-reference graphs no longer hang the editor or a CI run: the per-frame alias walk, the Link-to-Existing candidate scan and the CI gate's required-fields walk all refuse to re-enter an instance already on the walk, and Link to Existing no longer offers an aliased ancestor — the exact pick that could tie a field into a self-cycle.
- The `sr:` Quick Search provider is explicit-only, so typing into a plain Search window no longer warms a cold usage index behind a modal full-project sweep. The index and delete-guard sweeps are pure text scans (they no longer load every asset), a failed warm-up resets the index to cold instead of serving partial answers all session, and deleting a **folder** of referenced scripts now warns the same way a single script does.
- Shared-reference bookkeeping got honest: empty fields no longer form phantom "shared groups" (badge numbering had holes); the IMGUI Make-unique / Smart-Fix paths drop the per-frame alias memo so a same-frame repaint never shows a stale notice; undo/redo invalidates that memo once globally instead of once per live field; the missing-type probe is memoised per repaint (legitimately empty fields paid a full YAML + location resolution per call); clicking a shared notice scrolls the inspector's own scroller (list members were never brought into view); the Smart-Fix ranking cache clears when assets change externally (e.g. a `git checkout`).
- The Asset References graph refuses YAML rewrites while the asset is open in a scene or Prefab Mode — the open copy's next save would silently clobber the fix (the Project view already guarded this). The Project References undo receipt only reverts entries that still hold the exact type it applied (an older receipt can no longer destroy a newer fix) and reports the count it actually touched; the bulk diff preview counts only computable entries.
- Assorted correctness: `SerializableType.Type` no longer throws for a code-constructed instance (null stored name); the required string-field notice no longer captures a disposable `SerializedProperty` in its deferred callbacks; Recent picks record a constructed generic's open definition (the closed form could never surface and silently evicted real entries); Prefab Mode repairs bail when the stage is dirty (the sibling-index replay could land on the wrong asset object); the duplicate guard fires only on exact single-element growth, so a bulk restore (Paste Component Values, Revert) no longer silently de-aliases intentional sharing; settings mirrors survive dock moves and a clicked switch (the focus guard now skips only in-progress text edits); the theme override is stored per project; `AspidSwitch` neutrals flip with the editor skin so the handle stays visible on the light theme; the `<None>` row no longer wears the current-value ✓ in pickers that have no current value at all (a list `+` append, a missing-type Fix, the bulk project picker).

### Added (internal)
- First package test coverage: an `Aspid.FastTools.Unity.Editor.SerializeReferences.Tests` EditMode assembly exercising the `SerializeReferenceYamlEditor` parser (missing-type discovery, propertyPath → rid resolution incl. nested/list, stored-type reading, round-trip rewrite, entry removal, aliased-pointer nulling + the dialog pointer-count helper, diff-preview consistency) plus the YAML probe cache, and the write-path hardening (non-Unity-file refusal, tab-indent bail).
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
