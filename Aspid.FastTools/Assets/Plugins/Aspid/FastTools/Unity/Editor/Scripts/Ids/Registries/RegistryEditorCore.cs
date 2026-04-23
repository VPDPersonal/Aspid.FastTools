#nullable enable
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Builds the shared inspector UI for both Id registry types.
    /// UI-only — all storage access goes through <see cref="IRegistryAccessor"/>.
    /// </summary>
    internal sealed class RegistryEditorCore
    {
        private readonly IRegistryAccessor _accessor;

        private readonly List<EntryView> _viewModel = new();
        private Label? _emptyLabel;
        private ListView? _listView;
        private VisualElement? _warningRow;
        private Label? _warningLabel;
        private string _searchQuery = string.Empty;

        public RegistryEditorCore(IRegistryAccessor accessor)
        {
            _accessor = accessor;
        }

        public VisualElement Build()
        {
            var root = new VisualElement()
                .AddStyleSheetsFromResource(Constants.Registry.StyleSheetPath)
                .AddStyleSheetsFromResource(StyleClasses.DefaultStyleSheet)
                .AddClass(Constants.Registry.Root)
                .AddClass("aspid-fasttools-inspector-container");

            root.Add(new AspidInspectorHeader(_accessor.Target.name, _accessor.Target)
            {
                Subtext = _accessor.Target.GetType().Name,
            });

            var typeContainer = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-dark")
                .AddClass("aspid-fasttools-background");

            typeContainer.Add(new AspidLabel("Type").SetMarginBottom(5));
            typeContainer.Add(new PropertyField(_accessor.TargetStructTypeProperty, label: string.Empty));
            typeContainer.Add(BuildNextIdRow());

            var container = new VisualElement()
                .SetMarginTop(5)
                .AddClass("aspid-fasttools-light")
                .AddClass("aspid-fasttools-background");

            container.Add(BuildSectionTitle("IDs"));
            container.Add(BuildWarningRow());

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
                itemsSource = _viewModel,
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
            container.Add(BuildAddRow());

            container.TrackSerializedObjectValue(_accessor.SerializedObject, _ => RebuildEntries());
            RebuildEntries();

            return root
                .AddChild(typeContainer)
                .AddChild(container);
        }

        private static VisualElement BuildSectionTitle(string text) =>
            new AspidLabel(text, new LabelPreset()
                .SetTheme(ThemeStyle.Light)
                .SetLabelSize(AspidLabelSizeStyle.H2)
                .SetLineSize(DividingLineSize.Medium));

        private void RebuildEntries()
        {
            _viewModel.Clear();
            var count = _accessor.Count;

            if (_emptyLabel != null)
                _emptyLabel.EnableInClassList(Constants.Registry.EmptyVisible, count == 0);

            var duplicates = new HashSet<string>();
            var seen = new HashSet<string>();
            for (var i = 0; i < count; i++)
            {
                var name = _accessor.GetName(i);
                if (!string.IsNullOrEmpty(name) && !seen.Add(name))
                    duplicates.Add(name);
            }

            var query = _searchQuery?.Trim() ?? string.Empty;
            for (var i = 0; i < count; i++)
            {
                var name = _accessor.GetName(i);
                var id = _accessor.GetId(i);
                if (!MatchesQuery(name, id, query)) continue;

                _viewModel.Add(new EntryView(i, name, id, duplicates.Contains(name)));
            }

            _listView?.Rebuild();
            UpdateListScrollState();
            RefreshWarningRow();
        }

        private void UpdateListScrollState()
        {
            if (_listView == null) return;

            if (_viewModel.Count >= Constants.Registry.ScrollThreshold)
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
            if (!string.IsNullOrEmpty(name) && name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return id.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private VisualElement CreateEntryRow()
        {
            var row = new IdRegistryEntryVisualElement();
            row.NameFocusIn += OnRowNameFocusIn;
            row.NameChanging += OnRowNameChanging;
            row.NameCommitRequested += OnRowNameCommitRequested;
            row.DeleteRequested += OnRowDeleteRequested;
            return row;
        }

        private void BindEntryRow(VisualElement element, int visibleIndex)
        {
            if (visibleIndex < 0 || visibleIndex >= _viewModel.Count) return;
            var view = _viewModel[visibleIndex];
            ((IdRegistryEntryVisualElement)element).Bind(new IdRegistryEntryData(
                originalIndex: view.OriginalIndex,
                name: view.Name,
                id: view.Id,
                isDuplicate: view.IsDuplicate));
        }

        private void OnRowNameFocusIn(IdRegistryEntryVisualElement row, IdRegistryEntryData data)
        {
            if (data.IsDuplicate)
                row.SetError("Name already exists.");
        }

        private void OnRowNameChanging(IdRegistryEntryVisualElement row, IdRegistryEntryData data, string newValue)
        {
            var trimmed = newValue?.Trim() ?? string.Empty;

            if (trimmed == data.Name)
            {
                row.SetEditMode(false);
                row.ClearError();
                return;
            }

            var existing = CollectExistingNames(exceptIndex: data.OriginalIndex);
            if (IdRegistryValidator.IsValidName(trimmed, existing, out var error))
            {
                row.SetEditMode(true, canConfirm: true);
                row.ClearError();
            }
            else
            {
                row.SetEditMode(true, canConfirm: false);
                row.SetError(error!);
            }
        }

        private void OnRowNameCommitRequested(IdRegistryEntryVisualElement row, IdRegistryEntryData data, string rawValue)
        {
            var trimmed = rawValue?.Trim() ?? string.Empty;
            if (trimmed == data.Name || string.IsNullOrEmpty(trimmed)) return;

            var existing = CollectExistingNames(exceptIndex: data.OriginalIndex);
            if (!IdRegistryValidator.IsValidName(trimmed, existing, out _)) return;

            _accessor.Record($"Rename ID '{data.Name}' → '{trimmed}'");
            _accessor.SetName(data.OriginalIndex, trimmed);
            _accessor.Commit();
            row.SetEditMode(false);
            row.ClearError();
        }

        private void OnRowDeleteRequested(IdRegistryEntryVisualElement row, IdRegistryEntryData data)
        {
            var name = data.Name;
            if (!EditorUtility.DisplayDialog(
                    "Delete ID",
                    $"Delete '{name}'?\n\nAssets referencing this ID will display <Missing> until reassigned.",
                    "Delete",
                    "Cancel"))
                return;

            _accessor.Record($"Delete ID '{name}'");
            _accessor.RemoveAt(data.OriginalIndex);
            _accessor.Commit();
        }

        private VisualElement BuildNextIdRow()
        {
            var row = new VisualElement().AddClass(Constants.Registry.NextIdRow);

            var label = new Label("Next ID").AddClass(Constants.Registry.NextIdLabel);

            var field = new IntegerField
            {
                value = _accessor.NextIdProperty.intValue,
                tooltip = "Id that will be assigned to the next Add operation. Manual override is allowed.",
            }.AddClass(Constants.Registry.NextIdField);

            var warning = new Image
            {
                image = EditorGUIUtility.IconContent("console.warnicon.sml").image,
                tooltip = string.Empty,
            }.AddClass(Constants.Registry.NextIdWarning);

            field.RegisterValueChangedCallback(e =>
            {
                var newValue = e.newValue;
                UpdateNextIdWarning(warning, newValue);

                _accessor.Record("Set Next ID");
                _accessor.NextIdProperty.intValue = newValue;
                _accessor.Commit();
            });

            UpdateNextIdWarning(warning, _accessor.NextIdProperty.intValue);

            row.Add(label);
            row.Add(field);
            row.Add(warning);
            return row;
        }

        private void UpdateNextIdWarning(Image warning, int value)
        {
            var maxAssigned = _accessor.MaxAssignedId;
            var show = value <= maxAssigned && value >= 1;
            warning.EnableInClassList(Constants.Registry.NextIdWarningVisible, show);
            warning.tooltip = show
                ? $"Reusing ID {value} may silently remap references: assets that previously pointed to this ID will appear bound to the next name you create. Proceed only if you know these IDs are unused."
                : value < 1
                    ? "Next ID must be ≥ 1."
                    : string.Empty;
        }

        private VisualElement BuildWarningRow()
        {
            var row = new VisualElement().AddClass(Constants.Registry.Warning);
            _warningRow = row;

            _warningLabel = new Label().AddClass(Constants.Registry.WarningLabel);
            var reviewButton = new Button { text = "Review" }.AddClass(Constants.Registry.WarningButton);
            reviewButton.clicked += ShowCleanUpDialog;

            row.Add(_warningLabel);
            row.Add(reviewButton);
            return row;
        }

        private void RefreshWarningRow()
        {
            if (_warningRow == null || _warningLabel == null) return;
            var summary = IdRegistryValidator.Summarize(_accessor);
            var visible = summary.Total > 0;
            _warningRow.EnableInClassList(Constants.Registry.WarningVisible, visible);
            if (visible) _warningLabel.text = summary.ToShortLabel();
        }

        private void ShowCleanUpDialog()
        {
            var summary = IdRegistryValidator.Summarize(_accessor);
            if (summary.Total == 0) return;

            var message = $"This will remove {summary.Total} invalid entr{(summary.Total == 1 ? "y" : "ies")}:\n"
                        + (summary.DuplicateCount > 0 ? $"  • {summary.DuplicateCount} duplicate name(s)\n" : string.Empty)
                        + (summary.EmptyCount > 0 ? $"  • {summary.EmptyCount} empty name(s)\n" : string.Empty)
                        + (summary.StructuralIssues > 0 ? "  • structural inconsistencies\n" : string.Empty)
                        + "\nProceed?";

            if (!EditorUtility.DisplayDialog("Clean up invalid entries", message, "Clean up", "Cancel"))
                return;

            _accessor.Record("Clean Up Invalid IDs");

            var seen = new HashSet<string>();
            var toRemove = new List<int>();
            for (var i = 0; i < _accessor.Count; i++)
            {
                var name = _accessor.GetName(i);
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                    toRemove.Add(i);
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
                _accessor.RemoveAt(toRemove[i]);

            _accessor.Commit();
        }

        private VisualElement BuildAddRow()
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
                var existing = CollectExistingNames(exceptIndex: -1);
                var ok = IdRegistryValidator.IsValidName(val, existing, out _);
                addButton.SetEnabled(ok);
            });

            addButton.clicked += () =>
            {
                var val = inputField.value?.Trim();
                if (string.IsNullOrEmpty(val)) return;

                _accessor.Record($"Add ID '{val}'");
                _accessor.Add(val);
                _accessor.Commit();

                inputField.SetValueWithoutNotify(string.Empty);
                addButton.SetEnabled(false);

                RebuildEntries();
                var newIndex = _viewModel.FindIndex(v => v.Name == val);
                if (newIndex < 0 || _listView == null) return;
                _listView.schedule.Execute(() => _listView.ScrollToItem(newIndex)).StartingIn(0);
            };

            row.Add(inputField);
            row.Add(addButton);
            return row;
        }

        private HashSet<string> CollectExistingNames(int exceptIndex)
        {
            var set = new HashSet<string>();
            var count = _accessor.Count;
            for (var i = 0; i < count; i++)
            {
                if (i == exceptIndex) continue;
                var name = _accessor.GetName(i);
                if (!string.IsNullOrEmpty(name))
                    set.Add(name);
            }
            return set;
        }

        private readonly struct EntryView
        {
            public readonly int OriginalIndex;
            public readonly string Name;
            public readonly int Id;
            public readonly bool IsDuplicate;

            public EntryView(int originalIndex, string name, int id, bool isDuplicate)
            {
                OriginalIndex = originalIndex;
                Name = name;
                Id = id;
                IsDuplicate = isDuplicate;
            }
        }
    }
}
