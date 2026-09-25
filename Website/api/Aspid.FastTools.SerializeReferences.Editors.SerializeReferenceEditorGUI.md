---
title: "Class SerializeReferenceEditorGUI"
sidebar_label: "SerializeReferenceEditorGUI"
description: "Class SerializeReferenceEditorGUI — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializeReferenceEditorGUI {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI}

Namespace: [Aspid.FastTools.SerializeReferences.Editors](Aspid.FastTools.SerializeReferences.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Provides utility methods for drawing managed-reference type pickers in custom inspectors.

```csharp
public static class SerializeReferenceEditorGUI
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializeReferenceEditorGUI](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md)


## Remarks

Use [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) and [`SerializeReferenceEditorGUI.CreateList`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) in [`CreateInspectorGUI`](https://docs.unity3d.com/ScriptReference/Editor-CreateInspectorGUI.html),
and [`SerializeReferenceEditorGUI.DrawFieldLayout`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) in [`OnInspectorGUI`](https://docs.unity3d.com/ScriptReference/Editor-OnInspectorGUI.html).

## Methods

### CreateField\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateField_UnityEditor_SerializedProperty_System_String_System_Type___}

Creates a UI Toolkit type picker with nested fields and managed-reference notices.

```csharp
public static VisualElement CreateField(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

<code class="paramref">property</code> label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> uses its display name.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Extra base types every candidate must be assignable to besides the field type; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

#### Returns

 VisualElement

The field to add to the inspector's visual tree.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a managed reference.

### CreateList\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateList_UnityEditor_SerializedProperty_System_String_System_Type___}

Creates a UI Toolkit managed-reference list whose add button selects a type and appends an independent instance.

```csharp
public static VisualElement CreateList(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

An array/list property whose elements are managed references.

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

<code class="paramref">property</code> header label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> uses its display name.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Extra base types every element type must be assignable to besides the declared one; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

#### Returns

 VisualElement

The list to add to the inspector's visual tree.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a managed-reference array.

### DrawFieldLayout\(SerializedProperty, GUIContent, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_DrawFieldLayout_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type___}

Draws a managed-reference type picker and its nested fields in an IMGUI layout.

```csharp
public static void DrawFieldLayout(SerializedProperty property, GUIContent label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` GUIContent

<code class="paramref">property</code> label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> uses its display name.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Extra base types every candidate must be assignable to besides the field type; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or an empty array adds none.

#### Remarks

Lists use [`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md).

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a managed reference.

