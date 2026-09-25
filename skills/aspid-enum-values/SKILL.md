---
name: aspid-enum-values
description: "Aspid.FastTools EnumValues<TEnum, TValue> and EnumValues<TValue>: a serializable enum-to-value table edited in the Unity Inspector. Use when mapping enum members to per-member data (multipliers, colors, sounds, configs), replacing an enum switch or a set of per-member fields, building a [Flags] lookup, or reading/iterating/debugging an existing EnumValues field."
---

# EnumValues

Namespace `Aspid.FastTools.Enums`. If the user's scripts have an `.asmdef`, it must reference `Aspid.FastTools`.

A read-only table of rows (enum member -> value) plus a **Default Value** returned when no row matches.
Data comes only from Unity serialization: there is no `Add`, `Remove`, indexer or setter.

```csharp
using Aspid.FastTools.Enums;
using UnityEngine;

public sealed class DamageConfig : ScriptableObject
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers;

    public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
}
```

- Default to `EnumValues<TEnum, TValue>` (typed, no boxing). Use `EnumValues<TValue>` (`GetValue(Enum)`) only when
  the asset author must pick the enum in the Inspector. Switching a field between the two keeps its data when the
  selected enum equals `TEnum`.
- Rows are **not** created automatically. After adding a field, tell the user to set Default Value and add rows by
  hand or via right-click on the property -> **Populate Missing Enum Members**. Only members whose value differs from
  the default need a row.
- `foreach (var (key, value) in table)` yields rows in list order (struct enumerator, no allocation), never the
  default, and skips rows whose key no longer resolves.

## Lookup rules

- First row (in list order) with the same numeric value wins; a found row wins even if its value is `null`/`0`.
- `[Flags]`: an exact row wins; otherwise the **first row in list order** whose bits are all contained in the lookup
  value (not the most specific one - put combination rows above single flags); otherwise Default Value.
  Zero matches only zero.
- `EnumValues<TValue>`: a `null` key or a key of another enum type returns Default Value.
- `table.Equals(lookup, storedKey)` is this matching rule, asymmetric for `[Flags]` (`lookup` must contain all bits
  of `storedKey`). It is not equality - use `==` for that.

## Pitfalls

- Keys are stored by member **name**. Reordering or renumbering members is safe. Renaming or deleting one logs
  `Couldn't parse key ...` and the row is skipped; drawing that row in the Inspector silently rewrites its key to the
  enum's first member. When renaming enum members, tell the user to fix the affected rows.
- A new member returns Default Value until a row is added.
- `EnumValues<TValue>` with no enum selected, or with a type that no longer resolves, logs a warning/error and always
  returns Default Value.
