---
title: "Class BaseFieldEnumExtensions"
sidebar_label: "BaseFieldEnumExtensions"
description: "Class BaseFieldEnumExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldEnumExtensions {#Aspid_FastTools_UIElements_BaseFieldEnumExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldEnumExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldEnumExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Enum`](https://learn.microsoft.com/dotnet/api/system.enum).

```csharp
public static class BaseFieldEnumExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldEnumExtensions](Aspid.FastTools.UIElements.BaseFieldEnumExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldEnumExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Enum>
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

