# Serializable Type System

Type selection in the Inspector. The type persists with a component or asset and is read in code as `System.Type`. The wrappers store the type only — your code creates the instance. For an instance with editable data, use [SerializeReference Selector](03-serialize-reference-selector.md).

## Quick start

Unity does not serialize a `System.Type` field directly. Instead of manually filling and resolving a string, declare `SerializableType<T>`. The argument `T` constrains selection to compatible types. This example uses `Collider`:

| Before — a type-name string | After — SerializableType |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _colliderTypeName;&#10;&#10;public System.Type ColliderType =&gt;&#10;    string.IsNullOrEmpty(_colliderTypeName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _colliderTypeName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Collider&gt;&#10;    _colliderType;&#10;&#10;public System.Type ColliderType =&gt;&#10;    _colliderType?.Type;</code></pre> |

The wrapper already has a picker; the attribute here excludes abstract classes and interfaces.

![Selecting a serializable type in the Inspector](Images/serializable-type-quick-start.gif)

Selecting a serializable type in the Inspector

## Choosing a tool

| Task | Tool |
|---|---|
| Store a type, including generic types and types declared inside another class | [`SerializableType`](#serializabletype) |
| Retain selection when renaming a class and its file | [`SerializableMonoScript`](#serializablemonoscript) |
| Add a type picker to a string or constrain a field | [`TypeSelector`](#typeselectorattribute) |
| Store an instance in `[SerializeReference]` | [SerializeReference Selector](03-serialize-reference-selector.md) |
| Customize a candidate's name, group, icon, or visibility | [`TypeSelectorDisplay`](#typeselectordisplay) |
| Open the window from editor code | [`TypeSelectorWindow`](#typeselectorwindow) |

## SerializableType

`SerializableType` stores an assembly-qualified name: the type name together with its assembly.

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

The type must be compatible with `T`, otherwise the constructor throws `ArgumentException`. Pass `null` for an empty wrapper; there is no public parameterless constructor.

| Property or call | Result |
|---|---|
| `Type` | The resolved `System.Type`; `null` for an empty selection or unresolved name |
| `AssemblyQualifiedName` | The stored name, even if the type is missing; an empty string for no selection |
| `BaseType` | `typeof(object)`, or `typeof(T)` for the generic variant |
| `ToString()` | The resolved type's short name; otherwise the stored name |

### Empty values and renames

Renaming a class, namespace, or assembly can break the stored name. The Inspector then shows `<Missing>`; check `.Type` for `null` before using it.

> [!NOTE]
> Unity serializes a wrapper by the field's declared type. Assigning `SerializableType<T>` to a `SerializableType` field preserves the selected type after loading, but loses the `T` constraint. Declare the generic variant on the field itself. The same rule applies to `SerializableMonoScript<T>`.

## SerializableMonoScript

`SerializableMonoScript` links the selected type to a script asset, retaining selection when the class and file are renamed or moved together. Choose the type in the Inspector or drag its `.cs` file from **Project**.

| Stored name | Script-asset reference |
|---|---|
| <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableMonoScript&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> |

| Capability | SerializableType | SerializableMonoScript |
|---|---|---|
| Searchable picker | Yes | Yes, only types backed by a suitable MonoScript |
| Generic types and types declared inside another class | Yes | No |
| Built-in Unity types without a `MonoScript` asset, such as `BoxCollider` | Yes | No |
| Name update after a script rename | Manual | From the stored MonoScript during serialization |
| Construction from a `Type` in code | Public constructor | No public constructor |
| In a player | Type name | Type name; the MonoScript reference is editor-only |

`BoxCollider` ships in the `UnityEngine.PhysicsModule` assembly, so there is no corresponding script asset in the project.

The script must declare a top-level, non-generic class in a matching file, and `MonoScript.GetClass()` must return that class. Keep the asset and its `.meta` when renaming. If Unity can no longer resolve the class, the wrapper retains the last known name.

Read the selected type through `.Type` or implicit conversion to `System.Type`, as with `SerializableType`.

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

`IDamageable` is your interface. `_damageableType` offers components that both inherit `MonoBehaviour` and implement `IDamageable`. All constraints apply together (**AND**) on every kind of field. Arrays and lists get a picker for each entry.

On `[SerializeReference]`, the field type is the first constraint. Suppose `Sword` implements `IWeapon` and `IMelee`, and `Glaive` implements `IWeapon`, `IMelee` and `IRanged`:

```csharp
[TypeSelector(typeof(IMelee), typeof(IRanged))]
[SerializeReference] private IWeapon _weapon;
```

The field offers only `Glaive`: `Sword` does not implement `IRanged`. To allow a fixed set of classes, give them a common interface and pass it: listing the classes themselves (`typeof(Pistol), typeof(Rifle)`) leaves the picker empty, and analyzer `AFT0009` reports it. See [instance selector configuration](03-serialize-reference-selector.md#configuring-selection).

### Constructors and properties

| Property | Default | Behaviour |
|---|---|---|
| `Allow` | `TypeAllow.All` | `Abstract` adds abstract classes; `Interface` adds interfaces. `All` enables both categories; `None` excludes them. Ignored on `[SerializeReference]` |
| `Required` | `false` | Warns about an empty type name or a `null` managed reference |

Static classes are excluded. On a string or wrapper, `Allow` filters type categories without checking for a parameterless constructor.

<details>
<summary>TypeSelector argument forms</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(MonoBehaviour))]
[TypeSelector(typeof(MonoBehaviour), typeof(IDamageable))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_category))]
```

Apply one `[TypeSelector]` per field. It accepts `Type` or `string` arguments: one value, multiple comma-separated values (`params`), or an array. Without arguments, it adds no constraints. A string is first resolved as a field or property name, then as a type name if no such member exists.

</details>

### The Required notice

```csharp
[TypeSelector(Required = true, Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider> _requiredType;
```

![An empty required field shows a notice beside the picker](Images/type-selector-required.png)

An empty required field shows a notice beside the picker

With `Required = true`, `<None>` remains selectable: clearing the field shows a warning beside it. For strings and wrappers, the check tests for an empty stored name; a missing type with a nonempty name passes this check.

For project-wide and CI validation, see [required-field checks](04-serialize-reference-tooling.md#where-required-fields-are-checked).

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

Use `typeof` for a type and `nameof` for a field or property. If a string names neither an object member nor an available type, the Inspector shows a warning.

![The typo _categroy instead of _category triggers a warning. Use nameof(_category) to avoid this mistake.](Images/type-selector-constraint-warning.png)

The typo _categroy instead of _category triggers a warning. Use nameof(_category) to avoid this mistake.

## TypeSelectorDisplay

`TypeSelectorDisplay` customizes a type's label, group, icon, and tooltip in the picker:

```csharp
using Aspid.FastTools.Types;

[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```

![The Damage × name, icon, and Combat/Modifiers group in the picker](Images/type-selector-display.png)

The Damage × name, icon, and Combat/Modifiers group in the picker

| Property | Result |
|---|---|
| `Name` | Caption in the list and closed field. Search still matches the real type name |
| `Group` | Grouping instead of the namespace; `/` separates levels, such as `Combat/Melee` |
| `Tooltip` | Text shown on hover |
| `Icon` | An `EditorGUIUtility.IconContent` name, an asset path with extension, or a `Resources` path without extension |
| `Hidden` | When `true`, hides the type from normal selection. Not inherited; code assignment and display of stored values still work |

> [!NOTE]
> `TypeSelectorDisplay` depends on `UNITY_EDITOR` in the assembly where the attribute is applied. A class compiled into an external DLL without that symbol carries none of these settings, including `Hidden`.

## TypeSelectorWindow

The picker groups types by namespace or `Group` and distinguishes identical names by assembly. Use `TypeSelectorWindow` to open it from a custom inspector or editor window.

![Favorites and Recent on the picker root page](Images/type-selector-window.png)

Favorites and Recent on the picker root page

| Action | Control |
|---|---|
| Move / select / close | Arrow keys / Enter / Escape |
| Return to the parent group | Left arrow or breadcrumbs |
| Toggle a favourite | Space or the star on hover |
| Clear the value | `<None>` |

Configure **Favorites**, **Recent**, and history capacity in the FastTools window's **Settings** tab.

### Generic types

Picking an open generic type opens its argument pages and returns a constructed closed type. For example, choosing `int` for `Container<T>` produces `Container<int>`. A generic argument can itself be generic; the window resolves its parameters first.

![Choosing a generic type argument in the picker](Images/type-selector-generic.gif)

Choosing a generic type argument in the picker

Arguments must satisfy the generic parameter's constraints; `[Serializable]` is not required. For `[SerializeReference]`, see the [serialization and inference rules](03-serialize-reference-selector.md#generic-types).

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

`currentAqn` controls the current mark: an empty string marks `<None>`, while `null` leaves selection unmarked.

### Window filters

`TypeSelectorFilter` is a struct. Its `default` has `Allow = None`, unlike the `[TypeSelector]` attribute, which defaults to `All`. Set the mode explicitly when you need abstract classes or interfaces.

<details>
<summary>Window filter properties</summary>

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

Use `Predicate` to narrow the list; `AdditionalTypes` adds candidates that bypass constraints.

</details>

For a window that edits assets, see [EditorTools](../Samples~/EditorTools/Documentation/README.md).

## Troubleshooting selection

| Symptom | What to check |
|---|---|
| A class is missing | Compatibility with the base and every constraint, `Allow`, `Hidden`, and compilation errors |
| A type appears in SerializableType but not SerializableMonoScript | Whether it has a separate script file and `MonoScript.GetClass()` returns the intended class |
| Changing Category leaves the old value | Constraints change the candidate list, not the dependent field's stored value |
| `<Missing>` with a nonempty name | Whether the class, namespace, or assembly changed; select an existing type again |
| Required does not warn about a missing type | For strings and wrappers, it checks an empty name rather than successful resolution |
| A type is selected but no object appears | Storing a type does not instantiate it; use your creation code or [SerializeReference Selector](03-serialize-reference-selector.md) |

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md).

![A wave of regular and elite enemies moves toward the center.](../Samples~/Types/Documentation/Images/demo.gif)

A wave of regular and elite enemies moves toward the center.
