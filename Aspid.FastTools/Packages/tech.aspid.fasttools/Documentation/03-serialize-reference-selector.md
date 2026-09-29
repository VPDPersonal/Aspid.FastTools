# SerializeReference Selector

Pick an implementation right in the Inspector — from a searchable list, without a custom editor.

<a id="inspector-type-dropdown"></a>

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary =&#10;    new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

## Which classes are offered

The list follows the field type; the attribute can narrow it with extra types.

```csharp
public interface IMelee : IWeapon { }
public interface IRanged : IWeapon { }
public sealed class Sword : IMelee { }

public abstract class StatusEffect { }

public class Modifier<T> : IModifier { }
public sealed class DamageModifier : Modifier<float> { }
```

| <code lang="class-name">Loadout</code> field with <code lang="csharp">[TypeSelector]</code> | Classes in the list |
|---|---|
| <code lang="csharp">IWeapon _primary;</code> | <code lang="class-name">Crossbow</code>, <code lang="class-name">Pistol</code>, <code lang="class-name">Railgun</code>, <code lang="class-name">Shotgun</code>, <code lang="class-name">Sword</code> |
| <code lang="csharp">IWeapon _meleeBackup;</code> and <code lang="csharp">typeof(IMelee)</code> | <code lang="class-name">Sword</code> |
| <code lang="csharp">StatusEffect _onHit;</code>, an abstract class | <code lang="class-name">BurnEffect</code>, <code lang="class-name">FreezeEffect</code> |
| <code lang="csharp">Modifier&lt;float&gt; _damageModifier;</code> | <code lang="class-name">DamageModifier</code>, <code lang="class-name">Modifier&lt;Single&gt;</code> |
| <code lang="csharp">List&lt;IModifier&gt; _perks;</code> | <code lang="class-name">AmmoModifier</code>, <code lang="class-name">DamageModifier</code>, <code lang="class-name">NameModifier</code>, <code lang="class-name">Modifier&lt;T&gt;</code> with a choice of <code lang="class-name">T</code> |

