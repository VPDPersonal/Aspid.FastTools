---
title: "Class PopupFieldExtensions"
sidebar_label: "PopupFieldExtensions"
description: "Class PopupFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class PopupFieldExtensions {#Aspid_FastTools_UIElements_PopupFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`PopupField<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1.html).

```csharp
public static class PopupFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[PopupFieldExtensions](Aspid.FastTools.UIElements.PopupFieldExtensions.md)


## Remarks

Only [`PopupFieldExtensions.SetChoices<T1, T2>`](Aspid.FastTools.UIElements.PopupFieldExtensions.md#Aspid_FastTools_UIElements_PopupFieldExtensions_SetChoices__2___0_System_Collections_Generic_List___1__) infers the choice type. The other methods need explicit type
arguments; [`DropdownField`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.DropdownField.html) has overloads without them in [`DropdownFieldExtensions`](Aspid.FastTools.UIElements.DropdownFieldExtensions.md).

## Methods

### SetChoices\<TField, TChoice\>\(TField, List\<TChoice\>\) {#Aspid_FastTools_UIElements_PopupFieldExtensions_SetChoices__2___0_System_Collections_Generic_List___1__}

Sets [`choices`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BasePopupField_2-choices.html).

```csharp
public static TField SetChoices<TField, TChoice>(this TField element, List<TChoice> value) where TField : PopupField<TChoice>
```

#### Parameters

`element` TField

The element to modify.

`value` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)\<TChoice\>

The list of choices to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TChoice` 

The choice type held by the field.

### SetFormatListItemCallback\<TField, TChoice\>\(TField, Func\<TChoice, string\>\) {#Aspid_FastTools_UIElements_PopupFieldExtensions_SetFormatListItemCallback__2___0_System_Func___1_System_String__}

Sets [`formatListItemCallback`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-formatListItemCallback.html), replacing any existing callback.

```csharp
public static TField SetFormatListItemCallback<TField, TChoice>(this TField element, Func<TChoice, string> value) where TField : PopupField<TChoice>
```

#### Parameters

`element` TField

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<TChoice, [string](https://learn.microsoft.com/dotnet/api/system.string)\>

The callback that formats a choice shown in the popup list.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TChoice` 

The choice type held by the field.

### SetFormatSelectedValueCallback\<TField, TChoice\>\(TField, Func\<TChoice, string\>\) {#Aspid_FastTools_UIElements_PopupFieldExtensions_SetFormatSelectedValueCallback__2___0_System_Func___1_System_String__}

Sets [`formatSelectedValueCallback`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-formatSelectedValueCallback.html), replacing any existing callback.

```csharp
public static TField SetFormatSelectedValueCallback<TField, TChoice>(this TField element, Func<TChoice, string> value) where TField : PopupField<TChoice>
```

#### Parameters

`element` TField

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<TChoice, [string](https://learn.microsoft.com/dotnet/api/system.string)\>

The callback that formats the selected value shown in the field.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TChoice` 

The choice type held by the field.

#### Remarks

Unity calls <code class="paramref">value</code> at once with the current value, which is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for a
reference type before a choice is selected.

### SetIndex\<TField, TChoice\>\(TField, int\) {#Aspid_FastTools_UIElements_PopupFieldExtensions_SetIndex__2___0_System_Int32_}

Sets [`index`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.PopupField_1-index.html).

```csharp
public static TField SetIndex<TField, TChoice>(this TField element, int value) where TField : PopupField<TChoice>
```

#### Parameters

`element` TField

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the selected choice to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TChoice` 

The choice type held by the field.

