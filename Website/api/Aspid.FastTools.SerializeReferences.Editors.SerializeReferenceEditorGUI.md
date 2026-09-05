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
Assembly: Aspid.FastTools.Unity.Editor.dll  

The custom-editor entry point to the SerializeReference dropdown field: draws a <code>[SerializeReference]</code>
property with the package's type-dropdown UI from an editor's own code, no <code>[TypeSelector]</code> attribute
needed. This is how a custom editor offers the same fields Unity's own inspector would need
<code>[TypeSelector]</code> for: [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) / [`SerializeReferenceEditorGUI.CreateList`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from
<code>CreateInspectorGUI</code>, [`SerializeReferenceEditorGUI.DrawFieldLayout`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from an IMGUI <code>OnInspectorGUI</code> (lists there:
[`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)).

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


## Methods

### CreateField\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateField_UnityEditor_SerializedProperty_System_String_System_Type___}

Builds the dropdown field for a single <code>[SerializeReference]</code> property: a foldout whose header
carries the type dropdown (backed by the hierarchical type picker) and whose content hosts the assigned
instance's fields, with the package's usual notices (missing type, shared reference, mixed selection).

```csharp
public static VisualElement CreateField(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

Field label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Optional base types narrowing the picker below the field's declared type,
    mirroring the <code>[TypeSelector(...)]</code> arguments.

#### Returns

 VisualElement

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

The property is not a managed reference.

### CreateList\(SerializedProperty, string, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_CreateList_UnityEditor_SerializedProperty_System_String_System_Type___}

Builds the list for a <code>[SerializeReference]</code> array/list property: every element renders as the
dropdown field and the "+" opens the type picker, appending a fresh typed instance (never a rid-aliased
duplicate of the last element).

```csharp
public static VisualElement CreateList(SerializedProperty property, string label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

An array/list property whose elements are managed references.

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

Header label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Optional base types narrowing the picker below the declared element type,
    mirroring the <code>[TypeSelector(...)]</code> arguments.

#### Returns

 VisualElement

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

The property is not a managed-reference array/list.

### DrawFieldLayout\(SerializedProperty, GUIContent, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceEditorGUI_DrawFieldLayout_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type___}

IMGUI twin of [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) for an <code>OnInspectorGUI</code>-based editor: reserves the layout rect
and draws the same dropdown field into it. Lists have their own IMGUI entry,
[`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md).

```csharp
public static void DrawFieldLayout(SerializedProperty property, GUIContent label = null, params Type[] baseTypes)
```

#### Parameters

`property` SerializedProperty

A managed-reference property of the editor's [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

`label` GUIContent

Field label; the property's display name when omitted.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Optional base types narrowing the picker below the field's declared type,
    mirroring the <code>[TypeSelector(...)]</code> arguments.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

The property is not a managed reference.

