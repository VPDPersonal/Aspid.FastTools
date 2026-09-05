---
title: "Class ManipulatorExtensions"
sidebar_label: "ManipulatorExtensions"
description: "Class ManipulatorExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ManipulatorExtensions {#Aspid_FastTools_UIElements_ManipulatorExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class ManipulatorExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ManipulatorExtensions](Aspid.FastTools.UIElements.ManipulatorExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddClickable\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_}

Adds a new [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler.

```csharp
public static T AddClickable<T>(this T element, Action handler) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke when the element is clicked.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClickable\<T\>\(T, Action, out Clickable\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_Clickable__}

Adds a new [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler and outputs the created manipulator.

```csharp
public static T AddClickable<T>(this T element, Action handler, out Clickable manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke when the element is clicked.

`manipulator` Clickable

The created [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClickable\<T\>\(T, Action\<EventBase\>\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_EventBase__}

Adds a new [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler with the triggering [`EventBase`](https://docs.unity3d.com/ScriptReference/UIElements-EventBase.html).

```csharp
public static T AddClickable<T>(this T element, Action<EventBase> handler) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<EventBase\>

The action to invoke when the element is clicked, receiving the triggering event.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClickable\<T\>\(T, Action\<EventBase\>, out Clickable\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_EventBase__UnityEngine_UIElements_Clickable__}

Adds a new [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler with the triggering [`EventBase`](https://docs.unity3d.com/ScriptReference/UIElements-EventBase.html) and outputs the created manipulator.

```csharp
public static T AddClickable<T>(this T element, Action<EventBase> handler, out Clickable manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<EventBase\>

The action to invoke when the element is clicked, receiving the triggering event.

`manipulator` Clickable

The created [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClickable\<T\>\(T, Action, long, long\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_System_Int64_System_Int64_}

Adds a new repeating [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler after an initial delay and then at a fixed interval while pressed.

```csharp
public static T AddClickable<T>(this T element, Action handler, long delay, long interval) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke on each click tick.

`delay` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The delay, in milliseconds, before the first repeated invocation.

`interval` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The interval, in milliseconds, between subsequent invocations while pressed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClickable\<T\>\(T, Action, long, long, out Clickable\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_System_Int64_System_Int64_UnityEngine_UIElements_Clickable__}

Adds a new repeating [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator that invokes the specified handler after an initial delay and then at a fixed interval while pressed, and outputs the created manipulator.

```csharp
public static T AddClickable<T>(this T element, Action handler, long delay, long interval, out Clickable manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`handler` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The action to invoke on each click tick.

`delay` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The delay, in milliseconds, before the first repeated invocation.

`interval` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The interval, in milliseconds, between subsequent invocations while pressed.

`manipulator` Clickable

The created [`Clickable`](https://docs.unity3d.com/ScriptReference/UIElements-Clickable.html) manipulator.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddContextualMenuManipulator\<T\>\(T, Action\<ContextualMenuPopulateEvent\>\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddContextualMenuManipulator__1___0_System_Action_UnityEngine_UIElements_ContextualMenuPopulateEvent__}

Adds a new [`ContextualMenuManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-ContextualMenuManipulator.html) that uses the specified menu builder to populate the contextual menu.

```csharp
public static T AddContextualMenuManipulator<T>(this T element, Action<ContextualMenuPopulateEvent> menuBuilder) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`menuBuilder` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<ContextualMenuPopulateEvent\>

The action invoked to populate the menu when it is shown.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddContextualMenuManipulator\<T\>\(T, Action\<ContextualMenuPopulateEvent\>, out ContextualMenuManipulator\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddContextualMenuManipulator__1___0_System_Action_UnityEngine_UIElements_ContextualMenuPopulateEvent__UnityEngine_UIElements_ContextualMenuManipulator__}

Adds a new [`ContextualMenuManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-ContextualMenuManipulator.html) that uses the specified menu builder to populate the contextual menu, and outputs the created manipulator.

```csharp
public static T AddContextualMenuManipulator<T>(this T element, Action<ContextualMenuPopulateEvent> menuBuilder, out ContextualMenuManipulator manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`menuBuilder` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<ContextualMenuPopulateEvent\>

The action invoked to populate the menu when it is shown.

`manipulator` ContextualMenuManipulator

The created [`ContextualMenuManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-ContextualMenuManipulator.html).

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddKeyboardNavigationManipulator\<T\>\(T, Action\<KeyboardNavigationOperation, EventBase\>\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddKeyboardNavigationManipulator__1___0_System_Action_UnityEngine_UIElements_KeyboardNavigationOperation_UnityEngine_UIElements_EventBase__}

Adds a new [`KeyboardNavigationManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-KeyboardNavigationManipulator.html) that invokes the specified action for keyboard navigation operations.

```csharp
public static T AddKeyboardNavigationManipulator<T>(this T element, Action<KeyboardNavigationOperation, EventBase> action) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<KeyboardNavigationOperation, EventBase\>

The action to invoke with the [`KeyboardNavigationOperation`](https://docs.unity3d.com/ScriptReference/UIElements-KeyboardNavigationOperation.html) and triggering [`EventBase`](https://docs.unity3d.com/ScriptReference/UIElements-EventBase.html).

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddKeyboardNavigationManipulator\<T\>\(T, Action\<KeyboardNavigationOperation, EventBase\>, out KeyboardNavigationManipulator\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddKeyboardNavigationManipulator__1___0_System_Action_UnityEngine_UIElements_KeyboardNavigationOperation_UnityEngine_UIElements_EventBase__UnityEngine_UIElements_KeyboardNavigationManipulator__}

Adds a new [`KeyboardNavigationManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-KeyboardNavigationManipulator.html) that invokes the specified action for keyboard navigation operations, and outputs the created manipulator.

```csharp
public static T AddKeyboardNavigationManipulator<T>(this T element, Action<KeyboardNavigationOperation, EventBase> action, out KeyboardNavigationManipulator manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<KeyboardNavigationOperation, EventBase\>

The action to invoke with the [`KeyboardNavigationOperation`](https://docs.unity3d.com/ScriptReference/UIElements-KeyboardNavigationOperation.html) and triggering [`EventBase`](https://docs.unity3d.com/ScriptReference/UIElements-EventBase.html).

`manipulator` KeyboardNavigationManipulator

The created [`KeyboardNavigationManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-KeyboardNavigationManipulator.html).

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddManipulatorSelf\<T\>\(T, IManipulator\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_AddManipulatorSelf__1___0_UnityEngine_UIElements_IManipulator_}

Adds an [`IManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-IManipulator.html) to the element and returns the element for chaining.

```csharp
public static T AddManipulatorSelf<T>(this T element, IManipulator manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`manipulator` IManipulator

The manipulator to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveManipulatorSelf\<T\>\(T, IManipulator\) {#Aspid_FastTools_UIElements_ManipulatorExtensions_RemoveManipulatorSelf__1___0_UnityEngine_UIElements_IManipulator_}

Removes an [`IManipulator`](https://docs.unity3d.com/ScriptReference/UIElements-IManipulator.html) from the element and returns the element for chaining.

```csharp
public static T RemoveManipulatorSelf<T>(this T element, IManipulator manipulator) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`manipulator` IManipulator

The manipulator to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

