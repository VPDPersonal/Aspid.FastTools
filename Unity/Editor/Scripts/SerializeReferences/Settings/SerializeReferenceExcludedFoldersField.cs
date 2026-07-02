using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Editable list of scan-excluded project folders, replacing the free-text "one path per line" field with a proper
    /// add/remove list. A compact translucent panel carries its own "Excluded scan folders" header, then stacks one
    /// zebra-striped entry per excluded folder (path on the left, an ✕ remove on the right) over a matching add-row
    /// entry whose "+" sits at the right edge; while empty the add row carries the "No excluded folders" hint instead.
    /// Folder rows and the add row are the same entry element, and their light/dark wash alternates down the list so
    /// adjacent rows read as separate elements. Clicking the add row (the "+" or its hint) opens a folder picker and
    /// stores the project-relative path; clicking a folder row re-opens the picker to re-point that entry in place.
    /// Reads and writes <see cref="SerializeReferenceSettings.ExcludedFolders"/> and rebuilds on
    /// <see cref="SerializeReferenceSettings.ExcludedFoldersChanged"/>, so the in-window Settings tab and the Project
    /// Settings page stay mirrored. Self-contained styling (palette + own USS) so it renders on both surfaces.
    /// </summary>
    internal sealed class SerializeReferenceExcludedFoldersField : VisualElement
    {
        private const string StyleSheetPath =
            "UI/SerializeReferences/Aspid-FastTools-SerializeReference-ExcludedFolders";

        private const string RootClass = "aspid-fasttools-excluded-folders";
        private const string TitleClass = "aspid-fasttools-excluded-folders__title";
        private const string ListClass = "aspid-fasttools-excluded-folders__list";
        private const string EntryClass = "aspid-fasttools-excluded-folders__entry";
        private const string EntryEvenClass = "aspid-fasttools-excluded-folders__entry--even";
        private const string EntryOddClass = "aspid-fasttools-excluded-folders__entry--odd";
        private const string EntryHoverClass = "aspid-fasttools-excluded-folders__entry--hover";
        private const string EntryDangerClass = "aspid-fasttools-excluded-folders__entry--danger";
        private const string EntryAddClass = "aspid-fasttools-excluded-folders__entry--add";
        private const string HintClass = "aspid-fasttools-excluded-folders__hint";
        private const string PathClass = "aspid-fasttools-excluded-folders__path";
        private const string RemoveClass = "aspid-fasttools-excluded-folders__remove";
        private const string AddButtonClass = "aspid-fasttools-excluded-folders__add";

        // The three mutually-exclusive full-row hover tints; SetTint is their single writer.
        private static readonly string[] HoverTints = { EntryHoverClass, EntryDangerClass, EntryAddClass };

        private readonly VisualElement _list;
        private readonly VisualElement _addRow;
        private readonly Label _hint;

        public SerializeReferenceExcludedFoldersField()
        {
            this.AddClass(RootClass)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            // The control itself is the panel: its own header sits above the folder rows, which stack over one
            // persistent add row. Every row — each folder and the add row — is the same zebra-striped entry element; the
            // add row's "+" is pinned right and its left carries the empty hint when there is nothing to list.
            var title = new Label("Excluded scan folders").AddClass(TitleClass);
            _list = new VisualElement().AddClass(ListClass);

            // The whole add row is the add affordance: clicking anywhere on it (the "+", the hint, or the gaps between)
            // adds, and hovering it tints the row green. The "+" is a passive glyph (a Label, not a Button) so its click
            // bubbles up to the row instead of firing a second add.
            _hint = new Label { tooltip = "Add folder" }.AddClass(HintClass);
            var addGlyph = new Label("+") { tooltip = "Add folder" }.AddClass(AddButtonClass);
            _addRow = new VisualElement().AddClass(EntryClass)
                .AddChild(_hint)
                .AddChild(addGlyph);
            _addRow.RegisterCallback<ClickEvent>(_ => AddFolder());
            TintWhileOver(_addRow, _addRow, EntryAddClass, null);

            Add(title);
            Add(_list);
            Add(_addRow);

            Rebuild();

            // ExcludedFoldersChanged (not the broad Changed) fires only when the set really moves, so this rebuilds on
            // its own edits and on the sibling surface's edits, and never on an unrelated setting flip. Armed from
            // build time, then follows the panel lifecycle: docking/undocking re-parents the tree — a detach then an
            // attach WITHOUT a rebuild — so a build-time-only subscription would silently stop mirroring after the
            // first dock move (see AspidSettingsUI.SyncFromSettings for the same dance).
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

            var folders = SerializeReferenceSettings.ExcludedFolders;
            _hint.text = folders.Length == 0 ? "No excluded folders" : string.Empty;

            for (var i = 0; i < folders.Length; i++)
            {
                var path = folders[i];

                // The path label fills the row left of the ✕ (full height), so clicking it anywhere edits.
                var label = new Label(path) { tooltip = path }.AddClass(PathClass);
                label.RegisterCallback<ClickEvent>(_ => Edit(path));

                var remove = new Button(() => Remove(path)) { text = "✕", tooltip = "Remove" }.AddClass(RemoveClass);

                var row = new VisualElement().AddClass(EntryClass)
                    .AddChild(label)
                    .AddChild(remove);

                // Hovering the path lifts the row to the neutral tint (editable); hovering the ✕ turns the whole row red.
                TintWhileOver(row, label, EntryHoverClass, null);
                TintWhileOver(row, remove, EntryDangerClass, null);

                ApplyStripe(row, i);
                _list.Add(row);
            }

            // The add row continues the zebra right after the last folder row.
            ApplyStripe(_addRow, folders.Length);
        }

        // Alternates an entry's background wash by its position in the visible list, so adjacent rows read as separate
        // elements (zebra: even rows a faint light wash, odd rows a faint dark one).
        private static void ApplyStripe(VisualElement entry, int index)
        {
            var even = index % 2 == 0;
            entry.EnableInClassList(EntryEvenClass, even);
            entry.EnableInClassList(EntryOddClass, !even);
        }

        // Repaints the whole <paramref name="entry"/> while the pointer is over <paramref name="zone"/>: entering paints
        // <paramref name="tint"/>, leaving falls back to <paramref name="fallback"/> (null clears). This is how a child
        // (the path, the ✕, the "+") can tint its entire row, which USS can't express from a child's hover state.
        private static void TintWhileOver(VisualElement entry, VisualElement zone, string tint, string fallback)
        {
            zone.RegisterCallback<PointerEnterEvent>(_ => SetTint(entry, tint));
            zone.RegisterCallback<PointerLeaveEvent>(_ => SetTint(entry, fallback));
        }

        // Sets exactly one full-row hover tint on the entry (or clears all when <paramref name="tint"/> is null). Being
        // the single writer is what keeps the three tints mutually exclusive without USS specificity juggling.
        private static void SetTint(VisualElement entry, string tint)
        {
            foreach (var cls in HoverTints) entry.EnableInClassList(cls, cls == tint);
        }

        private void AddFolder()
        {
            var relative = PickProjectFolder("Exclude folder from scan", "Assets");
            if (relative == null) return;

            var current = SerializeReferenceSettings.ExcludedFolders;
            if (current.Contains(relative)) return;

            SerializeReferenceSettings.ExcludedFolders = current.Append(relative).ToArray();
        }

        // Clicking a folder row re-opens the picker seeded at that folder and swaps the chosen path in place, so a row
        // can be re-pointed without remove-then-add. A pick that lands on an existing entry collapses onto it (Distinct).
        private void Edit(string folder)
        {
            var relative = PickProjectFolder("Edit excluded folder", folder);
            if (relative == null || string.Equals(relative, folder, StringComparison.Ordinal)) return;

            SerializeReferenceSettings.ExcludedFolders = SerializeReferenceSettings.ExcludedFolders
                .Select(f => string.Equals(f, folder, StringComparison.Ordinal) ? relative : f)
                .Distinct()
                .ToArray();
        }

        // Opens the folder picker and returns the chosen path as a project-relative path, or null when the user
        // cancelled or picked a folder outside the project (the latter explains itself via a dialog). Shared by add/edit.
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
