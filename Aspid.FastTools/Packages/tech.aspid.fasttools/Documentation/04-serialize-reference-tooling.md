# SerializeReference Tooling

References to renamed and deleted classes are found across the project and repaired as a group, before the build trips over them.

<a id="check-the-project"></a>

## Quick start

1. Open **Tools → Aspid 🐍 → FastTools → Project References**.
2. Click **Scan Project**: missing references are grouped by stored class.
3. Click **Fix all** in a group, pick a class and confirm with **Rewrite**.

> [!NOTE]
> Scanning reads text YAML, so **Asset Serialization → Mode** must be **Force Text**: binary files and Git LFS pointers are skipped.

<a id="bulk-repair-tabs"></a>

## Project References: repair a group

Project References and Asset References are tabs of one window. **Scan Project** reads the `.prefab`, `.asset` and `.unity` files under `Assets/`, apart from **Excluded scan folders**, and not only the scenes in the build.

A group card shows the stored class with its entry and file counts, then the asset paths and `rid`s; a click on a row opens the asset in **Asset References**.

![Project References with Fix all, Smart Fix → Pistol and Migrate all groups](Images/aspid_fasttools_serialize_reference_project_references.png)

### Choosing an action

| Action | What it does |
|---|---|
| **Fix all** | Opens the class picker and applies the pick to every writable entry of the group |
| **Smart Fix → Pistol** | Applies the suggested class, with the same confirmation |
| **Migrate all** | Writes the class that <code lang="csharp">[MovedFrom]</code> names for the old one |
| **Reassign all** | Picks a different class for a group recognized as a migration |

**Smart Fix** appears when a class matches the stored name: through <code lang="csharp">[MovedFrom]</code>, the same name, the same name in another case, or a similar name.

