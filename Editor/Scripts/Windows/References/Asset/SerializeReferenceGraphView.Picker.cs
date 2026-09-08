using System;
using UnityEditor;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceGraphView
    {
        private const string PickerClass = RootClass + "__picker";
        private const string PickerAttachedClass = PickerClass + "--attached";
        private const string NodePickingClass = NodeClass + "--picking";

        private static readonly AuditPickerHost.PickerClasses _pickerClassSet =
            new(PickerClass, PickerAttachedClass, NodePickingClass);

        private void OpenMissingPicker(string assetPath, long fileId, long rid, AspidGradientButton anchor) =>
            TogglePicker(anchor, ManagedReferenceFilter.For(_constraints.Resolve(assetPath, fileId, rid), includeHidden: true),
                currentAqn: null, // a missing entry has no current value — nothing (not even <None>) wears the check
                assemblyQualifiedName => ApplyFix(assetPath, fileId, rid, assemblyQualifiedName));

        private void OpenLivePicker(string assetPath, long fileId, string graphPath, AspidGradientButton anchor)
        {
            var constraint = typeof(object);
            var currentAqn = string.Empty;

            if (SerializeReferenceGraphEditor.TryResolveLiveProperty(assetPath, fileId, graphPath, out var serializedObject, out var property))
            {
                using (serializedObject)
                {
                    constraint = SerializeReferenceHelpers.GetFieldType(property);
                    currentAqn = property.managedReferenceValue?.GetType().AssemblyQualifiedName ?? string.Empty;
                }
            }

            TogglePicker(anchor, ManagedReferenceFilter.For(constraint), currentAqn,
                assemblyQualifiedName => ApplyLive(assetPath, fileId, graphPath, assemblyQualifiedName));
        }

        private void OpenRequiredStringPicker(GateViolation violation, AspidGradientButton anchor)
        {
            var filter = default(TypeSelectorFilter);
            var currentAqn = string.Empty;

            if (SerializeReferenceGraphEditor.TryResolveRequiredStringProperty(violation, out var serializedObject, out var property))
            {
                using (serializedObject)
                {
                    currentAqn = property.stringValue ?? string.Empty;
                    filter = BuildRequiredStringFilter(serializedObject, property);
                }
            }

            TogglePicker(anchor, filter, currentAqn,
                assemblyQualifiedName => ApplyRequiredString(violation, assemblyQualifiedName));
        }

        private static TypeSelectorFilter BuildRequiredStringFilter(SerializedObject serializedObject, SerializedProperty property)
        {
            if (!TypeSelectorRequiredGate.TryGetRequired(property, out var selector)) return default;

            var types = new List<Type>();

            var path = property.propertyPath;
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex >= 0)
            {
                using var parentProperty = serializedObject.FindProperty(path[..lastDotIndex]);
                var parentField = parentProperty?.GetFieldInfo();
                if (parentField is not null &&
                    SerializableTypeUtility.TryGetBaseType(parentField.FieldType, out var wrapperBase) &&
                    wrapperBase is not null && wrapperBase != typeof(object))
                    types.Add(wrapperBase);
            }

            types.AddRange(TypeSelectorConstraintResolver.Resolve(
                serializedObject.targetObject, selector.AssemblyQualifiedNames).Types);

            return new TypeSelectorFilter
            {
                Types = types.Count > 0 ? types.ToArray() : null,
                Allow = selector.Allow,
            };
        }

        private void TogglePicker(AspidGradientButton anchor, TypeSelectorFilter filter, string currentAqn, Action<string> onSelected)
        {
            if (_picker.ToggleClosed(anchor)) return;

            _picker.Open(anchor, new TypeSelectorView(
                filter: filter,
                currentAqn: currentAqn, // null (no current-value concept) and "" (holds <None>) both pass through as-is
                onSelected: onSelected,
                onDismiss: _picker.Close));
        }

        private void ApplyFix(string assetPath, long fileId, long rid, string assemblyQualifiedName)
        {
            if (SerializeReferenceGraphEditor.ApplyFix(assetPath, fileId, rid, assemblyQualifiedName)) Rescan();
        }

        private void ApplyLive(string assetPath, long fileId, string graphPath, string assemblyQualifiedName)
        {
            if (SerializeReferenceGraphEditor.ApplyLive(assetPath, fileId, graphPath, assemblyQualifiedName)) Rescan();
        }

        private void ApplyRequiredString(GateViolation violation, string assemblyQualifiedName)
        {
            if (SerializeReferenceGraphEditor.ApplyRequiredString(violation, assemblyQualifiedName)) Rescan();
        }

        private void ClearOrphan(string assetPath, long fileId, long rid)
        {
            if (SerializeReferenceGraphEditor.TryClearOrphan(assetPath, fileId, rid, out var staleRescan))
            {
                Rescan();
                return;
            }

            if (staleRescan is not null) Rescan(staleRescan);
        }
    }
}
