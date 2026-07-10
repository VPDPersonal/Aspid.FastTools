using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
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
                Types = types,
            };

            if (!SerializeReferenceRequiredGate.TryGetRequired(property, out _))
                return field;

            // A [TypeSelector(Required = true)] string left empty shows a non-actionable warning below the field,
            // reusing the managed-reference notice; the dropdown above is the implied fix.
            var container = new VisualElement().AddChild(field);
            var notice = new SerializeReferenceNotice();

            // The tracked callback hands over a FRESH property each tick — closing over the ctor-time one would read
            // a disposed SerializedObject once the element outlives its source editor (the Persistent() contract).
            // The initial pass gets its own persistent copy for the same reason.
            container.TrackPropertyValue(property, Refresh);
            Refresh(property.Persistent());

            return container;

            void Refresh(SerializedProperty current)
            {
                if (!SerializeReferenceRequiredGate.IsViolation(current))
                {
                    notice.RemoveFromHierarchy();
                    return;
                }

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
