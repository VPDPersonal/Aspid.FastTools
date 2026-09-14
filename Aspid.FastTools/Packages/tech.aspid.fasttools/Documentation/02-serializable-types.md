# Serializable Type System

Choose a type in the Inspector, persist it with a component or asset, and read it as `System.Type` in code. The wrappers store the selected type; your code creates the instance. To store an instance with editable data, use [SerializeReference Selector](03-serialize-reference-selector.md).

## Quick start

Unity does not serialize a `System.Type` field directly. Instead of manually filling and resolving a string, declare `SerializableType<T>`. The argument `T` constrains selection to compatible types. This example uses `Collider`:

| Before — a type-name string | After — SerializableType |
|---|---|
| <pre lang="csharp"><code>[SerializeField]<br />private string _colliderTypeName;<br /><br />public System.Type ColliderType =&gt;<br />    string.IsNullOrEmpty(_colliderTypeName)<br />        ? null<br />        : System.Type.GetType(<br />            _colliderTypeName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]<br />[SerializeField]<br />private SerializableType&lt;Collider&gt;<br />    _colliderType;<br /><br />public System.Type ColliderType =&gt;<br />    _colliderType?.Type;</code></pre> |

The right-hand version requires `using UnityEngine;` and `using Aspid.FastTools.Types;`. The wrapper already has a picker; the attribute here excludes abstract classes and interfaces.

### Example: add the selected Collider

Save this code as `ColliderSpawner.cs`:

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public sealed class ColliderSpawner : MonoBehaviour
{
    [TypeSelector(Allow = TypeAllow.None)]
    [SerializeField] private SerializableType<Collider> _colliderType;

    private void Start()
    {
        var type = _colliderType?.Type;
        if (type == null || type.IsAbstract || type.ContainsGenericParameters)
            return;
        if (!typeof(Collider).IsAssignableFrom(type)) return;

        gameObject.AddComponent(type);
    }
}
```

1. Add `ColliderSpawner` to a GameObject.
2. Open **Collider Type**, search for **BoxCollider**, and select it.
3. Enter Play Mode: a **Box Collider** appears on the object.
4. Exit Play Mode and choose `<None>`: on the next run, the component adds nothing.

![Selecting a serializable type in the Inspector](Images/aspid_fasttools_serializable_type.gif)

Selecting a serializable type in the Inspector

Ready-made enemy-type and spawn-pattern scenarios are included in the [Types sample](../Samples~/Types/Documentation/README.md).

## Choosing a tool

| Task | Tool |
|---|---|
| Store a type, including nested and generic types | [`SerializableType`](#serializabletype) |
| Retain selection when renaming a class and its file | [`SerializableMonoScript`](#serializablemonoscript) |
| Add a type picker to a string or constrain a field | [`TypeSelector`](#typeselectorattribute) |
| Store an instance in `[SerializeReference]` | [SerializeReference Selector](03-serialize-reference-selector.md) |
| Customize a candidate's name, group, icon, or visibility | [`TypeSelectorDisplay`](#typeselectordisplay) |
| Open the window from editor code | [`TypeSelectorWindow`](#typeselectorwindow) |
| Switch the component or ScriptableObject's own type | [`ComponentTypeSelector`](#componenttypeselector) |

## SerializableType

`SerializableType` stores an assembly-qualified name: the type name together with its assembly. `Type` resolves the string lazily and caches the resulting `System.Type`; deserialization resets the cache. The wrapper does not store the selected class's fields.

| Variant | Selection constraint |
|---|---|
| `SerializableType` | No base-type constraint |
| `SerializableType<T>` | Types assignable to `T`, including interface implementations |

Both variants convert implicitly to `System.Type` and have a public constructor taking a `Type`:

```csharp
var selected = new SerializableType<Collider>(typeof(BoxCollider));
System.Type type = selected;

