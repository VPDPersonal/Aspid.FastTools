# Id Registries Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Spec:** [docs/superpowers/specs/2026-04-22-id-registries-redesign-design.md](../specs/2026-04-22-id-registries-redesign-design.md)

**Goal:** Bring the `Ids` module to a polished, consistent, bug-free state: split runtime contracts between `IdRegistry` (int-only) and `StringIdRegistry` (int↔string), share editor UX through an accessor abstraction, fix a concrete list of bugs, remove the usage scanner, and add name validation / Undo / explicit clean-up / Open-Registry navigation / sort-group / warned manual NextId.

**Architecture:** Two independent `ScriptableObject` types with different runtime layouts share a single `RegistryEditorCore` (UI) connected through an `IRegistryAccessor` adapter (storage). Editor-only fields live in `#if UNITY_EDITOR` partial classes next to the runtime types. A rewritten `IdRegistryResolver` replaces the reflection-based `StringIdRegistryHelper` and correctly searches both registry types.

**Tech Stack:** Unity 2022.3+, UIToolkit, `Aspid.FastTools` editor assembly. No new dependencies. Tests not included (explicitly out of scope).

**Verification model:** Since this is Unity editor code we cannot drive headlessly, verification per task is:
1. Unity Editor Console is empty (no compile errors, no red exceptions).
2. Specific manual inspector walk-through described per task.
3. Commit.

**Safety rails:**
- Never break existing `.asset` files — class names `IdRegistry` and `StringIdRegistry` stay stable, serialized field names (`_entries`, `_targetStructType`, `_nextId`) stay stable.
- Rename `.cs` + `.cs.meta` together via `git mv` to preserve Unity GUIDs.
- Commit after each task. Each task is independently revertable.

---

## Task ordering overview

```
Phase A — Safe small bug fixes (no architecture impact)
  Task 1  — Small bug bundle (HasDuplicate, dead USS, CacheInvalidator filter, CacheLifetime)
  Task 2  — Delete StringIdUsageScanner + simplify delete dialog

Phase B — Runtime fixes
  Task 3  — StringIdRegistry: sealed + dictionary caches
  Task 4  — IdRegistry runtime rewrite + editor partial

Phase C — Resolver and renames
  Task 5  — IdRegistryResolver (replaces StringIdRegistryHelper)
  Task 6  — Rename StringIdRegistryCacheInvalidator
  Task 7  — Rename + extend StringIdRegistryValidator (add IsValidName)
  Task 8  — Rename StringIdRegistryEntryVisualElement

Phase D — Accessor + Core (architectural lift)
  Task 9  — IRegistryAccessor + two implementations
  Task 10 — RegistryEditorCore skeleton (replicates current UI)
  Task 11 — Rewire StringIdRegistryEditor through Core
  Task 12 — Add IdRegistryEditor (thin delegate)
  Task 13 — Delete IdRegisterEditorExtensions

Phase E — UX additions
  Task 14 — Name validation (D) in Add/Rename flows
  Task 15 — Full Undo (E) via accessor.Record
  Task 16 — Explicit clean-up (F) — remove silent CleanUpInvalid
  Task 17 — Next ID control (J) with warning
  Task 18 — Sort/Group toolbar (H)

Phase F — Drawer integration
  Task 19 — Open Registry button (G) + Int-only registry read-only dropdown hint

Phase G — Docs
  Task 20 — CLAUDE.md update
```

---

## Phase A — Safe small bug fixes

### Task 1: Small bug bundle

Fixes bugs (4), (7), (8), (11) from spec §4. No architecture change. Warm-up to validate dev loop.

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryValidator.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructPropertyDrawer.cs`

- [ ] **Step 1.1: `HasDuplicate` — early exit instead of `Count > 1`**

Replace the `HasDuplicate` method in `StringIdRegistryValidator.cs` (currently uses LINQ `Count(e => e == entryName) > 1`). New version short-circuits on the second match:

```csharp
public static bool HasDuplicate(StringIdRegistry registry, string entryName)
{
    var seen = false;
    foreach (var name in registry.IdNames)
    {
        if (name != entryName) continue;
        if (seen) return true;
        seen = true;
    }
    return false;
}
```

- [ ] **Step 1.2: Remove dead USS classes**

Open `Aspid-FastTools-Id-Registry.uss`. Delete the four rule blocks (lines 6–49) that are not referenced from any `.AddClass(...)` call in C# code:

- `.aspid-fasttools-id-registry-header`
- `.aspid-fasttools-id-registry-header-icon`
- `.aspid-fasttools-id-registry-header-label`
- `.aspid-fasttools-id-registry-count-badge`

Keep the `.aspid-fasttools-id-registry` root class (line 1–4). The file should now start with `.aspid-fasttools-id-registry { margin: 2px 0; }` followed directly by `.aspid-fasttools-id-registry-entry`.

- [ ] **Step 1.3: CacheInvalidator — filter to `.asset` imports only**

Replace the body of `OnPostprocessAllAssets` in `StringIdRegistryCacheInvalidator.cs` with a helper that checks extensions:

```csharp
private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
{
    if (HasAssetPath(imported) || HasAssetPath(deleted) || HasAssetPath(moved))
        StringIdRegistryHelper.ClearCache();
}

