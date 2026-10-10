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

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableMonoScript\>\(SerializableMonoScript, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

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

<p>In the editor the script asset is the source of truth: on every serialization the stored assembly-qualified name is re-read from the script's class. The script reference is editor-only, so a player build carries just the name and resolves it exactly as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) does. When that name no longer resolves in the editor, a main-thread read takes the type from the script, so an asset not saved since a class rename still works in Play Mode; another thread gets <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> then.</p>
<p>Only types Unity maps to a script asset can be referenced this way: a top-level, non-generic class declared in a file of the same name or compiled into a DLL. From a DLL, Unity maps only [`MonoBehaviour`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/MonoBehaviour.html) and [`ScriptableObject`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/ScriptableObject.html) classes, by namespace and name, so renaming the class or its namespace breaks the reference. Use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) for nested and generic types and for other DLL classes.</p>

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

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

