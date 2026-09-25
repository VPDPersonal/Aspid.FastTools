---
title: "Class FocusableExtensions"
sidebar_label: "FocusableExtensions"
description: "Class FocusableExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class FocusableExtensions {#Aspid_FastTools_UIElements_FocusableExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`Focusable`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable.html).

```csharp
public static class FocusableExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[FocusableExtensions](Aspid.FastTools.UIElements.FocusableExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### BlurSelf\<T\>\(T\) {#Aspid_FastTools_UIElements_FocusableExtensions_BlurSelf__1___0_}

Removes focus from the element via [`Blur`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-Blur.html).

```csharp
public static T BlurSelf<T>(this T element) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### FocusSelf\<T\>\(T\) {#Aspid_FastTools_UIElements_FocusableExtensions_FocusSelf__1___0_}

Gives focus to the element via [`Focus`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-Focus.html).

```csharp
public static T FocusSelf<T>(this T element) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### IsFocused\(Focusable\) {#Aspid_FastTools_UIElements_FocusableExtensions_IsFocused_UnityEngine_UIElements_Focusable_}

Returns whether the element currently has keyboard focus.

```csharp
public static bool IsFocused(this Focusable element)
```

#### Parameters

`element` Focusable

The element to check.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the element holds keyboard focus; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### SetDelegatesFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetDelegatesFocus__1___0_System_Boolean_}

Sets [`delegatesFocus`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-delegatesFocus.html).

```csharp
public static T SetDelegatesFocus<T>(this T element, bool value) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, focus is delegated to the children.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetFocusable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetFocusable__1___0_System_Boolean_}

Sets [`focusable`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-focusable.html).

```csharp
public static T SetFocusable<T>(this T element, bool value) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the element can receive focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetTabIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetTabIndex__1___0_System_Int32_}

Sets [`tabIndex`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-tabIndex.html).

```csharp
public static T SetTabIndex<T>(this T element, int value) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The tab index to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

