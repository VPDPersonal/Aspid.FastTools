---
title: "Struct HorizontalScope"
sidebar_label: "HorizontalScope"
description: "Struct HorizontalScope — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct HorizontalScope {#Aspid_FastTools_Editors_HorizontalScope}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Disposable ref struct wrapper around [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html) /
[`EndHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndHorizontal.html) that exposes the resulting [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect).
Use in a <code>using</code> statement to automatically close the horizontal group.

```csharp
public readonly ref struct HorizontalScope
```


## Fields

### Rect {#Aspid_FastTools_Editors_HorizontalScope_Rect}

The [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) returned by [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html) for this group.

```csharp
public readonly Rect Rect
```

#### Field Value

 Rect

## Methods

### Begin\(params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_HorizontalScope_Begin_UnityEngine_GUILayoutOption___}

Begins a horizontal layout group with the given layout options.

```csharp
public static HorizontalScope Begin(params GUILayoutOption[] options)
```

#### Parameters

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html).

#### Returns

 [HorizontalScope](Aspid.FastTools.Editors.HorizontalScope.md)

A new [`HorizontalScope`](Aspid.FastTools.Editors.HorizontalScope.md) whose [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) reflects the group bounds.

### Begin\(GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_HorizontalScope_Begin_UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a horizontal layout group with a specific [`GUIStyle`](https://docs.unity3d.com/ScriptReference/GUIStyle.html) and layout options.

```csharp
public static HorizontalScope Begin(GUIStyle style, params GUILayoutOption[] options)
```

#### Parameters

`style` GUIStyle

The style to apply to the horizontal group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html).

#### Returns

 [HorizontalScope](Aspid.FastTools.Editors.HorizontalScope.md)

A new [`HorizontalScope`](Aspid.FastTools.Editors.HorizontalScope.md) whose [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) reflects the group bounds.

### Begin\(out Rect, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_HorizontalScope_Begin_UnityEngine_Rect__UnityEngine_GUILayoutOption___}

Begins a horizontal layout group and outputs the resulting rect via an <code>out</code> parameter.

```csharp
public static HorizontalScope Begin(out Rect rect, params GUILayoutOption[] options)
```

#### Parameters

`rect` Rect

Receives the [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) of the horizontal group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html).

#### Returns

 [HorizontalScope](Aspid.FastTools.Editors.HorizontalScope.md)

A new [`HorizontalScope`](Aspid.FastTools.Editors.HorizontalScope.md) whose [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) reflects the group bounds.

### Begin\(out Rect, GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_HorizontalScope_Begin_UnityEngine_Rect__UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a horizontal layout group with a specific [`GUIStyle`](https://docs.unity3d.com/ScriptReference/GUIStyle.html) and outputs the resulting rect via an <code>out</code> parameter.

```csharp
public static HorizontalScope Begin(out Rect rect, GUIStyle style, params GUILayoutOption[] options)
```

#### Parameters

`rect` Rect

Receives the [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) of the horizontal group.

`style` GUIStyle

The style to apply to the horizontal group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html).

#### Returns

 [HorizontalScope](Aspid.FastTools.Editors.HorizontalScope.md)

A new [`HorizontalScope`](Aspid.FastTools.Editors.HorizontalScope.md) whose [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect) reflects the group bounds.

### Dispose\(\) {#Aspid_FastTools_Editors_HorizontalScope_Dispose}

Ends the horizontal layout group by calling [`EndHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndHorizontal.html).

```csharp
public void Dispose()
```

