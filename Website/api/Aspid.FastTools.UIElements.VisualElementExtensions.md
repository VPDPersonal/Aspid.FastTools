---
title: "Class VisualElementExtensions"
sidebar_label: "VisualElementExtensions"
description: "Class VisualElementExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class VisualElementExtensions {#Aspid_FastTools_UIElements_VisualElementExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class VisualElementExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[VisualElementExtensions](Aspid.FastTools.UIElements.VisualElementExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddBoldUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddBoldUnityFontStyleAndWeight__1___0_}

Adds bold to [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any existing italic style.

```csharp
public static T AddBoldUnityFontStyleAndWeight<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html) → [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html),
[`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html) → [`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html).
Other values are left unchanged.

### AddChild\<T\>\(T, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChild__1___0_UnityEngine_UIElements_VisualElement_}

Adds an element to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChild<T>(this T element, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`child` VisualElement

The child element to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildIf\<T\>\(T, bool, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement_}

Conditionally adds an element to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildIf<T>(this T element, bool condition, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`child` VisualElement

The child element to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildren\<T\>\(T, Span\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Span_UnityEngine_UIElements_VisualElement__}

Adds a span of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildren<T>(this T element, Span<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`children` [Span](https://learn.microsoft.com/dotnet/api/system.span-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildren\<T\>\(T, List\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__}

Adds a list of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildren<T>(this T element, List<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`children` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildren\<T\>\(T, params VisualElement\[\]\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_UnityEngine_UIElements_VisualElement___}

Adds an array of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildren<T>(this T element, params VisualElement[] children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`children` VisualElement\[\]

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildren\<T\>\(T, IEnumerable\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__}

Adds an enumerable of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildren<T>(this T element, IEnumerable<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`children` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildren\<T\>\(T, ReadOnlySpan\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__}

Adds a read-only span of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildren<T>(this T element, ReadOnlySpan<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`children` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildrenIf\<T\>\(T, bool, Span\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Span_UnityEngine_UIElements_VisualElement__}

Conditionally adds a span of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildrenIf<T>(this T element, bool condition, Span<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`children` [Span](https://learn.microsoft.com/dotnet/api/system.span-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildrenIf\<T\>\(T, bool, List\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__}

Conditionally adds a list of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildrenIf<T>(this T element, bool condition, List<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`children` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildrenIf\<T\>\(T, bool, params VisualElement\[\]\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement___}

Conditionally adds an array of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildrenIf<T>(this T element, bool condition, params VisualElement[] children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`children` VisualElement\[\]

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildrenIf\<T\>\(T, bool, IEnumerable\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__}

Conditionally adds an enumerable of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildrenIf<T>(this T element, bool condition, IEnumerable<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`children` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddChildrenIf\<T\>\(T, bool, ReadOnlySpan\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__}

Conditionally adds a read-only span of child elements to the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T AddChildrenIf<T>(this T element, bool condition, ReadOnlySpan<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`children` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<VisualElement\>

The children to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddClass\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddClass__1___0_System_String_}

Adds a class to the class list of the element in order to assign styles from USS. Note the class name is case-sensitive.

```csharp
public static T AddClass<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The USS class name to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddItalicUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddItalicUnityFontStyleAndWeight__1___0_}

