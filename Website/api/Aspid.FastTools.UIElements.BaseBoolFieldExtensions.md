---
title: "Class BaseBoolFieldExtensions"
sidebar_label: "BaseBoolFieldExtensions"
description: "Class BaseBoolFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseBoolFieldExtensions {#Aspid_FastTools_UIElements_BaseBoolFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class BaseBoolFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseBoolFieldExtensions](Aspid.FastTools.UIElements.BaseBoolFieldExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseBoolFieldExtensions_SetLabel__1___0_System_String_}

Sets the [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html) property and returns the element for chaining.

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseBoolField
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The label text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetText\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseBoolFieldExtensions_SetText__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-BaseBoolField-text.html) and returns the element for chaining.

```csharp
public static T SetText<T>(this T element, string value) where T : BaseBoolField
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

Optional text that appears after the BaseBoolField.

### SetToggleOnLabelClick\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseBoolFieldExtensions_SetToggleOnLabelClick__1___0_System_Boolean_}

Sets [`toggleOnLabelClick`](https://docs.unity3d.com/ScriptReference/UIElements-BaseBoolField-toggleOnLabelClick.html) and returns the element for chaining.

```csharp
public static T SetToggleOnLabelClick<T>(this T element, bool value) where T : BaseBoolField
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether clicking the label activates the toggle.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Whether to activate the toggle when the user clicks the label.

