# Editor Helpers

Two methods for readable component and ScriptableObject display names in custom inspectors, lists and editor windows. They keep labels consistent and distinguish components of the same type on one GameObject.

Both methods are extensions: `GetDisplayName()` is called on any `UnityEngine.Object`, `GetDisplayNameWithIndex()` on a `Component`.

```csharp
using Aspid.FastTools.Editors;

[SerializeField] private AudioSource _audioSource;

var title = _audioSource.GetDisplayName();
// "Audio Source"

var indexedTitle = _audioSource.GetDisplayNameWithIndex();
// "Audio Source (2)" when it is the second AudioSource on the same GameObject
```

> [!NOTE]
> These methods are editor-only. Place calling code in an `Editor` folder or an assembly restricted to the Editor platform.

## Object display name

`GetDisplayName()` extends `UnityEngine.Object`. When the type has an `[AddComponentMenu]` attribute, including an inherited one, it uses `ObjectNames.GetInspectorTitle`. Otherwise, it formats the type name with `ObjectNames.NicifyVariableName`.

The method describes the type, not the instance: the GameObject or asset name does not affect the result.

## Component index

`GetDisplayNameWithIndex()` extends `Component` and counts components of the **exact same type** on the same GameObject. The suffix follows component order, starting at one.

| Components on the GameObject | Labels |
|---|---|
| `AudioSource` | `Audio Source` |
| `AudioSource`, `AudioSource` | `Audio Source (1)`, `Audio Source (2)` |
| `AudioSource`, `BoxCollider` | `Audio Source`, `Box Collider` |

Both methods return `string.Empty` for null or destroyed objects.

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), `GetDisplayName()` supplies the selected ability's pane title. Combine it with [`AddOpenScriptCommand`](07-visual-element-extensions.md#opening-scripts-and-finding-the-owner-window) to open the source file in your IDE on a double-click.
