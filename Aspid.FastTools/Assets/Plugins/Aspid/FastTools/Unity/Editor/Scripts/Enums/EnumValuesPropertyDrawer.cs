using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomPropertyDrawer(typeof(EnumValues<>))]
    internal sealed class EnumValuesPropertyDrawer : PropertyDrawer
    {
        private const string StylesheetPath = "Styles/Aspid-FastTools-EnumValues";
        private const string RootClass = "aspid-fasttools-enum-values";
        private const string HeaderClass = "aspid-fasttools-enum-values-header";
        private const string ContainerClass = "aspid-fasttools-enum-values-container";
        private const string ValuesClass = "aspid-fasttools-enum-values-values";
        private const string DefaultValueClass = "aspid-fasttools-enum-values-default-value";
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement()
                .AddStyleSheetsFromResource(StylesheetPath)
                .AddClass(RootClass);

            var values = property.FindPropertyRelative("_values");
            var enumType = property.FindPropertyRelative("_enumType");
            var defaultValueProperty = property.FindPropertyRelative("_defaultValue");
            
            var enumTypeField = new PropertyField(enumType, label: string.Empty);
            enumTypeField.RegisterValueChangeCallback(_ =>
            {
                UpdateValues();
            });
            
            var valuesField = new PropertyField(values).AddClass(ValuesClass);
            valuesField.RegisterValueChangeCallback(_ => UpdateValues());
            
            return root
                .AddChild(new VisualElement()
                    .AddClass(HeaderClass)
                    .AddChild(new Label(property.displayName))
                    .AddChild(enumTypeField)
                )
                .AddChild(new VisualElement()
                    .AddClass(ContainerClass)
                    .AddChild(valuesField)
                    .AddChild(new PropertyField(defaultValueProperty)
                            .AddClass(DefaultValueClass)
                    )
                );

            void UpdateValues()
            {
                for (var i = 0; i < values.arraySize; i++)
                {
                    var element = values.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("_enumType").SetStringAndApply(enumType.stringValue);
                }
            }
        }
    }
}