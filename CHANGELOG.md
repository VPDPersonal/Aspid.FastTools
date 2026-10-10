# Changelog

> Русская версия: [CHANGELOG.ru.md](CHANGELOG.ru.md).

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `RemoveChildren` and `RemoveChildrenIf` remove several children in one call and take the same `params`, `IEnumerable`, `List` and `ReadOnlySpan` overloads as `AddChildren`.
- `AddClasses`, `RemoveClasses`, `ToggleClasses` and `EnableClasses` change several USS classes in one call and take the same `params`, `IEnumerable`, `List` and `ReadOnlySpan` overloads as `AddChildren`. An `IEnumerable` may read `GetClasses()` of the same element. `EnableClasses` takes the flag first: `EnableClasses(isFree, "selected", "free")`.
- `InsertStyleSheet` inserts a style sheet at an index, `EnableStyleSheet(sheet, enable)` adds or removes it, and `ClearStyleSheets` removes every style sheet.
- `AddStyleSheets`, `InsertStyleSheets`, `RemoveStyleSheets` and `EnableStyleSheets` take several style sheets, with the same overloads as `AddClasses`.
- Every style sheet method has a `…FromResources` variant; the plural ones take several paths: `AddStyleSheetsFromResources("UI/Base", "UI/Dark")`.
- Every class and style sheet method has an `…If` variant, for example `AddClassIf`, `EnableClassIf` and `AddStyleSheetFromResourcesIf`.
- `ToggleButtonGroup` gets typed `SetValue`, `AddValueChanged`, `RemoveValueChanged` and `SetLabel` overloads for `ToggleButtonGroupState`, so calls such as `AddValueChanged(evt => …)` need no type arguments.
- `RectIntField` gets a typed `SetLabel`, so `SetLabel("Area")` needs no type arguments, as on `RectField`.
- `SetRootItemsSelf` fills a `TreeView` or `MultiColumnTreeView` in a chain: `tree.SetAutoExpand(true).SetRootItemsSelf(items)`.
- `TextField`, `IntegerField`, `LongField`, `UnsignedIntegerField`, `UnsignedLongField`, `FloatField`, `DoubleField` and `Hash128Field` get chainable setters: `SetMaxLength`, `SetMaskChar`, `SetDelayed`, `SetReadOnly`, `SetPassword`, `SetPlaceholder`, `SetHidePlaceholderOnFocus`, `SetKeyboardType`, `SetAutoCorrection`, `SetHideMobileInput`, `SetHideSoftKeyboard`, and for text selection `SetSelectable`, `SetSelectAllOnFocus`, `SetSelectAllOnMouseUp`, `SetDoubleClickSelectsWord`, `SetTripleClickSelectsLine`, `SetCursorIndex`, `SetSelectIndex`, `AddOnCursorIndexChange` / `RemoveOnCursorIndexChange`, `AddOnSelectIndexChange` / `RemoveOnSelectIndexChange`. The `ITextEdition` and `ITextSelection` setters do not reach these fields. Other value types use `TextInputBaseFieldExtensions` and `TextInputBaseFieldTextSelectionExtensions`.
- Added `AndApplyWithoutUndo` counterparts for every `SerializedProperty` setter with immediate application, including `SetValue` overloads, object references, enums, and array size helpers.
- Analyzer `AFT0009` (warning) — two `[TypeSelector]` base types have no type in common, so the selector is empty.
- Analyzer `AFT0010` (warning) — a `this.Marker()` call opens no profiler marker because the generator cannot support its type: the type is `private` or `protected` (or nested in such a type), or it reuses a type parameter name of a containing type.
- Analyzer `AFT0011` (warning) — the scope of `this.Marker()` is discarded (`this.Marker();` as a statement, `_ = this.Marker();`, a local nothing reads), so the sample it begins never ends.
- Missing `[SerializeReference]` types set by prefab instance overrides (prefab variants, nested prefabs, prefab instances in scenes) are now found: the build / CI gate counts them as missing types, Project References lists them in a **Prefab instance overrides** card, and the delete guard and breakage detection count them. They are not rewritten; fix them on the instance or revert the override. An old name that `[MovedFrom]` maps is listed there as a pending migration, since **Migrate all** does not rewrite overrides. The CI report gains a trailing `origin` column, `override` for these rows.
- A `SerializableType` or `SerializableMonoScript` field or a `[TypeSelector]` string whose stored type no longer resolves shows a **Missing type** notice under the field. **Fix** opens the type picker to choose a replacement. When the class only moved to another namespace or assembly and exactly one compatible type has its name, a **→ TypeName** button replaces the stored name in one click. The caption shows the type name without its assembly, `<Missing Game.Combat.Spear>`, and the tooltip shows the whole stored name.
- Missing `SerializableType` and `SerializableMonoScript` names are found across the project. **Project References → Scan Project** lists them in **type name** groups with **Fix all**, **Smart Fix** and **Undo**; prefab instance overrides and fields in managed references are rewritten too. The build / CI gate reports them as `MissingTypeName` rows, with the stored name in the `className` column and the wrapper field in `fieldPath`, and breakage detection reports a name that stops resolving. `[TypeSelector]` strings are still checked only in the Inspector.

