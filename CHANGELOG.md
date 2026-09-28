# Changelog

> Русская версия: [CHANGELOG.ru.md](CHANGELOG.ru.md).

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `RemoveChildren` and `RemoveChildrenIf` remove several children in one call and take the same `params`, `IEnumerable`, `List`, `Span` and `ReadOnlySpan` overloads as `AddChildren`.
- `ToggleButtonGroup` gets typed `SetValue`, `AddValueChanged`, `RemoveValueChanged` and `SetLabel` overloads for `ToggleButtonGroupState`, so calls such as `AddValueChanged(evt => …)` need no type arguments.
- `TextField`, `IntegerField`, `LongField`, `UnsignedIntegerField`, `UnsignedLongField`, `FloatField`, `DoubleField` and `Hash128Field` get chainable setters: `SetMaxLength`, `SetMaskChar`, `SetDelayed`, `SetReadOnly`, `SetPassword`, `SetPlaceholder`, `SetHidePlaceholderOnFocus`, `SetKeyboardType`, `SetAutoCorrection`, `SetHideMobileInput`, `SetHideSoftKeyboard`, and for text selection `SetSelectable`, `SetSelectAllOnFocus`, `SetSelectAllOnMouseUp`, `SetDoubleClickSelectsWord`, `SetTripleClickSelectsLine`, `SetCursorIndex`, `SetSelectIndex`, `AddOnCursorIndexChange` / `RemoveOnCursorIndexChange`, `AddOnSelectIndexChange` / `RemoveOnSelectIndexChange`. The `ITextEdition` and `ITextSelection` setters do not reach these fields. Other value types use `TextInputBaseFieldExtensions` and `TextInputBaseFieldTextSelectionExtensions`.
- Added `AndApplyWithoutUndo` counterparts for every `SerializedProperty` setter with immediate application, including `SetValue` overloads, object references, enums, and array size helpers.
- Analyzer `AFT0009` (warning) — two `[TypeSelector]` base types have no type in common, so the selector is empty.
- Analyzer `AFT0010` (warning) — a `this.Marker()` call opens no profiler marker because the generator cannot support its type: the type is `private` or `protected` (or nested in such a type), or it reuses a type parameter name of a containing type.
- Analyzer `AFT0011` (warning) — the scope of `this.Marker()` is discarded (`this.Marker();` as a statement, `_ = this.Marker();`, a local nothing reads), so the sample it begins never ends.

### Changed

- The Agent Skills for this package moved from the `aspid-fasttools` Claude Code plugin in [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) into this repository (`skills/`). Install them into Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI or another agent with `npx skills add VPDPersonal/Aspid.FastTools`; the plugin is no longer published.
- Renamed `GetScriptName()` to `GetDisplayName()` and `GetScriptNameWithIndex()` to `GetDisplayNameWithIndex()`; update existing calls to the new names. Both methods now return `string.Empty` for null or destroyed objects. Component indexing uses a pooled list instead of temporary arrays and LINQ.
- Renamed VisualElement extensions; update existing calls:
  - `SetIsDelayed` / `SetIsPassword` / `SetIsReadOnly` / `SetIsSelectable` → `SetDelayed` / `SetPassword` / `SetReadOnly` / `SetSelectable`;
  - `IsFocus` → `IsFocused`, `SetFocus` / `SetBlur` → `FocusSelf` / `BlurSelf`;
  - `EnableInClass` / `ToggleInClass` → `EnableClass` / `ToggleClass`;
  - `AddStyleSheets` / `RemoveStyleSheets` → `AddStyleSheet` / `RemoveStyleSheet`, `AddStyleSheetsFromResource` / `RemoveStyleSheetsFromResource` → `AddStyleSheetFromResources` / `RemoveStyleSheetFromResources`;
  - `SetImageFromResource`, `SetSpriteFromResource`, `SetVectorImageFromResource`, `SetBackgroundImageFromResource` → `…FromResources`;
  - `MarkDirtyLayout` → `MarkDirtyLayoutSelf`: `IMGUIContainer.MarkDirtyLayout()` hid the old extension, so the call returned `void`; the new name chains.
