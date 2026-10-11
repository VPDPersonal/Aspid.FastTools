---
title: "Struct ComponentTypeSelector"
sidebar_label: "ComponentTypeSelector"
description: "Struct ComponentTypeSelector — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct ComponentTypeSelector {#Aspid_FastTools_Types_ComponentTypeSelector}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Represents a marker field adding an Inspector dropdown that swaps the object's script to any subtype of the
field's declaring class.

```csharp
[Serializable]
public struct ComponentTypeSelector
```


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<ComponentTypeSelector\>\(ComponentTypeSelector, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples

Place a field of this type in the root class; the Inspector lists all subtypes of <code>BaseEnemy</code>:


```csharp
public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _typeSelector;
}

public class FastEnemy : BaseEnemy { }
public class TankEnemy : BaseEnemy { }
```


## Remarks

Picking a type writes the matching <code>MonoScript</code> asset to <code>m_Script</code>, turning the object into that
subtype. The picker is constrained to the declaring class automatically.

