using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Drives <see cref="SerializeReferenceBreakageDetector"/> when assets or scripts change: a renamed/deleted script
    /// surfaces as a <c>.cs</c> in the deleted/moved lists, an in-place class rename as an edited <c>.cs</c> in the
    /// imported list, and a re-saved prefab/asset/scene as a candidate import. The scan is debounced to one run per
    /// change burst via <see cref="EditorApplication.delayCall"/>.
    /// </summary>
    internal sealed class SerializeReferenceBreakageHook : AssetPostprocessor
    {
        private static bool _scheduled;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (Application.isBatchMode) return;

            // An in-place class rename (edit the .cs, no file move, no [MovedFrom]) reimports the script, so the edited
            // .cs lands in `imported` — not deleted/moved. That is the detector's headline case, so it must schedule a
            // scan too, alongside the rename/delete (.cs in deleted/moved) and re-saved-asset (candidate import) paths.
            var relevant = HasScript(imported) || HasScript(deleted) || HasScript(moved)
                           || HasCandidate(imported) || HasCandidate(deleted) || HasCandidate(moved);
            if (!relevant || _scheduled) return;

            _scheduled = true;
            EditorApplication.delayCall += () =>
            {
                _scheduled = false;
                SerializeReferenceBreakageDetector.Scan();
            };
        }

        private static bool HasScript(string[] paths) =>
            paths.Any(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceHelpers.IsScanCandidate);
    }
}
