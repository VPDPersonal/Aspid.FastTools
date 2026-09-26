# Serializable Type System

A `SerializableType<T>` field lists the types assignable to `T` in the Inspector, stores the chosen type with the component or asset, and returns it in code as `System.Type`. Your code creates the instance from that type.

## Quick start

The examples on this page add fields to the `WeaponMount` component and use a weapon hierarchy:

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }

public sealed class WeaponMount : MonoBehaviour { }
```

| Before — a type-name string | After — SerializableType |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon?.Type;</code></pre> |

The wrapper has a picker without the attribute; `Allow = TypeAllow.None` removes the abstract `Weapon`, `MeleeWeapon` and `RangedWeapon` from the list.

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
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

The type must be compatible with `T`, otherwise the constructor throws `ArgumentException`. Pass `null` for an empty wrapper; there is no public parameterless constructor.

| Property or call | `primary` | `empty` | Missing type |
|---|---|---|---|
| `Type` | `typeof(Sword)` | `null` | `null` |
| `AssemblyQualifiedName` | `Sword`'s name with its assembly | `""` | The stored name |
| `BaseType` | `typeof(Weapon)` | `typeof(Weapon)` | `typeof(Weapon)` |
| `ToString()` | `"Sword"` | `""` | The stored name |

A missing type is a stored name that no longer resolves after a class, namespace, or assembly rename; the Inspector shows it as `<Missing …>` with that name. Without `T`, `SerializableType.BaseType` is `typeof(object)`. For a resolved type, `ToString()` returns `Type.Name`, so a generic type reads ``Amplify`1``, not the picker's caption.

> [!NOTE]
> Unity serializes a wrapper by the field's declared type. Assigning `SerializableType<T>` to a `SerializableType` field preserves the selected type after loading, but loses the `T` constraint. Declare the generic variant on the field itself. The same rule applies to `SerializableMonoScript<T>`.

## SerializableMonoScript

`SerializableMonoScript` links the selected type to a script asset, retaining selection when the class and file are renamed or moved together. Choose the type in the Inspector or drag its `.cs` file from **Project**.

| Stored name | Script-asset reference |
|---|---|
| <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableMonoScript&lt;Weapon&gt;&#10;    _primaryWeapon;</code></pre> |

| Capability | SerializableType | SerializableMonoScript |
|---|---|---|
| Searchable picker | Yes | Yes, only types backed by a suitable script |
| Generic types and types declared inside another class | Yes | No |
| Types without their own `.cs` in the project: from DLLs and Unity modules | Yes | No |
| Name update after a script rename | Manual | From the stored MonoScript during serialization |
| Construction from a `Type` in code | Public constructor | No public constructor |

A suitable script is a file in a runtime assembly that declares a top-level, non-generic class named after the file: `Sword.cs` for `Sword`. Keep the asset and its `.meta` when renaming. If Unity can no longer resolve the class, the wrapper retains the last known name.

Read the selected type through `.Type` or implicit conversion to `System.Type`, as with `SerializableType`. In a player, the wrapper also stores just the type name.

## TypeSelectorAttribute

The attribute configures field selection and adds a picker to a plain string.

| Field | Selection result |
|---|---|
| `string` | Stores the assembly-qualified name |
| `SerializableType` / `SerializableMonoScript` | Configures the wrapper's selection |
| `[SerializeReference]` | Creates an instance of the selected implementation |

### Constraints and collections

```csharp
[TypeSelector(typeof(Weapon), Allow = TypeAllow.None)]
[SerializeField] private string _backupWeaponName;

[TypeSelector(typeof(ITwoHanded), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<MeleeWeapon> _heavyWeapon;

[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Weapon>[] _loadout;
```

`_heavyWeapon` offers only `Axe`: `Sword` does not implement `ITwoHanded`, and `Bow` does not inherit `MeleeWeapon`. All constraints apply together (**AND**) on every kind of field. Arrays and lists get a picker for each entry.

On `[SerializeReference]`, the field type is the first constraint, so this field offers only `Axe` as well:

```csharp
[TypeSelector(typeof(ITwoHanded))]
[SerializeReference] private MeleeWeapon _heldWeapon;
```

