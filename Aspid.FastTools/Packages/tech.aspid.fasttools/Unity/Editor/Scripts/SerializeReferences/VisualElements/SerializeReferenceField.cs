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
    /// Always bound to a managed-reference <see cref="SerializedProperty"/>; created by
    /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorPropertyDrawer"/>, not from UXML. The field keeps the live
    /// inspector property so child fields round-trip through Unity's binding (apply/Undo) and only rebuilds
    /// the nested properties when the assigned type actually changes.
    /// </remarks>
    internal sealed class SerializeReferenceField : VisualElement
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        private const string BlockClass = "aspid-fasttools-serialize-reference";
        private const string EmptyClass = BlockClass + "--empty";
        private const string MixedClass = BlockClass + "--mixed";
        private const string DropdownClass = BlockClass + "__dropdown";

        // Unity's mixed-value class — applied to the dropdown so the EnumField theme shows the standard "—" treatment
        // when the selected targets hold different managed-reference types under a multi-object selection.
        private const string MixedValueClass = "unity-base-field--show-mixed-value";

        // The caption Unity shows for a mixed (multiple-different-values) field.
        private const string MixedCaption = "—";

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
        private readonly Type _fieldType;
        private readonly Type[] _baseTypes;
        private readonly Func<Type, bool> _filter;

        private SerializeReferenceNotice _missingNotice;
        private SerializeReferenceNotice _sharedNotice;
        private SerializeReferenceNotice _mixedNotice;
        private Type _currentType;
        private bool _contentBuilt;
        private bool _mixedTypes;
        private float _arrowInset = float.NaN;

        public SerializeReferenceField(string label, SerializedProperty property, Type[] baseTypes = null)
        {
            _property = property;
            _fieldType = SerializeReferenceHelpers.GetFieldType(_property);
            _baseTypes = baseTypes;
            _filter = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes);

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
            // A saved-asset repair reimports the asset and invalidates this property's SerializedObject; the inspector
            // is rebuilt from a fresh selection instead, so a stale property here must no-op rather than throw.
            if (!IsPropertyAlive()) return;

            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(_property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(_property);
            var hasValue = currentType is not null;

            // With mixed types the foldout would expose only the first target's children, which do not represent the
            // selection; collapse it and key off the dim "different types" hint instead of the per-instance fields.
            _caption.SetText(mixedTypes ? MixedCaption : GetCaption(currentType));
            _caption.tooltip = mixedTypes ? "Mixed — the selected objects hold different types." : null;
            _openButton.SetDisplay(hasValue && !mixedTypes ? DisplayStyle.Flex : DisplayStyle.None);

            _dropdown.EnableInClassList(MixedValueClass, mixedTypes);
            EnableInClassList(MixedClass, mixedTypes);
            EnableInClassList(EmptyClass, !hasValue && !mixedTypes);
            _foldout.SetValueWithoutNotify(hasValue && !mixedTypes && _property.isExpanded);

            UpdateMissingBox();
            UpdateSharedBox();
            UpdateMixedBox(mixedTypes);

            // A mixed selection never renders child fields — Unity's per-field multi-edit cannot merge fields of
            // different types — so the content is cleared and rebuilt only when the (shared) type actually changes.
            var rebuild = forceRebuild || !_contentBuilt || currentType != _currentType || mixedTypes != _mixedTypes;
            if (rebuild)
            {
                _currentType = currentType;
                _mixedTypes = mixedTypes;
                RebuildContent(hasValue && !mixedTypes);
            }
        }

        // The property's SerializedObject can be torn down out from under this field (e.g. a saved-asset repair
        // reimports the asset); probing the target object reports that without throwing on the dangling handle.
        private bool IsPropertyAlive()
        {
            try { return _property.serializedObject?.targetObject != null; }
            catch (Exception) { return false; }
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

        private void UpdateMixedBox(bool mixedTypes)
        {
            if (!mixedTypes)
            {
                _mixedNotice?.RemoveFromHierarchy();
                return;
            }

            _mixedNotice ??= new SerializeReferenceNotice();
            if (_mixedNotice.parent is null) this.AddChild(_mixedNotice);

            // Stands in for the per-instance child fields, which cannot be merged across different types. Keep it
            // terse and non-actionable: selecting a single object restores its own field, or picking a type from the
            // dropdown rewrites every target to that one type.
            _mixedNotice.SetInfo(
                message: "Different types selected",
                detail: "The selected objects hold different managed-reference types, so their fields cannot be shown " +
                        "together.\nPick a type from the dropdown to set it on all of them, or select a single object " +
                        "to edit its own fields.");
        }

        private void UpdateMissingBox()
        {
            // Missing-type detection reads the first target's backing asset YAML and its repair rewrites that one file,
            // so the notice is meaningless (and potentially misleading) under a multi-object selection — suppress it.
            if (!SerializeReferenceHelpers.NoticesApply(_property) ||
                !SerializeReferenceHelpers.IsMissingType(_property))
            {
                _missingNotice?.RemoveFromHierarchy();
                return;
            }

            _missingNotice ??= new SerializeReferenceNotice();
            if (_missingNotice.parent is null) this.AddChild(_missingNotice);

            var typeName = SerializeReferenceHelpers.GetMissingTypeDisplayName(_property);

            // Offered for saved assets (YAML rewrite) and Prefab Mode objects (in-memory) — anything with a
            // resolvable backing document. Scene objects without one fall through to the read-only hint.
            var canFix = SerializeReferenceHelpers.TryGetRepairLocation(_property, out _, out _, out _);

            _missingNotice.Set(
                message: "Missing type —",
                actionText: canFix ? "Fix" : string.Empty,
                detail: canFix
                    ? $"Missing type: {typeName}.\nClick Fix to re-point this reference to an existing type, keeping its data."
                    : $"Missing type: {typeName}.\nOpen this asset from the Project window to repair it.",
                onAction: OpenFixSelector);

            UpdateSuggestion(canFix);
        }

        // The Smart Fix suggestion rides the missing notice as a second clickable segment ("· → Pistol?"): the highest
        // ranked existing type the renamed/moved reference most likely became. It is offered only where the field can
        // be repaired at all; clicking it applies the candidate directly through the same repair path as a manual Fix.
        private void UpdateSuggestion(bool canFix)
        {
            if (!canFix ||
                !SerializeReferenceHelpers.TryGetRepairSuggestion(_property, _baseTypes, out var suggestion))
            {
                _missingNotice.SetSuggestion(string.Empty, null, null);
                return;
            }

            _missingNotice.SetSuggestion(
                suggestionText: SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                detail: SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                onSuggestion: () => ApplySuggestion(suggestion.Type));
        }

        private void ApplySuggestion(Type suggestedType)
        {
            if (SerializeReferenceHelpers.TryFixMissingType(_property, suggestedType))
                Refresh(forceRebuild: true);
        }

        private void UpdateSharedBox()
        {
            // The shared-reference scan compares managedReferenceId only within the first target's SerializedObject and
            // Make-unique rewrites that one object, so neither generalises to a multi-object selection — suppress it.
            if (!SerializeReferenceHelpers.NoticesApply(_property) ||
                !SerializeReferenceHelpers.HasSharedReference(_property))
            {
                _sharedNotice?.RemoveFromHierarchy();
                return;
            }

            _sharedNotice ??= new SerializeReferenceNotice();
            if (_sharedNotice.parent is null) this.AddChild(_sharedNotice);

            _sharedNotice.Set(
                message: "Shared reference —",
                actionText: "Make unique",
                detail: "This reference is shared with another field — editing one changes both.\n" +
                        "Click Make unique to give this field its own independent copy.",
                onAction: MakeUnique);
        }

        private void OpenFixSelector()
        {
            var window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;
            if (!window) return;

            var bound = _missingNotice.worldBound;
            var screenRect = new Rect(
                window.position.x + bound.xMin,
                window.position.y + bound.yMax,
                bound.width,
                bound.height);

            SerializeReferenceHelpers.ShowFixTypeSelector(_property, screenRect, () => Refresh(forceRebuild: true), _baseTypes);
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

            // Under mixed types there is no single "current" type to pre-highlight in the picker — open it unselected.
            var currentType = SerializeReferenceHelpers.HasMixedTypes(_property)
                ? null
                : SerializeReferenceHelpers.GetCurrentType(_property);
            var screenRect = GetScreenRect();

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { _fieldType },
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)),
                filter: _filter,
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(_fieldType, _baseTypes),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);

            evt.StopPropagation();
            return;

            void Apply(Type type)
            {
                // Multi-object: each target gets its OWN instance, created from that target's previous value, so the
                // managed reference is never aliased across objects; <None> clears all. One Undo step covers them all.
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(_property))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        _property,
                        previous => SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));

                    // After a per-target pick the selection shares the new type, so the live foldout drives expansion;
                    // set it on the live property (the per-target writes went through disposed SerializedObjects).
                    _property.isExpanded = type is not null;
                }
                else
                {
                    var previous = _property.managedReferenceValue;
                    _property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                    _property.isExpanded = type is not null;
                }

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
            // Copy reads the first target's value (Unity's own convention for a multi-selection menu). Paste then
            // applies an independent instance PER target, so the pasted reference is never aliased across objects.
            evt.menu.AppendAction("Copy Serialize Reference",
                _ => SerializeReferenceClipboard.Copy(_property.managedReferenceValue));

            var canPaste = SerializeReferenceClipboard.CanPasteInto(_fieldType);

            evt.menu.AppendAction("Paste Serialize Reference",
                _ => PasteFromClipboard(),
                canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            // Make-unique is a single-asset cross-reference operation; it is only offered (and only correct) for a
            // single target — under a multi-object selection the shared-reference notice is already suppressed.
            if (SerializeReferenceHelpers.NoticesApply(_property) &&
                SerializeReferenceHelpers.HasSharedReference(_property))
                evt.menu.AppendAction("Make Unique Reference", _ => MakeUnique());
        }

        private void MakeUnique()
        {
            SerializeReferenceHelpers.MakeReferenceUnique(_property);
            Refresh(forceRebuild: true);
        }

        private void PasteFromClipboard()
        {
            // Multi-object: rebuild a fresh instance from the clipboard for EACH target so no two objects share the same
            // managed reference (CreateInstance already returns an independent instance per call). One Undo step covers all.
            if (SerializeReferenceHelpers.IsEditingMultipleObjects(_property))
            {
                SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                    _property,
                    _ => SerializeReferenceClipboard.CreateInstance());

                // All targets now share the pasted type, so the live foldout drives expansion; set it on the live
                // property (the per-target writes went through disposed SerializedObjects). Type is null only on an
                // empty-reference paste, which collapses the foldout — matching the single-object branch.
                _property.isExpanded = SerializeReferenceClipboard.Type is not null;
            }
            else
            {
                var value = SerializeReferenceClipboard.CreateInstance();
                _property.SetManagedReferenceAndApply(value);
                _property.isExpanded = value is not null;
            }

            Refresh(forceRebuild: true);
        }

        private string GetCaption(Type currentType)
        {
            if (currentType is not null)
                return TypeSelectorHelpers.GetTypeSelectorTitle(currentType);

            var missingName = SerializeReferenceHelpers.IsMissingType(_property)
                ? SerializeReferenceHelpers.GetMissingTypeDisplayName(_property)
                : null;

            return TypeSelectorHelpers.GetTypeSelectorTitle(null, missingName);
        }
    }
}