Adds italic to [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any existing bold style.

```csharp
public static T AddItalicUnityFontStyleAndWeight<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html) → [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html),
[`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html) → [`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html).
Other values are left unchanged.

### AddStyleSheets\<T\>\(T, StyleSheet\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddStyleSheets__1___0_UnityEngine_UIElements_StyleSheet_}

Adds a USS style sheet to the element's style sheet list.

```csharp
public static T AddStyleSheets<T>(this T element, StyleSheet value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleSheet

The style sheet to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddStyleSheetsFromResource\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_AddStyleSheetsFromResource__1___0_System_String_}

Loads and adds a USS style sheet from a Resources path.

```csharp
public static T AddStyleSheetsFromResource<T>(this T element, string path) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources-relative path to the style sheet asset.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### ClearChildren\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_ClearChildren__1___0_}

Removes all children from the element and returns the element for chaining.

```csharp
public static T ClearChildren<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### ClearChildrenIf\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_VisualElementExtensions_ClearChildrenIf__1___0_System_Boolean_}

Conditionally removes all children from the element and returns the element for chaining.

```csharp
public static T ClearChildrenIf<T>(this T element, bool condition) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### ClearClasses\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_ClearClasses__1___0_}

Removes all classes from the class list of this element.

```csharp
public static T ClearClasses<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### EnableInClass\<T\>\(T, string, bool\) {#Aspid_FastTools_UIElements_VisualElementExtensions_EnableInClass__1___0_System_String_System_Boolean_}

Enables or disables the class with the given name.

```csharp
public static T EnableInClass<T>(this T element, string className, bool enable) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`className` [string](https://learn.microsoft.com/dotnet/api/system.string)

The USS class name to enable or disable.

`enable` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to enable or disable the class.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChild\<T\>\(T, int, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChild__1___0_System_Int32_UnityEngine_UIElements_VisualElement_}

Inserts a child element at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChild<T>(this T element, int index, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to insert the child.

`child` VisualElement

The child element to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildIf\<T\>\(T, bool, int, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildIf__1___0_System_Boolean_System_Int32_UnityEngine_UIElements_VisualElement_}

Conditionally inserts a child element at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildIf<T>(this T element, bool condition, int index, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to insert the child.

`child` VisualElement

The child element to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildren\<T\>\(T, int, Span\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Span_UnityEngine_UIElements_VisualElement__}

Inserts a span of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildren<T>(this T element, int index, Span<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [Span](https://learn.microsoft.com/dotnet/api/system.span-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildren\<T\>\(T, int, List\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__}

Inserts a list of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildren<T>(this T element, int index, List<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildren\<T\>\(T, int, params VisualElement\[\]\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_UnityEngine_UIElements_VisualElement___}

Inserts an array of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildren<T>(this T element, int index, params VisualElement[] children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` VisualElement\[\]

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildren\<T\>\(T, int, IEnumerable\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__}

Inserts an enumerable of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildren<T>(this T element, int index, IEnumerable<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildren\<T\>\(T, int, ReadOnlySpan\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__}

Inserts a read-only span of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildren<T>(this T element, int index, ReadOnlySpan<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildrenIf\<T\>\(T, bool, int, Span\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Span_UnityEngine_UIElements_VisualElement__}

Conditionally inserts a span of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildrenIf<T>(this T element, bool condition, int index, Span<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [Span](https://learn.microsoft.com/dotnet/api/system.span-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildrenIf\<T\>\(T, bool, int, List\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__}

Conditionally inserts a list of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildrenIf<T>(this T element, bool condition, int index, List<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildrenIf\<T\>\(T, bool, int, params VisualElement\[\]\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_UnityEngine_UIElements_VisualElement___}

Conditionally inserts an array of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildrenIf<T>(this T element, bool condition, int index, params VisualElement[] children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` VisualElement\[\]

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildrenIf\<T\>\(T, bool, int, IEnumerable\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__}

Conditionally inserts an enumerable of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildrenIf<T>(this T element, bool condition, int index, IEnumerable<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### InsertChildrenIf\<T\>\(T, bool, int, ReadOnlySpan\<VisualElement\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__}

Conditionally inserts a read-only span of child elements starting at the specified index in the [`contentContainer`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-contentContainer.html) of this element and returns the element for chaining.

```csharp
public static T InsertChildrenIf<T>(this T element, bool condition, int index, ReadOnlySpan<VisualElement> children) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index at which to start inserting children.

`children` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<VisualElement\>

The children to insert.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveBoldUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveBoldUnityFontStyleAndWeight__1___0_}

Removes bold from [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any existing italic style.

```csharp
public static T RemoveBoldUnityFontStyleAndWeight<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html) → [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html),
[`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html) → [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html).
Other values are left unchanged.

### RemoveChild\<T\>\(T, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChild__1___0_UnityEngine_UIElements_VisualElement_}

Removes the specified child from the element and returns the element for chaining.

```csharp
public static T RemoveChild<T>(this T element, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`child` VisualElement

The child element to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveChildAt\<T\>\(T, int\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildAt__1___0_System_Int32_}

Removes the child at the specified index from the element and returns the element for chaining.

```csharp
public static T RemoveChildAt<T>(this T element, int index) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the child to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveChildAtIf\<T\>\(T, bool, int\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildAtIf__1___0_System_Boolean_System_Int32_}

Conditionally removes the child at the specified index from the element and returns the element for chaining.

```csharp
public static T RemoveChildAtIf<T>(this T element, bool condition, int index) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`index` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the child to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveChildIf\<T\>\(T, bool, VisualElement\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement_}

Conditionally removes the specified child from the element and returns the element for chaining.

```csharp
public static T RemoveChildIf<T>(this T element, bool condition, VisualElement child) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If true, performs the operation; otherwise skips it and returns the element unchanged.

`child` VisualElement

The child element to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveClass\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveClass__1___0_System_String_}

Removes a class from the class list of the element.

```csharp
public static T RemoveClass<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The USS class name to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveItalicUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveItalicUnityFontStyleAndWeight__1___0_}

Removes italic from [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any existing bold style.

```csharp
public static T RemoveItalicUnityFontStyleAndWeight<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html) → [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html),
[`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html) → [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html).
Other values are left unchanged.

### RemoveStyleSheets\<T\>\(T, StyleSheet\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveStyleSheets__1___0_UnityEngine_UIElements_StyleSheet_}

Removes a style sheet for the owner element.

```csharp
public static T RemoveStyleSheets<T>(this T element, StyleSheet value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleSheet

The style sheet to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveStyleSheetsFromResource\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveStyleSheetsFromResource__1___0_System_String_}

Loads and removes a USS style sheet identified by its Resources path.

```csharp
public static T RemoveStyleSheetsFromResource<T>(this T element, string path) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources-relative path to the style sheet asset.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetAlignContent\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignContent.html) and returns the element for chaining.

```csharp
public static T SetAlignContent<T>(this T element, StyleEnum<Align> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Align\>

The content alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of the whole area of children on the cross axis if they span over multiple lines in this container.

### SetAlignContent\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignContent__1___0_UnityEngine_UIElements_Align_}

Sets [`alignContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignContent.html) and returns the element for chaining.

```csharp
public static T SetAlignContent<T>(this T element, Align value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Align

The content alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of the whole area of children on the cross axis if they span over multiple lines in this container.

### SetAlignItems\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignItems__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignItems`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignItems.html) and returns the element for chaining.

```csharp
public static T SetAlignItems<T>(this T element, StyleEnum<Align> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Align\>

The children alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of children on the cross axis of this container.

### SetAlignItems\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignItems__1___0_UnityEngine_UIElements_Align_}

Sets [`alignItems`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignItems.html) and returns the element for chaining.

```csharp
public static T SetAlignItems<T>(this T element, Align value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Align

The children alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of children on the cross axis of this container.

### SetAlignSelf\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignSelf`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignSelf.html) and returns the element for chaining.

```csharp
public static T SetAlignSelf<T>(this T element, StyleEnum<Align> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Align\>

The alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Similar to align-items, but only for this specific element.

### SetAlignSelf\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_Align_}

Sets [`alignSelf`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignSelf.html) and returns the element for chaining.

```csharp
public static T SetAlignSelf<T>(this T element, Align value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Align

The alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Similar to align-items, but only for this specific element.

### SetAspectRatio\<T\>\(T, StyleRatio\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetAspectRatio__1___0_UnityEngine_UIElements_StyleRatio_}

Sets [`aspectRatio`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-aspectRatio.html) and returns the element for chaining.

```csharp
public static T SetAspectRatio<T>(this T element, StyleRatio value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleRatio

The aspect ratio to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Sets a preferred aspect ratio for the box, which will be used in the calculation of auto sizes and some other layout functions.

### SetBackgroundColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`backgroundColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundColor.html) and returns the element for chaining.

```csharp
public static T SetBackgroundColor<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The background color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background color to paint in the element's box.

### SetBackgroundColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundColor__1___0_System_String_}

Parses an HTML color string and sets [`backgroundColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundColor.html), returning the element for chaining.

```csharp
public static T SetBackgroundColor<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string to parse (e.g. "#RRGGBB" or a named color).

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background color to paint in the element's box.

### SetBackgroundImage\<T\>\(T, StyleBackground\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundImage__1___0_UnityEngine_UIElements_StyleBackground_}

Sets [`backgroundImage`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundImage.html) and returns the element for chaining.

```csharp
public static T SetBackgroundImage<T>(this T element, StyleBackground value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackground

The background image to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image to paint in the element's box.

### SetBackgroundImageFromResource\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundImageFromResource__1___0_System_String_}

Loads a [`Texture2D`](https://docs.unity3d.com/ScriptReference/Texture2D.html) from Resources and sets the [`backgroundImage`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundImage.html) property.

```csharp
public static T SetBackgroundImageFromResource<T>(this T element, string path) where T : VisualElement
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

### SetBackgroundPosition\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPosition__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html), [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the element for chaining.

```csharp
public static T SetBackgroundPosition<T>(this T element, StyleBackgroundPosition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackgroundPosition

The background position to apply to both axes.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>
<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundPosition\<T\>\(T, StyleBackgroundPosition?, StyleBackgroundPosition?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPosition__1___0_System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html), [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the element for chaining.

```csharp
public static T SetBackgroundPosition<T>(this T element, StyleBackgroundPosition? x = null, StyleBackgroundPosition? y = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`x` StyleBackgroundPosition?

The horizontal background position, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`y` StyleBackgroundPosition?

The vertical background position, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>
<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundPositionX\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPositionX__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html) and returns the element for chaining.

```csharp
public static T SetBackgroundPositionX<T>(this T element, StyleBackgroundPosition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackgroundPosition

The horizontal background position to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>

### SetBackgroundPositionY\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPositionY__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the element for chaining.

```csharp
public static T SetBackgroundPositionY<T>(this T element, StyleBackgroundPosition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackgroundPosition

The vertical background position to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundRepeat\<T\>\(T, StyleBackgroundRepeat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundRepeat__1___0_UnityEngine_UIElements_StyleBackgroundRepeat_}

Sets [`backgroundRepeat`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundRepeat.html) and returns the element for chaining.

```csharp
public static T SetBackgroundRepeat<T>(this T element, StyleBackgroundRepeat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackgroundRepeat

The background repeat mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image repeat value.

### SetBackgroundSize\<T\>\(T, StyleBackgroundSize\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundSize__1___0_UnityEngine_UIElements_StyleBackgroundSize_}

Sets [`backgroundSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundSize.html) and returns the element for chaining.

```csharp
public static T SetBackgroundSize<T>(this T element, StyleBackgroundSize value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleBackgroundSize

The background size to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image size value. Transitions are fully supported only when using size in pixels or percentages, such as pixel-to-pixel or percentage-to-percentage transitions.

### SetBorderColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html), [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html),
[`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColor<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The border color to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_System_String_}

Sets the border color on all sides by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColor<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColor\<T\>\(T, StyleColor?, StyleColor?, StyleColor?, StyleColor?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html), [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html),
[`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColor<T>(this T element, StyleColor? top = null, StyleColor? right = null, StyleColor? bottom = null, StyleColor? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleColor?

The top border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleColor?

The right border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleColor?

The bottom border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleColor?

The left border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorBottom\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorBottom__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorBottom<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The bottom border color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>

### SetBorderColorBottom\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorBottom__1___0_System_String_}

Sets the bottom border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorBottom<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColorLeft\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorLeft__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorLeft<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The left border color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorLeft\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorLeft__1___0_System_String_}

Sets the left border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorLeft<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColorRight\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorRight__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorRight<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The right border color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightColor</code> –– Color of the element's right border.</p>

### SetBorderColorRight\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorRight__1___0_System_String_}

Sets the right border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorRight<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColorTop\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorTop__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorTop<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The top border color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>

### SetBorderColorTop\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorTop__1___0_System_String_}

Sets the top border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorTop<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColorX\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorX__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorX<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The border color to apply to the left and right sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorX\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorX__1___0_System_String_}

Sets the left and right border colors by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorX<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderColorY\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorY__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html) and [`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html) and returns the element for chaining.

```csharp
public static T SetBorderColorY<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The border color to apply to the top and bottom sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>

### SetBorderColorY\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorY__1___0_System_String_}

Sets the top and bottom border colors by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorY<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBorderRadius\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadius__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html),
[`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadius<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The border radius to apply to all corners.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadius\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadius__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html),
[`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadius<T>(this T element, StyleLength? topLeft = null, StyleLength? topRight = null, StyleLength? bottomRight = null, StyleLength? bottomLeft = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`topLeft` StyleLength?

The top-left radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`topRight` StyleLength?

The top-right radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottomRight` StyleLength?

The bottom-right radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottomLeft` StyleLength?

The bottom-left radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusBottom<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The radius to apply to both bottom corners.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottomLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottomLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusBottomLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The bottom-left corner radius to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottomRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottomRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusBottomRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The bottom-right corner radius to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The radius to apply to both left corners.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html), [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The radius to apply to both right corners.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusTop<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The radius to apply to both top corners.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTopLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTopLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusTopLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The top-left corner radius to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTopRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTopRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html) and returns the element for chaining.

```csharp
public static T SetBorderRadiusTopRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The top-right corner radius to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderWidth\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidth__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html), [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html),
[`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html), [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidth<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The border width to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidth\<T\>\(T, StyleFloat?, StyleFloat?, StyleFloat?, StyleFloat?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidth__1___0_System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html), [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html),
[`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html), [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidth<T>(this T element, StyleFloat? top = null, StyleFloat? right = null, StyleFloat? bottom = null, StyleFloat? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleFloat?

The top border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleFloat?

The right border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleFloat?

The bottom border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleFloat?

The left border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthBottom\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthBottom__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthBottom<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The bottom border width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>

### SetBorderWidthLeft\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthLeft__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthLeft<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The left border width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthRight\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthRight__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthRight<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The right border width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>

### SetBorderWidthTop\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthTop__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthTop<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The top border width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>

### SetBorderWidthX\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthX__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthX<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The border width to apply to the left and right sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthY\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthY__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html) and [`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html) and returns the element for chaining.

```csharp
public static T SetBorderWidthY<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The border width to apply to the top and bottom sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>

### SetBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html) and returns the element for chaining.

```csharp
public static T SetBottom<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The bottom offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>

### SetColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`color`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-color.html) and returns the element for chaining.

```csharp
public static T SetColor<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The text color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Color to use when drawing the text of an element.

### SetColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetColor__1___0_System_String_}

Parses an HTML color string and sets [`color`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-color.html), returning the element for chaining.

```csharp
public static T SetColor<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string to parse (e.g. "#RRGGBB" or a named color).

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Color to use when drawing the text of an element.

### SetCursor\<T\>\(T, StyleCursor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetCursor__1___0_UnityEngine_UIElements_StyleCursor_}

Sets [`cursor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-cursor.html) and returns the element for chaining.

```csharp
public static T SetCursor<T>(this T element, StyleCursor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleCursor

The cursor style to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Mouse cursor to display when the mouse pointer is over an element.

### SetDataSource\<T\>\(T, object\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSource__1___0_System_Object_}

Sets [`dataSource`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-dataSource.html) and returns the element for chaining.

```csharp
public static T SetDataSource<T>(this T element, object value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

The data source to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Assigns a data source to this VisualElement which overrides any inherited data source. This data source is inherited by all children.

### SetDataSourcePath\<T\>\(T, PropertyPath\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSourcePath__1___0_Unity_Properties_PropertyPath_}

Sets [`dataSourcePath`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-dataSourcePath.html) and returns the element for chaining.

```csharp
public static T SetDataSourcePath<T>(this T element, PropertyPath value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` PropertyPath

The data source path to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Path from the data source to the value.

### SetDataSourceType\<T\>\(T, Type\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSourceType__1___0_System_Type_}

Sets [`dataSourceType`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-dataSourceType.html) and returns the element for chaining.

```csharp
public static T SetDataSourceType<T>(this T element, Type value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The data source type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The possible type of data source assignable to this VisualElement.
This information is only used by the UI Builder as a hint to provide some completion to the data source path field when the effective data source cannot be specified at design time.

### SetDisablePlayModeTint\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisablePlayModeTint__1___0_System_Boolean_}

Sets [`disablePlayModeTint`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-disablePlayModeTint.html) and returns the element for chaining.

```csharp
public static T SetDisablePlayModeTint<T>(this T element, bool value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to disable the play-mode tint.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Play-mode tint is applied by default unless this is set to true. It's applied hierarchically to this VisualElement and to all its children that exist on an editor panel.

### SetDisplay\<T\>\(T, StyleEnum\<DisplayStyle\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisplay__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_DisplayStyle__}

Sets [`display`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-display.html) and returns the element for chaining.

```csharp
public static T SetDisplay<T>(this T element, StyleEnum<DisplayStyle> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<DisplayStyle\>

The display mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Defines how an element is displayed in the layout.

### SetDisplay\<T\>\(T, DisplayStyle\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisplay__1___0_UnityEngine_UIElements_DisplayStyle_}

Sets [`display`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-display.html) and returns the element for chaining.

```csharp
public static T SetDisplay<T>(this T element, DisplayStyle value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` DisplayStyle

The display mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Defines how an element is displayed in the layout.

### SetDistance\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistance__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html),
[`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the element for chaining.

```csharp
public static T SetDistance<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The distance to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistance\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistance__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html),
[`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the element for chaining.

```csharp
public static T SetDistance<T>(this T element, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleLength?

The top offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistanceX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistanceX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the element for chaining.

```csharp
public static T SetDistanceX<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The horizontal offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistanceY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistanceY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html) and returns the element for chaining.

```csharp
public static T SetDistanceY<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The vertical offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>

### SetEnabledSelf\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetEnabledSelf__1___0_System_Boolean_}

Changes the VisualElement enabled state and returns the element for chaining.

```csharp
public static T SetEnabledSelf<T>(this T element, bool value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the element is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

A disabled visual element does not receive most events.

### SetFilter\<T\>\(T, StyleList\<FilterFunction\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFilter__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_FilterFunction__}

Sets [`filter`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-filter.html) and returns the element for chaining.

```csharp
public static T SetFilter<T>(this T element, StyleList<FilterFunction> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleList\<FilterFunction\>

The filter effects to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Filter effects to apply to the element.

### SetFlexBasis\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexBasis__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`flexBasis`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexBasis.html) and returns the element for chaining.

```csharp
public static T SetFlexBasis<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The flex basis to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Initial main size of a flex item, on the main flex axis. The final layout might be smaller or larger, according to the flex shrinking and growing determined by the other flex properties.

### SetFlexDirection\<T\>\(T, StyleEnum\<FlexDirection\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_FlexDirection__}

Sets [`flexDirection`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexDirection.html) and returns the element for chaining.

```csharp
public static T SetFlexDirection<T>(this T element, StyleEnum<FlexDirection> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<FlexDirection\>

The flex direction to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Direction of the main axis to layout children in a container.

### SetFlexDirection\<T\>\(T, FlexDirection\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_FlexDirection_}

Sets [`flexDirection`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexDirection.html) and returns the element for chaining.

```csharp
public static T SetFlexDirection<T>(this T element, FlexDirection value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` FlexDirection

The flex direction to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Direction of the main axis to layout children in a container.

### SetFlexGrow\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexGrow__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`flexGrow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexGrow.html) and returns the element for chaining.

```csharp
public static T SetFlexGrow<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The flex grow factor to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies how the item will grow relative to the rest of the flexible items inside the same container.

### SetFlexShrink\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexShrink__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`flexShrink`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexShrink.html) and returns the element for chaining.

```csharp
public static T SetFlexShrink<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The flex shrink factor to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies how the item will shrink relative to the rest of the flexible items inside the same container.

### SetFlexWrap\<T\>\(T, StyleEnum\<Wrap\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Wrap__}

Sets [`flexWrap`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexWrap.html) and returns the element for chaining.

```csharp
public static T SetFlexWrap<T>(this T element, StyleEnum<Wrap> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Wrap\>

The flex wrap mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Placement of children over multiple lines if not enough space is available in this container.

### SetFlexWrap\<T\>\(T, Wrap\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_Wrap_}

Sets [`flexWrap`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexWrap.html) and returns the element for chaining.

```csharp
public static T SetFlexWrap<T>(this T element, Wrap value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Wrap

The flex wrap mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Placement of children over multiple lines if not enough space is available in this container.

### SetFontSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetFontSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`fontSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-fontSize.html) and returns the element for chaining.

```csharp
public static T SetFontSize<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The font size to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font size to draw the element's text, specified in point size.

### SetHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the element for chaining.

```csharp
public static T SetHeight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The height to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetJustifyContent\<T\>\(T, StyleEnum\<Justify\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Justify__}

Sets [`justifyContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-justifyContent.html) and returns the element for chaining.

```csharp
public static T SetJustifyContent<T>(this T element, StyleEnum<Justify> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Justify\>

The justify content mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Justification of children on the main axis of this container.

### SetJustifyContent\<T\>\(T, Justify\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_Justify_}

Sets [`justifyContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-justifyContent.html) and returns the element for chaining.

```csharp
public static T SetJustifyContent<T>(this T element, Justify value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Justify

The justify content mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Justification of children on the main axis of this container.

### SetLanguageDirection\<T\>\(T, LanguageDirection\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetLanguageDirection__1___0_UnityEngine_UIElements_LanguageDirection_}

Sets [`languageDirection`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-languageDirection.html) and returns the element for chaining.

```csharp
public static T SetLanguageDirection<T>(this T element, LanguageDirection value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` LanguageDirection

The language direction to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Indicates the directionality of the element's text. The value will propagate to the element's children.

### SetLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the element for chaining.

```csharp
public static T SetLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The left offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetLetterSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetLetterSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`letterSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-letterSpacing.html) and returns the element for chaining.

```csharp
public static T SetLetterSpacing<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The letter spacing to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between characters.

### SetMargin\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMargin__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html),
[`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the element for chaining.

```csharp
public static T SetMargin<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The margin to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMargin\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMargin__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html),
[`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the element for chaining.

```csharp
public static T SetMargin<T>(this T element, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleLength?

The top margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html) and returns the element for chaining.

```csharp
public static T SetMarginBottom<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The bottom margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>

### SetMarginLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the element for chaining.

```csharp
public static T SetMarginLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The left margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html) and returns the element for chaining.

```csharp
public static T SetMarginRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The right margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>

### SetMarginTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html) and returns the element for chaining.

```csharp
public static T SetMarginTop<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The top margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>

### SetMarginX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the element for chaining.

```csharp
public static T SetMarginX<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The horizontal margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html) and returns the element for chaining.

```csharp
public static T SetMarginY<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The vertical margin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>

### SetMaxHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the element for chaining.

```csharp
public static T SetMaxHeight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The maximum height to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html), [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the element for chaining.

```csharp
public static T SetMaxSize<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The maximum size to apply to both width and height.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>
<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html), [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the element for chaining.

```csharp
public static T SetMaxSize<T>(this T element, StyleLength? maxWidth = null, StyleLength? maxHeight = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`maxWidth` StyleLength?

The maximum width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`maxHeight` StyleLength?

The maximum height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>
<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html) and returns the element for chaining.

```csharp
public static T SetMaxWidth<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The maximum width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>

### SetMinHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the element for chaining.

```csharp
public static T SetMinHeight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The minimum height to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html), [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the element for chaining.

```csharp
public static T SetMinSize<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The minimum size to apply to both width and height.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>
<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html), [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the element for chaining.

```csharp
public static T SetMinSize<T>(this T element, StyleLength? minWidth = null, StyleLength? minHeight = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`minWidth` StyleLength?

The minimum width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`minHeight` StyleLength?

The minimum height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>
<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html) and returns the element for chaining.

```csharp
public static T SetMinWidth<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The minimum width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>

### SetName\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetName__1___0_System_String_}

Sets [`name`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-name.html) and returns the element for chaining.

```csharp
public static T SetName<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The name to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The name of this VisualElement.

### SetNormalUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetNormalUnityFontStyleAndWeight__1___0_}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) to [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html), removing bold and italic.

```csharp
public static T SetNormalUnityFontStyleAndWeight<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Sets the value unconditionally, regardless of any current bold or italic style.

### SetOpacity\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetOpacity__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`opacity`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-opacity.html) and returns the element for chaining.

```csharp
public static T SetOpacity<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The opacity to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the transparency of an element and of its children.

### SetOverflow\<T\>\(T, StyleEnum\<Overflow\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Overflow__}

Sets [`overflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-overflow.html) and returns the element for chaining.

```csharp
public static T SetOverflow<T>(this T element, StyleEnum<Overflow> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Overflow\>

The overflow behavior to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

How a container behaves if its content overflows its own box.

### SetOverflow\<T\>\(T, Overflow\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetOverflow__1___0_UnityEngine_UIElements_Overflow_}

Sets [`overflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-overflow.html) and returns the element for chaining.

```csharp
public static T SetOverflow<T>(this T element, Overflow value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Overflow

The overflow behavior to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

How a container behaves if its content overflows its own box.

### SetPadding\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPadding__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html),
[`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the element for chaining.

```csharp
public static T SetPadding<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The padding to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPadding\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPadding__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html),
[`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the element for chaining.

```csharp
public static T SetPadding<T>(this T element, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleLength?

The top padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html) and returns the element for chaining.

```csharp
public static T SetPaddingBottom<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The bottom padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>

### SetPaddingLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the element for chaining.

```csharp
public static T SetPaddingLeft<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The left padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html) and returns the element for chaining.

```csharp
public static T SetPaddingRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The right padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>

### SetPaddingTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html) and returns the element for chaining.

```csharp
public static T SetPaddingTop<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The top padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>

### SetPaddingX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the element for chaining.

```csharp
public static T SetPaddingX<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The horizontal padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html)  and returns the element for chaining.

```csharp
public static T SetPaddingY<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The vertical padding to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>

### SetPickingMode\<T\>\(T, PickingMode\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPickingMode__1___0_UnityEngine_UIElements_PickingMode_}

Sets [`pickingMode`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-pickingMode.html) and returns the element for chaining.

```csharp
public static T SetPickingMode<T>(this T element, PickingMode value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` PickingMode

The picking mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Determines if this element can be the target of pointer events or picked by IPanel.Pick queries.

### SetPosition\<T\>\(T, StyleEnum\<Position\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Position__}

Sets [`position`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-position.html) and returns the element for chaining.

```csharp
public static T SetPosition<T>(this T element, StyleEnum<Position> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Position\>

The position type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Element's positioning in its parent container.

### SetPosition\<T\>\(T, Position\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetPosition__1___0_UnityEngine_UIElements_Position_}

Sets [`position`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-position.html) and returns the element for chaining.

```csharp
public static T SetPosition<T>(this T element, Position value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Position

The position type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Element's positioning in its parent container.

### SetRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html) and returns the element for chaining.

```csharp
public static T SetRight<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The right offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>right</code> –– Right distance from the element's box during layout.</p>

### SetRotate\<T\>\(T, StyleRotate\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetRotate__1___0_UnityEngine_UIElements_StyleRotate_}

Sets [`rotate`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-rotate.html) and returns the element for chaining.

```csharp
public static T SetRotate<T>(this T element, StyleRotate value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleRotate

The rotation to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

A rotation transformation.

### SetScale\<T\>\(T, StyleScale\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetScale__1___0_UnityEngine_UIElements_StyleScale_}

Sets [`scale`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-scale.html) and returns the element for chaining.

```csharp
public static T SetScale<T>(this T element, StyleScale value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleScale

The scale transformation to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

A scaling transformation.

### SetSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html), [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the element for chaining.

```csharp
public static T SetSize<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The size to apply to both width and height.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>
<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html), [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the element for chaining.

```csharp
public static T SetSize<T>(this T element, StyleLength? width = null, StyleLength? height = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`width` StyleLength?

The width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`height` StyleLength?

The height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>
<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetTextOverflow\<T\>\(T, StyleEnum\<TextOverflow\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflow__}

Sets [`textOverflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textOverflow.html) and returns the element for chaining.

```csharp
public static T SetTextOverflow<T>(this T element, StyleEnum<TextOverflow> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<TextOverflow\>

The text overflow mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow mode.

### SetTextOverflow\<T\>\(T, TextOverflow\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_TextOverflow_}

Sets [`textOverflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textOverflow.html) and returns the element for chaining.

```csharp
public static T SetTextOverflow<T>(this T element, TextOverflow value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` TextOverflow

The text overflow mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow mode.

### SetTextShadow\<T\>\(T, StyleTextShadow\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextShadow__1___0_UnityEngine_UIElements_StyleTextShadow_}

Sets [`textShadow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textShadow.html) and returns the element for chaining.

```csharp
public static T SetTextShadow<T>(this T element, StyleTextShadow value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleTextShadow

The text shadow to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Drop shadow of the text.

### SetTooltip\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTooltip__1___0_System_String_}

Sets [`tooltip`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-tooltip.html) and returns the element for chaining.

```csharp
public static T SetTooltip<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The tooltip text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Text to display inside an information box after the user hovers the element for a small amount of time. This is only supported in the Editor UI.

### SetTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html) and returns the element for chaining.

```csharp
public static T SetTop<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The top offset to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>

### SetTransformOrigin\<T\>\(T, StyleTransformOrigin\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransformOrigin__1___0_UnityEngine_UIElements_StyleTransformOrigin_}

Sets [`transformOrigin`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transformOrigin.html) and returns the element for chaining.

```csharp
public static T SetTransformOrigin<T>(this T element, StyleTransformOrigin value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleTransformOrigin

The transform origin to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The transformation origin is the point around which a transformation is applied.

### SetTransitionDelay\<T\>\(T, StyleList\<TimeValue\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionDelay__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__}

Sets [`transitionDelay`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionDelay.html) and returns the element for chaining.

```csharp
public static T SetTransitionDelay<T>(this T element, StyleList<TimeValue> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleList\<TimeValue\>

The transition delays to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Duration to wait before starting a property's transition effect when its value changes.

### SetTransitionDuration\<T\>\(T, StyleList\<TimeValue\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionDuration__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__}

Sets [`transitionDuration`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionDuration.html) and returns the element for chaining.

```csharp
public static T SetTransitionDuration<T>(this T element, StyleList<TimeValue> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleList\<TimeValue\>

The transition durations to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Time a transition animation should take to complete.

### SetTransitionProperty\<T\>\(T, StyleList\<StylePropertyName\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionProperty__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_StylePropertyName__}

Sets [`transitionProperty`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionProperty.html) and returns the element for chaining.

```csharp
public static T SetTransitionProperty<T>(this T element, StyleList<StylePropertyName> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleList\<StylePropertyName\>

The transition properties to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Properties to which a transition effect should be applied.

### SetTransitionTimingFunction\<T\>\(T, StyleList\<EasingFunction\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionTimingFunction__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_EasingFunction__}

Sets [`transitionTimingFunction`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionTimingFunction.html) and returns the element for chaining.

```csharp
public static T SetTransitionTimingFunction<T>(this T element, StyleList<EasingFunction> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleList\<EasingFunction\>

The transition timing functions to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Determines how intermediate values are calculated for properties modified by a transition effect.

### SetTranslate\<T\>\(T, StyleTranslate\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetTranslate__1___0_UnityEngine_UIElements_StyleTranslate_}

Sets [`translate`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-translate.html) and returns the element for chaining.

```csharp
public static T SetTranslate<T>(this T element, StyleTranslate value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleTranslate

The translation to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

A translate transformation.

### SetUnityBackgroundImageTintColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityBackgroundImageTintColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`unityBackgroundImageTintColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityBackgroundImageTintColor.html) and returns the element for chaining.

```csharp
public static T SetUnityBackgroundImageTintColor<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The background image tint color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Tinting color for the element's backgroundImage.

### SetUnityBackgroundImageTintColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityBackgroundImageTintColor__1___0_System_String_}

Sets the background image tint color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetUnityBackgroundImageTintColor<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetUnityEditorTextRenderingMode\<T\>\(T, StyleEnum\<EditorTextRenderingMode\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_EditorTextRenderingMode__}

Sets [`unityEditorTextRenderingMode`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityEditorTextRenderingMode.html) and returns the element for chaining.

```csharp
public static T SetUnityEditorTextRenderingMode<T>(this T element, StyleEnum<EditorTextRenderingMode> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<EditorTextRenderingMode\>

The editor text rendering mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

TextElement editor rendering mode.

### SetUnityEditorTextRenderingMode\<T\>\(T, EditorTextRenderingMode\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_EditorTextRenderingMode_}

Sets [`unityEditorTextRenderingMode`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityEditorTextRenderingMode.html) and returns the element for chaining.

```csharp
public static T SetUnityEditorTextRenderingMode<T>(this T element, EditorTextRenderingMode value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` EditorTextRenderingMode

The editor text rendering mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

TextElement editor rendering mode.

### SetUnityFont\<T\>\(T, StyleFont\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFont__1___0_UnityEngine_UIElements_StyleFont_}

Sets [`unityFont`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFont.html) and returns the element for chaining.

```csharp
public static T SetUnityFont<T>(this T element, StyleFont value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFont

The font to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font to draw the element's text, defined as a Font object.

### SetUnityFontDefinition\<T\>\(T, StyleFontDefinition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontDefinition__1___0_UnityEngine_UIElements_StyleFontDefinition_}

Sets [`unityFontDefinition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontDefinition.html) and returns the element for chaining.

```csharp
public static T SetUnityFontDefinition<T>(this T element, StyleFontDefinition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFontDefinition

The font definition to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font to draw the element's text, defined as a FontDefinition structure. It takes precedence over -unity-font.

### SetUnityFontStyleAndWeight\<T\>\(T, StyleEnum\<FontStyle\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_FontStyle__}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) and returns the element for chaining.

```csharp
public static T SetUnityFontStyleAndWeight<T>(this T element, StyleEnum<FontStyle> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<FontStyle\>

The font style and weight to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font style and weight (normal, bold, italic) to draw the element's text.

### SetUnityFontStyleAndWeight\<T\>\(T, FontStyle\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_FontStyle_}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) and returns the element for chaining.

```csharp
public static T SetUnityFontStyleAndWeight<T>(this T element, FontStyle value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` FontStyle

The font style and weight to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font style and weight (normal, bold, italic) to draw the element's text.

### SetUnityMaterial\<T\>\(T, StyleMaterialDefinition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityMaterial__1___0_UnityEngine_UIElements_StyleMaterialDefinition_}

Sets [`unityMaterial`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityMaterial.html) and returns the element for chaining.

```csharp
public static T SetUnityMaterial<T>(this T element, StyleMaterialDefinition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleMaterialDefinition

The material to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Custom material to use on the element.

### SetUnityOverflowClipBox\<T\>\(T, StyleEnum\<OverflowClipBox\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_OverflowClipBox__}

Sets [`unityOverflowClipBox`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityOverflowClipBox.html) and returns the element for chaining.

```csharp
public static T SetUnityOverflowClipBox<T>(this T element, StyleEnum<OverflowClipBox> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<OverflowClipBox\>

The overflow clip box to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies which box the element content is clipped against.

### SetUnityOverflowClipBox\<T\>\(T, OverflowClipBox\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_OverflowClipBox_}

Sets [`unityOverflowClipBox`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityOverflowClipBox.html) and returns the element for chaining.

```csharp
public static T SetUnityOverflowClipBox<T>(this T element, OverflowClipBox value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` OverflowClipBox

The overflow clip box to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies which box the element content is clipped against.

### SetUnityParagraphSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityParagraphSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`unityParagraphSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityParagraphSpacing.html) and returns the element for chaining.

```csharp
public static T SetUnityParagraphSpacing<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The paragraph spacing to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between paragraphs.

### SetUnitySlice\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySlice__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html),
[`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the element for chaining.

```csharp
public static T SetUnitySlice<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The slice width to apply to all sides.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySlice\<T\>\(T, StyleInt?, StyleInt?, StyleInt?, StyleInt?\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySlice__1___0_System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html),
[`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the element for chaining.

```csharp
public static T SetUnitySlice<T>(this T element, StyleInt? top = null, StyleInt? right = null, StyleInt? bottom = null, StyleInt? left = null) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`top` StyleInt?

The top slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleInt?

The right slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleInt?

The bottom slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleInt?

The left slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceBottom\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceBottom__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceBottom<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The bottom slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>

### SetUnitySliceLeft\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceLeft__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceLeft<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The left slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceRight\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceRight__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceRight<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The right slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>

### SetUnitySliceScale\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceScale__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`unitySliceScale`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceScale.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceScale<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The slice scale to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Scale applied to an element's slices.

### SetUnitySliceTop\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceTop__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceTop<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The top slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>

### SetUnitySliceType\<T\>\(T, StyleEnum\<SliceType\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_SliceType__}

Sets [`unitySliceType`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceType.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceType<T>(this T element, StyleEnum<SliceType> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<SliceType\>

The slice type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the type of slicing.

### SetUnitySliceType\<T\>\(T, SliceType\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_SliceType_}

Sets [`unitySliceType`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceType.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceType<T>(this T element, SliceType value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` SliceType

The slice type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the type of slicing.

### SetUnitySliceX\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceX__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceX<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The horizontal slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceY\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceY__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html) and returns the element for chaining.

```csharp
public static T SetUnitySliceY<T>(this T element, StyleInt value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleInt

The vertical slice width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>

### SetUnityTextAlign\<T\>\(T, StyleEnum\<TextAnchor\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAlign__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextAnchor__}

Sets [`unityTextAlign`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAlign.html) and returns the element for chaining.

```csharp
public static T SetUnityTextAlign<T>(this T element, StyleEnum<TextAnchor> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<TextAnchor\>

The text alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Horizontal and vertical text alignment in the element's box.

### SetUnityTextAlign\<T\>\(T, TextAnchor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAlign__1___0_UnityEngine_TextAnchor_}

Sets [`unityTextAlign`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAlign.html) and returns the element for chaining.

```csharp
public static T SetUnityTextAlign<T>(this T element, TextAnchor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` TextAnchor

The text alignment to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Horizontal and vertical text alignment in the element's box.

### SetUnityTextAutoSize\<T\>\(T, StyleTextAutoSize\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAutoSize__1___0_UnityEngine_UIElements_StyleTextAutoSize_}

Sets [`unityTextAutoSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAutoSize.html) and returns the element for chaining.

```csharp
public static T SetUnityTextAutoSize<T>(this T element, StyleTextAutoSize value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleTextAutoSize

The text auto size settings to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Overrides any explicit font-size to scale text within the defined minimum and maximum bounds, recalculating as needed to fit its container.

### SetUnityTextGenerator\<T\>\(T, StyleEnum\<TextGeneratorType\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextGenerator__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextGeneratorType__}

Sets [`unityTextGenerator`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextGenerator.html) and returns the element for chaining.

```csharp
public static T SetUnityTextGenerator<T>(this T element, StyleEnum<TextGeneratorType> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<TextGeneratorType\>

The text generator type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Switches between Unity's standard and advanced text generator.

### SetUnityTextGenerator\<T\>\(T, TextGeneratorType\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextGenerator__1___0_UnityEngine_TextGeneratorType_}

Sets [`unityTextGenerator`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextGenerator.html) and returns the element for chaining.

```csharp
public static T SetUnityTextGenerator<T>(this T element, TextGeneratorType value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` TextGeneratorType

The text generator type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Switches between Unity's standard and advanced text generator.

### SetUnityTextOutlineColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`unityTextOutlineColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOutlineColor.html) and returns the element for chaining.

```csharp
public static T SetUnityTextOutlineColor<T>(this T element, StyleColor value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleColor

The text outline color to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Outline color of the text.

### SetUnityTextOutlineColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineColor__1___0_System_String_}

Sets the text outline color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetUnityTextOutlineColor<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetUnityTextOutlineWidth\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineWidth__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`unityTextOutlineWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOutlineWidth.html) and returns the element for chaining.

```csharp
public static T SetUnityTextOutlineWidth<T>(this T element, StyleFloat value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleFloat

The text outline width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Outline width of the text.

### SetUnityTextOverflowPosition\<T\>\(T, StyleEnum\<TextOverflowPosition\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflowPosition__}

Sets [`unityTextOverflowPosition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOverflowPosition.html) and returns the element for chaining.

```csharp
public static T SetUnityTextOverflowPosition<T>(this T element, StyleEnum<TextOverflowPosition> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<TextOverflowPosition\>

The text overflow position to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow position.

### SetUnityTextOverflowPosition\<T\>\(T, TextOverflowPosition\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_TextOverflowPosition_}

Sets [`unityTextOverflowPosition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOverflowPosition.html) and returns the element for chaining.

```csharp
public static T SetUnityTextOverflowPosition<T>(this T element, TextOverflowPosition value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` TextOverflowPosition

The text overflow position to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow position.

### SetUsageHints\<T\>\(T, UsageHints\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUsageHints__1___0_UnityEngine_UIElements_UsageHints_}

Sets [`usageHints`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-usageHints.html) and returns the element for chaining.

```csharp
public static T SetUsageHints<T>(this T element, UsageHints value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` UsageHints

The usage hints to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

A combination of hint values that specify high-level intended usage patterns for the VisualElement.
This property can only be set when the VisualElement is not yet part of a Panel. Once part of a Panel, this property becomes effectively read-only, and attempts to change it will throw an exception.
The specification of proper UsageHints drives the system to make better decisions on how to process or accelerate certain operations based on the anticipated usage pattern.
Note that those hints do not affect behavioral or visual results, but only affect the overall performance of the panel and the elements within.
It's advised to always consider specifying the proper UsageHints, but keep in mind that some UsageHints might be internally ignored under certain conditions (e.g. due to hardware limitations on the target platform).

### SetUserData\<T\>\(T, object\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetUserData__1___0_System_Object_}

Sets [`userData`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-userData.html) and returns the element for chaining.

```csharp
public static T SetUserData<T>(this T element, object value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

The user data to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property can be used to associate application-specific user data with this VisualElement.

### SetViewDataKey\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetViewDataKey__1___0_System_String_}

Sets [`viewDataKey`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-viewDataKey.html) and returns the element for chaining.

```csharp
public static T SetViewDataKey<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The view data key to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Used for view data persistence, such as tree expanded states, scroll position, or zoom level.

### SetVisibility\<T\>\(T, StyleEnum\<Visibility\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisibility__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Visibility__}

Sets [`visibility`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-visibility.html) and returns the element for chaining.

```csharp
public static T SetVisibility<T>(this T element, StyleEnum<Visibility> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<Visibility\>

The visibility to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies whether an element is visible.

### SetVisibility\<T\>\(T, Visibility\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisibility__1___0_UnityEngine_UIElements_Visibility_}

Sets [`visibility`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-visibility.html) and returns the element for chaining.

```csharp
public static T SetVisibility<T>(this T element, Visibility value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` Visibility

The visibility to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies whether an element is visible.

### SetVisible\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisible__1___0_System_Boolean_}

Sets [`visible`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement-visible.html) and returns the element for chaining.

```csharp
public static T SetVisible<T>(this T element, bool value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the element is visible.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Indicates whether or not this element should be rendered.

### SetWhiteSpace\<T\>\(T, StyleEnum\<WhiteSpace\>\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_WhiteSpace__}

Sets [`whiteSpace`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-whiteSpace.html) and returns the element for chaining.

```csharp
public static T SetWhiteSpace<T>(this T element, StyleEnum<WhiteSpace> value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleEnum\<WhiteSpace\>

The white-space mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Word wrap over multiple lines if not enough space is available to draw the text of an element.

### SetWhiteSpace\<T\>\(T, WhiteSpace\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_WhiteSpace_}

Sets [`whiteSpace`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-whiteSpace.html) and returns the element for chaining.

```csharp
public static T SetWhiteSpace<T>(this T element, WhiteSpace value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` WhiteSpace

The white-space mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Word wrap over multiple lines if not enough space is available to draw the text of an element.

### SetWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html) and returns the element for chaining.

```csharp
public static T SetWidth<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The width to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>

### SetWordSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_VisualElementExtensions_SetWordSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`wordSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-wordSpacing.html) and returns the element for chaining.

```csharp
public static T SetWordSpacing<T>(this T element, StyleLength value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` StyleLength

The word spacing to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between words.

### ToggleInClass\<T\>\(T, string\) {#Aspid_FastTools_UIElements_VisualElementExtensions_ToggleInClass__1___0_System_String_}

Toggles between adding and removing the given class name from the class list.

```csharp
public static T ToggleInClass<T>(this T element, string value) where T : VisualElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The USS class name to toggle.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

