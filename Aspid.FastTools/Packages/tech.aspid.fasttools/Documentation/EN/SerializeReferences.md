# SerializeReference Selector

Reference for the `[SerializeReference]` tooling around the `[TypeSelector]` dropdown: repairing broken managed references, the bulk-repair tabs, project settings and the build/CI gate. For an overview of the dropdown itself, see the [README](README.md#serializereference-selector).

## Repairing broken references

| Case | Fix |
|---|---|
| **Missing type** (renamed or deleted) | A yellow notice instead of a silent clear. The underlined **Fix** opens the picker and re-points the type while keeping its data — at any depth, in saved assets and live in Prefab Mode. |
| **Smart Fix** | Next to **Fix**, suggests the most likely replacement (`[MovedFrom]`, a different namespace/assembly, casing, a near-miss name) and applies it in one click — never automatically. |
| **Shared reference** (two fields share one instance) | Flagged with a notice; **Make unique** splits it into an independent copy. Duplicating a list element (Ctrl+D, `+`) no longer aliases the reference. |

<!-- TODO(media): optional gif aspid_fasttools_serialize_reference_repair.gif — BrokenWeaponPreset → missing-type notice → Smart Fix, data preserved -->

Bulk repair lives in two dedicated tabs:

| Tab | Purpose |
|---|---|
| **Asset References** (`Tools → Aspid 🐍 → FastTools → Asset References`) | Maps an asset's whole managed-reference graph from its YAML — a per-component tree with field paths, shared and orphaned references, `MISSING` / `SHARED` badges, and an inline type dropdown on every card. Surfaces the missing references the Inspector cannot show. |
| **Project References** (`Tools → Aspid 🐍 → FastTools → Project References`) | `Scan Project` sweeps every `.prefab` / `.asset` / `.unity` under `Assets/`, groups broken references by stored type, and rewrites a whole group with a single `Fix all` (plus Smart Fix). A group whose stored type matches a declared `[MovedFrom]` rename reads as a pending migration instead of a breakage — one **Migrate all** click bakes the rename into the files, after which the attribute can be removed from code. |

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

For headless CI, `SerializeReferenceCiGate.RunCheck` (invoked via `-batchmode -executeMethod`) writes a report and honours the committed gate severity:

- `Off` skips the check, `Warn` logs but exits 0, `Fail` exits non-zero when violations exist.
- `-srGateRequired` also flags unset `[TypeSelector(Required = true)]` fields across prefabs, ScriptableObjects and scenes (top-level fields, pure-YAML pass).
- `-srGateWarnOnly` / `-srGateFail` override the committed severity per run.
