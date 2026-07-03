<img src="Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

<p>
  <a href="https://assetstore.unity.com/packages/slug/365584"><img src="https://img.shields.io/badge/Unity_6.0%2B-000000?style=flat&logo=unity&logoColor=white&color=4fa35d" alt="Unity 6.0+" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm?label=Stable&labelColor=254d2c&color=4fa35d" alt="Stable" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm-preview?label=Preview&labelColor=4d4425&color=a3923d" alt="Preview" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/VPDPersonal/Aspid.FastTools?label=License&labelColor=254d2c&color=4fa35d" alt="License" /></a>
</p>

**Aspid.FastTools** is a set of tools designed to minimize routine code writing in Unity: `SerializeReference` tooling (an inspector type picker plus a reference explorer window), Roslyn-powered source generators, and a collection of runtime and editor utilities — from a serializable `System.Type` to fluent UI Toolkit extensions.

### \[[Unity Asset Store](https://assetstore.unity.com/packages/slug/365584)\] \[[Donate](#donate)\]

## Table of Contents

- **Getting Started**
  - [Integration](#integration)
  - [Claude Code Plugin](#claude-code-plugin)
  - [Donate](#donate)
- **Features**
  - [ProfilerMarker](#profilermarker)
  - [Serializable Type System](#serializable-type-system)
  - [SerializeReference Selector](#serializereference-selector)
  - [Enum System](#enum-system)
  - [ID System (Beta)](#id-system-beta)
  - [VisualElement Extensions](#visualelement-extensions)
  - [SerializedProperty Extensions](#serializedproperty-extensions)
  - [IMGUI Layout Scopes](#imgui-layout-scopes)
  - [Editor Helper Extensions](#editor-helper-extensions)

---

## Integration

Install Aspid.FastTools via UPM: in the Package Manager click **+ → Install package from git URL…** and paste one of the URLs below.

### Stable

The `upm` branch always points to the latest **stable** release:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm
```

To install a specific version, target the immutable per-release tag (see [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) for the list of available versions):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm/1.0.0
```

Prefer a manual install? Download the `.unitypackage` from the [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) page, or get the package from the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584).

<details>
<summary><strong>Preview</strong></summary>

<br>

The `upm-preview` branch always points to the latest **preview** release (rc, beta, alpha, …):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

To install a specific preview version, target the immutable per-release tag (see [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) for the list of available versions):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.2
```

</details>

---

## Claude Code Plugin

If you use [Claude Code](https://docs.claude.com/en/docs/claude-code), the companion [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) marketplace ships the `aspid-fasttools` plugin — a set of skills that teach Claude Code this package's conventions and APIs.

Add the marketplace and install the plugin:

```sh
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

```sh
/plugin install aspid-fasttools@aspid-claude-plugins
```

Included skills:

- **`aspid-id-struct`** — scaffold a new `IId` struct and `[UniqueId]` fields for the [ID System](#id-system-beta).
- **`aspid-profiler-marker`** — insert `this.Marker()` call sites with the right `using`/scope shape.
- **`aspid-visual-element-fluent`** — build editor or runtime UI using the fluent `VisualElement` extensions.

---

## Donate

This project is developed on a voluntary basis. If you find it useful, you can support its development financially. This helps allocate more time to improving and maintaining **Aspid.FastTools**.

You can donate via the following platforms:
* \[[Unity Asset Store](https://assetstore.unity.com/packages/slug/365584)\]

---

## ProfilerMarker

Provides source-generated `ProfilerMarker` registration. The generator creates a static marker per call-site, identified by the calling method and line number.

```csharp
using UnityEngine;

public class MyBehaviour : MonoBehaviour
{
    private void Update()
    {
        DoSomething1();
        DoSomething2();
    }

    private void DoSomething1()
    {
        using var _ = this.Marker();
        // Some code
    }

    private void DoSomething2()
    {
        using (this.Marker())
        {
            // Some code
            using var _ = this.Marker().WithName("Calculate");
            // Some code
        }
    }
}
```

<details>
<summary><b>Generated code</b></summary>
<br/>

```csharp
using Unity.Profiling;
using System.Runtime.CompilerServices;

internal static class __MyBehaviourProfilerMarkerExtensions
{
    private static readonly ProfilerMarker DoSomething1_Marker_Line_13 = new("MyBehaviour.DoSomething1 (13)");
    private static readonly ProfilerMarker DoSomething2_Marker_Line_19 = new("MyBehaviour.DoSomething2 (19)");
    private static readonly ProfilerMarker DoSomething2_Marker_Line_22 = new("MyBehaviour.Calculate (22)");

    public static ProfilerMarker.AutoScope Marker(this MyBehaviour _, [CallerLineNumberAttribute] int line = -1)
    {
#if ENABLE_PROFILER
        if (line is 13) return DoSomething1_Marker_Line_13.Auto();
        if (line is 19) return DoSomething2_Marker_Line_19.Auto();
        if (line is 22) return DoSomething2_Marker_Line_22.Auto();
#endif
        return default;
    }
}
```

</details>

### Result

![aspid_fasttools_profiler_markers.png](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_profiler_markers.png)

---

## Serializable Type System

Allows serializing a `System.Type` reference in the Unity Inspector. The selected type is stored as an assembly-qualified name and resolved lazily on first access.

### SerializableType

Two variants are available:

- **`SerializableType`** — stores any type (base type is `object`)
- **`SerializableType<T>`** — stores a type constrained to `T` or its subclasses

Both support implicit conversion to `System.Type`.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class Ability : MonoBehaviour
{
    public abstract void Activate();
}

public sealed class AbilitySelector : MonoBehaviour
{
    [SerializeField] private SerializableType<Ability> _abilityType;

    private void Start()
    {
        var ability = (Ability)gameObject.AddComponent(_abilityType.Type);
        ability.Activate();
    }
}
```
![aspid_fasttools_serializable_type.gif](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_serializable_type.gif)

### TypeSelectorAttribute

An editor-only `PropertyAttribute` that restricts the type selection popup to specific base types. Applied to `string` fields that store assembly-qualified type names.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class TypeSelectorAttribute : PropertyAttribute
{
    public TypeSelectorAttribute() // base type: object
    public TypeSelectorAttribute(Type type)
    public TypeSelectorAttribute(params Type[] types)
    public TypeSelectorAttribute(string assemblyQualifiedName)
    public TypeSelectorAttribute(params string[] assemblyQualifiedNames)

    public TypeAllow Allow { get; set; }  // default: TypeAllow.None
    public bool Required { get; set; }    // default: false
}

[Flags]
public enum TypeAllow
{
    None      = 0,
    Abstract  = 1,
    Interface = 2,
    All       = Abstract | Interface
}
```

| Property | Description |
|----------|-------------|
| `Allow` | Which special type categories (abstract classes, interfaces) the picker includes in addition to plain concrete classes. Default: `TypeAllow.None` |
| `Required` | Flags an unset field: a `[SerializeReference]` managed reference left `null`, or a `string` field left empty, shows an inline "required" warning in the Inspector and counts as a violation for the build/CI gate. Default: `false` |

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class AbilityModifier
{
    public abstract void Apply();
}

public sealed class AbilitySelector : MonoBehaviour
{
    // Each element of the array is its own picker constrained to AbilityModifier.
    [TypeSelector(typeof(AbilityModifier))]
    [SerializeField] private string[] _modifierTypes;
}
```

> The complete sample — `Ability` / `AbilitySelector` / `EnemyBase` and their subclasses — ships in the `Types` sample (Package Manager → Aspid.FastTools → Samples).

Decorate a candidate type with `[TypeSelectorDisplay]` to tune how it appears in the picker — an editor-only attribute (`[Conditional("UNITY_EDITOR")]`) in `Aspid.FastTools.Types` that carries no runtime cost:

```csharp
using Aspid.FastTools.Types;

// Rename the type in the picker, place it under an explicit group, give it a tooltip and an icon:
[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```

| Member | Description |
|--------|-------------|
| `Name` | Display name shown instead of the type's short name — in the picker rows and in the closed dropdown's caption. Search still matches the real type name too, and the hover tooltip keeps revealing the full `Namespace.Class, Assembly` identity. `null` or whitespace means no override. |
| `Group` | Explicit picker path with `/` separating levels (e.g. `"Combat/Melee"`). **Replaces** the type's namespace placement — the type appears only under this path, and path segments are shared between types. `null` or whitespace keeps the namespace placement. |
| `Tooltip` | Tooltip shown when hovering the type's row. `null` means no tooltip override. |
| `Icon` | Editor icon shown left of the label — an `EditorGUIUtility.IconContent` name, a project-relative asset path with extension (loaded via `AssetDatabase`), or a `Resources` texture path without extension. `null` means no icon. |

---

### Type Selector Window

The Inspector shows a button that opens a searchable popup window with:

- Hierarchical namespace organization
- Text search with filtering
- Keyboard navigation (Arrow keys, Enter, Escape; Space toggles a favorite)
- Breadcrumb trail with back navigation (Left arrow or a click on a crumb)
- Assembly disambiguation for types with identical names
- **Favorites** (★ on hover) and **Recent** (last picks) sections on the root page — stored locally per project (`EditorPrefs`, never committed), hidden while searching
- A `<None>` option pinned at the top and a ✓ mark on the current value — its row is pre-selected on open
- Type counters on namespace/group rows and section headers
- Generic type support — picking an open generic walks through its type parameters and emits the constructed type
- Favorites/Recent tuning (on/off, Recent capacity) in the Settings tab of the SerializeReference window

![aspid_fasttools_type_selector_window.png](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_type_selector_window.png)

The same window is available as a public API — open it from any editor code (custom inspectors, `EditorWindow`, menu items) when you need a type picker outside the standard `SerializableType` / `[TypeSelector]` flow.

```csharp
namespace Aspid.FastTools.Types.Editors
{
    public sealed class TypeSelectorWindow : EditorWindow
    {
        public static void Show(
            Rect screenRect,
            TypeSelectorFilter filter = default,
            string currentAqn = "",
            Action<string> onSelected = null);
    }
}
```

| Parameter | Description |
|-----------|-------------|
| `screenRect` | Screen-space rectangle the dropdown is anchored to. |
| `filter` | Bundles which types the selector offers: base types (`Types`, only types assignable to **all** entries are listed; defaults to `typeof(object)`), the included kinds (`Allow`), an optional per-type `Predicate`, verbatim `AdditionalTypes`, and the open-generic `ArgumentFilter`. |
| `currentAqn` | Assembly-qualified name of the currently selected type, used to pre-navigate to its location. Pass `null` or empty to start at the root. |
| `onSelected` | Callback invoked with the assembly-qualified name of the selected type, or `null` if the user chose `<None>`. |

### ComponentTypeSelector

A serializable struct that renders a type-switching dropdown in the Inspector. Add it as a field to a base class — picking a subtype rewrites `m_Script` on the `SerializedObject`, effectively changing the component or ScriptableObject to the chosen subtype.

The dropdown is automatically constrained to subtypes of the class that declares the field. No additional configuration is required.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField] [Min(0)] private float _health = 100f;

    public abstract void Attack();
}

public sealed class FastEnemy : EnemyBase
{
    [SerializeField] [Min(0)] private float _speed = 25f;

    public override void Attack() =>
        Debug.Log($"Fast enemy strikes! (speed: {_speed})");
}

public sealed class TankEnemy : EnemyBase
{
    [SerializeField] [Min(0)] private float _armor = 50f;

    public override void Attack() =>
        Debug.Log($"Tank attacks! (armor: {_armor})");
}
```

<!-- TODO(media): re-record aspid_fasttools_component_type_selector.gif — FastEnemy ↔ TankEnemy switch with the current picker window -->
![aspid_fasttools_component_type_selector.gif](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_component_type_selector.gif)

---

## SerializeReference Selector

A drop-in type-picker dropdown for `[SerializeReference]` fields. Add `[TypeSelector]` next to `[SerializeReference]` and the Inspector replaces the default managed-reference UI with the same searchable, hierarchical picker used by `SerializableType`. You choose which concrete implementation of the field's type is instantiated, right in the Inspector; `<None>` clears the reference.

```csharp
using System;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Types;

public interface IWeapon
{
    void Fire();
}

[Serializable]
public sealed class Pistol : IWeapon
{
    [SerializeField] [Min(0)] private int _damage = 10;

    public void Fire() => Debug.Log($"Pistol: {_damage} dmg");
}

[Serializable]
public sealed class Railgun : IWeapon
{
    [SerializeField] [Min(0)] private float _chargeTime = 1.5f;

    public void Fire() => Debug.Log($"Railgun charged for {_chargeTime}s");
}

public sealed class Loadout : MonoBehaviour
{
    [SerializeReference] [TypeSelector]
    private IWeapon _primary;

    [SerializeReference] [TypeSelector]
    private List<IWeapon> _sidearms;
}
```

The attribute is editor-only (`[Conditional("UNITY_EDITOR")]`) and carries no runtime cost. It works on single fields, arrays, and `List<T>`, in both IMGUI and UIToolkit inspectors.

### Capabilities

| Capability | What it does |
|---|---|
| **Pick an implementation** | The list shows the concrete, non-`UnityEngine.Object` classes assignable to the field's type. `[TypeSelector(typeof(IMelee))]` narrows it to `IMelee` implementations. |
| **Inline inspector** | The selected instance's serialized fields are drawn under a foldout. |
| **Open generics** | `Modifier<T>` and the like: arguments are inferred from a closed-generic field, or picked on a second page inside the picker. |
| **Data preserved** | Switching type carries over fields shared by name and serialized shape instead of resetting them to defaults. |
| **Copy / Paste** | Right-click the header to copy the value and paste it as an independent instance into any compatible field. |
| **Multi-object editing** | A mixed selection shows a mixed dropdown; picking a type or pasting applies an independent instance to each object in one Undo group. |
| **Compile-time checks** | Roslyn analyzer: `AFT0004` (error) — the type derives from `UnityEngine.Object`; `AFT0005` (warning) — the picker would be empty. |

### Repairing broken references

| Case | Fix |
|---|---|
| **Missing type** (renamed or deleted) | A yellow notice instead of a silent clear. The underlined **Fix** opens the picker and re-points the type while keeping its data — at any depth, in saved assets and live in Prefab Mode. |
| **Smart Fix** | Next to **Fix**, suggests the most likely replacement (`[MovedFrom]`, a different namespace/assembly, casing, a near-miss name) and applies it in one click — never automatically. |
| **Shared reference** (two fields share one instance) | Flagged with a notice; **Make unique** splits it into an independent copy. Duplicating a list element (Ctrl+D, `+`) no longer aliases the reference. |

<!-- TODO(media): optional gif aspid_fasttools_serialize_reference_repair.gif — BrokenWeaponPreset → missing-type notice → Smart Fix, data preserved -->

Bulk repair lives in two dedicated tabs:

| Tab | Purpose |
|---|---|
| **Asset References** (`Tools → Aspid 🐍 → FastTools → Asset References`) | Maps an asset's whole managed-reference graph from its YAML — a per-component tree with field paths, shared and orphaned references, `MISSING` / `SHARED` badges, and an inline type dropdown on every card. Surfaces the missing references the Inspector cannot show. |
| **Project References** (`Tools → Aspid 🐍 → FastTools → Project References`) | `Scan Project` sweeps every `.prefab` / `.asset` / `.unity` under `Assets/`, groups broken references by stored type, and rewrites a whole group with a single `Fix all` (plus Smart Fix). A group whose stored type matches a declared `[MovedFrom]` rename reads as a pending migration instead of a breakage — one **Migrate all** click bakes the rename into the files, after which the attribute can be removed from code. |

### Project settings & the build/CI gate

**`Project Settings → Aspid FastTools → SerializeReference`** exposes:

| Setting | Scope | What it does |
|---|---|---|
| **Breakage detection** | per-user | The proactive toast + console warning when references newly become missing after a recompile / import. |
| **Auto de-alias duplicated list elements** | committed | A duplicated list element gets its own instance instead of sharing the original's reference id. |
| **Build / CI gate** | committed | `Off` / `Warn` / `Fail`: at player-build time, log or abort on missing (and, for CI, unset-required) managed references. |
| **Excluded scan folders** | committed | Paths skipped by every project scan. |

- Committed values live in `ProjectSettings/SerializeReferenceSharedSettings.asset` — commit it so teammates and CI behave identically; breakage detection stays per-machine (`EditorPrefs`).
- Rid colours are not a setting — a shared reference is always colour-coded by id, so matching colours reveal shared instances at a glance.

The same options are mirrored in the window's **Settings** tab (`Tools → Aspid 🐍 → FastTools → Settings`) and at **`Preferences → Aspid FastTools`**, alongside the picker's per-user preferences:

- **Favorites** — section on/off toggle.
- **Recent items** — capacity slider (0–20; 0 hides the section and pauses recording without wiping history).
- **Saved lists** — clears the stored Favorites / Recent.
- **Appearance** — editor-theme override `StyleSheet` with **Create template…**.
- **Welcome** — auto-show toggle.

Every row carries a scope stripe (green — committed, blue — per-user); a pinned footer offers **Reset to defaults** per scope (saved Favorites / Recent lists survive a reset). All surfaces stay in live sync.

For headless CI, `SerializeReferenceCiGate.RunCheck` (invoked via `-batchmode -executeMethod`) writes a report and honours the committed gate severity:

- `Off` skips the check, `Warn` logs but exits 0, `Fail` exits non-zero when violations exist.
- `-srGateRequired` also flags unset `[TypeSelector(Required = true)]` fields across prefabs, ScriptableObjects and scenes (top-level fields, pure-YAML pass).
- `-srGateWarnOnly` / `-srGateFail` override the committed severity per run.

> The full sample — `Loadout` / `IWeapon` / `Modifier<T>` and the missing-reference repair scenarios — ships in the `SerializeReferences` sample (Package Manager → Aspid.FastTools → Samples). A step-by-step walkthrough lives in that sample's `TUTORIAL.md`.

---

## Enum System

Provides serializable enum-to-value mappings configurable from the Inspector.

### EnumValues\<TValue\>

A serializable collection of `EnumValue<TValue>` entries with a configurable default value. Implements `IEnumerable<KeyValuePair<Enum, TValue>>`.

| Member | Description |
|--------|-------------|
| `TValue GetValue(Enum enumValue)` | Returns the mapped value, or `_defaultValue` if not found |
| `bool Equals(Enum, Enum)` | Equality check with proper `[Flags]` support |

Supports `[Flags]` enums: `Equals` uses `HasFlag` and treats `0`-valued members correctly.

```csharp
using System;
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType { Physical, Fire, Ice, Poison }

[Flags]
public enum StatusEffect
{
    None    = 0,
    Burning = 1,
    Frozen  = 2,
    Slowed  = 4,
    Stunned = 8,
}

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<float> _damageMultipliers;
    [SerializeField] private EnumValues<Color> _damageColors;

    // Flag combinations (e.g. Burning | Slowed) match via HasFlag and first-hit wins,
    // so list composite entries BEFORE their constituent flags.
    [SerializeField] private EnumValues<float> _speedMultipliersByStatus;

    [SerializeField] private DamageType _currentType;
    [SerializeField] private StatusEffect _activeEffects;

    private void DealDamage()
    {
        var multiplier = _damageMultipliers.GetValue(_currentType);
        var color      = _damageColors.GetValue(_currentType);
        var speedMod   = _speedMultipliersByStatus.GetValue(_activeEffects);
        // ...
    }
}
```
![aspid_fasttools_enum_values.png](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_enum_values.png)

In the Inspector, select the enum type in the `EnumValues` header, then assign a value for each enum member. Right-click the property to open a context menu with **Populate Missing Enum Members** — it appends an entry for every enum member not yet in the list, seeded with the current Default Value.

> The complete sample — `DamageDealer` / `DamageType` / `StatusEffect` — ships in the `EnumValues` sample (Package Manager → Aspid.FastTools → Samples).

### EnumValues\<TEnum, TValue\>

The typed counterpart of `EnumValues<TValue>` for the common case where the enum type is already known in code. The enum is fixed by the generic argument, so the Inspector's type picker is disabled and lookups are compile-time safe. Lookups are also boxing-free — keys are compared as cached numeric values — and `foreach` over either variant binds to a struct enumerator, so iteration does not allocate. Implements `IEnumerable<KeyValuePair<TEnum, TValue>>`.

| Member | Description |
|--------|-------------|
| `TValue GetValue(TEnum enumValue)` | Returns the mapped value, or `_defaultValue` if not found |
| `bool Equals(TEnum, TEnum)` | Equality check with proper `[Flags]` support |

```csharp
public sealed class HitEffect : MonoBehaviour
{
    // The type picker in the Inspector is disabled — the enum is fixed to DamageType.
    [SerializeField] private EnumValues<DamageType, Color> _damageColors;

    public Color GetColor(DamageType type) => _damageColors.GetValue(type);
}
```

Lookup semantics (including `[Flags]` handling) are identical to `EnumValues<TValue>`, and the serialized layout is compatible — switching a field between the two variants keeps the existing data as long as the enum type matches.

---

## ID System (Beta)

> **Beta:** the ID System is currently in beta. The public API, generated code layout and editor workflow may change in future releases.

Maps an asset-assignable name to a stable integer ID. Use the resulting `int` in `switch` statements and `Dictionary` keys without paying for string lookups at runtime.

A single `IdRegistry` ScriptableObject maps string names to stable integer IDs and provides full `int ↔ string` lookups at runtime.

### Setup

**1.** Declare a `partial struct` implementing `IId`. The source generator adds the required fields and property automatically:

```csharp
using Aspid.FastTools.Ids;

public partial struct EnemyId : IId { }
```

Generated code:

```csharp
public partial struct EnemyId
{
    [SerializeField] private string __stringId; // editor-only field, stripped from player builds
    [SerializeField] private int _id;

    public int Id => _id;
}
```

The generator reports `AFID001` if the struct is missing `partial`, and `AFID002` if your code already declares `_id`, `Id`, or `__stringId` (the generator skips emission so you get a clear error pointing at the struct rather than a CS compile error inside generated source). Generic targets (`EnemyId<T>`) and generic containing types are supported.

**2.** Create the registry asset and bind it to the struct type in its Inspector:
- `Assets → Create → Aspid → Id Registry`

**3.** Use the struct as a serialized field. The Inspector shows a dropdown of registered names; the selector window also lets you create new entries on the fly:

```csharp
using UnityEngine;
using Aspid.FastTools.Ids;

[CreateAssetMenu]
public class EnemyDefinition : ScriptableObject
{
    [UniqueId] [SerializeField] private EnemyId _id;
}
```

```csharp
using UnityEngine;
using Aspid.FastTools.Ids;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyId _targetEnemy;

    private void Spawn()
    {
        int id = _targetEnemy.Id; // stable integer, safe for switch / Dictionary
    }
}
```

![aspid_fasttools_id_selector.gif](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_id_selector.gif)

### UniqueIdAttribute

Marks a field as requiring a unique value across all assets of the declaring type. The Inspector shows a warning if two assets share the same ID.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class UniqueIdAttribute : PropertyAttribute { }
```

![aspid_fasttools_id_collision.gif](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_id_collision.gif)

### IdRegistry

`ScriptableObject` in `Aspid.FastTools.Ids` that stores `(int, string)` entries and keeps the lookup tables available at runtime. Each name is assigned a stable, auto-incrementing ID that never changes when other entries are added or removed.

| Member | Description |
|--------|-------------|
| `bool TryGetId(string name, out int id)` | Returns `true` and the ID when found; otherwise `false` |
| `bool TryGetName(int id, out string name)` | Returns `true` and the name when found; otherwise `false` and `string.Empty` |
| `bool Contains(int id)` | Whether an ID is registered |
| `bool Contains(string name)` | Whether a name is registered |
| `int Count` | Number of entries |
| `IReadOnlyList<int> Ids` · `IReadOnlyList<string> IdNames` | Registered IDs / names, in registration order |
| `IEnumerator<KeyValuePair<int, string>> GetEnumerator()` | Iterate `(id, name)` pairs |

The registry derives from `ScriptableObject` directly and exposes a generic counterpart `IdRegistry<T>` (with `T : struct, IId`) that adds typed `Contains(T)` and `TryGetName(T, out string)` overloads. Edits — adding, renaming, removing entries — happen through the registry inspector and `RegistryEditorCore`, not via a public runtime API.

![aspid_fasttools_id_registry.png](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_id_registry.png)

---

## VisualElement Extensions

Fluent extension methods for building UIToolkit trees in code. All methods return `T` (the element itself) for chaining.

> Full method-by-method reference: [VisualElementExtensions.md](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/VisualElementExtensions.md)

### Example

A reactive editor for an `AbilityConfig` `ScriptableObject` — title and status pill in the header, `PropertyField` body, and a Warning `HelpBox` that toggles based on `ManaCost`.

```csharp
[CustomEditor(typeof(AbilityConfig))]
internal sealed class AbilityConfigEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var config = (AbilityConfig)target;

        var badge = new Label()
            .SetFontSize(10).SetUnityFontStyleAndWeight(FontStyle.Bold)
            .SetPaddingX(10).SetPaddingY(3)
            .SetBorderRadius(10).SetBorderWidth(1);

        var helpBox = new HelpBox(
                "This ability costs no mana — is that intentional?",
                HelpBoxMessageType.Warning)
            .SetMarginTop(8).SetBorderRadius(6);

        var manaField = new PropertyField(serializedObject.FindProperty("_manaCost"))
            .AddValueChanged(_ => Refresh());

        Refresh();
        return new VisualElement()
            .SetBorderRadius(10).SetBorderWidth(1)
            .AddChild(new VisualElement()
                .SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                .SetPaddingX(14).SetPaddingY(12)
                .AddChild(new Label(target.GetScriptName())
                    .SetFlexGrow(1).SetFontSize(15)
                    .SetUnityFontStyleAndWeight(FontStyle.Bold))
                .AddChild(badge))
            .AddChild(new VisualElement()
                .SetPaddingX(14).SetPaddingY(12)
                .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_description")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
                .AddChild(manaField)
                .AddChild(helpBox));

        void Refresh()
        {
            var isFree = config.ManaCost is 0;
            badge.SetText(isFree ? "FREE" : $"{config.ManaCost} MP");
            helpBox.SetDisplay(isFree ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}
```

> The complete sample — `AbilityConfig.cs`, the polished `AbilityConfigEditor.cs` (custom colors, subtitle and divider, used in the screenshot below) and two `.asset` examples — ships in the `VisualElements` sample (Package Manager → Aspid.FastTools → Samples).

### Result

![aspid_fasttools_visual_element.gif](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_visual_element.gif)

---

## SerializedProperty Extensions

Chainable extensions on `SerializedProperty` for synchronizing the owning `SerializedObject`, writing typed values, and reflecting on the underlying field.

```csharp
property
    .Update()
    .SetVector3(Vector3.up)
    .SetBool(true)
    .ApplyModifiedProperties();
```

The package covers:

- **Update / Apply** — `Update`, `UpdateIfRequiredOrScript`, `ApplyModifiedProperties`.
- **Typed setters** — `SetValue` (generic dispatch) and `SetXxx` for `int`/`uint`/`long`/`ulong`/`float`/`double`/`bool`/`string`/`Color`/`Gradient`/`Hash128`/`Rect`/`RectInt`/`Bounds`/`BoundsInt`/`Vector2..4` (and `Vector2/3Int`)/`Quaternion`/`AnimationCurve`/`EntityId` (Unity 6.2+). Each comes with a paired `SetXxxAndApply` variant.
- **Enum setters** — `SetEnumFlag` and `SetEnumIndex` (each + `AndApply`).
- **Arrays** — `SetArraySize`, `AddArraySize`, `RemoveArraySize` (each + `AndApply`).
- **References** — `SetManagedReference`, `SetObjectReference`, `SetExposedReference`, and `SetBoxed` (Unity 6+).
- **Reflection helpers** — `GetPropertyType`, `GetMemberInfo`, `GetClassInstance` for resolving the C# member and runtime instance behind a property.

> Full method-by-method reference: [SerializedPropertyExtensions.md](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/SerializedPropertyExtensions.md)

---

## IMGUI Layout Scopes

Three `ref struct` scopes — `VerticalScope`, `HorizontalScope`, `ScrollViewScope` — wrap `EditorGUILayout.Begin*` / `End*`. Each exposes a `Rect` property and calls the matching `End*` method on `Dispose`:

```csharp
using (VerticalScope.Begin())
{
    EditorGUILayout.LabelField("Item 1");
    EditorGUILayout.LabelField("Item 2");
}

using (HorizontalScope.Begin())
{
    EditorGUILayout.LabelField("Left");
    EditorGUILayout.LabelField("Right");
}

var scrollPos = Vector2.zero;
using (ScrollViewScope.Begin(ref scrollPos))
{
    EditorGUILayout.LabelField("Scrollable content");
}
```

Capture the group rect with the `out`-overload when needed:

```csharp
using (VerticalScope.Begin(out var rect, GUI.skin.box))
{
    EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.1f));
    EditorGUILayout.LabelField("Boxed content");
}
```

All `Begin` overloads match the corresponding `EditorGUILayout.Begin*` signatures (optional `GUIStyle`, `GUILayoutOption[]`, scroll view options, etc.).

---

## Editor Helper Extensions

Utility methods for getting display names of Unity objects in custom editors.

```csharp
public static string GetScriptName(this Object obj)
```

Returns the display name of a Unity object:
- If the type has `[AddComponentMenu]`, returns `ObjectNames.GetInspectorTitle(obj)`
- Otherwise returns `ObjectNames.NicifyVariableName(typeName)`

```csharp
public static string GetScriptNameWithIndex(this Component targetComponent)
```

Returns the display name with a count suffix when multiple components of the same type exist on the same GameObject. For example, if two `AudioSource` components are attached, the second returns `"Audio Source (2)"`.

```csharp
[CustomEditor(typeof(MyBehaviour))]
public class MyBehaviourEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // "My Behaviour" — or "Custom Name" if [AddComponentMenu("Custom Name")] is present
        var name = target.GetScriptName();

        // "My Behaviour (2)" when a second component of the same type exists
        var nameWithIndex = ((Component)target).GetScriptNameWithIndex();

        return new Label(name);
    }
}
```