private static bool HasAssetPath(string[] paths)
{
    for (var i = 0; i < paths.Length; i++)
        if (paths[i].EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
            return true;
    return false;
}
```

- [ ] **Step 1.4: `CheckIsUnique` cache lifetime 2s → 10s**

In `IdStructPropertyDrawer.cs`, change:

```csharp
private const double CacheLifetime = 2.0;
```

to:

```csharp
private const double CacheLifetime = 10.0;
```

Add a one-line comment above the const: `// Bumped to 10s pending a full rework of CheckIsUnique (see spec §4 row 11).`

- [ ] **Step 1.5: Verify**

In Unity: let the editor recompile. Console must be empty. Open any existing `StringIdRegistry` asset in Inspector — it should render exactly as before (no regressions from USS cleanup). Toggle the duplicate state by adding two entries with the same name — duplicate badge still highlights.

- [ ] **Step 1.6: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryValidator.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructPropertyDrawer.cs
git commit -m "Fix small IdRegistry bugs: duplicate scan, USS dead classes, cache filter, unique-check cache"
```

---

### Task 2: Delete `StringIdUsageScanner`

Removes spec bug (10). Simplifies `StringIdRegistryEditor.TryDeleteEntry` to not show usage counts.

**Files:**
- Delete: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdUsageScanner.cs` (+`.meta`)
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs`

- [ ] **Step 2.1: Delete scanner files**

```bash
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdUsageScanner.cs
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdUsageScanner.cs.meta
```

- [ ] **Step 2.2: Simplify `TryDeleteEntry`**

In `StringIdRegistryEditor.cs`, replace the entire `TryDeleteEntry` method and remove the now-unused `GetStructType` method:

```csharp
private void TryDeleteEntry(int index)
{
    var nameProp = _entriesProp.GetArrayElementAtIndex(index)
        .FindPropertyRelative("Name");

    var nameToDelete = nameProp.stringValue;
    var message = $"Delete '{nameToDelete}'?\n\nAssets referencing this ID will display <Missing> until reassigned.";

    if (EditorUtility.DisplayDialog("Delete ID", message, "Delete", "Cancel"))
        _entriesProp.DeleteArrayElementAtIndex(index);
}
```

Delete the `GetStructType()` method (lines 284–288 of the current file) and remove the now-unused `using System;` if no other use remains — check the rest of the file before removing.

- [ ] **Step 2.3: Verify no remaining references**

Run:

```bash
grep -rn "StringIdUsageScanner" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty output.

- [ ] **Step 2.4: Verify in Unity**

Unity recompiles cleanly. Open a `StringIdRegistry`, click the × button on an entry — the new simplified dialog appears. Confirming still deletes the entry.

- [ ] **Step 2.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
git commit -m "Remove StringIdUsageScanner and its delete-dialog usage count"
```

---

## Phase B — Runtime fixes

### Task 3: `StringIdRegistry` — `sealed` + dictionary caches

Fixes bugs (5) and (6). Adds O(1) lookups.

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/StringIdRegistry.cs`

- [ ] **Step 3.1: Rewrite `StringIdRegistry.cs`**

Replace the entire file body inside the namespace with:

```csharp
#nullable enable
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    /// <summary>
    /// A ScriptableObject that maps string names to stable integer IDs for a given struct type.
    /// Used by the <c>IdStruct</c> system to persist and resolve string/int ID pairs.
    /// </summary>
    [CreateAssetMenu(fileName = "StringIdRegistry", menuName = "Aspid/FastTools/String Id Registry")]
    public sealed partial class StringIdRegistry : ScriptableObject, IEnumerable<KeyValuePair<int, string>>
    {
        [SerializeField] private IdEntry[] _entries = Array.Empty<IdEntry>();

        [NonSerialized] private Dictionary<string, int>? _idByName;
        [NonSerialized] private Dictionary<int, string>? _nameById;
        [NonSerialized] private bool _cacheDirty = true;

        public IEnumerable<int> Ids =>
            this.Select(entry => entry.Key);

        public IEnumerable<string> IdNames =>
            this.Select(entry => entry.Value);

        public int GetId(string nameId)
        {
            EnsureCache();
            return _idByName!.TryGetValue(nameId, out var id) ? id : -1;
        }

        public string? GetNameId(int id)
        {
            EnsureCache();
            return _nameById!.TryGetValue(id, out var name) ? name : null;
        }

        public bool Contains(string nameId)
        {
            EnsureCache();
            return _idByName!.ContainsKey(nameId);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() =>
            _entries.Select(entry => new KeyValuePair<int, string>(entry.Id, entry.Name)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private void EnsureCache()
        {
            if (!_cacheDirty && _idByName != null && _nameById != null) return;

            _idByName = new Dictionary<string, int>(_entries.Length);
            _nameById = new Dictionary<int, string>(_entries.Length);
            foreach (var entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Name))
                    _idByName[entry.Name] = entry.Id;
                _nameById[entry.Id] = entry.Name ?? string.Empty;
            }

            _cacheDirty = false;
        }

        internal void InvalidateCache() => _cacheDirty = true;

#if UNITY_EDITOR
        private void OnValidate() => _cacheDirty = true;
#endif

        /// <summary>
        /// A single name-to-id mapping entry.
        /// </summary>
        [Serializable]
        private struct IdEntry
        {
            public int Id;
            public string Name;
        }
    }
}
```

Important: `sealed partial class` — `StringIdRegistry.Editor.cs` is a separate partial file and remains unchanged in Task 3.

- [ ] **Step 3.2: Verify**

Unity recompiles. Open an existing `StringIdRegistry` with ≥5 entries. Use `GetId(name)` / `GetNameId(id)` from any consuming code (or the dropdown in `IdStructDrawer` — the dropdown fetches names via `GetNameId` through `SyncStringFromInt`). Switching selected entry must update correctly.

Additional smoke test: create a new `StringIdRegistry`, add an entry "Foo", verify `Contains("Foo")` returns true via the Add-row validation (the + button correctly disables when typing "Foo" again).

- [ ] **Step 3.3: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/StringIdRegistry.cs
git commit -m "Seal StringIdRegistry and add dictionary caches for O(1) lookups"
```

---

### Task 4: `IdRegistry` runtime rewrite + editor partial

Fixes bugs (12) and (13). Separates runtime (int-only) from editor metadata (names, target-type, next-id).

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/IdRegistry.cs`
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/IdRegistry.Editor.cs`
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/IdRegistry.Editor.cs.meta` — Unity creates this on reimport

- [ ] **Step 4.1: Rewrite `IdRegistry.cs`**

Replace the entire file:

```csharp
#nullable enable
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    /// <summary>
    /// A ScriptableObject that holds a stable set of integer IDs for a given struct type.
    /// Names are stored and edited in the inspector but stripped from player builds.
    /// Use <see cref="Aspid.FastTools.StringIdRegistry"/> when name lookups are needed at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/FastTools/Id Registry")]
    public sealed partial class IdRegistry : ScriptableObject, IEnumerable<int>
    {
        [SerializeField] private int[] _ids = Array.Empty<int>();

        public int Count => _ids.Length;

        public bool Contains(int id)
        {
            for (var i = 0; i < _ids.Length; i++)
                if (_ids[i] == id) return true;
            return false;
        }

        public IEnumerator<int> GetEnumerator() =>
            ((IEnumerable<int>)_ids).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
```

- [ ] **Step 4.2: Create `IdRegistry.Editor.cs`**

New file:

```csharp
#if UNITY_EDITOR
using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public sealed partial class IdRegistry
    {
        [SerializeField] private string[] _names = Array.Empty<string>();

        [TypeSelector(typeof(IId))]
        [SerializeField] private string _targetStructType = string.Empty;

        [SerializeField] private int _nextId = 1;
    }
}
#endif
```

- [ ] **Step 4.3: Grep for in-the-wild `IdRegistry` assets**

```bash
grep -rln "IdRegistry" Aspid.FastTools/Assets --include="*.asset" | head -20
```

If any lines reference `m_Script: {fileID: ..., guid: <IdRegistry-guid>}`, those assets held the old `_entries` shape. Since the current `IdRegistry` stub has no usages (the menu item was introduced recently and the folder shows no authored `IdRegistry` assets in the repo), the expected output is empty. If any are found, stop this task and inform the user.

- [ ] **Step 4.4: Verify in Unity**

Unity recompiles. Create a new `IdRegistry` asset via `Assets → Create → Aspid → FastTools → Id Registry`. The asset appears with the default fields. It will render with the **default** inspector (no custom editor yet — we add it in Task 12). That default rendering shows `_ids`, `_names`, `_targetStructType`, `_nextId` — that's expected for now.

- [ ] **Step 4.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/IdRegistry.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Runtime/Ids/IdRegistry.Editor.cs
git commit -m "Split IdRegistry into int-only runtime + editor-only names partial"
```

---

## Phase C — Resolver and renames

### Task 5: Introduce `IdRegistryResolver` (replaces `StringIdRegistryHelper`)

Fixes bugs (1), (2), and introduces the dual-type lookup with uniqueness check. This task touches every drawer call site that currently references `StringIdRegistryHelper`.

**Files:**
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolver.cs`
- Delete: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryHelper.cs` (+`.meta`)
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs`

- [ ] **Step 5.1: Create `IdRegistryResolver.cs`**

New file:

```csharp
#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Ids;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Finds and creates Id registry assets bound to a given IdStruct type.
    /// Searches both <see cref="IdRegistry"/> and <see cref="StringIdRegistry"/>,
    /// enforcing one-registry-per-type at lookup time.
    /// </summary>
    internal static class IdRegistryResolver
    {
        private const string TargetStructTypeField = "_targetStructType";

        private static readonly Dictionary<string, ScriptableObject?> _cache = new();

        internal static void ClearCache() => _cache.Clear();

        public static ScriptableObject? Find(Type? declaringType)
        {
            if (declaringType == null) return null;

            var aqn = declaringType.AssemblyQualifiedName ?? string.Empty;
            if (_cache.TryGetValue(aqn, out var cached))
                return cached;

            ScriptableObject? first = null;
            List<string>? extraPaths = null;

            foreach (var path in EnumerateRegistryPaths())
            {
                var registry = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (registry == null) continue;

                var stored = ReadTargetStructType(registry);
                if (stored != aqn) continue;

                if (first == null)
                {
                    first = registry;
                    continue;
                }

                extraPaths ??= new List<string> { AssetDatabase.GetAssetPath(first) };
                extraPaths.Add(path);
            }

            if (extraPaths != null)
            {
                Debug.LogError(
                    $"Multiple registries found for type {declaringType.Name}: "
                    + string.Join(", ", extraPaths)
                    + ". Each IdStruct type must be bound to exactly one registry.");
            }

            _cache[aqn] = first;
            return first;
        }

        public static IdRegistry? FindIntOnly(Type? declaringType) =>
            Find(declaringType) as IdRegistry;

        public static StringIdRegistry? FindStringMapped(Type? declaringType) =>
            Find(declaringType) as StringIdRegistry;

        public static StringIdRegistry CreateStringMapped(Type declaringType)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));

            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/StringIdRegistry_{declaringType.Name}.asset");
            var reg = ScriptableObject.CreateInstance<StringIdRegistry>();
            AssetDatabase.CreateAsset(reg, path);

            var so = new SerializedObject(reg);
            so.FindProperty(TargetStructTypeField).stringValue = declaringType.AssemblyQualifiedName ?? string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            _cache[declaringType.AssemblyQualifiedName ?? string.Empty] = reg;
            return reg;
        }

        private static IEnumerable<string> EnumerateRegistryPaths()
        {
            var guids = AssetDatabase.FindAssets("t:IdRegistry t:StringIdRegistry");
            for (var i = 0; i < guids.Length; i++)
                yield return AssetDatabase.GUIDToAssetPath(guids[i]);
        }

        private static string ReadTargetStructType(ScriptableObject registry)
        {
            var so = new SerializedObject(registry);
            var prop = so.FindProperty(TargetStructTypeField);
            return prop != null ? prop.stringValue : string.Empty;
        }
    }
}
```

Key differences from the old `StringIdRegistryHelper`:
- Filter `"t:IdRegistry t:StringIdRegistry"` — correct types (bug 1 fix).
- Reads `_targetStructType` via `SerializedObject.FindProperty`, no reflection (bug 2 fix).
- Enforces uniqueness: multiple matches → `LogError` + return first.

- [ ] **Step 5.2: Update `IdStructDrawer.cs` call sites**

The drawer has 8 references to `StringIdRegistryHelper` (`FindRegistry` × 7, `CreateRegistry` × 1). Replace via a single search-and-replace:

- `StringIdRegistryHelper.FindRegistry(fieldType)` → `IdRegistryResolver.FindStringMapped(fieldType)`
- `StringIdRegistryHelper.CreateRegistry(fieldType)` → `IdRegistryResolver.CreateStringMapped(fieldType)`

Do it with sed or manual replace:

```bash
sed -i '' \
  -e 's/StringIdRegistryHelper\.FindRegistry/IdRegistryResolver.FindStringMapped/g' \
  -e 's/StringIdRegistryHelper\.CreateRegistry/IdRegistryResolver.CreateStringMapped/g' \
  Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs
```

- [ ] **Step 5.3: Update `StringIdRegistryCacheInvalidator.cs`**

Change the `ClearCache` call from `StringIdRegistryHelper.ClearCache()` to `IdRegistryResolver.ClearCache()`:

```csharp
// was:
StringIdRegistryHelper.ClearCache();
// now:
IdRegistryResolver.ClearCache();
```

- [ ] **Step 5.4: Delete `StringIdRegistryHelper.cs`**

```bash
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryHelper.cs
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryHelper.cs.meta
```

- [ ] **Step 5.5: Verify no remaining references**

```bash
grep -rn "StringIdRegistryHelper" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty.

- [ ] **Step 5.6: Verify in Unity**

Recompile. On any ScriptableObject / MonoBehaviour with an `IdStruct` field, open Inspector. The dropdown should still work — clicking it opens `StringIdSelectorWindow` with the correct entries. The bug (1) scenario: before the fix, the `"t:IdRegistry"` query succeeded only because the stub was identically-named; after Task 4 renamed the struct shape, this would have broken — now the new resolver searches both types correctly.

Create a second `StringIdRegistry` in a fresh path and set its `_targetStructType` to the same AQN as an existing registry (via Debug Inspector). Open the IdStruct drawer — Console shows the new `LogError` listing both paths.

- [ ] **Step 5.7: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolver.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs
git commit -m "Replace StringIdRegistryHelper with IdRegistryResolver (dual-type search, no reflection)"
```

---

### Task 6: Rename `StringIdRegistryCacheInvalidator`

Pure mechanical rename. Kept separate so the diff is small and reviewable.

**Files:**
- Rename: `StringIdRegistryCacheInvalidator.cs` → `IdRegistryResolverCacheInvalidator.cs` (+`.meta`)

- [ ] **Step 6.1: `git mv`**

```bash
git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolverCacheInvalidator.cs

git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryCacheInvalidator.cs.meta \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolverCacheInvalidator.cs.meta
```

- [ ] **Step 6.2: Update class name**

In `IdRegistryResolverCacheInvalidator.cs`:

```csharp
// was:
internal sealed class StringIdRegistryCacheInvalidator : AssetPostprocessor
// now:
internal sealed class IdRegistryResolverCacheInvalidator : AssetPostprocessor
```

- [ ] **Step 6.3: Verify**

```bash
grep -rn "StringIdRegistryCacheInvalidator" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty.

Unity recompiles. Asset-postprocessor still fires (add a new `.asset` file — Console should show no errors).

- [ ] **Step 6.4: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolverCacheInvalidator.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryResolverCacheInvalidator.cs.meta
git commit -m "Rename StringIdRegistryCacheInvalidator to IdRegistryResolverCacheInvalidator"
```

---

### Task 7: Rename + extend `StringIdRegistryValidator` (add `IsValidName`)

Mechanical rename + adds the C#-identifier validation used later by Task 14.

**Files:**
- Rename: `StringIdRegistryValidator.cs` → `IdRegistryValidator.cs` (+`.meta`)
- Modify (same file, after rename): add `IsValidName` and the reserved-keyword set
- Modify callers: `StringIdRegistryEditor.cs` (references `StringIdRegistryValidator` at 3 call sites)

- [ ] **Step 7.1: `git mv`**

```bash
git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryValidator.cs \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs

git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryValidator.cs.meta \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs.meta
```

- [ ] **Step 7.2: Rewrite `IdRegistryValidator.cs`**

Replace the file body with:

```csharp
#nullable enable
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class IdRegistryValidator
    {
        private const int MaxNameLength = 255;

        private static readonly Regex IdentifierPattern =
            new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private static readonly HashSet<string> ReservedKeywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
            "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while"
        };

        /// <summary>
        /// Validates a candidate id name. Rules, in order: not whitespace,
        /// valid C# identifier, not a reserved keyword, length ≤ 255,
        /// not in the optional <paramref name="existing"/> set.
        /// </summary>
        public static bool IsValidName(string? input, HashSet<string>? existing, out string? error)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Name cannot be empty.";
                return false;
            }

            if (!IdentifierPattern.IsMatch(input))
            {
                error = "Name must be a valid C# identifier (letters, digits, underscore; cannot start with a digit).";
                return false;
            }

            if (ReservedKeywords.Contains(input))
            {
                error = $"'{input}' is a reserved C# keyword.";
                return false;
            }

            if (input.Length > MaxNameLength)
            {
                error = $"Name is too long (max {MaxNameLength} chars).";
                return false;
            }

            if (existing != null && existing.Contains(input))
            {
                error = $"'{input}' already exists.";
                return false;
            }

            error = null;
            return true;
        }

        public static HashSet<string> GetDuplicates(SerializedProperty entriesProp)
        {
            var seen  = new HashSet<string>();
            var dupes = new HashSet<string>();

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var val = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (!string.IsNullOrEmpty(val) && !seen.Add(val))
                    dupes.Add(val);
            }

            return dupes;
        }

        public static bool HasDuplicate(StringIdRegistry registry, string entryName)
        {
            var seen = false;
            foreach (var name in registry.IdNames)
            {
                if (name != entryName) continue;
                if (seen) return true;
                seen = true;
            }
            return false;
        }

        // CleanUpInvalid stays for now — removed in Task 16 when the explicit clean-up row replaces it.
        public static void CleanUpInvalid(Object target)
        {
            var so       = new SerializedObject(target);
            var entries  = so.FindProperty("_entries");
            if (entries == null) return;

            var seen     = new HashSet<string>();
            var toRemove = new List<int>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                var val = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (string.IsNullOrEmpty(val) || !seen.Add(val))
                    toRemove.Add(i);
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
                entries.DeleteArrayElementAtIndex(toRemove[i]);

            if (toRemove.Count > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
```

- [ ] **Step 7.3: Update callers in `StringIdRegistryEditor.cs`**

Three call sites + one using. Search-replace:

```bash
sed -i '' 's/StringIdRegistryValidator/IdRegistryValidator/g' \
  Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
```

- [ ] **Step 7.4: Verify**

```bash
grep -rn "StringIdRegistryValidator" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty.

Unity recompiles. Inspector still shows duplicate highlighting when two entries share a name.

- [ ] **Step 7.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs.meta \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
git commit -m "Rename validator to IdRegistryValidator and add C# identifier validation"
```

---

### Task 8: Rename `StringIdRegistryEntryVisualElement`

**Files:**
- Rename: `StringIdRegistryEntryVisualElement.cs` → `IdRegistryEntryVisualElement.cs` (+`.meta`)
- Modify callers: `StringIdRegistryEditor.cs`

- [ ] **Step 8.1: `git mv`**

```bash
git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEntryVisualElement.cs \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEntryVisualElement.cs

git mv Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEntryVisualElement.cs.meta \
       Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEntryVisualElement.cs.meta
```

- [ ] **Step 8.2: Rename class and data struct**

In `IdRegistryEntryVisualElement.cs`:

```bash
sed -i '' 's/StringIdRegistryEntryData/IdRegistryEntryData/g; s/StringIdRegistryEntryVisualElement/IdRegistryEntryVisualElement/g' \
  Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEntryVisualElement.cs
```

In `StringIdRegistryEditor.cs`:

```bash
sed -i '' 's/StringIdRegistryEntryData/IdRegistryEntryData/g; s/StringIdRegistryEntryVisualElement/IdRegistryEntryVisualElement/g' \
  Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
```

- [ ] **Step 8.3: Verify**

```bash
grep -rn "StringIdRegistryEntry" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty.

Unity recompiles. Inspector renders rows identically to before.

- [ ] **Step 8.4: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEntryVisualElement.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEntryVisualElement.cs.meta \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
git commit -m "Rename entry visual element and data struct to IdRegistry*"
```

---

## Phase D — Accessor + Core

This is the architectural centerpiece. Three new files (`IRegistryAccessor`, `IdRegistryAccessor`, `StringIdRegistryAccessor`), one large core (`RegistryEditorCore`), one new thin editor (`IdRegistryEditor`), and a rewrite of `StringIdRegistryEditor` to delegate to Core. `IdRegisterEditorExtensions.cs` is deleted at the end once accessors replace it.

### Task 9: `IRegistryAccessor` + two implementations

**Files:**
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IRegistryAccessor.cs`
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryAccessor.cs`
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryAccessor.cs`

Core contracts — each new file is self-contained.

- [ ] **Step 9.1: Create `IRegistryAccessor.cs`**

```csharp
#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Adapter over the two registry types' storage layouts.
    /// Core inspector code talks to this interface, never to the concrete registry.
    /// </summary>
    internal interface IRegistryAccessor
    {
        Object Target { get; }
        SerializedObject SerializedObject { get; }
        SerializedProperty TargetStructTypeProperty { get; }
        SerializedProperty NextIdProperty { get; }

        int Count { get; }
        int GetId(int index);
        string GetName(int index);

        /// <summary>
        /// Returns the new Id assigned to <paramref name="name"/> (and bumps NextId).
        /// Caller is responsible for Undo and dirty-marking via <see cref="Record"/>.
        /// </summary>
        int Add(string name);

        /// <summary>
        /// Renames the entry at <paramref name="index"/>.
        /// </summary>
        void SetName(int index, string name);

        /// <summary>
        /// Removes the entry at <paramref name="index"/>. For IdRegistry this removes
        /// from both _ids and _names atomically.
        /// </summary>
        void RemoveAt(int index);

        bool Contains(string name);

        /// <summary>Largest Id currently assigned, or 0 if the registry is empty.</summary>
        int MaxAssignedId { get; }

        /// <summary>Registers an Undo group covering the upcoming mutation.</summary>
        void Record(string operationName);

        /// <summary>Applies pending SerializedObject edits and dirties the asset.</summary>
        void Commit();

        /// <summary>True when storage is in an inconsistent state (e.g. parallel arrays diverged).</summary>
        bool HasStructuralDamage(out string reason);

        /// <summary>Enumerates duplicates and empty-name entries for the Clean-up flow.</summary>
        IEnumerable<int> EnumerateInvalidIndices();
    }
}
```

- [ ] **Step 9.2: Create `StringIdRegistryAccessor.cs`**

```csharp
#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class StringIdRegistryAccessor : IRegistryAccessor
    {
        private readonly StringIdRegistry _registry;
        private readonly SerializedProperty _entriesProp;

        public Object Target => _registry;
        public SerializedObject SerializedObject { get; }
        public SerializedProperty TargetStructTypeProperty { get; }
        public SerializedProperty NextIdProperty { get; }

        public StringIdRegistryAccessor(StringIdRegistry registry)
        {
            _registry = registry;
            SerializedObject = new SerializedObject(registry);
            _entriesProp = SerializedObject.FindProperty("_entries");
            TargetStructTypeProperty = SerializedObject.FindProperty("_targetStructType");
            NextIdProperty = SerializedObject.FindProperty("_nextId");
        }

        public int Count => _entriesProp.arraySize;

        public int GetId(int index) =>
            _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Id").intValue;

        public string GetName(int index) =>
            _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name").stringValue;

        public int Add(string name)
        {
            var id = NextIdProperty.intValue;
            NextIdProperty.intValue = id + 1;

            var newIndex = _entriesProp.arraySize;
            _entriesProp.arraySize = newIndex + 1;
            var element = _entriesProp.GetArrayElementAtIndex(newIndex);
            element.FindPropertyRelative("Id").intValue = id;
            element.FindPropertyRelative("Name").stringValue = name;
            return id;
        }

        public void SetName(int index, string name) =>
            _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name").stringValue = name;

        public void RemoveAt(int index) =>
            _entriesProp.DeleteArrayElementAtIndex(index);

        public bool Contains(string name)
        {
            for (var i = 0; i < Count; i++)
                if (GetName(i) == name) return true;
            return false;
        }

        public int MaxAssignedId
        {
            get
            {
                var max = 0;
                for (var i = 0; i < Count; i++)
                {
                    var id = GetId(i);
                    if (id > max) max = id;
                }
                return max;
            }
        }

        public void Record(string operationName) =>
            Undo.RegisterCompleteObjectUndo(_registry, operationName);

        public void Commit()
        {
            SerializedObject.ApplyModifiedProperties();
            _registry.InvalidateCache();
            EditorUtility.SetDirty(_registry);
        }

        public bool HasStructuralDamage(out string reason)
        {
            reason = string.Empty;
            return false; // StringIdRegistry has a single-property storage, always consistent.
        }

        public IEnumerable<int> EnumerateInvalidIndices()
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < Count; i++)
            {
                var name = GetName(i);
                if (string.IsNullOrEmpty(name))
                {
                    yield return i;
                    continue;
                }
                if (!seen.Add(name))
                    yield return i;
            }
        }
    }
}
```

- [ ] **Step 9.3: Create `IdRegistryAccessor.cs`**

```csharp
#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Ids;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class IdRegistryAccessor : IRegistryAccessor
    {
        private readonly IdRegistry _registry;
        private readonly SerializedProperty _idsProp;
        private readonly SerializedProperty _namesProp;

        public Object Target => _registry;
        public SerializedObject SerializedObject { get; }
        public SerializedProperty TargetStructTypeProperty { get; }
        public SerializedProperty NextIdProperty { get; }

        public IdRegistryAccessor(IdRegistry registry)
        {
            _registry = registry;
            SerializedObject = new SerializedObject(registry);
            _idsProp = SerializedObject.FindProperty("_ids");
            _namesProp = SerializedObject.FindProperty("_names");
            TargetStructTypeProperty = SerializedObject.FindProperty("_targetStructType");
            NextIdProperty = SerializedObject.FindProperty("_nextId");
        }

        public int Count => Mathf.Min(_idsProp.arraySize, _namesProp.arraySize);

        public int GetId(int index) =>
            _idsProp.GetArrayElementAtIndex(index).intValue;

        public string GetName(int index) =>
            _namesProp.GetArrayElementAtIndex(index).stringValue;

        public int Add(string name)
        {
            var id = NextIdProperty.intValue;
            NextIdProperty.intValue = id + 1;

            var newIndex = _idsProp.arraySize;
            _idsProp.arraySize = newIndex + 1;
            _namesProp.arraySize = newIndex + 1;
            _idsProp.GetArrayElementAtIndex(newIndex).intValue = id;
            _namesProp.GetArrayElementAtIndex(newIndex).stringValue = name;
            return id;
        }

        public void SetName(int index, string name) =>
            _namesProp.GetArrayElementAtIndex(index).stringValue = name;

        public void RemoveAt(int index)
        {
            _idsProp.DeleteArrayElementAtIndex(index);
            if (index < _namesProp.arraySize)
                _namesProp.DeleteArrayElementAtIndex(index);
        }

        public bool Contains(string name)
        {
            for (var i = 0; i < Count; i++)
                if (GetName(i) == name) return true;
            return false;
        }

        public int MaxAssignedId
        {
            get
            {
                var max = 0;
                for (var i = 0; i < Count; i++)
                {
                    var id = GetId(i);
                    if (id > max) max = id;
                }
                return max;
            }
        }

        public void Record(string operationName) =>
            Undo.RegisterCompleteObjectUndo(_registry, operationName);

        public void Commit()
        {
            SerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
        }

        public bool HasStructuralDamage(out string reason)
        {
            if (_idsProp.arraySize == _namesProp.arraySize)
            {
                reason = string.Empty;
                return false;
            }
            reason = $"Length mismatch: _ids has {_idsProp.arraySize} entries, _names has {_namesProp.arraySize}.";
            return true;
        }

        public IEnumerable<int> EnumerateInvalidIndices()
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < Count; i++)
            {
                var name = GetName(i);
                if (string.IsNullOrEmpty(name))
                {
                    yield return i;
                    continue;
                }
                if (!seen.Add(name))
                    yield return i;
            }
        }
    }
}
```

- [ ] **Step 9.4: Verify compile**

Unity recompiles. No call sites yet consume the new accessors, so verification is just "Console clean".

- [ ] **Step 9.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IRegistryAccessor.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryAccessor.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryAccessor.cs
git commit -m "Introduce IRegistryAccessor and two concrete implementations"
```

