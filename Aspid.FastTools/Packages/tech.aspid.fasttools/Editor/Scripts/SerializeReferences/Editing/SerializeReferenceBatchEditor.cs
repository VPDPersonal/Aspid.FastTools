using System;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceBatchEditor
    {
        public static void SplitWritable(IReadOnlyList<MissingReferenceLocation> source,
            out List<MissingReferenceLocation> onDisk, out List<MissingReferenceLocation> inMemory)
        {
            var prefabStagePath = SerializeReferenceOpenCopyGuard.CurrentPrefabStagePath();
            var verdicts = new Dictionary<string, bool>(StringComparer.Ordinal);
            onDisk = new List<MissingReferenceLocation>(source.Count);
            inMemory = new List<MissingReferenceLocation>();

            foreach (var entry in source)
            {
                if (SerializeReferenceOpenCopyGuard.IsRewriteSafe(entry.AssetPath, prefabStagePath, verdicts)) onDisk.Add(entry);
                else inMemory.Add(entry);
            }
        }

        // skipped counts the entries held back because an open copy would clobber the file edit on its next save, or
        // the edit's reimport would discard an asset's unsaved changes.
        public static List<MissingReferenceLocation> FilterWritable(IReadOnlyList<MissingReferenceLocation> source, out int skipped)
        {
            var prefabStagePath = SerializeReferenceOpenCopyGuard.CurrentPrefabStagePath();
            var verdicts = new Dictionary<string, bool>(StringComparer.Ordinal);
            var writable = new List<MissingReferenceLocation>(source.Count);
            skipped = 0;

            foreach (var entry in source)
            {
                if (SerializeReferenceOpenCopyGuard.IsRewriteSafe(entry.AssetPath, prefabStagePath, verdicts)) writable.Add(entry);
                else skipped++;
            }

            return writable;
        }

        // The entries a receipt for appliedType may safely revert; diverged counts the rest. A group can have been
        // re-broken and fixed to a DIFFERENT type since the receipt was written, and rewriting blindly would destroy
        // that newer fix. "Still holds it" is tested as a rewrite whose old line already equals its new one.
        public static List<MissingReferenceLocation> FilterStillHolding(IReadOnlyList<MissingReferenceLocation> source,
            ManagedTypeName appliedType, out int diverged)
        {
            var edits = ComputeRewrites(source, appliedType);
            var holding = new List<MissingReferenceLocation>(source.Count);
            diverged = 0;

            for (var i = 0; i < source.Count; i++)
            {
                if (edits[i].IsValid && string.Equals(edits[i].OldLine, edits[i].NewLine, StringComparison.Ordinal))
                    holding.Add(source[i]);
                else
                    diverged++;
            }

            return holding;
        }

        // The edit a rewrite to newType would make for each entry, in the order of entries, with one read per file.
        // A slot whose IsValid is false is an entry that could not be computed.
        public static RewriteEdit[] ComputeRewrites(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName newType)
        {
            var edits = new RewriteEdit[entries.Count];

            var byFile = Enumerable.Range(0, entries.Count)
                .GroupBy(index => entries[index].AssetPath, StringComparer.Ordinal);

            foreach (var file in byFile)
            {
                var indices = file.ToArray();
                var fileEdits = SerializeReferenceYamlEditor.ComputeRewrites(
                    file.Key,
                    indices.Select(index => entries[index].Entry).ToArray(),
                    newType);

                for (var i = 0; i < indices.Length; i++)
                    edits[indices[i]] = fileEdits[i];
            }

            return edits;
        }

        public static int Rewrite(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName targetType, string progressTitle) =>
            RunBatch(entries, progressTitle, (path, fileEntries) =>
                SerializeReferenceYamlEditor.RewriteTypes(path, fileEntries, targetType));

        public static int Null(IReadOnlyList<MissingReferenceLocation> entries, string progressTitle) =>
            RunBatch(entries, progressTitle, SerializeReferenceYamlEditor.NullReferences);

        public static int ClearOpenInMemory(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName storedType)
        {
            var cleared = 0;
            foreach (var entry in entries)
            {
                if (SerializeReferenceHelpers.TryClearMissingReferenceInMemory(entry.AssetPath, entry.Entry.Rid, storedType))
                    cleared++;
            }

            return cleared;
        }

        public static int CountFiles(IEnumerable<MissingReferenceLocation> entries) =>
            entries.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();

        // Edits each file once: edit gets all the file's entries and returns how many it applied.
        private static int RunBatch(IReadOnlyList<MissingReferenceLocation> entries, string progressTitle,
            Func<string, IReadOnlyList<MissingReferenceEntry>, int> edit)
        {
            var byFile = entries
                .GroupBy(entry => entry.AssetPath, StringComparer.Ordinal)
                .ToArray();

            var applied = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < byFile.Length; i++)
                {
                    var file = byFile[i];
                    EditorUtility.DisplayProgressBar(
                        progressTitle,
                        $"{file.Key}  ({i + 1}/{byFile.Length})",
                        (float)i / byFile.Length);

                    // Checked out once up front: a read-only file is reported once and skipped, and a checkout makes
                    // it writable before its first entry, even when that entry turns out stale.
                    if (File.Exists(file.Key) && !SerializeReferenceYamlEditor.TryMakeEditable(file.Key)) continue;

                    var fileApplied = edit(file.Key, file.Select(location => location.Entry).ToArray());
                    if (fileApplied == 0) continue;

                    applied += fileApplied;
                    AssetDatabase.ImportAsset(file.Key, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return applied;
        }
    }
}
