---
title: "Class TypeSelectorWindow"
sidebar_label: "TypeSelectorWindow"
description: "Class TypeSelectorWindow — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeSelectorWindow {#Aspid_FastTools_Types_Editors_TypeSelectorWindow}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

[`EditorWindow`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/EditorWindow.html) for selecting a type from a filtered hierarchy.

```csharp
public sealed class TypeSelectorWindow : EditorWindow
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
Object ← 
ScriptableObject ← 
EditorWindow ← 
[TypeSelectorWindow](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md)


#### Extension Methods

[EditorExtensions.GetDisplayName\(Object\)](Aspid.FastTools.Editors.EditorExtensions.md#Aspid_FastTools_Editors_EditorExtensions_GetDisplayName_UnityEngine_Object_),
[ProfilerMarkerExtensionsForGenerator.Marker\<TypeSelectorWindow\>\(TypeSelectorWindow, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Methods

### Show\(Rect, TypeSelectorFilter, string, Action\<string\>\) {#Aspid_FastTools_Types_Editors_TypeSelectorWindow_Show_UnityEngine_Rect_Aspid_FastTools_Types_Editors_TypeSelectorFilter_System_String_System_Action_System_String__}

Opens the selector as a dropdown anchored to <code class="paramref">screenRect</code>.

```csharp
public static void Show(Rect screenRect, TypeSelectorFilter filter = default, string currentAqn = "", Action<string> onSelected = null)
```

#### Parameters

`screenRect` Rect

Screen-space rectangle the dropdown is anchored to.

`filter` [TypeSelectorFilter](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md)

Which types the selector offers.

`currentAqn` [string](https://learn.microsoft.com/dotnet/api/system.string)

The current type name; empty selects the empty row, while <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> leaves the selection unset.

`onSelected` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

Receives the assembly-qualified name of the selected type — the constructed
    closed type for a resolved open generic — or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for <code>&lt;None&gt;</code>; a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> callback is ignored.

