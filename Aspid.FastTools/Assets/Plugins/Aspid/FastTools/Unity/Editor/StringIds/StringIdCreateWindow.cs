#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal sealed class StringIdCreateWindow : EditorWindow
    {
        private TextField _idField = null!;
        private Button _createButton = null!;
        private Label _errorLabel = null!;
        private Action<string>? _onCreated;
        private StringIdRegistry? _registry;
        private Type? _structType;

        public static void Show(StringIdRegistry? registry, Type? structType, Rect screenRect, Action<string> onCreated)
        {
            var window = CreateInstance<StringIdCreateWindow>();
            window.Initialize(registry, structType, screenRect, onCreated);
        }

        private void Initialize(StringIdRegistry? registry, Type? structType, Rect screenRect, Action<string> onCreated)
        {
            _onCreated = onCreated;
            _registry = registry;
            _structType = structType;

            BuildUI();

            var size = new Vector2(Mathf.Max(220, screenRect.width), 68);
            ShowAsDropDown(screenRect, size);

            _idField.Focus();
        }

        private void BuildUI()
        {
            _idField = new TextField { label = string.Empty };
            _idField.style.flexGrow = 1;
            _idField.RegisterValueChangedCallback(e => UpdateValidation(e.newValue));
            _idField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                    TryCreate();
            });

            _errorLabel = new Label()
                .SetDisplay(DisplayStyle.None);
            _errorLabel.style.color = new Color(1f, 0.4f, 0.4f);
            _errorLabel.style.fontSize = 10;

            _createButton = new Button(TryCreate) { text = "Create" };
            _createButton.SetEnabled(false);

            rootVisualElement
                .SetPadding(top: 6, bottom: 6, left: 6, right: 6)
                .SetFlexDirection(FlexDirection.Column)
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetMargin(bottom: 2)
                    .AddChild(_idField)
                    .AddChild(_createButton))
                .AddChild(_errorLabel);
        }

        private void UpdateValidation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _errorLabel.SetDisplay(DisplayStyle.None);
                _createButton.SetEnabled(false);
                return;
            }

            if (_registry != null && _registry.Contains(value))
            {
                _errorLabel.text = "ID already exists";
                _errorLabel.SetDisplay(DisplayStyle.Flex);
                _createButton.SetEnabled(false);
                return;
            }

            _errorLabel.SetDisplay(DisplayStyle.None);
            _createButton.SetEnabled(true);
        }

        private void TryCreate()
        {
            var id = _idField.value?.Trim();
            if (string.IsNullOrEmpty(id)) return;

            if (_registry == null)
                _registry = StringIdRegistryHelper.CreateRegistry(_structType);

            _registry.Add(id);
            EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssetIfDirty(_registry);

            _onCreated?.Invoke(id);
            Close();
        }
    }
}
