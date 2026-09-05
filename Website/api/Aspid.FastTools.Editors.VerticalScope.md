---
title: "Struct VerticalScope"
sidebar_label: "VerticalScope"
description: "Struct VerticalScope — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct VerticalScope {#Aspid_FastTools_Editors_VerticalScope}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Disposable ref struct wrapper around [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html) /
[`EndVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndVertical.html) that exposes the resulting [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect).
Use in a <code>using</code> statement to automatically close the vertical group.

```csharp
public readonly ref struct VerticalScope
```


## Fields

### Rect {#Aspid_FastTools_Editors_VerticalScope_Rect}

The [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) returned by [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html) for this group.

```csharp
public readonly Rect Rect
```

#### Field Value

 Rect

## Methods

### Begin\(params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_VerticalScope_Begin_UnityEngine_GUILayoutOption___}

Begins a vertical layout group with the given layout options.

```csharp
public static VerticalScope Begin(params GUILayoutOption[] options)
```

#### Parameters

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html).

#### Returns

 [VerticalScope](Aspid.FastTools.Editors.VerticalScope.md)

A new [`VerticalScope`](Aspid.FastTools.Editors.VerticalScope.md) whose [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) reflects the group bounds.

### Begin\(GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_VerticalScope_Begin_UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a vertical layout group with a specific [`GUIStyle`](https://docs.unity3d.com/ScriptReference/GUIStyle.html) and layout options.

```csharp
public static VerticalScope Begin(GUIStyle style, params GUILayoutOption[] options)
```

#### Parameters

`style` GUIStyle

The style to apply to the vertical group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html).

#### Returns

 [VerticalScope](Aspid.FastTools.Editors.VerticalScope.md)

A new [`VerticalScope`](Aspid.FastTools.Editors.VerticalScope.md) whose [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) reflects the group bounds.

### Begin\(out Rect, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_VerticalScope_Begin_UnityEngine_Rect__UnityEngine_GUILayoutOption___}

Begins a vertical layout group and outputs the resulting rect via an <code>out</code> parameter.

```csharp
public static VerticalScope Begin(out Rect rect, params GUILayoutOption[] options)
```

#### Parameters

`rect` Rect

Receives the [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) of the vertical group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html).

#### Returns

 [VerticalScope](Aspid.FastTools.Editors.VerticalScope.md)

A new [`VerticalScope`](Aspid.FastTools.Editors.VerticalScope.md) whose [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) reflects the group bounds.

### Begin\(out Rect, GUIStyle, params GUILayoutOption\[\]\) {#Aspid_FastTools_Editors_VerticalScope_Begin_UnityEngine_Rect__UnityEngine_GUIStyle_UnityEngine_GUILayoutOption___}

Begins a vertical layout group with a specific [`GUIStyle`](https://docs.unity3d.com/ScriptReference/GUIStyle.html) and outputs the resulting rect via an <code>out</code> parameter.

```csharp
public static VerticalScope Begin(out Rect rect, GUIStyle style, params GUILayoutOption[] options)
```

#### Parameters

`rect` Rect

Receives the [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) of the vertical group.

`style` GUIStyle

The style to apply to the vertical group.

`options` GUILayoutOption\[\]

Optional layout options passed to [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html).

#### Returns

 [VerticalScope](Aspid.FastTools.Editors.VerticalScope.md)

A new [`VerticalScope`](Aspid.FastTools.Editors.VerticalScope.md) whose [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect) reflects the group bounds.

### Dispose\(\) {#Aspid_FastTools_Editors_VerticalScope_Dispose}

Ends the vertical layout group by calling [`EndVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndVertical.html).

```csharp
public void Dispose()
```

