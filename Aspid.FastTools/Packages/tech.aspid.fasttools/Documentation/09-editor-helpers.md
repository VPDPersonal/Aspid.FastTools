# Editor Helpers

Editor Helpers is a collection of small utilities for everyday tasks in the Unity Editor. More tools will be added over time.

Currently, it provides two methods for readable component and ScriptableObject display names in custom inspectors, lists and editor windows. They keep labels consistent and distinguish components of the same type on one GameObject.

```csharp
using Aspid.FastTools.Editors;

var title = target.GetDisplayName();
// For AudioSource: "Audio Source"

var indexedTitle = component.GetDisplayNameWithIndex();
// For the second AudioSource on the same GameObject: "Audio Source (2)"
```

These methods are editor-only. Place calling code in an `Editor` folder or an assembly restricted to the Editor platform.

## Object display name

`GetDisplayName()` extends `UnityEngine.Object`. When the type has an `[AddComponentMenu]` attribute, including an inherited one, it uses `ObjectNames.GetInspectorTitle`. Otherwise, it formats the type name with `ObjectNames.NicifyVariableName`.

The result describes the type: renaming a GameObject or asset does not make it return `object.name`.

## Component index

`GetDisplayNameWithIndex()` extends `Component` and counts components of the **exact same type** on the same GameObject. The suffix follows component order, starting at one.

| Components on the GameObject | Labels |
|---|---|
| One `AudioSource` | `Audio Source` |
| Two `AudioSource` components | `Audio Source (1)`, `Audio Source (2)` |
| Components of different types | Each has its own name without an index |

Both methods return `string.Empty` for null or destroyed objects.

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), `GetDisplayName()` supplies the selected ability's pane title. Combine it with the [open-script command](07-visual-element-extensions.md#editor-extensions) to open the source file in your IDE on a double-click.
