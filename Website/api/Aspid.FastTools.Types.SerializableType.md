---
title: "Class SerializableType"
sidebar_label: "SerializableType"
description: "Class SerializableType — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableType {#Aspid_FastTools_Types_SerializableType}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

A wrapper around [`Type`](https://learn.microsoft.com/dotnet/api/system.type) that supports Unity Inspector serialization.
The type is stored by its <code>AssemblyQualifiedName</code> and resolved lazily on first access.

```csharp
[Serializable]
public sealed class SerializableType : ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md), 
ISerializationCallbackReceiver


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<SerializableType, TValue\>\(SerializableType, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableType, TValue\>\(SerializableType, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<SerializableType, TValue\>\(SerializableType, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<SerializableType, TValue\>\(SerializableType, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<SerializableType, TValue\>\(SerializableType, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<SerializableType, TValue\>\(SerializableType, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Declare a serializable type field and use the resolved type at runtime:


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType _targetType;

    private void Start()
    {
        Type type = _targetType;  // implicit conversion
        if (type != null)
            Debug.Log(type.FullName);
    }
}
```


## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_BaseType}

The constraint that stored types must satisfy — candidate types offered
by the editor picker are assignable to it; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### Type {#Aspid_FastTools_Types_SerializableType_Type}

The resolved [`Type`](https://learn.microsoft.com/dotnet/api/system.type), or <code>null</code> when no type is stored
or the stored assembly-qualified name cannot be resolved.

```csharp
public Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

## Operators

### implicit operator Type?\(SerializableType?\) {#Aspid_FastTools_Types_SerializableType_op_Implicit_Aspid_FastTools_Types_SerializableType__System_Type}

Resolves and returns the wrapped type; equivalent to [`SerializableType.Type`](Aspid.FastTools.Types.SerializableType.md#Aspid_FastTools_Types_SerializableType_Type).
A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> wrapper converts to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
public static implicit operator Type?(SerializableType? type)
```

#### Parameters

`type` [SerializableType](Aspid.FastTools.Types.SerializableType.md)?

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

