---
title: "Class CallbackEventHandlerExtensions"
sidebar_label: "CallbackEventHandlerExtensions"
description: "Class CallbackEventHandlerExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class CallbackEventHandlerExtensions {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`CallbackEventHandler`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.html).

```csharp
public static class CallbackEventHandlerExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[CallbackEventHandlerExtensions](Aspid.FastTools.UIElements.CallbackEventHandlerExtensions.md)


## Methods

### RegisterCallbackOnceSelf\<T, TEventType\>\(T, EventCallback\<TEventType\>, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_RegisterCallbackOnceSelf__2___0_UnityEngine_UIElements_EventCallback___1__UnityEngine_UIElements_TrickleDown_}

Registers a callback that runs for one event and is then removed, via [`RegisterCallbackOnce<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.RegisterCallbackOnce.html).

```csharp
public static T RegisterCallbackOnceSelf<T, TEventType>(this T element, EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType\>

The callback to invoke when the first event reaches the element.

`useTrickleDown` TrickleDown

Selects the phase of the callback: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type to listen for.

#### Remarks

Name both type arguments unless the callback has a typed parameter: <code>RegisterCallbackOnceSelf&lt;Button, ClickEvent&gt;(OnClick)</code>.

### RegisterCallbackOnceSelf\<T, TEventType, TUserArgsType\>\(T, EventCallback\<TEventType, TUserArgsType\>, TUserArgsType, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_RegisterCallbackOnceSelf__3___0_UnityEngine_UIElements_EventCallback___1___2____2_UnityEngine_UIElements_TrickleDown_}

Registers a callback with user arguments that runs for one event and is then removed, via [`RegisterCallbackOnce<T1, T2>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.RegisterCallbackOnce.html).

```csharp
public static T RegisterCallbackOnceSelf<T, TEventType, TUserArgsType>(this T element, EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType, TUserArgsType\>

The callback to invoke when the first event reaches the element.

`userArgs` TUserArgsType

The value passed to the callback.

`useTrickleDown` TrickleDown

Selects the phase of the callback: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type to listen for.

`TUserArgsType` 

The type of the user arguments.

#### Remarks

Name all type arguments unless the callback has typed parameters: <code>RegisterCallbackOnceSelf&lt;Button, ClickEvent, int&gt;(OnClick, 5)</code>.

### RegisterCallbackSelf\<T, TEventType\>\(T, EventCallback\<TEventType\>, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_RegisterCallbackSelf__2___0_UnityEngine_UIElements_EventCallback___1__UnityEngine_UIElements_TrickleDown_}

Registers a callback for an event via [`RegisterCallback<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.RegisterCallback.html).

```csharp
public static T RegisterCallbackSelf<T, TEventType>(this T element, EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType\>

The callback to invoke when the event reaches the element.

`useTrickleDown` TrickleDown

Selects the phase of the callback: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type to listen for.

#### Remarks

Name both type arguments unless the callback has a typed parameter: <code>RegisterCallbackSelf&lt;Button, ClickEvent&gt;(OnClick)</code>.

### RegisterCallbackSelf\<T, TEventType, TUserArgsType\>\(T, EventCallback\<TEventType, TUserArgsType\>, TUserArgsType, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_RegisterCallbackSelf__3___0_UnityEngine_UIElements_EventCallback___1___2____2_UnityEngine_UIElements_TrickleDown_}

Registers a callback with user arguments for an event via [`RegisterCallback<T1, T2>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.RegisterCallback.html).

```csharp
public static T RegisterCallbackSelf<T, TEventType, TUserArgsType>(this T element, EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType, TUserArgsType\>

The callback to invoke when the event reaches the element.

`userArgs` TUserArgsType

The value passed to the callback with every event.

`useTrickleDown` TrickleDown

Selects the phase of the callback: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type to listen for.

`TUserArgsType` 

The type of the user arguments.

#### Remarks

Name all type arguments unless the callback has typed parameters: <code>RegisterCallbackSelf&lt;Button, ClickEvent, int&gt;(OnClick, 5)</code>.

### UnregisterCallbackSelf\<T, TEventType\>\(T, EventCallback\<TEventType\>, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_UnregisterCallbackSelf__2___0_UnityEngine_UIElements_EventCallback___1__UnityEngine_UIElements_TrickleDown_}

Removes a callback for an event via [`UnregisterCallback<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.UnregisterCallback.html).

```csharp
public static T UnregisterCallbackSelf<T, TEventType>(this T element, EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType\>

The callback to remove.

`useTrickleDown` TrickleDown

The phase the callback was registered for: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type the callback listens for.

#### Remarks

Name both type arguments unless the callback has a typed parameter: <code>UnregisterCallbackSelf&lt;Button, ClickEvent&gt;(OnClick)</code>.

### UnregisterCallbackSelf\<T, TEventType, TUserArgsType\>\(T, EventCallback\<TEventType, TUserArgsType\>, TrickleDown\) {#Aspid_FastTools_UIElements_CallbackEventHandlerExtensions_UnregisterCallbackSelf__3___0_UnityEngine_UIElements_EventCallback___1___2__UnityEngine_UIElements_TrickleDown_}

Removes a callback with user arguments for an event via [`UnregisterCallback<T1, T2>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.CallbackEventHandler.UnregisterCallback.html).

```csharp
public static T UnregisterCallbackSelf<T, TEventType, TUserArgsType>(this T element, EventCallback<TEventType, TUserArgsType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where T : CallbackEventHandler where TEventType : EventBase<TEventType>, new()
```

#### Parameters

`element` T

The element to modify.

`callback` EventCallback\<TEventType, TUserArgsType\>

The callback to remove.

`useTrickleDown` TrickleDown

The phase the callback was registered for: [`TrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.TrickleDown.html) for trickle-down, [`NoTrickleDown`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TrickleDown.NoTrickleDown.html) for bubble-up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TEventType` 

The event type the callback listens for.

`TUserArgsType` 

The type of the user arguments.

#### Remarks

Name all type arguments unless the callback has typed parameters: <code>UnregisterCallbackSelf&lt;Button, ClickEvent, int&gt;(OnClick)</code>.

