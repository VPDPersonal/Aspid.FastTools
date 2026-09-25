---
title: "Class SerializableMonoScript"
sidebar_label: "SerializableMonoScript"
description: "Class SerializableMonoScript — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableMonoScript {#Aspid_FastTools_Types_SerializableMonoScript}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) referencing it through its <code>MonoScript</code>
asset, so renaming or moving the class does not break the field.

```csharp
[Serializable]
public class SerializableMonoScript : SerializableTypeBase, ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)

#### Derived

[SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
ISerializationCallbackReceiver


#### Extension Methods

[TextInputBaseFieldTextSelectionExtensions.AddOnCursorIndexChange\<SerializableMonoScript, TValue\>\(SerializableMonoScript, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.AddOnSelectIndexChange\<SerializableMonoScript, TValue\>\(SerializableMonoScript, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.AddValueChanged\<SerializableMonoScript, TValue\>\(SerializableMonoScript, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableMonoScript\>\(SerializableMonoScript\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnCursorIndexChange\<SerializableMonoScript, TValue\>\(SerializableMonoScript, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnSelectIndexChange\<SerializableMonoScript, TValue\>\(SerializableMonoScript, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableMonoScript, TValue\>\(SerializableMonoScript, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[TextInputBaseFieldExtensions.SetAutoCorrection\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetCursorIndex\<SerializableMonoScript, TValue\>\(SerializableMonoScript, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetDelayed\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetDoubleClickSelectsWord\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideMobileInput\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHidePlaceholderOnFocus\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideSoftKeyboard\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_),
[SliderExtensions.SetHighValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_),
[TextInputBaseFieldExtensions.SetKeyboardType\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TouchScreenKeyboardType\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_),
[BaseFieldExtensions.SetLabel\<SerializableMonoScript, TValue\>\(SerializableMonoScript, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[SliderExtensions.SetLowValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_),
[TextInputBaseFieldExtensions.SetMaskChar\<SerializableMonoScript, TValue\>\(SerializableMonoScript, char\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_),
[TextInputBaseFieldExtensions.SetMaxLength\<SerializableMonoScript, TValue\>\(SerializableMonoScript, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetPassword\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetPlaceholder\<SerializableMonoScript, TValue\>\(SerializableMonoScript, string\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_),
[TextInputBaseFieldExtensions.SetReadOnly\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnFocus\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnMouseUp\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectIndex\<SerializableMonoScript, TValue\>\(SerializableMonoScript, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectable\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetTripleClickSelectsLine\<SerializableMonoScript, TValue\>\(SerializableMonoScript, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_),
[INotifyValueChangedExtensions.SetValue\<SerializableMonoScript, TValue\>\(SerializableMonoScript, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples


```csharp
public class Spawner : MonoBehaviour
{
    [SerializeField] private SerializableMonoScript _componentType;

    private void Start()
    {
        Type type = _componentType;  // implicit conversion
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Remarks

<p>
In the editor the script asset is the source of truth: on every serialization the stored assembly-qualified
name is re-read from the script's class. The script reference is editor-only, so a player build carries just
the name and resolves it exactly as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) does.
</p>
<p>
Only types Unity maps to a script asset can be referenced this way — a top-level, non-generic class declared
in a file of the same name. Use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) for nested and generic types.
</p>

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

## Operators

### implicit operator Type?\(SerializableMonoScript?\) {#Aspid_FastTools_Types_SerializableMonoScript_op_Implicit_Aspid_FastTools_Types_SerializableMonoScript__System_Type}

Converts the wrapper to the type it holds.

```csharp
public static implicit operator Type?(SerializableMonoScript? type)
```

#### Parameters

`type` [SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)?

The wrapper to convert.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The wrapped type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the wrapper is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or holds no
resolvable type.

