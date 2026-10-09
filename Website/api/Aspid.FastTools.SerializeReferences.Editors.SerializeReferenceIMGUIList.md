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


## Remarks

The add button creates an independent instance in every selected object; element fields retain their registered property drawers.
A <code>[TypeSelector]</code> on the list field adds its constraints to <code>baseTypes</code>.

## Methods

### Draw\(SerializedProperty, GUIContent, Type, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList_Draw_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type_System_Type___}

Draws a managed-reference list whose add button selects a type and appends an independent instance.

```csharp
public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
```

#### Parameters

`listProperty` SerializedProperty

An array/list property whose elements are managed references.

`label` GUIContent

The list header; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> uses the display name of <code class="paramref">listProperty</code>, [`none`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/GUIContent-none.html) displays no label.

`elementType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The declared element type constraining the picker, supplied even when the list is empty.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Additional constraints below <code class="paramref">elementType</code>; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">listProperty</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">listProperty</code> is not a managed-reference array.

### Draw\(SerializedProperty, GUIContent, Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList_Draw_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type___}

Draws a managed-reference list whose add button selects a type and appends an independent instance, taking the element type from the field declaration.

```csharp
public static void Draw(SerializedProperty listProperty, GUIContent label = null, Type[] baseTypes = null)
```

#### Parameters

`listProperty` SerializedProperty

An array/list property whose elements are managed references.

`label` GUIContent

The list header; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> uses the display name of <code class="paramref">listProperty</code>, [`none`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/GUIContent-none.html) displays no label.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Additional constraints below the element type; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

#### Remarks

When the list is empty and its field cannot be found by reflection, the element type falls back to [`Object`](https://learn.microsoft.com/dotnet/api/system.object); pass it to the overload with <code>elementType</code> then.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">listProperty</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">listProperty</code> is not a managed-reference array.

