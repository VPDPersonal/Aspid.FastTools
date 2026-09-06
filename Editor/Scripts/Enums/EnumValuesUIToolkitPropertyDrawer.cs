using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    internal static class EnumValuesUIToolkitPropertyDrawer
    {
        private const string StylesheetPath = "UI/Enums/Aspid-FastTools-EnumValues";

        private const string UssClass = "aspid-fasttools-enum-values";
        private const string HeaderClass = UssClass + "__header";
        private const string ContainerClass = UssClass + "__container";

        public static VisualElement Draw(SerializedProperty property, bool isTyped)
        {
            var serializedObject = property.serializedObject;
            var valuesPath = property.FindPropertyRelative("_values").propertyPath;
            var enumTypePath = property.FindPropertyRelative("_enumType").propertyPath;
            var defaultValuePath = property.FindPropertyRelative("_defaultValue").propertyPath;

            UpdateValues();

            var header = new VisualElement()
                .AddClass(HeaderClass)
                .AddChild(new Label(property.displayName));

            header.AddChild(isTyped
                ? new InspectorTypeField(label: null, serializedObject.FindProperty(enumTypePath))
                {
                    IsReadOnly = true
                }
                : new PropertyField(serializedObject.FindProperty(enumTypePath), label: string.Empty));

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

            // The TypeSelector drawer writes straight into the SerializedProperty, which a
            // PropertyField change callback never sees; track the property itself instead.
            if (!isTyped)
                root.TrackPropertyValue(serializedObject.FindProperty(enumTypePath), _ => UpdateValues());

            return root;

            void UpdateValues() => EnumValuesPropertyDrawerHelper.SyncEntryEnumTypes(
                serializedObject.FindProperty(valuesPath),
                serializedObject.FindProperty(enumTypePath));
        }
    }
}
