# Types — Step-by-Step Tutorial

Three complementary ways to pick a `System.Type` from the Inspector, all storing the assembly-qualified
name and resolving lazily:

- `SerializableType<T>` — a strongly typed wrapper; the generic argument constrains the picker to `T` and its
  subtypes, and the implicit conversion to `Type?` is free.
- `[TypeSelector(typeof(Base))]` on a raw `string` / `string[]` — the same picker window on an un-wrapped
  field, for when you want to skip the wrapper or narrow the candidate list yourself.
- `ComponentTypeSelector` — an empty serialized struct that adds a subtype dropdown to a `MonoBehaviour`'s
  Inspector and rewrites `m_Script` in place when you switch subtypes.

Each lesson maps to one `STEP` section of the `TypesTutorial` component.

## Open the tutorial

1. Open the Welcome window (**Tools → Aspid 🐍 → FastTools → Welcome**) and import the **Types** sample.
2. Open **`Scenes/TypesTutorial.unity`** and select the **Types Tutorial** GameObject.

---

## Lesson 1 — Pick a type with `SerializableType<T>`

**Field:** `SerializableType<Ability> _step1Ability` — pre-set to `Dash`.

The whole declaration is one line — see `Scripts/Tutorial/TypesTutorial.cs`:

```csharp
[SerializeField] private SerializableType<Ability> _step1Ability;
```

1. Click the field — a type picker opens, scoped to `Ability` and everything assignable to it: the abstract
   base `Ability` itself plus its subtypes `Dash`, `Fireball`, `Heal`.
2. Pick another entry, e.g. `Fireball`.

- The generic argument `T` scopes the list to `T` and its subtypes; unrelated types never appear.
- `SerializableType<T>` shows candidates with `TypeAllow.All`, so the base type `T` itself — **including when it
  is abstract or an interface** — is listed too. This is a general-purpose type reference, so a base/abstract
  type is a valid value; if you resolve it to instantiate (`AddComponent` / `Activator`), pick a **concrete**
  subtype, since an abstract type cannot be instantiated. `[TypeSelector]` on a raw `string` (Lesson 2) now
  shares this `TypeAllow.All` default, so both behave the same.
- What is serialized is the **assembly-qualified name** string; `.Type` performs the `GetType()` lookup on
  first access and caches the result.
- `.Type` returns `null` if the stored name no longer resolves (e.g. the type was renamed) — always null-check
  before using it.

---

## Lesson 2 — `[TypeSelector]` on a raw `string[]`

**Field:** `[TypeSelector(typeof(AbilityModifier))] string[] _step2ModifierTypes` — pre-filled with two modifiers.

```csharp
[TypeSelector(typeof(AbilityModifier))]
[SerializeField] private string[] _step2ModifierTypes;
```

1. Each array element renders as its **own picker**, constrained to `AbilityModifier` subtypes.
2. Press **+** to add an element and pick a modifier; reorder or remove elements as usual.

- Annotating a plain `string` (or `string[]`) gives you the picker **without** the `SerializableType<T>`
  wrapper — useful when you already store the name yourself, or want multiple base constraints across fields.
- `Allow` defaults to `TypeAllow.All`: abstract bases and interfaces are shown alongside concrete types. Set
  `[TypeSelector(typeof(AbilityModifier), Allow = TypeAllow.None)]` to restrict the picker to concrete types only.
- The same `[TypeSelector]` attribute also backs a `SerializableType` field — the string is the storage, the
  attribute is the picker.

---

## Lesson 3 — Generic types in the picker

**Field:** `[TypeSelector(typeof(AbilityModifier))] string _step3GenericModifier` — empty; pick a type to fill it.

```csharp
[TypeSelector(typeof(AbilityModifier))]
[SerializeField] private string _step3GenericModifier;
```

`StackModifier<T>` (see `Scripts/Modifiers/StackModifier.cs`) is a **concrete open generic** `AbilityModifier`.

1. Open the picker — alongside the concrete modifiers it lists the open generic **`StackModifier<T>`**.
2. Pick `StackModifier<T>` — a **second page** opens asking for `T`. Choose `float`.
3. The stored value is the **closed** type `StackModifier<float>`; `Log Tutorial Lookups` prints it (with
   `IsConstructedGenericType == true`).

- Picking an open generic never stores `StackModifier<>` itself — you always land on a closed, constructible
  type once `T` is chosen; `Type.GetType` resolves that closed name back to a `Type`.
- The base constraint still applies to the closed forms: only type arguments that keep the result assignable to
  `AbilityModifier` are accepted on the second page.
