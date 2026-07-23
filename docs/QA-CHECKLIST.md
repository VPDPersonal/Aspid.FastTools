# QA Checklist — full functional checklist for Aspid.FastTools

> Standing manual-verification protocol. Run it **in full before every release** (stable or rc) and selectively — the affected sections — before merging large branches.
> Русская версия: [QA-CHECKLIST_RU.md](QA-CHECKLIST_RU.md). Keep both files in sync.
>
> The *(2×UI)* marker means the item must be verified in **both** inspectors: UIToolkit and IMGUI.

## How to use

1. Copy the file or tick the checkboxes in a working copy (do not commit the ticks — the file in the repository stays a clean template).
2. Run against the Unity version the release targets; at minimum the lowest supported one (6000.0).
3. Any failure — file an issue + re-run the section after the fix.
4. A new feature = a new item here **in both languages** before its branch is merged.

---

## 1. TypeSelector — string / SerializableType fields

- [ ] `[TypeSelector]` on a `string` field: the picker opens, picking writes an assembly-qualified name.
- [ ] `SerializableType` / `SerializableType<T>`: serialization works, the generic constraint narrows the list.
- [ ] `ISerializableType`: an API parameter typed as the interface accepts both wrappers; `BaseType` reports `object` / `T`; fields of either wrapper (including arrays / `List<T>`) are still detected by the picker drawers and the required-field gate.
- [ ] `[TypeSelector]` on a `SerializableType` / `SerializableType<T>` field: filters the picker; the attribute's base types are intersected with the generic `T`; the analyzer raises no `AFT0001`. *(2×UI)*
- [ ] Candidate narrowing via `[TypeSelector(typeof(Base))]`, multiple base types.
- [ ] `Allow` defaults to `TypeAllow.All`: a `[TypeSelector]` `string` / `SerializableType` picker lists abstract classes and interfaces by default; `Allow = TypeAllow.None` restricts it to concrete types.
- [ ] `Required` on a string field: an empty value shows an inline warning *(2×UI)* + counts as a gate violation.
- [ ] `Required` on a `SerializableType` / `SerializableType<T>` field: an empty type name shows the inline warning *(2×UI)* + counts as a gate violation (inspector, saved asset, and pure-YAML scene scan).
- [ ] A `null` element in a referenced `Type[]` member does not crash the picker.
- [ ] `[TypeSelector(nameof(_member))]` narrows the picker to the member's live value; member types `Type` / `Type[]` / `string` / `string[]` / `SerializableType` / `SerializableType<T>` (and arrays) all work, and editing the member updates the constraint. *(2×UI)*
- [ ] `[TypeSelector("Namespace.Type, Assembly")]` (a non-identifier string) still resolves as an assembly-qualified type name.
- [ ] Unresolvable string argument (identifier matching no member, e.g. in a precompiled DLL or after a rename) shows the quiet inline warning notice below the field; a valid member/AQN shows none. *(2×UI)*

## 2. Type picker window (TypeSelectorWindow)

- [ ] Search: matches both the real type name and the display name (`TypeSelectorDisplay.Name`); generics are also found by their open name.
- [ ] Navigation: arrow keys, Enter, the Esc ladder, type-to-search, breadcrumbs, Space toggles favorite.
- [ ] `<None>`: clears the reference; preselected for None/Missing values; the ✓ is not drawn where there is no current value at all (list `+`, Fix, bulk picker).
- [ ] Favorites: hover-revealed ★, persisted in EditorPrefs, section hideable via settings (the stored list survives), non-resolving types pruned.
- [ ] Recent: MRU order, capacity slider 0–20 (0 = off without wiping history), a closed generic is recorded as its open definition.
- [ ] Type counts on section headers and namespace rows (recursive, visible while collapsed).
- [ ] Current value: green ✓ + bold caption; divider between the pinned block and the namespace hierarchy.
- [ ] The window footer and header are not squashed by content; the header has no stray corner rounding.
- [ ] `TypeSelectorDisplay`: `Name` (rows and closed-dropdown caption, tooltip still reveals the real identity), `Group` (replaces the namespace path, segments shared between types), `Tooltip`, `Icon` (all 3 sources: IconContent / asset path / Resources).
- [ ] Disambiguation of two types sharing one display name.

