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
        internal const string StyleSheetPath = "UI/Types/Aspid-FastTools-TypeProperty";
        internal const string MissingClass = "aspid-fasttools-type-property--missing";
        internal const string StripeClass = "aspid-fasttools-type-property__stripe";

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

            IsolateNameChanges(container);
            container.TrackSerializedObjectValue(property.serializedObject,
                _ => container.schedule.Execute(RefreshFromObject));
            typeField.RegisterValueChangedCallback(_ => container.schedule.Execute(RefreshFromObject));
            typeField.NameWritten += (previousName, newName) =>
                SendNameChanged(root: container, previousName: previousName, newName: newName);
            RefreshFromObject();

            return container;

            // Each refresh reads a fresh copy of the object and releases it; no copy means the property is gone.
            void RefreshFromObject()
            {
                var current = property.Persistent();
                if (current is null) return;

                using var owner = current.serializedObject;
                Refresh(current);
            }

            void Refresh(SerializedProperty current)
            {
                typeField.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: current.stringValue);
                var missing = TypeMissingRepair.IsMissing(property: current);
                container.EnableInClassList(className: MissingClass, enable: missing);

                if (missing)
                {
                    notice ??= new InspectorNotice();
                    ShowMissing(container: container, stripe: stripe, notice: notice, field: typeField,
                        storedName: current.stringValue,
                        suggestion: TypeMissingRepair.GetSuggestion(storedName: current.stringValue,
                            types: typeField.Types, allow: typeField.Allow,
                            excludeEditorOnly: typeField.ExcludeEditorOnlyTypes),
                        onSuggestionApplied: RefreshFromObject);
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

        // A bound string field reports an edit as ChangeEvent<string>; the drawer's root does the same, so a PropertyField
        // around the drawer notifies its listeners.
        internal static void SendNameChanged(VisualElement root, string previousName, string newName)
        {
            if (root.panel is null) return;

            using var evt = ChangeEvent<string>.GetPooled(previousValue: previousName, newValue: newName);
            evt.target = root;
            root.SendEvent(evt);
        }

        // The root speaks for the drawer with the event above. Text elements in the drawer (the caption, a notice)
        // send their own ChangeEvent<string> on Unity 6; it carries display text and must not reach a PropertyField.
        internal static void IsolateNameChanges(VisualElement root) =>
            root.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.target != root) evt.StopPropagation();
            });

        // Fix and the suggestion both repair through the field: a bound field writes its property, and a wrapper
        // field leaves the write to its change handler.
        internal static void ShowMissing(
            VisualElement container,
            VisualElement stripe,
            InspectorNotice notice,
            InspectorTypeField field,
            string storedName,
            Type suggestion,
            Action onSuggestionApplied)
        {
            if (stripe.parent is null) container.AddChild(stripe);

            notice.Set(message: "Missing type", actionText: "Fix",
                detail: TypeMissingRepair.GetDetail(storedName: storedName),
                onAction: () => field.ShowSelector(repair: true));

            if (suggestion is not null)
                notice.SetSuggestion(
                    suggestionText: $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion)}",
                    detail: $"Replace the stored type name with {suggestion.AssemblyQualifiedName}.",
                    onSuggestion: () =>
                    {
                        if (field.IsReadOnly || !field.enabledInHierarchy) return;
                        field.ApplyPicked(assemblyQualifiedName: suggestion.AssemblyQualifiedName);
                        onSuggestionApplied();
                    });

            if (notice.parent is null) container.AddChild(notice);
        }
    }
}
