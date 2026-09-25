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
Assembly: Aspid.FastTools.dll  

Provides the extension methods that mark call sites for the profiler-marker source generator.

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

Opens a `ProfilerMarker` scope unique to this call site.

```csharp
public static ProfilerMarker.AutoScope Marker(this object instance)
```

#### Parameters

`instance` [object](https://learn.microsoft.com/dotnet/api/system.object)

The instance the scope is opened on; its value is never read.

#### Returns

 ProfilerMarker.AutoScope

An empty scope, since this body never runs.

#### Remarks

For every type that calls this method the generator emits a closer overload that overload
resolution picks instead, holding one `ProfilerMarker` per enclosing type, member and line.

### WithName\(in AutoScope, string\) {#ProfilerMarkerExtensionsForGenerator_WithName_Unity_Profiling_ProfilerMarker_AutoScope__System_String_}

Names the `ProfilerMarker` that the generator creates for the preceding [`ProfilerMarkerExtensionsForGenerator.Marker`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_) call.

```csharp
public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name)
```

#### Parameters

`marker` ProfilerMarker.AutoScope

The scope returned by [`ProfilerMarkerExtensionsForGenerator.Marker`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_).

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

The text replacing the member part of the marker name. Read from the source at compile time,
so it must be a string literal or an interpolated string without holes; anything else leaves the name untouched.

#### Returns

 ProfilerMarker.AutoScope

<code class="paramref">marker</code> unchanged — at runtime the call is a pass-through.