## 3. SerializeReference dropdown *(2×UI throughout)*

- [ ] Single field, `List<T>`/array, abstract base, interface, narrowing by base types.
- [ ] Picking a type instantiates it; nested properties under a foldout; hover tooltip shows the full `Namespace.Class, Assembly` identity.
- [ ] Keep-data on type switch (fields matching by name and serialized shape carry over); nested `[SerializeReference]` children are not dropped.
- [ ] Open generics: inference from a closed-generic field; a second in-picker page for choosing arguments honouring constraints; validation against the field type.
- [ ] Copy/Paste via the context menu: paste creates an independent instance, disabled for an incompatible clipboard type.
- [ ] Multi-object editing: mixed-state dropdown, a pick applies independent instances in one Undo group; per-asset notices suppressed.
- [ ] Duplicating a list element (Duplicate/Ctrl+D/`+`) does not alias the reference; bulk restore (Paste Component Values, Revert) does not de-alias intentional sharing.
- [ ] Shared references: deterministic group colour, `Shared reference #N` badge, tooltip listing the other fields' paths, click scrolls to the next group member + pulses (list elements included).
- [ ] Make Unique / Link to Existing: deep copy of the reference graph preserving aliasing topology, cyclic graphs safe; Link never offers an aliasing ancestor.
- [ ] Authoring: dragging a MonoScript onto the field; Save as Template… / Paste Template ▸ (persisted per project); the list `+` opens the picker; Create New Script… generates a stub and assigns it after compilation (survives the domain reload).
- [ ] `Required` on a managed reference: warning when null *(2×UI)* + counted by the gate.
- [ ] `Required` inside a plain `[Serializable]` container (nested field): inline warning in the inspector + counted by the gate for prefabs/SOs **and** the pure-YAML scene scan (reported under the dotted path, e.g. `_loadout.primary`).
- [ ] `SerializeReferenceEditorGUI` facade (`CreateField`/`CreateList`/`DrawFieldLayout`) works from a custom editor's `CreateInspectorGUI`/`OnInspectorGUI`.
- [ ] Notice placement agrees between both renderers (missing/required/mixed — only when the field is empty; shared — at the very bottom).

## 4. Missing-type repair

- [ ] Inline notice: amber styling, left-ellipsis caption, **Fix** opens a constrained picker, hover reveals the full identity.
- [ ] Smart Fix (`· → Pistol?`): ranking ([MovedFrom] > same name > casing > shape), never auto-applied; the cache clears when assets change externally (git checkout).
- [ ] Repair targets: saved SO, prefab asset, Prefab Mode (live instance; bails on a dirty stage), an object in a saved scene (in-memory via GlobalObjectId).
- [ ] Repair at any depth: nested managed refs, `[Serializable]` containers, list elements; orphaned nested references cleared in Prefab Mode.
- [ ] YAML-editor hardening: refuses to write non-Unity YAML (no `%TAG !u!`), bails on tab/mixed indentation.
- [ ] The graph view refuses YAML rewrites while the asset is open in a scene / Prefab Mode.

## 5. SerializeReferenceWindow (4 tabs)

