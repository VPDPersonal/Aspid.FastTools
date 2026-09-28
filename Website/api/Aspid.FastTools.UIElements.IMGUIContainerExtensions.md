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

Provides extension methods for [`IMGUIContainer`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer.html).

```csharp
public static class IMGUIContainerExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IMGUIContainerExtensions](Aspid.FastTools.UIElements.IMGUIContainerExtensions.md)


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

The element type.

### MarkDirtyLayoutSelf\<T\>\(T\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_MarkDirtyLayoutSelf__1___0_}

Marks the IMGUI layout as dirty via [`MarkDirtyLayout`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-MarkDirtyLayout.html).

```csharp
public static T MarkDirtyLayoutSelf<T>(this T element) where T : IMGUIContainer
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

The element type.

### SetContextType\<T\>\(T, ContextType\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_SetContextType__1___0_UnityEngine_UIElements_ContextType_}

Sets [`contextType`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-contextType.html).

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

The element type.

#### Remarks

Only [`Editor`](https://docs.unity3d.com/ScriptReference/UIElements-ContextType-Editor.html) is currently supported by Unity.

### SetCullingEnabled\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_IMGUIContainerExtensions_SetCullingEnabled__1___0_System_Boolean_}

Sets [`cullingEnabled`](https://docs.unity3d.com/ScriptReference/UIElements-IMGUIContainer-cullingEnabled.html).

```csharp
public static T SetCullingEnabled<T>(this T element, bool value) where T : IMGUIContainer
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the handler is not called while the element is outside the viewport.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

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

The element type.

