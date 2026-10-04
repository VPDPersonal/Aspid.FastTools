# Serializable Types

A type as an ordinary field: Unity saves it, and the Inspector lets you pick it from a list.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

<code lang="csharp">[TypeSelector]</code> sets the [selection settings](03-type-selector.md): <code lang="csharp">Allow = TypeAllow.None</code> leaves only concrete types in the list.

![Selecting a serializable type in the Inspector](Images/serializable-type-quick-start.gif)

## Which wrapper to use

| Task | Field |
|---|---|
| Store a type, including DLL, nested or generic types | <code lang="class-name">SerializableType</code>, <code lang="class-name">SerializableType&lt;T&gt;</code> |
| Keep the selection when your script is renamed | <code lang="class-name">SerializableMonoScript</code>, <code lang="class-name">SerializableMonoScript&lt;T&gt;</code> |

The <code lang="class-name">T</code> variant offers only types compatible with <code lang="class-name">T</code> in the picker.

## SerializableType

<code lang="class-name">SerializableType</code> stores an assembly-qualified name: the type name together with its assembly. Set a default in code:

```csharp
[SerializeField]
private SerializableType<Weapon> _primaryWeapon = new(typeof(Sword));
```

### Missing type

After a class, namespace or assembly rename, the stored name no longer resolves: the field shows `<Missing …>` with a **Missing type** notice under it. The caption gives the type name without its assembly; the tooltip gives the whole stored name.

- <code lang="csharp">.Type</code> returns <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> and <code lang="csharp">ToString()</code> keep the old name.

![The missing Game.Combat.Spear type with the Fix and → Spear buttons](Images/serializable-type-missing.png)

**Fix** opens the type picker, and the type you pick replaces the stored name. When the class only moved to another namespace or assembly and exactly one compatible type has its name, the notice also offers it, for example **→ Spear**.

[Project References](06-serialize-reference-tooling.md#type-names) finds every missing name in the project and repairs them in groups. The [build check](07-serialize-reference-validation.md) and [breakage detection](07-serialize-reference-validation.md#detecting-new-breakages) report new ones.

## SerializableMonoScript

<code lang="class-name">SerializableMonoScript</code> remembers the script asset itself, so the selection survives a script rename. Pick the type in the Inspector or drag a `.cs` file from **Project** onto the field.

| After renaming `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code>, a [missing type](#missing-type) | <code lang="class-name">Blade</code>, in Play Mode too |
| Stored name | <code lang="string">Sword</code> | <code lang="string">Blade</code> once the asset is saved again |

Limitations:

- only a top-level, non-generic class declared in a `.cs` file of the same name can be picked;
- there is no public constructor, so the field cannot be created in code.

The link breaks, and the field shows a [missing type](#missing-type) with the same notice, when:

- the class is renamed without its file;
- the file is renamed outside Unity without its `.meta`.

## Types in a player build

> [!WARNING]
> In a player, both wrappers resolve the stored type name. If a class is used only through this selection, **Managed Stripping Level** Low or higher may strip it: <code lang="csharp">.Type</code> returns <code lang="csharp">null</code> while the editor resolves it. Keep the class with <code lang="csharp">[Preserve]</code> (<code lang="csharp">UnityEngine.Scripting</code>) or `link.xml`.

## Package sample

For Inspector selection of enemy types and spawn patterns, see [Types](../Samples~/Types/Documentation/README.md).
