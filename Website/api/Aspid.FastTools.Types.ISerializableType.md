---
title: "Interface ISerializableType"
sidebar_label: "ISerializableType"
description: "Interface ISerializableType — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Interface ISerializableType {#Aspid_FastTools_Types_ISerializableType}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.Unity.dll  

Common contract of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers
([`SerializableType`](Aspid.FastTools.Types.SerializableType.md) and [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md)).

```csharp
public interface ISerializableType
```

#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<ISerializableType, TValue\>\(ISerializableType, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<ISerializableType, TValue\>\(ISerializableType, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<ISerializableType, TValue\>\(ISerializableType, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<ISerializableType, TValue\>\(ISerializableType, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<ISerializableType, TValue\>\(ISerializableType, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<ISerializableType, TValue\>\(ISerializableType, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Properties

### BaseType {#Aspid_FastTools_Types_ISerializableType_BaseType}

The constraint that stored types must satisfy — candidate types offered
by the editor picker are assignable to it; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### Type {#Aspid_FastTools_Types_ISerializableType_Type}

The resolved [`Type`](https://learn.microsoft.com/dotnet/api/system.type), or <code>null</code> when no type is stored
or the stored assembly-qualified name cannot be resolved.

```csharp
Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

