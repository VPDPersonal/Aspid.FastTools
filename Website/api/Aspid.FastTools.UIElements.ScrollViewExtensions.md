---
title: "Class ScrollViewExtensions"
sidebar_label: "ScrollViewExtensions"
description: "Class ScrollViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ScrollViewExtensions {#Aspid_FastTools_UIElements_ScrollViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`ScrollView`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView.html).

```csharp
public static class ScrollViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ScrollViewExtensions](Aspid.FastTools.UIElements.ScrollViewExtensions.md)


## Methods

### SetElasticAnimationIntervalMs\<T\>\(T, long\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetElasticAnimationIntervalMs__1___0_System_Int64_}

Sets [`elasticAnimationIntervalMs`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-elasticAnimationIntervalMs.html).

```csharp
public static T SetElasticAnimationIntervalMs<T>(this T element, long value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The minimum interval between elastic and inertia animation updates in milliseconds.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetElasticity\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetElasticity__1___0_System_Single_}

Sets [`elasticity`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-elasticity.html).

```csharp
public static T SetElasticity<T>(this T element, float value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The elasticity of the bounce at the content edges, clamped to 0 or more.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHorizontalPageSize\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetHorizontalPageSize__1___0_System_Single_}

Sets [`horizontalPageSize`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-horizontalPageSize.html).

```csharp
public static T SetHorizontalPageSize<T>(this T element, float value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The horizontal page size in pixels; <code>-1</code> derives it from the scroller.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHorizontalScrollerVisibility\<T\>\(T, ScrollerVisibility\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetHorizontalScrollerVisibility__1___0_UnityEngine_UIElements_ScrollerVisibility_}

Sets [`horizontalScrollerVisibility`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-horizontalScrollerVisibility.html).

```csharp
public static T SetHorizontalScrollerVisibility<T>(this T element, ScrollerVisibility value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` ScrollerVisibility

The visibility of the horizontal scroller to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMode\<T\>\(T, ScrollViewMode\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetMode__1___0_UnityEngine_UIElements_ScrollViewMode_}

Sets [`mode`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-mode.html).

```csharp
public static T SetMode<T>(this T element, ScrollViewMode value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` ScrollViewMode

The scrolling directions to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMouseWheelScrollSize\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetMouseWheelScrollSize__1___0_System_Single_}

Sets [`mouseWheelScrollSize`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-mouseWheelScrollSize.html).

```csharp
public static T SetMouseWheelScrollSize<T>(this T element, float value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The distance in pixels that one mouse wheel step scrolls.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetNestedInteractionKind\<T\>\(T, NestedInteractionKind\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetNestedInteractionKind__1___0_UnityEngine_UIElements_ScrollView_NestedInteractionKind_}

Sets [`nestedInteractionKind`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-nestedInteractionKind.html).

```csharp
public static T SetNestedInteractionKind<T>(this T element, ScrollView.NestedInteractionKind value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` ScrollView.NestedInteractionKind

How scrolling passes to a parent scroll view when this one reaches its limit.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetScrollDecelerationRate\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetScrollDecelerationRate__1___0_System_Single_}

Sets [`scrollDecelerationRate`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-scrollDecelerationRate.html).

```csharp
public static T SetScrollDecelerationRate<T>(this T element, float value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The deceleration rate of inertial scrolling, clamped to 0 or more.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetScrollOffset\<T\>\(T, Vector2\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetScrollOffset__1___0_UnityEngine_Vector2_}

Sets [`scrollOffset`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-scrollOffset.html).

```csharp
public static T SetScrollOffset<T>(this T element, Vector2 value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` Vector2

The scroll offset in pixels.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

The scrollers limit the offset to the scrollable range, so add the content and wait for the layout first.

### SetTouchScrollBehavior\<T\>\(T, TouchScrollBehavior\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetTouchScrollBehavior__1___0_UnityEngine_UIElements_ScrollView_TouchScrollBehavior_}

Sets [`touchScrollBehavior`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-touchScrollBehavior.html).

```csharp
public static T SetTouchScrollBehavior<T>(this T element, ScrollView.TouchScrollBehavior value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` ScrollView.TouchScrollBehavior

How touch scrolling behaves at the content edges.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetVerticalPageSize\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetVerticalPageSize__1___0_System_Single_}

Sets [`verticalPageSize`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-verticalPageSize.html).

```csharp
public static T SetVerticalPageSize<T>(this T element, float value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The vertical page size in pixels; <code>-1</code> derives it from the scroller.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetVerticalScrollerVisibility\<T\>\(T, ScrollerVisibility\) {#Aspid_FastTools_UIElements_ScrollViewExtensions_SetVerticalScrollerVisibility__1___0_UnityEngine_UIElements_ScrollerVisibility_}

Sets [`verticalScrollerVisibility`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ScrollView-verticalScrollerVisibility.html).

```csharp
public static T SetVerticalScrollerVisibility<T>(this T element, ScrollerVisibility value) where T : ScrollView
```

#### Parameters

`element` T

The element to modify.

`value` ScrollerVisibility

The visibility of the vertical scroller to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

