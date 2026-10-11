---
title: "Enum TypeAllow"
sidebar_label: "TypeAllow"
description: "Enum TypeAllow — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Enum TypeAllow {#Aspid_FastTools_Types_TypeAllow}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Specifies which special type categories the type picker offers in addition to concrete classes.

```csharp
[Flags]
public enum TypeAllow
```

#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<TypeAllow\>\(TypeAllow, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Fields

`None = 0` 

Only concrete types are offered.



`Abstract = 1` 

Abstract classes are offered too. Static classes never are.



`Interface = 2` 

Interfaces are offered too.



`All = 3` 

Both abstract classes and interfaces are offered.



## See Also

[TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)

