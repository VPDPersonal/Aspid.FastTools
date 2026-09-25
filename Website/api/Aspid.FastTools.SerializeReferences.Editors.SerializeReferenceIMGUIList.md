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

Provides utility methods for drawing managed-reference lists with a type picker for new elements in IMGUI.

```csharp
public static class SerializeReferenceIMGUIList
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializeReferenceIMGUIList](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Remarks

The add button creates an independent instance; element fields retain their registered property drawers.

## Methods

### Draw\(SerializedProperty, GUIContent, Type, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList_Draw_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type_System_Type___}

Draws a managed-reference list whose add button selects a type and appends an independent instance.

```csharp
public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
```

#### Parameters

`listProperty` SerializedProperty

The array or list of managed references; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or a non-array property draws nothing.

`label` GUIContent

The list header; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> displays no label.

`elementType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The declared element type constraining the picker, supplied even when the list is empty.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Additional constraints below <code class="paramref">elementType</code>; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

