---
title: "Class SerializableMonoScript<T>"
sidebar_label: "SerializableMonoScript<T>"
description: "Class SerializableMonoScript<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableMonoScript\<T\> {#Aspid_FastTools_Types_SerializableMonoScript_1}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

[`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md) constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableMonoScript<T> : SerializableMonoScript, ISerializableType, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

Base constraint type; the picker offers only types assignable to it.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md) ← 
[SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md), 
ISerializationCallbackReceiver


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<SerializableMonoScript\<T\>, TValue\>\(SerializableMonoScript\<T\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples


```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private SerializableMonoScript<Enemy> _enemyType;

    private void Spawn() =>
        gameObject.AddComponent(_enemyType.Type);
}
```


## Constructors

### SerializableMonoScript\(\) {#Aspid_FastTools_Types_SerializableMonoScript_1__ctor}

Creates an empty wrapper.

```csharp
public SerializableMonoScript()
```

### SerializableMonoScript\(Type?\) {#Aspid_FastTools_Types_SerializableMonoScript_1__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code> by name only: no script asset is attached, so the
wrapper is not rename-safe until a type is picked in the Inspector.

```csharp
public SerializableMonoScript(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when <code class="paramref">type</code> is not assignable to <code class="typeparamref">T</code>.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_1_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

