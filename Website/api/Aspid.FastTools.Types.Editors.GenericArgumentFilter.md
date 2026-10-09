---
title: "Delegate GenericArgumentFilter"
sidebar_label: "GenericArgumentFilter"
description: "Delegate GenericArgumentFilter — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Delegate GenericArgumentFilter {#Aspid_FastTools_Types_Editors_GenericArgumentFilter}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Represents the method that decides whether <code class="paramref">argument</code> may close
<code class="paramref">parameter</code>.

```csharp
public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument)
```

#### Parameters

`openDefinition` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The generic definition being closed.

`parameter` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type parameter being closed.

`argument` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The concrete type proposed for it.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">argument</code> may close <code class="paramref">parameter</code>; otherwise,
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<GenericArgumentFilter\>\(GenericArgumentFilter, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)
