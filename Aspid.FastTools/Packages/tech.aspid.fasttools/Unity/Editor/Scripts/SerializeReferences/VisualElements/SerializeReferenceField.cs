using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// UIToolkit field for a <c>[SerializeReference]</c> property: a foldout whose header carries an
    /// EnumField-style type dropdown (backed by <see cref="TypeSelectorWindow"/>) and an open-script button,
    /// whose content hosts the assigned instance's nested properties, and which surfaces a missing-type
    /// warning when the stored type can no longer be resolved.
    /// </summary>
    /// <remarks>
    /// Always bound to a managed-reference <see cref="SerializedProperty"/>; created by the
    /// <see cref="SerializeReferenceSelectorPropertyDrawer"/>, not from UXML. The field keeps the live
    /// inspector property so child fields round-trip through Unity's binding (apply/Undo) and only rebuilds
    /// the nested properties when the assigned type actually changes.
    /// </remarks>
    internal sealed class SerializeReferenceField : VisualElement
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        private const string BlockClass = "aspid-fasttools-serialize-reference";
        private const string EmptyClass = BlockClass + "--empty";
        private const string DropdownClass = BlockClass + "__dropdown";

        private readonly Foldout _foldout;
        private readonly TextElement _caption;
        private readonly VisualElement _dropdown;
        private readonly Button _openButton;
        private readonly VisualElement _content;
        private readonly SerializedProperty _property;
        private readonly Type[] _types;

        private AspidHelpBox _missingBox;
        private Type _currentType;
        private bool _contentBuilt;

        public SerializeReferenceField(string label, SerializedProperty property)
        {
            _property = property;
            _types = new[] { SerializeReferenceHelpers.GetFieldType(_property) };

            this.AddClass(BlockClass)
                .AddClass(PropertyField.ussClassName)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet);

            _foldout = new Foldout().SetText(label);
            _foldout.RegisterValueChangedCallback(OnFoldoutToggled);
            _content = _foldout.contentContainer;

            _caption = new TextElement()
                .AddClass(EnumField.textUssClassName)
                .SetPickingMode(PickingMode.Ignore);

            _dropdown = new VisualElement()
                .AddClass(EnumField.ussClassName)
                .AddClass(EnumField.inputUssClassName)
                .AddClass(DropdownClass)
                .AddChild(_caption)
                .AddChild(new VisualElement()
                    .AddClass(EnumField.arrowUssClassName)
                    .SetPickingMode(PickingMode.Ignore));

            _dropdown.RegisterCallback<PointerDownEvent>(OnDropdownClicked);

            _openButton = new Button()
                .AddChild(new VisualElement())
                .AddClicked(() => SerializeReferenceHelpers.GetCurrentType(_property)?.OpenInScriptEditor());

            _foldout.Q<Toggle>()
                .AddChild(_dropdown)
                .AddChild(_openButton);

            this.AddChild(_foldout);

            Refresh(forceRebuild: true);
            this.TrackPropertyValue(_property, _ => Refresh(forceRebuild: false));
        }

        private void Refresh(bool forceRebuild)
        {
            var currentType = SerializeReferenceHelpers.GetCurrentType(_property);
            var hasValue = currentType is not null;

            _caption.SetText(GetCaption(currentType));
            _openButton.SetDisplay(hasValue ? DisplayStyle.Flex : DisplayStyle.None);

            EnableInClassList(EmptyClass, !hasValue);
            _foldout.SetValueWithoutNotify(hasValue && _property.isExpanded);

            UpdateMissingBox();

            if (forceRebuild || !_contentBuilt || currentType != _currentType)
            {
                _currentType = currentType;
                RebuildContent(hasValue);
            }
        }

        private void RebuildContent(bool hasValue)
        {
            _content.Clear();
            _contentBuilt = true;
            if (!hasValue) return;

            var iterator = _property.Copy();
            var end = _property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                var child = iterator.Copy();
                var field = new PropertyField(child);
                field.BindProperty(child);

                _content.Add(field);
            }
        }

        private void UpdateMissingBox()
        {
            if (!SerializeReferenceHelpers.IsMissingType(_property))
            {
                _missingBox?.RemoveFromHierarchy();
                return;
            }

            _missingBox ??= new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Warning));
            _missingBox.Message = $"Missing type: {_property.managedReferenceFullTypename}";

            if (_missingBox.parent is null) this.AddChild(_missingBox);
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            if (evt.target != _foldout) return;
            _property.isExpanded = evt.newValue;
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (evt.button is not 0) return;

            var window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;

            if (!window) return;

            var currentType = SerializeReferenceHelpers.GetCurrentType(_property);

            TypeSelectorWindow.Show(
                screenRect: GetScreenRect(),
                types: _types,
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName =>
                {
                    var selectedType = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    _property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstance(selectedType));
                    _property.isExpanded = selectedType is not null;
                    Refresh(forceRebuild: true);
                },
                filter: SerializeReferenceHelpers.IsAssignableManagedReference);

            evt.StopPropagation();
            return;

            Rect GetScreenRect() => new(
                window.position.x + _dropdown.worldBound.xMin,
                window.position.y + _dropdown.worldBound.yMin,
                _dropdown.worldBound.width,
                _dropdown.worldBound.height);
        }

        private string GetCaption(Type currentType)
        {
            if (currentType is not null)
                return TypeSelectorHelpers.GetTypeSelectorTitle(currentType);

            var missingName = SerializeReferenceHelpers.IsMissingType(_property)
                ? _property.managedReferenceFullTypename
                : null;

            return TypeSelectorHelpers.GetTypeSelectorTitle(null, missingName);
        }
    }
}
