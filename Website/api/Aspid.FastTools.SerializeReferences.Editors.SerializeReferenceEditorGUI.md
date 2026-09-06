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

Provides utility methods for drawing <code>[SerializeReference]</code> properties with the package's type-dropdown
UI from a custom editor's own code, with no <code>[TypeSelector]</code> attribute.

```csharp
public static class SerializeReferenceEditorGUI
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializeReferenceEditorGUI](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Examples


```csharp
public override VisualElement CreateInspectorGUI()
{
    var root = new VisualElement();
    root.Add(SerializeReferenceEditorGUI.CreateField(serializedObject.FindProperty("_weapon")));
    root.Add(SerializeReferenceEditorGUI.CreateList(serializedObject.FindProperty("_modifiers")));
    return root;
}
```


## Remarks

Call [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) and [`SerializeReferenceEditorGUI.CreateList`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from <code>CreateInspectorGUI</code>, and
[`SerializeReferenceEditorGUI.DrawFieldLayout`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from an IMGUI <code>OnInspectorGUI</code>; IMGUI lists go through
[`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md).

## Methods

### CreateField\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateField_UnityEditor_SerializedProperty_System_String_System_Type___}

Builds the dropdown field for one <code>[SerializeReference]</code> property: a foldout whose header carries the
type dropdown and whose content hosts the instance's fields, with the package's usual notices.

```csharp
public static VisualElement CreateField(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

Field label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Base types narrowing the picker below the field's declared type.

#### Returns

 VisualElement

The field to add to the inspector's visual tree.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when <code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when the property is not a managed reference.

### CreateList\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateList_UnityEditor_SerializedProperty_System_String_System_Type___}

Builds the list for a <code>[SerializeReference]</code> array: every element renders as the dropdown field and
the "+" opens the type picker, appending a fresh instance instead of a rid-aliased duplicate.

```csharp
public static VisualElement CreateList(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

An array/list property whose elements are managed references.

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

Header label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Base types narrowing the picker below the declared element type.

#### Returns

 VisualElement

The list to add to the inspector's visual tree.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when <code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when the property is not a managed-reference array.

### DrawFieldLayout\(SerializedProperty, GUIContent, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_DrawFieldLayout_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type___}

Reserves a layout rect and draws into it the same dropdown field as [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md).

```csharp
public static void DrawFieldLayout(SerializedProperty property, GUIContent label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` GUIContent

Field label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Base types narrowing the picker below the field's declared type.

#### Remarks

Lists use [`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md).

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when <code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when the property is not a managed reference.

