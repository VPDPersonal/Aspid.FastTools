using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    internal static class EnumValueUIToolkitPropertyDrawer
    {
        private const string UssClass = "aspid-fasttools-enum-value";
        private const string KeyClass = UssClass + "__key";
        private const string ValueClass = UssClass + "__value";
        private const string InlineClass = UssClass + "--inline";

        public static VisualElement Draw(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var keyPath = property.FindPropertyRelative("_key").propertyPath;
            var valuePath = property.FindPropertyRelative("_value").propertyPath;
            var enumTypePath = property.FindPropertyRelative("_enumType").propertyPath;

            var keyEnumField = new EnumField(label: string.Empty)
                .SetDisplay(DisplayStyle.None)
                .AddValueChanged(e => OnKeyChanged(e.newValue));

            var keyEnumFlagField = new EnumFlagsField(label: string.Empty)
                .SetDisplay(DisplayStyle.None)
                .AddValueChanged(e => OnKeyChanged(e.newValue));

            var keyField = new PropertyField(serializedObject.FindProperty(keyPath), label: string.Empty)
                .SetDisplay(DisplayStyle.None);

            UpdateValue();

            var valueProperty = serializedObject.FindProperty(valuePath);
            var hasFoldout = valueProperty.HasFoldout();

            var root = new VisualElement()
                .AddClass(UssClass)
                .AddChild(new VisualElement()
                    .AddClass(KeyClass)
                    .AddChild(keyField)
                    .AddChild(keyEnumField)
                    .AddChild(keyEnumFlagField)
                )
                .AddChild(new PropertyField(valueProperty, label: hasFoldout ? "Value" : string.Empty)
                    .AddClass(ValueClass)
                );

            if (!hasFoldout)
                root.AddClass(InlineClass);

            // The parent drawer writes _enumType straight into the SerializedProperty, which a
            // hidden bound PropertyField does not report; track the property itself instead.
            root.TrackPropertyValue(serializedObject.FindProperty(enumTypePath), _ => UpdateValue());

            return root;

            void OnKeyChanged(Enum value) => serializedObject
                .FindProperty(keyPath)
                .SetStringAndApply(value.ToString());

            void UpdateValue()
            {
                var keyProperty = serializedObject.FindProperty(keyPath);
                var enumTypeProperty = serializedObject.FindProperty(enumTypePath);

                keyField.SetDisplay(DisplayStyle.None);
                keyEnumField.SetDisplay(DisplayStyle.None);
                keyEnumFlagField.SetDisplay(DisplayStyle.None);

                if (EnumValuesPropertyDrawerHelper.ResolveKey(keyProperty, enumTypeProperty) is not { } enumValue)
                {
                    keyField.SetDisplay(DisplayStyle.Flex);
                    return;
                }

                if (EnumInfo.IsFlags(enumValue.GetType()))
                {
                    // EnumFlagsField's dropdown keeps choices from the previous enum type otherwise.
                    keyEnumFlagField
                        .SetValue(null, notify: false)
                        .Initialize(enumValue)
                        .SetDisplay(DisplayStyle.Flex);
                }
                else
                {
                    keyEnumField
                        .Initialize(enumValue)
                        .SetDisplay(DisplayStyle.Flex);
                }
            }
        }
    }
}
