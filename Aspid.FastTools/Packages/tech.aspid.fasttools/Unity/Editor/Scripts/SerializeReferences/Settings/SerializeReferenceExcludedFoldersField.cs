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
    /// add/remove list. "Add folder" opens a folder picker and stores the project-relative path; each row shows a path
    /// with a remove button. Reads and writes <see cref="SerializeReferenceSettings.ExcludedFolders"/> and rebuilds on
    /// <see cref="SerializeReferenceSettings.ExcludedFoldersChanged"/>, so the in-window Settings tab and the Project
    /// Settings page stay mirrored. Self-contained styling (palette + own USS) so it renders on both surfaces.
    /// </summary>
    internal sealed class SerializeReferenceExcludedFoldersField : VisualElement
    {
        private const string StyleSheetPath =
            "UI/SerializeReferences/Aspid-FastTools-SerializeReference-ExcludedFolders";

        private const string RootClass = "aspid-fasttools-excluded-folders";
        private const string ListClass = "aspid-fasttools-excluded-folders__list";
        private const string EmptyClass = "aspid-fasttools-excluded-folders__empty";
        private const string RowClass = "aspid-fasttools-excluded-folders__row";
        private const string PathClass = "aspid-fasttools-excluded-folders__path";
        private const string RemoveClass = "aspid-fasttools-excluded-folders__remove";
        private const string AddButtonClass = "aspid-fasttools-excluded-folders__add";

        private readonly VisualElement _list;

        public SerializeReferenceExcludedFoldersField()
        {
            this.AddClass(RootClass)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            _list = new VisualElement().AddClass(ListClass);
            Add(_list);
            Add(new Button(AddFolder) { text = "+  Add folder" }.AddClass(AddButtonClass));

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
            if (folders.Length == 0)
            {
                _list.Add(new Label("No excluded folders").AddClass(EmptyClass));
                return;
            }

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
