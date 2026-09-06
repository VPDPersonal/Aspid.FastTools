---
title: "Class TypeSelectorAttribute"
sidebar_label: "TypeSelectorAttribute"
description: "Class TypeSelectorAttribute — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeSelectorAttribute {#Aspid_FastTools_Types_TypeSelectorAttribute}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Draws the field with the type-selector window.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class TypeSelectorAttribute : PropertyAttribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
PropertyAttribute ← 
[TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples


```csharp
[TypeSelector(typeof(MonoBehaviour))]
[SerializeField] private string _behaviorType;
```


```csharp
[TypeSelector]
[SerializeField] private string _anyType;
```


```csharp
[TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
[SerializeField] private string _type;
```


## Remarks

With several base types the picker shows only types assignable to all of them.

## Constructors

### TypeSelectorAttribute\(\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor}

Creates an unconstrained attribute: any type is offered.

```csharp
public TypeSelectorAttribute()
```

### TypeSelectorAttribute\(Type\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type_}

Creates an attribute constrained to a single base type.

```csharp
public TypeSelectorAttribute(Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The base constraint type.

### TypeSelectorAttribute\(params Type\[\]\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type___}

Creates an attribute constrained to one or more base types.

```csharp
public TypeSelectorAttribute(params Type[] types)
```

#### Parameters

`types` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

The base constraint types.

### TypeSelectorAttribute\(string\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String_}

Creates an attribute constrained to a single base type named by a string.

```csharp
public TypeSelectorAttribute(string assemblyQualifiedName)
```

#### Parameters

`assemblyQualifiedName` [string](https://learn.microsoft.com/dotnet/api/system.string)

An assembly-qualified type name (<code>"MyGame.IWeapon, MyGame"</code>) or the name of a member supplying the
constraint — see the remarks.

#### Examples


```csharp
[SerializeField] private SerializableType _category;
```


```csharp
[TypeSelector(nameof(_category))]
[SerializeField] private string _subType;
```


#### Remarks

Resolved member-first: an identifier matching an instance field or property on the target object supplies
the constraint from its current value, so it can be driven live by another field; anything else is treated
as an assembly-qualified type name. A member may be a [`Type`](https://learn.microsoft.com/dotnet/api/system.type), a <code>string</code>, a
[`SerializableType`](Aspid.FastTools.Types.SerializableType.md), or an array of these. Prefer <code>nameof(...)</code> so a rename keeps the
reference intact. A name that resolves to nothing is surfaced as an inline inspector notice.

### TypeSelectorAttribute\(params string\[\]?\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String___}

Creates an attribute constrained to one or more base types, each named by a string.

```csharp
public TypeSelectorAttribute(params string[]? assemblyQualifiedNames)
```

#### Parameters

`assemblyQualifiedNames` [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]?

Each entry is resolved independently, member-first; see [`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md).

## Properties

### Allow {#Aspid_FastTools_Types_TypeSelectorAttribute_Allow}

Gets or sets which special type categories the picker offers besides concrete classes;
[`TypeAllow.All`](Aspid.FastTools.Types.TypeAllow.md) by default.

```csharp
public TypeAllow Allow { get; set; }
```

#### Property Value

 [TypeAllow](Aspid.FastTools.Types.TypeAllow.md)

#### Remarks

Ignored on a <code>[SerializeReference]</code> field, which always lists only instantiable types.

### AssemblyQualifiedNames {#Aspid_FastTools_Types_TypeSelectorAttribute_AssemblyQualifiedNames}

Gets the raw constraint arguments: assembly-qualified names of base types, or names of members supplying
them (see [`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md)). Empty for an unconstrained selector.

```csharp
public string[] AssemblyQualifiedNames { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### Required {#Aspid_FastTools_Types_TypeSelectorAttribute_Required}

Gets or sets a value indicating whether an unset field shows an inline "required" warning and counts as a
violation for the build/CI gate.

```csharp
public bool Required { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Examples


```csharp
[TypeSelector(typeof(IWeapon), Required = true)]
[SerializeReference] private IWeapon _weapon;
```


#### Remarks

"Unset" means <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for a <code>[SerializeReference]</code> field and an empty name for a
<code>string</code> or [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) field. A reference that is set but whose type no longer
resolves is not a violation of this flag — the separate missing-type gate covers that.

