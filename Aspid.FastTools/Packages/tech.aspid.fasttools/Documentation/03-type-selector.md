# TypeSelector

Control which types are offered and how they appear in the picker.

## Quick start

```csharp
[TypeSelector(typeof(ITwoHanded), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Weapon> _weapon;
```

The field offers only concrete two-handed weapons: classes derived from <code lang="class-name">Weapon</code> that implement <code lang="class-name">ITwoHanded</code>.

## Supported fields

| Field | Selection result |
|---|---|
| <code lang="csharp">string</code> | Stores the assembly-qualified name |
| [Serializable Types](02-serializable-types.md) | Configures the wrapper's selection |
| <code lang="csharp">[SerializeReference]</code> | Creates an instance of the selected implementation — see [SerializeReference Selector](04-serialize-reference-selector.md) |

## Which types are offered

The attribute's constraints apply **together**. A wrapper also constrains candidates by <code lang="class-name">T</code>; a <code lang="csharp">[SerializeReference]</code> field constrains them by its field type.

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }
```

| Constraint | Result |
|---|---|
| <code lang="csharp">typeof(Weapon)</code> | Types assignable to <code lang="class-name">Weapon</code> |
| <code lang="csharp">typeof(Weapon), typeof(ITwoHanded)</code> | Weapons that implement <code lang="class-name">ITwoHanded</code> |
| <code lang="csharp">typeof(Sword), typeof(Axe)</code> | Empty list: a class cannot inherit both; analyzer `AFT0009` reports it |

To allow several classes, use their common base class or interface. On an array or list, the constraint applies to each element.

## Properties

| Property | Default | Behaviour |
|---|---|---|
| <code lang="csharp">Allow</code> | <code lang="csharp">TypeAllow.All</code> | Lets abstract classes (<code lang="csharp">Abstract</code>), interfaces (<code lang="csharp">Interface</code>), both or neither into the list. Ignored on <code lang="csharp">[SerializeReference]</code> |
| <code lang="csharp">Required</code> | <code lang="csharp">false</code> | Warns about an empty type name or a <code lang="csharp">null</code> managed reference |

> [!NOTE]
> In the Inspector of a runtime object, the picker leaves out types from editor-only assemblies (`UnityEditor`, Editor-only asmdefs and `Editor` folders): a player build cannot resolve them. The rule follows the object's class, so a runtime object's field declared under <code lang="csharp">#if UNITY_EDITOR</code> leaves them out too.

## Required field

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![An empty required field shows a warning below the picker](Images/type-selector-required.png)

With <code lang="csharp">Required = true</code>, `<None>` remains selectable. For strings and wrappers, the check tests for an empty stored name; a missing type with a nonempty name passes this check.

For project-wide and CI validation, see [required-field checks](07-serialize-reference-validation.md#what-each-run-checks).

## Constraint from another field

Pass <code lang="csharp">nameof(...)</code> to let a field or property's current value control the candidate list:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Choose <code lang="class-name">MeleeWeapon</code> in **Weapon Class**, and **Weapon Name** offers its concrete subclasses. Changing a constraint does not clear an earlier selection.

![Choosing MeleeWeapon in Weapon Class leaves only Axe and Sword in Weapon Name](Images/type-selector-member-constraint.gif)

| Constraint source | Constrains to |
|---|---|
| <code lang="class-name">System.Type</code> | One type |
| <code lang="csharp">string</code> | A type name resolved through <code lang="csharp">Type.GetType()</code> |
| <code lang="class-name">SerializableType</code> / <code lang="class-name">SerializableMonoScript</code> | The resolved <code lang="csharp">.Type</code> value |
| An array of these values | Multiple simultaneous constraints; <code lang="class-name">List&lt;T&gt;</code> is not supported |

- A string is first looked up among the instance fields and readable properties of the class that declares the field, inherited ones included, then as a type name.
- For a field inside a <code lang="csharp">[Serializable]</code> class or a list element, the source is read from that same instance.
- While the source is empty or unresolved it adds no constraint: the string **Weapon Name** then offers every concrete class in the project. A wrapper keeps its own <code lang="class-name">T</code>.

## Errors in string arguments

```csharp
[TypeSelector("Spear, Assembly-CSharp")]
[SerializeField] private string _weaponName;
```

Analyzers catch mistakes in the strings:

- `AFT0006` — a one-word string that names no member of the class;
- `AFT0007` — the member cannot supply base types;
- `AFT0008` — the string is not a valid type name.

If a well-formed type name refers to a type that is not loaded, like <code lang="class-name">Spear</code> above, the Inspector shows a warning:

![The constraint did not resolve, so the Inspector shows a warning below the field](Images/type-selector-constraint-warning.png)

## TypeSelectorDisplay

<code lang="csharp">[TypeSelectorDisplay]</code> on a class changes only its row in the picker:

| Parameter on <code lang="class-name">Sword</code> | In the picker |
|---|---|
| <code lang="csharp">Name = "Longsword"</code> | Longsword in the list and the closed field; search still finds Sword |
| <code lang="csharp">Group = "Weapons/Melee"</code> | Weapons → Melee → Longsword instead of the namespace |
| <code lang="csharp">Tooltip = "A balanced blade"</code> | Tooltip on hover |
| <code lang="csharp">Icon = "d_ScriptableObject Icon"</code> | Icon: a built-in one by name, an asset by path with extension, or one from `Resources` without extension |
| <code lang="csharp">Hidden = true</code> | Not listed; code assignment and an already stored value still work |

Subclasses do not inherit these settings.

![The Longsword name, icon, and Weapons/Melee group in the picker](Images/type-selector-display.png)

> [!NOTE]
> <code lang="csharp">[TypeSelector]</code> and <code lang="csharp">[TypeSelectorDisplay]</code> are marked <code lang="csharp">[Conditional("UNITY_EDITOR")]</code>. Classes compiled into an external DLL without that symbol carry none of their settings, including <code lang="csharp">Hidden</code>.

## The picker

The picker groups types by namespace or <code lang="csharp">Group</code> and distinguishes identical names by assembly.

![Favorites and Recent on the picker root page](Images/type-selector-window.png)

The picker's root page keeps the types you need most often:

- **Favorites**: your picks. To add a type, click the star at the right of its row, or press Space while the row is selected.
- **Recent**: the types chosen last.

Whether Favorites is shown and how long Recent is (0 hides it) are set in the FastTools window's **Settings** tab. The gear at the bottom right of the picker opens it, and both lists are cleared there too.

### Generic types

Picking an open generic type opens its argument pages and returns a constructed closed type:

```csharp
public abstract class Enchantment { }
public sealed class Fire : Enchantment { }
public sealed class Frost : Enchantment { }

