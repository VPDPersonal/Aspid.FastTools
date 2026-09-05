---
title: "Class ProfilerMarkerExtensionsForGenerator"
sidebar_label: "ProfilerMarkerExtensionsForGenerator"
description: "Class ProfilerMarkerExtensionsForGenerator — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ProfilerMarkerExtensionsForGenerator {#ProfilerMarkerExtensionsForGenerator}

Namespace:   
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class ProfilerMarkerExtensionsForGenerator
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ProfilerMarkerExtensionsForGenerator](ProfilerMarkerExtensionsForGenerator.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### Marker\(object\) {#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_}

Marker for the source generator. At runtime this method is never called —
the generator replaces every call site with a unique `ProfilerMarker` scoped to the enclosing type, method, and line number.

```csharp
public static ProfilerMarker.AutoScope Marker(this object _)
```

#### Parameters

`_` [object](https://learn.microsoft.com/dotnet/api/system.object)

#### Returns

 ProfilerMarker.AutoScope

### WithName\(in AutoScope, string\) {#ProfilerMarkerExtensionsForGenerator_WithName_Unity_Profiling_ProfilerMarker_AutoScope__System_String_}

Marker for the source generator. Allows specifying a custom display name for the generated `ProfilerMarker`.
At runtime this method is never called — the generator uses the supplied name when creating the marker.

```csharp
public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string _)
```

#### Parameters

`marker` ProfilerMarker.AutoScope

`_` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 ProfilerMarker.AutoScope