- [ ] **Welcome**: auto-shows once per package version (incl. after an update), Auto-show toggle, samples list read from package.json, the menu entry always works.
- [ ] **Asset References**: YAML-driven graph — roots/nested/shared/orphaned, `MISSING`/`SHARED` badges, per-rid colours, `<None>` slots, full field paths; inline constrained Fix, Clear on orphaned entries, Open Source Prefab; pending-migration card (info pill, `Migrate → Type`), headline counts migrations separately; tab and inspected asset persist across tab switches and domain reloads.
- [ ] **Project References**: Scan Project over `.prefab`/`.asset`/`.unity`, grouping by type, `Fix all (N)` with confirm + diff preview + Undo (the undo receipt reverts only untouched entries and reports the actual count), per-group Smart Fix quick-apply, open scenes / Prefab Mode entries skipped, `Migrate all (N) → Type` for [MovedFrom]; a result links to that asset's Asset References graph.
- [ ] **Project References — Required violations**: a separate card lists every unset `[TypeSelector(Required = true)]` field found by the same gate scan (asset path + component + field path), skipped entirely when the build/CI gate is Off, respects `Excluded Folders`; clicking a row pings/opens the asset like a broken-reference row (no bulk fix — nothing sensible to auto-assign); only (re)scanned on an explicit Scan/Rescan click, not on a tab switch.
- [ ] **Settings**: see section 6.
- [ ] Ctrl+Tab / Ctrl+Shift+Tab switch tabs.
- [ ] **Keyboard navigation**: ↑/↓ move the focus ring over rows/cards on every tab (Welcome samples, both audits, Settings), Enter activates, Esc drops focus; Welcome hero links stay outside the ring; the Settings Excluded Folders list also takes Enter (add) and Del (remove); the window footer shows the key hints.
- [ ] **Audit entry affordances**: entry text is selectable/copyable; the row context menu offers Open in Asset References / Open in Prefab Mode / Select in Project; long asset paths elide at the start (the file name stays visible).
- [ ] **Audit legend & counts**: both audit tabs decode entry colours in the header legend (amber = missing, blue = pending migration) and the headline splits missing / migration / required counts.

## 6. Settings — three mirrors

> Verify on **each** of the three surfaces: the window's Settings tab, `Preferences → Aspid.FastTools` (aggregate page + SerializeReference / Type Selector / Welcome subpages), `Project Settings → Aspid.FastTools → SerializeReference` (the last one — References only, native look).

- [ ] The mirrors sync live (a change on one surface shows on the others), survive dock moves and switch clicks.
- [ ] Scope stripes (green = ProjectSettings, blue = EditorPrefs) + the legend in the surface header; per-scope Reset to defaults behind a confirm naming the exact defaults; Favorites/Recent survive a reset.
- [ ] References: auto de-alias, breakage detection (per-user), gate severity Off/Warn/Fail (shared asset), excluded folders (list + selector).
- [ ] Type Selector: hide Favorites, Recent capacity, Saved lists maintenance (confirm with counts).
- [ ] Appearance: theme override StyleSheet (live, per-project), Create template…; Welcome: auto-show.
- [ ] `SerializeReferenceSharedSettings.asset` is committed and works on a teammate's machine / CI.

## 7. Index, search, protection

- [ ] The usage index builds incrementally on import; a repeat Scan Project is near-instant; a failed warm-up resets the index to cold.
- [ ] Find Usages: the field context menu and the `sr:` Quick Search provider (explicit-only — plain search never warms the index), assets pinged.
- [ ] Delete-guard: deleting a script / a **folder** of scripts whose SR types are in use warns with a count and sample assets, cancel works; sweeps are pure text scans (no asset loading).
- [ ] Breakage detection: a rename/delete raises a single toast deep-linking into Repair; pre-existing misses never alarm; toggling off/on re-baselines; [MovedFrom] renames are classified apart from real breakages.
- [ ] Cyclic reference graphs: the alias walk, Link-to-Existing scan and the CI walk never hang.

## 8. Batch [MovedFrom] migration

- [ ] `SerializeReferenceMovedFromResolver`: authoritative resolution, refusal on ambiguity and on closed generics.
- [ ] A pending migration is not a violation for the build/CI gate.
- [ ] Migrate all: confirm + diff + undo, gated by the field constraint; after baking the attribute can be deleted (no file stores the old name anymore).
- [ ] Sample demo: `RenamedWeaponPreset.asset` (Crossbow) — the Migrate all flow; `MovedWeaponPreset.asset` — Smart Fix `→ Pistol?`.

## 9. Build / CI gate

