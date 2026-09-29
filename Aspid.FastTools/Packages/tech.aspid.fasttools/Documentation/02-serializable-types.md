# Serializable Type System

A class type as an ordinary field: Unity saves it, and the Inspector picks it from a list.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Selecting a serializable type in the Inspector](Images/serializable-type-quick-start.gif)

Selecting a serializable type in the Inspector

## SerializableType

<code lang="class-name">SerializableType</code> stores an assembly-qualified name: the type name together with its assembly.

| Variant | Selection constraint |
|---|---|
| <code lang="class-name">SerializableType</code> | No base-type constraint |
| <code lang="csharp">SerializableType&lt;T&gt;</code> | Types assignable to <code lang="class-name">T</code>, including interface implementations |

Both variants convert implicitly to <code lang="csharp">System.Type</code> and are created in code with a constructor taking a <code lang="class-name">Type</code>; a type incompatible with <code lang="class-name">T</code> throws <code lang="class-name">ArgumentException</code>:

```csharp
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

A missing type is a stored name that no longer resolves after a class, namespace, or assembly rename; the Inspector shows it as `<Missing …>` with that name.

| Property or call | <code lang="csharp">primary</code> | <code lang="csharp">empty</code> | Missing type |
|---|---|---|---|
| <code lang="csharp">Type</code> | <code lang="csharp">typeof(Sword)</code> | <code lang="csharp">null</code> | <code lang="csharp">null</code> |
| <code lang="csharp">AssemblyQualifiedName</code> | <code lang="class-name">Sword</code>'s name with its assembly | <code lang="csharp">""</code> | The stored name |
| <code lang="csharp">BaseType</code> | <code lang="csharp">typeof(Weapon)</code> | <code lang="csharp">typeof(Weapon)</code> | <code lang="csharp">typeof(Weapon)</code> |
| <code lang="csharp">ToString()</code> | <code lang="csharp">"Sword"</code> | <code lang="csharp">""</code> | The stored name |

- Without <code lang="class-name">T</code>, <code lang="class-name">SerializableType</code>'s <code lang="csharp">BaseType</code> is <code lang="csharp">typeof(object)</code>.
- For a resolved type, <code lang="csharp">ToString()</code> returns <code lang="csharp">Type.Name</code>, so a generic type reads <code lang="string">Amplify`1</code>, not the picker's caption.

> [!NOTE]
> Unity serializes a wrapper by the field's declared type. Assigning <code lang="csharp">SerializableType&lt;T&gt;</code> to a <code lang="class-name">SerializableType</code> field preserves the selected type after loading, but loses the <code lang="class-name">T</code> constraint. Declare the generic variant on the field itself. The same rule applies to <code lang="csharp">SerializableMonoScript&lt;T&gt;</code>.

## SerializableMonoScript

<code lang="class-name">SerializableMonoScript</code> links the selected type to a script asset, retaining selection when the class and file are renamed or moved together. The field is declared the same way: <code lang="csharp">SerializableMonoScript&lt;Weapon&gt; _primaryWeapon</code>. Choose the type in the Inspector or drag its `.cs` file from **Project**.

| Capability | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| Searchable picker | Yes | Yes, only types backed by a suitable script |
| Generic types and types declared inside another class | Yes | No |
| Types without their own `.cs` in the project: from DLLs and Unity modules | Yes | No |
| Name update after a script rename | Manual | From the stored MonoScript during serialization |
| Construction from a <code lang="class-name">Type</code> in code | Public constructor | No public constructor |

A suitable script is a file in a runtime assembly that declares a top-level, non-generic class named after the file: `Sword.cs` for <code lang="class-name">Sword</code>.

- Keep the asset and its `.meta` when renaming.
- If the stored name no longer resolves, the editor takes the type from the script, in Play Mode too.
- If Unity can no longer resolve the class, the wrapper retains the last known name.

> [!WARNING]
> In a player, <code lang="class-name">SerializableType</code> and <code lang="class-name">SerializableMonoScript</code> find the type by its stored name, and managed code stripping does not see names stored in scenes and assets. From **Managed Stripping Level** Low up, the build may drop a class referenced only from the Inspector, and <code lang="csharp">.Type</code> then returns <code lang="csharp">null</code> while the editor resolves it. Mark such classes <code lang="csharp">[Preserve]</code> (`UnityEngine.Scripting`) or list them in `link.xml`. The same applies to <code lang="csharp">[TypeSelector]</code> on a <code lang="csharp">string</code>.

## TypeSelector

The attribute configures field selection and adds a picker to a plain string.

| Field | Selection result |
|---|---|
| <code lang="csharp">string</code> | Stores the assembly-qualified name |
| <code lang="class-name">SerializableType</code> / <code lang="class-name">SerializableMonoScript</code> | Configures the wrapper's selection |
| <code lang="csharp">[SerializeReference]</code> | Creates an instance of the selected implementation — see [SerializeReference Selector](03-serialize-reference-selector.md) |

