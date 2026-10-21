# C# code style

`.editorconfig` holds everything a formatter or analyzer can check: indentation, braces, `var`, naming, namespace
style, attribute brackets, expression bodies and ternary wrapping. Rider and `dotnet format` apply it. This file holds
the rest. It covers the package, the samples, and the generator and analyzer solutions.

## File

- `using` directives go above the namespace, sorted by ascending line length, not alphabetically. `System` is not
  forced first.
- `// ReSharper disable once CheckNamespace` sits on the line right above the namespace: namespaces follow the
  feature, not the folder.
- Block-scoped namespace on the Unity side, file-scoped in `Aspid.FastTools.Generators` and `Aspid.FastTools.Analyzers`.
- One top-level type per file, named after the file.

```csharp
using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    ...
}
```

## Member order

1. Constants.
2. `static readonly` fields.
3. Serialized fields.
4. Other instance fields.
5. Properties.
6. Constructors.
7. Unity messages, in lifecycle order: `Awake`, `OnEnable`, `Start`, `Update`, `LateUpdate`, `OnPreCull`, `OnDisable`,
   `OnDestroy`, `OnValidate`.
8. Public methods.
9. Private methods.
10. Static methods, the private helpers last.

Inside a group, a shorter declaration usually comes first, as with `using` directives. Meaning wins when the two
disagree: keep fields that belong together next to each other.

## Types and members

- Leaf classes are `sealed` unless designed for inheritance. `internal` for implementation, `public` for API.
- Interface members carry an explicit `public`: `public Vector3 GetPosition(int index);`.
- A method that touches no instance state is `static`.
- `#region` only groups a family of overloads in a large extension class. Ordinary classes have none.

## Attributes

- One attribute per bracket pair. `.editorconfig` enforces this in Rider.
- Type and method attributes each sit on their own line.
- On a field, `[SerializeField]` and short inspector attributes (`[Min]`, `[Range]`, `[TextArea]`) stay on the field
  line. An attribute that configures a drawer goes on the line above.

```csharp
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public sealed class SampleFrame : MonoBehaviour
{
    [SerializeField] [Min(0.1f)] private float _speed = 3f;

    [TypeSelector(Allow = TypeAllow.None)]
    [SerializeField] private SerializableType<ISpawnPattern> _pattern;
}
```

## Expressions

- An expression-bodied method puts its body on the next line. A property keeps `=>` and its expression on its own line.
- Every ternary puts `?` and `:` on their own lines, also inside an argument.
- Name the arguments of a call that takes more than one, above all literals and flags:
  `Mathf.Max(a: 1f, b: aspect)`, `Destroy(obj: _trail.gameObject, t: _lifetime)`. The first argument may stay
  positional when the method name says what it is: `Type.GetType(_aqn, throwOnError: false)`. A lone literal or flag
  still gets its name. Lambdas and LINQ selectors stay positional.
- An object initializer passed as an argument spans several lines.
- A local does not reuse the name of an inherited member: `detailsTitle`, not `title` in an `EditorWindow`.
- A `bool` reads as a question: `isDefault`, `isElite`.

```csharp
private void OnDisable() =>
    FinishTrail();

public float Slow => Time.time < _freezeUntil
    ? _slow
    : 0f;

var eliteType = string.IsNullOrEmpty(_eliteType)
    ? null
    : Type.GetType(_eliteType, throwOnError: false);
```

## Blank lines

A blank line separates the steps of a method: the declarations from the code that uses them, a statement from the
`if` that checks its result, and the closing `return` from the work before it. A one-line guard at the top of a method
needs none.

```csharp
var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Ability.asset");

AssetDatabase.CreateAsset(asset: ability, path: path);
_search.SetValueWithoutNotify(string.Empty);

Reload(ability);
```

## Comments

- As a rule, code carries no comments: names and structure explain it.
- A comment is for a place that is hard to read without one: a non-obvious algorithm or workaround, `unsafe` code,
  Burst code. It says why, in English, and does not retell the code.
- Config files (`.editorconfig`, YAML, JSON) get short section headers at most.
- The public API of the package carries XML docs: `<summary>`, `<param>`, `<returns>`, `<see cref/>`. Internal code
  and samples need none.

## Samples

- A sample shows one feature. Its scripts keep only what the feature needs.
- A sample hides nothing it serializes: no `[HideInInspector]` and no `[AddComponentMenu("")]`. A value the user
  should not tune is a `const`.
