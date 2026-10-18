using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A list in the look of Unity's own, framed in one block with its caption: a "−" at the end of each row and a "+"
    // at the end of the last one, a click that selects a row and a double click that picks another folder for it.
    internal sealed class SerializeReferenceExcludedFoldersField : VisualElement
    {
        private const string StyleSheetPath =
            "UI/SerializeReferences/Aspid-FastTools-SerializeReference-ExcludedFolders";

        private const string RootClass = "aspid-fasttools-excluded-folders";
        private const string BoxClass = "aspid-fasttools-excluded-folders__box";
        private const string CaptionClass = "aspid-fasttools-excluded-folders__caption";
        private const string ListClass = "aspid-fasttools-excluded-folders__list";
        private const string EntryClass = "aspid-fasttools-excluded-folders__entry";
        private const string EntrySelectedClass = "aspid-fasttools-excluded-folders__entry--selected";
        private const string EmptyClass = "aspid-fasttools-excluded-folders__empty";
        private const string PathClass = "aspid-fasttools-excluded-folders__path";
        private const string RowButtonClass = "aspid-fasttools-excluded-folders__row-button";
        private const string RowSlotClass = "aspid-fasttools-excluded-folders__row-slot";
        internal const string AddButtonClass = "aspid-fasttools-excluded-folders__add";
        internal const string AddTargetClass = "aspid-fasttools-excluded-folders__add-target";
        internal const string RemoveButtonClass = "aspid-fasttools-excluded-folders__remove";

        private readonly VisualElement _list;
        private readonly VisualElement _addTarget;

        private readonly List<(VisualElement Row, string Path)> _rows = new();
        private string _selected;

        // The hosting keyboard ring listens to re-collect its targets, since a rebuild replaces every row element.
        internal event Action RowsRebuilt;

        public SerializeReferenceExcludedFoldersField()
        {
            this.AddClass(RootClass)
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            var caption = new Label("Excluded scan folders").AddClass(CaptionClass);
            _list = new VisualElement().AddClass(ListClass);

            // The "+" rides on a layer that spans the last row behind its text, so the keyboard focus outlines the
            // whole row, as it does for a row and its "−". One layer moves to the last row on every rebuild, so the
            // ring keeps a single add target; it passes clicks through to the row except on the "+".
            var addButton = new Button(AddFolder) { text = "+", tooltip = "Add a folder" }
                .AddClass(RowButtonClass)
                .AddClass(AddButtonClass);

            _addTarget = new VisualElement().AddClass(AddTargetClass)
                .SetPickingMode(PickingMode.Ignore)
                .AddChild(addButton);

            // The frame is an inner block, so the scope stripe of the row stays at the row's edge.
            Add(new VisualElement().AddClass(BoxClass)
                .AddChild(caption)
                .AddChild(_list));

            Rebuild();

            // Armed at build time, then following the panel lifecycle: docking re-parents the tree — a detach and an
            // attach with no rebuild — which would kill a build-time-only subscription.
            var subscribed = false;

            void Arm()
            {
                if (subscribed) return;
                subscribed = true;
                SerializeReferenceSettings.ExcludedFoldersChanged += Rebuild;
                Rebuild();
            }

            void Disarm()
            {
                if (!subscribed) return;
                subscribed = false;
                SerializeReferenceSettings.ExcludedFoldersChanged -= Rebuild;
            }

            RegisterCallback<AttachToPanelEvent>(_ => Arm());
            RegisterCallback<DetachFromPanelEvent>(_ => Disarm());
            Arm();
        }

        private void Rebuild()
        {
            _list.Clear();
            _rows.Clear();

            var folders = SerializeReferenceSettings.ExcludedFolders;
            if (!folders.Contains(_selected, StringComparer.Ordinal)) _selected = null;

            // The box stays framed when empty: its one row says so and holds the "+".
            if (folders.Length == 0)
            {
                _list.Add(new VisualElement().AddClass(EntryClass)
                    .AddChild(_addTarget)
                    .AddChild(new Label("List is empty").AddClass(EmptyClass))
                    .AddChild(CreateSlot()));
            }

            foreach (var path in folders)
            {
                var remove = new Button(() => Remove(path)) { text = "−", tooltip = "Remove this folder" }
                    .AddClass(RowButtonClass)
                    .AddClass(RemoveButtonClass);

                // Every row keeps a slot where the last one shows its "+", so the "−" buttons stay in one column.
                var row = new VisualElement { tooltip = path }.AddClass(EntryClass)
                    .AddChild(new Label(path).AddClass(PathClass))
                    .AddChild(remove)
                    .AddChild(CreateSlot());

                row.RegisterCallback<ClickEvent>(evt =>
                {
                    // The buttons handle their own clicks; the row only selects and edits.
                    if (evt.target is Button) return;

                    if (evt.clickCount >= 2) Edit(path);
                    else Select(path);
                });

                _list.Add(row);
                _rows.Add((row, path));
            }

            if (_rows.Count > 0) _rows[^1].Row.Insert(0, _addTarget);

            UpdateSelection();
            RowsRebuilt?.Invoke();
        }

        private static VisualElement CreateSlot() =>
            new VisualElement().AddClass(RowButtonClass).AddClass(RowSlotClass).SetPickingMode(PickingMode.Ignore);

        private void Select(string path)
        {
            _selected = path;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            foreach (var (row, path) in _rows)
                row.EnableInClassList(EntrySelectedClass, string.Equals(path, _selected, StringComparison.Ordinal));
        }

        internal IEnumerable<(VisualElement Element, Action Activate, Action Remove)> GetNavTargets()
        {
            // A focused row stands for its "−": Enter and Delete remove the folder, and the stylesheet lights the "−"
            // up. Picking another folder for a row stays on the double click.
            foreach (var (row, path) in _rows)
                yield return (row, () => Remove(path), () => Remove(path));

            yield return (_addTarget, AddFolder, null);
        }

        private void AddFolder()
        {
            var relative = PickProjectFolder("Exclude folder from scan", "Assets");
            if (relative == null) return;

            SerializeReferenceSettings.UpdateExcludedFolders(current =>
                current.Contains(relative) ? current : current.Append(relative).ToArray());
        }

        private void Edit(string folder)
        {
            var relative = PickProjectFolder("Edit excluded folder", folder);
            if (relative == null || string.Equals(relative, folder, StringComparison.Ordinal)) return;

            SerializeReferenceSettings.UpdateExcludedFolders(current => current
                .Select(f => string.Equals(f, folder, StringComparison.Ordinal) ? relative : f)
                .Distinct()
                .ToArray());
        }

        private static string PickProjectFolder(string title, string startFolder)
        {
            var absolute = EditorUtility.OpenFolderPanel(title, startFolder, string.Empty);
            if (string.IsNullOrEmpty(absolute)) return null;

            var relative = FileUtil.GetProjectRelativePath(absolute);
            if (IsScannedFolder(relative)) return relative;

            EditorUtility.DisplayDialog(
                "Folder not scanned",
                "Project scans cover only Assets/. Pick Assets or a folder under it.",
                "OK");
            return null;
        }

        // Scans walk only Assets/, so an exclusion anywhere else would be a dead entry in the shared settings.
        internal static bool IsScannedFolder(string relative) =>
            string.Equals(relative, "Assets", StringComparison.Ordinal) ||
            (relative?.StartsWith("Assets/", StringComparison.Ordinal) ?? false);

        private void Remove(string folder) =>
            SerializeReferenceSettings.UpdateExcludedFolders(current => current
                .Where(f => !string.Equals(f, folder, StringComparison.Ordinal))
                .ToArray());
    }
}
