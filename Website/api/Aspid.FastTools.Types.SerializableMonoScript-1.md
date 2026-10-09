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

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableMonoScript\<T\>\>\(SerializableMonoScript\<T\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private SerializableMonoScript<Enemy> _enemyType;

    private void Spawn()
    {
        if (_enemyType.Type is { } type)
            gameObject.AddComponent(type);
    }
}
```


## Remarks

Unity serializes a field by its declared type, so a [`SerializableMonoScript<T>`](Aspid.FastTools.Types.SerializableMonoScript-1.md) assigned from code
to a field declared as [`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md) is reloaded unconstrained: the type survives, the
constraint does not. Only the Inspector checks <code class="typeparamref">T</code> when a type is picked or dropped: a
loaded name is not re-checked, so after <code class="typeparamref">T</code> or the stored class's base changes, the type
may no longer be assignable to it.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_1_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

