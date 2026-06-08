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

        // Unity's BaseField input class — applied to the dropdown's inner input so it picks up the
        // same flex/indent the EnumField theme rules target on a real field's visualInput.
        private const string BaseFieldInputClass = "unity-base-field__input";

        // Small gap kept between the value column and the dropdown's left edge.
        private const float DropdownGap = 2f;

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
        private float _arrowInset = float.NaN;

        public SerializeReferenceField(string label, SerializedProperty property)
        {
            _property = property;
            _types = new[] { SerializeReferenceHelpers.GetFieldType(_property) };

            this.AddClass(BlockClass)
                .AddClass(PropertyField.ussClassName)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet);

            _foldout = new Foldout();
            _foldout.RegisterValueChangedCallback(OnFoldoutToggled);
            _content = _foldout.contentContainer;

            _caption = new TextElement()
                .AddClass(EnumField.textUssClassName)
                .SetPickingMode(PickingMode.Ignore);

            // Mirror SerializableType's TypeField structure: an enum-field "root" wrapping a separate
            // "__input" child. Unity's theme indents the caption through descendant selectors
            // (".unity-enum-field .unity-enum-field__input"), which only match when the input is a
            // child of the field — collapsing both classes onto one element drops that indent.
            var dropdownInput = new VisualElement()
                .AddClass(BaseFieldInputClass)
                .AddClass(EnumField.inputUssClassName)
                .AddChild(_caption)
                .AddChild(new VisualElement()
                    .AddClass(EnumField.arrowUssClassName)
                    .SetPickingMode(PickingMode.Ignore));

            _dropdown = new VisualElement()
                .AddClass(EnumField.ussClassName)
                .AddClass(DropdownClass)
                .AddChild(dropdownInput);

            _dropdown.RegisterCallback<PointerDownEvent>(OnDropdownClicked);

            _openButton = new Button()
                .AddChild(new VisualElement())
                .AddClicked(() => SerializeReferenceHelpers.GetCurrentType(_property)?.OpenInScriptEditor());

            // Carry the foldout caption on the toggle's BaseField label and opt into Unity's
            // inspector field alignment so the label width tracks the value column exactly as
            // SerializableType does (see InspectorTypeField). The expand arrow stays on the far
            // left; the dropdown is then offset by the arrow width so it begins at the value column.
            var toggle = _foldout.Q<Toggle>();
            toggle.AddClass(BaseField<bool>.alignedFieldUssClassName);
            toggle.labelElement.AddClass(PropertyField.labelUssClassName);
            toggle.label = label;

            var arrow = toggle.Q(className: Foldout.inputUssClassName);
            toggle.Insert(0, arrow);
            arrow.RegisterCallback<GeometryChangedEvent>(OnArrowGeometryChanged);

            toggle.AddChild(_dropdown)
                .AddChild(_openButton);

            // Copy/Paste lives on the header only — child PropertyFields keep their own contextual menus.
            toggle.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));

            this.AddChild(_foldout);

            Refresh(forceRebuild: true);
            this.TrackPropertyValue(_property, _ => Refresh(forceRebuild: false));
        }

        // The arrow sits in-flow before the aligned label, so the label (and the dropdown that
        // follows it) overshoot the value column by the arrow's width. Pull the dropdown back by
        // that measured width so its left edge lands on the value column at any nesting depth.
        private void OnArrowGeometryChanged(GeometryChangedEvent evt)
        {
            var inset = ((VisualElement)evt.target).resolvedStyle.width;
            if (Mathf.Approximately(inset, _arrowInset)) return;

            _arrowInset = inset;
            _dropdown.style.marginLeft = DropdownGap - inset;
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
            var fieldType = _types.Length > 0 ? _types[0] : typeof(object);
            var screenRect = GetScreenRect();

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: _types,
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)),
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(fieldType),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);

            evt.StopPropagation();
            return;

            void Apply(Type type)
            {
                var previous = _property.managedReferenceValue;
                _property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                _property.isExpanded = type is not null;
                Refresh(forceRebuild: true);
            }

            Rect GetScreenRect() => new(
                window.position.x + _dropdown.worldBound.xMin,
                window.position.y + _dropdown.worldBound.yMin,
                _dropdown.worldBound.width,
                _dropdown.worldBound.height);
        }

        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Copy Serialize Reference",
                _ => SerializeReferenceClipboard.Copy(_property.managedReferenceValue));

            var fieldType = _types.Length > 0 ? _types[0] : typeof(object);
            var canPaste = SerializeReferenceClipboard.CanPasteInto(fieldType);

            evt.menu.AppendAction("Paste Serialize Reference",
                _ => PasteFromClipboard(),
                canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        }

        private void PasteFromClipboard()
        {
            var value = SerializeReferenceClipboard.CreateInstance();
            _property.SetManagedReferenceAndApply(value);
            _property.isExpanded = value is not null;
            Refresh(forceRebuild: true);
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
