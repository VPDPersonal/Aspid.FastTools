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
        // A scan waits for the scripts to compile, and the compile ends with a domain reload that would drop the wait.
        // The wait is kept here and resumes after the reload.
        private const string PendingKey = "Aspid.FastTools.SerializeReferences.Breakage.ScanPending";
        private const string PendingAssetsKey = "Aspid.FastTools.SerializeReferences.Breakage.ScanPendingAssets";
        private const char PathSeparator = '\n';

        // Assets that change which assembly holds a type, besides the scripts.
        private static readonly string[] _scriptExtensions = { ".cs", ".asmdef", ".asmref", ".dll" };

        private static bool _scheduled;
        private static readonly HashSet<string> _changedAssets = new(StringComparer.Ordinal);

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssemblyReloadEvents.beforeAssemblyReload += KeepPendingScan;
            ResumePendingScan();
        }

        private static void ResumePendingScan()
        {
            if (!SessionState.GetBool(PendingKey, false)) return;

            var paths = SessionState.GetString(PendingAssetsKey, string.Empty).Split(PathSeparator);
            _changedAssets.UnionWith(paths.Where(path => path.Length > 0));
            ClearPendingScan();

            Schedule();
        }

        private static void KeepPendingScan()
        {
            if (!_scheduled) return;

            SessionState.SetBool(PendingKey, true);
            SessionState.SetString(PendingAssetsKey, string.Join(PathSeparator.ToString(), _changedAssets));
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (Application.isBatchMode) return;

            Collect(imported, deleted, moved, movedFrom);
        }

        internal static bool Collect(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // An in-place class rename lands the edited .cs in `imported` (no move, no delete) — the detector's
            // headline case, so it must schedule a scan alongside the rename/delete and re-saved-asset paths.
            // A new .asmdef or .asmref moves types to another assembly the same way, and a .dll adds or removes them.
            var relevant = HasScript(imported) || HasScript(deleted) || HasScript(moved)
                           || HasCandidate(imported) || HasCandidate(deleted) || HasCandidate(moved);
            if (!relevant) return false;

            // Collected across coalesced batches: the detector re-reads each one to keep its baseline current.
            _changedAssets.UnionWith(imported.Concat(deleted).Concat(moved).Concat(movedFrom)
                .Where(SerializeReferenceHelpers.IsScanCandidate));

            Schedule();
            return true;
        }

        private static void Schedule()
        {
            if (_scheduled) return;

            _scheduled = true;
            EditorApplication.delayCall += RunScan;
        }

        private static void RunScan()
        {
            // Type resolution flaps while scripts compile, so wait (never drop) until the editor settles.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunScan;
                return;
            }

            _scheduled = false;

            var changedAssets = _changedAssets.ToArray();
            _changedAssets.Clear();

            SerializeReferenceBreakageDetector.Scan(changedAssets);
            TypeNameBreakageDetector.Scan(changedAssets);
        }

        // Tests start from no scan waiting.
        internal static void ResetForTests()
        {
            EditorApplication.delayCall -= RunScan;
            _scheduled = false;
            _changedAssets.Clear();
            ClearPendingScan();
        }

        // What a domain reload does: the memory state is lost, and the scan kept in SessionState comes back.
        internal static void SimulateDomainReloadForTests()
        {
            KeepPendingScan();

            EditorApplication.delayCall -= RunScan;
            _scheduled = false;
            _changedAssets.Clear();

            ResumePendingScan();
        }

        internal static bool IsScanScheduled => _scheduled;

        internal static IReadOnlyCollection<string> PendingAssets => _changedAssets;

        private static void ClearPendingScan()
        {
            SessionState.EraseBool(PendingKey);
            SessionState.EraseString(PendingAssetsKey);
        }

        private static bool HasScript(string[] paths) =>
            paths.Any(path => _scriptExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)));

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceHelpers.IsScanCandidate);
    }
}
