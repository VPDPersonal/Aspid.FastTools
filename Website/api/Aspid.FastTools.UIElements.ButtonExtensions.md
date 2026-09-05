---
title: "Class ButtonExtensions"
sidebar_label: "ButtonExtensions"
description: "Class ButtonExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ButtonExtensions {#Aspid_FastTools_UIElements_ButtonExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class ButtonExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ButtonExtensions](Aspid.FastTools.UIElements.ButtonExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddClicked\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ButtonExtensions_AddClicked__1___0_System_Action_}

Subscribes to the [`clicked`](https://docs.unity3d.com/ScriptReference/UIElements-Button-clicked.html) event.

```csharp
public static T AddClicked<T>(this T element, Action action) where T : Button
```

#### Parameters

`element` T

The element to modify.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke when the button is clicked.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveClicked\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ButtonExtensions_RemoveClicked__1___0_System_Action_}

Unsubscribes from the [`clicked`](https://docs.unity3d.com/ScriptReference/UIElements-Button-clicked.html) event.

```csharp
public static T RemoveClicked<T>(this T element, Action action) where T : Button
```

#### Parameters

`element` T

The element to modify.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetClickable\<T\>\(T, Clickable\) {#Aspid_FastTools_UIElements_ButtonExtensions_SetClickable__1___0_UnityEngine_UIElements_Clickable_}

Sets [`clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Button-clickable.html) and returns the element for chaining.

```csharp
public static T SetClickable<T>(this T element, Clickable value) where T : Button
```

#### Parameters

`element` T

The element to modify.

`value` Clickable

The clickable manipulator to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Clickable MouseManipulator for this Button.

### SetClickable\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ButtonExtensions_SetClickable__1___0_System_Action_}

Sets the click handler of the button by replacing the clickable manipulator.

```csharp
public static T SetClickable<T>(this T element, Action action) where T : Button
```

#### Parameters

`element` T

The element to modify.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke on click.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetIconImage\<T\>\(T, Background\) {#Aspid_FastTools_UIElements_ButtonExtensions_SetIconImage__1___0_UnityEngine_UIElements_Background_}

Sets [`iconImage`](https://docs.unity3d.com/ScriptReference/UIElements-Button-iconImage.html) and returns the element for chaining.

```csharp
public static T SetIconImage<T>(this T element, Background value) where T : Button
```

#### Parameters

`element` T

The element to modify.

`value` Background

The icon image to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The Texture, Sprite, or VectorImage that will represent an icon within a Button element.

