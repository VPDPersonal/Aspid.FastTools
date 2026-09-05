# ID System

> [!WARNING]
> **Beta:** the ID System is currently in beta. The public API, generated code layout and editor workflow may change in future releases.

Maps asset-assignable names to stable integer IDs stored in an `IdRegistry` ScriptableObject, with full `int ↔ string` lookups at runtime. Use the resulting `int` in `switch` statements and `Dictionary` keys without paying for string comparisons.

## Setup

**1.** Declare a `partial struct` implementing `IId`. The source generator adds the required fields and property automatically:

```csharp
using Aspid.FastTools.Ids;

public partial struct EnemyId : IId { }
```

Generated code:

```csharp
public partial struct EnemyId
{
    [SerializeField] private string __stringId; // editor-only field, stripped from player builds
    [SerializeField] private int _id;

    public int Id => _id;
}
```

**2.** Create the registry asset and bind it to the struct type in its Inspector:
- `Assets → Create → Aspid → Id Registry`

**3.** Use the struct as a serialized field. The Inspector shows a dropdown of registered names; the selector window also lets you create new entries on the fly:

```csharp
using UnityEngine;
using Aspid.FastTools.Ids;

[CreateAssetMenu]
public class EnemyDefinition : ScriptableObject
{
    [UniqueId] [SerializeField] private EnemyId _id;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyId _targetEnemy;

    private void Spawn()
    {
        int id = _targetEnemy.Id; // stable integer, safe for switch / Dictionary
    }
}
```

![Id selector dropdown in the Inspector](Images/aspid_fasttools_id_selector.gif)

## Generator diagnostics

The generator reports `AFID001` if the struct is missing `partial`, and `AFID002` if your code already declares `_id`, `Id`, or `__stringId` (the generator skips emission so you get a clear error pointing at the struct rather than a CS compile error inside generated source). Generic targets (`EnemyId<T>`) and generic containing types are supported.

## UniqueIdAttribute

Marks a field as requiring a unique value across all assets of the declaring type. The Inspector shows a warning if two assets share the same ID.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class UniqueIdAttribute : PropertyAttribute { }
```

![Duplicate-ID warning in the Inspector](Images/aspid_fasttools_id_collision.gif)

## IdRegistry

`ScriptableObject` in `Aspid.FastTools.Ids` that stores `(int, string)` entries and keeps the lookup tables available at runtime. Each name is assigned a stable, auto-incrementing ID that never changes when other entries are added or removed.

![IdRegistry inspector](Images/aspid_fasttools_id_registry.png)

| Member | Description |
|--------|-------------|
| `bool TryGetId(string name, out int id)` | Returns `true` and the ID when found; otherwise `false` |
| `bool TryGetName(int id, out string name)` | Returns `true` and the name when found; otherwise `false` and `string.Empty` |
| `bool Contains(int id)` | Whether an ID is registered |
| `bool Contains(string name)` | Whether a name is registered |
| `int Count` | Number of entries |
| `IReadOnlyList<int> Ids` · `IReadOnlyList<string> IdNames` | Registered IDs / names, in registration order |
| `IEnumerator<KeyValuePair<int, string>> GetEnumerator()` | Iterate `(id, name)` pairs |

The registry derives from `ScriptableObject` directly and exposes a generic counterpart `IdRegistry<T>` (with `T : struct, IId`) that adds typed `Contains(T)` and `TryGetName(T, out string)` overloads. Edits — adding, renaming, removing entries — happen through the registry inspector, not via a public runtime API.
