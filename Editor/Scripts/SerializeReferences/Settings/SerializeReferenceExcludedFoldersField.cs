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
    // Editable list of scan-excluded project folders: a header row that is itself the add target, then one flat row
    // per folder with a remove button. Clicking a row re-opens the picker to re-point that entry in place. It
    // rebuilds on ExcludedFoldersChanged, so the window's Settings tab and the Project Settings page stay mirrored.
    internal sealed class SerializeReferenceExcludedFoldersField : VisualElement
    {
        private const string StyleSheetPath =
            "UI/SerializeReferences/Aspid-FastTools-SerializeReference-ExcludedFolders";

        private const string RootClass = "aspid-fasttools-excluded-folders";
        private const string HeaderClass = "aspid-fasttools-excluded-folders__header";
        private const string HeaderCaptionClass = "aspid-fasttools-excluded-folders__header-caption";
        private const string HeaderAddClass = "aspid-fasttools-excluded-folders__header--add";
        private const string ListClass = "aspid-fasttools-excluded-folders__list";
        private const string EntryClass = "aspid-fasttools-excluded-folders__entry";
        private const string EntryHoverClass = "aspid-fasttools-excluded-folders__entry--hover";
        private const string EntryDangerClass = "aspid-fasttools-excluded-folders__entry--danger";
        private const string HintClass = "aspid-fasttools-excluded-folders__hint";
        private const string PathClass = "aspid-fasttools-excluded-folders__path";
        private const string RemoveClass = "aspid-fasttools-excluded-folders__remove";
        private const string AddButtonClass = "aspid-fasttools-excluded-folders__add";

        // The two mutually exclusive row hover tints; SetTint is their single writer.
        private static readonly string[] HoverTints = { EntryHoverClass, EntryDangerClass };

        private readonly VisualElement _list;
        private readonly VisualElement _header;
        private readonly Label _hint;

        // The current rows with their paths, so the keyboard ring can walk them exactly as the pointer can.
        private readonly List<(VisualElement Row, string Path)> _rows = new();

        // The hosting keyboard ring listens to re-collect its targets, since a rebuild replaces every row element.
        internal event Action RowsRebuilt;

        public SerializeReferenceExcludedFoldersField()
        {
            this.AddClass(RootClass)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            // The whole header row is the add target, so the "+" and the hint are passive labels whose clicks bubble
            // up instead of firing a second add. The add-intent wash is code-toggled, so the ring can mirror it.
            _hint = new Label { tooltip = "Add folder" }.AddClass(HintClass);
            var caption = new Label("Excluded scan folders").AddClass(HeaderCaptionClass);
            var addGlyph = new Label("+") { tooltip = "Add folder" }.AddClass(AddButtonClass);

            _header = new VisualElement().AddClass(HeaderClass)
                .AddChild(caption)
                .AddChild(_hint)
                .AddChild(addGlyph);
            _header.RegisterCallback<ClickEvent>(_ => AddFolder());
            _header.RegisterCallback<PointerEnterEvent>(_ => _header.AddToClassList(HeaderAddClass));
            _header.RegisterCallback<PointerLeaveEvent>(_ => _header.RemoveFromClassList(HeaderAddClass));

            _list = new VisualElement().AddClass(ListClass);

            Add(_header);
            Add(_list);

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
            _hint.text = folders.Length == 0 ? "No excluded folders" : string.Empty;

            foreach (var path in folders)
            {
                // The path label fills the row's full height, so clicking anywhere on it edits.
                var label = new Label(path) { tooltip = path }.AddClass(PathClass);
                label.RegisterCallback<ClickEvent>(_ => Edit(path));

                var remove = new Button(() => Remove(path)) { text = "✕", tooltip = "Remove" }.AddClass(RemoveClass);

                var row = new VisualElement().AddClass(EntryClass)
                    .AddChild(label)
                    .AddChild(remove);

                TintWhileOver(row, label, EntryHoverClass, null);
                TintWhileOver(row, remove, EntryDangerClass, null);

                _list.Add(row);
                _rows.Add((row, path));
            }

            RowsRebuilt?.Invoke();
        }

        // USS cannot tint a row from a child's hover state, so the tint is driven from code.
        private static void TintWhileOver(VisualElement entry, VisualElement zone, string tint, string fallback)
        {
            zone.RegisterCallback<PointerEnterEvent>(_ => SetTint(entry, tint));
            zone.RegisterCallback<PointerLeaveEvent>(_ => SetTint(entry, fallback));
        }

        // The single writer, which is what keeps the row tints mutually exclusive; null clears them.
        private static void SetTint(VisualElement entry, string tint)
        {
            foreach (var cls in HoverTints) entry.EnableInClassList(cls, cls == tint);
        }

        // The keyboard-ring members in visual order, mirroring the pointer affordances exactly: the header row
        // activates the add picker, each folder row the edit picker, and removing a row does what its button does.
        internal IEnumerable<(VisualElement Element, Action Activate, Action Remove)> GetNavTargets()
        {
            yield return (_header, AddFolder, null);

            foreach (var (row, path) in _rows)
                yield return (row, () => Edit(path), () => Remove(path));
        }

        private void AddFolder()
        {
            var relative = PickProjectFolder("Exclude folder from scan", "Assets");
            if (relative == null) return;

            var current = SerializeReferenceSettings.ExcludedFolders;
            if (current.Contains(relative)) return;

            SerializeReferenceSettings.ExcludedFolders = current.Append(relative).ToArray();
        }

        // Re-points a row in place; a pick landing on an existing entry collapses onto it.
        private void Edit(string folder)
        {
            var relative = PickProjectFolder("Edit excluded folder", folder);
            if (relative == null || string.Equals(relative, folder, StringComparison.Ordinal)) return;

            SerializeReferenceSettings.ExcludedFolders = SerializeReferenceSettings.ExcludedFolders
                .Select(f => string.Equals(f, folder, StringComparison.Ordinal) ? relative : f)
                .Distinct()
                .ToArray();
        }

        // The picked folder as a project-relative path; null on cancel, or on an outside-project pick, which
        // explains itself through a dialog.
        private static string PickProjectFolder(string title, string startFolder)
        {
            var absolute = EditorUtility.OpenFolderPanel(title, startFolder, string.Empty);
            if (string.IsNullOrEmpty(absolute)) return null;

            var relative = FileUtil.GetProjectRelativePath(absolute);
            if (!string.IsNullOrEmpty(relative)) return relative;

            EditorUtility.DisplayDialog(
                "Folder outside project",
                "Pick a folder inside the project (under Assets/ or Packages/).",
                "OK");
            return null;
        }

        private void Remove(string folder) =>
            SerializeReferenceSettings.ExcludedFolders = SerializeReferenceSettings.ExcludedFolders
                .Where(f => !string.Equals(f, folder, StringComparison.Ordinal))
                .ToArray();
    }
}
