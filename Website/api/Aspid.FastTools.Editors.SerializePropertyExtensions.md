---
title: "Class SerializePropertyExtensions"
sidebar_label: "SerializePropertyExtensions"
description: "Class SerializePropertyExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializePropertyExtensions {#Aspid_FastTools_Editors_SerializePropertyExtensions}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Provides extension methods for synchronizing and assigning [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) values.

```csharp
public static class SerializePropertyExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializePropertyExtensions](Aspid.FastTools.Editors.SerializePropertyExtensions.md)


## Methods

### AddArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_AddArraySize__1___0_System_Int32_}

Increases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> and returns the property for chaining.

```csharp
public static T AddArraySize<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to add to the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### AddArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_AddArraySizeAndApply__1___0_System_Int32_}

Increases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties.

```csharp
public static T AddArraySizeAndApply<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to add to the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### AddArraySizeAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_AddArraySizeAndApplyWithoutUndo__1___0_System_Int32_}

Increases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties without recording Undo.

```csharp
public static T AddArraySizeAndApplyWithoutUndo<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to add to the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### ApplyModifiedProperties\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_ApplyModifiedProperties__1___0_}

Calls [`ApplyModifiedProperties`](https://docs.unity3d.com/ScriptReference/SerializedObject-ApplyModifiedProperties.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T ApplyModifiedProperties<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object changes should be applied.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### ApplyModifiedPropertiesWithoutUndo\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_ApplyModifiedPropertiesWithoutUndo__1___0_}

Calls [`ApplyModifiedPropertiesWithoutUndo`](https://docs.unity3d.com/ScriptReference/SerializedObject-ApplyModifiedPropertiesWithoutUndo.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T ApplyModifiedPropertiesWithoutUndo<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object changes should be applied without registering an undo step.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### GetDeclaringInstance\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetDeclaringInstance_UnityEditor_SerializedProperty_}

Returns the instance declaring the backing field, including the collection owner for array or list elements.

```csharp
public static object GetDeclaringInstance(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property whose declaring instance to resolve.

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)

The declaring instance; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if the path cannot be resolved.

#### Remarks

A struct instance is a boxed copy; modifying it does not update the serialized object.

### GetFieldInfo\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetFieldInfo_UnityEditor_SerializedProperty_}

Resolves the backing field on the runtime type of the declaring instance or its base classes.

```csharp
public static FieldInfo GetFieldInfo(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property whose backing field to locate.

#### Returns

 [FieldInfo](https://learn.microsoft.com/dotnet/api/system.reflection.fieldinfo)

The backing field; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if it cannot be resolved.

#### Remarks

For an array or list element, returns the collection field.

### GetMemberName\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetMemberName_UnityEditor_SerializedProperty_}

Returns the backing field name, including the collection field for an array or list element.

```csharp
public static string GetMemberName(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property to inspect.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The backing field name without collection indices.

### GetPropertyType\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetPropertyType_UnityEditor_SerializedProperty_}

Returns the backing field type or, for an array or list element, its element type.

```csharp
public static Type GetPropertyType(this SerializedProperty serializedProperty)
```

#### Parameters

`serializedProperty` SerializedProperty

The property to inspect.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

The declared type; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if the backing field cannot be resolved.

### HasFoldout\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_HasFoldout_UnityEditor_SerializedProperty_}

Determines whether a generic serialized value has visible children.

```csharp
public static bool HasFoldout(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property to inspect.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> for a generic value with visible children; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Remarks

Managed references and custom drawer layouts are not covered by this check.

### IsArrayElement\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_IsArrayElement_UnityEditor_SerializedProperty_}

Determines whether <code class="paramref">property</code> represents an array or list element.

```csharp
public static bool IsArrayElement(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property to inspect.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the path ends with an element index; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### Persistent\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_Persistent_UnityEditor_SerializedProperty_}

Returns the property at the same path on an independent [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

```csharp
public static SerializedProperty Persistent(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The source property.

#### Returns

 SerializedProperty

The independent property; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if the path no longer exists on the targets.

#### Remarks

The caller owns the serialized object of a non-<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> result and must dispose it when finished.
Pending changes on the source are not copied until they have been applied to its targets.

### RemoveArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_RemoveArraySize__1___0_System_Int32_}

Decreases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> and returns the property for chaining.

```csharp
public static T RemoveArraySize<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to subtract from the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### RemoveArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_RemoveArraySizeAndApply__1___0_System_Int32_}

Decreases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties.

```csharp
public static T RemoveArraySizeAndApply<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to subtract from the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### RemoveArraySizeAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_RemoveArraySizeAndApplyWithoutUndo__1___0_System_Int32_}

Decreases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties without recording Undo.

```csharp
public static T RemoveArraySizeAndApplyWithoutUndo<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to subtract from the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetAnimationCurve\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetAnimationCurve__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) and returns the property for chaining.

```csharp
public static T SetAnimationCurve<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetAnimationCurveAndApply\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetAnimationCurveAndApply__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties.

```csharp
public static T SetAnimationCurveAndApply<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetAnimationCurveAndApplyWithoutUndo\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetAnimationCurveAndApplyWithoutUndo__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetAnimationCurveAndApplyWithoutUndo<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetArraySize__1___0_System_Int32_}

Sets [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) and returns the property for chaining.

```csharp
public static T SetArraySize<T>(this T property, int size) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`size` [int](https://learn.microsoft.com/dotnet/api/system.int32)

New array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetArraySizeAndApply__1___0_System_Int32_}

Sets [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) then applies modified properties.

```csharp
public static T SetArraySizeAndApply<T>(this T property, int size) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`size` [int](https://learn.microsoft.com/dotnet/api/system.int32)

New array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetArraySizeAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetArraySizeAndApplyWithoutUndo__1___0_System_Int32_}

Sets [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) then applies modified properties without recording Undo.

```csharp
public static T SetArraySizeAndApplyWithoutUndo<T>(this T property, int size) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`size` [int](https://learn.microsoft.com/dotnet/api/system.int32)

New array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBool\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBool__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) and returns the property for chaining.

```csharp
public static T SetBool<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoolAndApply\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoolAndApply__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties.

```csharp
public static T SetBoolAndApply<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoolAndApplyWithoutUndo\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoolAndApplyWithoutUndo__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetBoolAndApplyWithoutUndo<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBounds\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBounds__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) and returns the property for chaining.

```csharp
public static T SetBounds<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsAndApply\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsAndApply__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties.

```csharp
public static T SetBoundsAndApply<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsAndApplyWithoutUndo\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsAndApplyWithoutUndo__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetBoundsAndApplyWithoutUndo<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsInt\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsInt__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) and returns the property for chaining.

```csharp
public static T SetBoundsInt<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsIntAndApply\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsIntAndApply__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties.

```csharp
public static T SetBoundsIntAndApply<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsIntAndApplyWithoutUndo\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsIntAndApplyWithoutUndo__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetBoundsIntAndApplyWithoutUndo<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoxed\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoxed__1___0_System_Object_}

Sets [`boxedValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html) and returns the property for chaining.

```csharp
public static T SetBoxed<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Boxed value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoxedAndApply\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoxedAndApply__1___0_System_Object_}

Sets [`boxedValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html) then applies modified properties.

```csharp
public static T SetBoxedAndApply<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Boxed value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoxedAndApplyWithoutUndo\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoxedAndApplyWithoutUndo__1___0_System_Object_}

Sets [`boxedValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetBoxedAndApplyWithoutUndo<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Boxed value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetColor\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetColor__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) and returns the property for chaining.

```csharp
public static T SetColor<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetColorAndApply\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetColorAndApply__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties.

```csharp
public static T SetColorAndApply<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetColorAndApplyWithoutUndo\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetColorAndApplyWithoutUndo__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetColorAndApplyWithoutUndo<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetDouble\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetDouble__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) and returns the property for chaining.

```csharp
public static T SetDouble<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetDoubleAndApply\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetDoubleAndApply__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties.

```csharp
public static T SetDoubleAndApply<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetDoubleAndApplyWithoutUndo\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetDoubleAndApplyWithoutUndo__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetDoubleAndApplyWithoutUndo<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEntityId\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEntityId__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) and returns the property for chaining.

```csharp
public static T SetEntityId<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEntityIdAndApply\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEntityIdAndApply__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties.

```csharp
public static T SetEntityIdAndApply<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEntityIdAndApplyWithoutUndo\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEntityIdAndApplyWithoutUndo__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetEntityIdAndApplyWithoutUndo<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEnumFlag\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlag__1___0_System_Int32_}

Sets [`enumValueFlag`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueFlag.html) and returns the property for chaining.

```csharp
public static T SetEnumFlag<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Flag value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum flags because it would conflict with
[`SerializePropertyExtensions.SetInt<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md#Aspid_FastTools_Editors_SerializePropertyExtensions_SetInt__1___0_System_Int32_). Call [`SerializePropertyExtensions.SetEnumFlag<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlag__1___0_System_Int32_) explicitly.

### SetEnumFlagAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlagAndApply__1___0_System_Int32_}

Sets [`enumValueFlag`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueFlag.html) then applies modified properties.

```csharp
public static T SetEnumFlagAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Flag value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEnumFlagAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlagAndApplyWithoutUndo__1___0_System_Int32_}

Sets [`enumValueFlag`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueFlag.html) then applies modified properties without recording Undo.

```csharp
public static T SetEnumFlagAndApplyWithoutUndo<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Flag value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEnumIndex\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndex__1___0_System_Int32_}

Sets [`enumValueIndex`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueIndex.html) and returns the property for chaining.

```csharp
public static T SetEnumIndex<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Index value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum index because it would conflict with
[`SerializePropertyExtensions.SetInt<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md#Aspid_FastTools_Editors_SerializePropertyExtensions_SetInt__1___0_System_Int32_). Call [`SerializePropertyExtensions.SetEnumIndex<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndex__1___0_System_Int32_) explicitly.

### SetEnumIndexAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndexAndApply__1___0_System_Int32_}

Sets [`enumValueIndex`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueIndex.html) then applies modified properties.

```csharp
public static T SetEnumIndexAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Index value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEnumIndexAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndexAndApplyWithoutUndo__1___0_System_Int32_}

Sets [`enumValueIndex`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueIndex.html) then applies modified properties without recording Undo.

```csharp
public static T SetEnumIndexAndApplyWithoutUndo<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Index value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetExposedReference\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetExposedReference__1___0_UnityEngine_Object_}

Sets [`exposedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-exposedReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetExposedReference<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) exposed reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetExposedReferenceAndApply\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetExposedReferenceAndApply__1___0_UnityEngine_Object_}

Sets [`exposedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-exposedReferenceValue.html) then applies modified properties.

```csharp
public static T SetExposedReferenceAndApply<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) exposed reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetExposedReferenceAndApplyWithoutUndo\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetExposedReferenceAndApplyWithoutUndo__1___0_UnityEngine_Object_}

Sets [`exposedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-exposedReferenceValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetExposedReferenceAndApplyWithoutUndo<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) exposed reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetFloat\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetFloat__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) and returns the property for chaining.

