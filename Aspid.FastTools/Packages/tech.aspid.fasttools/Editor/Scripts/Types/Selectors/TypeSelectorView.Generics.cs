using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed partial class TypeSelectorView
    {
        private void BeginResolveGeneric(Type openDefinition, Type[] validationFieldTypes, Func<Type, string> validate,
            Action<Type> onClosed)
        {
            // Inference bypasses the argument pages, so the closed type must also satisfy every narrowing
            // constraint and the page's own check. A narrowing type can pin arguments the field type leaves open.
            foreach (var fieldType in validationFieldTypes)
            {
                if (!GenericTypeResolver.TryInferFromFieldType(fieldType, openDefinition, out var inferred, _inferredArgumentFilter)) continue;
                if (!GenericTypeResolver.IsAssignableToFieldTypes(inferred, validationFieldTypes)) continue;
                if (validate?.Invoke(inferred) is not null) continue;

                onClosed(inferred);
                return;
            }

            PickParam(openDefinition, validationFieldTypes, validate, Array.Empty<Type>(), _pages.Count, onClosed);
        }

        private void PickParam(Type openDefinition, Type[] validationFieldTypes, Func<Type, string> validate,
            Type[] argsSoFar, int startDepth, Action<Type> onClosed)
        {
            var parameters = openDefinition.GetGenericArguments();

            if (argsSoFar.Length == parameters.Length)
            {
                if (!GenericTypeResolver.TryConstruct(openDefinition, argsSoFar, validationFieldTypes, out var closed, out var error))
                {
                    ShowError(error);
                    return;
                }

                // Checked before the pages close, so a rejected type leaves the user where the last argument was picked.
                var rejection = validate?.Invoke(closed);
                if (rejection is not null)
                {
                    ShowError(rejection);
                    return;
                }

                PopToDepth(startDepth);
                onClosed(closed);
                return;
            }

            var index = argsSoFar.Length;
            var parameter = parameters[index];

            var page = BuildParamPage(openDefinition, argsSoFar, index, parameter, picked =>
                PickParam(openDefinition, validationFieldTypes, validate, Append(argsSoFar, picked), startDepth, onClosed));

            PushPage(page);
        }

        private PickerPage BuildParamPage(Type openDefinition, Type[] argsSoFar, int index, Type parameter, Action<Type> onPicked)
        {
            var baseTypes = GenericTypeResolver.GetConstraintBaseTypes(parameter);
            var constraintType = baseTypes.Length == 1 ? baseTypes[0] : typeof(object);

            // Open definitions are offered as arguments too, so generics can nest; picking one resolves its own
            // arguments first. Every constraint base type goes in, not just the collapsed one, so a multi-constraint
            // parameter narrows the nested definitions up front instead of offering ones that fail every later pick.
            // The argument filter cannot judge an open definition, so here it meets only the special constraints;
            // Validate checks the type it closes to.
            var nested = GenericTypeResolver
                .GetAssignableGenericDefinitions(baseTypes[0], baseTypes, _inferredArgumentFilter, includeValueTypes: true)
                .Where(candidate => candidate.IsGenericTypeDefinition
                    ? GenericTypeResolver.SatisfiesSpecialConstraints(parameter, candidate)
                    : Filter(candidate));

            var hierarchy = HierarchyBuilder.Build(baseTypes, TypeAllow.None, (Func<Type, bool>)Filter, nested,
                includeNoneOption: false, includeHidden: _includeHidden, excludeEditorOnly: _excludeEditorOnly);

            return new PickerPage
            {
                Navigation = new NavigationController(hierarchy),
                TitlePrefix = $"{FormatBuilding(openDefinition, argsSoFar, index)}  ▸  {parameter.Name}",
                ConstraintType = constraintType,
                OnPicked = onPicked,
                Validate = closed => Filter(closed)
                    ? null
                    : $"{TypeSelectorHelpers.GetTypeSelectorTitle(closed)} is not allowed as {parameter.Name}.",
                IsBase = false,
            };

            bool Filter(Type candidate) =>
                GenericTypeResolver.SatisfiesSpecialConstraints(parameter, candidate)
                && (_argumentFilter?.Invoke(candidate) ?? true);
        }

        private void PushPage(PickerPage page)
        {
            _pages.Add(page);
            ResetSearchField();
            UpdateSearchChrome();
            RefreshView();
            SelectFirstItem();
            FocusPicker();
        }

        private void PopPage()
        {
            if (_pages.Count <= 1) return;

            _pages.RemoveAt(_pages.Count - 1);
            HideError();
            ResetSearchField();
            UpdateSearchChrome();
            RefreshView();
            SelectFirstItem();
            FocusPicker();
        }

        private void PopToDepth(int depth)
        {
            while (_pages.Count > depth)
                _pages.RemoveAt(_pages.Count - 1);
        }

        private void ResetSearchField()
        {
            _searchField.SetValueWithoutNotify(string.Empty);
            Nav.ApplySearch(string.Empty);
        }

        private void SelectFirstItem()
        {
            _listView.selectedIndex = Nav.CurrentItems.Count > 0 ? 0 : -1;
            _listView.ScrollToItem(0);
        }

        private static string FormatBuilding(Type openDefinition, Type[] argsSoFar, int currentIndex)
        {
            var parameters = openDefinition.GetGenericArguments();

            var custom = TypeSelectorHelpers.GetCustomDisplayName(openDefinition);
            var angle = custom?.IndexOf('<') ?? -1;
            var baseName = custom is null
                ? TypeUtility.StripArity(openDefinition.Name)
                : angle < 0 ? custom : custom[..angle];

            var parts = new string[parameters.Length];
            for (var k = 0; k < parameters.Length; k++)
            {
                if (k < argsSoFar.Length) parts[k] = TypeSelectorHelpers.GetTypeSelectorTitle(argsSoFar[k]);
                else if (k == currentIndex) parts[k] = "?";
                else parts[k] = parameters[k].Name;
            }

            return $"{baseName}<{string.Join(", ", parts)}>";
        }

        private static Type[] Append(Type[] array, Type value)
        {
            var result = new Type[array.Length + 1];
            Array.Copy(array, result, array.Length);
            result[^1] = value;
            return result;
        }

        private sealed class PickerPage
        {
            public NavigationController Navigation;
            public string TitlePrefix;
            public Type ConstraintType;
            public Action<Type> OnPicked;

            // Why a generic closed from this page's list cannot be picked, or null when it can.
            public Func<Type, string> Validate;

            public bool IsBase;
        }
    }
}
