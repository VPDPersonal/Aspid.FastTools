---
title: "Class BaseFieldRectExtensions"
sidebar_label: "BaseFieldRectExtensions"
description: "Class BaseFieldRectExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldRectExtensions {#Aspid_FastTools_UIElements_BaseFieldRectExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldRectExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldRectExtensions.md#Aspid_FastTools_UIElements_BaseFieldRectExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Rect`](https://docs.unity3d.com/ScriptReference/Rect.html).

```csharp
public static class BaseFieldRectExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldRectExtensions](Aspid.FastTools.UIElements.BaseFieldRectExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldRectExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Rect>
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