```csharp
public static T SetFloat<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetFloatAndApply\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetFloatAndApply__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties.

```csharp
public static T SetFloatAndApply<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetFloatAndApplyWithoutUndo\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetFloatAndApplyWithoutUndo__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetFloatAndApplyWithoutUndo<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetGradient\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetGradient__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) and returns the property for chaining.

```csharp
public static T SetGradient<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetGradientAndApply\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetGradientAndApply__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties.

```csharp
public static T SetGradientAndApply<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetGradientAndApplyWithoutUndo\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetGradientAndApplyWithoutUndo__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetGradientAndApplyWithoutUndo<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetHash128\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetHash128__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) and returns the property for chaining.

```csharp
public static T SetHash128<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetHash128AndApply\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetHash128AndApply__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties.

```csharp
public static T SetHash128AndApply<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetHash128AndApplyWithoutUndo\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetHash128AndApplyWithoutUndo__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetHash128AndApplyWithoutUndo<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetInt\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetInt__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) and returns the property for chaining.

```csharp
public static T SetInt<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetIntAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetIntAndApply__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties.

```csharp
public static T SetIntAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetIntAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetIntAndApplyWithoutUndo__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetIntAndApplyWithoutUndo<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetLong\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetLong__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) and returns the property for chaining.

