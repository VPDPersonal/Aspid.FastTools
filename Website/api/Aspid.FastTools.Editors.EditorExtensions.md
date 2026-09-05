---
title: "Class EditorExtensions"
sidebar_label: "EditorExtensions"
description: "Class EditorExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class EditorExtensions {#Aspid_FastTools_Editors_EditorExtensions}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Editor-side extension methods for [`Object`](https://docs.unity3d.com/ScriptReference/Object.html) and its subclass [`Component`](https://docs.unity3d.com/ScriptReference/Component.html)
that resolve human-readable script names, respecting the [`AddComponentMenu`](https://docs.unity3d.com/ScriptReference/AddComponentMenu.html) attribute.

```csharp
public static class EditorExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EditorExtensions](Aspid.FastTools.Editors.EditorExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### GetScriptName\(Object\) {#Aspid_FastTools_Editors_EditorExtensions_GetScriptName_UnityEngine_Object_}

Returns a human-readable display name for the given Unity object.
If the object's type (or any of its base types) is decorated with [`AddComponentMenu`](https://docs.unity3d.com/ScriptReference/AddComponentMenu.html),
the name is taken from [`GetInspectorTitle`](https://docs.unity3d.com/ScriptReference/ObjectNames-GetInspectorTitle.html), which honours the menu name;
otherwise it falls back to [`NicifyVariableName`](https://docs.unity3d.com/ScriptReference/ObjectNames-NicifyVariableName.html) applied to the type name.

```csharp
public static string GetScriptName(this Object obj)
```

#### Parameters

`obj` Object

The object whose display name should be resolved.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The display name string, or [`Empty`](https://learn.microsoft.com/dotnet/api/system.string.empty) if <code class="paramref">obj</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>
or has been destroyed.

### GetScriptNameWithIndex\(Component\) {#Aspid_FastTools_Editors_EditorExtensions_GetScriptNameWithIndex_UnityEngine_Component_}

Returns the display name of a component with a 1-based numeric suffix appended when multiple
components of the exact same type exist on the same [`GameObject`](https://docs.unity3d.com/ScriptReference/GameObject.html). The index reflects
the order returned by [`GetComponents`](https://docs.unity3d.com/ScriptReference/Component-GetComponents.html).
For example, the second <code>AudioSource</code> on the object is returned as <code>"Audio Source (2)"</code>.

```csharp
public static string GetScriptNameWithIndex(this Component targetComponent)
```

#### Parameters

`targetComponent` Component

The component whose indexed display name should be resolved.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The display name with an index suffix if duplicates exist on the same object,
the plain display name if there is only one such component,
or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if <code class="paramref">targetComponent</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>
or has been destroyed.

