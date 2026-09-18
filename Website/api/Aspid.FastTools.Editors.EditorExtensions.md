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

Provides extension methods for resolving Unity object display names.

```csharp
public static class EditorExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EditorExtensions](Aspid.FastTools.Editors.EditorExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### GetDisplayName\(Object\) {#Aspid_FastTools_Editors_EditorExtensions_GetDisplayName_UnityEngine_Object_}

Returns the inspector title when an inherited [`AddComponentMenu`](https://docs.unity3d.com/ScriptReference/AddComponentMenu.html) exists, or the nicified type name.

```csharp
public static string GetDisplayName(this Object obj)
```

#### Parameters

`obj` Object

The object whose display name to resolve.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The display name; otherwise, [`Empty`](https://learn.microsoft.com/dotnet/api/system.string.empty) if <code class="paramref">obj</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or destroyed.

### GetDisplayNameWithIndex\(Component\) {#Aspid_FastTools_Editors_EditorExtensions_GetDisplayNameWithIndex_UnityEngine_Component_}

Returns the component display name with a one-based suffix when its object has multiple components of the exact same type.

```csharp
public static string GetDisplayNameWithIndex(this Component targetComponent)
```

#### Parameters

`targetComponent` Component

The component whose indexed display name to resolve.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The display name, indexed in component order when duplicates exist; otherwise, [`Empty`](https://learn.microsoft.com/dotnet/api/system.string.empty) if <code class="paramref">targetComponent</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or destroyed.

