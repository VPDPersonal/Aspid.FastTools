# SerializeReference Selector

Pick an implementation right in the Inspector — from a searchable list, without a custom editor.

<a id="inspector-type-dropdown"></a>

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary =&#10;    new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

## Which classes are offered

An <code lang="class-name">IWeapon</code> field offers concrete implementations of that interface, such as <code lang="class-name">Pistol</code> and <code lang="class-name">Shotgun</code>. Selecting one creates an instance; `<None>` clears the field.

For extra constraints, required fields and list appearance, see [TypeSelector](03-type-selector.md).

- <code lang="csharp">Allow</code> has no effect here: the types must be instantiable. Analyzer `AFT0002` reports this redundant setting.
- Generic arguments are inferred from the field type; when they cannot be, the picker asks for types Unity can serialize.
- Incompatible constraints leave the list empty; analyzers `AFT0003`, `AFT0005` and `AFT0009` report them at compile time.

## Lists

In a list with <code lang="csharp">[TypeSelector]</code>, “+” opens the class picker and adds a new instance, and `<None>` adds an empty element. With several objects selected, each gets its own instance, all in one Undo group.

![“+” on Sidearms opens the class picker and adds a Shotgun](Images/aspid_fasttools_serialize_reference_list.gif)

## Switching the class

When switching classes, FastTools tries to carry compatible values of fields with the same names. Fields that cannot be transferred keep the new instance's values:

| Field | <code lang="class-name">Pistol</code> | → <code lang="class-name">Shotgun</code> |
|---|---|---|
| **Damage** | 37 | 37 |
| **Magazine Size** | 12 | — |
| **Pellets** | — | 8, the initial value |

![Switching from Pistol to Shotgun keeps Damage = 37](Images/aspid_fasttools_serialize_reference_selector.gif)

A nested reference carries over as the same instance when the field name and type are compatible.

Dragging a `.cs` file from **Project** onto the field header is another way to select a class.

## Header menu

Right-click the field header:

| Item | What it does |
|---|---|
| **Copy Serialize Reference** | Copies the field's class and data; the copy lasts until the next domain reload |
| **Paste Serialize Reference** | Pastes the copy into a field of a compatible type; a copied empty field clears it |
| **Find Usages of Pistol** | Searches the project for the class through Unity Search |
| **Create New Script…** | Creates a <code lang="csharp">[Serializable]</code> class for the field type and assigns it after compilation |
| **Save as Template…** | Saves the value under a name; templates live in the editor settings on this machine, not in the project |
| **Paste Template → …** | Creates an instance from a template; only templates that fit the field are listed |

> [!WARNING]
> Copy/Paste and templates do not carry nested <code lang="csharp">[SerializeReference]</code> fields: the pasted object loses those references.

**Paste Template → Remove Missing (N)…** deletes templates whose class no longer loads. The item appears when such templates exist.

## Shared references

**Link to Existing → …** in the header menu links the field to an instance from another field on the same object.

Two fields of an object can point at one instance: an edit through one shows in the other. Such fields are marked **Shared reference #N**, and **Make unique** gives the field its own copy, nested references included.

![Make unique creates an independent copy of a shared reference](Images/aspid_fasttools_serialize_reference_make_unique.png)

A duplicated list element gets its own instance instead of a reference to the same one. The **Auto de-alias duplicated list elements** setting under **Tools → Aspid 🐍 → FastTools → Settings** controls this and is on by default.

## Missing type

If the field shows **Missing type**, see [SerializeReference repair](06-serialize-reference-tooling.md) for **Fix**, bulk repair and the differences in data preservation and Undo.

## Custom inspector

In your own editor, a regular <code lang="class-name">PropertyField</code> draws a <code lang="csharp">[TypeSelector]</code> field: the class picker and the list's “+” come by themselves, with no package call.

| UI Toolkit — CreateInspectorGUI | IMGUI — OnInspectorGUI |
|---|---|
| <pre lang="csharp"><code>new PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"))</code></pre> | <pre lang="csharp"><code>EditorGUILayout.PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"));</code></pre> |

For fields without the attribute or list elements drawn separately, use [SerializeReferenceEditorGUI](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI) or [SerializeReferenceIMGUIList](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList).

## Package sample

For weapon selection, lists and shared references, see the [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md) sample.
