using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// UIToolkit field that displays an <see cref="IId"/>-style integer id as an EnumField-style
    /// dropdown backed by <see cref="IdSelectorDropdownWindow"/>. Optionally bound to an
    /// <see cref="IId"/> struct <see cref="SerializedProperty"/> whose generated children
    /// (<c>_id</c> and <c>__stringId</c>) are updated together; when no <see cref="IdRegistry"/>
    /// is bound to <see cref="IdType"/> or the id cannot be resolved to a name, the field renders
    /// a <c>&lt;Missing&gt;</c> caption instead of silently clearing.
    /// </summary>
    /// <remarks>
    /// Designed to be inheritable so subclasses (e.g. <see cref="InspectorIdField"/>) can layer
    /// Inspector-specific styling on top of the base behaviour. Set <see cref="IdType"/> to the
    /// id struct type that selects the registry — without it the dropdown does not open.
    /// </remarks>
    [UxmlElement]
    public partial class IdField : BaseField<int>
    {
        private const string StyleSheetPath = "UI/Ids/Aspid-FastTools-Id-Field";

        private readonly Button _openButton;
        private readonly TextElement _textElement;
        private readonly VisualElement _visualInput;
        private readonly DynamicSerializeProperty _intProperty;
        private readonly DynamicSerializeProperty _stringProperty;

        private string _currentName = string.Empty;
        private Type _idType;

        /// <summary>
        /// Id struct type — selects the <see cref="IdRegistry"/> via
        /// <see cref="IdRegistryResolver.Find"/>. The dropdown is disabled while this is <c>null</c>.
        /// Setting this refreshes the rendered caption against the (possibly newly available) registry.
        /// </summary>
        public Type IdType
        {
            get => _idType;
            set
            {
                if (_idType == value) return;
                _idType = value;
                UpdateDisplay();
            }
        }

        public IdField()
            : this(label: null) { }

        public IdField(SerializedProperty property)
            : this(property.displayName, property) { }

        public IdField(string label, SerializedProperty property)
            : this(label)
        {
            var intProp = property.FindPropertyRelative(Constants.IntIdFieldName);
            var stringProp = property.FindPropertyRelative(Constants.StringIdFieldName);

            _intProperty = new DynamicSerializeProperty(intProp);
            _stringProperty = new DynamicSerializeProperty(stringProp);

            _currentName = stringProp.stringValue ?? string.Empty;
            base.SetValueWithoutNotify(intProp.intValue);
            UpdateDisplay();
        }

        public IdField(string label, int defaultValue = 0)
            : this(label, visualInput: new VisualElement(), defaultValue) { }

        private IdField(string label, VisualElement visualInput, int defaultValue)
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
                    .SetPickingMode(PickingMode.Ignore)
                );

            visualInput.RegisterCallback<PointerDownEvent>(OnDropdownClicked);

            _openButton = new Button()
                .AddChild(new VisualElement())
                .AddClicked(OpenRegistryAsset);

            this.AddChild(_openButton);
            SetValueWithoutNotify(defaultValue);
        }

        /// <inheritdoc/>
        public sealed override void SetValueWithoutNotify(int newValue)
        {
            if (IdType is not null)
            {
                var registry = IdRegistryResolver.Find(IdType);
                if (registry is not null && registry.TryGetName(newValue, out var resolved))
                    _currentName = resolved;
            }

            base.SetValueWithoutNotify(newValue);
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the field value from a registry name without raising a change event.
        /// If the name cannot be resolved (or <see cref="IdType"/> is <c>null</c>), the original
        /// string is preserved so the field can render a <c>&lt;Missing&gt;</c> caption instead
        /// of silently clearing.
        /// </summary>
        public void SetValueFromNameWithoutNotify(string nameId)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(nameId) && IdType is not null)
            {
                var registry = IdRegistryResolver.Find(IdType);
                if (registry is not null && registry.TryGetId(nameId, out var found))
                    id = found;
            }

            _currentName = nameId ?? string.Empty;
            base.SetValueWithoutNotify(id);
            UpdateDisplay();
        }

        /// <summary>
        /// Re-reads the bound <see cref="SerializedProperty"/> (when constructed from one)
        /// and refreshes both the rendered caption and the cached name. No-op for unbound fields.
        /// </summary>
        public void RefreshFromBoundProperty()
        {
            var intProp = _intProperty?.GetProperty();
            if (intProp is null) return;

            _currentName = _stringProperty?.GetProperty()?.stringValue ?? string.Empty;
            base.SetValueWithoutNotify(intProp.intValue);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var registry = IdType is not null ? IdRegistryResolver.Find(IdType) : null;
            var caption = IsStructDrawerHelper.BuildCaption(registry, value, _currentName, out var isMissing);

            _textElement.SetText(caption);
            _textElement.EnableInClassList(StatusStyle.ErrorClass, isMissing);
            _openButton.SetDisplay(registry is not null ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OpenRegistryAsset()
        {
            if (IdType is null) return;

            var registry = IdRegistryResolver.Find(IdType);
            if (registry is null) return;

            EditorGUIUtility.PingObject(registry);
            Selection.activeObject = registry;
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (evt.button is not 0) return;
            if (IdType is null) return;

            var window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;

            if (!window) return;

            IdSelectorDropdownWindow.Show(
                screenRect: GetScreenRect(),
                idType: IdType,
                currentName: _currentName,
                onSelected: nameId =>
                {
                    var resolvedId = 0;
                    var resolvedName = nameId ?? string.Empty;

                    if (!string.IsNullOrEmpty(resolvedName))
                    {
                        var registry = IdRegistryResolver.Find(IdType);
                        if (registry is not null && registry.TryGetId(resolvedName, out var found))
                            resolvedId = found;
                    }

                    _currentName = resolvedName;
                    this.SetValue(resolvedId);

                    _intProperty?.GetProperty()?.SetIntAndApply(resolvedId);
                    _stringProperty?.GetProperty()?.SetStringAndApply(resolvedName);
                });

            evt.StopPropagation();
            return;

            Rect GetScreenRect() => new(
                window.position.x + _visualInput.worldBound.xMin,
                window.position.y + _visualInput.worldBound.yMin,
                _visualInput.worldBound.width,
                _visualInput.worldBound.height);
        }
    }
}
