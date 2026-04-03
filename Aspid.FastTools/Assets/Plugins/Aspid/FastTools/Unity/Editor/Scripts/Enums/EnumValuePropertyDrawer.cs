using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomPropertyDrawer(typeof(EnumValue<>))]
    internal sealed class EnumValuePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var keyProperty = property.FindPropertyRelative("_key");
            var enumTypeProperty = property.FindPropertyRelative("_enumType");

            var enumTypeField = new PropertyField(enumTypeProperty, label: string.Empty).SetDisplay(DisplayStyle.None);
            var keyField = new PropertyField(keyProperty, label: string.Empty).SetDisplay(DisplayStyle.None);
          
            var keyEnumField = new EnumField(label: string.Empty).SetDisplay(DisplayStyle.None); 
            var keyEnumFlagField = new EnumFlagsField(label: string.Empty).SetDisplay(DisplayStyle.None); 
            
            enumTypeField.RegisterValueChangeCallback(e =>
            {
                UpdateValue();
            });

            keyEnumField.RegisterValueChangedCallback(e =>
            {
                keyProperty.SetStringAndApply(e.newValue.ToString());
            });
            
            keyEnumFlagField.RegisterValueChangedCallback(e =>
            {
                keyProperty.SetStringAndApply(e.newValue.ToString());
            });

            return container
                .AddChild(enumTypeField)
                .AddChild(keyField)
                .AddChild(keyEnumField)
                .AddChild(keyEnumFlagField)
                .AddChild(new PropertyField(property.FindPropertyRelative("_value"), label: string.Empty));

            void UpdateValue()
            {
                var enumType = Type.GetType(enumTypeProperty.stringValue, throwOnError: false);

                keyField.SetDisplay(DisplayStyle.None);
                keyEnumField.SetDisplay(DisplayStyle.None);
                keyEnumFlagField.SetDisplay(DisplayStyle.None);
                
                if (enumType is null)
                {
                    keyField.SetDisplay(DisplayStyle.Flex);
                }
                else
                {
                    Enum enumValue;

                    try
                    {
                        enumValue = (Enum)Enum.Parse(enumType, keyProperty.stringValue);
                    }
                    catch
                    {
                        try
                        {
                            enumValue = (Enum)Enum.GetValues(enumType).GetValue(index: 0);
                        }
                        catch
                        {
                            keyField.SetDisplay(DisplayStyle.Flex);
                            return;
                        }
                    }
                    
                    keyProperty.SetStringAndApply(enumValue.ToString());
                    
                    if (enumType.IsDefined(typeof(FlagsAttribute), false))
                    {
                        keyEnumFlagField.SetDisplay(DisplayStyle.Flex);

                        keyEnumFlagField.value = null;
                        keyEnumFlagField.Init(enumValue);
                    }
                    else
                    {                   
                        keyEnumField.SetDisplay(DisplayStyle.Flex);
                        keyEnumField.Init(enumValue);
                    }
                }
            }
        }
    }
}