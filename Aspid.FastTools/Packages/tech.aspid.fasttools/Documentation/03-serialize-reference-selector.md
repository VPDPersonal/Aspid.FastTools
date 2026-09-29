# SerializeReference Selector

Pick an interface implementation right in the Inspector — from a searchable list, without a custom editor.

<a id="inspector-type-dropdown"></a>

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary =&#10;    new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

## Which classes are offered

The list follows the field type; the attribute can narrow it with extra types. For the fields of <code lang="class-name">Loadout</code>:

| Field with <code lang="csharp">[TypeSelector]</code> | Classes in the list |
|---|---|
| <code lang="csharp">IWeapon _primary</code> | Crossbow, Pistol, Railgun, Shotgun, Sword |
| <code lang="csharp">IWeapon _meleeBackup</code> and <code lang="csharp">typeof(IMelee)</code> | Sword |
| <code lang="csharp">StatusEffect _onHit</code>, an abstract class | BurnEffect, FreezeEffect |
| <code lang="csharp">Modifier&lt;float&gt; _damageModifier</code> | DamageModifier, Modifier&lt;Single&gt; |
| <code lang="csharp">List&lt;IModifier&gt; _perks</code> | AmmoModifier, DamageModifier, NameModifier, Modifier&lt;T&gt; with a choice of <code lang="class-name">T</code> |

