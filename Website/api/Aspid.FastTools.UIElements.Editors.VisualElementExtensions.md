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
Assembly: Aspid.FastTools.Editor.dll  

Provides extension methods for [`VisualElement`](https://docs.unity3d.com/ScriptReference/UIElements-VisualElement.html).

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

Registers a double-click handler that opens <code class="paramref">obj</code>'s script in the IDE.

```csharp
public static T AddOpenScriptCommand<T>(this T element, Object obj) where T : VisualElement
```

#### Parameters

`element` T

The element to register the command on.

`obj` Object

The object whose script is opened.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Supports [`MonoBehaviour`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) and [`ScriptableObject`](https://docs.unity3d.com/ScriptReference/ScriptableObject.html); a resolved script is required,
so it has no effect otherwise.

### BindTo\<T\>\(T, SerializedObject\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_BindTo__1___0_UnityEditor_SerializedObject_}

Binds the element to <code class="paramref">obj</code>.

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

The element type.

### GetOwnerWindow\(VisualElement\) {#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_GetOwnerWindow_UnityEngine_UIElements_VisualElement_}

Returns the [`EditorWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow.html) whose panel hosts <code class="paramref">element</code>, falling back to the
focused or hovered window when no panel matches.

```csharp
public static EditorWindow GetOwnerWindow(this VisualElement element)
```

#### Parameters

`element` VisualElement

The element whose hosting window is wanted.

#### Returns

 EditorWindow

The hosting window, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when none can be resolved.

#### Remarks

Use it instead of [`focusedWindow`](https://docs.unity3d.com/ScriptReference/EditorWindow-focusedWindow.html) when anchoring a dropdown to an element: a
click into an unfocused floating window dispatches its pointer event before focus moves, so a rect built
from the focused window's position lands in the wrong coordinate space.

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

