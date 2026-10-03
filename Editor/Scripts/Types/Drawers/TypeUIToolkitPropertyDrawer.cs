using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeUIToolkitPropertyDrawer
    {
        private const string StyleSheetPath = "UI/Types/Aspid-FastTools-TypeProperty";
        private const string MissingClass = "aspid-fasttools-type-property--missing";
        private const string StripeClass = "aspid-fasttools-type-property__stripe";

        internal static VisualElement Draw(
            string label,
            SerializedProperty property,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
            => Draw(label, property, allow, types, out _);

        internal static VisualElement Draw(
            string label,
            SerializedProperty property,
            TypeAllow allow,
            Type[] types,
            out InspectorTypeField field)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;

            field = new InspectorTypeField(label, property)
            {
                Allow = allow,
                Types = types,
            };

            var typeField = field;
            var container = new VisualElement()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddChild(field);
            var stripe = new VisualElement().AddClass(StripeClass).SetPickingMode(PickingMode.Ignore);
            InspectorNotice notice = null;

            container.TrackSerializedObjectValue(property.serializedObject,
                _ => container.schedule.Execute(() => Refresh(property.Persistent())));
            typeField.RegisterValueChangedCallback(
                _ => container.schedule.Execute(() => Refresh(property.Persistent())));
            Refresh(property.Persistent());

            return container;

            void Refresh(SerializedProperty current)
            {
                typeField.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: current.stringValue);
                var missing = TypeMissingRepair.IsMissing(property: current);
                container.EnableInClassList(className: MissingClass, enable: missing);

                if (missing)
                {
                    if (stripe.parent is null) container.AddChild(stripe);
                    notice ??= new InspectorNotice();
                    notice.Set(message: "Missing type", actionText: "Fix",
                        detail: TypeMissingRepair.GetDetail(storedName: current.stringValue),
                        onAction: () => typeField.ShowSelector(repair: true));

                    var suggestion = TypeMissingRepair.GetSuggestion(storedName: current.stringValue,
                        types: typeField.Types, allow: typeField.Allow,
                        excludeEditorOnly: typeField.ExcludeEditorOnlyTypes);
                    if (suggestion is not null)
                        notice.SetSuggestion(
                            suggestionText: $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion)}",
                            detail: $"Replace the stored type name with {suggestion.AssemblyQualifiedName}.",
                            onSuggestion: () =>
                            {
                                if (typeField.IsReadOnly || !typeField.enabledInHierarchy) return;
                                typeField.ApplyPicked(assemblyQualifiedName: suggestion.AssemblyQualifiedName);
                                Refresh(property.Persistent());
                            });

                    if (notice.parent is null) container.AddChild(notice);
                    return;
                }

                stripe.RemoveFromHierarchy();
                if (current.hasMultipleDifferentValues || !TypeSelectorRequiredGate.IsViolation(current))
                {
                    notice?.RemoveFromHierarchy();
                    return;
                }

                notice ??= new InspectorNotice();
                notice.Set(
                    message: "Required type is not set",
                    actionText: string.Empty,
                    detail: "This [TypeSelector] field is marked required but has no type. Pick a type from the dropdown.",
                    onAction: null);

                if (notice.parent is null) container.AddChild(notice);
            }
        }
    }
}
