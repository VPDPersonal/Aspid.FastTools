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

Supplies presentation metadata for a type in the type-selector window — display name, group, tooltip and
icon — or keeps the type out of the picker entirely with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden).

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

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorDisplayAttribute, TValue\>\(TypeSelectorDisplayAttribute, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

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

<code>[Conditional("UNITY_EDITOR")]</code> keeps this metadata out of player builds. The compiler evaluates the
symbol at the use site, so a type compiled outside Unity — a plugin <code>.dll</code> built by
<code>dotnet build</code> — carries no usage at all and none of these settings apply to it,
[`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden) included. Declare the attribute from inside the Unity project.

## Properties

### Group {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Group}

Gets or sets an explicit picker path, with <code>/</code> separating levels (<code>"Combat/Melee"</code>).
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace keeps the type under its namespace.

```csharp
public string? Group { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Remarks

The path replaces the type's namespace placement, so the type appears only under it. Empty segments are
ignored.

### Hidden {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden}

Gets or sets a value indicating whether the picker never offers this type — for types that are assignable
but not meant to be authored in the Inspector.

```csharp
public bool Hidden { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Assigning from code is unaffected, and a value already stored in a field keeps rendering. Not inherited,
so hiding a base type never hides the subclasses meant to be picked instead.

### Icon {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Icon}

Gets or sets the icon shown left of the label: an <code>EditorGUIUtility.IconContent</code> name
(<code>"d_ScriptableObject Icon"</code>), a project-relative asset path with extension
(<code>"Assets/Art/Icons/Damage.png"</code>), or a <code>Resources</code> path without extension
(<code>"Icons/Damage"</code>). Resolved lazily; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means no icon.

```csharp
public string? Icon { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Name {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Name}

Gets or sets the name shown instead of the type's short name. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace means
no override.

```csharp
public string? Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Remarks

Search still matches the real name, the tooltip still shows the full identity, and a generic type keeps
its formatted arguments appended (<code>Mod&lt;Single&gt;</code>).

### Tooltip {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Tooltip}

Gets or sets the tooltip shown on the type's row; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means no override.

```csharp
public string? Tooltip { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

