# EnumValues&lt;TValue&gt; — Step-by-Step Tutorial

`EnumValues<TValue>` is a serializable dictionary that maps members of any enum to values of any serializable
type, configured in the Inspector. Each lesson maps to one `STEP` section of the `EnumValuesTutorial` component.

Declare a field with only the value type — the enum type is picked in the Inspector:

```csharp
// Which enum? Decided in the Inspector.
[SerializeField] private EnumValues<float> _multipliers;
```

When the enum *is* known at compile time, use the typed twin `EnumValues<TEnum, TValue>` instead (Lesson 5).

## Open the tutorial

1. Open the Welcome window (**Tools → Aspid 🐍 → FastTools → Welcome**) and import the **EnumValues** sample.
2. Open **`Scenes/EnumValuesTutorial.unity`** and select the **EnumValues Tutorial** GameObject.

---

## Lesson 1 — Your first mapping

**Field:** `EnumValues<float> _step1Multipliers` — empty.

1. Click the type field in the header and pick **`DamageType`** in the type picker.
2. Press **+** on the list. A row appears: a **key dropdown** plus a **float value**.
3. Add a few rows, e.g. `Physical = 1.0`, `Fire = 1.5`.

- Until the enum type is picked, the field warns and every lookup returns the default value.
- Duplicate keys are allowed by the Inspector, but only the **first** row with a given key wins a lookup.
- The value side is an ordinary serialized field — any serializable type works (`Color`, `Sprite`, a custom
  `[Serializable]` class…).

---

## Lesson 2 — Populate missing members & the default value

**Field:** `EnumValues<Color> _step2Colors` — the enum type is pre-set to `DamageType`, the list is empty.

1. **Right-click** the field and choose **Populate Missing Enum Members** — one row per member appears, seeded
   with the **Default Value** (white).
2. Recolor a few: `Fire` orange, `Ice` cyan.
3. Right-click again — the menu item is now greyed out: nothing is missing.

- **Default Value** (the last field) is both the seed for populated rows and the fallback `GetValue` returns
  when no entry matches. You do **not** need a row per member — map the exceptions, let the default cover the rest.
- Populate only appends missing rows; existing rows are untouched.

---

## Lesson 3 — `[Flags]` keys and the lookup rules

**Field:** `EnumValues<float> _step3SpeedByStatus` — pre-filled, keyed on the `[Flags]` enum `StatusEffect`.

Each key renders as a **multi-select flags dropdown**; a combination like `Burning | Slowed` is a regular
entry of its own. The pre-filled list:

| # | Key | Value |
|---|---|---|
| 0 | `Burning` | `0.9` |
| 1 | `Frozen` | `0.2` |
| 2 | `Slowed` | `0.5` |
| 3 | `Burning \| Slowed` | `0.4` |

`GetValue` resolves a `[Flags]` lookup in three steps:

1. **Exact match wins first, regardless of order** — a lookup of exactly `Burning | Slowed` returns `0.4`
   even though that entry is last.
2. **No exact match → the first contained entry wins** — `Burning | Frozen | Slowed` has no exact entry, so
   the first row whose flags are all contained in the value wins: `Burning`, `0.9`.
3. **Nothing matches → Default Value** — `Stunned` has no entry: `1.0`. `None` (zero) only ever matches a
   `None` entry.

---

## Lesson 4 — Reading values from code

**Fields:** `DamageType _step4DamageType`, `StatusEffect _step4Effects` — the lookup keys.

```csharp
float multiplier = _step1Multipliers.GetValue(_step4DamageType);
float speed      = _step3SpeedByStatus.GetValue(_step4Effects);
```

Right-click the component header → **Log Tutorial Lookups** (works in Edit Mode). Change `_step4Effects` to
exercise the three rules from Lesson 3: `Burning | Frozen | Slowed` → `0.90`, `Stunned` or `None` → `1.00`.

- `GetValue` takes any value of the configured enum — no per-key registration in code.
- `foreach` over an `EnumValues` yields the configured `(key, value)` pairs in list order; the default value
  is **not** part of the iteration.

---

## Lesson 5 — The typed variant: `EnumValues<TEnum, TValue>`

**Field:** `EnumValues<DamageType, float> _step5TypedMultipliers` — pre-filled.

When the enum type is known at compile time, declare it in the field instead of picking it in the Inspector:

```csharp
// The enum is fixed at compile time.
[SerializeField] private EnumValues<DamageType, float> _multipliers;

// GetValue takes a DamageType, not a boxed Enum.
float m = _multipliers.GetValue(DamageType.Fire);
```

- The type-picker row in the header is **disabled** — the enum is fixed by the field declaration, and
  `GetValue` is compile-time safe. The open-script button next to it stays active.
- Everything from Lessons 1–4 carries over: *Populate Missing Enum Members*, the Default Value fallback, the
  `[Flags]` lookup rules, `foreach` over entries.
- The serialized layout matches `EnumValues<TValue>`: switching a field between the two variants (same enum
  configured) keeps the entries.
- **Rule of thumb:** enum known up front → `EnumValues<TEnum, TValue>`; enum chosen per-asset in the
  Inspector → `EnumValues<TValue>`.

---

## When the enum changes

The keys are stored as **name strings**, so:

- **Renamed / deleted member** — the stored key no longer parses. The Inspector migrates the row to the first
  member of the enum; at runtime an unresolved key logs an error and simply never matches (no crash).
- **Added member** — the new member just has no row (→ default value) until you add one, e.g. via
  *Populate Missing Enum Members*.
- **Switching the enum type** on a filled list keeps the rows and their values and re-parses each key against
  the new type — a key whose name exists under both types survives; the rest are migrated (and saved) as the
  new type's first member, so the original key names do **not** come back if you switch the type back.

> For a rename that must keep data, rename the serialized strings in the asset/scene file (or re-pick the key
> in the Inspector) — the value side of the row is never dropped by a key migration.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Tutorial/EnumValuesTutorial.cs` | All five lessons as numbered fields + the `Log Tutorial Lookups` context menu |
| `Scripts/DamageDealer.cs` | Both variants used by a real component (the `EnumValues.unity` demo scene) |
| `Scripts/DamageType.cs` / `Scripts/StatusEffect.cs` | A plain enum and a `[Flags]` enum used as keys |
| [README.md](README.md) | The compact demo-scene walkthrough and the lookup-rules summary |
