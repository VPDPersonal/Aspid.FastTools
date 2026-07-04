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
    /// UIToolkit rendering for <see cref="EnumValuePropertyDrawer"/>.
    /// </summary>
    internal static class EnumValueUIToolkitPropertyDrawer
    {
        public static VisualElement Draw(SerializedProperty property)
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

            // Sync visibility with the currently serialized enum type — without this the
            // EnumField/EnumFlagsField stay hidden until the user edits the type.
            UpdateValue();

            var root = new VisualElement()
                .AddChild(keyField)
                .AddChild(keyEnumField)
                .AddChild(keyEnumFlagField)
                .AddChild(new PropertyField(serializedObject.FindProperty(valuePath), label: string.Empty));

            // _enumType is stamped into the row by the parent EnumValues drawer via a direct
            // SerializedProperty write, which a hidden bound PropertyField won't reliably report
            // as a change event — track the property itself instead.
            root.TrackPropertyValue(serializedObject.FindProperty(enumTypePath), _ => UpdateValue());

            return root;

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

                // A resolvable non-enum type (e.g. an enum refactored into a class/struct with
                // the same name) would make Enum.TryParse/Enum.GetValues below throw — fall back
                // to the raw string field, same as EnumValuesPropertyDrawerHelper's guard.
                if (enumType is null || !enumType.IsEnum)
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