- Renamed the extension classes `BaseFieldExtensionsSetLabel<Type>` to `BaseField<Type>Extensions` (`BaseFieldExtensionsSetLabelInt` → `BaseFieldIntExtensions`) and `ProgressBarExtensions` to `AbstractProgressBarExtensions`. Extension-method calls are unaffected; only calls through the class name change.
- `SetShowMixedValue` no longer defaults its argument to `true`; pass the value explicitly.
- `SetDirection`, `SetFill`, `SetInverted`, `SetPageSize` and `SetShowInputField` now return the slider's own type (`Slider`, `SliderInt`) instead of `BaseSlider<TValue>`, so a chain keeps the slider's members. They accept `BaseSlider<float>` and `BaseSlider<int>`; the open `BaseSlider<TValue>` overloads are gone. The `BaseSlider<int>` overloads live in the new `SliderIntExtensions` class: extension-method calls are unaffected, but a call through `SliderExtensions.` on a `SliderInt` must switch to `SliderIntExtensions.`.
- The parameterless constructors of `SerializableType` / `SerializableType<T>` are no longer public: create a wrapper with `new SerializableType(type)` or `new SerializableType<T>(type)` (`null` gives an empty one). `SerializableMonoScript` / `SerializableMonoScript<T>` have no public constructors: declare them as serialized fields and pick the script in the Inspector.
- `[TypeSelector]` on a `[SerializeReference]` field now offers only types assignable to every attribute type — the rule `string` and `SerializableType` fields already follow; it used to offer types matching any one of them. The same applies to `baseTypes` of `SerializeReferenceEditorGUI.CreateField`, `CreateList` and `DrawFieldLayout`. A list of alternatives such as `typeof(Pistol), typeof(Rifle)` now leaves the selector empty and triggers `AFT0009`: give the allowed classes a common interface or base class and pass that instead. `AFT0005` now checks all attribute types together.
- In the Inspector of a runtime object, the type picker no longer offers types from editor-only assemblies (`UnityEditor`, Editor asmdefs and `Editor` folders): a player cannot resolve them, so the value silently became `null` or a missing reference in a build. Fields of editor windows and other editor-only objects still offer every type.
- The unconstrained type picker (`SerializableType` without `T`, `[TypeSelector]` on a string, `TypeField`) reuses its type list between openings instead of rebuilding it from every loaded type each time.
- `SerializeReferenceIMGUIList.Draw` now throws `ArgumentNullException` for a `null` property and `ArgumentException` for a property that is not a list of managed references, like `SerializeReferenceEditorGUI.CreateList`; it used to draw nothing. A `null` label now shows the property's display name; pass `GUIContent.none` to hide it.
- The generated `this.Marker()` code now declares its marker fields only under `ENABLE_PROFILER`, like the `Marker()` body that reads them: builds without the profiler no longer create every marker in a static constructor.
- `ProfilerMarkerExtensionsForGenerator.Marker(this object)` is now `Marker<T>(this T, [CallerLineNumber] int line = -1)`: a struct call site the generator cannot mark is no longer boxed, so a Burst job still compiles. It takes the same parameters as the generated overload, so a type without a namespace, as in Unity's script template, gets its markers too. A `this.Marker()` call inside an expression tree no longer compiles (CS0854), and a method group now needs `Func<int, AutoScope>`.
- `AFT0010` now reports every `this.Marker()` call that opens no marker, not only unsupported types: a receiver of another type (`other.Marker()`, calls in static classes), a default interface method, an explicit argument (`this.Marker(5)`) or type argument, `this?.Marker()`, the static call form, a method group and a call inside an expression tree. The generator no longer emits dead markers for such calls.
- A generic class names nested type arguments the way C# writes them (`Foo<List<Int32>>.Run (line)`, `Foo<Outer<Int32>.Inner>.Run (line)` instead of ``Foo<List`1>``, `Foo<Inner>`). A generic struct gets one marker per call site for all its closed types, named `Job<T>.Execute (line)`, because Burst cannot run the per-type label.
- The generated `Marker()` dispatches on the line with a `switch`, so its cost no longer grows with the number of call sites in a type.

### Removed

- Removed `SetExposedReferenceAndApply()` from `SerializedProperty` extensions; call `SetExposedReference()` instead. Without an `IExposedPropertyTable` context, Unity's `exposedReferenceValue` setter already applies the write and records Undo, so the extra apply did nothing, and a variant without Undo cannot be built on top of it.
- Removed the editor-only `SerializableMonoScript.Script`, which returned the `MonoScript` asset; no public accessor for the asset remains. The type is still available through `Type` or the implicit conversion to `Type`.
- Removed `AddMakeItem` / `RemoveMakeItem` of `ListView` and `TreeView`, and `AddMakeHeader` / `AddMakeFooter` / `AddMakeNoneElement` of `BaseListView` with their `Remove*` pairs; the view keeps one factory, so call `SetMakeItem`, `SetMakeHeader`, `SetMakeFooter` or `SetMakeNoneElement`.
- Removed the runtime `Aspid.FastTools.StringExtensions.ToKebabCase` and `Aspid.FastTools.TypeExtensions.GetMembersInfosIncludingBaseClasses` from the public API, without a replacement.