- **A closed field type fixes `T`**: constrain the field to `StackModifier<float>` directly
  (`[TypeSelector(typeof(StackModifier<float>))]`) and `T` is already known — the picker builds it in one step,
  with no second page. The same holds for `SerializableType<StackModifier<float>>`.

---

## Lesson 4 — Require a type with `[TypeSelector(Required = true)]`

**Field:** `[TypeSelector(typeof(Ability), Required = true)] string _step4RequiredAbility` — left **empty** on purpose.

```csharp
[TypeSelector(typeof(Ability), Required = true)]
[SerializeField] private string _step4RequiredAbility;
```

1. While the field is empty it renders an inline **"Required type is not set"** notice.
2. Pick any `Ability` from the dropdown — the notice disappears.

- `Required = true` flags an **unset** field: an empty `string` (or a null `[SerializeReference]`) shows the
  inline notice **and** counts as a violation for the build/CI gate (Settings → *SerializeReferences* →
  required-field gate: Off / Warn / Fail).
- The CI gate scans a type's **top-level declared fields** — `_step4RequiredAbility` is one, so it is covered.
  A required field nested inside a serializable container or array still shows the inline notice but is **not**
  seen by the gate (a known limitation).
- `Required` is orthogonal to `Allow`: it validates *emptiness*, not *which* types are offered.

---

## Lesson 5 — Swap a component with `ComponentTypeSelector`

**Field:** `EnemyBase _step5Enemy` — references the **Enemy** GameObject in the scene.

`EnemyBase` declares a single `ComponentTypeSelector` field (see `Scripts/Enemies/EnemyBase.cs`):

```csharp
[SerializeField] private ComponentTypeSelector _enemyType;
```

1. Select the **Enemy** GameObject in the Hierarchy.
2. At the top of its Inspector, open the **type dropdown** — it lists the subtypes of the declaring class
   (`FastEnemy`, `TankEnemy`).
3. Switch from `FastEnemy` to `TankEnemy` — the component's `m_Script` is rewritten **in place**; the
   `Health` field (declared on the shared `EnemyBase`) keeps its value across the swap.

- The picker auto-discovers subtypes from the field's declaring class — no configuration needed.
- Fields persist across the swap wherever the new subtype declares a **matching name**; `_speed` (FastEnemy)
  and `_armor` (TankEnemy) are unique to each, so they reset when you switch.
- Place one `ComponentTypeSelector` field per root class, typically at the top of the Inspector.

---

## Lesson 6 — Resolving types in code

Right-click the component header → **Log Tutorial Lookups** (works in Edit Mode). The Console shows each form
resolving its stored type:

```csharp
Type ability = _step1Ability.Type;              // SerializableType<T> — lazy, cached, null on failure
Type modifier = Type.GetType(qualifiedName);    // raw string — resolve manually
Type concrete = _step5Enemy.GetType();          // the subtype the component was swapped to
```

The real component `Scripts/Abilities/AbilitySelector.cs` uses the first two on `Start()`: it `AddComponent`s
the picked `Ability` subtype and `Activator.CreateInstance`s each `AbilityModifier` from its assembly-qualified
name.

- `SerializableType<T>` is safe to convert implicitly (`Type t = _step1Ability;`) — the picker only ever offers
  `T` subtypes.
- The raw-string form has no compile-time guarantee: `Type.GetType` returns `null` for an unresolved name, so
  guard every lookup.

---

## When a type is renamed or removed

The stored value is an **assembly-qualified name string**, so:

- **Renaming or moving a type** (namespace/assembly change) invalidates the stored name — `.Type` /
  `Type.GetType` return `null`. Re-pick the field, or update the serialized string in the asset.
- **Deleting a type** leaves fields pointing at a name that no longer resolves; the picker shows it as missing
  until you pick a valid type.
- `ComponentTypeSelector` resolves subtypes live from the declaring class, so a removed subtype simply drops
  out of the dropdown.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Tutorial/TypesTutorial.cs` | All six lessons as numbered fields + the `Log Tutorial Lookups` context menu |
| `Scripts/Abilities/AbilitySelector.cs` | `SerializableType<T>` + `[TypeSelector]` used by a real component, resolved on `Start()` |
| `Scripts/Modifiers/StackModifier.cs` | A concrete open generic `AbilityModifier` (`StackModifier<T>`) for the Lesson 3 picker |
| `Scripts/Enemies/EnemyBase.cs` | The one-line `ComponentTypeSelector` declaration and the swappable subtypes |
| [README.md](README.md) | The compact demo-scene walkthrough |
