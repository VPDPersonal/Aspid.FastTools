using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class IdRegistryEntryVisualElement : VisualElement
    {
        private readonly TextField _nameField;
        
        private readonly Label _idBadge;
        private readonly Label _errorLabel;
        
        private readonly Button _deleteButton;
        private readonly Button _confirmButton;

        public IdRegistryEntryData Data { get; private set; }

        public event Action<IdRegistryEntryVisualElement, IdRegistryEntryData> NameFocusIn;
        public event Action<IdRegistryEntryVisualElement, IdRegistryEntryData, string> NameChanging;
        public event Action<IdRegistryEntryVisualElement, IdRegistryEntryData, string> NameCommitRequested;
        public event Action<IdRegistryEntryVisualElement, IdRegistryEntryData> DeleteRequested;

        public IdRegistryEntryVisualElement()
        {
            _idBadge = new Label().SetIsSelectable(true);
            _nameField = new TextField();
            
            _deleteButton = new Button()
                .SetText("×")
                .AddClass(Constants.Registry.Delete);
            
            _confirmButton = new Button()
                .SetText("✓")
                .AddClass(Constants.Registry.Confirm)
                .SetDisplay(DisplayStyle.None);

            _errorLabel = new Label()
                .SetDisplay(DisplayStyle.None);

            this.AddChild(new VisualElement()
                    .AddChild(_idBadge)
                    .AddChild(_nameField)
                    .AddChild(_deleteButton)
                    .AddChild(_confirmButton))
                .AddChild(_errorLabel);

            _nameField.RegisterCallback<FocusInEvent>(_ => NameFocusIn?.Invoke(this, Data));
            _nameField.RegisterValueChangedCallback(e => NameChanging?.Invoke(this, Data, e.newValue));
            _nameField.RegisterCallback<FocusOutEvent>(OnNameFieldFocusOut);
            _deleteButton.clicked += () => DeleteRequested?.Invoke(this, Data);
            _confirmButton.clicked += () => NameCommitRequested?.Invoke(this, Data, _nameField.value);
        }

        private void OnNameFieldFocusOut(FocusOutEvent evt)
        {
            if (evt.relatedTarget is VisualElement target && IsCommitTarget(target)) return;
            if (_nameField.value == Data.Name) return;

            _nameField.SetValueWithoutNotify(Data.Name);
            SetEditMode(false);

            if (Data.IsDuplicate) SetError("Name already exists.");
            else ClearError();
        }

        private bool IsCommitTarget(VisualElement target) =>
            target == _confirmButton || _confirmButton.Contains(target);

        public void SetEditMode(bool editing, bool canConfirm = true)
        {
            _deleteButton.SetDisplay(editing ? DisplayStyle.None : DisplayStyle.Flex);
            _confirmButton.SetDisplay(editing ? DisplayStyle.Flex : DisplayStyle.None);
            _confirmButton.SetEnabled(canConfirm);
        }

        public void Bind(in IdRegistryEntryData data)
        {
            Data = data;
            
            _nameField.SetValueWithoutNotify(data.Name);
            _idBadge.text = data.Id.ToString();
            SetEditMode(false);

            if (data.IsDuplicate) SetError("Name already exists.");
            else ClearError();
        }

        public void SetError(string message)
        {
            _errorLabel.text = message;
            _errorLabel.SetDisplay(DisplayStyle.Flex);
            _idBadge.AddClass(StatusStyle.ErrorClass);
        }

        public void ClearError()
        {
            _errorLabel.SetDisplay(DisplayStyle.None);
            _idBadge.RemoveClass(StatusStyle.ErrorClass);
        }

        public void SetNameWithoutNotify(string name) =>
            _nameField.SetValueWithoutNotify(name);
    }
}
