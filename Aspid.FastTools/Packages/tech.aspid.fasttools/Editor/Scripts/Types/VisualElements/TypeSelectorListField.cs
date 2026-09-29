using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // A [TypeSelector] list of type names or type wrappers. The attribute applies to the collection, so Unity no longer
    // draws its elements with the picker; this list builds each element through the drawer instead, and keeps Unity's
    // own list look and native add.
    internal sealed class TypeSelectorListField : VisualElement
    {
        private const string BlockClass = "aspid-fasttools-type-selector-list";

        // Persists the header foldout's expanded state across selection changes, like Unity's own PropertyField list.
        private const string ViewDataKeyPrefix = "aspid-fasttools-type-selector-list::";

        private readonly SerializedProperty _property;
        private readonly Func<SerializedProperty, VisualElement> _createElementField;

        public TypeSelectorListField(string label, SerializedProperty property,
            Func<SerializedProperty, VisualElement> createElementField)
        {
            _property = property;
            _createElementField = createElementField;

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
                unbindItem = (element, _) => element.Clear(),
            };

            this.AddChild(listView);

            // A list built dynamically inside an already-drawn reference is reached by no ancestor Bind pass; a
            // second bind of the same path is a harmless no-op.
            listView.Bind(property.serializedObject);
        }

        private void BindItem(VisualElement element, int index)
        {
            element.Clear();

            var elementProperty = GetElementProperty(index);
            if (elementProperty is null) return;

            element.Add(_createElementField(elementProperty));
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
