---
title: "Class TypeExtensions"
sidebar_label: "TypeExtensions"
description: "Class TypeExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeExtensions {#Aspid_FastTools_Types_Editors_TypeExtensions}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Editor-side extension methods for [`Type`](https://learn.microsoft.com/dotnet/api/system.type): locate the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html)
asset that defines a type and open it in the external script editor.

```csharp
public static class TypeExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TypeExtensions](Aspid.FastTools.Types.Editors.TypeExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### FindMonoScript\(Type\) {#Aspid_FastTools_Types_Editors_TypeExtensions_FindMonoScript_System_Type_}

Searches the Asset Database for the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) that defines the given type.

```csharp
public static MonoScript FindMonoScript(this Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type to locate a script asset for.

#### Returns

 MonoScript

The matching [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) asset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if none is found.

#### Remarks

Falls back to scanning script text when [`GetClass`](https://docs.unity3d.com/ScriptReference/MonoScript-GetClass.html) yields no match,
so types whose file name differs from the type name are still found. A nested type owns no script
asset at all — neither the asset search nor [`GetClass`](https://docs.unity3d.com/ScriptReference/MonoScript-GetClass.html) can reach it — so the
lookup walks out to the declaring type, and accepts that script only when its text really declares
the nested type.

### OpenInScriptEditor\(Type\) {#Aspid_FastTools_Types_Editors_TypeExtensions_OpenInScriptEditor_System_Type_}

Opens the script that defines <code class="paramref">type</code> in the configured external
editor at the line of the type declaration. Logs a warning and is a no-op when
no [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) can be located; a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> type is silently ignored.

```csharp
public static void OpenInScriptEditor(this Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