---

### Task 10: `RegistryEditorCore` skeleton (replicates current UI)

This task produces a `RegistryEditorCore` that builds the same UI the current `StringIdRegistryEditor` builds, but sourced from an `IRegistryAccessor`. No new features — just relocation. UX additions (validation, Undo, clean-up row, sort/group, Next-Id section) come in Tasks 14–18.

**Files:**
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs`

- [ ] **Step 10.1: Create `RegistryEditorCore.cs`**

```csharp
#nullable enable
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Builds the shared inspector UI for both Id registry types.
    /// UI-only — all storage access goes through <see cref="IRegistryAccessor"/>.
    /// </summary>
    internal sealed class RegistryEditorCore
    {
        private readonly IRegistryAccessor _accessor;

        private readonly List<EntryView> _viewModel = new();
        private Label? _emptyLabel;
        private ListView? _listView;
        private string _searchQuery = string.Empty;

        public RegistryEditorCore(IRegistryAccessor accessor)
        {
            _accessor = accessor;
        }

        public VisualElement Build()
        {
            var root = new VisualElement()
                .AddStyleSheetsFromResource(Constants.Registry.StyleSheetPath)
                .AddStyleSheetsFromResource(StyleClasses.DefaultStyleSheet)
                .AddClass(Constants.Registry.Root)
                .AddClass("aspid-fasttools-inspector-container");

            root.Add(new AspidInspectorHeader(_accessor.Target.name, _accessor.Target)
            {
                Subtext = _accessor.Target.GetType().Name,
            });

            var typeContainer = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-dark")
                .AddClass("aspid-fasttools-background");

            typeContainer.Add(new AspidLabel("Type").SetMarginBottom(5));
            typeContainer.Add(new PropertyField(_accessor.TargetStructTypeProperty, label: string.Empty));

            var container = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-light")
                .AddClass("aspid-fasttools-background");

            container.Add(BuildSectionTitle("IDs"));

            var searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(e =>
            {
                _searchQuery = e.newValue ?? string.Empty;
                RebuildEntries();
            });
            container.Add(searchField);

            _listView = new ListView
            {
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = _viewModel,
                reorderable = false,
                showBorder = false,
                showFoldoutHeader = false,
                showBoundCollectionSize = false,
                showAddRemoveFooter = false,
            };
            _listView.AddClass(Constants.Registry.List);
            _listView.SetMakeItem(CreateEntryRow);
            _listView.SetBindItem(BindEntryRow);
            container.Add(_listView);

            _emptyLabel = new Label("No IDs yet. Add one below.")
                .AddClass(Constants.Registry.Empty);
            container.Add(_emptyLabel);
            container.Add(BuildAddRow());

            container.TrackSerializedObjectValue(_accessor.SerializedObject, _ => RebuildEntries());
            RebuildEntries();

            return root
                .AddChild(typeContainer)
                .AddChild(container);
        }

        private static VisualElement BuildSectionTitle(string text) =>
            new AspidLabel(text, new LabelPreset()
                .SetTheme(ThemeStyle.Light)
                .SetLabelSize(AspidLabelSizeStyle.H2)
                .SetLineSize(DividingLineSize.Medium));

        private void RebuildEntries()
        {
            _viewModel.Clear();
            var count = _accessor.Count;

            if (_emptyLabel != null)
                _emptyLabel.EnableInClassList(Constants.Registry.EmptyVisible, count == 0);

            var duplicates = new HashSet<string>();
            var seen = new HashSet<string>();
            for (var i = 0; i < count; i++)
            {
                var name = _accessor.GetName(i);
                if (!string.IsNullOrEmpty(name) && !seen.Add(name))
                    duplicates.Add(name);
            }

            var query = _searchQuery?.Trim() ?? string.Empty;
            for (var i = 0; i < count; i++)
            {
                var name = _accessor.GetName(i);
                var id = _accessor.GetId(i);
                if (!MatchesQuery(name, id, query)) continue;

                _viewModel.Add(new EntryView(i, name, id, duplicates.Contains(name)));
            }

            _listView?.Rebuild();
            UpdateListScrollState();
        }

        private void UpdateListScrollState()
        {
            if (_listView == null) return;

            if (_viewModel.Count >= Constants.Registry.ScrollThreshold)
            {
                const float height = Constants.Registry.MaxVisibleRows * Constants.Registry.RowHeight;
                _listView.AddToClassList(Constants.Registry.ListScrollable);
                _listView.style.height = height;
                _listView.style.maxHeight = height;
            }
            else
            {
                _listView.RemoveFromClassList(Constants.Registry.ListScrollable);
                _listView.style.height = StyleKeyword.Null;
                _listView.style.maxHeight = StyleKeyword.Null;
            }
        }

        private static bool MatchesQuery(string name, int id, string query)
        {
            if (string.IsNullOrEmpty(query)) return true;
            if (!string.IsNullOrEmpty(name) && name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return id.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private VisualElement CreateEntryRow()
        {
            var row = new IdRegistryEntryVisualElement();
            row.NameFocusIn += OnRowNameFocusIn;
            row.NameChanging += OnRowNameChanging;
            row.NameCommitRequested += OnRowNameCommitRequested;
            row.DeleteRequested += OnRowDeleteRequested;
            return row;
        }

        private void BindEntryRow(VisualElement element, int visibleIndex)
        {
            if (visibleIndex < 0 || visibleIndex >= _viewModel.Count) return;
            var view = _viewModel[visibleIndex];
            ((IdRegistryEntryVisualElement)element).Bind(new IdRegistryEntryData(
                originalIndex: view.OriginalIndex,
                name: view.Name,
                id: view.Id,
                isDuplicate: view.IsDuplicate));
        }

        private void OnRowNameFocusIn(IdRegistryEntryVisualElement row, IdRegistryEntryData data)
        {
            if (data.IsDuplicate)
                row.SetError("Name already exists.");
        }

        private void OnRowNameChanging(IdRegistryEntryVisualElement row, IdRegistryEntryData data, string newValue)
        {
            var trimmed = newValue?.Trim() ?? string.Empty;

            if (trimmed == data.Name)
            {
                row.SetEditMode(false);
                row.ClearError();
                return;
            }

            var existing = CollectExistingNames(exceptIndex: data.OriginalIndex);
            if (IdRegistryValidator.IsValidName(trimmed, existing, out var error))
            {
                row.SetEditMode(true, canConfirm: true);
                row.ClearError();
            }
            else
            {
                row.SetEditMode(true, canConfirm: false);
                row.SetError(error!);
            }
        }

        private void OnRowNameCommitRequested(IdRegistryEntryVisualElement row, IdRegistryEntryData data, string rawValue)
        {
            var trimmed = rawValue?.Trim() ?? string.Empty;
            if (trimmed == data.Name || string.IsNullOrEmpty(trimmed)) return;

            var existing = CollectExistingNames(exceptIndex: data.OriginalIndex);
            if (!IdRegistryValidator.IsValidName(trimmed, existing, out _)) return;

            _accessor.Record($"Rename ID '{data.Name}' → '{trimmed}'");
            _accessor.SetName(data.OriginalIndex, trimmed);
            _accessor.Commit();
            row.SetEditMode(false);
            row.ClearError();
        }

        private void OnRowDeleteRequested(IdRegistryEntryVisualElement row, IdRegistryEntryData data)
        {
            var name = data.Name;
            if (!EditorUtility.DisplayDialog(
                    "Delete ID",
                    $"Delete '{name}'?\n\nAssets referencing this ID will display <Missing> until reassigned.",
                    "Delete",
                    "Cancel"))
                return;

            _accessor.Record($"Delete ID '{name}'");
            _accessor.RemoveAt(data.OriginalIndex);
            _accessor.Commit();
        }

        private VisualElement BuildAddRow()
        {
            var row = new VisualElement().AddClass(Constants.Registry.AddRow);
            var inputField = new TextField();
            inputField.AddClass(Constants.Registry.AddInput);

            var addButton = new Button { text = "+" };
            addButton.AddClass(Constants.Registry.AddButton);
            addButton.SetEnabled(false);

            inputField.RegisterValueChangedCallback(e =>
            {
                var val = e.newValue?.Trim() ?? string.Empty;
                var existing = CollectExistingNames(exceptIndex: -1);
                var ok = IdRegistryValidator.IsValidName(val, existing, out _);
                addButton.SetEnabled(ok);
            });

            addButton.clicked += () =>
            {
                var val = inputField.value?.Trim();
                if (string.IsNullOrEmpty(val)) return;

                _accessor.Record($"Add ID '{val}'");
                _accessor.Add(val);
                _accessor.Commit();

                inputField.SetValueWithoutNotify(string.Empty);
                addButton.SetEnabled(false);

                RebuildEntries();
                var newIndex = _viewModel.FindIndex(v => v.Name == val);
                if (newIndex < 0 || _listView == null) return;
                _listView.schedule.Execute(() => _listView.ScrollToItem(newIndex)).StartingIn(0);
            };

            row.Add(inputField);
            row.Add(addButton);
            return row;
        }

        private HashSet<string> CollectExistingNames(int exceptIndex)
        {
            var set = new HashSet<string>();
            var count = _accessor.Count;
            for (var i = 0; i < count; i++)
            {
                if (i == exceptIndex) continue;
                var name = _accessor.GetName(i);
                if (!string.IsNullOrEmpty(name))
                    set.Add(name);
            }
            return set;
        }

        private readonly struct EntryView
        {
            public readonly int OriginalIndex;
            public readonly string Name;
            public readonly int Id;
            public readonly bool IsDuplicate;

            public EntryView(int originalIndex, string name, int id, bool isDuplicate)
            {
                OriginalIndex = originalIndex;
                Name = name;
                Id = id;
                IsDuplicate = isDuplicate;
            }
        }
    }
}
```

Notes:
- Name validation is already wired through `IdRegistryValidator.IsValidName` — this implements part of UX pack D up-front since it is simpler than adding it later.
- Undo/SetDirty are already wired through `accessor.Record` + `accessor.Commit` — this implements UX pack E up-front.
- `CleanUpInvalid` is not yet wired — removed-usage and Clean-up row arrive in Task 16.
- Next-Id section and Sort/Group arrive in Tasks 17–18.
- Core does not reference `IdRegisterEditorExtensions` — all mutations now go through `IRegistryAccessor.Add/SetName/RemoveAt/Record/Commit`.

- [ ] **Step 10.2: Verify compile**

Unity recompiles. `RegistryEditorCore` is not yet wired to any `CustomEditor`, so no UI changes yet.

- [ ] **Step 10.3: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs
git commit -m "Add RegistryEditorCore — storage-agnostic shared inspector UI"
```

---

### Task 11: Rewire `StringIdRegistryEditor` through Core

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs`

- [ ] **Step 11.1: Replace `StringIdRegistryEditor.cs` with a thin delegate**

Full file replacement:

```csharp
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    [CustomEditor(typeof(StringIdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var accessor = new StringIdRegistryAccessor((StringIdRegistry)target);
            return new RegistryEditorCore(accessor).Build();
        }
    }
}
```

Everything that previously lived in this file (viewmodel list, rebuild, row callbacks, dialog, `TryDeleteEntry`, `GetStructType`) now lives in `RegistryEditorCore` via the accessor.

Previously on `OnDisable`, `CleanUpInvalid` was called — we drop that call entirely here; Task 16 introduces the explicit Clean-up row.

- [ ] **Step 11.2: Verify in Unity**

Recompile. Open an existing `StringIdRegistry` asset in Inspector. Expected observations:
1. Header renders (name + typename).
2. Type selector field renders.
3. Search field + list + empty label + Add row all render.
4. Add a new entry — it appears in the list, asset is marked dirty, Ctrl+Z reverts it.
5. Rename an entry — click name → type new value → ✓ commits; Ctrl+Z reverts.
6. Delete an entry — × shows dialog; Delete confirms; Ctrl+Z reverts.
7. Enter an invalid name (e.g. `123abc` or `if`) — the Add button stays disabled; editing an existing name to that value shows a red error message.

- [ ] **Step 11.3: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/StringIdRegistryEditor.cs
git commit -m "Rewire StringIdRegistryEditor to delegate to RegistryEditorCore"
```

---

### Task 12: Add `IdRegistryEditor` (thin delegate)

**Files:**
- Create: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEditor.cs`

- [ ] **Step 12.1: Create `IdRegistryEditor.cs`**

```csharp
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.Ids;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    [CustomEditor(typeof(IdRegistry))]
    internal sealed class IdRegistryEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var accessor = new IdRegistryAccessor((IdRegistry)target);
            return new RegistryEditorCore(accessor).Build();
        }
    }
}
```

- [ ] **Step 12.2: Verify in Unity**

Recompile. Create an `IdRegistry` via `Assets → Create → Aspid → FastTools → Id Registry`. Inspector now shows the shared Core UI. Add/rename/delete entries — mutations write to `_ids` AND `_names` atomically; Undo works.

- [ ] **Step 12.3: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryEditor.cs
git commit -m "Add IdRegistryEditor delegating to RegistryEditorCore"
```

