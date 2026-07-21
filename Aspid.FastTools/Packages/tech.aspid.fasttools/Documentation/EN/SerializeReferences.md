# SerializeReference Selector

The stock Inspector cannot populate `[SerializeReference]` fields: a managed reference
cannot be created from the UI, and when a type is renamed or deleted Unity silently clears
the data. SerializeReference Selector closes both gaps: a dropdown implementation picker
right in the Inspector, plus tooling to find and repair broken references — from a single
field up to a whole-project sweep and a CI gate.

**Reference sections:**

* [`Inspector type dropdown`](#inspector-type-dropdown) — the `[TypeSelector]` dropdown
  on `[SerializeReference]` fields: implementation picking, nested inspector, generics,
  copy/paste;
* [`Repairing broken references`](#repairing-broken-references) — a yellow notice instead
  of a silent clear, **Fix** / **Smart Fix** / **Make unique**;
* [`Bulk repair tabs`](#bulk-repair-tabs) — the **Asset References** and
  **Project References** tabs for auditing and mass repair across the project;
* [`Project settings & the build/CI gate`](#project-settings--the-buildci-gate) —
  the Project Settings page, setting scopes and the player-build gate;
* [`Headless CI`](#headless-ci) — `SerializeReferenceCiGate.RunCheck` for batchmode pipelines.

**A shorter version with the same examples lives in the** [README](README.md#serializereference-selector).

## Inspector type dropdown

Add `[TypeSelector]` next to `[SerializeReference]` — the Inspector replaces the stock
managed-reference UI with the hierarchical [type-selection window](Types.md#typeselectorwindow)
with search. You pick which concrete implementation of the field's type gets created right
in the Inspector; `<None>` clears the reference.

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

public sealed class Loadout : MonoBehaviour
{
    [TypeSelector]
    [SerializeReference] private IWeapon _primary;

    [TypeSelector]
    [SerializeReference] private List<IWeapon> _sidearms;
}
```

The attribute is editor-only (`[Conditional("UNITY_EDITOR")]`) and carries no runtime
cost. It works with single fields, arrays and `List<T>`, in both IMGUI and UIToolkit
inspectors. The same attribute also works on `string` and `SerializableType` fields —
see [TypeSelectorAttribute](Types.md#typeselectorattribute).

![Picking an implementation into a managed reference: the picker and the nested inspector of the chosen instance](../Images/aspid_fasttools_serialize_reference_selector.gif)

| Feature | What it does |
|---|---|
| **Implementation picking** | The list shows concrete non-`UnityEngine.Object` classes compatible with the field type. `[TypeSelector(typeof(IMelee))]` narrows it to `IMelee` implementations. |
| **Open generics** | `Modifier<T>` and friends: arguments are inferred from a closed generic field, or picked on the selector's second page. |
| **Data preservation** | On a type switch, fields matching by name and serialized shape carry over instead of resetting to defaults. |
| **Copy / Paste** | Right-clicking the header copies the value and pastes it as an independent instance into any compatible field. |
| **Multi-selection** | A mixed selection shows a mixed dropdown state; a pick or paste applies to every object in a single Undo group. |
| **Compiler validation** | Roslyn analyzer: `AFT0004` (error) — the type inherits `UnityEngine.Object`; `AFT0005` (warning) — the selector would be empty. |

An empty field with `[TypeSelector(Required = true)]` shows a "required" notice in the
Inspector and counts as a violation for the [build/CI gate](#project-settings--the-buildci-gate) —
see the `Required` property on [TypeSelectorAttribute](Types.md#typeselectorattribute).

## Repairing broken references

When an asset's stored type stops resolving, or two fields silently share one instance,
the selector does not stay quiet — every problem gets an Inspector notice with a repair
button next to it:

| Case | Fix |
|---|---|
| **Missing type** (renamed or deleted) | A yellow notice instead of a silent clear. The underlined **Fix** opens the picker and re-points the type while keeping its data — at any depth, in saved assets and live in Prefab Mode. |
| **Smart Fix** | Next to **Fix**, suggests the most likely replacement (`[MovedFrom]`, a different namespace/assembly, casing, a near-miss name) and applies it in one click — never automatically. |
| **Shared reference** (two fields share one instance) | Flagged with a notice; **Make unique** splits it into an independent copy. Duplicating a list element (Ctrl+D, `+`) no longer aliases the reference. |

![Missing-type notice with the Fix and Smart Fix actions on a broken managed reference](../Images/aspid_fasttools_serialize_reference_repair.png)

![Shared-reference notice with the Make unique action on two fields aliasing one instance](../Images/aspid_fasttools_serialize_reference_make_unique.png)

## Bulk repair tabs

There is no need to fix references one by one: auditing and mass repair live in two
dedicated tabs of the FastTools window.

| Tab | Purpose |
|---|---|
| **Asset References** (`Tools → Aspid 🐍 → FastTools → Asset References`) | Maps an asset's whole managed-reference graph from its YAML — a per-component tree with field paths, shared and orphaned references, `MISSING` / `SHARED` badges, and an inline type dropdown on every card. Surfaces the missing references the Inspector cannot show. |
| **Project References** (`Tools → Aspid 🐍 → FastTools → Project References`) | `Scan Project` sweeps every `.prefab` / `.asset` / `.unity` under `Assets/`, groups broken references by stored type, and rewrites a whole group with a single `Fix all` (plus Smart Fix). A group whose stored type matches a declared `[MovedFrom]` rename reads as a pending migration instead of a breakage — one **Migrate all** click bakes the rename into the files, after which the attribute can be removed from code. |

The **Asset References** tab lays out one asset's managed-reference graph as cards with
`MISSING` / `SHARED` badges and inline repair:

![Asset References tab: an asset's reference graph with a Fix Missing card](../Images/aspid_fasttools_serialize_reference_asset_references.png)

The **Project References** tab groups the whole project's findings by stored type — one
group is repaired at once with a single `Fix all`:

![Project References tab: a group of broken references with Fix all and Smart Fix](../Images/aspid_fasttools_serialize_reference_project_references.png)

## Project settings & the build/CI gate

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
- **Welcome** — auto-show toggle.

Every row carries a scope stripe (green — committed, blue — per-user); a pinned footer offers **Reset to defaults** per scope (saved Favorites / Recent lists survive a reset). All surfaces stay in live sync.

## Headless CI

For headless CI, the same check runs via `SerializeReferenceCiGate.RunCheck`: it scans
the project, writes a report, logs every violation, and honours the committed gate
severity — `Off` skips the check, `Warn` logs but exits 0, `Fail` exits 1 when
violations exist (exit code 2 marks an internal failure of the check itself).

```bash
Unity -batchmode -quit -projectPath . \
  -executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt -srGateRequired
```

| Flag | Description |
|---|---|
| `-srGateReport <path>` | Report file path; defaults to `SerializeReferenceGateReport.txt` in the project root. Each violation is a machine-readable line with the violation kind, asset path and field path. |
| `-srGateRequired` | Also flags unset `[TypeSelector(Required = true)]` fields across prefabs, ScriptableObjects and scenes (top-level fields, pure-YAML pass). |
| `-srGateWarnOnly` | Overrides the committed severity to `Warn` for this run: violations are logged but the exit code is 0. Wins over `-srGateFail` if both are passed. |
| `-srGateFail` | Overrides the committed severity to `Fail` for this run: exit code 1 when violations exist. |