public sealed class Enchanted<T> : Weapon
    where T : Enchantment { }
```

Choose <code lang="class-name">Enchanted&lt;T&gt;</code> in the <code lang="class-name">SerializableType&lt;Weapon&gt;</code> field: the window offers the subclasses of <code lang="class-name">Enchantment</code>, and choosing <code lang="class-name">Fire</code> stores <code lang="class-name">Enchanted&lt;Fire&gt;</code>.

![Choosing a generic type argument in the picker](Images/type-selector-generic.gif)

- A generic argument can itself be generic: the window asks for its arguments first.
- When every argument can be inferred from the field type, the closed type is returned immediately.
- Interfaces, abstract classes, and hidden types are not offered as arguments.

## TypeSelectorWindow

<code lang="class-name">TypeSelectorWindow</code> opens the same picker from a custom inspector or editor window, for example from a UI Toolkit button:

```csharp
using Aspid.FastTools.Types.Editors;

var button = new Button { text = "Select weapon" };
button.clicked += () => TypeSelectorWindow.Show(
    screenRect: GUIUtility.GUIToScreenRect(button.worldBound),
    filter: new TypeSelectorFilter
    {
        Types = new[] { typeof(Weapon) }
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

The callback receives an assembly-qualified name, or <code lang="csharp">null</code> for `<None>`; dismissing the window without a choice does not invoke it.

- <code lang="csharp">currentAqn</code> marks its type on opening and <code lang="csharp">""</code> marks `<None>`; <code lang="csharp">null</code> or a name missing from the list marks nothing, so Enter right after opening cannot erase the stored name.
- <code lang="csharp">TypeSelectorFilter.Allow</code> defaults to <code lang="csharp">TypeAllow.None</code>, unlike <code lang="csharp">[TypeSelector]</code>: the window above offers only concrete weapons.

For filter properties and window parameters, see the API reference: [TypeSelectorFilter](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.Types.Editors.TypeSelectorFilter), [TypeSelectorWindow](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.Types.Editors.TypeSelectorWindow).

## Package sample

For a picker opened from editor code, see [EditorTools](../Samples~/EditorTools/Documentation/README.md).