### Fixed

- `GetDisplayName()` and `GetDisplayNameWithIndex()` no longer append " (Script)" when `[AddComponentMenu]` is inherited from a base class or its path is empty or ends with `/`; such types get the nicified type name. The title now comes from the attribute declared on the type itself, and an `[Obsolete]` type no longer gets " (Deprecated)".
- A `[TypeSelector(nameof(...))]` member reference on a field inside a `[Serializable]` class or a list element now resolves on the instance that declares the field, as analyzers `AFT0006`–`AFT0008` already check it; it used to be looked up on the inspected component or asset and showed a warning. The same applies to the picker of the Asset References window.
- Type picker search no longer matches the assembly part of a type name: short queries such as `Key`, `Token`, `ver` or `null` used to match every type. Search compares the label, the type name and the full name with the namespace and declaring types (`Namespace.Outer.Name`).
- Typing and Backspace in the type picker edit the query again after the Down arrow moves into the results; they used to be ignored until the search field was clicked.
- For a missing type the type picker no longer highlights `<None>`, so Enter right after opening does not erase the stored name.
- In a UI Toolkit Inspector, choosing `<None>` now clears a `SerializableMonoScript` whose type is missing; the field used to keep showing `<Missing …>`.
- **+** of a `[TypeSelector]` `[SerializeReference]` list nested in an element of another array (a list of structs, a list inside a list element) now appends to that list; it used to throw `InvalidOperationException` and add nothing.
- **+** of an array whose elements contain a `[TypeSelector]` `[SerializeReference]` field (such as `WeaponSlot[]` in the sample) now adds a regular element; once the array had an element, it used to open that field's type picker and throw.
- **+** of a managed-reference list with several objects selected now appends an independent instance to every object in one Undo group. In IMGUI it used to change only the first object; in UI Toolkit the new element shared its instance with the previous one.
- The `EnumValues` Inspector no longer rewrites keys while drawing: a row whose key the enum cannot parse (after switching `EnumValues<TValue>` to another enum, or renaming or deleting a member) shows its key as, for example, `<Missing Frozen>` and keeps it until you pick a member. It used to get the enum's first member, so switching the type back did not bring the rows back. A row added to an empty table now shows `<None>` and is skipped with an error until you pick a member; it used to get the first member automatically.
- **Populate Missing Enum Members** now works when the value is an array or a `List<T>`; it used to throw and leave a half-created row. For an enum with aliases (`Default = Medium`) it adds each value once and is disabled once every value has a row; it used to add a duplicate on every click.
- An `EnumValues` row of a `[Flags]` enum based on `long` or `ulong`, or on `uint` with a member in bit 31, gets a flags dropdown that keeps every bit; for a 64-bit enum the IMGUI Inspector used to throw on every repaint, and the UI Toolkit one showed a wrong mask and stored a different key.
- In the UI Toolkit Inspector the key dropdown of an `EnumValues` row now follows Undo, Revert and Paste, and the table header shows the label passed to `PropertyField` instead of always the field name.
- Undo after **Fix** of a missing type in an open scene or Prefab Mode brings back the missing reference with its stored data; it used to leave the field empty, and the next save wrote `rid: -2`. The replaced entry is now dropped when the scene or prefab is saved (Prefab Mode Auto Save saves right after the Fix), so Unity's missing-types notice stays until then; that save also clears the object's Undo history, so the repair can no longer be undone.
- The package's editor assembly now compiles on Unity 6000.5, where `Object.GetInstanceID()` became an error (CS0619); it used to leave the project in Safe Mode with every editor tool of the package broken.
- Fields drawn in a regular Inspector, UI Toolkit or IMGUI, now follow the light editor skin: the `EnumValues` drawer takes its backgrounds from Unity's theme, reference notices and the missing-type caption of `[SerializeReference]` fields use Unity's warning colour, and the warning, info and folder icons switch to their light-skin variants.
- The theme override now recolours the cards, panels, status tints, scrollbars, dots background and switches of the Aspid windows: their colours moved into `--aspid-colors-*` tokens, and switches read the optional `--aspid-colors-switch-*` tokens.
- On Unity 6000.0–6000.2 the `[SerializeReference]` field, the picker and the Project References and Asset References windows are styled again: two stylesheets used a color syntax these versions cannot parse, so each was dropped whole and the import logged an error.
- Buttons and foldouts inside a `[SerializeReference]` field drawn with UI Toolkit keep their own look: a button of a custom drawer or a nested list's `+` / `−` is no longer shrunk to 18×18 with a folder icon, and a nested foldout no longer takes the field header's layout.
- Fix, Fix all and the summary's Undo no longer rewrite the type of a healthy managed reference when a `[SerializeReference]` list inside another reference points at the broken one (a parent with a list of children). The Inspector reads the right stored type and fields of such references, and the Project References graph no longer shows phantom nodes whose Clear deleted healthy entries.
- The missing-list guard now snapshots a missing list element that another reference's list also points at, and restores it into the object's own list instead of a same-named list nested in another field.
- A list element restored by the missing-list guard keeps its original reference id when it is free, otherwise gets a random one. It used to take the next id after the file's maximum, which an override in a variant, nested prefab or scene often already used: that override lost its type and shared data with the restored element.
- Objects with a negative fileID (prefab components, sub-assets) are now read as separate documents: their missing references and unset required fields reach the scans and the build gate, and Fix, Clear and orphan removal no longer edit or delete entries of the neighbouring object.
- Fixing a missing `[SerializeReference]` type (retype, clear, remove an orphan, restore a list element) now checks the asset out in version control first, as Unity's own saves do. A read-only asset is left untouched with a clear error instead of an access exception, the edit replaces the file in one step so a failed write no longer leaves it truncated, and a UTF-8 byte-order mark is kept.
- Repairing a missing type no longer silently discards unsaved changes of the asset being repaired. **Fix** and **Smart Fix** in the Inspector and repairs and clears in Asset References offer to save such an asset first; **Fix all**, **Clear** and **Undo** in Project References skip it or clear it in memory. **Fix** in the Inspector no longer rewrites a prefab asset that is open in Prefab Mode.
- `this.Marker()` inside a `private` or `protected` nested type no longer breaks compilation with CS0122. The generated overload cannot see such a type, so the generator now skips it: the call compiles, opens no marker, and `AFT0010` reports it — make the type `internal` or `public` to profile it.
- `this.Marker()` in a type nested in a generic type (`Outer<T>.Inner`) now compiles; the generated overload used to miss the outer type parameters (CS0246).
- `this.Marker()` in an indexer accessor or a static constructor now compiles; the generated field names were invalid. The markers are named `Type.Indexer (line)` and `Type.StaticCtor (line)`.
- `this.Marker()` in an event accessor is now named after the event, like a property accessor: `Type.Changed (line)` instead of `Type.add_Changed (line)` / `Type.remove_Changed (line)`, including explicit interface implementations.
- `this.Marker()` no longer breaks compilation in an auto-property initializer, a static class or a default interface method, in a namespace or type parameter named with a keyword (`Game.@event`, `Foo<@event>`), or when type names differ only in case or flatten to the same name (`Foo`/`foo`, `Foo<T>`/`Foo_1`, `Outer.Inner`/`Outer_Inner`); some of these made every marker of the assembly disappear.
- `this.Marker()` in an `[Obsolete]` type, a type nested in one or a generic type constrained to one no longer raises CS0618 in the generated code, and in an `[Obsolete(..., true)]` type no longer breaks compilation with CS0619.
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
- **Asset References** — the asset's whole `[SerializeReference]` graph from YAML with a warning band on missing types and `SHARED` badges, inline Fix, Clear for orphans, Open Source Prefab.
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

#### Samples

- **Types**, **SerializeReferences**, **EnumValues**, **ProfilerMarkers**, **EditorTools** — one working scene (or window) each with a `README.md`.

#### Documentation and tooling

- English and Russian docs in `Documentation/`, published at https://vpdpersonal.github.io/Aspid.FastTools/.
- `aspid-fasttools` Claude Code plugin in [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins).
- `upm` / `upm/<version>` for stable releases, `upm-preview` for prereleases. Until the first stable release, the `upm` branch still holds the older `com.aspid.fasttools` package (`1.0.0-rc.2`).
- EditMode tests for the YAML editor and the CI-gate scan.

[1.0.0-rc.8]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8
