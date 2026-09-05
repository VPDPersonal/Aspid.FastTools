# Editor Helpers

Display-name helpers for Unity objects in custom editors:

| Method | Returns |
|---|---|
| `GetScriptName()` | The object's display name — `ObjectNames.GetInspectorTitle` when the type has `[AddComponentMenu]`, otherwise the nicified type name |
| `GetScriptNameWithIndex()` | The same name plus a count suffix when the GameObject holds several components of the same type — e.g. `"Audio Source (2)"` |

```csharp
using Aspid.FastTools.Editors;

[CustomEditor(typeof(MyBehaviour))]
public class MyBehaviourEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // "My Behaviour" — or "Custom Name" if [AddComponentMenu("Custom Name")] is present
        var name = target.GetScriptName();

        // "My Behaviour (2)" when a second component of the same type exists
        var nameWithIndex = ((Component)target).GetScriptNameWithIndex();

        return new Label(name);
    }
}
```
