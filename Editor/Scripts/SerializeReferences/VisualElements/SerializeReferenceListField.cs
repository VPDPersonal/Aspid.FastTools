using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceListField : VisualElement
    {
        private const string BlockClass = "aspid-fasttools-serialize-reference-list";

        // Persists the header foldout's expanded state across selection changes, like Unity's own PropertyField list.
        private const string ViewDataKeyPrefix = "aspid-fasttools-serialize-reference-list::";

        private readonly SerializedProperty _property;

        // Carried into the element fields, so a graph nested through lists counts toward the same depth cap as one
        // nested through plain fields.
        private readonly int _depth;

        // The element fields currently bound, so a re-resolved constraint reaches them without a rebind.
        private readonly HashSet<SerializeReferenceField> _elementFields = new();

        // Mutable on purpose: a [TypeSelector] member-referenced constraint re-resolves while the inspector is open
        // and replaces it through SetBaseTypes; the add picker reads it when it opens.
        private Type[] _baseTypes;

        public SerializeReferenceListField(string label, SerializedProperty property, Type elementType,
            Type[] baseTypes = null, int depth = 0)
        {
            _property = property;
            _baseTypes = baseTypes;
            _depth = depth;

            this.AddClass(BlockClass);

            var listView = new ListView
            {
                showBorder = true,
                reorderable = !property.IsNonReorderable(),
                showFoldoutHeader = true,
                headerTitle = label,
                showAddRemoveFooter = true,
                showBoundCollectionSize = true,
                selectionType = SelectionType.Multiple,
                reorderMode = ListViewReorderMode.Animated,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                bindingPath = property.propertyPath,
                viewDataKey = ViewDataKeyPrefix + property.propertyPath,
                makeItem = () => new VisualElement(),
                bindItem = BindItem,
                unbindItem = UnbindItem,
            };

            // Under a multi-object selection too: the native add would copy the last element's rid into every object,
            // and the duplicate guard does not watch multi-object selections. Set before any attach, so the item
            // fields' own install short-circuits on it.
            var serializedObject = property.serializedObject;
            var targets = serializedObject.targetObjects;
            var arrayPath = property.propertyPath;
            listView.overridingAddButtonBehavior = (_, button) =>
                SerializeReferenceListAddBehavior.OpenAppendPicker(targets, arrayPath, elementType, _baseTypes, button);

            this.AddChild(listView);

            // A list built dynamically inside an already-drawn reference is reached by no ancestor Bind pass; a
            // second bind of the same path is a harmless no-op.
            listView.Bind(serializedObject);
        }

        // Replaces the constraint after construction, for the add picker and every bound element field alike.
        internal void SetBaseTypes(Type[] baseTypes)
        {
            _baseTypes = baseTypes;
            foreach (var field in _elementFields) field.SetBaseTypes(baseTypes);
        }

        private void BindItem(VisualElement element, int index)
        {
            UnbindItem(element, index);

            var elementProperty = GetElementProperty(index);
            if (elementProperty is null) return;

            // The same per-element hand-off as the IMGUI list, so both backends draw an element with the same drawer.
            if (!SerializeReferenceNesting.DrawsOwnHeader(elementProperty, _depth))
            {
                var field = new PropertyField(elementProperty);
                field.BindProperty(elementProperty);
                element.Add(field);
                return;
            }

            var elementField = new SerializeReferenceField(elementProperty.displayName, elementProperty, _baseTypes, _depth);
            _elementFields.Add(elementField);
            element.Add(elementField);
        }

        private void UnbindItem(VisualElement element, int index)
        {
            if (element.childCount > 0 && element[0] is SerializeReferenceField elementField)
                _elementFields.Remove(elementField);

            element.Clear();
        }

        // Null while the view and the data disagree, such as just after a tail element is removed. The next binding
        // refresh rebuilds the rows, so a transient miss only has to avoid throwing.
        private SerializedProperty GetElementProperty(int index)
        {
            try
            {
                if (_property.serializedObject?.targetObject == null) return null;
                if (index < 0 || index >= _property.arraySize) return null;
                return _property.GetArrayElementAtIndex(index);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