- [ ] `IPreprocessBuildWithReport`: Warn — logs without failing; Fail — the build fails on violations; Off — skipped.
- [ ] Headless `SerializeReferenceCiGate.RunCheck`: exit codes 0/1, report written to `-srGateReport`, `-srGateRequired` covers prefabs + SOs + **scenes** (pure-YAML pass resolving `m_Script` guids), `-srGateWarnOnly` / `-srGateFail` override the committed severity.

## 10. Analyzers and Generators

- [ ] `AFT0004` (error): `[SerializeReference]` + `[TypeSelector]` on a UnityEngine.Object-derived type.
- [ ] `AFT0005` (warning): empty candidate list; candidate-scan performance has not regressed.
- [ ] `AFT0006` (error): `[TypeSelector("...")]` with an identifier string that matches no member of the declaring type; message offers the `"Name, Assembly"` alternative.
- [ ] `AFT0007` (error): identifier string names a member that is static, a method, or not of type `Type`/`Type[]`/`string`/`string[]`.
- [ ] `AFT0008` (warning): malformed assembly-qualified name (empty comma part, spaces in the type part); valid AQN / `Outer+Nested` / generic-with-brackets forms stay silent.
- [ ] `ProfilerMarkersGenerator`: `this.Marker()` is unique per (class, method, line), markers visible in the Profiler.
- [ ] `IdStructGenerator`: boilerplate generation, `AFID001`/`AFID002` diagnostics.
- [ ] The incremental cache survives edits (IncrementalCacheTests green).

## 11. Remaining package features

