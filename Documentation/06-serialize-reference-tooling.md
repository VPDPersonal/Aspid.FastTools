# SerializeReference repair

Restore missing references and type names after classes are renamed, moved or deleted.

<a id="check-the-project"></a>

## Quick start

1. Open **Tools → Aspid 🐍 → FastTools → Project References**.
2. Click **Scan Project**: missing references are grouped by stored class.
3. Click **Fix all** in a group, pick a class and confirm with **Rewrite**.

## Choosing a repair method

| Where to repair | Data preserved | Undo |
|---|---|---|
| **Fix** in an asset's Inspector | Entry data in the file | None |
| **Fix** in a scene or Prefab Mode | Only flat top-level fields | Undo until saved |
| **Asset References** | Entry data in the file | None |
| **Project References** | Group entries' data in the files | Undo button in the rewrite summary |

A file rewrite preserves entry data, but the selected class must fit the field and its data. Project References also repairs the names that <code lang="class-name">SerializableType</code> and <code lang="class-name">SerializableMonoScript</code> fields store, see [Type names](#type-names). To catch new breakages in the Editor, before a build or in CI, see [Build and CI checks](07-serialize-reference-validation.md).

## Fix in the Inspector

After a class is renamed, moved or deleted, the field shows **Missing type**, while the data stays in the asset.

![A missing reference with Fix and the → Pistol suggestion in the Inspector](Images/aspid_fasttools_serialize_reference_repair.png)

| Action | What it does |
|---|---|
| **Fix** | Opens the class picker, including classes hidden with <code lang="csharp">Hidden</code> |
| **→ Pistol** | Assigns the suggested class; the tooltip gives the reason: the same name, the same name in another case, or a similar name |

> [!WARNING]
> On an asset, Fix rewrites the file, and Undo does not revert it.
>
> In a scene or Prefab Mode the repair stays in memory: Undo reverts it, and saving makes it final and clears the object's Undo history. Such a repair brings back only flat top-level fields: nested objects, arrays, lists, vectors, colours and object references get their default values.

### When there is no Fix

| Case | What to do |
|---|---|
| Several objects selected | Select one: until then **Missing type** is not shown |
| Unsaved changes in the scene or Prefab Mode | Save: until then the field shows `<None>` without **Missing type** |
| Prefab instance, class stored in the source prefab | Repair the source prefab; the tooltip names it |
| Prefab instance, class set through an override | Choose a new class on the instance or revert the override |

<a id="bulk-repair-tabs"></a>

## Project References: repair a group

Project References and Asset References are tabs of one window. **Scan Project** reads the `.prefab`, `.asset` and `.unity` files under `Assets/`, apart from [**Excluded scan folders**](07-serialize-reference-validation.md#scan-scope).

![Project References with Fix all, Smart Fix → Pistol and Migrate all groups](Images/aspid_fasttools_serialize_reference_project_references.png)

### Group actions

| Group | Header button | Row under the header |
|---|---|---|
| Missing type | **Fix all ▼** — pick a class for every entry | **Smart Fix → Pistol** — apply a class with the same or a similar name; the tooltip gives the reason |
| Renamed with <code lang="csharp">[MovedFrom]</code> | **Reassign all ▼** — pick a different class instead of the new name | **Migrate all → Crossbow** — write the new name, see [Migrations](#migrations-with-movedfrom) |

Every action asks for **Rewrite**. `<None>` in the class picker clears the group's references and deletes their data, fields sharing the same `rid` included; it asks for **Clear** and has no Undo.

### What repair preserves

A repair rewrites only the entry's class, namespace and assembly; its data and `rid` stay. When the group's fields have different types, the pick may not fit every entry: incompatible ones become <code lang="csharp">null</code> on reimport.

The summary after a rewrite has **Undo**: it restores the old class on entries that still hold the new one. It is gone after **Rescan**, closing the window or a domain reload; **Edit → Undo** does not revert a rewrite.

### Prefab instance overrides

A missing class set through a prefab instance override — in a variant, a nested prefab or an instance in a scene — is listed in a separate **Prefab instance overrides** card.

![Prefab instance overrides card with a missing GhostRailgun in the EliteLoadout variant](Images/aspid_fasttools_serialize_reference_prefab_overrides.png)

**Fix all**, **Smart Fix**, **Migrate all** and `<None>` do not rewrite these entries: pick a new class on the instance in the Inspector, or revert the override. Asset References does not show references that exist only in overrides.

### Type names

A <code lang="class-name">SerializableType</code> or <code lang="class-name">SerializableMonoScript</code> field whose stored name no longer resolves goes to a group marked **type name**. A group holds one stored name and lists its fields by asset and field path. Fields in managed references and prefab instance overrides are listed too.

| Action | What it does |
|---|---|
| **Fix all ▼** | Opens the type picker with the constraint of the fields and writes the picked type to every field of the group |
| **Smart Fix → Spear** | Writes the only compatible type with the same class name |

Every action asks for **Rewrite**, and the summary has **Undo**. A <code lang="class-name">SerializableMonoScript</code> field also gets the script of the picked type. A prefab instance override is rewritten as well, because it stores the name as a plain value.

The scan finds a field by its `_assemblyQualifiedName` key. A <code lang="csharp">[TypeSelector]</code> string has no such key: repair it in the Inspector. Asset References does not list type names.

## Asset References: inspect one asset

Assign a saved prefab, ScriptableObject or scene to the field next to **Rescan**, or click an entry row in Project References.

![Asset References with a missing GhostCrossbow, a SHARED Pistol and an orphaned Railgun entry](Images/aspid_fasttools_serialize_reference_asset_references.png)

| Label | Meaning |
|---|---|
| Band with **Fix Missing ▼** | The stored class is not found; the button opens the class picker |
| **Smart Fix → Pistol** row | A class picked as by **Smart Fix** in Project References |
| **Migrate → Crossbow** row under a **Fix ▼** band | The class was renamed with <code lang="csharp">[MovedFrom]</code>: the row writes the new name, **Fix ▼** picks a different class |
| Band with **Change ▼**, **Assign ▼** or **Assign Required ▼** | Changes the class of a healthy reference, fills an empty or required field; the asset is saved at once |
| **SHARED** | Several fields point at one instance; matching colours mark the connected fields |
| **Orphaned** | An entry no field points at; **Clear** deletes it from the file, without Undo |

**Fix Missing**, **Smart Fix** and **Migrate** write the class to the file at once, without confirmation, and **Edit → Undo** does not revert it.

![GhostWeapon is repaired as Pistol in Asset References](Images/aspid_fasttools_serialize_reference_tooling.gif)

## Migrations with MovedFrom

When <code lang="class-name">CrossbowLauncher</code> is renamed to <code lang="class-name">Crossbow</code> with <code lang="csharp">[MovedFrom]</code>, Unity loads the old references itself. **Migrate all** writes the new name into the files so the attribute can be removed:

| In the file — before Migrate all | After |
|---|---|
| `type: {class: CrossbowLauncher, …}` | `type: {class: Crossbow, …}` |

A group becomes a pending migration only when exactly one class that fits the field lists the old name in <code lang="csharp">[MovedFrom]</code> and the stored class is not a closed generic; the build checks do not count such a group as missing.

Remove <code lang="csharp">[MovedFrom]</code> only when no file stores the old name any more. **Migrate all** does not rewrite it:

- in prefab instance overrides;
- in open, unsaved and locked files, see [limitations](#limitations);
- in **Excluded scan folders**;
- in binary assets and Git LFS files that were not fetched;
- in files outside `Assets/`.

## Limitations

| Where | Limitation |
|---|---|
| Open scenes, Prefab Mode, unsaved and locked files | Rewrites skip them: save and close the file, or use [Fix in the Inspector](#fix-in-the-inspector) with its data-transfer limitations |
| Scenes and fields under a missing parent reference | Asset References changes only missing types |
| Binary assets and unfetched Git LFS files | Not scanned: use **Force Text** and fetch LFS files |

## Package sample

Missing types, a <code lang="csharp">[MovedFrom]</code> rename and a shared reference for both tabs are in the assets of the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