### Changed

- The minimum Unity version is now 6000.0.53f1 instead of 6000.0.0f1. The package uses UI Toolkit APIs that later 6000.0 patches added (`IStyle.unityEditorTextRenderingMode`, `IStyle.unitySliceType`, `UxmlElement.libraryPath`), so it did not compile on earlier patches.
- The package no longer ships the `Documentation` folder with its pages and images, so they are not imported into your project; the documentation lives on the site: https://vpdpersonal.github.io/Aspid.FastTools/. Each sample keeps a short text README on how to open it, with a link to its full tutorial on the site.
- The Agent Skills for this package moved from the `aspid-fasttools` Claude Code plugin in [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) into this repository (`skills/`). Install them into Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI or another agent with `npx skills add VPDPersonal/Aspid.FastTools`; the plugin is no longer published.
- Renamed `GetScriptName()` to `GetDisplayName()` and `GetScriptNameWithIndex()` to `GetDisplayNameWithIndex()`; update existing calls to the new names. Both methods now return `string.Empty` for null or destroyed objects. Component indexing uses a pooled list instead of temporary arrays and LINQ.
- Renamed VisualElement extensions; update existing calls:
  - `SetIsDelayed` / `SetIsPassword` / `SetIsReadOnly` / `SetIsSelectable` → `SetDelayed` / `SetPassword` / `SetReadOnly` / `SetSelectable`;
  - `IsFocus` → `IsFocused`, `SetFocus` / `SetBlur` → `FocusSelf` / `BlurSelf`;
  - `EnableInClass` / `ToggleInClass` → `EnableClass` / `ToggleClass`;
  - `AddStyleSheetsFromResource` / `RemoveStyleSheetsFromResource` → `AddStyleSheetFromResources` / `RemoveStyleSheetFromResources`; `AddStyleSheets(sheet)` and `RemoveStyleSheets(sheet)` still compile, as the plural names now take several style sheets;
  - `SetImageFromResource`, `SetSpriteFromResource`, `SetVectorImageFromResource`, `SetBackgroundImageFromResource` → `…FromResources`;
  - `MarkDirtyLayout` → `MarkDirtyLayoutSelf`: `IMGUIContainer.MarkDirtyLayout()` hid the old extension, so the call returned `void`; the new name chains.
  - `SetText` of `TextElement` (`Label`, `Button` and others) → `SetTextSelf`: from Unity 6000.6 a string argument binds to Unity's own `TextElement.SetText(ReadOnlySpan<char>)`, which returns `void`, so chains such as `label.SetText(x).SetTooltip(y)` failed to compile; the new name chains on every Unity version. `SetText` of `Foldout`, `HelpBox` and `Toggle` keeps its name.
