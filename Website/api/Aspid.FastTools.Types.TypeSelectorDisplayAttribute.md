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
Assembly: Aspid.FastTools.Unity.dll  

Supplies presentation metadata for a type shown in the type-selector window: a display name,
a picker group, a tooltip and an icon — or, with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden), keeps the type out of the
picker altogether.

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

Rename the type in the picker, place it under an explicit group and give it a tooltip and an icon:


```csharp
[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```


Keep an adapter that only makes sense from code out of the picker:


```csharp
[TypeSelectorDisplay(Hidden = true)]
public sealed class DelegateModifier : IModifier { }
```


## Remarks

<code>[Conditional("UNITY_EDITOR")]</code> keeps this editor-only metadata out of player builds, matching the
other attributes in the package. The compiler evaluates the symbol at the <i>use</i> site, which also
means a type compiled outside Unity — a plugin <code>.dll</code> built by <code>dotnet build</code> — carries no
usage at all, so none of these settings apply to it, [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden) included. Declare the attribute
from inside the Unity project.

## Properties

### Group {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Group}

Explicit picker path for the type, with <code>/</code> separating levels (e.g. <code>"Combat/Melee"</code>).
The group <b>replaces</b> the type's namespace placement in the picker hierarchy — the type
appears only under this path. Empty segments are ignored; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace
means the type stays under its namespace.

```csharp
public string? Group { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Hidden {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden}

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the picker never offers this type — for types that are technically
assignable but not meant to be authored in the Inspector, such as a delegate-backed adapter or a
base implementation kept only for code. Assigning the type from code is unaffected, and a value
already stored in a field keeps rendering.

```csharp
public bool Hidden { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Not inherited, matching this attribute's [`AttributeUsageAttribute`](https://learn.microsoft.com/dotnet/api/system.attributeusageattribute): hiding a base type never hides
the subclasses meant to be picked instead of it.

### Icon {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Icon}

Editor icon to show left of the label. One of: an <code>EditorGUIUtility.IconContent</code> name
(e.g. <code>"d_ScriptableObject Icon"</code>); a project-relative asset path with extension
(e.g. <code>"Assets/Art/Icons/Damage.png"</code>, loaded via <code>AssetDatabase</code>); or a <code>Resources</code>
texture path without extension (e.g. <code>"Icons/Damage"</code>). The editor resolves the value lazily;
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means no icon.

```csharp
public string? Icon { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Name {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Name}

Display name shown instead of the type's short name — in the picker rows and in the closed
dropdown's caption. Search keeps matching the real type name as well, and the hover tooltip
still reveals the full <code>Namespace.Class, Assembly</code> identity. On a generic type the formatted
arguments (or parameters) are appended after the custom name (<code>Mod&lt;T&gt;</code>, <code>Mod&lt;Single&gt;</code>).
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace means no override.

```csharp
public string? Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Tooltip {#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Tooltip}

Tooltip shown when hovering the type's row. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means no tooltip override.

```csharp
public string? Tooltip { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)?

