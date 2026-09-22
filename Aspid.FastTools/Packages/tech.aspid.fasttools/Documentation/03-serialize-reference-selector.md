# SerializeReference Selector

Choose an interface or base-class implementation directly in a `[SerializeReference]` field. The selector creates an instance, expands its fields, and carries compatible data over when switching types. This page covers individual Inspector fields; project audits, bulk repair, and CI are covered in [SerializeReference Tooling](04-serialize-reference-tooling.md).

<a id="inspector-type-dropdown"></a>

## Quick start

Add `[TypeSelector]` next to `[SerializeReference]` to choose implementations in a searchable window without writing a custom editor.

| Creation in code | Selection in the Inspector |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary = new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

The selector stores an **instance with data**. To store only a class name and create the object later from code, use [Serializable Type System](02-serializable-types.md).

![Switching from Pistol to Shotgun preserves Damage = 37 and adds Pellets](Images/aspid_fasttools_serialize_reference_selector.gif)

Switching from Pistol to Shotgun preserves Damage = 37 and adds Pellets

A ready-made scene with weapons, effects, and nested modifiers is included in the [SerializeReferences sample](../Samples~/SerializeReferences/Documentation/README.md).

## Configuring selection

The field type sets the base compatibility: `IWeapon` offers its implementations, while an abstract class offers concrete subclasses. Apply `[Serializable]` to classes whose data Unity should persist.

