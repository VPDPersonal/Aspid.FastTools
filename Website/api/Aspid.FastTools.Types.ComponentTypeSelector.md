---
title: "Struct ComponentTypeSelector"
sidebar_label: "ComponentTypeSelector"
description: "Struct ComponentTypeSelector — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct ComponentTypeSelector {#Aspid_FastTools_Types_ComponentTypeSelector}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Adds an Inspector dropdown that lets you swap the object's script
to any subtype of the field's declaring class.

```csharp
[Serializable]
public struct ComponentTypeSelector
```


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<ComponentTypeSelector, TValue\>\(ComponentTypeSelector, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Place a field of this type inside the root component class.
The Inspector will render a dropdown listing all subtypes of <code>BaseEnemy</code>:


```csharp
public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _typeSelector;
}

public class FastEnemy : BaseEnemy { }
public class TankEnemy : BaseEnemy { }
```


Selecting "TankEnemy" turns the object into a <code>TankEnemy</code> instance.

## Remarks

<p>
When the user picks a type, the editor locates the corresponding <code>MonoScript</code>
asset and writes it to <code>m_Script</code> on the <code>SerializedObject</code>, effectively
changing the component or ScriptableObject to the chosen subtype.
</p>
<p>
The picker is automatically constrained to subtypes of the class that declares
the field — no extra configuration is needed.
</p>