To allow a fixed set of classes, give them a common interface or base class and pass it: listing the classes themselves (`typeof(Sword), typeof(Axe)`) leaves the picker empty, and analyzer `AFT0009` reports it. See [instance selector configuration](03-serialize-reference-selector.md#configuring-selection).

### Constructors and properties

| Property | Default | Behaviour |
|---|---|---|
| `Allow` | `TypeAllow.All` | `Abstract` adds abstract classes; `Interface` adds interfaces. `All` enables both categories; `None` excludes them. Ignored on `[SerializeReference]` |
| `Required` | `false` | Warns about an empty type name or a `null` managed reference |

Static classes are excluded. On a string or wrapper, `Allow` filters type categories without checking for a parameterless constructor.

In the Inspector of a runtime object, the picker also leaves out types from editor-only assemblies (`UnityEditor`, Editor-only asmdefs and `Editor` folders): a player build cannot resolve them. Fields of editor-only objects, such as editor windows and settings, still offer every type.

<details>
<summary>TypeSelector argument forms</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(Weapon))]
[TypeSelector(typeof(MeleeWeapon), typeof(ITwoHanded))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_weaponClass))]
```

Apply one `[TypeSelector]` per field. It accepts `Type` or `string` arguments: one value, multiple comma-separated values (`params`), or an array; one attribute cannot mix `Type` and `string`. Without arguments, it adds no constraints. A string is first resolved as a member of the class that declares the field, then as a type name if no such member exists.

</details>

### Required field

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![An empty required field shows a warning below the picker](Images/type-selector-required.png)

An empty required field shows a warning below the picker

With `Required = true`, `<None>` remains selectable. For strings and wrappers, the check tests for an empty stored name; a missing type with a nonempty name passes this check.

For project-wide and CI validation, see [required-field checks](04-serialize-reference-tooling.md#where-required-fields-are-checked).

## Dynamic base types via member references

Pass `nameof(...)` to let a field or property's current value control the candidate list:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Choose `MeleeWeapon` in **Weapon Class**, and **Weapon Name** offers `Sword` and `Axe`. Changing a constraint does not clear an earlier selection; review the dependent field and select a new type if needed.

| Constraint source | Support |
|---|---|
| `System.Type` | One type |
| `string` | A type name resolved through `Type.GetType` |
| `SerializableType`, `SerializableMonoScript`, and their generic variants | The resolved `.Type` value |
| An array of these values | Multiple simultaneous constraints; `List<T>` is not supported |

The source must be an instance field or readable property of the class that declares the attributed field, inherited members included; indexers are not supported. For a field inside a `[Serializable]` class or a list element, the source is read from that same instance. An empty or unresolved source contributes no constraint. A generic wrapper's own `T` continues to constrain selection.

Analyzers catch mistakes in string arguments: `AFT0006` when a string names neither a member nor a type, `AFT0007` when the member cannot supply base types, and `AFT0008` when the string is not a valid type name. If a well-formed type name refers to a type that is not loaded, the Inspector shows a warning.

![The constraint did not resolve, so the Inspector shows a warning below the field](Images/type-selector-constraint-warning.png)

The constraint did not resolve, so the Inspector shows a warning below the field

## TypeSelectorDisplay

`TypeSelectorDisplay` customizes a type's label, group, icon, and tooltip in the picker. Add a `SerializableType<CombatModifier> _modifier` field to `WeaponMount` and configure how `DamageModifier` appears in it:

```csharp
using Aspid.FastTools.Types;

public abstract class CombatModifier { }

[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier : CombatModifier { }
```

![The Damage × name, icon, and Combat/Modifiers group in the picker](Images/type-selector-display.png)

The Damage × name, icon, and Combat/Modifiers group in the picker

| Property | Result |
|---|---|
| `Name` | Caption in the list and closed field. Search still matches the real type name |
| `Group` | Grouping instead of the namespace; `/` separates levels |
| `Tooltip` | Text shown on hover |
| `Icon` | An `EditorGUIUtility.IconContent` name, an asset path starting with `Assets/` or `Packages/` with extension, or a `Resources` path without extension |
| `Hidden` | When `true`, hides the type from normal selection. Not inherited; code assignment and display of stored values still work |

> [!NOTE]
> `[TypeSelector]` and `[TypeSelectorDisplay]` are marked `[Conditional("UNITY_EDITOR")]`. Classes compiled into an external DLL without that symbol carry none of their settings, including `Hidden`.

## TypeSelectorWindow

The picker groups types by namespace or `Group` and distinguishes identical names by assembly. Use `TypeSelectorWindow` to open it from a custom inspector or editor window.

![Favorites and Recent on the picker root page](Images/type-selector-window.png)

Favorites and Recent on the picker root page

| Action | Control |
|---|---|
| Move / select | Up and Down arrows / Enter |
| Enter a group / go back | Right arrow / Left arrow or breadcrumbs |
| Search | Start typing |
| Toggle a favourite | Space or the star on hover |
| Clear the value | `<None>` |
| Close | Escape; while searching, the first presses clear and collapse the search |

The **Favorites** section and the **Recent** history length (0 hides it) are set in the FastTools window's **Settings** tab, which also clears both lists. The gear in the picker opens that tab.

### Generic types

Picking an open generic type opens its argument pages and returns a constructed closed type. For example, for `Amplify<T> : CombatModifier` with `where T : StatusEffect`, the window offers the subclasses of `StatusEffect`, and choosing `Burning` stores `Amplify<Burning>` in `_modifier`. A generic argument can itself be generic; the window resolves its parameters first. When every argument can be inferred from the field type, the closed type is returned immediately.

![Choosing a generic type argument in the picker](Images/type-selector-generic.gif)

Choosing a generic type argument in the picker

Arguments must satisfy the generic parameter's constraints; interfaces, abstract classes, and hidden types are not offered as arguments, and `[Serializable]` is not required. For `[SerializeReference]`, see the [serialization and inference rules](03-serialize-reference-selector.md#generic-types).

### Opening from code

`screenRect` is the button rectangle in **screen coordinates**, and `selectedTypeName` is the current type-name string:

```csharp
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

TypeSelectorWindow.Show(
    screenRect,
    new TypeSelectorFilter
    {
        Types = new[] { typeof(Weapon) },
        Allow = TypeAllow.None
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

The callback receives an assembly-qualified name, or `null` for `<None>`. Dismissing the window without a choice does not invoke it.

`currentAqn` controls the current mark: an empty string (the default) marks `<None>`, while `null` leaves selection unmarked, as does a name missing from the list, so Enter right after opening cannot erase a missing type's stored name.

### Window filters

`TypeSelectorFilter` is a struct. In its `default`, an empty `Types` admits any type and `Allow` is `None`, unlike the `[TypeSelector]` attribute, which defaults to `All`. Set `Allow` explicitly when you need abstract classes or interfaces.

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

</details>

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md); for a picker opened from editor code, see [EditorTools](../Samples~/EditorTools/Documentation/README.md).

![A wave of regular and elite enemies moves toward the center.](../Samples~/Types/Documentation/Images/demo.gif)

A wave of regular and elite enemies moves toward the center.
