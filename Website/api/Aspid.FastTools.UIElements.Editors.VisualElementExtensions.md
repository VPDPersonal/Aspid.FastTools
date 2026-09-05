---
title: "Class VisualElementExtensions"
sidebar_label: "VisualElementExtensions"
description: "Class VisualElementExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class VisualElementExtensions {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions}

Namespace: [Aspid.FastTools.UIElements.Editors](Aspid.FastTools.UIElements.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

```csharp
public static class VisualElementExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[VisualElementExtensions](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddOpenScriptCommand\<T\>\(T, Object\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_AddOpenScriptCommand__1___0_UnityEngine_Object_}

Registers a double-click handler on <code class="paramref">element</code> that opens the script associated with <code class="paramref">obj</code> in the IDE.
Supports [`MonoBehaviour`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) and [`ScriptableObject`](https://docs.unity3d.com/ScriptReference/ScriptableObject.html) instances. Has no effect if no script can be resolved.

```csharp
public static T AddOpenScriptCommand<T>(this T element, Object obj) where T : VisualElement
```

#### Parameters

`element` T

The element to register the double-click command on.

`obj` Object

The Unity object whose script should be opened on double-click.

#### Returns

 T

<code class="paramref">element</code> for method chaining.

#### Type Parameters

`T` 

### BindTo\<T\>\(T, SerializedObject\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_BindTo__1___0_UnityEditor_SerializedObject_}

Binds the element to the specified [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

```csharp
public static T BindTo<T>(this T element, SerializedObject obj) where T : VisualElement
```

#### Parameters

`element` T

The element to bind.

`obj` SerializedObject

The serialized object to bind to.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### GetOwnerWindow\(VisualElement\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_GetOwnerWindow_UnityEngine_UIElements_VisualElement_}

Returns the [`EditorWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow.html) whose panel hosts <code class="paramref">element</code>, falling back to
[`focusedWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow-focusedWindow.html) / [`mouseOverWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow-mouseOverWindow.html) when no window's
panel matches (e.g. the element is detached).

```csharp
public static EditorWindow GetOwnerWindow(this VisualElement element)
```

#### Parameters

`element` VisualElement

#### Returns

 EditorWindow

#### Remarks

Use this instead of [`focusedWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow-focusedWindow.html) when anchoring a dropdown to an element:
a click into an unfocused floating window dispatches its pointer event before focus moves, so
<code>focusedWindow</code> still points at the previously focused window and a rect built from its
[`position`](https://docs.unity3d.com/ScriptReference/EditorWindow-position.html) lands in the wrong window's coordinate space.

### UnbindFrom\<T\>\(T\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_UnbindFrom__1___0_}

Unbinds the element from its serialized object.

```csharp
public static T UnbindFrom<T>(this T element) where T : VisualElement
```

#### Parameters

`element` T

The element to unbind.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

