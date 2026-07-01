# TypeSelector for SerializeReference — Step-by-Step Tutorial

A guided, hands-on tour of everything `[TypeSelector]` does for `[SerializeReference]` fields. Work through the
lessons in order: each one builds on the previous and maps to one section of the `TypeSelectorTutorial` component.

**The one rule:** put `[SerializeReference]` **and** `[TypeSelector]` on the same field. The first tells Unity to
store a polymorphic instance; the second renders the searchable type picker.

```csharp
[SerializeReference] [TypeSelector]
private IWeapon _weapon;
```

## Open the tutorial

1. Import this sample from **Package Manager → Aspid.FastTools → Samples → SerializeReferences → Import**.
2. Open **`Scenes/TypeSelectorTutorial.unity`** and select the **TypeSelector Tutorial** GameObject.
3. The Inspector reads top to bottom as **STEP 1 → STEP 8**. A few steps come pre-filled so you can see working
   examples immediately; the rest are empty for you to try.

Prefer a clean slate? Add an empty GameObject and attach the **TypeSelectorTutorial** component.

Every lesson below works in both the default **UIToolkit** inspector and the **IMGUI** inspector — the package
ships a drawer for each path and they are at feature parity. (Every sample prefab ships an `IMGUI…` twin — via the
sibling `IMGUILoadout` / `IMGUISlottedLoadout` components — that forces the IMGUI path if you want to compare.)

---

## Lesson 1 — Your first picker

**Field:** `IWeapon _step1Single` · `[SerializeReference] [TypeSelector]`

Click the dropdown in the field header. A searchable, hierarchical window opens listing every concrete `IWeapon`
implementation: `Sword`, `Pistol`, `Shotgun`, `Railgun`.

- **Pick a type** → Unity instantiates it and its own serialized fields appear inline under the foldout.
- **`<None>`** (first row) → clears the reference back to `null`.
- **Type to search** — just start typing; or click the magnifier. `↑ ↓` navigate, `Enter` selects, `Esc` closes.
- **Favourites & Recent** — hover a row and click the star, or press `Space`, to pin a type to the **Favorites**
  group. Types you pick are remembered under **Recent**. Both groups are collapsible.

> The picker only ever lists types you can actually instantiate — concrete, non-abstract classes that are not
> `UnityEngine.Object`. That filtering is automatic; you never see an invalid choice.

---

## Lesson 2 — Lists and arrays

**Field:** `List<IWeapon> _step2List` · `[SerializeReference] [TypeSelector]`

A `List<T>` (or array) of a `[SerializeReference]` type turns every element into its own independent picker.

1. Press **+**. Instead of duplicating the last element, the **picker opens** so you choose the new element's type.
2. Add a few elements and give each a different weapon — they are fully independent instances.
3. Reorder or remove elements as usual.

This is the first behaviour the attribute *changes* versus stock Unity: the native "+" would clone the previous
element (and alias its data); here "+" always appends a fresh, typed instance.

---

## Lesson 3 — Abstract bases and interfaces

**Field:** `StatusEffect _step3Abstract` · `[SerializeReference] [TypeSelector]`

`StatusEffect` is an **abstract** class — you cannot `new` it.

**Try it:**
1. Open the picker.
2. Note that only the concrete subclasses `BurnEffect` and `FreezeEffect` are offered.

