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
    /// add/remove list. A compact translucent panel carries its own "Excluded scan folders" header, then stacks one row
    /// per excluded folder (path on the left, an ✕ remove on the right) over a single add row whose "+" sits at the
    /// right edge; while empty the add row carries the "No excluded folders" hint instead. "+" opens a folder picker and
    /// stores the project-relative path. Reads and writes <see cref="SerializeReferenceSettings.ExcludedFolders"/> and
    /// rebuilds on <see cref="SerializeReferenceSettings.ExcludedFoldersChanged"/>, so the in-window Settings tab and the
    /// Project Settings page stay mirrored. Self-contained styling (palette + own USS) so it renders on both surfaces.
    /// </summary>
    internal sealed class SerializeReferenceExcludedFoldersField : VisualElement
    {
        private const string StyleSheetPath =
            "UI/SerializeReferences/Aspid-FastTools-SerializeReference-ExcludedFolders";

        private const string RootClass = "aspid-fasttools-excluded-folders";
        private const string TitleClass = "aspid-fasttools-excluded-folders__title";
        private const string ListClass = "aspid-fasttools-excluded-folders__list";
        private const string AddRowClass = "aspid-fasttools-excluded-folders__add-row";
        private const string HintClass = "aspid-fasttools-excluded-folders__hint";
        private const string RowClass = "aspid-fasttools-excluded-folders__row";
        private const string PathClass = "aspid-fasttools-excluded-folders__path";
        private const string RemoveClass = "aspid-fasttools-excluded-folders__remove";
        private const string AddButtonClass = "aspid-fasttools-excluded-folders__add";

        private readonly VisualElement _list;
        private readonly Label _hint;

        public SerializeReferenceExcludedFoldersField()
        {
            this.AddClass(RootClass)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            // The control itself is the panel: its own header sits above the folder rows, which stack over one
            // persistent add row whose "+" is pinned right; the add row's left carries the empty hint when there is
            // nothing to list.
            var title = new Label("Excluded scan folders").AddClass(TitleClass);
            _list = new VisualElement().AddClass(ListClass);
            _hint = new Label().AddClass(HintClass);
            var addRow = new VisualElement().AddClass(AddRowClass)
                .AddChild(_hint)
                .AddChild(new Button(AddFolder) { text = "+", tooltip = "Add folder" }.AddClass(AddButtonClass));

            Add(title);
            Add(_list);
            Add(addRow);

            Rebuild();

            // ExcludedFoldersChanged (not the broad Changed) fires only when the set really moves, so this rebuilds on
            // its own edits and on the sibling surface's edits, and never on an unrelated setting flip.
            SerializeReferenceSettings.ExcludedFoldersChanged += Rebuild;
            RegisterCallback<DetachFromPanelEvent>(_ => SerializeReferenceSettings.ExcludedFoldersChanged -= Rebuild);
        }

        private void Rebuild()
        {
            _list.Clear();

            var folders = SerializeReferenceSettings.ExcludedFolders;
            _hint.text = folders.Length == 0 ? "No excluded folders" : string.Empty;

            foreach (var folder in folders)
            {
                var path = folder;
                _list.Add(new VisualElement().AddClass(RowClass)
                    .AddChild(new Label(path) { tooltip = path }.AddClass(PathClass))
                    .AddChild(new Button(() => Remove(path)) { text = "✕", tooltip = "Remove" }.AddClass(RemoveClass)));
            }
        }

        private void AddFolder()
        {
            var absolute = EditorUtility.OpenFolderPanel("Exclude folder from scan", "Assets", string.Empty);
            if (string.IsNullOrEmpty(absolute)) return;

            var relative = FileUtil.GetProjectRelativePath(absolute);
            if (string.IsNullOrEmpty(relative))
            {
                EditorUtility.DisplayDialog(
                    "Folder outside project",
                    "Pick a folder inside the project (under Assets/ or Packages/).",
                    "OK");
                return;
            }

            var current = SerializeReferenceSettings.ExcludedFolders;
            if (current.Contains(relative)) return;

            SerializeReferenceSettings.ExcludedFolders = current.Append(relative).ToArray();
        }

        private void Remove(string folder) =>
            SerializeReferenceSettings.ExcludedFolders = SerializeReferenceSettings.ExcludedFolders
                .Where(f => !string.Equals(f, folder, StringComparison.Ordinal))
                .ToArray();
    }
}
