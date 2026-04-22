using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    [CustomEditor(typeof(IdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        private readonly List<EntryItem> _filteredEntries = new();

        private SerializedProperty _targetTypeProp;
        private SerializedProperty _entriesProp;

        private Label _emptyLabel;
        private ListView _listView;
        private string _searchQuery = string.Empty;

        private void OnEnable()
        {
            _targetTypeProp = serializedObject.FindProperty("_targetStructType");
            _entriesProp = serializedObject.FindProperty("_entries");
        }

        private void OnDisable()
        {
            if (target == null) return;
            StringIdRegistryValidator.CleanUpInvalid(target);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement()
                .AddStyleSheetsFromResource(Constants.StyleSheetPath)
                .AddStyleSheetsFromResource(StyleClasses.DefaultStyleSheet)
                .AddClass(Constants.Registry.Root)
                .AddClass("aspid-fasttools-inspector-container");

            root.Add(new AspidInspectorHeader("None", target) { Subtext = "None" });

            var typeContainer = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-dark")
                .AddClass("aspid-fasttools-background");

            var container = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-light")
                .AddClass("aspid-fasttools-background");

            typeContainer.Add(new AspidLabel("Type").SetMarginBottom(5));
            typeContainer.Add(new PropertyField(_targetTypeProp, label: string.Empty));
            container.Add(BuildSectionTitle("IDs"));

            var searchField = new ToolbarSearchField();
            searchField.AddClass(Constants.Registry.Search);
            searchField.RegisterValueChangedCallback(e =>
            {
                _searchQuery = e.newValue ?? string.Empty;
                RebuildEntries();
            });
            container.Add(searchField);

            _listView = new ListView
            {
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = _filteredEntries,
                reorderable = false,
                showBorder = false,
                showFoldoutHeader = false,
                showBoundCollectionSize = false,
                showAddRemoveFooter = false,
            };
            _listView.AddClass(Constants.Registry.List);
            _listView.SetMakeItem(CreateEntryRow);
            _listView.SetBindItem(BindEntryRow);
            container.Add(_listView);

            _emptyLabel = new Label("No IDs yet. Add one below.")
                .AddClass(Constants.Registry.Empty);
            container.Add(_emptyLabel);
            container.Add(BuildRegistryAddRow());

            container.TrackSerializedObjectValue(serializedObject, _ => RebuildEntries());
            RebuildEntries();

            return root
                .AddChild(typeContainer)
                .AddChild(container);
        }

        private VisualElement BuildSectionTitle(string text)
        {
            var header = new VisualElement()
                .AddClass(Constants.Registry.SectionTitleHeader)
                .AddChild(new Label(text).AddClass(Constants.Registry.SectionTitleText));

            return new VisualElement()
                .AddClass(Constants.Registry.SectionTitle)
                .AddChild(header)
                .AddChild(new VisualElement().AddClass(Constants.Registry.SectionTitleLine));
        }

        private void RebuildEntries()
        {
            _filteredEntries.Clear();
            var duplicates = StringIdRegistryValidator.GetDuplicates(_entriesProp);
            var count = _entriesProp.arraySize;

            if (_emptyLabel != null)
                _emptyLabel.EnableInClassList(Constants.Registry.EmptyVisible, count == 0);

            var query = _searchQuery?.Trim() ?? string.Empty;
            for (int i = 0; i < count; i++)
            {
                var element = _entriesProp.GetArrayElementAtIndex(i);
                var name = element.FindPropertyRelative("Name").stringValue;
                var id = element.FindPropertyRelative("Id").intValue;

                if (!MatchesQuery(name, id, query)) continue;

                _filteredEntries.Add(new EntryItem(i, name, id, duplicates.Contains(name)));
            }

            _listView?.Rebuild();
            UpdateListScrollState();
        }

        private void UpdateListScrollState()
        {
            if (_listView == null) return;

            if (_filteredEntries.Count >= Constants.Registry.ScrollThreshold)
            {
                const float height = Constants.Registry.MaxVisibleRows * Constants.Registry.RowHeight;
                _listView.AddToClassList(Constants.Registry.ListScrollable);
                _listView.style.height = height;
                _listView.style.maxHeight = height;
            }
            else
            {
                _listView.RemoveFromClassList(Constants.Registry.ListScrollable);
                _listView.style.height = StyleKeyword.Null;
                _listView.style.maxHeight = StyleKeyword.Null;
            }
        }

        private static bool MatchesQuery(string name, int id, string query)
        {
            if (string.IsNullOrEmpty(query)) return true;
            if (name != null && name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return id.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private VisualElement CreateEntryRow()
        {
            var container = new VisualElement().AddClass(Constants.Registry.Entry);
            var row = new VisualElement().AddClass(Constants.Registry.Row);

            var idBadge = new Label().AddClass(Constants.Registry.IdBadge);
            row.Add(idBadge);

            var nameField = new TextField();
            nameField.AddClass(Constants.Registry.Name);
            row.Add(nameField);

            var deleteButton = new Button { text = "×" };
            deleteButton.AddClass(Constants.Registry.Delete);
            row.Add(deleteButton);

            container.Add(row);

            var errorLabel = new Label().AddClass(Constants.Drawer.Error);
            container.Add(errorLabel);

            var state = new EntryRowState
            {
                NameField = nameField,
                IdBadge = idBadge,
                ErrorLabel = errorLabel,
            };
            container.userData = state;

            nameField.RegisterCallback<FocusInEvent>(_ =>
            {
                if (!state.Item.HasValue) return;
                if (StringIdRegistryValidator.HasDuplicate((IdRegistry)target, state.Item.Value.Name))
                {
                    errorLabel.text = "Name already exists.";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
            });

            nameField.RegisterValueChangedCallback(e =>
            {
                if (!state.Item.HasValue) return;
                var item = state.Item.Value;
                var t = e.newValue?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                if (string.IsNullOrEmpty(t))
                {
                    errorLabel.text = "Name cannot be empty.";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
                else if (StringIdRegistryValidator.HasDuplicate(registry, t) || (t != item.Name && registry.Contains(t)))
                {
                    errorLabel.text = $"'{t}' already exists.";
                    state.IdBadge.AddClass(StyleClasses.Status.Error);
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
                else
                {
                    state.IdBadge.RemoveClass(StyleClasses.Status.Error);
                    errorLabel.SetDisplay(DisplayStyle.None);
                }
            });

            nameField.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (!state.Item.HasValue) return;
                var item = state.Item.Value;
                var t = nameField.value?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                if (!string.IsNullOrEmpty(t) && t != item.Name && !registry.Contains(t))
                {
                    serializedObject.ApplyModifiedProperties();
                    registry.Rename(item.Name, t);
                    EditorUtility.SetDirty(registry);
                    serializedObject.Update();
                    errorLabel.SetDisplay(DisplayStyle.None);
                }
                else
                {
                    nameField.SetValueWithoutNotify(item.Name);
                    if (StringIdRegistryValidator.HasDuplicate(registry, item.Name))
                    {
                        errorLabel.text = "Name already exists.";
                        errorLabel.SetDisplay(DisplayStyle.Flex);
                    }
                    else
                    {
                        errorLabel.SetDisplay(DisplayStyle.None);
                    }
                }
            });

            deleteButton.clicked += () =>
            {
                if (!state.Item.HasValue) return;
                TryDeleteEntry(state.Item.Value.OriginalIndex);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            };

            return container;
        }

        private void BindEntryRow(VisualElement element, int visibleIndex)
        {
            if (visibleIndex < 0 || visibleIndex >= _filteredEntries.Count) return;
            if (element.userData is not EntryRowState state) return;

            var item = _filteredEntries[visibleIndex];
            state.Item = item;

            element.EnableInClassList(Constants.Registry.EntryDuplicate, item.IsDuplicate);
            state.NameField.SetValueWithoutNotify(item.Name);
            state.IdBadge.text = item.Id.ToString();

            if (item.IsDuplicate)
            {
                state.ErrorLabel.text = "Name already exists.";
                state.ErrorLabel.SetDisplay(DisplayStyle.Flex);
            }
            else
            {
                state.ErrorLabel.SetDisplay(DisplayStyle.None);
            }
        }

        private VisualElement BuildRegistryAddRow()
        {
            var row = new VisualElement().AddClass(Constants.Registry.AddRow);

            var inputField = new TextField();
            inputField.AddClass(Constants.Registry.AddInput);

            var addButton = new Button { text = "Add" };
            addButton.AddClass(Constants.Registry.AddButton);
            addButton.SetEnabled(false);

            inputField.RegisterValueChangedCallback(e =>
            {
                var val = e.newValue?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                addButton.SetEnabled(!string.IsNullOrEmpty(val) && !registry.Contains(val));
            });

            addButton.clicked += () =>
            {
                var val = inputField.value?.Trim();
                if (string.IsNullOrEmpty(val)) return;
                var registry = (IdRegistry)target;
                serializedObject.ApplyModifiedProperties();
                registry.Add(val);
                EditorUtility.SetDirty(registry);
                serializedObject.Update();
                inputField.SetValueWithoutNotify(string.Empty);
                addButton.SetEnabled(false);
            };

            row.Add(inputField);
            row.Add(addButton);
            return row;
        }

        private void TryDeleteEntry(int index)
        {
            var nameProp = _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name");
            var nameToDelete = nameProp.stringValue;
            var usageCount = StringIdUsageScanner.CountUsages(GetStructType(), nameToDelete);

            var message = usageCount == 0
                ? $"Delete '{nameToDelete}'?"
                : $"'{nameToDelete}' is used in {usageCount} asset(s).\n\nFields referencing this ID will show <Missing> after deletion.\n\nDelete anyway?";

            if (EditorUtility.DisplayDialog("Delete ID", message, "Delete", "Cancel"))
                _entriesProp.DeleteArrayElementAtIndex(index);
        }

        private Type GetStructType()
        {
            var aqn = _targetTypeProp.stringValue;
            return string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false);
        }

        private readonly struct EntryItem
        {
            public readonly int OriginalIndex;
            public readonly string Name;
            public readonly int Id;
            public readonly bool IsDuplicate;

            public EntryItem(int originalIndex, string name, int id, bool isDuplicate)
            {
                OriginalIndex = originalIndex;
                Name = name;
                Id = id;
                IsDuplicate = isDuplicate;
            }
        }

        private sealed class EntryRowState
        {
            public EntryItem? Item;
            public TextField NameField;
            public Label IdBadge;
            public Label ErrorLabel;
        }
    }

}
