using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    [CustomEditor(typeof(StringIdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        private readonly List<StringIdRegistryEntryData> _filteredEntries = new();

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
                .AddStyleSheetsFromResource(Constants.Registry.StyleSheetPath)
                .AddStyleSheetsFromResource(StyleClasses.DefaultStyleSheet)
                .AddClass(Constants.Registry.Root)
                .AddClass("aspid-fasttools-inspector-container");

            root.Add(new AspidInspectorHeader(target.name, target) { Subtext = target.GetType().Name });

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

        private static VisualElement BuildSectionTitle(string text)
        {
            return new AspidLabel(text, new LabelPreset()
                .SetTheme(ThemeStyle.Light)
                .SetLabelSize(AspidLabelSizeStyle.H2)
                .SetLineSize(DividingLineSize.Medium));
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

                _filteredEntries.Add(new StringIdRegistryEntryData(i, name, id, duplicates.Contains(name)));
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
            var row = new StringIdRegistryEntryVisualElement();
            row.NameFocusIn += OnRowNameFocusIn;
            row.NameChanging += OnRowNameChanging;
            row.NameCommitRequested += OnRowNameCommitRequested;
            row.DeleteRequested += OnRowDeleteRequested;
            return row;
        }

        private void BindEntryRow(VisualElement element, int visibleIndex)
        {
            if (visibleIndex < 0 || visibleIndex >= _filteredEntries.Count) return;
            ((StringIdRegistryEntryVisualElement)element).Bind(_filteredEntries[visibleIndex]);
        }

        private void OnRowNameFocusIn(StringIdRegistryEntryVisualElement row, StringIdRegistryEntryData data)
        {
            if (StringIdRegistryValidator.HasDuplicate((StringIdRegistry)target, data.Name))
                row.SetError("Name already exists.");
        }

        private void OnRowNameChanging(StringIdRegistryEntryVisualElement row, StringIdRegistryEntryData data, string newValue)
        {
            var trimmed = newValue?.Trim() ?? string.Empty;
            var registry = (StringIdRegistry)target;

            if (trimmed == data.Name)
            {
                row.SetEditMode(false);
                row.ClearError();
            }
            else if (string.IsNullOrEmpty(trimmed))
            {
                row.SetEditMode(true, canConfirm: false);
                row.SetError("Name cannot be empty.");
            }
            else if (StringIdRegistryValidator.HasDuplicate(registry, trimmed) || registry.Contains(trimmed))
            {
                row.SetEditMode(true, canConfirm: false);
                row.SetError($"'{trimmed}' already exists.");
            }
            else
            {
                row.SetEditMode(true, canConfirm: true);
                row.ClearError();
            }
        }

        private void OnRowNameCommitRequested(StringIdRegistryEntryVisualElement row, StringIdRegistryEntryData data, string rawValue)
        {
            var trimmed = rawValue?.Trim() ?? string.Empty;
            var registry = (StringIdRegistry)target;

            if (string.IsNullOrEmpty(trimmed) || trimmed == data.Name || registry.Contains(trimmed)) return;

            serializedObject.ApplyModifiedProperties();
            registry.Rename(data.Name, trimmed);
            EditorUtility.SetDirty(registry);
            serializedObject.Update();
            row.SetEditMode(false);
            row.ClearError();
        }

        private void OnRowDeleteRequested(StringIdRegistryEntryVisualElement row, StringIdRegistryEntryData data)
        {
            TryDeleteEntry(data.OriginalIndex);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private VisualElement BuildRegistryAddRow()
        {
            var row = new VisualElement().AddClass(Constants.Registry.AddRow);

            var inputField = new TextField();
            inputField.AddClass(Constants.Registry.AddInput);

            var addButton = new Button { text = "+" };
            addButton.AddClass(Constants.Registry.AddButton);
            addButton.SetEnabled(false);

            inputField.RegisterValueChangedCallback(e =>
            {
                var val = e.newValue?.Trim() ?? string.Empty;
                var registry = (StringIdRegistry)target;
                addButton.SetEnabled(!string.IsNullOrEmpty(val) && !registry.Contains(val));
            });

            addButton.clicked += () =>
            {
                var val = inputField.value?.Trim();
                if (string.IsNullOrEmpty(val)) return;
                var registry = (StringIdRegistry)target;
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
            var nameProp = _entriesProp.GetArrayElementAtIndex(index)
                .FindPropertyRelative("Name");
            
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
    }
}
