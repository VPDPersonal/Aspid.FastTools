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


## Methods

### Marker\<T\>\(T, int\) {#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_}

Opens the [`ProfilerMarker`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Unity.Profiling.ProfilerMarker.html) of this call site, named <code>Type.Member (line)</code>.

```csharp
public static ProfilerMarker.AutoScope Marker<T>(this T instance, int line = -1)
```

#### Parameters

`instance` T

The instance the scope is opened on; its value is never read.

`line` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The line of the call, filled in by the compiler; the value is never read.

#### Returns

 ProfilerMarker.AutoScope

An empty scope: this overload runs only when the call gets no marker.

#### Type Parameters

`T` 

The type of <code class="paramref">instance</code>; generic, so a struct is not boxed.

#### Remarks

<p>
For every type that calls this method on its own instance the generator emits a closer overload that
overload resolution picks instead, holding one [`ProfilerMarker`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Unity.Profiling.ProfilerMarker.html) per line of that type.
This overload runs only for calls the generator cannot support; analyzer <code>AFT0010</code> reports them.
It takes the same parameters as the generated overload, so the generated one also wins for a type
in the global namespace; that overload takes a struct by <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/in">in</a>, so the call does not copy it.
</p>
<p>
The class has no namespace, so the method needs no <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/using">using</a> directive and code completion
offers it after any expression; only a call on an instance of the calling type gets a marker.
</p>

### WithName\(in AutoScope, string\) {#ProfilerMarkerExtensionsForGenerator_WithName_Unity_Profiling_ProfilerMarker_AutoScope__System_String_}

Names the [`ProfilerMarker`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Unity.Profiling.ProfilerMarker.html) that the generator creates for the preceding [`ProfilerMarkerExtensionsForGenerator.Marker<T>`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_) call.

```csharp
public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name)
```

#### Parameters

`marker` ProfilerMarker.AutoScope

The scope returned by [`ProfilerMarkerExtensionsForGenerator.Marker<T>`](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_).

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

The text replacing the member part of the marker name. Read from the source at compile time, so it must be
a constant: a string literal, a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/const">const</a> string, <code>nameof(...)</code>, or a concatenation or
interpolation of them. Anything else leaves the name untouched, and analyzer <code>AFT0014</code> reports it.

#### Returns

 ProfilerMarker.AutoScope

<code class="paramref">marker</code> unchanged — at runtime the call is a pass-through.

