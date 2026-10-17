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


## Remarks

Declared without a namespace, so code completion offers [`ProfilerMarkerExtensionsForGenerator.Marker<T>`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_) after any expression.

## Methods

### Marker\<T\>\(T, int\) {#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_}

Opens the [`ProfilerMarker`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Unity.Profiling.ProfilerMarker.html) of this call site, named <code>Type.Member (line)</code>.

```csharp
public static ProfilerMarker.AutoScope Marker<T>(this T instance, int line = -1)
```

#### Parameters

`instance` T

The instance of the calling type, usually <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/this">this</a>.

`line` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The line of the call; filled in by the compiler.

#### Returns

 ProfilerMarker.AutoScope

The scope of the call site's marker; an empty scope when the call gets no marker.

#### Type Parameters

`T` 

The calling type.

#### Remarks

Only a call on an instance of the calling type gets a marker. Any other call gets no marker of its own,
and analyzer <code>AFT0010</code> reports it.

### WithName\(in AutoScope, string\) {#ProfilerMarkerExtensionsForGenerator_WithName_Unity_Profiling_ProfilerMarker_AutoScope__System_String_}

Renames the [`ProfilerMarker`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Unity.Profiling.ProfilerMarker.html) of the preceding [`ProfilerMarkerExtensionsForGenerator.Marker<T>`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_) call.

```csharp
public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name)
```

#### Parameters

`marker` ProfilerMarker.AutoScope

The scope returned by [`ProfilerMarkerExtensionsForGenerator.Marker<T>`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_).

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

Replaces the member part of the marker name. Must be a compile-time constant: a literal, a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/const">const</a>
string, <code>nameof(...)</code>, or their concatenation or interpolation; otherwise analyzer <code>AFT0014</code> reports it.

#### Returns

 ProfilerMarker.AutoScope

<code class="paramref">marker</code>, unchanged.

