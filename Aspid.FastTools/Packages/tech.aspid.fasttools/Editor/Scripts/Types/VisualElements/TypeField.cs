using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Object = UnityEngine.Object;
using Aspid.FastTools.UIElements.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// <see cref="BaseField{TValueType}"/> of <see cref="Type"/> for selecting a type and optionally storing its assembly-qualified name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// UXML creates an unbound field: it ignores <c>binding-path</c>, and <see cref="Types"/> and <see cref="Predicate"/>
    /// have no UXML attributes. Create a bound or constrained field in C#.
    /// </para>
    /// <para>Not designed for inheritance outside the package.</para>
    /// </remarks>
    [UxmlElement]
    public partial class TypeField : BaseField<Type>
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-TypeField";

        private const string OpenIconClass = "aspid-fasttools-type-field__open-icon";
        private const string OpenButtonClass = "aspid-fasttools-type-field__open-button";
        private const string MissingTextClass = "aspid-fasttools-type-field__text--missing";

        private readonly Button _openButton;
        private readonly TextElement _textElement;
        private readonly VisualElement _visualInput;
        private readonly SerializedProperty _property;
        private readonly SerializedObject _sourceSerializedObject;

        private bool _isReadOnly;
        private string _shownPropertyValue;
        private string _missingAssemblyQualifiedName;
        private Type[] _types = { typeof(object) };

        /// <summary>
        /// Gets or sets which kinds of types can be picked.
        /// </summary>
        /// <remarks>
        /// <see cref="TypeAllow.None"/> by default, so only concrete types are offered, unlike
        /// <see cref="TypeSelectorAttribute.Allow"/>, which defaults to <see cref="TypeAllow.All"/>.
        /// </remarks>
        [UxmlAttribute]
        public TypeAllow Allow { get; set; } = TypeAllow.None;

        /// <summary>
        /// Gets or sets the base types; the dropdown lists only types assignable to every one of them.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> entries are dropped. For a closed generic type such as <c>IHandler&lt;int&gt;</c>,
        /// the dropdown also offers the generic classes that close to it, such as <c>Handler&lt;int&gt;</c>.
        /// </remarks>
        public Type[] Types
        {
            get => _types;
            set => _types = value?.Where(type => type is not null).ToArray();
        }

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
        /// Gets or sets a value indicating whether types from editor-only assemblies are left out, for a value that a
        /// player build must resolve.
        /// </summary>
        /// <remarks>A bound field turns it on when its property belongs to an object of a runtime assembly.</remarks>
        [UxmlAttribute]
        public bool ExcludeEditorOnlyTypes { get; set; }

        /// <summary>
        /// Gets the assembly-qualified name of the selected type, or the stored name of a missing type; an empty
        /// string when no type is selected.
        /// </summary>
        public string AssemblyQualifiedName =>
            value?.AssemblyQualifiedName ?? _missingAssemblyQualifiedName ?? string.Empty;

        /// <summary>
        /// Gets or sets the selected type, clearing any unresolved name and notifying listeners when it is cleared.
        /// </summary>
        public sealed override Type value
        {
            get => base.value;
            set
            {
                if (value is not null || rawValue is not null || _missingAssemblyQualifiedName is null || showMixedValue)
                {
                    base.value = value;
                    return;
                }

                // Missing and None both resolve to null, but clearing the stored name is still a change.
                SetValueWithoutNotify(newValue: null);
                if (panel is null) return;

                using var evt = ChangeEvent<Type>.GetPooled(previousValue: null, newValue: null);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Creates an unbound field without a label.
        /// </summary>
        public TypeField()
            : this(label: null) { }

        /// <summary>
        /// Creates a field bound to <paramref name="property"/>, labeled with its display name.
        /// </summary>
        /// <param name="property">A string property holding the assembly-qualified type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="property"/> is not a string property, or its path no longer exists on its targets.
        /// </exception>
        public TypeField(SerializedProperty property)
            : this(property?.displayName, property) { }

        /// <summary>
        /// Creates a bound field: the picked type's name is written to the property, and external edits are
        /// reflected back into the field.
        /// </summary>
        /// <remarks>
        /// Before <see cref="ChangeEvent{T}"/> is sent, a pick writes the property and calls
        /// <see cref="SerializedObject.Update"/> on the <see cref="SerializedObject"/> of <paramref name="property"/>:
        /// a change handler reads the picked name from <paramref name="property"/>, and edits it applies through that
        /// object keep the pick. Changes not yet applied to that object are lost.
        /// </remarks>
        /// <param name="label">The field label; <see langword="null"/> for none.</param>
        /// <param name="property">A string property holding the assembly-qualified type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="property"/> is not a string property, or its path no longer exists on its targets.
        /// </exception>
        public TypeField(string label, SerializedProperty property)
            : this(label)
        {
            if (property is null) throw new ArgumentNullException(nameof(property));

            if (property.propertyType is not SerializedPropertyType.String)
                throw new ArgumentException(
                    $"TypeField binds only to a string property; \"{property.propertyPath}\" is {property.propertyType}.",
                    nameof(property));

            _property = property.Persistent() ?? throw new ArgumentException(
                $"The property path \"{property.propertyPath}\" no longer exists on its targets.", nameof(property));
            _sourceSerializedObject = property.serializedObject;

            ExcludeEditorOnlyTypes = TypeSelectorHelpers.IsStoredInRuntimeObject(_property);
            RefreshProperty(property: _property);

            this.TrackPropertyValue(_property, RefreshProperty);
            this.AddContextualMenuManipulator(evt => AddPrefabOverrideActions(evt.menu));

            // Unity's trackers can miss edits to later targets when the first target's value stays the same.
            if (_property.serializedObject.isEditingMultipleObjects)
                schedule.Execute(() => PollProperty()).Every(intervalMs: 100).Until(HasDestroyedTarget);
        }

        /// <summary>
        /// Creates an unbound field without an initial type; the picked type is reported only through the change event.
        /// </summary>
        /// <param name="label">The field label; <see langword="null"/> for none.</param>
        public TypeField(string label)
            : this(label, defaultValue: null) { }

        /// <summary>
        /// Creates an unbound field; the picked type is reported only through the change event.
        /// </summary>
        /// <param name="label">The field label; <see langword="null"/> for none.</param>
        /// <param name="defaultValue">The type shown initially, or <see langword="null"/> for <c>&lt;None&gt;</c>.</param>
        public TypeField(string label, Type defaultValue)
            : this(label, visualInput: new VisualElement(), defaultValue) { }

        private TypeField(string label, VisualElement visualInput, Type defaultValue)
            : base(label, visualInput)
        {
            this.AddClass(EnumField.ussClassName)
                .AddStyleSheetFromResources(StyleSheetPath)
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
            visualInput.RegisterCallback<NavigationSubmitEvent>(OnDropdownSubmitted);

            _openButton = new Button()
                .AddClass(OpenButtonClass)
                .SetTooltip("Open the script")
                .AddChild(new VisualElement()
                    .AddClass(OpenIconClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddClicked(() => value.OpenInScriptEditor());

            this.AddChild(_openButton);
            RegisterCallback<AttachToPanelEvent>(WarnAboutBindingPath);
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

        /// <summary>
        /// Sets the displayed type and clears any unresolved name without raising a change event.
        /// </summary>
        /// <param name="newValue">The type to show, or <see langword="null"/> for an empty selection.</param>
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
        /// <param name="assemblyQualifiedName">The type name, or <see langword="null"/> or whitespace for an empty selection.</param>
        public void SetValueFromAssemblyQualifiedNameWithoutNotify(string assemblyQualifiedName)
        {
            var resolved = TypeUtility.GetTypeOrNull(assemblyQualifiedName);

            _missingAssemblyQualifiedName = resolved is null && !string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? assemblyQualifiedName
                : null;

            base.SetValueWithoutNotify(resolved);
            UpdateDisplay();
        }

        /// <summary>
        /// Called when the mixed-value state changes to refresh the caption and script button.
        /// </summary>
        protected sealed override void UpdateMixedValueContent() => UpdateDisplay();

        private void RefreshProperty(SerializedProperty property)
        {
            property.serializedObject.SetIsDifferentCacheDirty();
            property.serializedObject.Update();
            ShowProperty(property);
        }

        private void ShowProperty(SerializedProperty property)
        {
            _shownPropertyValue = property.stringValue;
            SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: _shownPropertyValue);
            showMixedValue = property.hasMultipleDifferentValues;
            EnableInClassList(className: BindingExtensions.prefabOverrideUssClassName, enable: IsPrefabOverride(property));
        }

        // Repaints only when a target changed, so an idle multi-object field does not rebuild its caption every tick.
        internal bool PollProperty()
        {
            if (HasDestroyedTarget()) return false;

            var serializedObject = _property.serializedObject;
            serializedObject.SetIsDifferentCacheDirty();
            serializedObject.Update();

            if (_property.stringValue == _shownPropertyValue && _property.hasMultipleDifferentValues == showMixedValue)
                return false;

            ShowProperty(_property);
            return true;
        }

        private bool HasDestroyedTarget() => Array.Exists(_property.serializedObject.targetObjects, target => target == null);

        private void UpdateDisplay()
        {
            var isMissing = _missingAssemblyQualifiedName is not null && !showMixedValue;

            // The tooltip lives on the dropdown, not the caption: the caption ignores picking, so it can never anchor one.
            _visualInput.tooltip = showMixedValue
                ? null
                : isMissing
                    ? $"Missing type: {_missingAssemblyQualifiedName}"
                    : TypeSelectorHelpers.GetTypeSelectorTooltip(value);

            _textElement
                .EnableClass(className: MissingTextClass, enable: isMissing)
                .EnableClass(className: mixedValueLabelUssClassName, enable: showMixedValue)
                .SetTextSelf(value: showMixedValue
                    ? mixedValueString
                    : TypeSelectorHelpers.GetTypeSelectorTitle(value,
                        assemblyQualifiedName: TypeSelectorHelpers.GetMissingDisplayName(_missingAssemblyQualifiedName)));

            _openButton.SetDisplay(value is not null && !showMixedValue ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (_isReadOnly || evt.button is not 0) return;

            ShowSelector();
            evt.StopPropagation();
        }

        private void OnDropdownSubmitted(NavigationSubmitEvent evt)
        {
            if (_isReadOnly || !enabledInHierarchy) return;

            ShowSelector();
            evt.StopPropagation();
        }

        internal void ShowSelector(bool repair = false)
        {
            if (_isReadOnly || !enabledInHierarchy) return;

            var window = _visualInput.GetOwnerWindow();
            if (window == null) return;

            TypeSelectorWindow.Show(
                screenRect: GetScreenRect(),
                filter: CreateFilter(repair),
                currentAqn: repair || showMixedValue ? null : value?.AssemblyQualifiedName ?? _missingAssemblyQualifiedName ?? string.Empty,
                onSelected: ApplyPicked);

            return;

            Rect GetScreenRect() => new(
                window.position.x + _visualInput.worldBound.xMin,
                window.position.y + _visualInput.worldBound.yMin,
                _visualInput.worldBound.width,
                _visualInput.worldBound.height);
        }

        internal TypeSelectorFilter CreateFilter(bool repair = false)
        {
            // The scan never offers an open generic class for a closed generic base type, so those come in separately;
            // they bypass the filter's predicate, which therefore runs on them here.
            var genericDefinitions = GenericTypeResolver.GetClosableGenericDefinitions(Types);

            return new TypeSelectorFilter
            {
                Types = Types,
                Allow = Allow,
                Predicate = Predicate,
                AdditionalTypes = Predicate is null ? genericDefinitions : genericDefinitions?.Where(Predicate),
                HideNoneOption = repair || HideNoneOption,
                ExcludeEditorOnly = ExcludeEditorOnlyTypes,
            };
        }

        internal void ApplyPicked(string assemblyQualifiedName)
        {
            // The property goes first. The picker window picks inside its own event, so Unity queues ChangeEvent until
            // this method returns, whatever the order here. The caller's SerializedObject is updated too, so a change
            // handler that applies it does not write the old name back.
            if (_property is not null)
            {
                var serializedObject = _property.serializedObject;
                serializedObject.Update();
                _property.SetStringAndApply(assemblyQualifiedName ?? string.Empty);
                serializedObject.Update();

                UpdateSourceSerializedObject();
                EnableInClassList(className: BindingExtensions.prefabOverrideUssClassName, enable: IsPrefabOverride(_property));
            }

            value = TypeUtility.GetTypeOrNull(assemblyQualifiedName);
        }

        private void UpdateSourceSerializedObject()
        {
            try
            {
                _sourceSerializedObject.Update();
            }
            catch (NullReferenceException)
            {
                // The caller disposed it: a disposed SerializedObject throws on access, and nothing reads it any more.
            }
        }

        // Unity's own fields draw the override for one target only, also when the whole component is an addition.
        private static bool IsPrefabOverride(SerializedProperty property)
        {
            if (property.serializedObject.targetObjects.Length != 1 || !property.isInstantiatedPrefab) return false;
            if (property.prefabOverride) return true;

            return property.serializedObject.targetObject is Component component &&
                   PrefabUtility.GetCorrespondingObjectFromSource(component.gameObject) != null &&
                   PrefabUtility.GetCorrespondingObjectFromSource(component) == null;
        }

        // The Apply and Revert items of Unity's property menu, which a field bound without binding-path does not get.
        internal void AddPrefabOverrideActions(DropdownMenu menu)
        {
            if (_property is null || !_property.isInstantiatedPrefab || !_property.prefabOverride) return;

            var serializedObject = _property.serializedObject;
            var target = serializedObject.targetObject;
            var status = _isReadOnly || !enabledInHierarchy
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal;

            // As in Unity, one target only: an item per prefab of the variant chain, the base prefab last.
            if (serializedObject.targetObjects.Length == 1)
            {
                var sources = new List<Object>();
                for (var source = PrefabUtility.GetCorrespondingObjectFromSource(target);
                     source != null;
                     source = PrefabUtility.GetCorrespondingObjectFromSource(source))
                    sources.Add(source);

                for (var i = 0; i < sources.Count; i++)
                {
                    var root = GetRootGameObject(sources[i]);
                    var prefabName = root != null ? root.name : sources[i].name;
                    var assetPath = AssetDatabase.GetAssetPath(sources[i]);
                    var canApply = root != null && !EditorUtility.IsPersistent(target) &&
                                   PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(root);

                    menu.AppendAction(
                        actionName: i == sources.Count - 1
                            ? $"Apply to Prefab '{prefabName}'"
                            : $"Apply as Override in Prefab '{prefabName}'",
                        action: _ => ApplyPrefabOverride(assetPath),
                        status: canApply ? status : DropdownMenuAction.Status.Disabled);
                }
            }

            menu.AppendAction(actionName: "Revert", action: _ => RevertPrefabOverride(), status: status);
        }

        private void ApplyPrefabOverride(string assetPath)
        {
            _property.serializedObject.Update();
            PrefabUtility.ApplyPropertyOverride(instanceProperty: _property, assetPath: assetPath, action: InteractionMode.UserAction);
            RefreshProperty(property: _property);
        }

        private void RevertPrefabOverride()
        {
            _property.serializedObject.Update();
            PrefabUtility.RevertPropertyOverride(instanceProperty: _property, action: InteractionMode.UserAction);
            RefreshProperty(property: _property);
        }

        private static GameObject GetRootGameObject(Object source) => source switch
        {
            Component component => component.transform.root.gameObject,
            GameObject gameObject => gameObject.transform.root.gameObject,
            _ => null,
        };

        // Unity's binding cannot drive a field of Type from a string property, so UXML binding-path only logs Unity's
        // own warning; this one says what to do instead.
        private void WarnAboutBindingPath(AttachToPanelEvent evt)
        {
            if (_property is not null || string.IsNullOrEmpty(bindingPath)) return;

            UnregisterCallback<AttachToPanelEvent>(WarnAboutBindingPath);
            Debug.LogWarning($"{GetType().Name} ignores binding-path \"{bindingPath}\". Create it in C# with " +
                             $"new {GetType().Name}(property) to bind it to a string property.");
        }
    }
}