---

### Task 13: Delete `IdRegisterEditorExtensions`

All call sites are now in accessors. This extension file is dead.

**Files:**
- Delete: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions/IdRegisterEditorExtensions.cs` (+`.meta`)

- [ ] **Step 13.1: Verify no references**

```bash
grep -rn "IdRegisterEditorExtensions\|registry\.Add\|registry\.Rename" Aspid.FastTools/Assets --include="*.cs"
```

The only legitimate matches should be inside the file itself and accessor `Add` methods. If you find calls like `(StringIdRegistry)target.Add(...)` or `registry.Rename(...)` — they're leftovers; migrate to accessor calls before deletion.

- [ ] **Step 13.2: Delete file**

```bash
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions/IdRegisterEditorExtensions.cs
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions/IdRegisterEditorExtensions.cs.meta
```

- [ ] **Step 13.3: Clean up empty folder**

If `Extensions/` folder is now empty, remove it and its `.meta`:

```bash
# Check folder is empty:
ls Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions/
# If empty (only .meta remains), remove:
rmdir Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions/
git rm Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Extensions.meta
```

But — before doing this, check if `IdStructDrawer` uses anything from the `Extensions` folder. If it does (it still wired `registry.Add(...)` via the old static extension before), those call sites must have already migrated to `accessor.Add(...)` earlier. If the IdStructDrawer still calls `registry.Add(...)` directly (which is a plausible hold-over since Task 5 only renamed the resolver), you have two options:
- Acceptable: `IdStructDrawer` calls `registry.Add(name)` directly — but `Add` on `StringIdRegistry` was an **extension method** in `IdRegisterEditorExtensions`. Removing the extension breaks the drawer.

So before Step 13.2, review `IdStructDrawer.cs`:

```bash
grep -n "registry\.Add\|reg\.Add\|reg2\.Add" Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs
```

If matches exist, **do Step 13.1a first** (sub-step):

- [ ] **Step 13.1a: Migrate `IdStructDrawer.Add` calls to accessor**

Replace any `registry.Add(name)` / `reg.Add(name)` / `reg2.Add(name)` call with:

```csharp
var accessor = new StringIdRegistryAccessor(registry);
accessor.Record($"Add ID '{name}'");
var assignedId = accessor.Add(name);
accessor.Commit();
```

Apply at both IMGUI and UIToolkit code paths (lines ~104 and ~189 of the current file after Task 5's renames).

- [ ] **Step 13.4: Verify**

Unity recompiles. Open a ScriptableObject with an `IdStruct` field, click Create → input a valid name → + — the registry gets the new entry, drawer shows it selected.

- [ ] **Step 13.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs
# If Extensions folder removed:
git add -A Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/
git commit -m "Delete IdRegisterEditorExtensions; migrate Add call sites to accessors"
```

