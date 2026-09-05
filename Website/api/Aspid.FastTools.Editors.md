---
title: "Namespace Aspid.FastTools.Editors"
sidebar_label: "Aspid.FastTools.Editors"
description: "Namespace Aspid.FastTools.Editors — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.Editors {#Aspid_FastTools_Editors}

### Classes

 [EditorExtensions](Aspid.FastTools.Editors.EditorExtensions.md)

Editor-side extension methods for [`Object`](https://docs.unity3d.com/ScriptReference/Object.html) and its subclass [`Component`](https://docs.unity3d.com/ScriptReference/Component.html)
that resolve human-readable script names, respecting the [`AddComponentMenu`](https://docs.unity3d.com/ScriptReference/AddComponentMenu.html) attribute.

 [SerializePropertyExtensions](Aspid.FastTools.Editors.SerializePropertyExtensions.md)

Fluent extension methods for [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) providing chainable wrappers
around [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html) synchronization and typed value setters.

### Structs

 [HorizontalScope](Aspid.FastTools.Editors.HorizontalScope.md)

Disposable ref struct wrapper around [`BeginHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginHorizontal.html) /
[`EndHorizontal`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndHorizontal.html) that exposes the resulting [`HorizontalScope.Rect`](Aspid.FastTools.Editors.HorizontalScope.md#Aspid_FastTools_Editors_HorizontalScope_Rect).
Use in a <code>using</code> statement to automatically close the horizontal group.

 [ScrollViewScope](Aspid.FastTools.Editors.ScrollViewScope.md)

Disposable ref struct wrapper around [`BeginScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginScrollView.html) /
[`EndScrollView`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndScrollView.html) that updates the caller's scroll position in place
and exposes it via [`ScrollViewScope.ScrollPosition`](Aspid.FastTools.Editors.ScrollViewScope.md#Aspid_FastTools_Editors_ScrollViewScope_ScrollPosition). Use in a <code>using</code> statement to automatically
close the scroll view.

 [VerticalScope](Aspid.FastTools.Editors.VerticalScope.md)

Disposable ref struct wrapper around [`BeginVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-BeginVertical.html) /
[`EndVertical`](https://docs.unity3d.com/ScriptReference/EditorGUILayout-EndVertical.html) that exposes the resulting [`VerticalScope.Rect`](Aspid.FastTools.Editors.VerticalScope.md#Aspid_FastTools_Editors_VerticalScope_Rect).
Use in a <code>using</code> statement to automatically close the vertical group.

