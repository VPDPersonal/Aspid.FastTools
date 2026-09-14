# EnumValues

An enum-keyed table you can configure in the Inspector for damage multipliers, colours, sounds, or asset references. `GetValue` returns the matching row's value, or the configured `Default Value` when no row matches.

## Quick start

Add `using Aspid.FastTools.Enums;` to a script that imports `UnityEngine`. The examples use this enum:

```csharp
public enum DamageType
{
    Physical, Fire, Ice, Poison
}
```

Inside a component, the table replaces separate serialized fields and a `switch`:

| Before — separate fields and switch | After — EnumValues |
|---|---|
| <pre lang="csharp"><code>[SerializeField]<br />private float _defaultMultiplier = 1f;<br />[SerializeField]<br />private float _fireMultiplier = 1.5f;<br /><br />public float GetMultiplier(<br />    DamageType type) =&gt; type switch<br />&#123;<br />    DamageType.Fire =&gt; _fireMultiplier,<br />    _ =&gt; _defaultMultiplier<br />&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]<br />private EnumValues&lt;DamageType, float&gt;<br />    _multipliers = new();<br /><br />public float GetMultiplier(<br />    DamageType type) =&gt;<br />    _multipliers.GetValue(type);</code></pre> |

For the same result, set `_multipliers` to **Default Value = 1** and add **Fire = 1.5**. The values on the right are configured in the Inspector; `new()` creates an empty table with an initial default of `0` for `float`.

| Call | Result with these settings |
|---|---|
| `_multipliers.GetValue(DamageType.Fire)` | `1.5` — the `Fire` row's value |
| `_multipliers.GetValue(DamageType.Ice)` | `1` — no `Ice` row exists |

<details>
<summary>Complete example: a damage multiplier component</summary>

Create `DamageDealer.cs`, add the component to a GameObject, and configure `Multipliers`:

```csharp
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType
{
    Physical, Fire, Ice, Poison
}

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers = new();

    public float CalculateDamage(DamageType type, float baseDamage) =>
        baseDamage * _multipliers.GetValue(type);

    [ContextMenu("Log Fire Damage")]
    private void LogFireDamage() =>
        Debug.Log(CalculateDamage(DamageType.Fire, 10f), this);
}
```

Set **Default Value = 1**, add **Fire = 1.5**, and choose **Log Fire Damage** from the component's context menu. The Console prints `15`.

If `DamageType` is already declared in your project, use the existing enum.

</details>

## Inspector setup

In `Multipliers`, the key type is `DamageType`, and each row holds a `float`:

1. Expand the table and set **Default Value** for keys without their own row.
2. Add rows manually, or right-click the property and choose **Populate Missing Enum Members**.
3. Configure the new rows, for example **Fire = 1.5**.

You do not need a row for every enum member. A table containing only overrides works: all other keys use `Default Value`.

### Populate Missing Enum Members

The command appends missing **names declared in the enum** and copies the current `Default Value` into each new row. Existing rows and their values remain unchanged. The operation supports Undo.

For `[Flags]`, it adds declared members, including named combinations. It does not generate every possible bit combination. The menu item is disabled when no names are missing.

Changing `Default Value` after populating does not replace existing row values. For example, an `Ice` row created with `1` still returns `1` if you later change the default to `2`.

## Choosing a variant

| Task | Field type | Enum selection in the Inspector |
|---|---|---|
| The enum is known in code | `EnumValues<TEnum, TValue>` | Fixed by `TEnum`; the type field is read-only |
| Asset authors should choose the enum | `EnumValues<TValue>` | Available in the table header |

Both variants support a default value, `[Flags]`, and enumeration. `TValue` must be supported by Unity serialization, such as `float`, `Color`, `AudioClip`, or your own serializable class.

### EnumValues\<TEnum, TValue\>

Use the typed variant when all lookups use one known enum. The compiler checks the key type:

```csharp
[SerializeField] private EnumValues<DamageType, Color> _colors = new();

public Color GetColor(DamageType type) => _colors.GetValue(type);
```

Lookup compares the keys' numeric values. After table initialization, the typed `GetValue` does not box the key into `object`.

### EnumValues\<TValue\>

Here, code specifies only the value type; the enum is selected in the Inspector:

```csharp
[SerializeField] private EnumValues<float> _multipliers = new();

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

For this example, select **DamageType** in the table header. The method accepts `System.Enum`, so the compiler allows other enum types; lookup checks their type at runtime. A key from a different enum returns `Default Value`, even if its numeric value matches.

If no enum type is selected, the table returns `Default Value` and logs a warning during initialization. If the stored type cannot be resolved, it logs an error and also returns the default.

## How GetValue works

For an ordinary enum, lookup returns the first row whose key has the exact numeric value. If there is no such row, it returns `Default Value`.

| Situation | Result |
|---|---|
| Key found | Its row's value |
| Empty table or missing key | `Default Value` |
| The matching row contains `0`, `false`, or `null` | That exact value; lookup does not fall back to `Default Value` |
| Several rows have the same numeric key | The first row |
| Different enum names share a numeric value | They are the same key for lookup |

The table is scanned sequentially, rather than accessed as a dictionary. For `[Flags]`, a second pass follows exact lookup as described below.

## Flag matching

Declare a `[Flags]` enum and a table field:

```csharp
[System.Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1,
    Slowed = 2,
    Frozen = 4
}
```

```csharp
[SerializeField]
private EnumValues<StatusEffect, float> _speedMultipliers = new();
```

Lookup proceeds in this order:

1. An **exact match** for the entire requested value.
2. Otherwise, the **first row whose bits are all present in the request**.
3. If no row matches, **Default Value**.

The value `0` matches only `0`; it is not a catch-all for other flags.

### Exact and partial matches

Suppose `Default Value` is `1`, and the rows are ordered as follows:

| Key | Value |
|---|---|
| `Burning` | `0.9` |
| `Slowed` | `0.5` |
| `Burning \| Slowed` | `0.3` |
| `None` | `1` |

| GetValue argument | Result | Reason |
|---|---|---|
| `Burning \| Slowed` | `0.3` | The exact row wins, even though it follows the individual flags |
| `Burning \| Frozen` | `0.9` | No exact row; `Burning` is the first match |
| `Frozen` | `1` | No matching row |
| `None` | `1` | The exact zero row |

For example:

```csharp
var multiplier = _speedMultipliers.GetValue(
    StatusEffect.Burning | StatusEffect.Slowed); // 0.3
```

### When row order matters

There is no exact row for `Burning | Slowed | Frozen`. `Burning` matches first, so the result is `0.9`. Moving the combined `Burning | Slowed` row above it changes the result to `0.3`.

Lookup chooses **one value**. It does not add or multiply matching values, or automatically select the combination with the most bits.

## Checking keys with Equals

`Equals(first, second)` checks keys using the same matching rules without reading row values. For ordinary enums, this is exact equality. For `[Flags]`, it checks whether the **first argument contains every bit of the second**, with a separate rule for zero:

```csharp
var combined = StatusEffect.Burning | StatusEffect.Slowed;

_speedMultipliers.Equals(combined, StatusEffect.Burning); // true
_speedMultipliers.Equals(StatusEffect.Burning, combined); // false
_speedMultipliers.Equals(combined, StatusEffect.None);    // false
_speedMultipliers.Equals(StatusEffect.None, StatusEffect.None); // true
```

For flags, argument order matters. Use the normal `==` operator for strict enum equality. In the untyped `EnumValues<TValue>`, both arguments must belong to the selected enum; otherwise the result is `false`.

## Enumerating rows

A direct `foreach` returns configured rows in list order:

```csharp
foreach (var entry in _multipliers)
{
    Debug.Log($"{entry.Key}: {entry.Value}");
}
```

Enumeration does not include `Default Value`. Keys that cannot be parsed are skipped. The typed table returns `TEnum` keys; the untyped table returns `System.Enum` keys.

Both variants use a struct enumerator. After initialization, a direct `foreach` does not allocate an enumerator; enumeration through `IEnumerable` boxes it. Code inside the loop, such as string interpolation for `Debug.Log`, may allocate separately.

## Changing the table and enum

Table values are configured through Unity serialization. The public API supports reading, key checks, and enumeration; it has no `Add`, `Remove`, or writable indexer.

Keys are stored as strings containing enum member names. A new member uses `Default Value` until a row is added; **Populate Missing Enum Members** helps fill those gaps. Renaming or deleting an existing member requires reviewing and reassigning stored keys.

## Practical example

In the [EnumValues sample](../Samples~/EnumValues/Documentation/README.md), the surface type controls tile and footprint colours, while terrain flags control the character's speed multiplier. `SurfacePalette.asset` contains two typed `EnumValues<SurfaceType, Color>` tables:

![Surface and footprint colour tables with separate Default Values](../Samples~/EnumValues/Documentation/Images/surface-tables.png)

Surface and footprint colour tables with separate Default Values

This is how `SurfacePalette` reads a tile colour from the table shown above:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

public Color GetTileColor(SurfaceType surface) =>
    _tileColors.GetValue(surface);
```

Import the sample and open `Scenes/EnumValues.unity`. Change the `Grass` colour in `Data/SurfacePalette.asset` to recolour the tiles. Then remove the `Stone` row from `Footprint Colors` to see `Default Value` take effect. Setup instructions and flag experiments are in the [sample documentation](../Samples~/EnumValues/Documentation/README.md).