var empty = new SerializableType<Collider>(null);
```

`new SerializableType<Collider>(typeof(string))` throws `ArgumentException`. There is no public parameterless constructor: pass `null` to create an empty wrapper from code, or let Unity initialize a serialized field.

| Property or call | Result |
|---|---|
| `Type` | The resolved `System.Type`; `null` for an empty selection or unresolved name |
| `AssemblyQualifiedName` | The stored name, even if the type is missing; an empty string for no selection |
| `BaseType` | `typeof(object)`, or `typeof(T)` for the generic variant |
| `ToString()` | The resolved type's short name; otherwise the stored name |

### Empty values and renames

Check `Type` before using it. Both an empty selection and a missing type return `null`, but a missing type retains its `AssemblyQualifiedName`, and the Inspector shows `<Missing>`. Renaming a class, namespace, or assembly can make the old string unresolvable; storing a name does not provide migration by itself.

> [!NOTE]
> Unity serializes a wrapper by the field's declared type. Assigning `SerializableType<T>` to a `SerializableType` field preserves the selected type after loading, but loses the `T` constraint. Declare the generic variant on the field itself. The same rule applies to `SerializableMonoScript<T>`.

## SerializableMonoScript

Use this family when the selected type is backed by a script file. In the editor, the wrapper stores a `MonoScript` reference and refreshes the type name from that asset during serialization. Selection survives renames and moves while Unity recognizes the intended class in the same script asset.

| Stored name | Script-asset reference |
|---|---|
| <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]<br />[SerializeField]<br />private SerializableType&lt;MonoBehaviour&gt;<br />    _componentType;</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]<br />[SerializeField]<br />private SerializableMonoScript&lt;MonoBehaviour&gt;<br />    _componentType;</code></pre> |

| Capability | SerializableType | SerializableMonoScript |
|---|---|---|
| Searchable picker | Yes | Yes, only types backed by a suitable MonoScript |
| Nested and generic types | Yes | No |
| Types without a separate script, such as BoxCollider | Yes | No |
| Name update after a script rename | Manual | From the stored MonoScript during serialization |
| Construction from a `Type` in code | Public constructor | No public constructor |
| In a player | Type name | Type name; the MonoScript reference is editor-only |

Choose a type through search or drag its `.cs` file from **Project**. It must be a class returned by `MonoScript.GetClass()`: top-level, non-generic, in a matching file. Rename the class and file together, retaining the asset and its `.meta`. If the script is deleted or no longer resolves its class, the wrapper keeps the last known name but does not restore the class automatically.

Both families derive from `SerializableTypeBase`, but `SerializableMonoScript` does not derive from `SerializableType`. Both expose `.Type` and implicit conversion to `System.Type`; configure `SerializableMonoScript` through the Inspector.

## TypeSelectorAttribute

The attribute configures field selection. Wrappers already have a picker without the attribute; on a plain string, it adds one.

| Field | Selection result |
|---|---|
| `string` | Stores the assembly-qualified name |
| `SerializableType` / `SerializableMonoScript` | Configures the wrapper's selection |
| `[SerializeReference]` | Creates an instance of the selected implementation |

### Constraints and collections

```csharp
[TypeSelector(typeof(MonoBehaviour), Allow = TypeAllow.None)]
[SerializeField] private string _componentTypeName;