Rewrites skip open scenes, Prefab Mode and assets with unsaved changes. Asset References offers to save such an asset; save and close the rest, or repair the field with [Fix in the Inspector](03-serialize-reference-selector.md#repairing-missing-types).

### What repair preserves

A repair rewrites only the entry's class, namespace and assembly; its data and `rid` stay. When the group's fields have different types, the confirmation warns that the pick may not fit every entry: incompatible ones become <code lang="csharp">null</code> on reimport.

The summary after a rewrite has **Undo**: it restores the old class on entries that still hold the new one. The summaries stay while you switch tabs, so you can open an entry in Asset References and come back; **Rescan**, closing the window or a domain reload clears them, their Undo included.

### Clearing with None

`<None>` clears the references and deletes their stored data; when several fields share a `rid`, all of them are cleared. It cannot be undone. In open scenes, Prefab Mode and assets with unsaved changes the references are cleared in memory, and the scan keeps listing them until they are saved.

### Prefab instance overrides

A class set through a prefab instance override — in a variant, a nested prefab or an instance in a scene — is listed in a separate **Prefab instance overrides** card, and the build checks count it as missing. An old name that <code lang="csharp">[MovedFrom]</code> maps is listed there as a pending migration and passes the checks.

**Fix all**, **Smart Fix**, **Migrate all** and `<None>` do not rewrite overrides: pick a new class on the instance in the Inspector, or revert the override. Asset References does not show references that exist only in overrides.

## Asset References: inspect one asset

Assign a saved prefab, ScriptableObject or scene to the field next to **Rescan**, or click a row in Project References. The graph groups references by host object and field path:

| Label | Meaning |
|---|---|
| Band with **Fix Missing ▼** | The stored class is not found |
| **SHARED** | Several fields point at one instance; matching colours mark the connected fields |
| **Orphaned** | An entry no field points at; **Clear** deletes it from the file, without Undo |
| `rid` | The reference ID within its host object |

**Fix Missing** opens the class picker:

![GhostWeapon is repaired as Pistol in Asset References](Images/aspid_fasttools_serialize_reference_tooling.gif)

A field in a scene or under a missing parent reference is not edited here: repair the parent first, or edit the field in the Inspector.

## Migrations with MovedFrom

<code lang="csharp">[MovedFrom]</code> ties the old name to the renamed class:

| Before — GhostWeapon | After — Pistol |
|---|---|
| <pre lang="csharp"><code>[Serializable]&#10;public sealed class GhostWeapon&#10;&#123;&#10;    public int Damage = 10;&#10;&#125;</code></pre> | <pre lang="csharp"><code>[Serializable]&#10;[MovedFrom(true,&#10;    sourceClassName: "GhostWeapon")]&#10;public sealed class Pistol&#10;&#123;&#10;    public int Damage = 10;&#10;&#125;</code></pre> |

After compilation, **Scan Project** lists the group as a pending migration when exactly one class that fits the field claims the old name; **Migrate all** writes the new name into the files. A pending migration does not count as missing for the build checks. When several classes claim one old name, or the stored class is a closed generic, the group stays a missing type.

Remove <code lang="csharp">[MovedFrom]</code> only after migrating all data that must remain loadable, including prefab instance overrides, assets outside the current project and folders excluded from scanning.

<a id="project-settings--the-buildci-gate"></a>

## Pre-build checks

Open **Project Settings → Aspid.FastTools → SerializeReference** and set **Build / CI gate**; the default is `Warn`:

| Mode | Player build | Standalone CI run |
|---|---|---|
| `Off` | Skips the check | No scan and no report, an older report stays; exit code `0` |
| `Warn` | Warns and keeps building | Violations in the log; exit code `0` |
| `Fail` | Missing types stop the build | Exit code `1` on violations |

Exit code `2` means the check itself failed, for example the report could not be written.

### Where required fields are checked

| Run | Missing types | Empty fields with <code lang="csharp">Required = true</code> |
|---|---|---|
| **Project References → Scan Project** | Yes, pending migrations included | In `Warn` or `Fail`, as a **Required violations** group |
| **Asset References** | Yes | Yes, in any mode |
| Player build | In `Warn` or `Fail` | No |
| CI without `-srGateRequired` | Unless `Off` | No |
| CI with `-srGateRequired` | Unless `Off` | Yes |

In scenes the Required check has [limits](#required-check-boundaries).

### Shared and personal settings

| Setting | Storage | Purpose |
|---|---|---|
| **Build / CI gate** | Project | Validation severity |
| **Excluded scan folders** | Project | Folders skipped by scans and checks |
| **Auto de-alias duplicated list elements** | Project | Creates an independent copy when duplicating a list entry |
| **Breakage detection** | Local `EditorPrefs` | A notification and a Console message about newly missing references after scripts or assets change |

Shared settings are saved in `ProjectSettings/SerializeReferenceSharedSettings.asset`: commit it so the team and CI use the same rules. Personal settings are in **Preferences → Aspid.FastTools**.

<a id="headless-ci"></a>

## Running in CI

```bash
Unity -batchmode -projectPath . \
  -executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt \
  -srGateRequired -srGateFail
```

The command checks missing types and empty required fields, writes the report and exits with `1` on violations; `-srGateFail` overrides the project's mode, even `Off`.

### Command-line flags

| Flag | Behaviour |
|---|---|
| `-srGateReport <path>` | Report path; defaults to `SerializeReferenceGateReport.txt` |
| `-srGateRequired` | Also checks unset fields with <code lang="csharp">Required = true</code> |
| `-srGateFail` | Uses `Fail` instead of the project setting |
| `-srGateWarnOnly` | Uses `Warn`; takes precedence over `-srGateFail` if both are passed |

Without a severity flag, the project setting applies. For a trial run, replace `-srGateFail` with `-srGateWarnOnly`.

### Required check boundaries

In prefabs and ScriptableObjects the check walks every serialized property, collection elements and managed references included. Scenes are read from YAML: top-level fields and fields of by-value containers are checked, collection elements and managed references are not, and a field absent from the scene file is not a violation. Missing types are found in every stored entry either way.

<a id="report-and-exit-codes"></a>

### Report

The header counts the violations and the files that were not scanned because they are not text YAML:

```text
# SerializeReference Gate Report
# Violations: 2
# Not scanned (not text YAML): 1
#   Binary	Assets/Legacy/OldLoadout.prefab
```

Skipped files do not change the exit code. Binary assets that cannot hold managed references, such as LightingData and NavMesh, are left out of the log warning; a binary prefab, scene or ScriptableObject is named in it, since its missing types went unchecked.

After the header, each violation occupies one line. Fields are tab-separated:

```text
KIND    assetPath    fileId    rid    className    fieldPath    origin
```

| Field | Contents |
|---|---|
| `KIND` | `MissingType` or `RequiredUnset` |
| `assetPath` | File path, such as `Assets/Presets/BrokenWeaponPreset.asset` |
| `fileId` | Host object ID within the file; for a prefab instance override, the ID of the prefab instance |
| `rid` | Managed-reference ID; `-2` for an empty <code lang="csharp">[SerializeReference]</code>, `0` for a <code lang="csharp">string</code> or <code lang="class-name">SerializableType</code> |
| `className` | Stored class name for `MissingType`, without separate namespace or assembly fields |
| `fieldPath` | Required field path; for a `MissingType` override, the overridden field when the instance overrides it; otherwise empty |
| `origin` | `override` for a type set by a prefab instance override; otherwise empty |

The asset path, `fileId` and `rid` locate the entry in Asset References; an `override` row is in the **Prefab instance overrides** card of Project References instead.

## Package sample

Assets with missing types, a <code lang="csharp">[MovedFrom]</code> rename and a shared reference for these windows are in the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