- Renamed the extension classes `BaseFieldExtensionsSetLabel<Type>` to `BaseField<Type>Extensions` (`BaseFieldExtensionsSetLabelInt` → `BaseFieldIntExtensions`) and `ProgressBarExtensions` to `AbstractProgressBarExtensions`. Extension-method calls are unaffected; only calls through the class name change.
- `SetShowMixedValue` no longer defaults its argument to `true`; pass the value explicitly.
- `SetDirection`, `SetFill`, `SetInverted`, `SetPageSize` and `SetShowInputField` now return the slider's own type (`Slider`, `SliderInt`) instead of `BaseSlider<TValue>`, so a chain keeps the slider's members. They accept `BaseSlider<float>` and `BaseSlider<int>`; the open `BaseSlider<TValue>` overloads are gone. The `BaseSlider<int>` overloads live in the new `SliderIntExtensions` class: extension-method calls are unaffected, but a call through `SliderExtensions.` on a `SliderInt` must switch to `SliderIntExtensions.`.
- The parameterless constructors of `SerializableType` / `SerializableType<T>` are no longer public: create a wrapper with `new SerializableType(type)` or `new SerializableType<T>(type)` (`null` gives an empty one). `SerializableMonoScript` / `SerializableMonoScript<T>` have no public constructors: declare them as serialized fields and pick the script in the Inspector.
- `[TypeSelector]` on a `[SerializeReference]` field now offers only types assignable to every attribute type — the rule `string` and `SerializableType` fields already follow; it used to offer types matching any one of them. The same applies to `baseTypes` of `SerializeReferenceEditorGUI.CreateField`, `CreateList` and `DrawFieldLayout`. A list of alternatives such as `typeof(Pistol), typeof(Rifle)` now leaves the selector empty and triggers `AFT0009`: give the allowed classes a common interface or base class and pass that instead. `AFT0005` now checks all attribute types together.
- An IMGUI inspector no longer runs a full duplicate scan of a `[SerializeReference]` list for every element on every GUI event, only once per list per editor update or when the list size changes; a shared field no longer creates a `SerializedObject` on every repaint.
- Breakage detection keeps its baseline in memory and writes it to the session once, before a script reload, instead of parsing and rewriting all of it on every asset save, which cost milliseconds per thousand assets. Its start-up read decodes only the assets that hold a managed reference or a type name, instead of reading every asset in full.
- Deleting or moving an asset no longer drops the warm usage index of Project References, the delete guard and breakage detection. A delete removes the usages of that asset, a move into or out of an **Excluded scan folder** removes or adds them, and a move between scanned folders changes nothing. An import of more than 64 scanned assets drops the index once instead of patching it asset by asset.
- In the Inspector of a runtime object, the type picker no longer offers types from editor-only assemblies (`UnityEditor`, Editor asmdefs and `Editor` folders): a player cannot resolve them, so the value silently became `null` or a missing reference in a build. Fields of editor windows and other editor-only objects still offer every type.
- The unconstrained type picker (`SerializableType` without `T`, `[TypeSelector]` on a string, `TypeField`) reuses its type list between openings instead of rebuilding it from every loaded type each time.
- `SerializeReferenceIMGUIList.Draw` now throws `ArgumentNullException` for a `null` property and `ArgumentException` for a property that is not a list of managed references, like `SerializeReferenceEditorGUI.CreateList`; it used to draw nothing. A `null` label now shows the property's display name; pass `GUIContent.none` to hide it.
- The generated `this.Marker()` code now declares its marker fields only under `ENABLE_PROFILER`, like the `Marker()` body that reads them: builds without the profiler no longer create every marker in a static constructor.
- `ProfilerMarkerExtensionsForGenerator.Marker(this object)` is now `Marker<T>(this T, [CallerLineNumber] int line = -1)`: a struct call site the generator cannot mark is no longer boxed, so a Burst job still compiles. It takes the same parameters as the generated overload, so a type without a namespace, as in Unity's script template, gets its markers too. A `this.Marker()` call inside an expression tree no longer compiles (CS0854), and a method group now needs `Func<int, AutoScope>`.
- `AFT0010` now reports every `this.Marker()` call that opens no marker, not only unsupported types: a receiver of another type (`other.Marker()`, calls in static classes), a default interface method, an explicit argument (`this.Marker(5)`) or type argument, `this?.Marker()`, the static call form, a method group and a call inside an expression tree. The generator no longer emits dead markers for such calls.
- A generic class names nested type arguments the way C# writes them (`Foo<List<Int32>>.Run (line)`, `Foo<Outer<Int32>.Inner>.Run (line)` instead of ``Foo<List`1>``, `Foo<Inner>`). A generic struct gets one marker per call site for all its closed types, named `Job<T>.Execute (line)`, because Burst cannot run the per-type label.
- The generated `Marker()` dispatches on the line with a `switch`, so its cost no longer grows with the number of call sites in a type.
- The generated `Marker()` takes a struct by `in`, so `this.Marker()` in a struct, such as a large job, no longer copies the whole instance on every call. The copy was made under Mono (the Editor, Mono players) and under IL2CPP when the call and the overload land in different C++ files.
- The Sample Themes window hooks the editor update loop, camera rendering and scene saving only while a **Light** or **Dark** preview is on; in the default **Authored** mode it adds no editor callbacks.
- `Aspid.FastTools.VisualElements.Math` is no longer compiled as an empty assembly in projects without `com.unity.mathematics`.
- The animated dot background of the FastTools window and the settings page draws all its dots as one mesh instead of tessellating a path per dot on every frame, and pauses while Unity is in the background, so an idle open window no longer keeps the editor busy.
- The FastTools window (Welcome, Asset References, Project References and Settings) and its pages in Project Settings and Preferences follow the light editor skin: its canvas, cards, text, status colours and the type picker inside it take a light palette, `Aspid-FastTools-Default-Light.uss`; they used to stay dark on both skins. A theme override still layers on top of either palette.
- `[TypeSelector]` on an array or `List<T>` field now applies to the collection itself (`applyToCollection`): its drawer draws the list and a picker for each element. `PropertyField` and `EditorGUILayout.PropertyField` on such a list now get the picker-backed **+** in UI Toolkit and IMGUI, and a `PropertyField` on a single element of it no longer shows the picker. `SerializeReferenceIMGUIList.Draw` and `SerializeReferenceEditorGUI.CreateList` add the constraints of a `[TypeSelector]` on the list field to `baseTypes`. Managed-reference lists drawn by the package now honor `[NonReorderable]`.
- Renamed the `value` parameter of `AddClass`, `RemoveClass`, `ToggleClass`, `AddStyleSheet`, `RemoveStyleSheet` and their `…If` variants to `className` and `styleSheet`; a call that names the `value:` argument needs the new name.
- Style sheet methods such as `AddStyleSheet` and `RemoveStyleSheet` skip a `null` style sheet instead of throwing `ArgumentNullException`.
- `RemoveChild` and `RemoveChildren` skip `null` and elements that are not children of the element instead of throwing, so `RemoveChildren` no longer stops halfway.
- The child, class and style sheet extensions carry nullable annotations, so a project with nullable reference types enabled sees which arguments may be `null`.

