# EnumValues

An enum-keyed table configured in the Inspector: damage multipliers, colours, sounds, asset references. `GetValue` returns the matching row's value, or `Default Value` when no row matches.

## Quick start

Add `using Aspid.FastTools.Enums;` to a script that imports `UnityEngine`. The examples use this enum:

```csharp
public enum DamageType
{
    Physical, Fire, Ice, Poison
}
```

One table replaces a set of serialized fields and a `switch`:

| Before — separate fields and switch | After — EnumValues |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private float _defaultMultiplier = 1f;&#10;[SerializeField]&#10;private float _fireMultiplier = 1.5f;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt; type switch&#10;&#123;&#10;    DamageType.Fire =&gt; _fireMultiplier,&#10;    _ =&gt; _defaultMultiplier&#10;&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]&#10;private EnumValues&lt;DamageType, float&gt;&#10;    _multipliers;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt;&#10;    _multipliers.GetValue(type);</code></pre> |

In the Inspector, set `Multipliers` to **Default Value = 1** and add a **Fire = 1.5** row:

| Call | Result |
|---|---|
| `_multipliers.GetValue(DamageType.Fire)` | `1.5` — the `Fire` row's value |
| `_multipliers.GetValue(DamageType.Ice)` | `1` — no `Ice` row, so `Default Value` is returned |

<details>
<summary>Complete example: a damage multiplier component</summary>

Create `DamageDealer.cs` and add the component to a GameObject:

```csharp
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType
{
    Physical, Fire, Ice, Poison
}

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers;

    public float CalculateDamage(DamageType type, float baseDamage) =>
        baseDamage * _multipliers.GetValue(type);

    [ContextMenu("Log Fire Damage")]
    private void LogFireDamage() =>
        Debug.Log(CalculateDamage(DamageType.Fire, 10f), this);
}
```

Set **Default Value = 1**, add a **Fire = 1.5** row and choose **Log Fire Damage** from the component's context menu. The Console prints `15`. If `DamageType` already exists in your project, use the existing enum.

</details>

## Inspector setup

Two `EnumValues<SurfaceType, Color>` tables from the [EnumValues sample](../Samples~/EnumValues/Documentation/README.md) look like this:

![Surface and footprint colour tables with separate Default Values](../Samples~/EnumValues/Documentation/Images/surface-tables.png)

Surface and footprint colour tables with separate Default Values

1. Expand the table and set **Default Value** — keys without a row of their own receive it.
2. Add rows by hand, or right-click the property and choose **Populate Missing Enum Members**.
3. Configure the values of the added rows.

Filling in every enum member is optional: a table with only the differing values works.

### Populate Missing Enum Members

The command appends the members **declared in the enum** that the table lacks and writes the current `Default Value` into the new rows. Existing rows stay untouched, and the operation is undoable. When no members are missing, the menu item is disabled.

For `[Flags]` it adds only declared members, including named combinations. It does not generate every possible bit combination.

> [!NOTE]
> `Default Value` is copied into the rows once, when they are populated. If you later change it to `2`, an `Ice` row created with `1` keeps returning `1`.

## Choosing a variant

| Task | Field type | Enum choice in the Inspector |
|---|---|---|
| The enum is known in code | `EnumValues<TEnum, TValue>` | Fixed by the `TEnum` argument; the type field is read-only |
| The asset author picks the enum | `EnumValues<TValue>` | Available in the table header |

Both variants support `Default Value`, `[Flags]` and row enumeration. `TValue` is any type Unity serializes: `float`, `Color`, `AudioClip`, your own `[Serializable]` class.

### EnumValues\<TEnum, TValue\>

The typed variant for tables that are always accessed with one known enum. The compiler checks the key type, and `GetValue` does not box the key:

```csharp
[SerializeField] private EnumValues<DamageType, Color> _colors;

public Color GetColor(DamageType type) => _colors.GetValue(type);
```

### EnumValues\<TValue\>

Only the value type is fixed in code; the enum is chosen in the Inspector:

```csharp
[SerializeField] private EnumValues<float> _multipliers;

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

For this example, select **DamageType** in the table header. `GetValue` accepts `System.Enum`, so the compiler lets a key from another enum through — it returns `Default Value` even when the numeric value happens to match.

> [!IMPORTANT]
> When no enum is selected, the table returns `Default Value` and logs a warning to the Console once. When the stored type is no longer found in the project, for example after a rename, it logs an error instead.

## Lookup rules

The table is scanned top to bottom. For a regular enum the first row with the same numeric key wins, otherwise the result is `Default Value`:

| Situation | Result |
|---|---|
| Key found | The row's value, including `0`, `false` or `null` |
| Key missing or table empty | `Default Value` |
| Several rows with the same numeric key | The first of them |
| Different enum names with the same numeric value | One key for lookup purposes |

### Flags

For a `[Flags]` enum, a second pass follows the exact search:

1. **Exact match** with the whole requested value.
2. Otherwise — the **first row whose bits are all contained** in the request.
3. Otherwise — `Default Value`.

Zero matches only zero; it is not an "empty mask" that matches the other flags. Example:

```csharp
[System.Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1,
    Slowed = 2,
    Frozen = 4
}

[SerializeField] private EnumValues<StatusEffect, float> _speedMultipliers;
```

`Default Value` is `1` and the rows are in this order:

| Key | Value |
|---|---|
| `Burning` | `0.9` |
| `Slowed` | `0.5` |
| `Burning \| Slowed` | `0.3` |
| `None` | `1` |

| GetValue argument | Result | Reason |
|---|---|---|
| `Burning \| Slowed` | `0.3` | The exact row wins even though it sits below the single flags |
| `Burning \| Frozen` | `0.9` | No exact row; the first matching row is `Burning` |
| `Burning \| Slowed \| Frozen` | `0.9` | No exact row; `Burning` sits above `Burning \| Slowed` |
| `Frozen` | `1` | No matching row |
| `None` | `1` | The exact zero row |

> [!NOTE]
> The lookup returns **one** value and does not combine matching rows. If `Burning | Slowed | Frozen` should yield `0.3`, move the `Burning | Slowed` row above `Burning`.

## Checking keys with Equals

`Equals(first, second)` compares keys by the same rules without reading row values. For a regular enum this is numeric equality. For `[Flags]` it checks whether the **first argument contains all bits of the second**; zero equals only zero:

```csharp
var combined = StatusEffect.Burning | StatusEffect.Slowed;

_speedMultipliers.Equals(combined, StatusEffect.Burning);        // true
_speedMultipliers.Equals(StatusEffect.Burning, combined);        // false
_speedMultipliers.Equals(combined, StatusEffect.None);           // false
_speedMultipliers.Equals(StatusEffect.None, StatusEffect.None);  // true
```

Use `==` for strict enum equality. In `EnumValues<TValue>` both arguments must belong to the selected enum, otherwise the result is `false`.

## Enumerating rows

`foreach` yields the configured rows in list order. `Default Value` and rows with an unresolved key are not included:

```csharp
foreach (var (type, multiplier) in _multipliers)
{
    Debug.Log($"{type}: {multiplier}");
}
```

The typed table yields `TEnum` keys, the generic one `System.Enum`. A direct `foreach` uses a struct enumerator and does not allocate; enumerating through the `IEnumerable` interface, for example in LINQ, boxes it.

## Changing the table and enum

The public API only reads the table: there is no `Add`, `Remove` or writable indexer, and values are set through Unity serialization.

Keys are stored by member **name**, so reordering members and changing their numeric values is safe. A new member returns `Default Value` until a row is added — **Populate Missing Enum Members** fills such gaps. A renamed or deleted member is no longer recognised: its row is skipped by lookup and enumeration, so review such keys in the Inspector.

## Practical example

In the [EnumValues sample](../Samples~/EnumValues/Documentation/README.md), the surface type sets tile and footprint colours through `EnumValues<SurfaceType, Color>`, while terrain flags set the character's speed multiplier through an `EnumValues<float>` with `TerrainFlags` selected in the Inspector:

![The character walks across different surfaces and leaves a continuous coloured trail.](../Samples~/EnumValues/Documentation/Images/demo.gif)

The character walks across different surfaces and leaves a continuous coloured trail.

Import the sample and open `Scenes/EnumValues.unity`. Change the `Grass` colour in `Data/SurfacePalette.asset` — the tiles recolour without Play Mode. Remove the `Stone` row from **Footprint Colors** to see `Default Value` in action. Flag and enumeration experiments are described in the [sample documentation](../Samples~/EnumValues/Documentation/README.md#try).
