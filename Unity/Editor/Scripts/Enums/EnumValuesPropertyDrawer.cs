using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors;
using Aspid.FastTools.UIElements.Manipulators;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    /// <summary>
    /// Property drawer for <see cref="EnumValues{TValue}"/> and <see cref="EnumValues{TEnum,TValue}"/>.
    /// Renders a header with the enum-type picker (untyped variant only — the typed variant fixes
    /// the enum at compile time), the entries list, and the default-value field, and exposes a
    /// context-menu action that fills in any missing enum members from the configured type.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumValues<>))]
    [CustomPropertyDrawer(typeof(EnumValues<,>))]
    internal sealed class EnumValuesPropertyDrawer : PropertyDrawer
    {
        private const string StylesheetPath = "UI/Enums/Aspid-FastTools-EnumValues";

        private const string UssClass = "aspid-fasttools-enum-values";
        private const string HeaderClass = UssClass + "__header";
        private const string ContainerClass = UssClass + "__container";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var valuesPath = property.FindPropertyRelative("_values").propertyPath;
            var enumTypePath = property.FindPropertyRelative("_enumType").propertyPath;
            var defaultValuePath = property.FindPropertyRelative("_defaultValue").propertyPath;

            // The typed variant stamps _enumType itself on every serialize pass (and building this
            // inspector's SerializedObject already forced one), so it needs no picker and no
            // tracking — the enum type can never change.
            var isTyped = IsTypedVariant();

            // Push the parent enum type into every existing entry up-front so already-serialized
            // arrays don't render with a stale per-element _enumType until the user re-edits.
            UpdateValues();

            var header = new VisualElement()
                .AddClass(HeaderClass)
                .AddChild(new Label(property.displayName));

            if (!isTyped)
                header.AddChild(new PropertyField(serializedObject.FindProperty(enumTypePath), label: string.Empty));

            var root = new VisualElement()
                .SetName($"enum-values-{property.name.ToKebabCase()}")
                .AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StylesheetPath)
                .AddManipulatorSelf(EnumValuesPropertyDrawerHelper.CreatePopulateMenuManipulator(
                    serializedObject: serializedObject,
                    values: valuesPath,
                    enumType: enumTypePath,
                    defaultValue: defaultValuePath)
                )
                .AddChild(header)
                .AddChild(new VisualElement()
                    .AddClass(ContainerClass)
                    .AddChild(new PropertyField(serializedObject.FindProperty(valuesPath))
                        .AddValueChanged(_ => UpdateValues())
                    )
                    .AddChild(new PropertyField(serializedObject.FindProperty(defaultValuePath)))
                );

            // The enum-type PropertyField hosts the TypeSelector custom drawer, which writes the
            // picked type straight into the SerializedProperty — PropertyField only forwards
            // UI-driven ChangeEvents from custom drawers, so a SerializedPropertyChangeEvent
            // callback would never fire. Track the property itself instead.
            // The typed variant's enum type never changes — nothing to track.
            if (!isTyped)
                root.TrackPropertyValue(serializedObject.FindProperty(enumTypePath), _ => UpdateValues());

            return root;

            void UpdateValues()
            {
                var values = serializedObject.FindProperty(valuesPath);
                var enumTypeValue = serializedObject.FindProperty(enumTypePath).stringValue;

                for (var i = 0; i < values.arraySize; i++)
                {
                    var enumTypeElement = values.GetArrayElementAtIndex(i).FindPropertyRelative("_enumType");

                    if (enumTypeElement.stringValue != enumTypeValue)
                        enumTypeElement.SetStringAndApply(enumTypeValue);
                }
            }
        }

        /// <summary>
        /// Whether the drawn field is an <see cref="EnumValues{TEnum,TValue}"/> (directly, or as
        /// an array/list element) — the variant with the enum fixed at compile time — rather than
        /// the untyped <see cref="EnumValues{TValue}"/>.
        /// </summary>
        private bool IsTypedVariant()
        {
            var type = fieldInfo.FieldType;

            if (type.IsArray)
                type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];

            return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(EnumValues<,>);
        }
    }
}