**Notice:**
- The abstract base itself is never listed — there would be nothing to instantiate.
- An interface-typed field behaves identically (Lesson 1's `IWeapon` is an interface).
- Declare the field as the broadest type that makes sense; the picker shows only the concrete leaves you can create.

---

## Lesson 4 — Narrowing the candidate list

**Fields:** three `IWeapon` fields with different attribute arguments.

```csharp
[SerializeReference] [TypeSelector(typeof(IRanged))]            private IWeapon _step4Ranged;
[SerializeReference] [TypeSelector(typeof(IMelee))]             private IWeapon _step4Melee;
[SerializeReference] [TypeSelector(typeof(IMelee), typeof(IRanged))] private IWeapon _step4MeleeOrRanged;
```

All three fields are declared `IWeapon`, yet the picker shows different sets. The base type(s) you pass to
`[TypeSelector(...)]` act as an **extra filter, applied below the declared field type**:

| Attribute | Offered types |
|---|---|
| `[TypeSelector]` (Lesson 1) | `Sword`, `Pistol`, `Shotgun`, `Railgun` (all `IWeapon`) |
| `[TypeSelector(typeof(IRanged))]` | `Pistol`, `Shotgun`, `Railgun` |
| `[TypeSelector(typeof(IMelee))]` | `Sword` |
| `[TypeSelector(typeof(IMelee), typeof(IRanged))]` | all four (multiple base types are OR-ed) |

Use this to keep a field's *type* broad (so your code stays generic) while constraining what designers can pick.

> The base types narrow **below** the declared field type — they can never widen it. `[TypeSelector(typeof(object))]`
> on an `IWeapon` field still only shows `IWeapon` implementations.

> **Note — `TypeAllow` does not apply here.** The `Allow = TypeAllow.Abstract / Interface` option on the attribute
> only affects `[TypeSelector]` on a **`string`** field (where you are naming a type, not instantiating it). For a
> `[SerializeReference]` field it is ignored — you can never pick an abstract class or interface, because there would
> be nothing to instantiate.

---

## Lesson 5 — Nested references (recursion)

**Field:** `IWeapon _step5Nested` · `[SerializeReference] [TypeSelector]`

**Try it:**
1. Pick **`Railgun`** and expand its foldout.
2. Find its `Charge Effect` field — itself a `[SerializeReference] [TypeSelector] StatusEffect`.
3. Open *that* picker and assign a `BurnEffect` or `FreezeEffect`.

**Notice:**
- A managed reference can contain another managed reference, to any depth.
- Every nested level gets its own picker, inline child fields, notices and context-menu actions — the drawer is fully recursive.

---

## Lesson 6 — Generic hierarchies

**Fields:** `IModifier _step6Open` and `Modifier<float> _step6Closed`.

`Modifier<T>` is a **concrete open generic** (`IModifier`), with closed subclasses `DamageModifier : Modifier<float>`,
`AmmoModifier : Modifier<int>`, `NameModifier : Modifier<string>`.

**On the `IModifier` field** (`_step6Open`), the picker offers:

- the three concrete closed subclasses, **and**
- the open generic **`Modifier<T>`** itself.

Pick `Modifier<T>` and a **second page** opens to choose the argument `T`. Try `string`, then `float` — only after `T`
is resolved is the closed type (`Modifier<string>` / `Modifier<float>`) constructed and assigned.

**On the `Modifier<float>` field** (`_step6Closed`), `T` is already fixed by the field type, so:

- candidates are constrained by assignability — only `DamageModifier` (a `Modifier<float>`) and `Modifier<float>`
  itself are offered; `AmmoModifier` (int) and `NameModifier` (string) are excluded, and
- picking `Modifier<…>` builds `Modifier<float>` **directly**, with no second page.

---

## Lesson 7 — References inside `[Serializable]` containers

**Fields:** `WeaponSlot _step7Slot` and `List<WeaponSlot> _step7Slots`.

`WeaponSlot` is a plain `[Serializable]` class (not a managed reference itself) holding a `label`, a `priority`, and a
`[SerializeReference] [TypeSelector] IWeapon`.

**Try it:**
1. Expand `_step7Slot` and pick a weapon for its inner field.
2. Add elements to `_step7Slots` — each element is a container whose weapon is its own picker.

**Notice:**
- A managed reference does **not** have to sit directly on the component.
- Everything you learned still applies at this depth: the picker, inline child fields, the missing-type warning and its inline **Fix**.
- It works on the component, one level inside a container, or inside each element of a `List<WeaponSlot>`.

---

## Lesson 8 — Required references

**Field:** `IWeapon _step8Required` · `[SerializeReference] [TypeSelector(Required = true)]`

Set `Required = true` on `[TypeSelector]` to mark a reference as mandatory.

```csharp
[SerializeReference, TypeSelector(Required = true)]
private IWeapon _weapon;
```

**Try it:**
1. Leave the field empty — an inline **"Required reference is not set"** warning appears.
2. Pick any `IWeapon` — the notice clears.

**Notice:**
- The same `Required = true` works on a `[TypeSelector] string` type field — there "unset" means an empty type name.
- A present-but-missing managed-reference type is never a *required* violation — it keeps its own missing-type notice.
- Required references also feed the **build / CI gate** (see *Project settings & the build/CI gate* below).

---

## Power-user gestures (right-click any field above)

These are not separate fields — they are gestures available on every `[TypeSelector]` field via its right-click
context menu, header, or drag-and-drop. Try them on the fields from Lessons 1–8.

| Gesture | What it does |
|---|---|
| **Switch the type** | Keeps the fields the old and new type share. Set a `Pistol`'s damage, switch to `Shotgun` and back — the value survives (data is carried over by name). |
| **Copy / Paste Serialize Reference** | Right-click the header. Paste rebuilds an **independent** instance; it is greyed-out when the copied type does not fit the field. |
| **Make Unique Reference** | When two fields share one instance (a "shared reference" notice appears — e.g. after duplicating a list element), this gives the field its own copy so edits stop bleeding across. |
| **Link to Existing ▸ …** | The inverse: deliberately point this field at a sibling field's instance, sharing one object across both. |
| **Drag a `MonoScript` onto the field** | Drag a `.cs` script from the Project window onto the header; if its class fits (honouring the narrowing from Lesson 4) a fresh instance is assigned. |
| **Create New Script…** | Generates a `[Serializable]` class deriving from the field's base type, then auto-assigns a new instance after the recompile. |
| **Save as Template… / Paste Template ▸ …** | Save the current value as a named, project-wide template and re-apply it to any compatible field later. |
| **Find Usages of …** | Opens the Search window (`sr:<Type>`) listing every place that type is used as a managed reference. |

---

## Maintenance: repairing broken references

When a managed-reference type is renamed, moved or deleted, its stored data is orphaned. The tutorial ships sibling
assets **pre-broken** on purpose so you can practise the recovery flow:

- `Presets/BrokenWeaponPreset.asset`, `Presets/BrokenArsenalPreset.asset` — `ScriptableObject`s referencing a missing `GhostWeapon`.
- `Prefabs/LoadoutMissingType.prefab` — a prefab whose `Sidearms → Element 0` references a missing `GhostPistol`.

### Inline repair (one field)

1. Select a broken asset **in the Project window**.
2. The missing field shows a `<Missing …>` caption, a **Missing type** warning and a **Fix** button (often with a one-click **Smart Fix** suggestion of the likely new type).
3. Click **Fix**, pick the replacement (e.g. `Pistol`) — the reference is rewritten **keeping its data** (the picker rewrites the stored type in the asset file rather than recreating the instance).

> Repair reads and rewrites the asset YAML directly, because Unity does not expose a missing type through its
> serialization API (and on GameObjects/prefabs even drops it from the live object — UUM-129100). It therefore needs a
> **saved asset file**: it works on ScriptableObjects and prefab assets selected in the Project, and on objects in
> Prefab Mode / a clean saved scene, but not on a dirty scene or a prefab-instance override.

### Whole-asset & project-wide repair

Open **`Tools → Aspid 🐍 → FastTools`**:

| Tab | What it does |
|---|---|
| **Asset References** | Maps a saved asset's entire `[SerializeReference]` graph and repairs any missing node inline — any depth, any child object the Inspector can't otherwise reach. |
| **Project References** | Sweeps every asset under `Assets/` and groups broken references **by stored type** — `BrokenWeaponPreset.asset` and `BrokenArsenalPreset.asset` collapse into one **GhostWeapon** group (`4 entries · 2 files`). **Fix all** re-points every entry across both files at once. |

### Guard rails (no window needed)

- **Delete guard** — deleting a `.cs` whose class is still used as a managed reference pops a confirm dialog listing the affected assets before it lets the delete through.
- **Breakage toast** — when references newly become missing after a recompile, a dismissable toast and one console warning deep-link straight to the repair window. Switch it off via **Breakage detection** in Project settings (below) if you find it intrusive.

---

## Project settings & the build/CI gate

**`Project Settings → Aspid FastTools → SerializeReference`** exposes:

- **Breakage detection** — toggle the proactive missing-reference toast on or off; turn it off to silence the
  domain-reload / import-time detection entirely. Per-machine.
- **Auto de-alias duplicated list elements** — give a duplicated list element its own instance instead of sharing the
  original's id.
- **Build / CI gate** — `Off` / `Warn` / `Fail`: at player-build time, log or abort on missing (and, for CI,
  unset-required) managed references.
- **Excluded scan folders** — paths skipped by every project scan.

Rid colours are not a setting — a shared reference is always colour-coded by id (in the inspector stripe/notice and
the graph window's chip), since it is the whole point of the feature: matching colours is what lets you tell which
fields share an instance at a glance.

Auto de-alias, the build/CI gate and the excluded folders must behave the same for every teammate and for CI, so
unlike breakage detection (per-machine `EditorPrefs`) they are saved to a **committed**
`ProjectSettings/SerializeReferenceSharedSettings.asset`. Commit that asset after changing any of these three values.

The same options are also available in the window's **Settings** tab (**`Tools → Aspid 🐍 → FastTools → Settings`**).

For headless CI, `SerializeReferenceCiGate.RunCheck` (invoked via `-batchmode -executeMethod`) writes a report and
honours the committed gate severity: `Off` skips the check, `Warn` logs but exits 0, `Fail` exits non-zero when
violations exist. `-srGateRequired` also flags unset `[TypeSelector(Required = true)]` fields across prefabs,
ScriptableObjects and scenes (scenes are checked for top-level required fields via a pure-YAML pass), and the per-run
flags `-srGateWarnOnly` (force exit 0) / `-srGateFail` (force fail on violations) override the committed severity.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Tutorial/TypeSelectorTutorial.cs` | All eight lessons as numbered fields |
| `Scripts/Weapons/` | `IWeapon` + `IMelee`/`IRanged` branches and `Sword`/`Pistol`/`Shotgun`/`Railgun` |
| `Scripts/Effects/` | abstract `StatusEffect` + `BurnEffect`/`FreezeEffect` |
| `Scripts/Modifiers/` | the `Modifier<T>` generic hierarchy |
| `Scripts/SlottedLoadout.cs` | references nested in containers (Lesson 7, standalone) — its `IMGUISlottedLoadout` twin forces the IMGUI path |
| `Scripts/WeaponPreset.cs` + `Presets/Broken*.asset` | the missing-type repair flow |
| `Scripts/IMGUILoadout.cs` / `Scripts/IMGUISlottedLoadout.cs` (+ their `Editor/` companions) | the same fields forced through the IMGUI path; every demo prefab has an `IMGUI…` twin |
