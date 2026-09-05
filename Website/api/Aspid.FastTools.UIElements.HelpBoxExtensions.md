---
title: "Class HelpBoxExtensions"
sidebar_label: "HelpBoxExtensions"
description: "Class HelpBoxExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class HelpBoxExtensions {#Aspid_FastTools_UIElements_HelpBoxExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class HelpBoxExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[HelpBoxExtensions](Aspid.FastTools.UIElements.HelpBoxExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetMessageType\<T\>\(T, HelpBoxMessageType\) {#Aspid_FastTools_UIElements_HelpBoxExtensions_SetMessageType__1___0_UnityEngine_UIElements_HelpBoxMessageType_}

Sets [`messageType`](https://docs.unity3d.com/ScriptReference/UIElements-HelpBox-messageType.html) and returns the element for chaining.

```csharp
public static T SetMessageType<T>(this T element, HelpBoxMessageType value) where T : HelpBox
```

#### Parameters

`element` T

The element to modify.

`value` HelpBoxMessageType

The message type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The type of message.

### SetText\<T\>\(T, string\) {#Aspid_FastTools_UIElements_HelpBoxExtensions_SetText__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-HelpBox-text.html) and returns the element for chaining.

```csharp
public static T SetText<T>(this T element, string value) where T : HelpBox
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The message text.

