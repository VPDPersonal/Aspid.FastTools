---
title: "Class SerializeReferenceIMGUIList"
sidebar_label: "SerializeReferenceIMGUIList"
description: "Class SerializeReferenceIMGUIList — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializeReferenceIMGUIList {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList}

Namespace: [Aspid.FastTools.SerializeReferences.Editors](Aspid.FastTools.SerializeReferences.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Provides utility methods for drawing an IMGUI <code>[SerializeReference]</code> list whose add button opens the type
picker and appends a fresh instance.

```csharp
public static class SerializeReferenceIMGUIList
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializeReferenceIMGUIList](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Remarks

In IMGUI a <code>[TypeSelector]</code> drawer is applied to array elements and can never reach the list's own "+", so
an editor that overrides <code>OnInspectorGUI</code> gets Unity's default add — which duplicates the last element and
leaves it rid-aliased. Call [`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md) for those lists instead. Elements still go through
[`PropertyField`](https://docs.unity3d.com/ScriptReference/EditorGUI-PropertyField.html), so the per-element drawer
applies exactly as it would by default.

## Methods

### Draw\(SerializedProperty, GUIContent, Type, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList_Draw_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type_System_Type___}

Draws a <code>[SerializeReference]</code> list with a picker-backed "+".

```csharp
public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
```

#### Parameters

`listProperty` SerializedProperty

The array/list property to draw. Its elements must be managed references.

`label` GUIContent

Header label for the list.

`elementType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

Declared element type constraining the picker; needed up front because an empty
    list has no element to read it from.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Base types narrowing the candidates below <code class="paramref">elementType</code>.

