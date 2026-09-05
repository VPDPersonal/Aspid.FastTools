---
title: "Struct ScrollViewScope"
sidebar_label: "ScrollViewScope"
description: "Struct ScrollViewScope — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct ScrollViewScope {#Aspid_FastTools_Editors_ScrollViewScope}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

Disposable ref struct wrapper around [`BeginScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginScrollView.html) /
[`EndScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndScrollView.html) that updates the caller's scroll position in place
and exposes it via [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition). Use in a <code>using</code> statement to automatically
close the scroll view.

```csharp
public readonly ref struct ScrollViewScope
```


## Fields

### ScrollPosition {#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition}

The updated scroll position returned by [`BeginScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginScrollView.html).

```csharp
public readonly Vector2 ScrollPosition
```

#### Field Value

 Vector2

## Methods

### Begin\(ref Vector2, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_ScrollViewScope_Begin_UnityEngine_Vector2__UnityEngine_GUILayoutOption___}

Begins a scroll view, updating the caller's scroll position variable in place.

```csharp
public static ScrollViewScope Begin(ref Vector2 scrollPosition, params GUILayoutOption[] options)
```

#### Parameters

`scrollPosition` Vector2

Reference to the current scroll position; updated to the new value.

`options` GUILayoutOption\[\]

Optional layout options.

#### Returns

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

A new [`ScrollViewScope`](Aspid.FastTools.Editors.ScrollViewScope.md) with the updated [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition).

### Begin\(ref Vector2, bool, bool, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_ScrollViewScope_Begin_UnityEngine_Vector2__System_Boolean_System_Boolean_UnityEngine_GUILayoutOption___}

Begins a scroll view with explicit scrollbar visibility flags, updating the caller's variable in place.

```csharp
public static ScrollViewScope Begin(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options)
```

#### Parameters

`scrollPosition` Vector2

Reference to the current scroll position; updated to the new value.

`alwaysShowHorizontal` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Always show the horizontal scrollbar.

`alwaysShowVertical` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Always show the vertical scrollbar.

`options` GUILayoutOption\[\]

Optional layout options.

#### Returns

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

A new [`ScrollViewScope`](Aspid.FastTools.Editors.ScrollViewScope.md) with the updated [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition).

### Begin\(ref Vector2, GUIStyle, GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_ScrollViewScope_Begin_UnityEngine_Vector2__UnityEngine_GUIStyle_UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a scroll view with custom scrollbar styles, updating the caller's variable in place.

```csharp
public static ScrollViewScope Begin(ref Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options)
```

#### Parameters

`scrollPosition` Vector2

Reference to the current scroll position; updated to the new value.

`horizontalScrollbar` GUIStyle

Style for the horizontal scrollbar.

`verticalScrollbar` GUIStyle

Style for the vertical scrollbar.

`options` GUILayoutOption\[\]

Optional layout options.

#### Returns

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

A new [`ScrollViewScope`](Aspid.FastTools.Editors.ScrollViewScope.md) with the updated [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition).

### Begin\(ref Vector2, GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_ScrollViewScope_Begin_UnityEngine_Vector2__UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a scroll view with a single style, updating the caller's variable in place.

```csharp
public static ScrollViewScope Begin(ref Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options)
```

#### Parameters

`scrollPosition` Vector2

Reference to the current scroll position; updated to the new value.

`style` GUIStyle

Style applied to the scroll view.

`options` GUILayoutOption\[\]

Optional layout options.

#### Returns

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

A new [`ScrollViewScope`](Aspid.FastTools.Editors.ScrollViewScope.md) with the updated [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition).

### Begin\(ref Vector2, bool, bool, GUIStyle, GUIStyle, GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_ScrollViewScope_Begin_UnityEngine_Vector2__System_Boolean_System_Boolean_UnityEngine_GUIStyle_UnityEngine_GUIStyle_UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a scroll view with full control over scrollbar visibility, styles, and background,
updating the caller's variable in place.

```csharp
public static ScrollViewScope Begin(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options)
```

#### Parameters

`scrollPosition` Vector2

Reference to the current scroll position; updated to the new value.

`alwaysShowHorizontal` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Always show the horizontal scrollbar.

`alwaysShowVertical` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Always show the vertical scrollbar.

`horizontalScrollbar` GUIStyle

Style for the horizontal scrollbar.

`verticalScrollbar` GUIStyle

Style for the vertical scrollbar.

`background` GUIStyle

Background style for the scroll view.

`options` GUILayoutOption\[\]

Optional layout options.

#### Returns

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

A new [`ScrollViewScope`](Aspid.FastTools.Editors.ScrollViewScope.md) with the updated [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition).

### Dispose\(\) {#Aspid_FastTools_Editors_ScrollViewScope_Dispose}

Ends the scroll view by calling [`EndScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndScrollView.html).

```csharp
public void Dispose()
```

