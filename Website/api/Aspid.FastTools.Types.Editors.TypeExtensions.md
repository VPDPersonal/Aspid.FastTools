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

Provides editor-side extension methods for locating and opening the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) defining a
[`Type`](https://learn.microsoft.com/dotnet/api/system.type).

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

Searches the Asset Database for the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) defining a type.

```csharp
public static MonoScript FindMonoScript(this Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type to locate a script asset for.

#### Returns

 MonoScript

The matching asset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when none is found.

#### Remarks

Falls back to scanning script text when [`GetClass`](https://docs.unity3d.com/ScriptReference/MonoScript-GetClass.html) finds no match, so a type whose
file name differs from its own is still found. A nested type owns no script asset, so the lookup walks out
to the declaring type and accepts that script only when its text really declares the nested type.

<p>
The result is the file the type is declared in, which for a nested type is not the file whose own class it
is, so a caller writing it into <code>m_Script</code> must check [`GetClass`](https://docs.unity3d.com/ScriptReference/MonoScript-GetClass.html) against the
type it asked for.
</p>

### OpenInScriptEditor\(Type\) {#Aspid_FastTools_Types_Editors_TypeExtensions_OpenInScriptEditor_System_Type_}

Opens the script defining <code class="paramref">type</code> at its declaration line.

```csharp
public static void OpenInScriptEditor(this Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type whose script to open.

#### Remarks

Logs a warning when no script can be located; a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> type is ignored.

