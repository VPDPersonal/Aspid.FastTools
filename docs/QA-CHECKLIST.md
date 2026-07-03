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
- [ ] Candidate narrowing via `[TypeSelector(typeof(Base))]`, multiple base types.
- [ ] `Required` on a string field: an empty value shows an inline warning *(2×UI)* + counts as a gate violation.
- [ ] A `null` element in a referenced `Type[]` member does not crash the picker.

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
- [ ] **Settings**: see section 6.
- [ ] Ctrl+Tab / Ctrl+Shift+Tab switch tabs.

## 6. Settings — three mirrors

> Verify on **each** of the three surfaces: the window's Settings tab, `Preferences → Aspid FastTools`, `Project Settings → Aspid FastTools → SerializeReference` (the last one — References only, native look).

- [ ] The mirrors sync live (a change on one surface shows on the others), survive dock moves and switch clicks.
- [ ] Scope stripes (green = ProjectSettings, blue = EditorPrefs) + legend; per-scope Reset to defaults behind a confirm naming the exact defaults; Favorites/Recent survive a reset.
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
- [ ] `ProfilerMarkersGenerator`: `this.Marker()` is unique per (class, method, line), markers visible in the Profiler.
- [ ] `IdStructGenerator`: boilerplate generation, `AFID001`/`AFID002` diagnostics.
- [ ] The incremental cache survives edits (IncrementalCacheTests green).

## 11. Remaining package features

- [ ] **EnumValues\<TValue\>**: plain and `[Flags]` enums, the key does not reset while editing, adding/removing entries.
- [ ] **Id Registries**: creation via `Assets → Create → Aspid → Id Registry`, `TryGetId`/`TryGetName`/`Contains`, an IId struct binds to exactly one registry (IdRegistryResolver), editor-side validation and mutation.
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
