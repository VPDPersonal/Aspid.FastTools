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
    /// <summary>
    /// Property drawer for <see cref="EnumValue{TValue}"/>. Picks an EnumField/EnumFlagsField
    /// for the row's key based on the enum type configured on the parent
    /// <see cref="EnumValues{TValue}"/>, and falls back to a raw string field when the type
    /// can't be resolved.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumValue<>))]
    internal sealed class EnumValuePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Re-fetch by path (never hold onto the SerializedProperty instances passed in)
            // inside every closure below — this row's index can shift or disappear from under
            // us via list reorder/delete, and a captured reference throws ObjectDisposedException
            // once its array slot is gone.
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

            var enumTypeField = new PropertyField(serializedObject.FindProperty(enumTypePath), label: string.Empty)
                .SetDisplay(DisplayStyle.None)
                .AddValueChanged(_ => UpdateValue());

            // Sync visibility with the currently serialized enum type — without this the
            // EnumField/EnumFlagsField stay hidden until the user edits the type.
            UpdateValue();

            return new VisualElement()
                .AddChild(enumTypeField)
                .AddChild(keyField)
                .AddChild(keyEnumField)
                .AddChild(keyEnumFlagField)
                .AddChild(new PropertyField(serializedObject.FindProperty(valuePath), label: string.Empty));

            void OnKeyChanged(Enum value) => serializedObject
                .FindProperty(keyPath)
                .SetStringAndApply(value.ToString());

            void UpdateValue()
            {
                var keyProperty = serializedObject.FindProperty(keyPath);
                var enumTypeProperty = serializedObject.FindProperty(enumTypePath);

                var enumType = Type.GetType(enumTypeProperty.stringValue, throwOnError: false);

                keyField.SetDisplay(DisplayStyle.None);
                keyEnumField.SetDisplay(DisplayStyle.None);
                keyEnumFlagField.SetDisplay(DisplayStyle.None);

                if (enumType is null)
                {
                    keyField.SetDisplay(DisplayStyle.Flex);
                    return;
                }

                if (!Enum.TryParse(enumType, keyProperty.stringValue, out var parsed))
                {
                    // Stored key doesn't match any member (first-time init, or the enum was
                    // edited/renamed since). Fall back to the first member and persist it,
                    // migrating the stale key rather than leaving the row unusable.
                    var values = Enum.GetValues(enumType);
                    if (values.Length is 0)
                    {
                        keyField.SetDisplay(DisplayStyle.Flex);
                        return;
                    }

                    parsed = values.GetValue(0);
                }

                var enumValue = (Enum)parsed;

                if (keyProperty.stringValue != enumValue.ToString())
                    keyProperty.SetStringAndApply(enumValue.ToString());

                if (enumType.IsDefined(typeof(FlagsAttribute), false))
                {
                    // Reset before Init — EnumFlagsField's checkbox dropdown can retain stale
                    // choices from the previous enum type otherwise. EnumField below has no such
                    // dropdown state and doesn't need it.
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
