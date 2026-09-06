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
    // UI Toolkit rendering of a SerializableMonoScript / SerializableMonoScript<T> wrapper: an unbound
    // InspectorTypeField kept in step with the wrapper property, whose picks write both the script reference and the
    // type name; a MonoScript can be dropped onto the field.
    internal static class MonoScriptUIToolkitPropertyDrawer
    {
        internal static VisualElement Draw(
            string label,
            SerializedProperty wrapperProperty,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
            => Draw(label, wrapperProperty, allow, types, out _);

        // The out overload hands the created field to callers that keep updating its Types after creation (live
        // member-referenced constraints — see TypeSelectorPropertyDrawer).
        internal static VisualElement Draw(
            string label,
            SerializedProperty wrapperProperty,
            TypeAllow allow,
            Type[] types,
            out InspectorTypeField field)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;
            var persistent = wrapperProperty.Persistent();

            var typeField = new InspectorTypeField(label)
            {
                Allow = allow,
                Types = types,
                Predicate = SerializableMonoScriptUtility.HasScript,
            };

            field = typeField;
            Refresh(persistent);

            // Undo/redo, prefab revert and scripted edits rewrite the wrapper outside this field.
            typeField.TrackPropertyValue(persistent, Refresh);
            typeField.RegisterValueChangedCallback(evt => SerializableMonoScriptUtility.Assign(persistent, evt.newValue));
            RegisterDragAndDrop(typeField, persistent);

            var nameProperty = SerializableTypeUtility.GetBackingProperty(persistent);
            if (!TypeSelectorRequiredGate.TryGetRequired(nameProperty, out _))
                return typeField;

            var container = new VisualElement().AddChild(typeField);
            var notice = new InspectorNotice();

            container.TrackPropertyValue(nameProperty, RefreshNotice);
            RefreshNotice(nameProperty);

            return container;

            void Refresh(SerializedProperty current)
            {
                var type = SerializableMonoScriptUtility.GetCurrentType(current, out var assemblyQualifiedName);

                if (type is not null) typeField.SetValueWithoutNotify(type);
                else typeField.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName);
            }

            void RefreshNotice(SerializedProperty current)
            {
                if (!TypeSelectorRequiredGate.IsViolation(current))
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

        private static void RegisterDragAndDrop(TypeField field, SerializedProperty wrapperProperty)
        {
            field.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = SerializableMonoScriptUtility.TryResolveDroppedType(field.Types, field.Allow, out _)
                    ? DragAndDropVisualMode.Link
                    : DragAndDropVisualMode.Rejected;
            });

            field.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (!SerializableMonoScriptUtility.TryResolveDroppedType(field.Types, field.Allow, out var dropped)) return;

                DragAndDrop.AcceptDrag();
                SerializableMonoScriptUtility.Assign(wrapperProperty, dropped);
            });
        }
    }
}