---

## Phase E — UX additions

### Task 14: Name validation (D) — verify the validation pipeline is correct

Tasks 10 and 11 already wired `IdRegistryValidator.IsValidName` into `RegistryEditorCore`. This task is a verification pass plus any tuning.

- [ ] **Step 14.1: Walk through validation cases**

In Unity, open a `StringIdRegistry`. In the Add-row, type each value and watch the `+` button:

| Input | Expected |
|---|---|
| `` (empty) | Disabled |
| ` ` (whitespace) | Disabled |
| `123Foo` | Disabled (starts with digit) |
| `Foo Bar` | Disabled (space) |
| `Foo-Bar` | Disabled (dash) |
| `if` | Disabled (reserved keyword) |
| `Enemy_Goblin` | Enabled |
| `_Enemy` | Enabled |

Rename an existing entry to `if` — error message appears, confirm button disabled.

- [ ] **Step 14.2: Inline error message sanity**

In the Rename flow, type `Enemy Goblin` — red error `"Name must be a valid C# identifier..."` appears under the row. Change to `EnemyGoblin` — error clears, ✓ enabled.

- [ ] **Step 14.3: Commit (no-op if no code changes needed)**

If walkthrough passed without code tweaks, no commit needed. If any adjustments were made (e.g., a typo in the error messages), commit them:

```bash
git add <changed-files>
git commit -m "Tune IdRegistryValidator error messages"
```

---

### Task 15: Full Undo (E) — verify

Tasks 10 and 11 already route mutations through `accessor.Record → mutate → Commit`. This task is a verification pass.

- [ ] **Step 15.1: Exercise Undo for every operation**

In Unity, open a `StringIdRegistry` asset with a few entries. Perform and Ctrl+Z each:

