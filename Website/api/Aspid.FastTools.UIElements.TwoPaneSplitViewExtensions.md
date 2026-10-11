---
title: "Class TwoPaneSplitViewExtensions"
sidebar_label: "TwoPaneSplitViewExtensions"
description: "Class TwoPaneSplitViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TwoPaneSplitViewExtensions {#Aspid_FastTools_UIElements_TwoPaneSplitViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TwoPaneSplitView`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TwoPaneSplitView.html).

```csharp
public static class TwoPaneSplitViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TwoPaneSplitViewExtensions](Aspid.FastTools.UIElements.TwoPaneSplitViewExtensions.md)


## Methods

### SetFixedPaneIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TwoPaneSplitViewExtensions_SetFixedPaneIndex__1___0_System_Int32_}

Sets [`fixedPaneIndex`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TwoPaneSplitView-fixedPaneIndex.html).

```csharp
public static T SetFixedPaneIndex<T>(this T element, int value) where T : TwoPaneSplitView
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the fixed pane: <code>0</code> or <code>1</code>.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetFixedPaneInitialDimension\<T\>\(T, float\) {#Aspid_FastTools_UIElements_TwoPaneSplitViewExtensions_SetFixedPaneInitialDimension__1___0_System_Single_}

Sets [`fixedPaneInitialDimension`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TwoPaneSplitView-fixedPaneInitialDimension.html).

```csharp
public static T SetFixedPaneInitialDimension<T>(this T element, float value) where T : TwoPaneSplitView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The initial width or height of the fixed pane in pixels.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Unity uses the value only when it has no saved view data for the split view.

### SetOrientation\<T\>\(T, TwoPaneSplitViewOrientation\) {#Aspid_FastTools_UIElements_TwoPaneSplitViewExtensions_SetOrientation__1___0_UnityEngine_UIElements_TwoPaneSplitViewOrientation_}

Sets [`orientation`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TwoPaneSplitView-orientation.html).

```csharp
public static T SetOrientation<T>(this T element, TwoPaneSplitViewOrientation value) where T : TwoPaneSplitView
```

#### Parameters

`element` T

The element to modify.

`value` TwoPaneSplitViewOrientation

The orientation of the split to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

