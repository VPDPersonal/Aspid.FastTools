# Id Registries Redesign

**Status:** Draft — ready for implementation plan
**Date:** 2026-04-22
**Scope:** `Aspid.FastTools.Unity` (Runtime + Editor) `Ids` module

## Goal

Bring the Id-registry subsystem to a polished, consistent, bug-free state:

1. Settle the two registry types (`IdRegistry` vs `StringIdRegistry`) as distinct, first-class tools with different runtime contracts.
2. Normalize naming — drop the inconsistent `IdRegister*` spelling everywhere.
3. Fix a concrete list of bugs and dead-code smells.
4. Improve editor UX: name validation, full Undo, explicit clean-up flow, navigation, sorting/grouping, manual `_nextId` with a safety warning.
5. Remove `StringIdUsageScanner` and its UI hooks — declared too resource-intensive.

Non-goals: generator integration for `IdRegistry` (how an `IdStruct` type picks its backing registry) and tests — both explicitly deferred.

## 1. Architecture and components

### Runtime (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/`)

```
IId.cs                              // unchanged
UniqueIdAttribute.cs                // unchanged
IdRegistry.cs                       // int-only ScriptableObject (runtime)
IdRegistry.Editor.cs                // #if UNITY_EDITOR partial: _names[], _targetStructType, _nextId
StringIdRegistry.cs                 // int↔string ScriptableObject (runtime)
StringIdRegistry.Editor.cs          // #if UNITY_EDITOR partial: _targetStructType, _nextId
```

`IdRegistry` runtime shape:

```csharp
[CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/FastTools/Id Registry")]
public sealed partial class IdRegistry : ScriptableObject, IEnumerable<int>
{
    [SerializeField] private int[] _ids = Array.Empty<int>();

    public int Count => _ids.Length;
    public bool Contains(int id) { /* linear over _ids */ }
    public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)_ids).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

`IdRegistry.Editor.cs` carries the parallel `_names[]` plus `_targetStructType` and `_nextId`. Invariant: `_names.Length == _ids.Length`. Kept under `#if UNITY_EDITOR` so strings do not ship with player builds.

`StringIdRegistry` runtime shape keeps its current layout (`IdEntry[] _entries`) and API (`GetId`, `GetNameId`, `Contains`, pair enumeration), becomes `sealed partial`, and adds internal dictionary caches for O(1) lookups.

Both types have `[CreateAssetMenu]`:
- `Aspid/FastTools/Id Registry` (int-only)
- `Aspid/FastTools/String Id Registry` (full mapping)

### Editor (`Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/`)

New / renamed / removed:

```
Registries/
  IRegistryAccessor.cs                          (new)
  IdRegistryAccessor.cs                         (new)
  StringIdRegistryAccessor.cs                   (new)
  RegistryEditorCore.cs                         (new)
  IdRegistryEditor.cs                           (new, thin delegate)
  StringIdRegistryEditor.cs                     (thin delegate, shrunk)
  IdRegistryEntryVisualElement.cs               (renamed from StringIdRegistryEntryVisualElement)
  IdRegistryValidator.cs                        (renamed from StringIdRegistryValidator; extended)
  IdRegistryResolver.cs                         (renamed from StringIdRegistryHelper; rewritten)
  IdRegistryResolverCacheInvalidator.cs         (renamed)
Drawers/
  IdStructDrawer.cs                             (edits: use Resolver, add Open button, drop scanner usage)
  IdStructPropertyDrawer.cs                     (edits: drop scanner usage)
Windows/
  StringIdSelectorWindow.cs                     (stays; minor edits to caption API if needed)
Constants.cs                                    (edits: remove dead classes, scanner constants)

REMOVED:
  Extensions/IdRegisterEditorExtensions.cs
  Registries/StringIdUsageScanner.cs
```

Key separation: `RegistryEditorCore` knows UI but not storage; `IRegistryAccessor` knows storage but not UI; two thin `CustomEditor` subclasses wire them together.

## 2. Data flow and contracts

### `IRegistryAccessor`

```csharp
internal interface IRegistryAccessor
{
    SerializedObject SerializedObject { get; }
    SerializedProperty TargetStructTypeProperty { get; }
    SerializedProperty NextIdProperty { get; }

    int Count { get; }
    int GetId(int index);
    string GetName(int index);
    void SetName(int index, string name);

    int Add(string name);        // returns assigned Id, bumps NextId
    void RemoveAt(int index);    // atomic across parallel arrays (IdRegistry)
    bool Contains(string name);

    int MaxAssignedId { get; }
    IEnumerable<int> GetGapIds(); // for Next-ID warning heuristic

    void Record(string operationName); // Undo.RegisterCompleteObjectUndo wrapper
}
```