1. Add `Foo` → Ctrl+Z → entry gone.
2. Rename `Foo` → `Bar` → Ctrl+Z → reverted to `Foo`.
3. Delete `Foo` → Ctrl+Z → entry back.

Check `Edit → Undo History` — operations labeled `Add ID 'Foo'`, `Rename ID 'Foo' → 'Bar'`, `Delete ID 'Foo'`.

Repeat on an `IdRegistry` (int-only) — same operations work; verify that delete keeps `_ids` and `_names` arrays in sync (via Debug Inspector on the registry asset).

- [ ] **Step 15.2: Commit (no-op if walkthrough passed)**

No code change expected. Skip commit.

---

### Task 16: Explicit Clean-up (F) — remove silent CleanUpInvalid, add warning row

Fixes bug (3) from spec §4. Replaces `OnDisable → CleanUpInvalid` with a visible affordance.

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs` (`CleanUpInvalid` removed, `BuildCleanUpSummary` added)
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs`

- [ ] **Step 16.1: Remove `CleanUpInvalid`, add summary helper**

In `IdRegistryValidator.cs`, replace the `CleanUpInvalid` method with:

```csharp
public readonly struct CleanUpSummary
{
    public readonly int EmptyCount;
    public readonly int DuplicateCount;
    public readonly int StructuralIssues;

    public CleanUpSummary(int emptyCount, int duplicateCount, int structuralIssues)
    {
        EmptyCount = emptyCount;
        DuplicateCount = duplicateCount;
        StructuralIssues = structuralIssues;
    }

    public int Total => EmptyCount + DuplicateCount + StructuralIssues;

    public string ToShortLabel()
    {
        var parts = new List<string>();
        if (DuplicateCount > 0) parts.Add($"{DuplicateCount} duplicates");
        if (EmptyCount > 0) parts.Add($"{EmptyCount} empty name" + (EmptyCount == 1 ? string.Empty : "s"));
        if (StructuralIssues > 0) parts.Add("structural issues");
        return $"⚠ {Total} invalid entr{(Total == 1 ? "y" : "ies")} ({string.Join(", ", parts)})";
    }
}

public static CleanUpSummary Summarize(IRegistryAccessor accessor)
{
    var empty = 0;
    var duplicates = 0;
    var structural = accessor.HasStructuralDamage(out _) ? 1 : 0;

    var seen = new HashSet<string>();
    for (var i = 0; i < accessor.Count; i++)
    {
        var name = accessor.GetName(i);
        if (string.IsNullOrEmpty(name)) empty++;
        else if (!seen.Add(name)) duplicates++;
    }

    return new CleanUpSummary(empty, duplicates, structural);
}
```

Required using: `using System.Collections.Generic;` (should already be present).

- [ ] **Step 16.2: Add USS for warning row**

Append to `Aspid-FastTools-Id-Registry.uss`:

```css
.aspid-fasttools-id-registry-warning
{
    display: none;
    flex-direction: row;
    align-items: center;
    padding: 5px 8px;
    margin-bottom: 4px;
    background-color: #4f1e1e;
    border-radius: 6px;
}

.aspid-fasttools-id-registry-warning--visible
{
    display: flex;
}

.aspid-fasttools-id-registry-warning-label
{
    flex-grow: 1;
    color: #ff9999;
    font-size: 11px;
}

.aspid-fasttools-id-registry-warning-button
{
    flex-shrink: 0;
    margin-left: 8px;
    padding: 2px 8px;
}
```

- [ ] **Step 16.3: Add class names to `Constants.cs`**

In `Constants.cs` in the `Registry` nested class, add:

```csharp
public const string Warning = "aspid-fasttools-id-registry-warning";
public const string WarningVisible = "aspid-fasttools-id-registry-warning--visible";
public const string WarningLabel = "aspid-fasttools-id-registry-warning-label";
public const string WarningButton = "aspid-fasttools-id-registry-warning-button";
```

- [ ] **Step 16.4: Wire warning row into `RegistryEditorCore`**

In `RegistryEditorCore.Build()`, after `container.Add(BuildSectionTitle("IDs"));` and before `container.Add(searchField)`, add:

```csharp
var warningRow = BuildWarningRow();
container.Add(warningRow);
```

Add fields at the top of the class:

```csharp
private VisualElement? _warningRow;
private Label? _warningLabel;
```

Add this method to the class:

```csharp
private VisualElement BuildWarningRow()
{
    var row = new VisualElement().AddClass(Constants.Registry.Warning);
    _warningRow = row;

    _warningLabel = new Label().AddClass(Constants.Registry.WarningLabel);
    var reviewButton = new Button { text = "Review" }.AddClass(Constants.Registry.WarningButton);
    reviewButton.clicked += ShowCleanUpDialog;

    row.Add(_warningLabel);
    row.Add(reviewButton);
    return row;
}

private void RefreshWarningRow()
{
    if (_warningRow == null || _warningLabel == null) return;
    var summary = IdRegistryValidator.Summarize(_accessor);
    var visible = summary.Total > 0;
    _warningRow.EnableInClassList(Constants.Registry.WarningVisible, visible);
    if (visible) _warningLabel.text = summary.ToShortLabel();
}

private void ShowCleanUpDialog()
{
    var summary = IdRegistryValidator.Summarize(_accessor);
    if (summary.Total == 0) return;

    var message = $"This will remove {summary.Total} invalid entr{(summary.Total == 1 ? "y" : "ies")}:\n"
                + (summary.DuplicateCount > 0 ? $"  • {summary.DuplicateCount} duplicate name(s)\n" : string.Empty)
                + (summary.EmptyCount > 0 ? $"  • {summary.EmptyCount} empty name(s)\n" : string.Empty)
                + (summary.StructuralIssues > 0 ? "  • structural inconsistencies\n" : string.Empty)
                + "\nProceed?";

    if (!EditorUtility.DisplayDialog("Clean up invalid entries", message, "Clean up", "Cancel"))
        return;

    _accessor.Record("Clean Up Invalid IDs");

    var seen = new HashSet<string>();
    var toRemove = new List<int>();
    for (var i = 0; i < _accessor.Count; i++)
    {
        var name = _accessor.GetName(i);
        if (string.IsNullOrEmpty(name) || !seen.Add(name))
            toRemove.Add(i);
    }

    for (var i = toRemove.Count - 1; i >= 0; i--)
        _accessor.RemoveAt(toRemove[i]);

    _accessor.Commit();
}
```

In `RebuildEntries()`, add at the end:

```csharp
RefreshWarningRow();
```

- [ ] **Step 16.5: Verify no `CleanUpInvalid` references remain**

```bash
grep -rn "CleanUpInvalid" Aspid.FastTools/Assets --include="*.cs"
```

Expected: empty. If `StringIdRegistryEditor.cs` was already fully replaced in Task 11, there should be no reference.

- [ ] **Step 16.6: Verify in Unity**

Open a `StringIdRegistry`. Use Debug Inspector to introduce an empty-name entry or a duplicate. Switch to Normal Inspector — the warning row appears with the correct count. Click Review → dialog shows summary; Clean up removes exactly those entries; Ctrl+Z restores them.

- [ ] **Step 16.7: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/IdRegistryValidator.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs
git commit -m "Replace silent CleanUpInvalid with explicit Review/Clean-up warning row"
```

---

### Task 17: Next ID control (J) — manual input with backward-step warning

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs`

- [ ] **Step 17.1: Add USS for Next Id section**

Append to `Aspid-FastTools-Id-Registry.uss`:

```css
.aspid-fasttools-id-registry-next-id-row
{
    flex-direction: row;
    align-items: center;
    margin-top: 4px;
    margin-bottom: 4px;
}

.aspid-fasttools-id-registry-next-id-label
{
    min-width: 60px;
    -unity-font-style: bold;
}

.aspid-fasttools-id-registry-next-id-field
{
    min-width: 80px;
    flex-grow: 0;
}

.aspid-fasttools-id-registry-next-id-warning
{
    display: none;
    margin-left: 6px;
    width: 16px;
    height: 16px;
}

.aspid-fasttools-id-registry-next-id-warning--visible
{
    display: flex;
}
```

- [ ] **Step 17.2: Add class names to `Constants.cs`**

In the `Registry` nested class:

```csharp
public const string NextIdRow = "aspid-fasttools-id-registry-next-id-row";
public const string NextIdLabel = "aspid-fasttools-id-registry-next-id-label";
public const string NextIdField = "aspid-fasttools-id-registry-next-id-field";
public const string NextIdWarning = "aspid-fasttools-id-registry-next-id-warning";
public const string NextIdWarningVisible = "aspid-fasttools-id-registry-next-id-warning--visible";
```

- [ ] **Step 17.3: Build Next Id row in `RegistryEditorCore`**

In `RegistryEditorCore.Build()`, after the `typeContainer` block and before the `container` definition, add:

```csharp
var nextIdRow = BuildNextIdRow();
typeContainer.Add(nextIdRow);
```

Add the helper:

```csharp
private VisualElement BuildNextIdRow()
{
    var row = new VisualElement().AddClass(Constants.Registry.NextIdRow);

    var label = new Label("Next ID").AddClass(Constants.Registry.NextIdLabel);

    var field = new IntegerField
    {
        value = _accessor.NextIdProperty.intValue,
        tooltip = "Id that will be assigned to the next Add operation. Manual override is allowed.",
    }.AddClass(Constants.Registry.NextIdField);

    var warning = new Image
    {
        image = EditorGUIUtility.IconContent("console.warnicon.sml").image,
        tooltip = string.Empty,
    }.AddClass(Constants.Registry.NextIdWarning);

    field.RegisterValueChangedCallback(e =>
    {
        var newValue = e.newValue;
        UpdateNextIdWarning(warning, newValue);

        _accessor.Record("Set Next ID");
        _accessor.NextIdProperty.intValue = newValue;
        _accessor.Commit();
    });

    _accessor.SerializedObject.Update(); // ensure fresh
    UpdateNextIdWarning(warning, _accessor.NextIdProperty.intValue);

    row.Add(label);
    row.Add(field);
    row.Add(warning);
    return row;
}

private void UpdateNextIdWarning(Image warning, int value)
{
    var maxAssigned = _accessor.MaxAssignedId;
    var show = value <= maxAssigned && value >= 1;
    warning.EnableInClassList(Constants.Registry.NextIdWarningVisible, show);
    warning.tooltip = show
        ? $"Reusing ID {value} may silently remap references: assets that previously pointed to this ID will appear bound to the next name you create. Proceed only if you know these IDs are unused."
        : value < 1
            ? "Next ID must be ≥ 1."
            : string.Empty;
}
```

