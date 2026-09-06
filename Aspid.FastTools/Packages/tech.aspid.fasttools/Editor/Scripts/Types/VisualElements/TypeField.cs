using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// UIToolkit field showing a <see cref="Type"/> as a dropdown backed by <see cref="TypeSelectorWindow"/>,
    /// optionally bound to a string property holding the type's assembly-qualified name.
    /// </summary>
    /// <remarks>
    /// An unresolved name is preserved and rendered as a <c>&lt;Missing&gt;</c> caption rather than silently
    /// cleared. Inheritable, so a subclass can layer its own styling on top.
    /// </remarks>
    [UxmlElement]
    public partial class TypeField : BaseField<Type>
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-SerializableType";

        private readonly Button _openButton;
        private readonly TextElement _textElement;
        private readonly VisualElement _visualInput;
        private readonly SerializedProperty _property;

        private bool _isReadOnly;
        private string _missingAssemblyQualifiedName;

        /// <summary>
        /// Gets or sets which kinds of types can be picked.
        /// </summary>
        [UxmlAttribute]
        public TypeAllow Allow { get; set; } = TypeAllow.None;

        /// <summary>
        /// Gets or sets the base types; the dropdown lists only types assignable to every one of them.
        /// </summary>
        public Type[] Types { get; set; } = { typeof(object) };

        /// <summary>
        /// Gets or sets the predicate applied to each candidate after the <see cref="Types"/> and
        /// <see cref="Allow"/> checks. <see langword="null"/> keeps every matching type.
        /// </summary>
        public Func<Type, bool> Predicate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <c>&lt;None&gt;</c> row is left out, for a field whose value
        /// must never be cleared.
        /// </summary>
        [UxmlAttribute]
        public bool HideNoneOption { get; set; }

        /// <summary>
        /// Creates an unbound field without a label.
        /// </summary>
        public TypeField()
            : this(label: null) { }

        /// <summary>
        /// Creates a field bound to <paramref name="property"/>, labeled with its display name.
        /// </summary>
        /// <param name="property">A string property holding the assembly-qualified type name.</param>
        public TypeField(SerializedProperty property)
            : this(property.displayName, property) { }

        /// <summary>
        /// Creates a bound field: the picked type's name is written to the property, and external edits are
        /// reflected back into the field.
        /// </summary>
        /// <param name="label">The field label; <see langword="null"/> for none.</param>
        /// <param name="property">A string property holding the assembly-qualified type name.</param>
        public TypeField(string label, SerializedProperty property)
            : this(label)
        {
            _property = property.Persistent();
            SetValueFromAssemblyQualifiedNameWithoutNotify(_property.stringValue);

            // Undo, revert-to-prefab and scripted edits rewrite the backing string outside this field; the tracked
            // callback hands over a fresh property each tick.
            this.TrackPropertyValue(_property, current =>
                SetValueFromAssemblyQualifiedNameWithoutNotify(current.stringValue));
        }

        /// <summary>
        /// Creates an unbound field; the picked type is reported only through the change event.
        /// </summary>
        /// <param name="label">The field label; <see langword="null"/> for none.</param>
        /// <param name="defaultValue">The type shown initially, or <see langword="null"/> for <c>&lt;None&gt;</c>.</param>
        public TypeField(string label, Type defaultValue = null)
            : this(label, visualInput: new VisualElement(), defaultValue) { }

        private TypeField(string label, VisualElement visualInput, Type defaultValue)
            : base(label, visualInput)
        {
            this.AddClass(EnumField.ussClassName)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();
            
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
                .AddClicked(() => value.OpenInScriptEditor());

            this.AddChild(_openButton);
            SetValueWithoutNotify(defaultValue);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dropdown is disabled, so the displayed type cannot be
        /// changed.
        /// </summary>
        /// <remarks>The open-in-script-editor button stays active.</remarks>
        [UxmlAttribute]
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                _visualInput.SetEnabled(!value);
            }
        }

        /// <inheritdoc/>
        public sealed override void SetValueWithoutNotify(Type newValue)
        {
            _missingAssemblyQualifiedName = null;
            base.SetValueWithoutNotify(newValue);
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the value from an assembly-qualified type name without raising a change event.
        /// </summary>
        /// <remarks>
        /// A name that cannot be resolved is preserved, so the field renders a <c>&lt;Missing&gt;</c> caption
        /// instead of silently clearing.
        /// </remarks>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the type to show.</param>
        public void SetValueFromAssemblyQualifiedNameWithoutNotify(string assemblyQualifiedName)
        {
            var resolved = TypeUtility.GetTypeOrNull(assemblyQualifiedName);

            _missingAssemblyQualifiedName = resolved is null && !string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? assemblyQualifiedName
                : null;

            base.SetValueWithoutNotify(resolved);
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            _textElement.SetText(TypeSelectorHelpers.GetTypeSelectorTitle(value, _missingAssemblyQualifiedName));
            _openButton.SetDisplay(value is not null ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (_isReadOnly || evt.button is not 0) return;

            var window = _visualInput.GetOwnerWindow();
            if (!window) return;

            var filter = new TypeSelectorFilter
            {
                Types = Types,
                Allow = Allow,
                Predicate = Predicate,
                HideNoneOption = HideNoneOption,
            };

            TypeSelectorWindow.Show(
                screenRect: GetScreenRect(),
                filter: filter,
                currentAqn: value?.AssemblyQualifiedName ?? _missingAssemblyQualifiedName ?? string.Empty,
                onSelected: assemblyQualifiedName =>
                {
                    this.SetValue(TypeUtility.GetTypeOrNull(assemblyQualifiedName));

                    // <None> arrives as null; the IMGUI path stores an empty string, so this one does too.
                    _property?.SetStringAndApply(assemblyQualifiedName ?? string.Empty);
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
