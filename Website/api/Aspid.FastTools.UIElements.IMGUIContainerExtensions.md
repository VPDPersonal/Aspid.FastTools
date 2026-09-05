---
title: "Class IMGUIContainerExtensions"
sidebar_label: "IMGUIContainerExtensions"
description: "Class IMGUIContainerExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IMGUIContainerExtensions {#Aspid_FastTools_UIElements_IMGUIContainerExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class IMGUIContainerExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IMGUIContainerExtensions](Aspid.FastTools.UIElements.IMGUIContainerExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddOnGUIHandler\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_AddOnGUIHandler__1___0_System_Action_}

Subscribes to the [`onGUIHandler`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-onGUIHandler.html) callback.

```csharp
public static T AddOnGUIHandler<T>(this T element, Action value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The handler to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### MarkDirtyLayout\<T\>\(T\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_MarkDirtyLayout__1___0_}

Marks the [`IMGUIContainer`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer.html) layout as dirty, forcing a relayout of its IMGUI content, and returns the element for chaining.

```csharp
public static T MarkDirtyLayout<T>(this T element) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveOnGUIHandler\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_RemoveOnGUIHandler__1___0_System_Action_}

Unsubscribes from the [`onGUIHandler`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-onGUIHandler.html) callback.

```csharp
public static T RemoveOnGUIHandler<T>(this T element, Action value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The handler to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetContextType\<T\>\(T, ContextType\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_SetContextType__1___0_UnityEngine_UIElements_ContextType_}

Sets [`contextType`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-contextType.html) and returns the element for chaining.

```csharp
public static T SetContextType<T>(this T element, ContextType value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` ContextType

The context type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

ContextType of this IMGUIContainer. Currently only supports ContextType.Editor.

### SetCullingEnabled\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_SetCullingEnabled__1___0_System_Boolean_}

Sets [`cullingEnabled`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-cullingEnabled.html) and returns the element for chaining.

```csharp
public static T SetCullingEnabled<T>(this T element, bool value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether culling is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When this property is set to true, onGUIHandler is not called when the Element is outside the viewport.

### SetOnGUIHandler\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_SetOnGUIHandler__1___0_System_Action_}

Sets the [`onGUIHandler`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-onGUIHandler.html) callback, replacing any existing handler.

```csharp
public static T SetOnGUIHandler<T>(this T element, Action value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The handler to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The function that's called to render and handle IMGUI events.

