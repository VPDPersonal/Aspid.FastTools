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

- Default to `EnumValues<TEnum, TValue>` (typed, the key is checked at compile time). Use `EnumValues<TValue>` only
  when the asset author must pick the enum in the Inspector; `GetValue(type)` with a typed enum argument binds to
  `GetValue<TEnum>` and does not box the key, while a `System.Enum` variable binds to `GetValue(Enum)`. Switching a
  field between the two keeps its data when the selected enum equals `TEnum`.
- Rows are **not** created automatically. After adding a field, tell the user to set Default Value and add rows by
  hand or via right-click on the table header -> **Populate Missing Enum Members**. For enums without `[Flags]`,
  only members whose value differs from the default need a row. A flags row set to Default Value can override a partial match.
- `foreach (var (key, value) in table)` yields rows in list order (struct enumerator, no allocation), never the
  default, and skips rows whose key no longer resolves. `Count` is the number of rows it yields.
- `table.TryGetValue(key, out var value)` returns `true` when a row matches. `value` is the row's value, or Default
  Value on a miss, so it tells a missing row from a row equal to Default Value. `EnumValues<TValue>` also has
  `TryGetValue<TEnum>`.

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
  `Couldn't parse key ...` and the row is skipped; the Inspector shows it with its key (`<Missing Frozen>`) and keeps
  the key until a member is picked (also after switching `EnumValues<TValue>` to another enum). When renaming enum
  members, tell the user to fix the affected rows.
- A new member returns Default Value until a row is added. A row added to an empty table has an empty key (`<None>`)
  and logs `Couldn't parse key ...` until a member is picked.
- `EnumValues<TValue>` with no enum selected, or with a type that no longer resolves, logs a warning/error and always
  returns Default Value. In a player the enum is found by its stored name, so Managed Stripping Level Low or higher
  can strip an enum that is used only through this selection: keep it with `[Preserve]` or `link.xml`.
- Lookups, `Count` and `foreach` may run on any thread, including the first access that initializes the rows.
