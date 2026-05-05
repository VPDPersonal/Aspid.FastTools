using System;
using Aspid.FastTools.Editors;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    [UxmlElement]
    public sealed partial class TypeSelectorField : BaseField<Type>
    {
        private readonly Button _openButton;
        private readonly TextElement _textElement;
        private readonly VisualElement _visualInput;

        private readonly Object _target;
        private readonly string _propertyPath;
        
        private string _missingAssemblyQualifiedName;

        public Type[] Types { get; set; }
        
        [UxmlAttribute]
        public TypeAllow Allow { get; set; } = TypeAllow.All;

        public TypeSelectorField()
            : this(label: null) { }

        public TypeSelectorField(SerializedProperty property)
            : this(property.displayName, property) { }
        
        public TypeSelectorField(string label, SerializedProperty property)
            : this(label)
        {
            _propertyPath = property.propertyPath;
            _target = property.serializedObject.targetObject;
            
            SetValueFromAssemblyQualifiedNameWithoutNotify(property.stringValue);
        }

        public TypeSelectorField(string label, Type defaultValue = null)
            : this(label, visualInput: new VisualElement(), defaultValue) { }

        private TypeSelectorField(string label, VisualElement visualInput, Type defaultValue)
            : base(label, visualInput)
        {
            this.AddClass(EnumField.ussClassName)
                .AddStyleSheetsFromResource("UI/Aspid-FastTools-Default-Dark")
                .AddStyleSheetsFromResource("UI/Types/Aspid-FastTools-SerializableType");
            
            _visualInput = visualInput;
            
            _textElement = new TextElement()
                .AddClass(EnumField.textUssClassName)
                .SetPickingMode(PickingMode.Ignore);

            visualInput
                .AddClass(EnumField.inputUssClassName)
                .AddChild(_textElement)
                .AddChild(new VisualElement()
                    .AddClass(EnumField.arrowUssClassName)
                    .SetPickingMode(PickingMode.Ignore));
            
            visualInput.RegisterCallback<PointerDownEvent>(OnDropdownClicked);
            
            _openButton = new Button()
                .AddChild(new VisualElement())
                .AddClicked(OpenScript);

            this.AddChild(_openButton);
            SetValueWithoutNotify(defaultValue);
        }

        public override void SetValueWithoutNotify(Type newValue)
        {
            _missingAssemblyQualifiedName = null;
            base.SetValueWithoutNotify(newValue);
            UpdateDisplay();
        }

        public void SetValueFromAssemblyQualifiedNameWithoutNotify(string assemblyQualifiedName)
        {
            var resolved = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

            _missingAssemblyQualifiedName = resolved is null && !string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? assemblyQualifiedName
                : null;

            base.SetValueWithoutNotify(resolved);
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (_missingAssemblyQualifiedName is not null)
            {
                _textElement.text = Constants.MissingOption;
                _textElement.tooltip = $"Missing type: {_missingAssemblyQualifiedName}";
                _openButton.SetDisplay(DisplayStyle.None);
                return;
            }

            _textElement.text = value is null ? Constants.NoneOption : value.Name;
            _textElement.tooltip = value?.FullName ?? string.Empty;
            
            _openButton.SetDisplay(value is not null ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (evt.button is not 0) return;
            
            var window = EditorWindow.focusedWindow;
            if (!window) return;
            

            TypeSelectorWindow.Show(
                screenRect: GetScreenRect(),
                types: Types,
                currentAqn: value?.AssemblyQualifiedName ?? _missingAssemblyQualifiedName ?? string.Empty,
                allow: Allow,
                onSelected: assemblyQualifiedName =>
                {
                    this.SetValue(string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false));

                    new SerializedObject(_target).FindProperty(_propertyPath).SetStringAndApply(assemblyQualifiedName);
                });

            evt.StopPropagation();
            return;

            Rect GetScreenRect() => new(
                window.position.x + _visualInput.worldBound.xMin,
                window.position.y + _visualInput.worldBound.yMin,
                _visualInput.worldBound.width,
                _visualInput.worldBound.height);
        }

        private void OpenScript()
        {
            if (value is null) return;
            var (monoScript, lineNumber) = value.FindMonoScriptWithLine();

            if (monoScript is not null)
                AssetDatabase.OpenAsset(monoScript, lineNumber);
        }
    }
}
