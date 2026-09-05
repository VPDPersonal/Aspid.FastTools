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
Assembly: Aspid.FastTools.Unity.Editor.dll  

IMGUI parity for the UIToolkit ListView's picker-backed "+": draws a <code>[SerializeReference]</code> list/array whose
add button opens the type picker and appends a fresh typed instance (or an empty <code>&lt;None&gt;</code> element),
mirroring [`Editors.SerializeReferenceListAddBehavior`](Aspid.FastTools.SerializeReferences.Editors.md).

```csharp
public static class SerializeReferenceIMGUIList
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializeReferenceIMGUIList](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Remarks

Unity applies a <code>[TypeSelector]</code> [`PropertyDrawer`](https://docs.unity3d.com/ScriptReference/PropertyDrawer.html) to array <b>elements</b> in the IMGUI path, so
the drawer can never reach the list's own "+" button — the UIToolkit side only manages it by walking up to the
live <code>ListView</code>, which immediate-mode IMGUI has no equivalent of. A custom [`Editor`](https://docs.unity3d.com/ScriptReference/Editor.html) that forces
IMGUI (overrides <code>OnInspectorGUI</code> without <code>CreateInspectorGUI</code>) therefore gets Unity's default add on its
<code>[SerializeReference]</code> lists — duplicating the last element and leaving it rid-aliased. Call
[`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md) for those lists instead to restore the picker-backed, de-aliased add. Elements are drawn with
[`PropertyField`](https://docs.unity3d.com/ScriptReference/EditorGUI-PropertyField.html), so each still routes through the
<code>[TypeSelector]</code> drawer exactly as the default list drawing would.

## Methods

### Draw\(SerializedProperty, GUIContent, Type, params Type\[\]\) {#Aspid_FastTools_SerializeReferences_Editors_SerializeReferenceIMGUIList_Draw_UnityEditor_SerializedProperty_UnityEngine_GUIContent_System_Type_System_Type___}

Draws <code class="paramref">listProperty</code> (a <code>[SerializeReference]</code> list/array) with a picker-backed "+".

```csharp
public static void Draw(SerializedProperty listProperty, GUIContent label, Type elementType, params Type[] baseTypes)
```

#### Parameters

`listProperty` SerializedProperty

The array/list property to draw. Its elements must be managed references.

`label` GUIContent

Header label for the list.

`elementType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The declared element type constraining the picker (e.g. the list's <code>T</code>). Needed
    up front because an empty list has no element to read it from.

`baseTypes` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

Optional base types that narrow the candidate list below <code class="paramref">elementType</code>,
    mirroring the <code>[TypeSelector(...)]</code> arguments.