### Removed

- Removed `SetExposedReferenceAndApply()` from `SerializedProperty` extensions; call `SetExposedReference()` instead. Without an `IExposedPropertyTable` context, Unity's `exposedReferenceValue` setter already applies the write and records Undo, so the extra apply did nothing, and a variant without Undo cannot be built on top of it.
- Removed the editor-only `SerializableMonoScript.Script`, which returned the `MonoScript` asset; no public accessor for the asset remains. The type is still available through `Type` or the implicit conversion to `Type`.
- Removed `AddMakeItem` / `RemoveMakeItem` of `ListView` and `TreeView`, and `AddMakeHeader` / `AddMakeFooter` / `AddMakeNoneElement` of `BaseListView` with their `Remove*` pairs; the view keeps one factory, so call `SetMakeItem`, `SetMakeHeader`, `SetMakeFooter` or `SetMakeNoneElement`.
- Removed the `Span<VisualElement>` overloads of `AddChildren`, `InsertChildren` and their `…If` variants. A `Span` argument converts to the `ReadOnlySpan` overload, so existing calls compile unchanged.
- Removed the runtime `Aspid.FastTools.StringExtensions.ToKebabCase` and `Aspid.FastTools.TypeExtensions.GetMembersInfosIncludingBaseClasses` from the public API, without a replacement.

### Fixed

