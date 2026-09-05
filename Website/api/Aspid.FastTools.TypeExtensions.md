---
title: "Class TypeExtensions"
sidebar_label: "TypeExtensions"
description: "Class TypeExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeExtensions {#Aspid_FastTools_TypeExtensions}

Namespace: [Aspid.FastTools](Aspid.FastTools.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class TypeExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TypeExtensions](Aspid.FastTools.TypeExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### GetMembersInfosIncludingBaseClasses\(Type, BindingFlags, Type?\) {#Aspid_FastTools_TypeExtensions_GetMembersInfosIncludingBaseClasses_System_Type_System_Reflection_BindingFlags_System_Type_}

Returns the members of <code class="paramref">type</code> and its base classes in declaration order (base → derived),
matching the Unity inspector's traversal order.

```csharp
public static IReadOnlyList<MemberInfo> GetMembersInfosIncludingBaseClasses(this Type type, BindingFlags bindingFlags, Type? stopAt = null)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type to inspect.

`bindingFlags` [BindingFlags](https://learn.microsoft.com/dotnet/api/system.reflection.bindingflags)

The binding flags used to filter members. [`DeclaredOnly`](https://learn.microsoft.com/dotnet/api/system.reflection.bindingflags.declaredonly) is forced on internally to avoid duplicate members from base classes.

`stopAt` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

Optional ancestor type at which to stop walking the chain. When <code>null</code>, walks all the way to the root type.

#### Returns

 [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[MemberInfo](https://learn.microsoft.com/dotnet/api/system.reflection.memberinfo)\>

A flat list of [`MemberInfo`](https://learn.microsoft.com/dotnet/api/system.reflection.memberinfo) instances ordered from the topmost base class down to <code class="paramref">type</code>.