Only concrete classes that do not derive from <code lang="class-name">UnityEngine.Object</code> are offered. In the Inspector of a runtime object, classes from editor-only assemblies (`UnityEditor`, Editor-only asmdefs, `Editor` folders) are left out: a player build cannot create them. Generic arguments are inferred from the field type; when they cannot be, the window asks for each one and offers only types Unity can serialize. A constraint can also come [from another field](02-serializable-types.md#constraint-from-another-field): <code lang="csharp">[TypeSelector(nameof(_category))]</code>.

## How a class appears in the list

<code lang="csharp">[TypeSelectorDisplay]</code> on a class changes only its row in the list:

| Parameter on <code lang="class-name">Shotgun</code> | In the list |
|---|---|
| <code lang="csharp">Group = "Weapons/Ranged"</code> | Weapons → Ranged → Shotgun |
| <code lang="csharp">Name = "Scattergun"</code> | Scattergun; search still finds Shotgun |
| <code lang="csharp">Tooltip = "Fires pellets"</code> | Tooltip on hover |
| <code lang="csharp">Icon = "Icons/Shotgun"</code> | Icon from `Resources`, an asset path or a built-in one |
| <code lang="csharp">Hidden = true</code> | Not listed; a Shotgun already assigned stays in the field |

Subclasses do not inherit these settings.

## Required field

```csharp
[TypeSelector(Required = true)]
[SerializeReference] private IWeapon _primary;
```

An empty field shows **Required reference is not set**; `<None>` can still be chosen. In CI, the [`-srGateRequired`](04-serialize-reference-tooling.md#running-in-ci) flag checks such fields.

## Lists and nested fields

In a list with <code lang="csharp">[TypeSelector]</code>, **+** opens the class picker and adds a new instance; `<None>` adds an empty element; with several objects selected, each gets its own instance in one Undo group. A <code lang="csharp">[SerializeReference]</code> field inside the chosen class — such as <code lang="csharp">_chargeEffect</code> on <code lang="class-name">Railgun</code> — gets the selector without the attribute.

A nested field keeps its own drawer instead of the automatic selector when it has <code lang="csharp">[TypeSelector]</code>, an attribute with a `[CustomPropertyDrawer]`, or a `[CustomPropertyDrawer]` for its declared type, a base class, an interface or an open generic type. In a list such a drawer draws each element, and **+** still opens the class picker. A drawer for the chosen class alone, such as <code lang="class-name">Pistol</code>, does not replace the selector.

## Switching the class

The new instance receives the values of fields with the same names:

| Field | <code lang="class-name">Pistol</code> | → <code lang="class-name">Shotgun</code> |
|---|---|---|
| <code lang="csharp">_damage</code> | <code lang="csharp">37</code> | <code lang="csharp">37</code> |
| <code lang="csharp">_magazineSize</code> | <code lang="csharp">12</code> | — |
| <code lang="csharp">_pellets</code> | — | <code lang="csharp">8</code>, the initial value |

![Switching from Pistol to Shotgun keeps Damage = 37](Images/aspid_fasttools_serialize_reference_selector.gif)

Switching from Pistol to Shotgun keeps Damage = 37

A nested reference with the same name moves to the new class as the same instance, not a copy.

## Header menu

Right-click the field header:

| Item | What it does |
|---|---|
| **Copy / Paste Serialize Reference** | Moves the class and data to another compatible field; a copied empty field clears the target on paste |
| **Save as Template…**, **Paste Template** | Saves the value under a name and creates an instance from it; templates stay on this machine |
| **Paste Template → Remove Missing (N)…** | Deletes, after confirmation, templates whose class does not load; shown only when there are any |
| **Link to Existing** | Points the field at the instance of another field on the same object |
| **Find Usages of Pistol** | Searches the project for the class through Unity Search |
| **Create New Script…** | Creates a <code lang="csharp">[Serializable]</code> class for the field type and assigns it after compilation |

A `.cs` script dragged from Project onto the header assigns its class and carries data over, as picking from the list does.

> [!WARNING]
> Copy/Paste and templates do not carry nested <code lang="csharp">[SerializeReference]</code> fields: a copied <code lang="class-name">Railgun</code> pastes without its <code lang="csharp">_chargeEffect</code>.

## Shared references

Two fields of an object can point at one instance: an edit through one shows in the other. Such fields are marked **Shared reference #N**, and **Make unique** gives the field its own copy, nested references included.

![Make unique creates an independent copy of a shared reference](Images/aspid_fasttools_serialize_reference_make_unique.png)

Make unique creates an independent copy of a shared reference

A duplicated list element gets its own copy automatically — that is the **Auto de-alias duplicated list elements** setting in the [shared settings](04-serialize-reference-tooling.md#shared-and-personal-settings).

<a id="repairing-broken-references"></a>

## Repairing missing types

After a class is renamed, moved or deleted, the field shows **Missing type**, while the data stays in the asset.

![A missing reference with Fix and the → Pistol? suggestion in the Inspector](Images/aspid_fasttools_serialize_reference_repair.png)

A missing reference with Fix and the → Pistol? suggestion in the Inspector

| Action | What it does |
|---|---|
| **Fix** | Opens the class picker, including classes hidden with <code lang="csharp">Hidden</code> |
| **→ Pistol?** | Assigns the suggested class; the tooltip gives the reason: [`[MovedFrom]`](04-serialize-reference-tooling.md#migrations-with-movedfrom), the same or a similar name, shared fields |

For an asset on disk the repair rewrites the file, and Undo does not revert it. In a scene or Prefab Mode it applies in memory and restores only top-level fields. Until you save, Undo brings the missing reference back with its data; saving (Prefab Mode Auto Save included) makes the repair final and clears that object's Undo history. If the asset has unsaved changes, Fix first offers to save it, because the reimport would discard them.

Fix is unavailable with several objects selected, in an unsaved scene, and on a component inherited from a prefab. When the class is missing in a source prefab, repair it there (the tooltip names it); when the instance sets the class through an override, choose a new class on the instance or revert the override. While a prefab is open in Prefab Mode, Fix on its asset in the Project window is refused: repair the field in Prefab Mode. For everything else use [SerializeReference Tooling](04-serialize-reference-tooling.md).

## Custom IMGUI inspector

In your own IMGUI editor, a list gets **+** with a class picker through <code lang="csharp">SerializeReferenceIMGUIList.Draw</code>; a regular <code lang="csharp">PropertyField</code> draws the other fields.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>EditorGUILayout.PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"));</code></pre> | <pre lang="csharp"><code>SerializeReferenceIMGUIList.Draw(&#10;    serializedObject&#10;        .FindProperty("_sidearms"),&#10;    new GUIContent("Sidearms"),&#10;    typeof(IWeapon));</code></pre> |

Extra <code lang="csharp">[TypeSelector]</code> constraints go in the following arguments: <code lang="csharp">Draw</code> does not read them from the field.

## Limitations

- **Depth.** Nested references get the selector down to the eighth level; deeper, Unity draws the field.
- **Constructor.** An instance is created with the parameterless constructor, non-public included; without one, field initializers do not run.
- **<code lang="csharp">Allow</code> on <code lang="csharp">[SerializeReference]</code>** has no effect — analyzer `AFT0002` reports it.
- **Empty list.** When no class meets the constraints, analyzers `AFT0003`, `AFT0005` and `AFT0009` report it, and a field whose type derives from <code lang="class-name">UnityEngine.Object</code> is error `AFT0004`.

## Package sample

The <code lang="class-name">Loadout</code> fields from this page and assets with missing types to try **Fix** on are in the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