[TypeSelector(typeof(IDamageable), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<MonoBehaviour> _damageableType;

[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider>[] _colliderTypes;
```

`IDamageable` is your interface. `_damageableType` offers components that both inherit `MonoBehaviour` and implement `IDamageable`. For strings and wrappers, multiple constraints intersect: a candidate must satisfy **every** constraint. Arrays and lists get a picker for each entry.

For `[SerializeReference]`, see the [instance selector rules](03-serialize-reference-selector.md#configuring-selection). The attribute has `[Conditional("UNITY_EDITOR")]`, so its usage is omitted from player builds.

### Constructors and properties

| Property | Default | Behaviour |
|---|---|---|
| `Allow` | `TypeAllow.All` | `Abstract` adds abstract classes; `Interface` adds interfaces. `All` enables both categories; `None` excludes them. Ignored on `[SerializeReference]` |
| `Required` | `false` | Warns about an empty type name or a `null` managed reference |

`TypeAllow.None` means concrete types, not an empty list. Static classes are not offered. This filter does not check for the constructor your code needs: account for your factory's requirements before creating an ordinary C# object.

<details>
<summary>TypeSelector argument forms</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(MonoBehaviour))]
[TypeSelector(typeof(MonoBehaviour), typeof(IDamageable))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_category))]
```

These are alternatives, not attributes to stack on one field. Constructors accept a `Type`, a `Type` array, a string, or a string array. A string identifies an object member or a type name with its assembly.

</details>

### The Required notice

```csharp
[TypeSelector(Required = true, Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider> _requiredType;
```

![An empty required field shows a notice beside the picker](Images/aspid_fasttools_type_selector_required.png)

An empty required field shows a notice beside the picker

`Required` does not select a type automatically, block `<None>`, or replace validation in code. For strings and wrappers, it checks an **empty stored name**: a nonempty name for a missing type is a different problem and does not become a `Required` violation.

Project-wide required-field checks run in [Project References](04-serialize-reference-tooling.md#where-required-fields-are-checked) when validation is enabled. CI requires [`-srGateRequired`](04-serialize-reference-tooling.md#running-in-ci); the normal player pre-build check does not validate these fields.

## Dynamic base types via member references

Pass `nameof(...)` to let a field or property's current value control the candidate list. For example, a base category and a dependent selection:

```csharp
[SerializeField] private SerializableType<MonoBehaviour> _category;

[TypeSelector(nameof(_category), Allow = TypeAllow.None)]
[SerializeField] private string _componentTypeName;
```

Change **Category**, then open **Component Type Name**: the list is constrained to the selected type and its subclasses. Changing a constraint does not clear an earlier selection by itself; review the dependent field and select a new type if needed.

| Constraint source | Support |
|---|---|
| `System.Type` | One type |
| `string` | A type name resolved through `Type.GetType` |
| `SerializableType`, `SerializableMonoScript`, and their generic variants | The resolved `.Type` value |
| An array of these values | Multiple simultaneous constraints |

The source must be an instance field or readable property on the object being edited. Inherited members work; indexers do not. An empty source contributes no constraint. A generic wrapper's own `T` continues to constrain selection.

The string is resolved as an object member first, then as a type name. Prefer `typeof` for an accessible type and `nameof` for a member. Analyzer rules `AFT0006`–`AFT0008` validate member references; if a constraint cannot be resolved while the Inspector is running, the field shows a notice.

## TypeSelectorDisplay

The attribute changes a candidate's presentation while retaining its real name and identity in stored data. For an ordinary class:

```csharp
using Aspid.FastTools.Types;

[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```

![The Damage × name, icon, and Combat/Modifiers group in the picker](Images/aspid_fasttools_type_selector_display.png)

The Damage × name, icon, and Combat/Modifiers group in the picker

| Property | Result |
|---|---|
| `Name` | Caption in the list and closed field. Search still matches the real type name |
| `Group` | Grouping instead of the namespace; `/` separates levels, such as `Combat/Melee` |
| `Tooltip` | Text shown on hover |
| `Icon` | An `EditorGUIUtility.IconContent` name, an asset path with extension, or a `Resources` path without extension |
| `Hidden` | When `true`, hides the type from normal selection. Not inherited; code assignment and display of stored values still work |

Empty `Name` and `Group` values retain the default presentation; `null` for `Tooltip` or `Icon` supplies no override. Generic captions retain their argument list. A class stored only as a type name does not need `[Serializable]`; if stored as a `[SerializeReference]` instance, serialization requirements apply separately.

Manual **Fix** and bulk repair can offer hidden types. **Smart Fix** does not suggest them: hiding a type from normal selection must not prevent recovery of older data.

> [!NOTE]
> `TypeSelectorDisplay` depends on `UNITY_EDITOR` in the assembly where the attribute is applied. A class compiled into an external DLL without that symbol carries none of these settings, including `Hidden`.

## TypeSelectorWindow

The shared picker used by fields is also a public API for custom inspectors and `EditorWindow` tools. Types are grouped by namespace or `Group`; search finds candidates without manual navigation. The window disambiguates identical names by assembly.

![Favorites and Recent on the picker root page](Images/aspid_fasttools_type_selector_window.png)

Favorites and Recent on the picker root page

| Action | Control |
|---|---|
| Move / select / close | Arrow keys / Enter / Escape |
| Return to the parent group | Left arrow or breadcrumbs |
| Toggle a favourite | Space or the star on hover |
| Clear the value | `<None>` |

**Favorites** and **Recent** are stored locally per project in `EditorPrefs` and hidden while searching. Configure visibility and history capacity in the FastTools window's **Settings** tab. The current type has a check mark, and groups show candidate counts.

### Generic types

Picking an open generic type opens its argument pages and returns a constructed closed type. For example, choosing `int` for `Container<T>` produces `Container<int>`. A generic argument can itself be generic; the window resolves its parameters first.

![Choosing a generic type argument in the picker](Images/aspid_fasttools_type_selector_generic.gif)

Choosing a generic type argument in the picker

Arguments must satisfy the parameter's base-type, interface, and `where` constraints. The ordinary type-name picker does not require `[Serializable]` on an argument: it stores a name rather than a value of that type. `[SerializeReference]` adds its own [serialization and inference rules](03-serialize-reference-selector.md#generic-types).

### Opening from code

In an editor script, import `Aspid.FastTools.Types.Editors`. `screenRect` is the button rectangle in **screen coordinates**, and `selectedTypeName` is the current type-name string:

```csharp
TypeSelectorWindow.Show(
    screenRect,
    new TypeSelectorFilter
    {
        Types = new[] { typeof(MonoBehaviour) },
        Allow = TypeAllow.None
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

The callback receives an assembly-qualified name, or `null` for `<None>`. Dismissing the window without a choice does not assign a value. If the result belongs to an asset, write it through `SerializedProperty` and apply the changes.

<details>
<summary>Complete IMGUI window with a picker button</summary>

Create `TypePickerWindow.cs` in an `Editor` folder and open **Tools → Type Picker**. It displays the selected name without creating a component:

```csharp
using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

public sealed class TypePickerWindow : EditorWindow
{
    [SerializeField] private string _selectedTypeName;

    [MenuItem("Tools/Type Picker")]
    private static void Open() => GetWindow<TypePickerWindow>("Type Picker");

    private void OnGUI()
    {
        var caption = string.IsNullOrEmpty(_selectedTypeName)
            ? "<None>"
            : Type.GetType(_selectedTypeName, false)?.Name ?? "<Missing>";
        var rect = GUILayoutUtility.GetRect(new GUIContent(caption), GUI.skin.button);

        if (GUI.Button(rect, caption))
        {
            TypeSelectorWindow.Show(
                GUIUtility.GUIToScreenRect(rect),
                new TypeSelectorFilter
                {
                    Types = new[] { typeof(MonoBehaviour) },
                    Allow = TypeAllow.None
                },
                _selectedTypeName,
                aqn =>
                {
                    _selectedTypeName = aqn;
                    Repaint();
                });
        }
    }
}
```

</details>

### Window filters

`TypeSelectorFilter` is a struct. Its `default` has `Allow = None`, unlike the `[TypeSelector]` attribute, which defaults to `All`. Set the mode explicitly when you need abstract classes or interfaces.

| Property | Purpose |
|---|---|
| `Types` | Base types every candidate must satisfy |
| `Allow` | Allowed categories: abstract classes and interfaces |
| `Predicate` | An additional condition after type and category checks |
| `AdditionalTypes` | Candidates that bypass `Types`, `Allow`, and `Predicate`; `Hidden` filtering remains |
| `ArgumentFilter` | An additional filter for manually selected arguments |
| `InferredArgumentFilter` | A filter for arguments inferred from the field type |
| `IncludeHidden` | Offer types marked `Hidden = true` |
| `HideNoneOption` | Hide `<None>` on the root page |

`AdditionalTypes` is not an extra constraint: use `Predicate` to narrow the list. `currentAqn` controls the current mark; an empty string selects the empty row, while `null` leaves selection unmarked.

For a window that edits assets, see [EditorTools](../Samples~/EditorTools/Documentation/README.md).

## ComponentTypeSelector

This selector changes the **component or ScriptableObject itself**, rather than storing a type for later creation. Add the field to a base class: selection is restricted to concrete types assignable to the class that declares the field. `<None>` is hidden.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField, Min(0)] private float _health = 100f;

    public abstract void Attack();
}
```

Save the base class as `EnemyBase.cs`. Create subclasses in **separate files** matching their class names:

| FastEnemy.cs | ArmoredEnemy.cs |
|---|---|
| <pre lang="csharp"><code>using UnityEngine;<br /><br />public sealed class FastEnemy : EnemyBase<br />&#123;<br />    [SerializeField] private float _speed = 25f;<br /><br />    public override void Attack() =&gt;<br />        Debug.Log($"Speed: &#123;_speed&#125;");<br />&#125;</code></pre> | <pre lang="csharp"><code>using UnityEngine;<br /><br />public sealed class ArmoredEnemy : EnemyBase<br />&#123;<br />    [SerializeField] private int _armor = 10;<br /><br />    public override void Attack() =&gt;<br />        Debug.Log($"Armor: &#123;_armor&#125;");<br />&#125;</code></pre> |

Add **FastEnemy** to a GameObject, set **Health = 75**, and select **ArmoredEnemy** in the picker. The shared `Health` remains, `Speed` disappears, and `Armor` appears. Do not assume fields unique to the previous class will survive a later switch back.

![Switching a component type with ComponentTypeSelector](Images/aspid_fasttools_component_type_selector.gif)

Switching a component type with ComponentTypeSelector

The editor replaces `m_Script` through Unity serialization. This is not a general migration between incompatible classes: review shared and new fields after switching. The selected class must have its own `MonoScript`, whose `GetClass()` returns that exact class. If no suitable script is found, the type stays unchanged and the Console receives a warning.

In UI Toolkit inspectors, the standard **Script** row is hidden while the selector is present; IMGUI inspectors still draw that row themselves. Try type switching and shared-field preservation in the [Types sample](../Samples~/Types/Documentation/README.md).

## Troubleshooting selection

| Symptom | What to check |
|---|---|
| A class is missing | Compatibility with the base and every constraint, `Allow`, `Hidden`, and compilation errors |
| A type appears in SerializableType but not SerializableMonoScript | Whether it has a separate script file and `MonoScript.GetClass()` returns the intended class |
| Changing Category leaves the old value | Constraints change the candidate list, not the dependent field's stored value |
| `<Missing>` with a nonempty name | Whether the class, namespace, or assembly changed; select an existing type again |
| Required does not warn about a missing type | For strings and wrappers, it checks an empty name rather than successful resolution |
| A type is selected but no object appears | Storing a type does not instantiate it; use your creation code or SerializeReference Selector |

## Next steps

- [Types](../Samples~/Types/Documentation/README.md) — a working scene with enemies, dependent selection, and component replacement.
- [SerializeReference Selector](03-serialize-reference-selector.md) — instances with data, nested references, and repair.
- [SerializeReference Tooling](04-serialize-reference-tooling.md) — project validation and CI.
