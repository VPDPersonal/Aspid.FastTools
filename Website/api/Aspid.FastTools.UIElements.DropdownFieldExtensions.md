---
title: "Class DropdownFieldExtensions"
sidebar_label: "DropdownFieldExtensions"
description: "Class DropdownFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class DropdownFieldExtensions {#Aspid_FastTools_UIElements_DropdownFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`DropdownField`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.DropdownField.html).

```csharp
public static class DropdownFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[DropdownFieldExtensions](Aspid.FastTools.UIElements.DropdownFieldExtensions.md)


## Remarks

Counterparts of [`PopupFieldExtensions`](Aspid.FastTools.UIElements.PopupFieldExtensions.md) that need no type arguments.
[`PopupFieldExtensions.SetChoices<T1, T2>`](Aspid.FastTools.UIElements.PopupFieldExtensions.md#Aspid_FastTools_UIElements_PopupFieldExtensions_SetChoices__2___0_System_Collections_Generic_List___1__) needs none either, so it has no counterpart.

## Methods

### SetFormatListItemCallback\<T\>\(T, Func\<string, string\>\) {#Aspid_FastTools_UIElements_DropdownFieldExtensions_SetFormatListItemCallback__1___0_System_Func_System_String_System_String__}

Sets [`formatListItemCallback`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-formatListItemCallback.html), replacing any existing callback.

```csharp
public static T SetFormatListItemCallback<T>(this T element, Func<string, string> value) where T : DropdownField
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

The callback that formats a choice shown in the popup list.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

### SetFormatSelectedValueCallback\<T\>\(T, Func\<string, string\>\) {#Aspid_FastTools_UIElements_DropdownFieldExtensions_SetFormatSelectedValueCallback__1___0_System_Func_System_String_System_String__}

Sets [`formatSelectedValueCallback`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-formatSelectedValueCallback.html), replacing any existing callback.

```csharp
public static T SetFormatSelectedValueCallback<T>(this T element, Func<string, string> value) where T : DropdownField
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

The callback that formats the selected value shown in the field.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

#### Remarks

Unity calls <code class="paramref">value</code> at once with the current value, which is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for a
[`DropdownField`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.DropdownField.html) before a choice is selected.

### SetIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_DropdownFieldExtensions_SetIndex__1___0_System_Int32_}

Sets [`index`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-index.html).

```csharp
public static T SetIndex<T>(this T element, int value) where T : DropdownField
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the selected choice to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

