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

Provides extension methods for [`Button`](https://docs.unity3d.com/ScriptReference/UIElements-Button.html).

```csharp
public static class ButtonExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ButtonExtensions](Aspid.FastTools.UIElements.ButtonExtensions.md)


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

The element type.

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

The element type.

### SetClickable\<T\>\(T, Clickable\) {#Aspid_FastTools_UIElements_ButtonExtensions_SetClickable__1___0_UnityEngine_UIElements_Clickable_}

Sets [`clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Button-clickable.html).

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

### SetClickable\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ButtonExtensions_SetClickable__1___0_System_Action_}

Replaces [`clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Button-clickable.html) with a new [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) that invokes <code class="paramref">action</code>.

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

Sets [`iconImage`](https://docs.unity3d.com/ScriptReference/UIElements-Button-iconImage.html).

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

The element type.

