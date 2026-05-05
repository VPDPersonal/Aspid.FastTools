using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    [UxmlElement]
    public sealed partial class TypeField : BaseField<Type>
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-SerializableType";
        
        private readonly Button _openButton;
        private readonly TextElement _textElement;
        private readonly VisualElement _visualInput;
        private readonly DynamicSerializeProperty _property;

        private string _missingAssemblyQualifiedName;

        [UxmlAttribute]
        public TypeAllow Allow { get; set; } = TypeAllow.None;

        public Type[] Types { get; set; } = { typeof(object) };
        
        public TypeField()
            : this(label: null) { }

        public TypeField(SerializedProperty property)
            : this(property.displayName, property) { }
        
        public TypeField(string label, SerializedProperty property)
            : this(label)
        {
            _property = new DynamicSerializeProperty(property);
            SetValueFromAssemblyQualifiedNameWithoutNotify(property.stringValue);
        }

        public TypeField(string label, Type defaultValue = null)
            : this(label, visualInput: new VisualElement(), defaultValue) { }

        private TypeField(string label, VisualElement visualInput, Type defaultValue)
            : base(label, visualInput)
        {
            this.AddClass(EnumField.ussClassName)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet);
            
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
            _textElement.SetText(TypeSelectorHelpers.GetTypeSelectorTitle(value, _missingAssemblyQualifiedName));
            _openButton.SetDisplay(_missingAssemblyQualifiedName is null ? DisplayStyle.Flex : DisplayStyle.None);
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

                    _property?.GetProperty()?.SetStringAndApply(assemblyQualifiedName);
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