Add `using UnityEditor.UIElements;` at the top of the file if not already present (required for `IntegerField`).

- [ ] **Step 17.4: Verify in Unity**

Open a `StringIdRegistry` with max Id = 5. The Next Id field shows `6`. Change to `5` — warning icon appears with the tooltip about silent remapping. Change to `10` — warning gone. Change to `0` — icon hidden, tooltip says `Next ID must be ≥ 1`.

Cross-check: adding a new entry now takes the current Next Id value, increments it, and writes.

- [ ] **Step 17.5: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs
git commit -m "Add Next ID control with backward-step warning"
```

---

### Task 18: Sort/Group (H)

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs`

- [ ] **Step 18.1: USS**

Append:

```css
.aspid-fasttools-id-registry-toolbar
{
    flex-direction: row;
    margin-top: 4px;
    margin-bottom: 4px;
}

.aspid-fasttools-id-registry-sort
{
    margin-right: 8px;
    min-width: 120px;
}

.aspid-fasttools-id-registry-group
{
    min-width: 120px;
}

.aspid-fasttools-id-registry-group-foldout
{
    margin-top: 4px;
}

.aspid-fasttools-id-registry-group-foldout > .unity-foldout__toggle > .unity-toggle__input > .unity-label
{
    -unity-font-style: bold;
    color: #b0b0b0;
}
```

- [ ] **Step 18.2: Constants**

Add:

```csharp
public const string Toolbar = "aspid-fasttools-id-registry-toolbar";
public const string Sort = "aspid-fasttools-id-registry-sort";
public const string Group = "aspid-fasttools-id-registry-group";
public const string GroupFoldout = "aspid-fasttools-id-registry-group-foldout";
```

- [ ] **Step 18.3: Sort/Group enums**

Add to `RegistryEditorCore.cs`, as private nested enums:

```csharp
private enum SortMode { RegistryOrder, NameAZ, NameZA, IdAsc, IdDesc }
private enum GroupMode { None, ByPrefix }
```

- [ ] **Step 18.4: SessionState keys + state fields**

Add to the `RegistryEditorCore` class:

```csharp
private SortMode _sortMode = SortMode.RegistryOrder;
private GroupMode _groupMode = GroupMode.None;
private string _assetGuid = string.Empty;

private string SortKey => $"Aspid.FastTools.Ids.Registry:{_assetGuid}:Sort";
private string GroupKey => $"Aspid.FastTools.Ids.Registry:{_assetGuid}:Group";
private string GroupExpandedKey(string group) => $"Aspid.FastTools.Ids.Registry:{_assetGuid}:Group:{group}:Expanded";
```

At the start of `Build()`, compute `_assetGuid` and load SessionState:

```csharp
var assetPath = AssetDatabase.GetAssetPath(_accessor.Target);
_assetGuid = string.IsNullOrEmpty(assetPath) ? _accessor.Target.GetInstanceID().ToString()
                                              : AssetDatabase.AssetPathToGUID(assetPath);
_sortMode = (SortMode)SessionState.GetInt(SortKey, (int)SortMode.RegistryOrder);
_groupMode = (GroupMode)SessionState.GetInt(GroupKey, (int)GroupMode.None);
```

- [ ] **Step 18.5: Build toolbar**

In `Build()`, immediately after the search field, add:

```csharp
var toolbar = BuildSortGroupToolbar();
container.Add(toolbar);
```

Add the helper:

```csharp
private VisualElement BuildSortGroupToolbar()
{
    var row = new VisualElement().AddClass(Constants.Registry.Toolbar);

    var sort = new EnumField(_sortMode).AddClass(Constants.Registry.Sort);
    sort.tooltip = "Sort order";
    sort.RegisterValueChangedCallback(e =>
    {
        _sortMode = (SortMode)e.newValue;
        SessionState.SetInt(SortKey, (int)_sortMode);
        RebuildEntries();
    });

    var group = new EnumField(_groupMode).AddClass(Constants.Registry.Group);
    group.tooltip = "Group entries by";
    group.RegisterValueChangedCallback(e =>
    {
        _groupMode = (GroupMode)e.newValue;
        SessionState.SetInt(GroupKey, (int)_groupMode);
        RebuildEntries();
    });

    row.Add(sort);
    row.Add(group);
    return row;
}
```

- [ ] **Step 18.6: Apply sort in `RebuildEntries`**

Right before the final `_listView?.Rebuild();`, insert:

```csharp
ApplySort(_viewModel);
```

Add:

```csharp
private void ApplySort(List<EntryView> list)
{
    switch (_sortMode)
    {
        case SortMode.NameAZ:
            list.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));
            break;
        case SortMode.NameZA:
            list.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(b.Name, a.Name));
            break;
        case SortMode.IdAsc:
            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            break;
        case SortMode.IdDesc:
            list.Sort((a, b) => b.Id.CompareTo(a.Id));
            break;
        case SortMode.RegistryOrder:
        default:
            // _viewModel was already built in registry-index order.
            break;
    }
}
```

- [ ] **Step 18.7: Group rendering**

Grouping replaces `_listView` with a stack of Foldouts when `_groupMode == ByPrefix`. Add a container around the list and swap its content on rebuild.

Refactor: introduce a `_listContainer` field and build the list inside it. In `Build()`:

```csharp
_listContainer = new VisualElement();
container.Add(_listContainer);
```

Move `_listView` construction into a helper `BuildListView()` that returns the ListView and adds it to `_listContainer`. Replace the old `container.Add(_listView);` with `_listContainer.Add(_listView);`.

Add field:

```csharp
private VisualElement? _listContainer;
```

Modify `RebuildEntries` to branch on group mode:

```csharp
private void RebuildEntries()
{
    _viewModel.Clear();
    // ... (existing collection + filtering code) ...

    ApplySort(_viewModel);

    if (_listContainer == null) return;
    _listContainer.Clear();

    if (_groupMode == GroupMode.None)
    {
        if (_listView == null) _listView = BuildListView();
        _listView.itemsSource = _viewModel;
        _listContainer.Add(_listView);
        _listView.Rebuild();
    }
    else
    {
        RenderGroupedView();
    }

    RefreshWarningRow();
    UpdateListScrollState();
}

private void RenderGroupedView()
{
    if (_listContainer == null) return;

    var buckets = new Dictionary<string, List<EntryView>>();
    foreach (var view in _viewModel)
    {
        var prefix = PrefixOf(view.Name);
        if (!buckets.TryGetValue(prefix, out var list))
        {
            list = new List<EntryView>();
            buckets[prefix] = list;
        }
        list.Add(view);
    }

    foreach (var kv in buckets)
    {
        var groupName = kv.Key;
        var foldout = new Foldout
        {
            text = $"{groupName} ({kv.Value.Count})",
            value = SessionState.GetBool(GroupExpandedKey(groupName), defaultValue: true),
        }.AddClass(Constants.Registry.GroupFoldout);

        foldout.RegisterValueChangedCallback(e =>
            SessionState.SetBool(GroupExpandedKey(groupName), e.newValue));

        foreach (var view in kv.Value)
        {
            var row = CreateEntryRow();
            ((IdRegistryEntryVisualElement)row).Bind(new IdRegistryEntryData(
                originalIndex: view.OriginalIndex,
                name: view.Name,
                id: view.Id,
                isDuplicate: view.IsDuplicate));
            foldout.Add(row);
        }

        _listContainer.Add(foldout);
    }
}

private static string PrefixOf(string name)
{
    if (string.IsNullOrEmpty(name)) return "<ungrouped>";
    var idx = name.IndexOf('_');
    return idx <= 0 ? "<ungrouped>" : name.Substring(0, idx);
}
```

Important: `UpdateListScrollState` must not run while `_groupMode == ByPrefix` — add an early return:

```csharp
private void UpdateListScrollState()
{
    if (_listView == null || _groupMode != GroupMode.None) return;
    // ... existing body ...
}
```

- [ ] **Step 18.8: Verify in Unity**

Open a `StringIdRegistry` with 15+ entries covering multiple prefixes (e.g. `Enemy_Goblin`, `Enemy_Orc`, `Quest_Intro`, `LoneName`).

1. Default view: `RegistryOrder` + `None` — identical to before.
2. Sort `NameAZ` — entries alphabetized.
3. Sort `IdAsc` — entries by Id ascending.
4. Group `ByPrefix` — two Foldouts (`Enemy`, `Quest`) plus `<ungrouped>` (for `LoneName`).
5. Collapse `Enemy` — close Unity, reopen — Enemy still collapsed (SessionState persists across domain reload; resets on Unity restart).

Edge cases:
- Search "Enemy" + Group ByPrefix: shows only the Enemy foldout.
- Rename an entry to move it between groups — foldouts rebuild correctly.

- [ ] **Step 18.9: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Registries/RegistryEditorCore.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Registry.uss \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs
git commit -m "Add Sort and Group toolbar with SessionState persistence"
```

---

## Phase F — Drawer integration

### Task 19: Open Registry button (G) + Int-only read-only hint

**Files:**
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Drawer.uss`
- Modify: `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs`

- [ ] **Step 19.1: Add USS for Open button**

Append to `Aspid-FastTools-Id-Drawer.uss`:

```css
.aspid-fasttools-id-drawer-open-button
{
    flex-shrink: 0;
    margin: 0 2px;
    padding: 0;
    min-width: 22px;
    max-width: 22px;
    min-height: 18px;
    max-height: 18px;
}

.aspid-fasttools-id-drawer-open-button Image
{
    width: 14px;
    height: 14px;
    -unity-background-scale-mode: scale-to-fit;
}

.aspid-fasttools-id-drawer-int-only-hint
{
    color: #aa8833;
    font-size: 10px;
    margin-top: 2px;
    -unity-font-style: italic;
}
```

- [ ] **Step 19.2: Add class names to `Constants.cs`**

In the `Drawer` nested class:

```csharp
public const string OpenButton = "aspid-fasttools-id-drawer-open-button";
public const string IntOnlyHint = "aspid-fasttools-id-drawer-int-only-hint";
```

- [ ] **Step 19.3: UIToolkit drawer — add Open button and int-only hint**

In `IdStructDrawer.DrawUIToolkit`, modify:

- Right after `createToggleButton` is declared, add:

```csharp
var openButton = new Button().AddClass(Constants.Drawer.OpenButton);
var openIcon = new Image { image = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image };
openButton.Add(openIcon);
openButton.tooltip = "Open the registry asset in Inspector";
```

- At the end of `dropdownButton.clicked` setup (the existing event wiring), also wire:

```csharp
openButton.clicked += () =>
{
    var reg = IdRegistryResolver.Find(fieldType);
    if (reg == null) return;
    EditorGUIUtility.PingObject(reg);
    Selection.activeObject = reg;
};
```

- Keep an ongoing enable-state refresh: after the initial `dropdownButton.schedule.Execute(SyncStringFromInt).StartingIn(0);` line, add:

```csharp
openButton.SetEnabled(IdRegistryResolver.Find(fieldType) != null);
dropdownButton.TrackPropertyValue(intIdProp, _ =>
{
    SyncStringFromInt();
    openButton.SetEnabled(IdRegistryResolver.Find(fieldType) != null);
});
```

(Replaces the previous `dropdownButton.TrackPropertyValue(intIdProp, _ => SyncStringFromInt());` — remove the old one.)

- Add `openButton` to the main row **between** `dropdownButton` and `createToggleButton`:

```csharp
mainRow.AddChild(dropdownButton).AddChild(openButton).AddChild(createToggleButton);
```

(Replaces `mainRow.AddChild(dropdownButton).AddChild(createToggleButton);`.)

- Add int-only hint: after the main `root` assembly, conditionally append a hint if only an `IdRegistry` is found:

```csharp
var intOnlyHint = new Label("Bound to an int-only IdRegistry — names unavailable")
    .AddClass(Constants.Drawer.IntOnlyHint)
    .SetDisplay(DisplayStyle.None);

void RefreshIntOnlyHint()
{
    var found = IdRegistryResolver.Find(fieldType);
    var isIntOnly = found is Aspid.FastTools.Ids.IdRegistry;
    intOnlyHint.SetDisplay(isIntOnly ? DisplayStyle.Flex : DisplayStyle.None);
    dropdownButton.SetEnabled(!isIntOnly);
    createToggleButton.SetEnabled(!isIntOnly);
}

root.schedule.Execute(RefreshIntOnlyHint).StartingIn(0);
root.Add(intOnlyHint);
```

Insert `root.Add(intOnlyHint);` after the existing `return root.AddChild(mainRow).AddChild(createRow).AddChild(errorLabel);` — actually restructure the return so it appends the hint. Change:

```csharp
return root.AddChild(mainRow).AddChild(createRow).AddChild(errorLabel);
```

to:

```csharp
return root.AddChild(mainRow).AddChild(createRow).AddChild(errorLabel).AddChild(intOnlyHint);
```

- [ ] **Step 19.4: IMGUI drawer — add Open button**

In `IdStructDrawer.DrawIMGUI`, replace the widths and layout around `mainRect/dropRect/btnRect` to reserve space for a new Open button. New code (replace lines ~56–69):

```csharp
const float OpenButtonWidth = 22f;
const float Gap = 2f;

var mainRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
var dropRect = new Rect(mainRect.x, mainRect.y,
    mainRect.width - CreateButtonWidth - OpenButtonWidth - Gap * 2f, mainRect.height);
var openRect = new Rect(dropRect.xMax + Gap, mainRect.y, OpenButtonWidth, mainRect.height);
var btnRect  = new Rect(openRect.xMax + Gap, mainRect.y, CreateButtonWidth, mainRect.height);

var currentName = stringIdProp?.stringValue ?? string.Empty;

if (EditorGUI.DropdownButton(dropRect, new GUIContent(Caption(currentName)), FocusType.Passive))
{
    var reg = IdRegistryResolver.FindStringMapped(fieldType);
    var sp = GUIUtility.GUIToScreenPoint(new Vector2(dropRect.x, dropRect.y));
    var sr = new Rect(sp.x, sp.y, dropRect.width, dropRect.height);
    StringIdSelectorWindow.Show(reg, sr, currentName,
        selected => ApplySelection(property, stringIdProp, intIdProp, fieldType, selected));
}

using (new EditorGUI.DisabledScope(IdRegistryResolver.Find(fieldType) == null))
{
    if (GUI.Button(openRect, EditorGUIUtility.IconContent("d_ScriptableObject Icon")))
    {
        var reg = IdRegistryResolver.Find(fieldType);
        if (reg != null)
        {
            EditorGUIUtility.PingObject(reg);
            Selection.activeObject = reg;
        }
    }
}

if (GUI.Button(btnRect, state.creating ? "Cancel" : "Create"))
{
    _imguiState[key] = state.creating ? (false, string.Empty) : (true, string.Empty);
    state = _imguiState[key];
}
```

- [ ] **Step 19.5: Verify in Unity**

Open a ScriptableObject with an `IdStruct` field for which a `StringIdRegistry` exists.
1. Open button enabled, icon visible. Click → registry asset pings and becomes selected in Project.
2. Delete the registry asset. Open button goes disabled.
3. Manually create an `IdRegistry` bound to the same struct type (set its `_targetStructType` via Debug Inspector). The drawer now shows the int-only hint label in italic; Create and the dropdown are disabled; the Open button is enabled and pings the `IdRegistry`.

IMGUI path: switch Inspector to Debug mode (or select a component whose drawer falls back to IMGUI) — same flow.

- [ ] **Step 19.6: Commit**

```bash
git add Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Drawers/IdStructDrawer.cs \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Resources/Styles/Aspid-FastTools-Id-Drawer.uss \
        Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Unity/Editor/Scripts/Ids/Constants.cs
git commit -m "Add Open Registry button and int-only registry hint to IdStructDrawer"
```

---

## Phase G — Docs

### Task 20: Update `CLAUDE.md`

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 20.1: Update the StringIds section**

Locate the paragraph starting with `**StringIds** (\`Unity/Runtime/Ids/\`...)`. Replace with:

```markdown
**Id Registries** (`Unity/Runtime/Ids/`, `Unity/Editor/Scripts/Ids/`): Two ScriptableObject types with different runtime contracts:

- `StringIdRegistry` — full `int ↔ string` mapping at runtime; `GetId(name)`, `GetNameId(id)`, `Contains(name)`.
- `IdRegistry` — int-only at runtime; names are stored in an editor-only partial and stripped from player builds.

Each struct type decorated with `[UniqueId]` / implementing `IId` should be bound to exactly **one** registry of either kind — uniqueness is enforced at lookup time by `IdRegistryResolver`, which searches both types.

Editor UI is shared through `RegistryEditorCore` + `IRegistryAccessor` (two implementations). Features: C#-identifier name validation, full Undo, explicit Clean-up flow for invalid entries, Sort/Group toolbar, manual Next ID with backward-step warning, Open-Registry shortcut on the `IdStruct` drawer.

The `IdStructGenerator` generates boilerplate for the struct side; the registry picks for that struct are made via `Assets → Create → Aspid/FastTools/Id Registry` (int-only) or `.../String Id Registry`.
```

- [ ] **Step 20.2: Verify**

Read the resulting `CLAUDE.md` section — it should read coherently in the context of the surrounding text.

- [ ] **Step 20.3: Commit**

```bash
git add CLAUDE.md
git commit -m "Update CLAUDE.md to document both Id registry types"
```

---

## Self-Review

**Spec coverage check:**

| Spec section | Covered by |
|---|---|
| §1 Runtime: `IdRegistry.cs` + `IdRegistry.Editor.cs` | Task 4 |
| §1 Runtime: `StringIdRegistry` sealed + caches | Task 3 |
| §1 Editor file layout | Tasks 9 (accessors), 10 (Core), 11–12 (editors), 13 (deletions), 2 (scanner delete), 5–8 (renames) |
| §2 `IRegistryAccessor` | Task 9 |
| §2 `RegistryEditorCore` | Tasks 10, 16, 17, 18 (incremental) |
| §2 `IdRegistryResolver` (dual-type, no reflection) | Task 5 |
| §2 Drawer ↔ Resolver integration | Task 19 |
| §3-D Name validation | Tasks 7 (helper), 10 (wiring), 14 (verification) |
| §3-E Full Undo | Tasks 9 (`Record`), 10 (wiring), 15 (verification) |
| §3-F Explicit Clean-up | Task 16 |
| §3-G Open Registry button | Task 19 |
| §3-H Sort / Group | Task 18 |
| §3-J Manual Next ID with warning | Task 17 |
| §4 Bug 1 (`t:IdRegistry` filter) | Task 5 |
| §4 Bug 2 (reflection `_targetStructType`) | Task 5 |
| §4 Bug 3 (silent `CleanUpInvalid`) | Task 16 |
| §4 Bug 4 (`HasDuplicate` Count) | Task 1 |
| §4 Bug 5 (O(n) lookups) | Task 3 |
| §4 Bug 6 (`StringIdRegistry` sealed) | Task 3 |
| §4 Bug 7 (CacheInvalidator filter) | Task 1 |
| §4 Bug 8 (dead USS) | Task 1 |
| §4 Bug 9 (`IdRegisterEditorExtensions` delete) | Task 13 |
| §4 Bug 10 (`StringIdUsageScanner` delete) | Task 2 |
| §4 Bug 11 (`CheckIsUnique` cache bump) | Task 1 |
| §4 Bug 12 (`IdRegistry` sealed) | Task 4 |
| §4 Bug 13 (`IdRegistry` int[] + editor partial) | Task 4 |
| §5 Migration safety | Task 4 Step 4.3 (grep check), stable class names preserved throughout |
| §6 Naming / USS / internal sealed | All tasks observe these rules |
| §7 Out of scope | Not touched |
| §8 Deliverable shape | Matches task-by-task file operations |

No gaps.

**Placeholder scan:** No `TBD`, `TODO`, `implement later`. Every code block contains complete code. Every shell command is exact.

**Type consistency:**
- `IRegistryAccessor` used identically in Tasks 9, 10, 16, 17, 18, 11, 12.
- `IdRegistryResolver.FindStringMapped / Find / FindIntOnly / CreateStringMapped` signatures used identically in Tasks 5 and 19.
- `IdRegistryValidator.IsValidName(string?, HashSet<string>?, out string?)` consistent in Tasks 7 and 10.
- `Summarize(IRegistryAccessor)` and `CleanUpSummary` only appear in Task 16.
- USS class constants added incrementally but are never referenced before they are declared.

No inconsistencies found.