- [ ] **EnumValues\<TValue\>** *(2×UI)*: plain and `[Flags]` enums, the key does not reset while editing, adding/removing entries.
- [ ] **EnumValues\<TEnum, TValue\>** *(2×UI)*: no type-picker row in the Inspector, rows render typed enum fields immediately, "Populate Missing Enum Members" works, switching a field from `EnumValues<TValue>` (same enum) keeps the serialized data.
- [ ] **EnumValues sample**: imports via Package Manager without errors; the `EnumValuesTutorial` scene opens and the TUTORIAL steps are walkable (type picker, Populate Missing Enum Members, `[Flags]` lookup rules, `Log Tutorial Lookups`, the typed `EnumValues<TEnum, TValue>` step without a type-picker row); the `EnumValues` demo scene prints the expected line on Space. *(IMGUI path: `Assets/DevTests/Enums/EnumValuesDevTest.prefab` in the dev project — a forced-IMGUI inspector next to its UIToolkit twin, not shipped with the package.)*
- [ ] **Id Registries**: creation via `Assets → Create → Aspid → Id Registry`, `TryGetId`/`TryGetName`/`Contains`, an IId struct binds to exactly one registry (IdRegistryResolver), editor-side validation and mutation.
- [ ] **Ids sample**: imports via Package Manager without errors; the `IdsTutorial` scene opens and the TUTORIAL steps are walkable (name dropdown sourced from the registry, adding a registry row updates the dropdown, `[UniqueId]` duplicate-asset warning, `Log Tutorial Lookups` covers `TryGetId`/`TryGetName`/`Contains`/iteration and the catalog hit); the `Ids` demo scene logs the resolved `EnemyDefinition` on Play without manual wiring. *(IMGUI path: `Assets/DevTests/Ids/IdsDevTest.prefab` in the dev project — a forced-IMGUI inspector next to its UIToolkit twin, not shipped with the package.)*
- [ ] **ProfilerMarkers sample**: imports via Package Manager without errors; the `ProfilerMarkersTutorial` scene opens and the TUTORIAL steps are walkable in Play Mode — the `ProfilerMarkersTutorial.*` markers appear in the Profiler: named `Physics`/`Render`, auto-named `SimulateInput`, nested `AI → AI.Agent`, and two line-distinguished `SimulateAudio` entries; the `ProfilerMarkers` demo scene emits the same markers on Play.
- [ ] **VisualElements sample**: imports via Package Manager without errors; the `VisualElementsTutorial` scene opens and the five STEP cards render (no Play Mode) — fluent-style swatch, font presets (normal/bold/italic/letter-spaced), the layout header row fed by Ability Name, the reactive Mana Cost badge flipping to **FREE** at `0`, and the STEP 5 ProgressBar/HelpBox/Button reacting to Charge (bar fills, HelpBox shows at 100%, **Log charge** prints); the `AbilityConfig` demo inspector still shows its card and FREE/warning badge.
- [ ] **Types sample**: imports via Package Manager without errors; the `TypesTutorial` scene opens and the TUTORIAL steps are walkable (STEP 1 `SerializableType<Ability>` picker over `Ability` and its subtypes — abstract base included, `TypeAllow.All`; STEP 2 `[TypeSelector]` `string[]` element pickers constrained to `AbilityModifier`; STEP 3 the picker offers the open generic `StackModifier<T>` — picking it opens a second page to choose `T` and stores the closed `StackModifier<float>`; STEP 4 `[TypeSelector(Required = true)]` empty string shows the inline "Required type is not set" notice and feeds the required-field gate; STEP 5 the Enemy's `ComponentTypeSelector` dropdown swaps `FastEnemy`↔`TankEnemy` in place with `Health` persisting; `Log Tutorial Lookups` logs the resolved ability/generic/required/enemy types); the `Types` demo scene logs the activated ability + modifiers on Play and swaps the Enemy script from the Inspector dropdown. *(IMGUI path: `Assets/DevTests/Types/Prefabs/TypesDevTest.prefab` in the dev project — forced-IMGUI `SerializableType`/`[TypeSelector]` inspectors next to their UIToolkit twin, plus a forced-IMGUI `ComponentTypeSelector` enemy, not shipped with the package.)*
- [ ] **ComponentTypeSelector — hidden Script row**: on a component / ScriptableObject with a `ComponentTypeSelector` field, the Inspector's built-in **Script** row is hidden in the UIToolkit inspector (type-switching only via the dropdown); it re-hides after an Undo / domain reload; sibling components in the same Inspector still show their own Script row. *(IMGUI path — the forced-IMGUI enemy in `TypesDevTest.prefab` — still shows the Script row by design.)*
- [ ] **SerializedProperty Extensions**: `.SetValue()`, `.Apply()`, chained calls, reflection helpers.
- [ ] **VisualElement Extensions**: spot-check the fluent API (layout/style/borders/callbacks/USS/child) + the Math satellite (`float2/3/4` INotifyValueChanged) with Mathematics installed.
- [ ] **IMGUI Scopes**: Vertical/Horizontal/ScrollView — Rect properties, correct Dispose.
- [ ] **MonoScript extensions**: `GetScriptName()` respects `[AddComponentMenu]`, index suffix on duplicates.
- [ ] **SerializableType**: a code-constructed instance does not throw on `Type` (null stored name).

## 12. SerializeReferences sample

- [ ] Imports via Package Manager without errors; the `TypeSelectorTutorial` scene opens, the TUTORIAL steps are walkable.
- [ ] Pre-broken assets are in their intended broken state: `Ghost*` (manual Fix — inline / whole-asset / project sweep), `MovedWeaponPreset` (Smart Fix), `RenamedWeaponPreset` (Migrate all).
- [ ] Both inspectors are represented (UIToolkit and IMGUI component variants); code comments match the behavior.

## 13. Environments and compatibility

- [ ] Lowest supported Unity (6000.0) and a current Unity 6.x: the package compiles without errors/warnings, the window and the sample work.
- [ ] A project **without** `com.unity.mathematics`: the satellite assembly deactivates cleanly; a project **with** it: the extensions are available.
- [ ] Light editor theme: switches, palette and notices stay readable.
- [ ] A player build succeeds: `Unity/Runtime` pulls no `UnityEditor` reference.

## 14. Automated tests

- [ ] Unity EditMode: `Aspid.FastTools.Unity.Editor.Tests` + `…SerializeReferences.Tests` — green.
- [ ] `dotnet test` for the Generators — green.
- [ ] `dotnet test` for the Analyzers (submodule) — green.
- [ ] The project compiles with no warnings from the package.