`IdRegistryAccessor` mutates `_ids` and `_names` together inside `Record → mutate → ApplyModifiedProperties` blocks. If lengths ever diverge (e.g. hand-edit via Debug Inspector), the inspector detects it and surfaces a Clean-up affordance instead of silently patching.

`StringIdRegistryAccessor` works against a single `SerializedProperty _entries` and its `Id`/`Name` children.

### `RegistryEditorCore`

One entry point: `VisualElement Build(IRegistryAccessor accessor, SerializedObject so, Object target, Type registryType)`.

Layout, top to bottom:

1. `AspidInspectorHeader` (target name + typename).
2. **Type** section — `PropertyField(TargetStructTypeProperty)` with the existing `[TypeSelector(typeof(IId))]`.
3. **Next ID** section — `IntegerField` bound to `NextIdProperty` with live warning (see §3-J).
4. **IDs** section:
   - `ToolbarSearchField`
   - Sort / Group toolbar (§3-H)
   - Warning row for invalid entries (§3-F) — hidden when clean.
   - `ListView` of the filtered/sorted view model (entries keep their `OriginalIndex` so mutations hit the backing store regardless of view order).
   - Empty-state label.
   - Add-row: `TextField` + `+` with live validation (§3-D).

`serializedObject.TrackSerializedObjectValue` re-runs the view-model rebuild. Every mutation path goes through `accessor.Record(...)` first (see §3-E).

### `IdRegistryResolver`

```csharp
internal static class IdRegistryResolver
{
    public static ScriptableObject? Find(Type declaringType);
    public static IdRegistry? FindIntOnly(Type declaringType);
    public static StringIdRegistry? FindStringMapped(Type declaringType);
    public static StringIdRegistry CreateStringMapped(Type declaringType);
    internal static void ClearCache();
}
```

Lookup path (inside `Find`):

1. Check cache (`Dictionary<string /*AQN*/, ScriptableObject?>`).
2. `AssetDatabase.FindAssets("t:IdRegistry t:StringIdRegistry")`.
3. For each GUID: load as `ScriptableObject`, read `_targetStructType` via `new SerializedObject(obj).FindProperty("_targetStructType").stringValue` — no reflection.
4. Collect all matches for this AQN. If >1 → `Debug.LogError` listing their paths, return first (failsafe). If 1 → cache and return. If 0 → cache null and return null.

Uniqueness is enforced at runtime of the resolver, not at asset creation — so a user who duplicates an asset outside the inspector still gets a clear error rather than undefined behaviour.

### Drawer ↔ Resolver

`IdStructDrawer` consumes string names, so it calls `IdRegistryResolver.FindStringMapped`. If `Find` returns an `IdRegistry` (int-only) for the same type, the dropdown shows a read-only badge: "This struct is bound to an int-only IdRegistry — names unavailable." The drawer's Create button always creates a `StringIdRegistry` via `CreateStringMapped`. Int-only `IdRegistry` assets are created manually through `Assets → Create → Aspid/FastTools/Id Registry`.

## 3. UX details

### D — Name validation

Single source of truth: `IdRegistryValidator.IsValidName(string input, HashSet<string>? existing, out string? error)`. Rules, in order:

