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


```csharp
public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _typeSelector;
}
```