| Task | Configuration |
|---|---|
| Offer only melee weapons | `[TypeSelector(typeof(IMelee))]` on an `IWeapon` field: candidates must fit the field and implement `IMelee` |
| Warn when a field is empty | `[TypeSelector(Required = true)]` |
| Drive constraints from another field | `[TypeSelector(nameof(_category))]`; see [dynamic constraints](02-serializable-types.md#dynamic-base-types-via-member-references) |
| Change the name, group, tooltip, or icon | `[TypeSelectorDisplay(...)]` on the class |
| Hide an implementation from normal selection | `[TypeSelectorDisplay(Hidden = true)]` |

`TypeSelector.Allow` is ignored on `[SerializeReference]`: the selector instantiates concrete classes. Interfaces, abstract classes, structs, `string`, delegates, and `UnityEngine.Object` subclasses cannot be the created value.

### Display name and group

Add the attribute to `Shotgun` from the example:

```csharp
[Serializable]
[TypeSelectorDisplay(
    Name = "Shotgun",
    Group = "Weapons/Ranged",
    Tooltip = "A weapon that fires multiple pellets")]
public sealed class Shotgun : IWeapon
{
    [SerializeField, Min(0)] private int _damage = 20;
    [SerializeField, Min(1)] private int _pellets = 6;

    public void Fire() => Debug.Log($"Shotgun: {_damage} dmg, {_pellets} pellets");
}
```

The class appears under **Weapons → Ranged → Shotgun**. Search still matches its real name, `Shotgun`. These labels do not rename the stored type.

`Hidden = true` hides a type from normal selection, but existing values keep rendering and assignment from code remains available. Subclasses do not inherit the setting. See [TypeSelectorDisplay](02-serializable-types.md#typeselectordisplay) for all parameters.

### Required fields

```csharp
[TypeSelector(Required = true)]
[SerializeReference] private IWeapon _primary;
```

An empty field shows **Required reference is not set**. The attribute does not create a value or prevent choosing `<None>`; it also does not replace runtime `null` checks. A missing type is diagnosed separately from an unset field.

To check required fields in CI, enable [`-srGateRequired`](04-serialize-reference-tooling.md#running-in-ci). The normal pre-build check looks for missing types; the scope of `Required` checks is documented in [SerializeReference Tooling](04-serialize-reference-tooling.md#where-required-fields-are-checked).

## Lists and nested references

For arrays and lists, apply both attributes to the collection field. One list can contain different implementations and `null` entries.

```csharp
// Also import: using System.Collections.Generic;

[TypeSelector]
[SerializeReference] private List<IWeapon> _sidearms = new();

[TypeSelector]
[SerializeReference] private IWeapon[] _slots = new IWeapon[2];
```

In a UI Toolkit list, **+** opens the type picker and appends a new instance. Choosing `<None>` appends an empty entry. For the same behaviour in a custom IMGUI Inspector, use `SerializeReferenceIMGUIList.Draw` — see the [example below](#custom-imgui-inspectors).

### Nested selectors without repeated attributes

An inner `[SerializeReference]` field gets a selector automatically. For example, add a weapon that wraps another weapon:

```csharp
[Serializable]
public sealed class DoubleShot : IWeapon
{
    [SerializeReference] public IWeapon Weapon;

    public void Fire()
    {
        Weapon?.Fire();
        Weapon?.Fire();
    }
}
```

Choose **DoubleShot** in `Primary`, then **Pistol** in its **Weapon** field. You do not need to repeat `[TypeSelector]` on `Weapon`. Nested arrays and lists of managed references work the same way.

Automatic drawing covers eight nesting levels, after which Unity's standard drawing takes over. This is a drawing limit, not a restriction on storing deeper graphs. A child field with its own `[TypeSelector]` or `[CustomPropertyDrawer]` keeps that drawer.

## Working with data

### What happens when switching types

The selector creates an instance of the selected class and attempts to carry data over from the previous value. For the quick-start example:

| Field | Pistol before switching | Shotgun after switching |
|---|---|---|
| `_damage` | `37` | `37`: matching name and data shape |
| `_pellets` | Not present | `6`: the new instance's initial value |

Transfer targets compatible serialized fields. Renamed fields and incompatible data structures need a separate migration. Fields absent from the new type are not retained for later: set **Pellets = 12**, switch to `Pistol`, then back to `Shotgun`, and **Pellets** becomes `6`.

Nested `[SerializeReference]` fields with matching names and compatible types retain their existing instances. Switching the outer type does not make those references independent copies.

<details>
<summary>Initial values and constructors</summary>

Creation calls the parameterless constructor, including a non-public one. If there is none, the instance is created without running a constructor, so field initializers cannot be relied on. Keep a parameterless constructor for predictable initial values.

</details>

### Copy / Paste and templates

Right-click the **reference field's header** to open its context menu.

| Action | Result |
|---|---|
| **Copy Serialize Reference** | Stores the current value's type and serializable data |
| **Paste Serialize Reference** | Creates a new instance in a compatible field, respecting its type and additional constraints |
| **Save as Template…** | Saves the current value under a name |
| **Paste Template → name** | Creates an instance from a compatible saved template |

Copying an empty reference is meaningful: the next paste clears the destination. With multiple objects selected, Copy reads the first object's value; selection and Paste create an independent instance per object in one Undo group. A type switch carries data over from each object's own previous value. Check `Required`, `Missing type`, and `Shared reference` notices with a single object selected.

> [!NOTE]
> The clipboard and templates transfer data through `JsonUtility`; they do not copy an entire nested `[SerializeReference]` graph. To separate a shared reference together with its nested managed references, use **Make unique**.

Templates are stored locally in `EditorPrefs` for the current project. They are personal presets and are not shared with the team through Git. Saving under an existing name asks for overwrite confirmation.

### Other header actions

- **Drag a `.cs` file from Project** to assign an instance of its compatible script class. Data transfers follow the same rules as type selection.
- **Find Usages of …** searches for uses of the current type in the project.
- **Create New Script…** saves a serializable class stub compatible with the declared field type. After successful compilation, the selector assigns a new instance. Add your own logic to the stub: interface methods may contain `NotImplementedException`, and abstract base-class members need manual implementation.

## Shared references and Make unique

Two fields on the same component or `ScriptableObject` can point to one instance. Editing its data through either field affects both; the selector labels this **Shared reference**. Sharing may be intentional.

To create a shared reference, open the destination field's context menu and choose **Link to Existing → type and path**. It offers references compatible with the field type within the same host object. This links an existing instance and replaces the destination's previous value.

![Make unique creates an independent copy of a shared reference](Images/aspid_fasttools_serialize_reference_make_unique.png)

Make unique creates an independent copy of a shared reference

Click **Make unique** in the notice or **Make Unique Reference** in the context menu to edit the field independently. Nested managed references are copied too; repeated references within the copy retain their internal sharing.

Automatic splitting after duplicating list entries is controlled by **Auto de-alias duplicated list elements** in [FastTools settings](04-serialize-reference-tooling.md#pre-build-checks). It is enabled by default.

## Generic types

The selector infers generic arguments from the field type where possible. If some arguments remain unknown, the window offers them on the next page.

```csharp
public interface IModifier { }

[Serializable]
public class Modifier<T> : IModifier
{
    public T Value;
}

// T is known: creates Modifier<float>.
[TypeSelector]
[SerializeReference] private Modifier<float> _damageModifier;

// Modifier<T> asks you to choose T in the selector.
[TypeSelector]
[SerializeReference] private IModifier _modifier;
```

Declare the interface and class alongside the other types, and add the fields to `Loadout`. The first field fixes the argument to `float`; for the second, choose an argument such as `int` or `string` on the argument page.

<details>
<summary>Inference through interfaces and argument constraints</summary>

Arguments are also inferred through implemented interfaces: an `IConverter<string, string>` field closes `Sequence<T> : IConverter<T, T>` as `Sequence<String>`.

A candidate is excluded if it cannot be closed to fit the field. For example, `ToString<TFrom> : IConverter<TFrom, string>` does not fit `IConverter<float, float>`. If the output parameter of `IConverter` is covariant, it can fit `IConverter<float, object>`.

An inferred argument must support by-value serialization only where the candidate stores it by value. A parameter behind `[SerializeReference]` follows managed-reference rules. The manual argument page offers serializable types.

</details>

<a id="repairing-broken-references"></a>

## Repairing missing types

Renaming, moving, or deleting a class can leave its stored name unresolved. The field shows **Missing type**. While the reference's data remains in the asset, it can be reassigned to an existing implementation.

![A missing reference with Fix and Smart Fix actions in the Inspector](Images/aspid_fasttools_serialize_reference_repair.png)

A missing reference with Fix and Smart Fix actions in the Inspector

| Action | When to use it |
|---|---|
| **Fix** | You know a suitable replacement: open the picker and select an existing type |
| **Smart Fix** | You want to use the suggested replacement: check the type and reason in the tooltip, then click the suggestion |

Smart Fix considers `[MovedFrom]`, the name, namespace, assembly, and field similarity. It only applies when clicked. The **Fix** picker also permits `Hidden` types: recovering old data may require an implementation removed from normal selection.

For an asset on disk, Fix rewrites the stored type and reimports the asset; that file write has no ordinary Undo. In an open saved scene or Prefab Mode, repair affects the object in memory — verify the result and save the scene or prefab. Preserving data does not automatically convert incompatible fields; in-memory repair also does not guarantee recovery of the entire nested graph.

If Fix is unavailable, select one object and ensure the scene or Prefab Mode is saved with no pending changes. For a prefab instance in a scene, open its source prefab. If the problem is inside a missing parent and the field is inaccessible, use [Asset References](04-serialize-reference-tooling.md#asset-references-inspect-one-asset).

Accompany planned renames with [`[MovedFrom]`](04-serialize-reference-tooling.md#migrations-with-movedfrom). For auditing and repairing multiple assets, see [SerializeReference Tooling](04-serialize-reference-tooling.md).

## Custom IMGUI inspectors

The selector works in IMGUI and UI Toolkit. In a custom IMGUI editor, a regular `PropertyField` uses the field's drawer, but a list's **+** button needs `SerializeReferenceIMGUIList.Draw` to open a type picker.

For `Loadout` with the `_sidearms` field above, put this editor in an `Editor` folder:

```csharp
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.SerializeReferences.Editors;

[CustomEditor(typeof(Loadout))]
public sealed class LoadoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("_primary"), true);

        SerializeReferenceIMGUIList.Draw(
            serializedObject.FindProperty("_sidearms"),
            new GUIContent("Sidearms"),
            typeof(IWeapon));

        serializedObject.ApplyModifiedProperties();
    }
}
```

Add any other fields to the editor as needed. To create controls without `[TypeSelector]`, use `SerializeReferenceEditorGUI.CreateField`, `CreateList`, or `DrawFieldLayout`; a complete editor is included in the [SerializeReferences sample](../Samples~/SerializeReferences/Documentation/README.md#the-imgui-path).

## If a type is missing from the list

Check that the class is concrete, compatible with the field and its additional constraints, does not inherit `UnityEngine.Object`, and is not marked `Hidden = true`. A generic candidate must have valid arguments. After compilation errors, wait for scripts to compile successfully.

Analyzer `AFT0004` reports incompatibility with `UnityEngine.Object`; `AFT0003` and `AFT0009` report constraints no type can meet together; `AFT0005` warns about a potentially empty selector. `Allow` does not broaden the set of instantiable managed references.

`[TypeSelector]` and `[TypeSelectorDisplay]` attributes apply only in the editor. Implementations and their serialized data remain part of the game.