```csharp
public static T SetLong<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetLongAndApply\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetLongAndApply__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties.

```csharp
public static T SetLongAndApply<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetLongAndApplyWithoutUndo\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetLongAndApplyWithoutUndo__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetLongAndApplyWithoutUndo<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetManagedReference\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetManagedReference__1___0_System_Object_}

Sets [`managedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetManagedReference<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property (must be a <code>[SerializeReference]</code> field).

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Managed reference value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetManagedReferenceAndApply\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetManagedReferenceAndApply__1___0_System_Object_}

Sets [`managedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html) then applies modified properties.

```csharp
public static T SetManagedReferenceAndApply<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property (must be a <code>[SerializeReference]</code> field).

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Managed reference value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetManagedReferenceAndApplyWithoutUndo\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetManagedReferenceAndApplyWithoutUndo__1___0_System_Object_}

Sets [`managedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetManagedReferenceAndApplyWithoutUndo<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property (must be a <code>[SerializeReference]</code> field).

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Managed reference value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetObjectReference\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetObjectReference__1___0_UnityEngine_Object_}

Sets [`objectReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-objectReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetObjectReference<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetObjectReferenceAndApply\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetObjectReferenceAndApply__1___0_UnityEngine_Object_}

Sets [`objectReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-objectReferenceValue.html) then applies modified properties.