1. Not whitespace.
2. Matches `^[A-Za-z_][A-Za-z0-9_]*$` (valid C# identifier).
3. Not a C# reserved keyword (static `HashSet<string>` enumerating the standard keyword list — no Roslyn dependency in editor assembly).
4. Length ≤ 255.
5. Not already in `existing` (case-sensitive).

Used by Add-row, Rename-row, and the Clean-up preview. Failure renders in the row's `_errorLabel`; confirm/add buttons are disabled until valid.

### E — Undo

Every mutation path:

```
accessor.Record("Add ID 'Goblin'");       // Undo.RegisterCompleteObjectUndo(target, op)
// mutate _ids / _names / _entries / _nextId directly (or via SerializedObject)
EditorUtility.SetDirty(target);
```

Operations: `Add ID '{name}'`, `Rename ID '{old}' → '{new}'`, `Delete ID '{name}'`, `Set Next ID`, `Clean Up Invalid IDs`. Replaces the current mix of SO-level undo and reflection-like helpers, so Ctrl+Z always reverts the logical operation, not the intermediate state.

### F — Remove silent `CleanUpInvalid`

Drop the `OnDisable → CleanUpInvalid` call. Instead:

- Each `RegistryEditorCore` rebuild computes `invalidEntries` (empty names + duplicates by name + `_names`/`_ids` length mismatch for `IdRegistry`).
- If any, show a warning row above the list:
  ```
  ⚠ 3 invalid entries (2 duplicates, 1 empty name)   [ Review ]
  ```
- `Review` opens a modal diff dialog. On `Clean up`, run a single Undo-group `"Clean Up Invalid IDs"`. On `Cancel`, nothing happens.
- Data is never mutated without an explicit click.

### G — Open Registry button in `IdStructDrawer`

Small icon button (18×18) inserted between `dropdownButton` and `createToggleButton` in the main row (and a 22px variant in IMGUI). Icon: `EditorGUIUtility.IconContent("d_ScriptableObject Icon")` (fallback `d_Folder Icon`).

Behaviour:
```csharp
openButton.SetEnabled(resolver.Find(fieldType) != null);
openButton.clicked += () =>
{
    var reg = IdRegistryResolver.Find(fieldType);
    if (reg == null) return;
    EditorGUIUtility.PingObject(reg);
    Selection.activeObject = reg;
};
```

### H — Sort / Group

Second toolbar row under `ToolbarSearchField`:

- `EnumField` **Sort**: `RegistryOrder | NameAZ | NameZA | IdAsc | IdDesc` (default `RegistryOrder`).
- `EnumField` **Group**: `None | ByPrefix` (default `None`).

State stored in `SessionState` under `Aspid.FastTools.Ids.Registry:{assetGuid}:Sort` and `:Group`. Persists across recompiles, resets on Unity restart — intentional, since this is a view preference.

View-model pipeline order: **filter (search) → sort → group**. Search works on the underlying name/id strings; sort runs over the filtered set; grouping buckets the sorted result.

Sort operates on the view model only; `_ids` / `_entries` are never reordered. View items carry `OriginalIndex` so edits hit the correct backing slot.

`Group ByPrefix`: prefix = substring before first `_`; entries without `_` go into `<ungrouped>`. The `ListView` is replaced by a vertical stack of `Foldout`s (one per group). Per-group expanded state stored in `SessionState:{assetGuid}:Group:{groupName}:Expanded`.

### J — Manual Next ID

Next-ID section has an `IntegerField` bound to `NextIdProperty` and an adjacent warning icon (`console.warnicon.sml`), `Display:None` by default. `TrackPropertyValue` plus initial bind run `Revalidate`:

- `value > accessor.MaxAssignedId` → icon hidden (forward skip is safe).
- `value ≤ accessor.MaxAssignedId` → icon visible with tooltip:
  > "Reusing ID {value} may silently remap references: assets that previously pointed to this ID will appear bound to the next name you create. Proceed only if you know these IDs are unused."
- `value < 1` → inline error, field gets `aspid-fasttools-status-error` class.

Writes go through the `"Set Next ID"` Undo group. We warn but do not block — the user explicitly chose that trade-off.

## 4. Bug fixes

| # | Location | Bug | Fix |
|---|---|---|---|
| 1 | `StringIdRegistryHelper.cs` L24 | `FindAssets("t:IdRegistry")` loads as `StringIdRegistry` — works by accident. | `IdRegistryResolver` uses `"t:IdRegistry t:StringIdRegistry"` and loads each by its concrete type. |
| 2 | `StringIdRegistryHelper.cs` L30–35 | Reflection to read `_targetStructType`. | Direct `SerializedObject.FindProperty("_targetStructType").stringValue`. |
| 3 | `StringIdRegistryEditor.cs` L31–34 | `OnDisable → CleanUpInvalid` silently mutates. | Removed. Replaced by explicit Clean-up row (§3-F). |
| 4 | `StringIdRegistryValidator.cs` L27 | `HasDuplicate` uses `Count(…) > 1` (no short-circuit). | Early-exit scan: first match sets flag, second returns `true`. |
| 5 | `StringIdRegistry.cs` L26–48 | `GetId` / `GetNameId` / `Contains` are O(n) linear scans. | Lazy `Dictionary<string,int>` + `Dictionary<int,string>` caches, invalidated through an `[NonSerialized] bool _cacheDirty` flag. Dirtied on `OnValidate` and editor mutations. |
| 6 | `StringIdRegistry.cs` L16 | `partial class`, not sealed. | `sealed partial class`. |
| 7 | `StringIdRegistryCacheInvalidator.cs` L10 | Invalidates on every asset import, including unrelated files. | Filter: only when an imported/deleted/moved path ends in `.asset` (narrowest meaningful filter without loading the asset). |
| 8 | `Aspid-FastTools-Id-Registry.uss` L6–49 | Dead classes `header`, `header-icon`, `header-label`, `count-badge`. | Removed. |
| 9 | `IdRegisterEditorExtensions.cs` | Misnamed, ad-hoc wrapper around SerializedProperty. | Deleted. Logic lives in accessors. |
| 10 | `StringIdUsageScanner.cs` | Resource-intensive, not wanted. | Deleted. |
| 11 | `IdStructPropertyDrawer.cs` L99–118 | `CheckIsUnique` scans all assets of the type per redraw; 2s cache is too short. | Out-of-scope note: bump `CacheLifetime` from 2s to 10s inside this spec, leave full rework as a TODO. |
| 12 | `IdRegistry.cs` L10 | `partial class`, not sealed. | `sealed partial class`. |
| 13 | `IdRegistry.cs` L24–28 | `IdEntry { int Id; string Name; }` keeps strings in runtime type. | Replaced by `int[] _ids` runtime + editor-only `string[] _names` partial (see §1). |

`Constants.StringIdFieldName` stays — it is still needed by `IdStructDrawer` for `FindPropertyRelative` calls; only its usage inside the deleted scanner goes away.

## 5. Migration

Registry **asset types** do not rename, so existing `.asset` files keep working:

- `StringIdRegistry` class name is stable — just becomes `sealed partial`.
- `IdRegistry` class name is stable — becomes `sealed partial`, runtime field changes from `IdEntry[] _entries` to `int[] _ids`. Since the current `IdRegistry` is a stub with no authored assets in the repo, no migration guard is needed; but to be safe, the implementation plan will grep the workspace for any in-the-wild `IdRegistry` asset before deleting the old `_entries` field.
- `_targetStructType`, `_nextId` serialized names are preserved on both types.

Renamed non-SO classes (`StringIdRegistryHelper`, `IdRegisterEditorExtensions`, `StringIdRegistryValidator`, `StringIdRegistryEntryVisualElement`, `StringIdRegistryCacheInvalidator`) are internal editor classes — no asset/meta implications.

`.cs.meta` files for deleted source files are deleted alongside them.

## 6. Style & naming rules applied

- All renamed/new classes land in `Aspid.FastTools.Ids.Editors` namespace.
- USS classes remain in the `aspid-fasttools-id-registry-*` family; no new top-level stylesheet.
- Registry `CustomEditor` types are `internal sealed`, consistent with the rest of the package.
- Editor-only runtime partials use `#if UNITY_EDITOR` guards inside the `Unity/Runtime/Ids/` directory — matching the existing `StringIdRegistry.Editor.cs` pattern.

## 7. Out of scope (explicit)

- `IdStructGenerator` changes and the design decision on how an `IdStruct` type declares its backing registry kind.
- Full rework of `IdStructPropertyDrawer.CheckIsUnique` (only the 2s → 10s cache bump).
- Automated tests.
- Refactoring `Samples~/Ids/`.

## 8. Deliverable shape

Final PR touches:

- Rewrites `IdRegistry.cs`, adds `IdRegistry.Editor.cs`.
- Updates `StringIdRegistry.cs` (sealed, cache), touches `StringIdRegistry.Editor.cs` minimally.
- Adds `IRegistryAccessor`, two accessors, `RegistryEditorCore`, `IdRegistryEditor`.
- Rewrites `StringIdRegistryEditor` to delegate to Core.
- Renames / rewrites `IdRegistryResolver`, `IdRegistryValidator`, `IdRegistryEntryVisualElement`, `IdRegistryResolverCacheInvalidator`.
- Updates `IdStructDrawer`, `IdStructPropertyDrawer`, `Constants.cs`.
- Updates `Aspid-FastTools-Id-Registry.uss` (removes dead classes).
- Deletes `IdRegisterEditorExtensions.cs`, `StringIdUsageScanner.cs` (and their `.meta`).
- CLAUDE.md — updates the `StringIds` section to mention both registries and drops the `IdStructGenerator` note about scanner.
