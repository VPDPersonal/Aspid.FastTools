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

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type), stored by its <code>AssemblyQualifiedName</code>
and resolved lazily on first access.

```csharp
[Serializable]
public class SerializableType : SerializableTypeBase, ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Derived

[SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

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


## Remarks

Unity serializes a field by its declared type, so a [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md) assigned from code to a
field declared as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) is reloaded unconstrained: the type survives, the constraint
does not.

## Constructors

### SerializableType\(\) {#Aspid_FastTools_Types_SerializableType__ctor}

Creates an empty wrapper.

```csharp
public SerializableType()
```

### SerializableType\(Type?\) {#Aspid_FastTools_Types_SerializableType__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code>.

```csharp
public SerializableType(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

## Operators

### implicit operator Type?\(SerializableType?\) {#Aspid_FastTools_Types_SerializableType_op_Implicit_Aspid_FastTools_Types_SerializableType__System_Type}

Converts the wrapper to the type it holds.

```csharp
public static implicit operator Type?(SerializableType? type)
```

#### Parameters

`type` [SerializableType](Aspid.FastTools.Types.SerializableType.md)?

The wrapper to convert.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The wrapped type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the wrapper is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or holds no
resolvable type.

