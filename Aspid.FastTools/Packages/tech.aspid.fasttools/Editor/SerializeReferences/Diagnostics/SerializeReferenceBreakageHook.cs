using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Drives the breakage detector on asset/script changes, debounced to one scan per change burst.
    internal sealed class SerializeReferenceBreakageHook : AssetPostprocessor
    {
        private static bool _scheduled;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (Application.isBatchMode) return;

            // An in-place class rename lands the edited .cs in `imported` (no move, no delete) — the detector's
            // headline case, so it must schedule a scan alongside the rename/delete and re-saved-asset paths.
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
