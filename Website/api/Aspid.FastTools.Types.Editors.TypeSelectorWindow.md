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

Dropdown window for browsing and selecting a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) from a filtered list.

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

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[EditorExtensions.GetScriptName\(Object\)](Aspid.FastTools.Editors.EditorExtensions.md#Aspid_FastTools_Editors_EditorExtensions_GetScriptName_UnityEngine_Object_), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorWindow, TValue\>\(TypeSelectorWindow, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Remarks

A thin host around the selector view, which owns the search, navigation and generic-argument flow.

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

Assembly-qualified name of the current type, pre-navigated to; empty starts at
    the root.

`onSelected` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

Receives the assembly-qualified name of the selected type — the constructed
    closed type for a resolved open generic — or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for <code>&lt;None&gt;</code>.

