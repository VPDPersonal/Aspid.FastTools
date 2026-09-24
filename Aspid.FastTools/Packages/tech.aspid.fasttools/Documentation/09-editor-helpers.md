# Editor Helpers

For an object label in a custom editor window, Unity offers `ObjectNames.GetInspectorTitle`, but it appends “(Script)” to scripts without `[AddComponentMenu]` and gives two identical components on one GameObject the same title. `GetDisplayName()` returns the readable type name (`AbilityConfig` → “Ability Config”), and `GetDisplayNameWithIndex()` numbers the duplicates: “Ability Config (1)”, “Ability Config (2)”.

## Quick start

The examples on this page work with the `AbilityConfig` component:

```csharp
public sealed class AbilityConfig : MonoBehaviour { }
```

```csharp
using Aspid.FastTools.Editors;

var title = new Label(config.GetDisplayNameWithIndex());
```

> [!NOTE]
> The methods are editor-only: call them from an `Editor` folder or from an Editor-only Assembly Definition that references `Aspid.FastTools.Editor`.

## GetDisplayName()

Extends `UnityEngine.Object`. When the type has `[AddComponentMenu]`, it returns the Inspector title — the last segment of the menu path; otherwise, the type name split into words. A null or destroyed object returns `string.Empty`.

| `[AddComponentMenu]` on `AbilityConfig` | `GetInspectorTitle()` | `GetDisplayName()` |
|---|---|---|
| None | `Ability Config (Script)` | `Ability Config` |
| `"Gameplay/Ability"` | `Ability` | `Ability` |

## GetDisplayNameWithIndex()

Extends `Component`. It counts components of **exactly the same type** on the GameObject and adds the component's position among them, starting at one. The number is computed on every call, so after components are removed or reordered, call the method again. A null or destroyed component returns `string.Empty`.

| Components on the GameObject | Labels |
|---|---|
| `AbilityConfig` | `Ability Config` |
| `AbilityConfig`, `AbilityConfig` | `Ability Config (1)`, `Ability Config (2)` |

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), `GetDisplayName()` titles the custom inspector of the `AbilityConfig` asset.
