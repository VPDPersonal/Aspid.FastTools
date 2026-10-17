---
title: "Class TypeSelectorDisplayAttribute"
sidebar_label: "TypeSelectorDisplayAttribute"
description: "Class TypeSelectorDisplayAttribute — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeSelectorDisplayAttribute {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Sets how the type-selector window shows a type, or hides the type with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden).

```csharp
[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface, Inherited = false)]
public sealed class TypeSelectorDisplayAttribute : Attribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
[TypeSelectorDisplayAttribute](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<TypeSelectorDisplayAttribute\>\(TypeSelectorDisplayAttribute, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```


```csharp
[TypeSelectorDisplay(Hidden = true)]
public sealed class DelegateModifier : IModifier { }
```


## Remarks

Has no effect on a type compiled outside Unity, such as a <code>dotnet build</code> DLL:
<code>[Conditional("UNITY_EDITOR")]</code> drops the usage there.

## Properties

### Group {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Group}

Gets or sets the picker path that replaces the namespace, with <code>/</code> separating levels
(<code>"Combat/Melee"</code>); <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace keeps the namespace.

```csharp
public string? Group { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Hidden {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden}

Gets or sets a value indicating whether the picker hides this type.

```csharp
public bool Hidden { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Not inherited by subclasses. Assigning from code and values already stored are unaffected.

### Icon {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Icon}

Gets or sets the icon shown left of the label: an <code>EditorGUIUtility.IconContent</code> name
(<code>"d_ScriptableObject Icon"</code>), an asset path with extension (<code>"Assets/Art/Icons/Damage.png"</code>) or
a <code>Resources</code> path without one (<code>"Icons/Damage"</code>); <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

```csharp
public string? Icon { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Name {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Name}

Gets or sets the name shown instead of the type's short name; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace keeps
the short name.

```csharp
public string? Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Remarks

Search still matches the real name, and a generic type keeps its arguments appended
(<code>Mod&lt;Single&gt;</code>).

### Tooltip {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Tooltip}

Gets or sets the tooltip shown on the type's row; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means no override.

```csharp
public string? Tooltip { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