- Only concrete classes that do not derive from <code lang="class-name">UnityEngine.Object</code> are offered.
- In the Inspector of a runtime object, classes from editor-only assemblies (`UnityEditor`, Editor-only asmdefs, `Editor` folders) are left out: a player build cannot create them.
- Generic arguments are inferred from the field type; when they cannot be, the window asks for each one and offers only types Unity can serialize.
- A constraint can also come [from another field](02-serializable-types.md#constraint-from-another-field).
- A class's row in the list is set by [`[TypeSelectorDisplay]`](02-serializable-types.md#typeselectordisplay).

## Required field

```csharp
[TypeSelector(Required = true)]
[SerializeReference] private IWeapon _primary;
```

With <code lang="csharp">Required = true</code>, an empty field shows **Required reference is not set**; see [Required field](02-serializable-types.md#required-field).

## Lists

In a list with <code lang="csharp">[TypeSelector]</code>, “+” opens the class picker and adds a new instance, and `<None>` adds an empty element. With several objects selected, each gets its own instance, all in one Undo group.

## Switching the class

The new instance receives the values of fields with the same names:

| Field | <code lang="class-name">Pistol</code> | → <code lang="class-name">Shotgun</code> |
|---|---|---|
| **Damage** | 37 | 37 |
| **Magazine Size** | 12 | — |
| **Pellets** | — | 8, the initial value |

![Switching from Pistol to Shotgun keeps Damage = 37](Images/aspid_fasttools_serialize_reference_selector.gif)

A nested reference with the same name moves to the new class as the same instance, not a copy.

## Header menu

Right-click the field header:

| Item | What it does |
|---|---|
| **Copy Serialize Reference** | Copies the field's class and data; the copy lasts until the next domain reload |
| **Paste Serialize Reference** | Pastes the copy into a field of a compatible type; a copied empty field clears it |
| **Make Unique Reference** | Gives the field its own copy of a [shared reference](#shared-references); not shown on an unshared one |
| **Find Usages of Pistol** | Searches the project for the class through Unity Search |
| **Link to Existing → …** | Points the field at the instance of another field on the same object |
| **Create New Script…** | Creates a <code lang="csharp">[Serializable]</code> class for the field type and assigns it after compilation |
| **Save as Template…** | Saves the value under a name; templates live in the editor settings on this machine, not in the project |
| **Paste Template → …** | Creates an instance from a template; only templates that fit the field are listed |
| **Paste Template → Remove Missing (N)…** | Deletes templates whose class does not load; shown only when there are any |

A `.cs` script dragged from Project onto the header assigns its class and carries data over, as picking from the list does.

> [!WARNING]
> Copy/Paste and templates do not carry nested <code lang="csharp">[SerializeReference]</code> fields: a copied <code lang="class-name">Railgun</code> pastes without its <code lang="csharp">_chargeEffect</code>.

## Shared references

Two fields of an object can point at one instance: an edit through one shows in the other. Such fields are marked **Shared reference #N**, and **Make unique** gives the field its own copy, nested references included.

![Make unique creates an independent copy of a shared reference](Images/aspid_fasttools_serialize_reference_make_unique.png)

A duplicated list element gets its own instance instead of a reference to the same one. The **Auto de-alias duplicated list elements** setting in the [shared settings](04-serialize-reference-tooling.md#shared-and-personal-settings) controls this and is on by default.

<a id="repairing-broken-references"></a>

## Repairing missing types

After a class is renamed, moved or deleted, the field shows **Missing type**, while the data stays in the asset.

![A missing reference with Fix and the → Pistol suggestion in the Inspector](Images/aspid_fasttools_serialize_reference_repair.png)

| Action | What it does |
|---|---|
| **Fix** | Opens the class picker, including classes hidden with <code lang="csharp">Hidden</code> |
| **→ Pistol** | Assigns the suggested class; the tooltip gives the reason: [`[MovedFrom]`](04-serialize-reference-tooling.md#migrations-with-movedfrom), the same name, the same name in another case, or a similar name |

On an asset, Fix rewrites the file, and Undo does not revert it. In a scene or Prefab Mode the repair stays in memory: Undo reverts it, and saving makes it final and clears the object's Undo history.

> [!WARNING]
> An in-memory repair brings back only flat top-level fields: nested objects, arrays, lists, vectors and colours get their default values.

### When there is no Fix

| Case | What to do |
|---|---|
| Several objects selected | Select one |
| Unsaved changes in the scene or Prefab Mode | Save — Fix appears |
| Prefab instance, class stored in the source prefab | Repair the source prefab; the tooltip names it |
| Prefab instance, class set through an override | Choose a new class on the instance or revert the override |
| Prefab asset in Project while the prefab is open in Prefab Mode | Repair the field in Prefab Mode |

[SerializeReference Tooling](04-serialize-reference-tooling.md) repairs everything else.

## Custom inspector

In your own editor, a regular <code lang="class-name">PropertyField</code> draws a <code lang="csharp">[TypeSelector]</code> field: the class picker and the list's “+” come by themselves, with no package call.

| UI Toolkit — CreateInspectorGUI | IMGUI — OnInspectorGUI |
|---|---|
| <pre lang="csharp"><code>new PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"))</code></pre> | <pre lang="csharp"><code>EditorGUILayout.PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"));</code></pre> |

If a <code lang="csharp">[SerializeReference]</code> field has no <code lang="csharp">[TypeSelector]</code> attribute, or a list element is drawn on its own through <code lang="function">GetArrayElementAtIndex</code>, <code lang="class-name">PropertyField</code> shows no class picker. Your own editor can draw it by calling one of these methods:

| Method | Draws |
|---|---|
| <code lang="csharp">SerializeReferenceEditorGUI.CreateField()</code> | A field in <code lang="function">CreateInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceEditorGUI.CreateList()</code> | A list in <code lang="function">CreateInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceEditorGUI.DrawFieldLayout()</code> | A field in <code lang="function">OnInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceIMGUIList.Draw()</code> | A list in <code lang="function">OnInspectorGUI</code> |

Constraints on top of the field type go in the <code lang="csharp">baseTypes</code> argument, like the types in <code lang="csharp">[TypeSelector(...)]</code>.

## Limitations

- **Constructor.** An instance is created with the parameterless constructor, non-public included; without one, field initializers do not run.
- **<code lang="csharp">Allow</code> on <code lang="csharp">[SerializeReference]</code>** has no effect — analyzer `AFT0002` reports it.
- **No candidates.** When no class meets the constraints, analyzers `AFT0003`, `AFT0005` and `AFT0009` report it, and a field whose type derives from <code lang="class-name">UnityEngine.Object</code> is error `AFT0004`.

## Package sample

The <code lang="class-name">Loadout</code> fields from this page and assets with missing types to try **Fix** on are in the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