```csharp
public static T SetObjectReferenceAndApply<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetObjectReferenceAndApplyWithoutUndo\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetObjectReferenceAndApplyWithoutUndo__1___0_UnityEngine_Object_}

Sets [`objectReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-objectReferenceValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetObjectReferenceAndApplyWithoutUndo<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetQuaternion\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetQuaternion__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) and returns the property for chaining.

```csharp
public static T SetQuaternion<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetQuaternionAndApply\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetQuaternionAndApply__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties.

```csharp
public static T SetQuaternionAndApply<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetQuaternionAndApplyWithoutUndo\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetQuaternionAndApplyWithoutUndo__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetQuaternionAndApplyWithoutUndo<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRect\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRect__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) and returns the property for chaining.

```csharp
public static T SetRect<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectAndApply\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectAndApply__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties.

```csharp
public static T SetRectAndApply<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectAndApplyWithoutUndo\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectAndApplyWithoutUndo__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetRectAndApplyWithoutUndo<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectInt\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectInt__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) and returns the property for chaining.

```csharp
public static T SetRectInt<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectIntAndApply\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectIntAndApply__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties.

```csharp
public static T SetRectIntAndApply<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectIntAndApplyWithoutUndo\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectIntAndApplyWithoutUndo__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetRectIntAndApplyWithoutUndo<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetString\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetString__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) and returns the property for chaining.

```csharp
public static T SetString<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetStringAndApply\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetStringAndApply__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties.

```csharp
public static T SetStringAndApply<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetStringAndApplyWithoutUndo\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetStringAndApplyWithoutUndo__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetStringAndApplyWithoutUndo<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUint\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUint__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) and returns the property for chaining.

```csharp
public static T SetUint<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUintAndApply\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUintAndApply__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties.

```csharp
public static T SetUintAndApply<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUintAndApplyWithoutUndo\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUintAndApplyWithoutUndo__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetUintAndApplyWithoutUndo<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUlong\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUlong__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) and returns the property for chaining.

```csharp
public static T SetUlong<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUlongAndApply\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUlongAndApply__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties.

```csharp
public static T SetUlongAndApply<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUlongAndApplyWithoutUndo\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUlongAndApplyWithoutUndo__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetUlongAndApplyWithoutUndo<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApplyWithoutUndo\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApplyWithoutUndo__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetValueAndApplyWithoutUndo<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) and returns the property for chaining.

```csharp
public static T SetVector2<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2AndApply\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2AndApply__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties.

```csharp
public static T SetVector2AndApply<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2AndApplyWithoutUndo\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2AndApplyWithoutUndo__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetVector2AndApplyWithoutUndo<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2Int\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2Int__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) and returns the property for chaining.

```csharp
public static T SetVector2Int<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2IntAndApply\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2IntAndApply__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties.

```csharp
public static T SetVector2IntAndApply<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2IntAndApplyWithoutUndo\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2IntAndApplyWithoutUndo__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetVector2IntAndApplyWithoutUndo<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) and returns the property for chaining.

```csharp
public static T SetVector3<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3AndApply\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3AndApply__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties.

```csharp
public static T SetVector3AndApply<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3AndApplyWithoutUndo\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3AndApplyWithoutUndo__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetVector3AndApplyWithoutUndo<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3Int\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3Int__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) and returns the property for chaining.

```csharp
public static T SetVector3Int<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3IntAndApply\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3IntAndApply__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties.

```csharp
public static T SetVector3IntAndApply<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3IntAndApplyWithoutUndo\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3IntAndApplyWithoutUndo__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties without recording Undo.

```csharp
public static T SetVector3IntAndApplyWithoutUndo<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector4\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector4__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) and returns the property for chaining.

```csharp
public static T SetVector4<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector4AndApply\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector4AndApply__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties.

```csharp
public static T SetVector4AndApply<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector4AndApplyWithoutUndo\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector4AndApplyWithoutUndo__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties without recording Undo.

```csharp
public static T SetVector4AndApplyWithoutUndo<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### Update\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_Update__1___0_}

Calls [`Update`](https://docs.unity3d.com/ScriptReference/SerializedObject-Update.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T Update<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object should be updated.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### UpdateIfRequiredOrScript\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_UpdateIfRequiredOrScript__1___0_}

Calls [`UpdateIfRequiredOrScript`](https://docs.unity3d.com/ScriptReference/SerializedObject-UpdateIfRequiredOrScript.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T UpdateIfRequiredOrScript<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object should be conditionally updated.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

