---
title: "Class SerializableType<T>"
sidebar_label: "SerializableType<T>"
description: "Class SerializableType<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableType\<T\> {#Aspid_FastTools_Types_SerializableType_1}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.Unity.dll  

A wrapper around [`Type`](https://learn.microsoft.com/dotnet/api/system.type) that supports Unity Inspector serialization,
constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableType<T> : ISerializableType, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

The base constraint type. The editor picker offers only types assignable to it.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md), 
ISerializationCallbackReceiver


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Constrain the picker to <code>MonoBehaviour</code> subtypes only:


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType<MonoBehaviour> _behaviourType;

    private void Start()
    {
        Type type = _behaviourType;  // always a MonoBehaviour subtype or null
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_1_BaseType}

The constraint that stored types must satisfy — candidate types offered
by the editor picker are assignable to it; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### Type {#Aspid_FastTools_Types_SerializableType_1_Type}

The resolved [`Type`](https://learn.microsoft.com/dotnet/api/system.type), or <code>null</code> when no type is stored
or the stored assembly-qualified name cannot be resolved.

```csharp
public Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

## Operators

### implicit operator Type?\(SerializableType\<T\>?\) {#Aspid_FastTools_Types_SerializableType_1_op_Implicit_Aspid_FastTools_Types_SerializableType__0___System_Type}

Resolves and returns the wrapped type; equivalent to [`SerializableType<T>.Type`](Aspid.FastTools.Types.SerializableType-1.md#Aspid_FastTools_Types_SerializableType_1_Type).
A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> wrapper converts to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
public static implicit operator Type?(SerializableType<T>? type)
```

#### Parameters

`type` [SerializableType](Aspid.FastTools.Types.SerializableType-1.md)\<T\>?

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

