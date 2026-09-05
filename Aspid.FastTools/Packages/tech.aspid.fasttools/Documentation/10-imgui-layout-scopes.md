# IMGUI Layout Scopes

Three `ref struct` scopes — `VerticalScope`, `HorizontalScope`, `ScrollViewScope` — wrap `EditorGUILayout.Begin*` / `End*`. Each exposes a `Rect` property and calls the matching `End*` method on `Dispose`:

```csharp
using Aspid.FastTools.Editors;

using (VerticalScope.Begin())
{
    EditorGUILayout.LabelField("Item 1");
    EditorGUILayout.LabelField("Item 2");
}

var scrollPos = Vector2.zero;
using (ScrollViewScope.Begin(ref scrollPos))
{
    EditorGUILayout.LabelField("Scrollable content");
}
```

All `Begin` overloads match the corresponding `EditorGUILayout.Begin*` signatures (optional `GUIStyle`, `GUILayoutOption[]`, scroll view options), plus an `out Rect` variant for drawing into the group's rect.
