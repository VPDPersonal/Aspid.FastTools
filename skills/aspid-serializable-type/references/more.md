# TypeSelectorWindow, ComponentTypeSelector and the CI gate

## TypeSelectorWindow (editor code)

```csharp
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

TypeSelectorWindow.Show(
    screenRect,                                   // button rect in SCREEN coordinates
    new TypeSelectorFilter { Types = new[] { typeof(Weapon) }, Allow = TypeAllow.None },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);   // null for <None>; not called when dismissed
```

- `TypeSelectorFilter` is a struct whose default `Allow` is `None` (the attribute's default is `All`). Other members:
  `Predicate`, `AdditionalTypes`, `ArgumentFilter`, `InferredArgumentFilter`, `IncludeHidden`, `HideNoneOption`.
- `currentAqn: ""` (default) marks `<None>` as current; `null` marks nothing.
- Picking an open generic type walks the user through its arguments and returns the closed type's name.

## ComponentTypeSelector

```csharp
using Aspid.FastTools.Types;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;   // marker struct, holds no data
    [SerializeField, Min(0)] private float _health = 100f;
}
```

The dropdown lists the declaring class (unless it is abstract) and its concrete subclasses, with no `<None>`, and
rewrites the object's script, keeping the fields both classes share. On a runtime component it leaves out classes
from editor-only assemblies. Each subclass must live in its own file named after the class, otherwise the
switch is skipped with a Console warning. The switch adds the new class's `[RequireComponent]` components and is
refused (with a warning) when it would duplicate a `[DisallowMultipleComponent]` class, drop a class another
component requires, or need a component that cannot be added (an abstract class such as `Collider`). It is also
refused for a component inherited from a prefab (an instance in a scene or a variant): Unity keeps no override for
a changed script, so change the class in the source prefab. With several components selected, one refusal cancels
the change for all of them.

## CI gate

Fails a CI job on missing types (`[SerializeReference]`, `SerializableType`, `SerializableMonoScript`) and, with
`-srGateRequired`, on empty `Required = true` fields. Use this exact call. Do not invent another `-executeMethod`.

```bash
Unity -batchmode -projectPath . \
  -executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt -srGateRequired -srGateFail
```

- `-batchmode` is required; without it the call is ignored.
- `-srGateFail` fails on violations with exit code `1`. `-srGateWarnOnly` only logs them, exit code `0`. Without
  either flag the **Build / CI gate** mode of the project settings applies, and `Off` skips the check.
- Exit code `2` means the check itself failed.
- `-srGateReport <path>` (default `SerializeReferenceGateReport.txt`; the folder must exist) gets one tab-separated
  line per violation: `KIND assetPath fileId rid className fieldPath origin`. The report starts with `#` comment
  lines (counts and files not scanned); skip empty lines and lines that start with `#`.
- Details: [Build and CI checks](https://vpdpersonal.github.io/Aspid.FastTools/docs/serialize-reference-validation).
