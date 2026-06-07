using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Floating editor window that resolves the type arguments of an open generic definition selected in a
    /// <c>[SerializeReferenceSelector]</c> dropdown. It renders one row per generic parameter; each row reuses
    /// the existing <see cref="TypeSelectorWindow"/> (hierarchy + search) to pick a concrete argument, honouring
    /// the parameter's base-type and special (<c>struct</c>/<c>class</c>/<c>new()</c>) constraints. Pressing
    /// <c>Create</c> closes the definition via <see cref="Type.MakeGenericType"/> and invokes the callback.
    /// </summary>
    /// <remarks>
    /// Shown via <see cref="EditorWindow.ShowUtility"/> (not as a dropdown) so it survives the focus changes
    /// caused by opening the per-parameter <see cref="TypeSelectorWindow"/>.
    /// </remarks>
    internal sealed class GenericArgumentSelectorWindow : EditorWindow
    {
        private const string SelectArgumentText = "Select Type";

        private Type _openDefinition;
        private Type _fieldType;
        private Type[] _parameters;
        private Type[] _selected;
        private Button[] _argumentButtons;
        private Button _createButton;
        private HelpBox _errorBox;
        private Action<Type> _onResolved;

        /// <summary>
        /// Opens the window anchored near <paramref name="anchor"/> to resolve the arguments of
        /// <paramref name="openDefinition"/>. The constructed closed type is validated against
        /// <paramref name="fieldType"/> before <paramref name="onResolved"/> is invoked; it is never invoked
        /// if the window is closed first or the closed type is not assignable to the field.
        /// </summary>
        public static void Show(Rect anchor, Type openDefinition, Type fieldType, Action<Type> onResolved)
        {
            var window = CreateInstance<GenericArgumentSelectorWindow>();
            window.titleContent = new GUIContent("Select Generic Arguments");

            window._openDefinition = openDefinition;
            window._fieldType = fieldType;
            window._parameters = openDefinition.GetGenericArguments();
            window._selected = new Type[window._parameters.Length];
            window._onResolved = onResolved;

            window.BuildUI();
            window.ShowUtility();

            var height = 28f + window._parameters.Length * 24f + 30f;
            window.position = new Rect(anchor.x, anchor.yMax, Mathf.Max(320f, anchor.width), height);
        }

        private void BuildUI()
        {
            _argumentButtons = new Button[_parameters.Length];

            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 6;

            root.Add(new Label($"Arguments for {FormatDefinitionName(_openDefinition)}")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            });

            for (var i = 0; i < _parameters.Length; i++)
            {
                var index = i;
                var parameter = _parameters[i];

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                row.Add(new Label($"[{i}] {parameter.Name}")
                {
                    style = { minWidth = 70, marginRight = 4 }
                });

                var button = new Button { text = SelectArgumentText, style = { flexGrow = 1 } };
                button.clicked += () => ShowArgumentPicker(index, button);

                _argumentButtons[i] = button;
                row.Add(button);
                root.Add(row);
            }

            _errorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error)
            {
                style = { display = DisplayStyle.None, marginTop = 4 }
            };
            root.Add(_errorBox);

            _createButton = new Button(CreateGenericType) { text = "Create", style = { marginTop = 6 } };
            root.Add(_createButton);

            RefreshCreateButton();
        }

        private void ShowArgumentPicker(int parameterIndex, Button button)
        {
            var parameter = _parameters[parameterIndex];
            var baseTypes = GetConstraintBaseTypes(parameter);
            var constraintType = baseTypes.Length == 1 ? baseTypes[0] : typeof(object);
            var filter = BuildArgumentFilter(parameter);

            // Offer open generic definitions as arguments too, so the user can nest generics (e.g. choose
            // Modifier<T> for T) — picking one resolves its own arguments recursively before it is used here.
            var genericDefinitions = SerializeReferenceHelpers.GetAssignableGenericDefinitions(constraintType);

            var screenRect = new Rect(
                position.x + button.worldBound.xMin,
                position.y + button.worldBound.yMin,
                button.worldBound.width,
                button.worldBound.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: baseTypes,
                currentAqn: _selected[parameterIndex]?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName =>
                {
                    var selectedType = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    if (selectedType is { IsGenericTypeDefinition: true })
                        SerializeReferenceHelpers.ResolveGenericType(
                            selectedType, constraintType, screenRect, resolved => SetArgument(parameterIndex, resolved));
                    else
                        SetArgument(parameterIndex, selectedType);
                },
                filter: filter,
                additionalTypes: genericDefinitions);
        }

        private void SetArgument(int parameterIndex, Type type)
        {
            _selected[parameterIndex] = type;
            _argumentButtons[parameterIndex].text =
                type is null ? SelectArgumentText : TypeSelectorHelpers.GetTypeSelectorTitle(type);
            RefreshCreateButton();
        }

        private void CreateGenericType()
        {
            if (_selected.Any(type => type is null)) return;

            Type closed;
            try
            {
                closed = _openDefinition.MakeGenericType(_selected);
            }
            catch (Exception exception)
            {
                ShowError($"Cannot construct {FormatDefinitionName(_openDefinition)}: {exception.Message}");
                return;
            }

            // The chosen arguments may satisfy the type parameter's own constraints yet still produce a type
            // that is not assignable to the managed-reference field — guard against a value Unity would drop.
            if (_fieldType is not null && !_fieldType.IsAssignableFrom(closed))
            {
                ShowError($"{closed.Name} is not assignable to {_fieldType.Name}.");
                return;
            }

            _onResolved?.Invoke(closed);
            Close();
        }

        private void ShowError(string message)
        {
            _errorBox.text = message;
            _errorBox.style.display = DisplayStyle.Flex;
        }

        private void RefreshCreateButton()
        {
            _createButton.SetEnabled(_selected.All(type => type is not null));
            _errorBox.style.display = DisplayStyle.None;
        }

        private static Type[] GetConstraintBaseTypes(Type parameter)
        {
            var constraints = parameter.GetGenericParameterConstraints()
                .Where(constraint => !constraint.IsGenericParameter && !constraint.ContainsGenericParameters)
                .ToArray();

            return constraints.Length > 0 ? constraints : new[] { typeof(object) };
        }

        private static Func<Type, bool> BuildArgumentFilter(Type parameter)
        {
            var special = parameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
            var requireValueType = (special & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
            var requireReferenceType = (special & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            var requireDefaultCtor = (special & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

            return type =>
            {
                if (!SerializeReferenceHelpers.IsValidGenericArgument(type)) return false;
                if (requireValueType && !type.IsValueType) return false;
                if (requireReferenceType && type.IsValueType) return false;

                return !requireDefaultCtor || type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null;
            };
        }

        private static string FormatDefinitionName(Type definition)
        {
            var name = definition.Name;
            var tick = name.IndexOf('`');
            var baseName = tick >= 0 ? name[..tick] : name;
            var arguments = string.Join(", ", definition.GetGenericArguments().Select(argument => argument.Name));
            return $"{baseName}<{arguments}>";
        }
    }
}
