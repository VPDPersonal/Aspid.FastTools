using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // UIToolkit list for a [SerializeReference] array whose field carries no [TypeSelector]: every element renders
    // as a SerializeReferenceField and the "+" opens the type picker. With the attribute, Unity routes each element
    // through the drawer and the add behavior retrofits the picker onto Unity's own ListView; without it there is no
    // element drawer to route through, so this field rebuilds that list shape itself.
    internal sealed class SerializeReferenceListField : VisualElement
    {
        private const string BlockClass = "aspid-fasttools-serialize-reference-list";

        // Persists the header foldout's expanded state across selection changes, like Unity's own PropertyField list.
        private const string ViewDataKeyPrefix = "aspid-fasttools-serialize-reference-list::";

        private readonly Type[] _baseTypes;
        private readonly SerializedProperty _property;

        // Carried into the element fields, so a graph nested through lists counts toward the same depth cap as one
        // nested through plain fields.
        private readonly int _depth;

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
                reorderable = true,
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

            // Single-object only: under a multi-object selection the native add stays and the duplicate guard
            // de-aliases the copies. Set before any attach, so the item fields' own install short-circuits on it.
            var serializedObject = property.serializedObject;
            if (!serializedObject.isEditingMultipleObjects)
            {
                var target = serializedObject.targetObject;
                var arrayPath = property.propertyPath;
                listView.overridingAddButtonBehavior = (_, button) =>
                    SerializeReferenceListAddBehavior.OpenAppendPicker(target, arrayPath, elementType, _baseTypes, button);
            }

            this.AddChild(listView);

            // A list built dynamically inside an already-drawn reference is reached by no ancestor Bind pass; a
            // second bind of the same path is a harmless no-op.
            listView.Bind(serializedObject);
        }

        private void BindItem(VisualElement element, int index)
        {
            element.Clear();

            var elementProperty = GetElementProperty(index);
            if (elementProperty is null) return;

            element.Add(new SerializeReferenceField(elementProperty.displayName, elementProperty, _baseTypes, _depth));
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
