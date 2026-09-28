using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceBreakageHook : AssetPostprocessor
    {
        private static bool _scheduled;
        private static readonly HashSet<string> _changedAssets = new(StringComparer.Ordinal);

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (Application.isBatchMode) return;

            // An in-place class rename lands the edited .cs in `imported` (no move, no delete) — the detector's
            // headline case, so it must schedule a scan alongside the rename/delete and re-saved-asset paths.
            var relevant = HasScript(imported) || HasScript(deleted) || HasScript(moved)
                           || HasCandidate(imported) || HasCandidate(deleted) || HasCandidate(moved);
            if (!relevant) return;

            // Collected across coalesced batches: the detector re-reads each one to keep its baseline current.
            _changedAssets.UnionWith(imported.Concat(deleted).Concat(moved).Concat(movedFrom)
                .Where(SerializeReferenceHelpers.IsScanCandidate));

            if (_scheduled) return;

            _scheduled = true;
            EditorApplication.delayCall += () =>
            {
                _scheduled = false;

                var changedAssets = _changedAssets.ToArray();
                _changedAssets.Clear();

                SerializeReferenceBreakageDetector.Scan(changedAssets);
            };
        }

        private static bool HasScript(string[] paths) =>
            paths.Any(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceHelpers.IsScanCandidate);
    }
}
