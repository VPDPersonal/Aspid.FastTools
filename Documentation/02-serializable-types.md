# Serializable Types

A type as an ordinary field: Unity saves it, and you pick it from a list in the Inspector.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Selecting a serializable type in the Inspector](Images/serializable-type-quick-start.gif)

## Which wrapper to use

| Task | Field |
|---|---|
| Store a type, including DLL, nested or generic types | <code lang="class-name">SerializableType</code> |
| Keep the selection when your script is renamed | <code lang="class-name">SerializableMonoScript</code> |

Both wrappers have a <code lang="class-name">T</code> variant that constrains the selection to compatible types. The <code lang="csharp">[TypeSelector]</code> attribute adds [selection settings](03-type-selector.md); the [quick start](#quick-start) uses <code lang="csharp">Allow = TypeAllow.None</code> to offer only concrete types.

## SerializableType

<code lang="class-name">SerializableType</code> stores an assembly-qualified name: the type name together with its assembly.

```csharp
[SerializeField]
private SerializableType<Weapon> _primaryWeapon = new(typeof(Sword));
```

### Missing type

After a class, namespace or assembly rename, the stored name no longer resolves, and the Inspector shows `<Missing …>`.

![The missing Game.Combat.Spear type in an Inspector field](Images/serializable-type-missing.png)

- <code lang="csharp">.Type</code> returns <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> and <code lang="csharp">ToString()</code> keep the old name.

To fix the field, pick a type again or restore the class under its old name.

## SerializableMonoScript

<code lang="class-name">SerializableMonoScript</code> remembers the script asset itself, so the selection survives a class rename. Pick the type in the Inspector or drag a `.cs` file from **Project** onto the field.

| After renaming `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code>, a [missing type](#missing-type) | <code lang="class-name">Blade</code>, in Play Mode too |
| Stored name | <code lang="string">Sword</code> | <code lang="string">Blade</code> once the asset is saved again |

Limitations:

- only a top-level, non-generic class declared in a `.cs` file of the same name can be picked; from a DLL, only a <code lang="class-name">MonoBehaviour</code> or <code lang="class-name">ScriptableObject</code>;
- there is no public constructor, so the field cannot be created in code.

The link breaks, and the field shows a [missing type](#missing-type), when:

- the class is renamed without its file;
- the file is renamed outside Unity without its `.meta`;
- a class from a DLL is renamed or moved to another namespace.

## Types in a player build

> [!WARNING]
> In a player, both wrappers resolve the stored type name. If a class is used only through this selection, **Managed Stripping Level** Low or higher may strip it: <code lang="csharp">.Type</code> returns <code lang="csharp">null</code> while the editor resolves it. Keep the class with <code lang="csharp">[Preserve]</code> (<code lang="csharp">UnityEngine.Scripting</code>) or `link.xml`.

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md).

![A wave of regular and elite enemies in the Types scene](../Samples~/Types/Documentation/Images/demo.gif)