### Constraints and collections

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }
```

```csharp
[TypeSelector(typeof(Weapon), Allow = TypeAllow.None)]
[SerializeField] private string _backupWeaponName;

[TypeSelector(typeof(ITwoHanded), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<MeleeWeapon> _heavyWeapon;

[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Weapon>[] _loadout;
```

<code lang="csharp">_heavyWeapon</code> offers only <code lang="class-name">Axe</code>: <code lang="class-name">Sword</code> does not implement <code lang="class-name">ITwoHanded</code>, and <code lang="class-name">Bow</code> does not inherit <code lang="class-name">MeleeWeapon</code>. All constraints apply together (**AND**) on every kind of field. Arrays and lists get a picker for each entry.

On <code lang="csharp">[SerializeReference]</code>, the field type is the first constraint, so this field offers only <code lang="class-name">Axe</code> as well:

```csharp
[TypeSelector(typeof(ITwoHanded))]
[SerializeReference] private MeleeWeapon _heldWeapon;
```

To allow a fixed set of classes, give them a common interface or base class and pass it: listing the classes themselves (<code lang="csharp">typeof(Sword), typeof(Axe)</code>) leaves the picker empty, and analyzer `AFT0009` reports it. See [instance selector configuration](03-serialize-reference-selector.md#which-classes-are-offered).

<details>
<summary>TypeSelector argument forms</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(Weapon))]
[TypeSelector(typeof(MeleeWeapon), typeof(ITwoHanded))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_weaponClass))]
```

- A field takes one <code lang="csharp">[TypeSelector]</code>.
- Arguments are <code lang="class-name">Type</code> or <code lang="csharp">string</code>: one value, several comma-separated values, or an array; one attribute cannot mix them.
- Without arguments, the attribute adds no constraints.
- A string is first resolved as a member of the class that declares the field, then as a type name.

</details>

### Properties

| Property | Default | Behaviour |
|---|---|---|
| <code lang="csharp">Allow</code> | <code lang="csharp">TypeAllow.All</code> | <code lang="csharp">Abstract</code> adds abstract classes; <code lang="csharp">Interface</code> adds interfaces. <code lang="csharp">All</code> enables both categories; <code lang="csharp">None</code> excludes them. Ignored on <code lang="csharp">[SerializeReference]</code> |
| <code lang="csharp">Required</code> | <code lang="csharp">false</code> | Warns about an empty type name or a <code lang="csharp">null</code> managed reference |

In the Inspector of a runtime object, the picker leaves out types from editor-only assemblies (`UnityEditor`, Editor-only asmdefs and `Editor` folders): a player build cannot resolve them.

- Fields of editor-only objects, such as editor windows and settings, offer every type.
- The rule follows the object's class, so a runtime object's field declared under <code lang="csharp">#if UNITY_EDITOR</code> leaves them out too.

### Required field

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![An empty required field shows a warning below the picker](Images/type-selector-required.png)

An empty required field shows a warning below the picker

With <code lang="csharp">Required = true</code>, `<None>` remains selectable. For strings and wrappers, the check tests for an empty stored name; a missing type with a nonempty name passes this check.

For project-wide and CI validation, see [required-field checks](04-serialize-reference-tooling.md#where-required-fields-are-checked).

### Constraint from another field

Pass <code lang="csharp">nameof(...)</code> to let a field or property's current value control the candidate list:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Choose <code lang="class-name">MeleeWeapon</code> in **Weapon Class**, and **Weapon Name** offers <code lang="class-name">Sword</code> and <code lang="class-name">Axe</code>. Changing a constraint does not clear an earlier selection.

| Constraint source | Support |
|---|---|
| <code lang="csharp">System.Type</code> | One type |
| <code lang="csharp">string</code> | A type name resolved through <code lang="csharp">Type.GetType</code> |
| <code lang="class-name">SerializableType</code>, <code lang="class-name">SerializableMonoScript</code>, and their generic variants | The resolved <code lang="csharp">.Type</code> value |
| An array of these values | Multiple simultaneous constraints; <code lang="csharp">List&lt;T&gt;</code> is not supported |

- The source must be an instance field or readable property of the class that declares the attributed field, inherited members included; indexers are not supported.
- For a field inside a <code lang="csharp">[Serializable]</code> class or a list element, the source is read from that same instance.
- An empty or unresolved source contributes no constraint.
- A generic wrapper's own <code lang="class-name">T</code> continues to constrain selection.

Analyzers catch mistakes in string arguments:

- `AFT0006` — a one-word string that names no member of the class;
- `AFT0007` — the member cannot supply base types;
- `AFT0008` — the string is not a valid type name.

If a well-formed type name refers to a type that is not loaded, the Inspector shows a warning.

![The constraint did not resolve, so the Inspector shows a warning below the field](Images/type-selector-constraint-warning.png)

The constraint did not resolve, so the Inspector shows a warning below the field

## TypeSelectorDisplay

<code lang="csharp">[TypeSelectorDisplay]</code> on a class changes only its row in the picker:

| Parameter on <code lang="class-name">DamageModifier</code> | In the picker |
|---|---|
| <code lang="csharp">Name = "Damage ×"</code> | Damage × in the list and the closed field; search still finds DamageModifier |
| <code lang="csharp">Group = "Combat/Modifiers"</code> | Combat → Modifiers → Damage × instead of the namespace |
| <code lang="csharp">Tooltip = "Scales incoming damage"</code> | Tooltip on hover |
| <code lang="csharp">Icon = "d_ScriptableObject Icon"</code> | Icon by an `EditorGUIUtility.IconContent` name, by an asset path starting with `Assets/` or `Packages/` with extension, or by a `Resources` path without extension |
| <code lang="csharp">Hidden = true</code> | Not listed; code assignment and an already stored value still work |

Subclasses do not inherit these settings.

![The Damage × name, icon, and Combat/Modifiers group in the picker](Images/type-selector-display.png)

The Damage × name, icon, and Combat/Modifiers group in the picker

> [!NOTE]
> <code lang="csharp">[TypeSelector]</code> and <code lang="csharp">[TypeSelectorDisplay]</code> are marked <code lang="csharp">[Conditional("UNITY_EDITOR")]</code>. Classes compiled into an external DLL without that symbol carry none of their settings, including <code lang="csharp">Hidden</code>.

## TypeSelectorWindow

The picker groups types by namespace or <code lang="csharp">Group</code> and distinguishes identical names by assembly. Use <code lang="class-name">TypeSelectorWindow</code> to open it from a custom inspector or editor window.

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

Picking an open generic type opens its argument pages and returns a constructed closed type:

```csharp
public abstract class CombatModifier { }
public abstract class StatusEffect { }
public sealed class Burning : StatusEffect { }

public sealed class Amplify<T> : CombatModifier
    where T : StatusEffect { }

[SerializeField] private SerializableType<CombatModifier> _modifier;
```

Choose <code lang="csharp">Amplify&lt;T&gt;</code> in <code lang="csharp">_modifier</code>: the window offers the subclasses of <code lang="class-name">StatusEffect</code>, and choosing <code lang="class-name">Burning</code> stores <code lang="csharp">Amplify&lt;Burning&gt;</code>.

![Choosing a generic type argument in the picker](Images/type-selector-generic.gif)

Choosing a generic type argument in the picker

- A generic argument can itself be generic: the window asks for its arguments first.
- When every argument can be inferred from the field type, the closed type is returned immediately.
- Arguments must satisfy the generic parameter's constraints; interfaces, abstract classes, and hidden types are not offered as arguments.
- For <code lang="csharp">[SerializeReference]</code>, arguments are inferred from the field type — see [SerializeReference Selector](03-serialize-reference-selector.md#which-classes-are-offered).

### Opening from code

<code lang="csharp">screenRect</code> is the button rectangle in **screen coordinates**, and <code lang="csharp">selectedTypeName</code> is the current type-name string:

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

The callback receives an assembly-qualified name, or <code lang="csharp">null</code> for `<None>`; dismissing the window without a choice does not invoke it.

<code lang="csharp">currentAqn</code> controls the current mark:

- an empty string (the default) marks `<None>`;
- <code lang="csharp">null</code> leaves selection unmarked;
- a name missing from the list stays unmarked too, so Enter right after opening cannot erase a missing type's stored name.

### Window filters

<code lang="class-name">TypeSelectorFilter</code> is a struct. In its <code lang="csharp">default</code>:

- an empty <code lang="csharp">Types</code> admits any type;
- <code lang="csharp">Allow</code> is <code lang="csharp">None</code>, not <code lang="csharp">All</code> as on <code lang="csharp">[TypeSelector]</code>: set <code lang="csharp">Allow</code> explicitly when you need abstract classes or interfaces.

<details>
<summary>Window filter properties</summary>

| Property | Purpose |
|---|---|
| <code lang="csharp">Types</code> | Base types every candidate must satisfy |
| <code lang="csharp">Allow</code> | Allowed categories: abstract classes and interfaces |
| <code lang="csharp">Predicate</code> | An additional condition after type and category checks |
| <code lang="csharp">AdditionalTypes</code> | Candidates that bypass <code lang="csharp">Types</code>, <code lang="csharp">Allow</code>, and <code lang="csharp">Predicate</code>; <code lang="csharp">Hidden</code> filtering remains |
| <code lang="csharp">ArgumentFilter</code> | An additional filter for manually selected arguments |
| <code lang="csharp">InferredArgumentFilter</code> | A filter for arguments inferred from the field type |
| <code lang="csharp">IncludeHidden</code> | Offer types marked <code lang="csharp">Hidden = true</code>, as generic arguments too |
| <code lang="csharp">HideNoneOption</code> | Hide `<None>` on the root page |

</details>

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md); for a picker opened from editor code, see [EditorTools](../Samples~/EditorTools/Documentation/README.md).

![A wave of regular and elite enemies moves toward the center.](../Samples~/Types/Documentation/Images/demo.gif)
