---
title: "Class ImageExtensions"
sidebar_label: "ImageExtensions"
description: "Class ImageExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ImageExtensions {#Aspid_FastTools_UIElements_ImageExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`Image`](https://docs.unity3d.com/ScriptReference/UIElements-Image.html).

```csharp
public static class ImageExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ImageExtensions](Aspid.FastTools.UIElements.ImageExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetImage\<T\>\(T, Texture\) {#Aspid_FastTools_UIElements_ImageExtensions_SetImage__1___0_UnityEngine_Texture_}

Sets [`image`](https://docs.unity3d.com/ScriptReference/UIElements-Image-image.html).

```csharp
public static T SetImage<T>(this T element, Texture value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` Texture

The texture to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetImageFromResources\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ImageExtensions_SetImageFromResources__1___0_System_String_}

Loads a [`Texture`](https://docs.unity3d.com/ScriptReference/Texture.html) from Resources and sets the [`image`](https://docs.unity3d.com/ScriptReference/UIElements-Image-image.html) property.

```csharp
public static T SetImageFromResources<T>(this T element, string path) where T : Image
```

#### Parameters

`element` T

The element to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources path of the texture to load.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Logs a warning and leaves the element unchanged when no asset is found at <code class="paramref">path</code>.

### SetScaleMode\<T\>\(T, ScaleMode\) {#Aspid_FastTools_UIElements_ImageExtensions_SetScaleMode__1___0_UnityEngine_ScaleMode_}

Sets [`scaleMode`](https://docs.unity3d.com/ScriptReference/UIElements-Image-scaleMode.html).

```csharp
public static T SetScaleMode<T>(this T element, ScaleMode value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` ScaleMode

The scale mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSourceRect\<T\>\(T, Rect\) {#Aspid_FastTools_UIElements_ImageExtensions_SetSourceRect__1___0_UnityEngine_Rect_}

Sets [`sourceRect`](https://docs.unity3d.com/ScriptReference/UIElements-Image-sourceRect.html).

```csharp
public static T SetSourceRect<T>(this T element, Rect value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` Rect

The source rect to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSprite\<T\>\(T, Sprite\) {#Aspid_FastTools_UIElements_ImageExtensions_SetSprite__1___0_UnityEngine_Sprite_}

Sets [`sprite`](https://docs.unity3d.com/ScriptReference/UIElements-Image-sprite.html).

```csharp
public static T SetSprite<T>(this T element, Sprite value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` Sprite

The sprite to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSpriteFromResources\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ImageExtensions_SetSpriteFromResources__1___0_System_String_}

Loads a [`Sprite`](https://docs.unity3d.com/ScriptReference/Sprite.html) from Resources and sets the [`sprite`](https://docs.unity3d.com/ScriptReference/UIElements-Image-sprite.html) property.

```csharp
public static T SetSpriteFromResources<T>(this T element, string path) where T : Image
```

#### Parameters

`element` T

The element to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources path of the sprite to load.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Logs a warning and leaves the element unchanged when no asset is found at <code class="paramref">path</code>.

### SetTintColor\<T\>\(T, Color\) {#Aspid_FastTools_UIElements_ImageExtensions_SetTintColor__1___0_UnityEngine_Color_}

Sets [`tintColor`](https://docs.unity3d.com/ScriptReference/UIElements-Image-tintColor.html).

```csharp
public static T SetTintColor<T>(this T element, Color value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` Color

The tint color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetUv\<T\>\(T, Rect\) {#Aspid_FastTools_UIElements_ImageExtensions_SetUv__1___0_UnityEngine_Rect_}

Sets [`uv`](https://docs.unity3d.com/ScriptReference/UIElements-Image-uv.html).

```csharp
public static T SetUv<T>(this T element, Rect value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` Rect

The UV rect to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetVectorImage\<T\>\(T, VectorImage\) {#Aspid_FastTools_UIElements_ImageExtensions_SetVectorImage__1___0_UnityEngine_UIElements_VectorImage_}

Sets [`vectorImage`](https://docs.unity3d.com/ScriptReference/UIElements-Image-vectorImage.html).

```csharp
public static T SetVectorImage<T>(this T element, VectorImage value) where T : Image
```

#### Parameters

`element` T

The element to modify.

`value` VectorImage

The vector image to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetVectorImageFromResources\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ImageExtensions_SetVectorImageFromResources__1___0_System_String_}

Loads a [`VectorImage`](https://docs.unity3d.com/ScriptReference/UIElements-VectorImage.html) from Resources and sets the [`vectorImage`](https://docs.unity3d.com/ScriptReference/UIElements-Image-vectorImage.html) property.

```csharp
public static T SetVectorImageFromResources<T>(this T element, string path) where T : Image
```

#### Parameters

`element` T

The element to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources path of the vector image to load.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Logs a warning and leaves the element unchanged when no asset is found at <code class="paramref">path</code>.