- `InsertChildren` and `InsertChildrenIf` skip a `null` entry without moving the index: `InsertChildren(0, a, null, b)` puts `b` right after `a`; it used to leave a gap or throw `ArgumentOutOfRangeException` at the end of the parent.
- The `SHARED` badge in Asset References now fits its text; it used to shrink to the width of its colour dot, so the text ran out of it and the dot covered the letter "H".
- Asset References shows unset required fields (the **Required type is not set** cards and REQUIRED badges) of an asset in an **Excluded scan folder**, as it already showed its missing types; the folder still keeps the asset out of Project References and the build / CI checks.
- `GetDisplayName()` and `GetDisplayNameWithIndex()` no longer append " (Script)" when `[AddComponentMenu]` is inherited from a base class or its path is empty or ends with `/`; such types get the nicified type name. The title now comes from the attribute declared on the type itself, and an `[Obsolete]` type no longer gets " (Deprecated)".
- A `[TypeSelector(nameof(...))]` member reference on a field inside a `[Serializable]` class or a list element now resolves on the instance that declares the field, as analyzers `AFT0006`–`AFT0008` already check it; it used to be looked up on the inspected component or asset and showed a warning. The same applies to the picker of the Asset References window.
- `AFT0007` no longer fails compilation on a `Type?` member under `#nullable enable`, and it now reports a property without a getter, which the Inspector cannot read. `AFT0004` now also reports a `UnityEngine.Object?` field.
- `AFT0005` no longer warns about a generic field or base type (`IState<Enemy>`, `Base<int>`, `typeof(IFoo<int>)`) that has a closed or open generic implementation, or about an implementation through `out`/`in` variance, which also no longer triggers `AFT0003`. Neither warns about a field of a type parameter (`T` in `Slot<T>`). An unbound `typeof(Base<>)` no longer triggers `AFT0003` or `AFT0005` when an open generic implementation such as `Impl<T> : Base<T>` can fill the field.
- `[field: TypeSelector]` and `[field: SerializeReference, TypeSelector]` on an auto-property are now checked by `AFT0001`–`AFT0009`, and `AFT0003` now also covers `SerializableType<T>` and `SerializableMonoScript<T>` fields.
- **Fix** and **Smart Fix** of a missing type on a saved asset in a UI Toolkit inspector no longer log an exception; reference fields in other inspectors refresh right after the repair.
- In Edit Mode the *Shared reference* notice, its number and the *Make Unique Reference* item now update right after an edit from an IMGUI inspector, *Link to Existing* or the reference graph window; they used to keep the state from before the edit until the selection changed.
- A nested `[SerializeReference]` field now keeps a custom drawer registered for its declared type through an open generic type (`typeof(Effect<>)`), a base class or an interface, even without `useForChildren`, and a drawer registered for a base attribute class; the package used to draw its own header and type picker over them. In a nested list such a drawer draws each element in both IMGUI and UI Toolkit, and **+** keeps the type picker. A drawer of the stored type alone does not replace the type picker.
- The per-user settings reset no longer lists the removed "Dropdown without [TypeSelector]" option in its tooltip and confirmation dialog.
- `ComponentTypeSelector` now adds the components the new class requires with `[RequireComponent]`, and refuses the switch with a Console warning when it would duplicate a `[DisallowMultipleComponent]` class, remove a class another component requires, or need a component that cannot be added. The required components are added before the new class, so its `OnValidate` finds them. It used to leave the object without the required components or with duplicates.
- **Create New Script…** on a `[SerializeReference]` field suggests `NewInteractable` for `IInteractable` (was `Newnteractable`) and `NewEffect` for `IEffect<T>` or a generic base class (was the invalid ``NewEffect`1``, rejected on accept).
- A saved template whose type does not resolve at the moment (another branch, a removed package, a class moved to another namespace or assembly) is now only hidden from **Paste Template**; opening the context menu used to delete it for good, in every copy of the project. **Paste Template → Remove Missing (N)…** deletes such templates on request, and overwriting one by name says that its type is missing.
- **Excluded scan folders** accepts only `Assets` and folders under it, the only ones project scans walk; a folder under `Packages/` or elsewhere was saved but excluded nothing.
- Changing **Excluded scan folders** now restarts the baseline of breakage detection: a type that only an excluded folder uses no longer raises a breakage notification, and a folder that is no longer excluded is covered again.
- Breakage detection now also reacts to changes of `.asmdef`, `.asmref` and `.dll` files, which move types between assemblies. A scan that waits for scripts to compile is no longer lost with the domain reload that ends the compile.
- Type picker search no longer matches the assembly part of a type name: short queries such as `Key`, `Token`, `ver` or `null` used to match every type. Search compares the label, the type name and the full name with the namespace and declaring types (`Namespace.Outer.Name`).
- Typing and Backspace in the type picker edit the query again after the Down arrow moves into the results; they used to be ignored until the search field was clicked.
- On Unity 6000.6 and later, a search that shortens the type picker's list past the selected row no longer overflows the stack, which usually crashed the Editor: the list's refresh reported a selection change, and the picker answered it with another refresh.
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
- The type picker is readable on the light editor skin: opened from an Inspector field it takes a light palette, with light-skin folder, Favorites and Recent icons; it used to draw the dark palette's light text on Unity's light popup background.
- The Welcome tab of the FastTools window takes the theme override too; it kept the built-in palette.
- The theme override now recolours the cards, panels, status tints, scrollbars, dots background and switches of the Aspid windows: their colours moved into `--aspid-colors-*` tokens, and switches read the optional `--aspid-colors-switch-*` tokens.
- On Unity 6000.0–6000.2 the `[SerializeReference]` field, the picker and the Project References and Asset References windows are styled again: two stylesheets used a color syntax these versions cannot parse, so each was dropped whole and the import logged an error.
- Buttons and foldouts inside a `[SerializeReference]` field drawn with UI Toolkit keep their own look: a button of a custom drawer or a nested list's `+` / `−` is no longer shrunk to 18×18 with a folder icon, and a nested foldout no longer takes the field header's layout.
- Fix, Fix all and the summary's Undo no longer rewrite the type of a healthy managed reference when a `[SerializeReference]` list inside another reference points at the broken one (a parent with a list of children). The Inspector reads the right stored type and fields of such references, and the Project References graph no longer shows phantom nodes whose Clear deleted healthy entries.
- The missing-list guard now snapshots a missing list element that another reference's list also points at, and restores it into the object's own list instead of a same-named list nested in another field.
- The missing-list guard no longer brings back a missing list element after you delete it or set it to `<None>` and save; when you delete another element, it restores the missing ones at their new indexes. If the deleted element sat among other missing or `<None>` elements, the saved file doesn't say which one it was: the guard keeps the `<None>` elements and the first missing ones and leaves the last missing one out. Loaded scenes and prefabs saved from Prefab Mode are no longer guarded: the guard doesn't rewrite their files and doesn't restore their missing list elements.
- A list element restored by the missing-list guard keeps its original reference id when it is free, otherwise gets a random one. It used to take the next id after the file's maximum, which an override in a variant, nested prefab or scene often already used: that override lost its type and shared data with the restored element.
- Objects with a negative fileID (prefab components, sub-assets) are now read as separate documents: their missing references reach the scans and the build gate, their unset required fields reach the scans, and Fix, Clear and orphan removal no longer edit or delete entries of the neighbouring object.
- A `[SerializeReference]` field of a prefab variant, nested prefab or scene instance whose type is missing now shows **Missing type** instead of an empty `<None>`; the tooltip names the source prefab to repair, or says the type is stored in a prefab override. A required field with such a missing type no longer counts as unset in `-srGateRequired` and the References windows.
- The breakage notification no longer waits for Project References to scan the project in the session: a background read of the asset files at session start sets its baseline, and assets saved later update it, so a type first assigned during the session is covered too. A type that breaks before that read completes is not reported.
- Deleting a script that declares only components or ScriptableObjects no longer scans every asset of the project; for other scripts a progress bar appears when the check takes long, and cancelling it cancels the delete.
- The delete guard now warns for every type declared in a script, including a file with no class named after it (`Pistol` and `Rifle` in `Weapons.cs`) and nested types (`Outer.Inner`); it used to check only the class named after the file. A `partial` type counts only when no remaining script declares another part of it.
- The SerializeReference scans skip binary assets and Git LFS pointers after reading their first bytes, instead of decoding every file as text on each build. The build gate and the CI check warn when files went unchecked (any binary file outside **Force Text**, a binary prefab, scene or ScriptableObject in **Force Text**, a pointer that was never pulled), and the CI report counts and lists them; before, such a project passed with 0 violations.
- A `[TypeSelector(Required = true)]` array or list in a scene is no longer reported as unset by `-srGateRequired` and Project References; scenes skip collections, as documented.
- Project References no longer shows "Project clean" or "Nothing left to repair" while required fields are unchecked (before the first Scan Project, after a clear in Prefab Mode or an open scene, or with gate severity Off); it says why they were not checked and to Rescan. Clearing references on disk now re-checks the edited files and the variants and prefabs nesting an edited prefab, so their required violations stay listed and the header warns about them.
- The required-field sweep of `-srGateRequired` and Scan Project now unloads the prefabs and assets it loaded, so its memory no longer grows with the size of the project.
- Assign Required for a `SerializableMonoScript` field in Asset References now offers only types with a script asset, as the Inspector does; it used to accept nested, generic and DLL types and drop the script reference.
- An unset required `SerializableType`, `SerializableType<T>` or `SerializableMonoScript` field of a prefab or ScriptableObject is now reported by `-srGateRequired`, the CI report and the References windows under its own path (`weaponType`), as in a scene; the path used to end in `._assemblyQualifiedName`.
- In the editor, `SerializableMonoScript` now takes the type from its script when the stored name no longer resolves, so an object loaded in Play Mode after a class rename (an additional scene, `Resources.Load`, entering Play Mode without domain and scene reload) no longer gets `null` before its asset is saved again.
- `SerializableType` and `SerializableMonoScript` now cache a failed lookup as well: reading `.Type` of a missing type no longer calls `Type.GetType` every time, and the cache resets when the stored name changes.
- Fixing a missing `[SerializeReference]` type (retype, clear, remove an orphan, restore a list element) now checks the asset out in version control first, as Unity's own saves do. A read-only asset is left untouched with a clear error instead of an access exception, the edit replaces the file in one step so a failed write no longer leaves it truncated, and a UTF-8 byte-order mark is kept.
- Repairing a missing type no longer silently discards unsaved changes of the asset being repaired. **Fix** and **Smart Fix** in the Inspector and repairs and clears in Asset References offer to save such an asset first; **Fix all**, **Clear** and **Undo** in Project References skip it or clear it in memory. **Fix** in the Inspector no longer rewrites a prefab asset that is open in Prefab Mode.
- The summaries in Project References and their **Undo** now survive a switch to another tab of the window, including the jump to Asset References from an entry row; they used to disappear with the tab. **Rescan**, closing the window or a domain reload still clears them.
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
- `AddChildren`, `InsertChildren` and their `…If` variants with an `IEnumerable` no longer throw `InvalidOperationException` ("Collection was modified") when given `Children()` of another element: `target.AddChildren(source.Children())` moves every child.
- Types sample: the `Enemy Type` picker no longer offers the abstract `Enemy`, which spawned empty capsules and an error per enemy; the rename step of the README now uses the IDE's Rename refactoring, so `ArmoredGrunt` keeps compiling.
- SerializeReferences sample: no more CS0414 warning on import, and deleting the Training Dummy in Play Mode no longer throws `MissingReferenceException` on every shot.
- EditorTools sample: the Ability Catalog window follows assets created or deleted in the Project window, and **Create** selects the new asset even while a search is active.
- EnumValues and ProfilerMarkers samples no longer break compilation of a project without the built-in Physics module; their scripts are skipped there, and the sample descriptions say they need it.
- Samples no longer import ten unused screenshots (about 1 MB) as textures.
- `Persistent()` keeps the `context` of the source `SerializedObject`, so an `ExposedReference` read or written through the copy resolves in the same table (for example a `PlayableDirector`) instead of the default value in the asset.
- *Link to Existing* in the `[SerializeReference]` context menu now offers only instances the field's type picker accepts under its `[TypeSelector]` base types, including member-referenced ones; it used to offer any instance assignable to the declared field type.
- *Find Usages* in the `[SerializeReference]` context menu now lists usages of the exact type in the field (namespace and assembly included); it used to match every stored class name containing its name, such as `PistolMk2` or a `Pistol` from another namespace. A typed `sr:` query still matches class names by substring.
- *Find Usages of …* and *Link to Existing* in the `[SerializeReference]` context menu name a generic type as the type picker does, `Modifier<Single>` or its `[TypeSelectorDisplay]` name; they used to show ``Modifier`1``. A `/` in a display name no longer opens a submenu.
- In the default UI Toolkit Inspector, **+** of an empty `[TypeSelector]` `[SerializeReference]` list now opens the type picker; it used to append a `<None>` entry, and the picker appeared only once the list had an element.
- The breakage notification and the breakage detection setting point at Project References; they still named its old tab, Repair.

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
