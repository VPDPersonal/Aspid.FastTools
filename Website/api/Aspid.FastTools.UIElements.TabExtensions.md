---
title: "Class TabExtensions"
sidebar_label: "TabExtensions"
description: "Class TabExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TabExtensions {#Aspid_FastTools_UIElements_TabExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`Tab`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab.html).

```csharp
public static class TabExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TabExtensions](Aspid.FastTools.UIElements.TabExtensions.md)


## Methods

### AddClosed\<T\>\(T, Action\<Tab\>\) {#Aspid_FastTools_UIElements_TabExtensions_AddClosed__1___0_System_Action_UnityEngine_UIElements_Tab__}

Subscribes to the [`closed`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-closed.html) event.

```csharp
public static T AddClosed<T>(this T element, Action<Tab> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<Tab\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddClosing\<T\>\(T, Func\<bool\>\) {#Aspid_FastTools_UIElements_TabExtensions_AddClosing__1___0_System_Func_System_Boolean__}

Subscribes to the [`closing`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-closing.html) event.

```csharp
public static T AddClosing<T>(this T element, Func<bool> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The callback to subscribe; it returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> to cancel the closing.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

When several callbacks are subscribed, Unity runs all of them and uses only the result of the last one.

### AddSelected\<T\>\(T, Action\<Tab\>\) {#Aspid_FastTools_UIElements_TabExtensions_AddSelected__1___0_System_Action_UnityEngine_UIElements_Tab__}

Subscribes to the [`selected`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-selected.html) event.

```csharp
public static T AddSelected<T>(this T element, Action<Tab> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<Tab\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveClosed\<T\>\(T, Action\<Tab\>\) {#Aspid_FastTools_UIElements_TabExtensions_RemoveClosed__1___0_System_Action_UnityEngine_UIElements_Tab__}

Unsubscribes from the [`closed`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-closed.html) event.

```csharp
public static T RemoveClosed<T>(this T element, Action<Tab> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<Tab\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveClosing\<T\>\(T, Func\<bool\>\) {#Aspid_FastTools_UIElements_TabExtensions_RemoveClosing__1___0_System_Func_System_Boolean__}

Unsubscribes from the [`closing`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-closing.html) event.

```csharp
public static T RemoveClosing<T>(this T element, Func<bool> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveSelected\<T\>\(T, Action\<Tab\>\) {#Aspid_FastTools_UIElements_TabExtensions_RemoveSelected__1___0_System_Action_UnityEngine_UIElements_Tab__}

Unsubscribes from the [`selected`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-selected.html) event.

```csharp
public static T RemoveSelected<T>(this T element, Action<Tab> value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<Tab\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetCloseable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TabExtensions_SetCloseable__1___0_System_Boolean_}

Sets [`closeable`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-closeable.html).

```csharp
public static T SetCloseable<T>(this T element, bool value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the header shows a close button.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetIconImage\<T\>\(T, Background\) {#Aspid_FastTools_UIElements_TabExtensions_SetIconImage__1___0_UnityEngine_UIElements_Background_}

Sets [`iconImage`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-iconImage.html).

```csharp
public static T SetIconImage<T>(this T element, Background value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` Background

The header icon to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_TabExtensions_SetLabel__1___0_System_String_}

Sets [`label`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.Tab-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : Tab
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The header text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

