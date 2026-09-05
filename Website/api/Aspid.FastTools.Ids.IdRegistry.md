---
title: "Class IdRegistry"
sidebar_label: "IdRegistry"
description: "Class IdRegistry — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IdRegistry {#Aspid_FastTools_Ids_IdRegistry}

Namespace: [Aspid.FastTools.Ids](Aspid.FastTools.Ids.md)  
Assembly: Aspid.FastTools.dll  

A ScriptableObject that maps string names to stable integer IDs for a given struct type.

```csharp
[CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/Id Registry")]
public class IdRegistry : ScriptableObject, IEnumerable<KeyValuePair<int, string>>, IEnumerable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
Object ← 
ScriptableObject ← 
[IdRegistry](Aspid.FastTools.Ids.IdRegistry.md)

#### Derived

[IdRegistry\<T\>](Aspid.FastTools.Ids.IdRegistry-1.md)

#### Implements

[IEnumerable\<KeyValuePair\<int, string\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1), 
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<IdRegistry, TValue\>\(IdRegistry, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[EditorExtensions.GetScriptName\(Object\)](Aspid.FastTools.Editors.EditorExtensions.md#Aspid_FastTools_Editors_EditorExtensions_GetScriptName_UnityEngine_Object_), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<IdRegistry, TValue\>\(IdRegistry, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<IdRegistry, TValue\>\(IdRegistry, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<IdRegistry, TValue\>\(IdRegistry, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<IdRegistry, TValue\>\(IdRegistry, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<IdRegistry, TValue\>\(IdRegistry, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Properties

### Count {#Aspid_FastTools_Ids_IdRegistry_Count}

```csharp
public int Count { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### IdNames {#Aspid_FastTools_Ids_IdRegistry_IdNames}

```csharp
public IReadOnlyList<string> IdNames { get; }
```

#### Property Value

 [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

### Ids {#Aspid_FastTools_Ids_IdRegistry_Ids}

```csharp
public IReadOnlyList<int> Ids { get; }
```

#### Property Value

 [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### IsCacheDirty {#Aspid_FastTools_Ids_IdRegistry_IsCacheDirty}

```csharp
public bool IsCacheDirty { get; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### Contains\(int\) {#Aspid_FastTools_Ids_IdRegistry_Contains_System_Int32_}

```csharp
public bool Contains(int id)
```

#### Parameters

`id` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Contains\(string\) {#Aspid_FastTools_Ids_IdRegistry_Contains_System_String_}

```csharp
public bool Contains(string nameId)
```

#### Parameters

`nameId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### EnsureCache\(\) {#Aspid_FastTools_Ids_IdRegistry_EnsureCache}

```csharp
public void EnsureCache()
```

### GetEnumerator\(\) {#Aspid_FastTools_Ids_IdRegistry_GetEnumerator}

Returns an enumerator that iterates through the collection.

```csharp
public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
```

#### Returns

 [IEnumerator](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerator-1)\<[KeyValuePair](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2)\<[int](https://learn.microsoft.com/dotnet/api/system.int32), [string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

An enumerator that can be used to iterate through the collection.

### InvalidateCache\(\) {#Aspid_FastTools_Ids_IdRegistry_InvalidateCache}

```csharp
public void InvalidateCache()
```

### OnEnable\(\) {#Aspid_FastTools_Ids_IdRegistry_OnEnable}

```csharp
protected virtual void OnEnable()
```

### OnValidate\(\) {#Aspid_FastTools_Ids_IdRegistry_OnValidate}

```csharp
protected virtual void OnValidate()
```

### TryGetId\(string, out int\) {#Aspid_FastTools_Ids_IdRegistry_TryGetId_System_String_System_Int32__}

```csharp
public bool TryGetId(string nameId, out int id)
```

#### Parameters

`nameId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`id` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### TryGetName\(int, out string\) {#Aspid_FastTools_Ids_IdRegistry_TryGetName_System_Int32_System_String__}

```csharp
public bool TryGetName(int id, out string nameId)
```

#### Parameters

`id` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`nameId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

