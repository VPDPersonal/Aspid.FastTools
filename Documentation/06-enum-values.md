# EnumValues

Serializable enum-to-value mappings configurable from the Inspector.

## EnumValues\<TValue\>

A serializable collection of `EnumValue<TValue>` entries with a configurable default value. Implements `IEnumerable<KeyValuePair<Enum, TValue>>`.

`GetValue` returns the mapped value, falling back to the configured default when the key is missing. `[Flags]` enums are supported: matching uses `HasFlag` and treats `0`-valued members correctly.

```csharp
using System;
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType { Physical, Fire, Ice, Poison }

[Flags]
public enum StatusEffect { None = 0, Burning = 1, Frozen = 2, Slowed = 4, Stunned = 8 }

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<float> _damageMultipliers;

    // Flag combinations (e.g. Burning | Slowed) match via HasFlag and first-hit wins,
    // so list composite entries BEFORE their constituent flags.
    [SerializeField] private EnumValues<float> _speedMultipliersByStatus;

    public float GetMultiplier(DamageType type) => _damageMultipliers.GetValue(type);

    public float GetSpeedModifier(StatusEffect effects) => _speedMultipliersByStatus.GetValue(effects);
}
```

![EnumValues in the Inspector](Images/aspid_fasttools_enum_values.png)

In the Inspector, select the enum type in the `EnumValues` header, then assign a value for each enum member. Right-click the property to open a context menu with **Populate Missing Enum Members** — it appends an entry for every enum member not yet in the list, seeded with the current Default Value.

## EnumValues\<TEnum, TValue\>

The typed counterpart of `EnumValues<TValue>` for the common case where the enum type is already known in code. The enum is fixed by the generic argument, so the Inspector's type picker is disabled and lookups are compile-time safe. Lookups are also boxing-free — keys are compared as cached numeric values — and `foreach` over either variant binds to a struct enumerator, so iteration does not allocate. Implements `IEnumerable<KeyValuePair<TEnum, TValue>>`.

```csharp
public sealed class HitEffect : MonoBehaviour
{
    // The type picker in the Inspector is disabled — the enum is fixed to DamageType.
    [SerializeField] private EnumValues<DamageType, Color> _damageColors;

    public Color GetColor(DamageType type) => _damageColors.GetValue(type);
}
```

Lookup semantics (including `[Flags]` handling) are identical to `EnumValues<TValue>`.
