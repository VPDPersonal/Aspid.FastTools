---
title: "Class SerializableMonoScript"
sidebar_label: "SerializableMonoScript"
description: "Class SerializableMonoScript — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableMonoScript {#Aspid_FastTools_Types_SerializableMonoScript}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) referencing it through its <code>MonoScript</code>
asset, so renaming or moving the class does not break the field.

```csharp
[Serializable]
public class SerializableMonoScript : SerializableTypeBase, ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)

#### Derived

[SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md), 
ISerializationCallbackReceiver


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<SerializableMonoScript, TValue\>\(SerializableMonoScript, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableMonoScript, TValue\>\(SerializableMonoScript, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<SerializableMonoScript, TValue\>\(SerializableMonoScript, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples


```csharp
public class Spawner : MonoBehaviour
{
    [SerializeField] private SerializableMonoScript _componentType;

    private void Start()
    {
        Type type = _componentType;  // implicit conversion
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Remarks

<p>
In the editor the script asset is the source of truth: on every serialization the stored assembly-qualified
name is re-read from the script's class. The script reference is editor-only, so a player build carries just
the name and resolves it exactly as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) does.
</p>
<p>
Only types Unity maps to a script asset can be referenced this way — a top-level, non-generic class declared
in a file of the same name. Use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) for nested and generic types.
</p>
<p>
Unity serializes a field by its declared type, so a [`SerializableMonoScript<T>`](Aspid.FastTools.Types.SerializableMonoScript-1.md) assigned from code
to a field declared as [`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md) is reloaded unconstrained: the type survives, the
constraint does not.
</p>

## Constructors

### SerializableMonoScript\(\) {#Aspid_FastTools_Types_SerializableMonoScript__ctor}

Creates an empty wrapper.

```csharp
public SerializableMonoScript()
```

### SerializableMonoScript\(Type?\) {#Aspid_FastTools_Types_SerializableMonoScript__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code> by name only: no script asset is attached, so the
wrapper is not rename-safe until a type is picked in the Inspector.

```csharp
public SerializableMonoScript(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

### Script {#Aspid_FastTools_Types_SerializableMonoScript_Script}

Gets the editor-only script asset declaring the type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no type is stored or
the wrapper was constructed from code.

```csharp
public MonoScript? Script { get; }
```

#### Property Value

 MonoScript?

## Operators

### implicit operator Type?\(SerializableMonoScript?\) {#Aspid_FastTools_Types_SerializableMonoScript_op_Implicit_Aspid_FastTools_Types_SerializableMonoScript__System_Type}

Converts the wrapper to the type it holds.

```csharp
public static implicit operator Type?(SerializableMonoScript? type)
```

#### Parameters

`type` [SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)?

The wrapper to convert.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The wrapped type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the wrapper is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or holds no
resolvable type.

