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
Assembly: Aspid.FastTools.dll  

[`SerializableType`](Aspid.FastTools.Types.SerializableType.md) constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableType<T> : SerializableType, ISerializableType, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

Base constraint type; the picker offers only types assignable to it.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md) ← 
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


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType<MonoBehaviour> _behaviorType;

    private void Start()
    {
        Type type = _behaviorType;  // always a MonoBehaviour subtype or null
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Constructors

### SerializableType\(\) {#Aspid_FastTools_Types_SerializableType_1__ctor}

Creates an empty wrapper.

```csharp
public SerializableType()
```

### SerializableType\(Type?\) {#Aspid_FastTools_Types_SerializableType_1__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code>.

```csharp
public SerializableType(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when <code class="paramref">type</code> is not assignable to <code class="typeparamref">T</code>.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_1_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

