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

Provides extension methods for locating and opening the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) defining a
[`Type`](https://learn.microsoft.com/dotnet/api/system.type).

```csharp
public static class TypeExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TypeExtensions](Aspid.FastTools.Types.Editors.TypeExtensions.md)


## Methods

### FindMonoScript\(Type\) {#Aspid_FastTools_Types_Editors_TypeExtensions_FindMonoScript_System_Type_}

Searches script assets for a declaration of <code class="paramref">type</code>.

```csharp
public static MonoScript FindMonoScript(this Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type to locate, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no match.

#### Returns

 MonoScript

The matching script asset; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if no declaration is found.

#### Remarks

Text matching is limited to assets matching the type name and, for nested types, the declaring type's script.
Check [`GetClass`](https://docs.unity3d.com/ScriptReference/MonoScript-GetClass.html) before assigning the result to a component's <code>m_Script</code> property.

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

