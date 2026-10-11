---
title: "Class BaseFieldGuidExtensions"
sidebar_label: "BaseFieldGuidExtensions"
description: "Class BaseFieldGuidExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldGuidExtensions {#Aspid_FastTools_UIElements_BaseFieldGuidExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldGuidExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldGuidExtensions.md#Aspid_FastTools_UIElements_BaseFieldGuidExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BaseField_1.html) of [`GUID`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/GUID.html).

```csharp
public static class BaseFieldGuidExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldGuidExtensions](Aspid.FastTools.UIElements.BaseFieldGuidExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldGuidExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BaseField_1-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<GUID>
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

