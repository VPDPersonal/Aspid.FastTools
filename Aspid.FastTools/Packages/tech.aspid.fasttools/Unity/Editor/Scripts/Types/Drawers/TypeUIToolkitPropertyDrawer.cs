using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeUIToolkitPropertyDrawer
    {
        public static VisualElement Draw(
            string label,
            SerializedProperty property,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;

            var field = new InspectorTypeField(label, property)
            {
                Allow = allow,
                Types = types
            };

            // No required marker → return the bare field, with no notice scaffolding or per-change tracking.
            if (!SerializeReferenceRequiredGate.TryGetRequired(property, out _)) return field;

            // A [TypeSelector(Required = true)] string left empty shows a non-actionable warning below the field,
            // reusing the managed-reference notice; the dropdown above is the implied fix.
            var container = new VisualElement().AddChild(field);
            var notice = new SerializeReferenceNotice();

            container.TrackPropertyValue(property, _ => Refresh());
            Refresh();

            return container;

            void Refresh()
            {
                if (!SerializeReferenceRequiredGate.IsViolation(property))
                {
                    notice.RemoveFromHierarchy();
                    return;
                }

                SerializeReferenceRequiredGate.TryGetRequired(property, out var selector);
                var message = string.IsNullOrEmpty(selector?.RequiredMessage) ? "Required type is not set" : selector.RequiredMessage;

                notice.Set(
                    message: message,
                    actionText: string.Empty,
                    detail: "This [TypeSelector] field is marked required but has no type. Pick a type from the dropdown.",
                    onAction: null);

                if (notice.parent is null) container.AddChild(notice);
            }
        }
    }
}
