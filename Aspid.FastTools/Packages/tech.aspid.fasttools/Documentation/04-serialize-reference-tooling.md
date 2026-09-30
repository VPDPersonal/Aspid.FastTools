# SerializeReference Tooling

References to renamed and deleted classes are found across the project and repaired in one go — before they turn into null in a build.

<a id="check-the-project"></a>

## Quick start

1. Open **Tools → Aspid 🐍 → FastTools → Project References**.
2. Click **Scan Project**: missing references are grouped by stored class.
3. Click **Fix all** in a group, pick a class and confirm with **Rewrite**.

> [!NOTE]
> Scanning reads text YAML only: Project References silently skips binary files and Git LFS pointers, which the build check and the CI report list. So **Asset Serialization → Mode** must be **Force Text**.

<a id="bulk-repair-tabs"></a>

## Project References: repair a group

Project References and Asset References are tabs of one window. **Scan Project** reads the `.prefab`, `.asset` and `.unity` files under `Assets/`, apart from **Excluded scan folders**, and not only the scenes in the build. A click on a group row opens the asset in **Asset References**.

![Project References with Fix all, Smart Fix → Pistol and Migrate all groups](Images/aspid_fasttools_serialize_reference_project_references.png)

### Choosing an action

| Action | What it does |
|---|---|
| **Fix all** | Opens the class picker and applies the pick to every writable entry of the group |
| **Smart Fix → Pistol** | Applies a class found through <code lang="csharp">[MovedFrom]</code>, the same name, the same name in another case or a similar name; same confirmation |
| **Migrate all** | Writes the class that <code lang="csharp">[MovedFrom]</code> names for the old one |
| **Reassign all** | Picks a different class for a group recognized as a migration |
| `<None>` in the class picker | Clears the group's references and deletes their data, fields sharing the same `rid` included; no Undo |

### What repair preserves

A repair rewrites only the entry's class, namespace and assembly; its data and `rid` stay. When the group's fields have different types, the confirmation warns that the pick may not fit every entry: incompatible ones become <code lang="csharp">null</code> on reimport.

The summary after a rewrite has **Undo**: it restores the old class on entries that still hold the new one. It is gone after **Rescan**, closing the window or a domain reload; **Edit → Undo** does not revert a rewrite.

### Prefab instance overrides

A class set through a prefab instance override — in a variant, a nested prefab or an instance in a scene — is listed in a separate **Prefab instance overrides** card, and the build checks count it as missing. An old name listed in <code lang="csharp">[MovedFrom]</code> is shown there as a pending migration and passes the checks.

## Asset References: inspect one asset

Assign a saved prefab, ScriptableObject or scene to the field next to **Rescan**, or click a row in Project References. References are grouped by host object, each with its field path and `rid`:

| Label | Meaning |
|---|---|
| Band with **Fix Missing ▼** | The stored class is not found; the button opens the class picker |
| **Migrate → Crossbow** row | The class was renamed with <code lang="csharp">[MovedFrom]</code>; a click writes the new name into the file |
| **SHARED** | Several fields point at one instance; matching colours mark the connected fields |
| **Orphaned** | An entry no field points at; **Clear** deletes it from the file, without Undo |

![GhostWeapon is repaired as Pistol in Asset References](Images/aspid_fasttools_serialize_reference_tooling.gif)

> [!WARNING]
> **Fix Missing**, **Smart Fix** and **Migrate** write the class to the file at once, without confirmation, and Undo cannot revert it.

## Migrations with MovedFrom

<code lang="csharp">[MovedFrom]</code> ties the old name to the renamed class, and Unity loads such references itself. **Migrate all** writes the new name into the files so the attribute can be removed:

| Before — CrossbowLauncher | After — Crossbow |
|---|---|
| <pre lang="csharp"><code>[Serializable]&#10;public sealed class CrossbowLauncher&#10;&#123;&#10;    public int Damage = 14;&#10;&#125;</code></pre> | <pre lang="csharp"><code>[Serializable]&#10;[MovedFrom(false,&#10;    sourceClassName: "CrossbowLauncher")]&#10;public sealed class Crossbow&#10;&#123;&#10;    public int Damage = 14;&#10;&#125;</code></pre> |

A group becomes a pending migration when exactly one class in the project lists the old name in <code lang="csharp">[MovedFrom]</code> and it fits the field; the build checks do not count such a group as missing. When several classes list the name, or the stored class is a closed generic, the group stays a missing type.

Remove <code lang="csharp">[MovedFrom]</code> only when no file stores the old name any more. **Migrate all** does not rewrite it:

- in prefab instance overrides;
- in **Excluded scan folders**;
- in binary files and Git LFS pointers;
- in files outside `Assets/`.

<a id="project-settings--the-buildci-gate"></a>

## Pre-build checks

Open **Project Settings → Aspid.FastTools → SerializeReference** and set **Build / CI gate**; the default is `Warn`:

| Mode | Player build | Standalone CI run |
|---|---|---|
| `Off` | Skips the check | No scan and no report, an older report stays; exit code `0` |
| `Warn` | Warns and keeps building | Report and violations in the log; exit code `0` |
| `Fail` | Missing types stop the build | Report; exit code `1` on violations |

### What each run checks

| Run | Missing types | Empty fields with <code lang="csharp">Required = true</code> |
|---|---|---|
| **Project References → Scan Project** | Yes, with pending migrations | Unless the mode is `Off`, as a **Required violations** group |
| **Asset References** | Yes | Yes, in any mode |
| Player build | Unless the mode is `Off` | No |
| CI without `-srGateRequired` | Unless the mode is `Off` | No |
| CI with `-srGateRequired` | Unless the mode is `Off` | Unless the mode is `Off` |

A field is made required with <code lang="csharp">[TypeSelector(Required = true)]</code>; see [Required field](02-serializable-types.md#required-field). In scenes the Required check has [limitations](#limitations).

### Shared and personal settings

| Setting | Storage | Purpose |
|---|---|---|
| **Build / CI gate** | Project | Validation severity |
| **Excluded scan folders** | Project | Folders skipped by scans and checks |
| **Auto de-alias duplicated list elements** | Project | Creates an independent copy when duplicating a list entry |
| **Breakage detection** | Local `EditorPrefs` | A notification and a Console message about newly missing references after scripts or assets change |

Shared settings are saved in `ProjectSettings/SerializeReferenceSharedSettings.asset`, personal ones in **Preferences → Aspid.FastTools → SerializeReference**.

<a id="headless-ci"></a>

## Running in CI

```bash
Unity -batchmode -projectPath . \
  -executeMethod \
  Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt \
  -srGateRequired -srGateFail
```

Exit code `2` means the check itself failed, for example when the report's folder does not exist.

### Command-line flags

| Flag | Behaviour |
|---|---|
| `-srGateReport <path>` | Report path from the project root, `SerializeReferenceGateReport.txt` by default; the folder must exist, the file is overwritten |
| `-srGateRequired` | Also checks unset fields with <code lang="csharp">Required = true</code> |
| `-srGateFail` | Uses `Fail` instead of the project's mode, even `Off` |
| `-srGateWarnOnly` | Uses `Warn` instead of the project's mode, even `Off`; takes precedence over `-srGateFail` if both are passed |

<a id="report-and-exit-codes"></a>

### Report

The header counts the violations and the files that were not scanned because they are not text YAML:

```text
# SerializeReference Gate Report
# Violations: 2
# Not scanned (not text YAML): 2
#   Binary	Assets/Legacy/OldLoadout.prefab
#   LfsPointer	Assets/Levels/Arena.unity
```

Skipped files do not change the exit code.

After the header, each violation occupies one line. Fields are tab-separated:

```text
KIND    assetPath    fileId    rid    className    fieldPath    origin
```

| Field | Contents |
|---|---|
| `KIND` | `MissingType` or `RequiredUnset` |
| `assetPath` | File path, such as `Assets/Presets/BrokenWeaponPreset.asset` |
| `fileId` | Host object ID within the file; for a prefab instance override, the ID of the prefab instance |
| `rid` | Managed-reference ID; in `RequiredUnset` rows, `-2` for an empty <code lang="csharp">[SerializeReference]</code> and `0` for a <code lang="csharp">string</code> or <code lang="class-name">SerializableType</code> |
| `className` | Stored class name for `MissingType`, without separate namespace or assembly fields |
| `fieldPath` | Required field path; for a `MissingType` override, the overridden field when the instance overrides it; otherwise empty |
| `origin` | `override` for a type set by a prefab instance override; otherwise empty |

The asset path, `fileId` and `rid` locate the entry in Asset References; an `override` row is in the **Prefab instance overrides** card of Project References instead.

## Limitations

- **Open copies.** Rewrites skip open scenes, Prefab Mode and assets with unsaved changes. Asset References offers to save an unsaved asset; save and close the scene or Prefab Mode, or repair the field with [Fix in the Inspector](03-serialize-reference-selector.md#repairing-missing-types). `<None>` clears such references in memory, and the scan keeps listing them until they are saved.
- **Prefab instance overrides.** **Fix all**, **Smart Fix**, **Migrate all** and `<None>` do not rewrite them: pick a new class on the instance in the Inspector, or revert the override. Asset References does not show references that exist only in overrides.
- **Scenes and missing parents.** In a scene or under a missing parent reference, Asset References repairs only missing types; change other fields in the Inspector.
- **Required in scenes.** Scenes are read from YAML: fields of components and by-value containers are checked, <code lang="csharp">[SerializeReference]</code> fields themselves included; fields inside managed references, collections and prefab instance overrides are not. A field absent from the scene file is not a violation.

## Package sample

Missing types, a <code lang="csharp">[MovedFrom]</code> rename and a shared reference for both windows are in the assets of the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
