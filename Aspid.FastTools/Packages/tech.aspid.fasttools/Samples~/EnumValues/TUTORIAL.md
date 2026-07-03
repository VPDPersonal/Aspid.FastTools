# EnumValues&lt;TValue&gt; — Step-by-Step Tutorial

A guided, hands-on tour of `EnumValues<TValue>` — a serializable dictionary that maps members of any enum to
values of any serializable type, configured entirely in the Inspector. Work through the lessons in order: each
one builds on the previous and maps to one `STEP` section of the `EnumValuesTutorial` component.

**The one rule:** declare a field of `EnumValues<TValue>` where `TValue` is the value type; the enum type is
*not* part of the field declaration — you pick it in the Inspector.

```csharp
[SerializeField] private EnumValues<float> _multipliers;   // which enum? decided in the Inspector
```

When the enum *is* known at compile time, there is a typed twin — `EnumValues<TEnum, TValue>` — that skips the
Inspector type-picker entirely. Lessons 1–4 teach the untyped variant; Lesson 5 introduces the typed one.

## Open the tutorial

1. Import this sample from **Package Manager → Aspid.FastTools → Samples → EnumValues → Import**.
2. Open **`Scenes/EnumValuesTutorial.unity`** and select the **EnumValues Tutorial** GameObject.
3. **Checkpoint:** the Inspector shows five numbered sections, **STEP 1 → STEP 5**, with STEP 1 reading
   *"Your first mapping"*. If the fields render as plain foldouts, the sample assembly has not finished
   compiling — wait for Unity to recompile. STEP 1 is empty on purpose (you build it), STEP 2 is half-done
   (you populate it), STEP 3 and STEP 5 come pre-filled so you can study a working `[Flags]` setup and the
   typed variant immediately.

Prefer a clean slate? Add an empty GameObject and attach the **EnumValuesTutorial** component.

---

## Lesson 1 — Your first mapping

**Field:** `EnumValues<float> _step1Multipliers` — completely empty.

1. Click the type field in the header — a searchable type picker opens, listing every enum visible to the
   project. Pick **`DamageType`** (type `damage` to filter).
2. Press **+** on the list. A row appears: a **key dropdown** on the left (`Physical` — the first member) and
   a **float value** on the right.
3. Add a few more rows and give each a different key and value, e.g. `Physical = 1.0`, `Fire = 1.5`.
4. **Checkpoint:** the row keys render as proper enum dropdowns, not text fields — the drawer resolved your
   enum type.

**Notice:**

- The enum type is a **required** setting: until it is picked, the field warns and every lookup returns the
  default value.
- Duplicate keys are allowed by the Inspector, but only the **first** row with a given key ever wins a lookup —
  keep keys unique.
- The value side is an ordinary serialized field — `float` here, but any serializable type works (`Color`,
  `Sprite`, a custom `[Serializable]` class…).

---

## Lesson 2 — Populate missing members & the default value

**Field:** `EnumValues<Color> _step2Colors` — the enum type is pre-set to `DamageType`, the list is empty.

1. **Right-click** anywhere on the field and choose **Populate Missing Enum Members**.
2. Four rows appear — one per `DamageType` member — each seeded with the **Default Value** (white).
3. Recolor a few: `Fire` orange, `Ice` cyan. Leave the rest as they are, or delete the rows you don't need.
4. Right-click again — the menu item is now **greyed out**: nothing is missing.

**Notice:**

- **Default Value** (the last field) plays two roles: the seed for populated rows, and the fallback `GetValue`
  returns when no entry matches. You do **not** need a row per member — map the exceptions, let the default
  cover the rest.
- Populate never touches existing rows — it only appends what is missing, so it is safe on a half-filled list.
- Deleting and reordering rows works like any Unity list; order only matters for `[Flags]` lookups (next lesson).

---

## Lesson 3 — `[Flags]` keys and the lookup rules

**Field:** `EnumValues<float> _step3SpeedByStatus` — pre-filled, keyed on the `[Flags]` enum `StatusEffect`.

Because `StatusEffect` is `[Flags]`, each key renders as a **multi-select flags dropdown**, and a combination
like `Burning | Slowed` is a regular entry of its own. The pre-filled list is:

