---
title: "Class BaseFieldBoundsExtensions"
sidebar_label: "BaseFieldBoundsExtensions"
description: "Class BaseFieldBoundsExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldBoundsExtensions {#Aspid_FastTools_UIElements_BaseFieldBoundsExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldBoundsExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldBoundsExtensions.md#Aspid_FastTools_UIElements_BaseFieldBoundsExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Bounds`](https://docs.unity3d.com/ScriptReference/Bounds.html).

```csharp
public static class BaseFieldBoundsExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldBoundsExtensions](Aspid.FastTools.UIElements.BaseFieldBoundsExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldBoundsExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Bounds>
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

The field type.

