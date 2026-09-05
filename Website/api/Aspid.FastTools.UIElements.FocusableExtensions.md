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

```csharp
public static class FocusableExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[FocusableExtensions](Aspid.FastTools.UIElements.FocusableExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### IsFocus\(Focusable\) {#Aspid_FastTools_UIElements_FocusableExtensions_IsFocus_UnityEngine_UIElements_Focusable_}

Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if this element currently has keyboard focus.

```csharp
public static bool IsFocus(this Focusable element)
```

#### Parameters

`element` Focusable

The element to check.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the element holds keyboard focus; otherwise <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### SetBlur\<T\>\(T\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetBlur__1___0_}

Tells the element to release the focus and returns the element for chaining.

```csharp
public static T SetBlur<T>(this T element) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetDelegatesFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetDelegatesFocus__1___0_System_Boolean_}

Sets [`delegatesFocus`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-delegatesFocus.html) and returns the element for chaining.

```csharp
public static T SetDelegatesFocus<T>(this T focusable, bool value) where T : Focusable
```

#### Parameters

`focusable` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether focus is delegated to children.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Whether the element delegates the focus to its children.

### SetFocus\<T\>\(T\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetFocus__1___0_}

Attempts to give the focus to this element and returns the element for chaining.

```csharp
public static T SetFocus<T>(this T element) where T : Focusable
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetFocusable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetFocusable__1___0_System_Boolean_}

Sets [`focusable`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-focusable.html) and returns the element for chaining.

```csharp
public static T SetFocusable<T>(this T focusable, bool value) where T : Focusable
```

#### Parameters

`focusable` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether this element can receive focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Whether an element can potentially receive focus.

### SetTabIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_FocusableExtensions_SetTabIndex__1___0_System_Int32_}

Sets [`tabIndex`](https://docs.unity3d.com/ScriptReference/UIElements-Focusable-tabIndex.html) and returns the element for chaining.

```csharp
public static T SetTabIndex<T>(this T focusable, int value) where T : Focusable
```

#### Parameters

`focusable` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The tab index to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

An integer used to sort focusable elements in the focus ring. Must be greater than or equal to zero.