| # | Key | Value |
|---|---|---|
| 0 | `Burning` | `0.9` |
| 1 | `Frozen` | `0.2` |
| 2 | `Slowed` | `0.5` |
| 3 | `Burning \| Slowed` | `0.4` |

`GetValue` resolves a `[Flags]` lookup in three steps:

1. **Exact match wins first, regardless of order** — the composite `Burning | Slowed` entry is deliberately
   *last*, yet a lookup of exactly `Burning | Slowed` returns `0.4`.
2. **No exact match → the first contained entry wins** — a lookup of `Burning | Frozen | Slowed` has no exact
   entry, so the first row (in list order) whose flags are all contained in the value wins: `Burning`, `0.9`.
3. **Nothing matches → Default Value** — `Stunned` has no entry: `1.0`. `None` (zero) only ever matches a
   `None` entry, never a flag entry.

You will verify all three in the next lesson.

---

## Lesson 4 — Reading values from code

**Fields:** `DamageType _step4DamageType`, `StatusEffect _step4Effects` — the lookup keys.

The API is one call:

```csharp
float multiplier = _step1Multipliers.GetValue(_step4DamageType);
float speed      = _step3SpeedByStatus.GetValue(_step4Effects);
```

**Try it** (no Play Mode needed — this works in Edit Mode):

1. Right-click the component header and choose **Log Tutorial Lookups**. The Console prints one line per
   lookup, plus the iteration over STEP 3's entries.
2. Set `_step4Effects` to `Burning | Frozen | Slowed` and log again → `0.90` (rule 2: first contained entry).
3. Set it to `Stunned`, then to `None` → `1.00` both times (rule 3: default value).

**Notice:**

- `GetValue` takes any value of the configured enum — there is no per-key registration step in code.
- `foreach` over an `EnumValues` yields the configured `(key, value)` pairs in list order; the default value is
  **not** part of the iteration.
- All entries resolve lazily on first access and re-resolve after Inspector edits — no manual refresh call.

---

## Lesson 5 — The typed variant: `EnumValues<TEnum, TValue>`

**Field:** `EnumValues<DamageType, float> _step5TypedMultipliers` — pre-filled.

When the enum type is known at compile time, declare it in the field instead of picking it in the Inspector:

```csharp
[SerializeField] private EnumValues<DamageType, float> _multipliers;   // enum fixed at compile time

float m = _multipliers.GetValue(DamageType.Fire);   // takes a DamageType, not a boxed Enum
```

1. Select the tutorial GameObject and expand **STEP 5**. **Checkpoint:** there is **no type-picker row** in
   the header — the rows render typed `DamageType` dropdowns immediately.
2. Right-click the field → **Populate Missing Enum Members** works exactly as in Lesson 2 (only `Poison` is
   missing here).
3. Run **Log Tutorial Lookups** — the STEP 5 line uses the same `_step4DamageType` key as STEP 1, so you can
   compare both variants side by side.

**Notice:**

- Everything from Lessons 1–4 carries over: the same Inspector rows, *Populate Missing Enum Members*, the
  Default Value fallback, the `[Flags]` lookup rules, `foreach` over configured entries.
- `GetValue` is compile-time safe — it accepts a `DamageType`, so there is no way to look up a key of the
  wrong enum type.
- The serialized layout matches `EnumValues<TValue>`: switching a field between the two variants (with the
  same enum configured) keeps the entries. `DamageDealer._damageColors` in this sample went through exactly
  that migration.
- **Rule of thumb:** enum known up front → `EnumValues<TEnum, TValue>`; enum chosen per-asset in the
  Inspector → `EnumValues<TValue>`.

---

## When the enum changes

Real projects rename and delete enum members. The keys are stored as **name strings**, so:

- **Renamed / deleted member** — the stored key no longer parses. The Inspector migrates the row to the first
  member of the enum; at runtime an unresolved key logs an error and simply never matches (no crash).
- **Added member** — nothing breaks; the new member just has no row (→ default value) until you add one, e.g.
  via *Populate Missing Enum Members*.
- **Switching the enum type** on a filled list keeps the rows and their values and re-parses each key against
  the new type — a key whose name exists under both types survives; the rest are migrated (and saved) as the
  new type's first member, so the original key names do **not** come back if you switch the type back. Try it
  on STEP 1 once you're done with it.

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
