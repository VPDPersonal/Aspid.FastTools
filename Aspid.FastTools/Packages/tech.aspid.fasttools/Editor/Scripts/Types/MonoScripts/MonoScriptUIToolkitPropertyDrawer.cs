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
    internal static class MonoScriptUIToolkitPropertyDrawer
    {
        internal static VisualElement Draw(
            string label,
            SerializedProperty wrapperProperty,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
            => Draw(label, wrapperProperty, allow, types, out _);

        internal static VisualElement Draw(
            string label,
            SerializedProperty wrapperProperty,
            TypeAllow allow,
            Type[] types,
            out InspectorTypeField field)
        {
            label = string.IsNullOrWhiteSpace(label) ? null : label;

            // Every read and write opens a copy of the object when it runs and disposes it afterwards: the drawer
            // outlives the property Unity hands it, and holds no copy of its own.
            var detached = new DetachedProperty(property: wrapperProperty);

            var typeField = new InspectorTypeField(label)
            {
                Allow = allow,
                Types = types,
                Predicate = SerializableMonoScriptUtility.HasScript,
                ExcludeEditorOnlyTypes = TypeSelectorHelpers.IsStoredInRuntimeObject(wrapperProperty),
            };

            field = typeField;

            var container = new VisualElement()
                .AddStyleSheetFromResources(TypeUIToolkitPropertyDrawer.StyleSheetPath)
                .AddChild(typeField);
            var stripe = new VisualElement()
                .AddClass(TypeUIToolkitPropertyDrawer.StripeClass)
                .SetPickingMode(PickingMode.Ignore);
            InspectorNotice notice = null;

            TypeUIToolkitPropertyDrawer.IsolateNameChanges(root: container);
            RefreshFromObject();

            typeField.TrackPropertyValue(wrapperProperty, _ => RefreshFromObject());
            container.TrackSerializedObjectValue(wrapperProperty.serializedObject,
                _ => container.schedule.Execute(RefreshFromObject));
            typeField.RegisterValueChangedCallback(evt => Assign(type: evt.newValue));
            RegisterDragAndDrop(field: typeField, assign: Assign);

            return container;

            // Every write goes through here, so a pick and a drop both report the change to a PropertyField above.
            void Assign(Type type)
            {
                var assigned = false;
                var wasMixed = false;
                string previousName = null;

                detached.Use(persistent =>
                {
                    persistent.serializedObject.SetIsDifferentCacheDirty();
                    previousName = SerializableTypeUtility.GetBackingProperty(wrapperProperty: persistent).stringValue;
                    wasMixed = SerializableMonoScriptUtility.HasMultipleDifferentValues(wrapperProperty: persistent);

                    SerializableMonoScriptUtility.Assign(wrapperProperty: persistent, type: type);
                    assigned = true;
                });

                // A skipped write, for a target destroyed or a path gone, changed nothing to report.
                if (!assigned) return;

                var newName = type?.AssemblyQualifiedName ?? string.Empty;
                if (wasMixed || previousName != newName)
                    TypeUIToolkitPropertyDrawer.SendNameChanged(root: container, previousName: previousName, newName: newName);
            }

            // As in the SerializableType drawer: a fresh SerializedObject sees values the tracked one has not re-read.
            // No copy means the property is gone.
            void RefreshFromObject() => detached.Use(Refresh);

            void Refresh(SerializedProperty current)
            {
                var type = SerializableMonoScriptUtility.GetCurrentType(current, out var assemblyQualifiedName);

                if (type is not null) typeField.SetValueWithoutNotify(type);
                else typeField.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName);

                current.serializedObject.SetIsDifferentCacheDirty();
                typeField.showMixedValue = SerializableMonoScriptUtility.HasMultipleDifferentValues(wrapperProperty: current);

                RefreshNotice(current);
            }

            // A missing type takes the notice slot, so the required notice shows only while the name is empty.
            void RefreshNotice(SerializedProperty current)
            {
                var missing = TypeMissingRepair.IsMissingMonoScript(wrapperProperty: current);
                container.EnableInClassList(className: TypeUIToolkitPropertyDrawer.MissingClass, enable: missing);

                var nameProperty = SerializableTypeUtility.GetBackingProperty(current);
                if (missing)
                {
                    notice ??= new InspectorNotice();
                    TypeUIToolkitPropertyDrawer.ShowMissing(container: container, stripe: stripe, notice: notice,
                        field: typeField, storedName: nameProperty.stringValue,
                        suggestion: TypeMissingRepair.GetSuggestion(storedName: nameProperty.stringValue,
                            types: typeField.Types, allow: typeField.Allow,
                            excludeEditorOnly: typeField.ExcludeEditorOnlyTypes,
                            predicate: SerializableMonoScriptUtility.HasScript),
                        onSuggestionApplied: RefreshFromObject);
                    return;
                }

                stripe.RemoveFromHierarchy();
                if (current.hasMultipleDifferentValues || !TypeSelectorRequiredGate.IsViolation(nameProperty))
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

        private static void RegisterDragAndDrop(TypeField field, Action<Type> assign)
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
                assign(dropped);
            });
        }
    }
}
