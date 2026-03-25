#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal sealed class StringIdSelectorWindow : EditorWindow
    {
        private const string NoneOption = "<None>";

        private ToolbarSearchField _searchField = null!;
        private ListView _listView = null!;
        private Action<string>? _onSelected;
        private string[] _allIds = Array.Empty<string>();
        private readonly List<string> _filteredIds = new();
        private string _current = string.Empty;

        public static void Show(IReadOnlyList<string> ids, Rect screenRect, string current, Action<string> onSelected)
        {
            var window = CreateInstance<StringIdSelectorWindow>();
            window.Initialize(ids, screenRect, current, onSelected);
        }

        private void Initialize(IReadOnlyList<string> ids, Rect screenRect, string current, Action<string> onSelected)
        {
            _onSelected = onSelected;
            _current = current ?? string.Empty;
            _allIds = ids?.ToArray() ?? Array.Empty<string>();

            BuildUI();
            RefreshList(string.Empty);

            var size = new Vector2(Mathf.Max(250, screenRect.width), 250);
            ShowAsDropDown(screenRect, size);

            _searchField.Focus();
        }

        private void BuildUI()
        {
            _searchField = new ToolbarSearchField()
                .SetMargin(bottom: 4)
                .SetPadding(right: 4)
                .SetSize(width: Length.Auto());

            _searchField.RegisterValueChangedCallback(e => RefreshList(e.newValue ?? string.Empty));
            _searchField.RegisterCallback<NavigationMoveEvent>(e =>
            {
                if (e.move == Vector2.down)
                    _listView.Focus();
            }, TrickleDown.TrickleDown);

            _listView = new ListView
            {
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = _filteredIds,
            };

            _listView.SetMakeItem(CreateItem);
            _listView.SetBindItem(BindItem);
            _listView.itemsChosen += items => SelectItem(items.OfType<string>().FirstOrDefault());

            var container = new VisualElement()
                .SetFlexDirection(FlexDirection.Column)
                .SetFlexGrow(1)
                .SetPadding(top: 4, bottom: 4, left: 4, right: 4)
                .AddChild(_searchField)
                .AddChild(_listView);

            rootVisualElement.Add(container);
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private static VisualElement CreateItem()
        {
            return new Label()
                .SetFlexGrow(1)
                .SetUnityTextAlign(TextAnchor.MiddleLeft)
                .SetPadding(top: 2, bottom: 2, left: 6, right: 6)
                .SetSize(height: 22);
        }

        private void BindItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredIds.Count) return;
            var label = (Label)element;
            var id = _filteredIds[index];
            label.text = id;
            label.style.unityFontStyleAndWeight = id == _current ? FontStyle.Bold : FontStyle.Normal;
        }

        private void RefreshList(string search)
        {
            _filteredIds.Clear();
            _filteredIds.Add(NoneOption);

            foreach (var id in _allIds)
            {
                if (string.IsNullOrEmpty(search) || id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    _filteredIds.Add(id);
            }

            _listView.Rebuild();
        }

        private void SelectItem(string? item)
        {
            if (item == null) return;
            _onSelected?.Invoke(item == NoneOption ? string.Empty : item);
            Close();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Escape)
                Close();
        }
    }
}
