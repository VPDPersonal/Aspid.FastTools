---
title: "Class IdRegistry<T>"
sidebar_label: "IdRegistry<T>"
description: "Class IdRegistry<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IdRegistry\<T\> {#Aspid_FastTools_Ids_IdRegistry_1}

Namespace: [Aspid.FastTools.Ids](Aspid.FastTools.Ids.md)  
Assembly: Aspid.FastTools.dll  

A strongly typed wrapper around [`IdRegistry`](Aspid.FastTools.Ids.IdRegistry.md) that exposes [`IId`](Aspid.FastTools.Ids.IId.md)-aware membership checks.

```csharp
public class IdRegistry<T> : IdRegistry, IEnumerable<KeyValuePair<int, string>>, IEnumerable where T : struct, IId
```

#### Type Parameters

`T` 

The id struct type bound to this registry.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
Object ← 
ScriptableObject ← 
[IdRegistry](Aspid.FastTools.Ids.IdRegistry.md) ← 
[IdRegistry\<T\>](Aspid.FastTools.Ids.IdRegistry-1.md)

#### Implements

[IEnumerable\<KeyValuePair\<int, string\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1), 
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[EditorExtensions.GetScriptName\(Object\)](Aspid.FastTools.Editors.EditorExtensions.md#Aspid_FastTools_Editors_EditorExtensions_GetScriptName_UnityEngine_Object_), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<IdRegistry\<T\>, TValue\>\(IdRegistry\<T\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Methods

### Contains\(T\) {#Aspid_FastTools_Ids_IdRegistry_1_Contains__0_}

```csharp
public bool Contains(T id)
```

#### Parameters

`id` T

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### TryGetName\(T, out string\) {#Aspid_FastTools_Ids_IdRegistry_1_TryGetName__0_System_String__}

```csharp
public bool TryGetName(T id, out string nameId)
```

#### Parameters

`id` T

`nameId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

