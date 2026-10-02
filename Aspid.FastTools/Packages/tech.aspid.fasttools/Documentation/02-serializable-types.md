# Serializable Types

A class type as an ordinary field: Unity saves it, and the Inspector picks it from a list.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Selecting a serializable type in the Inspector](Images/serializable-type-quick-start.gif)

## Which wrapper to use

| Task | Field |
|---|---|
| Store a type, including DLL, nested or generic types | <code lang="class-name">SerializableType</code> |
| Keep the selection when your script is renamed | <code lang="class-name">SerializableMonoScript</code> |

Both wrappers have a <code lang="class-name">T</code> variant that constrains the selection to compatible types. The <code lang="csharp">[TypeSelector]</code> attribute adds [selection settings](03-type-selector.md); the example uses <code lang="csharp">Allow = TypeAllow.None</code> to offer only concrete classes.

To store an instance of the selected class, use [SerializeReference Selector](04-serialize-reference-selector.md). To change an existing component's class, use [ComponentTypeSelector](05-component-type-selector.md).

## SerializableType

<code lang="class-name">SerializableType</code> stores an assembly-qualified name: the type name together with its assembly.

In code, the constructor creates a wrapper; a type incompatible with <code lang="class-name">T</code> throws <code lang="class-name">ArgumentException</code>:

```csharp
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

For a resolved type, <code lang="csharp">ToString()</code> returns <code lang="csharp">Type.Name</code>, so a generic type reads <code lang="string">Enchanted`1</code>, not the picker's caption.

### Missing type

A stored name that no longer resolves after a class, namespace, or assembly rename. The Inspector shows it as `<Missing …>`.

![The missing Game.Combat.Spear type in an Inspector field](Images/serializable-type-missing.png)

- <code lang="csharp">Type</code> returns <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> and <code lang="csharp">ToString()</code> keep the old name. Pick an available type again or restore the class under its old name.

## SerializableMonoScript

The same field, but the selection survives a class rename: the field remembers the script asset itself. Pick the type in the Inspector or drag a `.cs` file from **Project** onto the field.

| After renaming `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code>, a [missing type](#missing-type) | <code lang="class-name">Blade</code>, in Play Mode too |
| Stored name | <code lang="class-name">Sword</code> | <code lang="class-name">Blade</code> once the asset is saved again |

Limitations:

- only classes with their own `.cs` are listed: top-level, non-generic, named after the file;
- generic types, nested classes and types from DLLs cannot be picked;
- there is no public constructor, so the field cannot be created in code;
- renaming the class without its file, or the file outside Unity without its `.meta`, breaks the link and the field shows a missing type.

## Types in a player build

> [!WARNING]
> In a player, both wrappers resolve the stored type name. If a class is used only through this selection, from **Managed Stripping Level** Low up stripping may remove it and <code lang="csharp">.Type</code> returns <code lang="csharp">null</code> while the editor resolves it. Keep the class with <code lang="csharp">[Preserve]</code> (<code lang="csharp">UnityEngine.Scripting</code>) or `link.xml`. This also applies to strings with <code lang="csharp">[TypeSelector]</code>.

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md).
