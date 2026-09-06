using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The bulk half of the repair tooling: rewriting or nulling many entries at once, batched per file so each asset
    // is reimported exactly once. Pure file work — confirmations, receipts and rendering belong to the caller.
    // A failed write is skipped, so every returned count is what actually changed on disk, not what was asked for.
    internal static class SerializeReferenceBatchEditor
    {
        // Splits entries into those safe to rewrite on disk and those open elsewhere, which must be repaired in
        // memory instead.
        public static void SplitWritable(IReadOnlyList<MissingReferenceLocation> source,
            out List<MissingReferenceLocation> onDisk, out List<MissingReferenceLocation> inMemory)
        {
            var prefabStagePath = SerializeReferenceOpenCopyGuard.CurrentPrefabStagePath();
            onDisk = new List<MissingReferenceLocation>(source.Count);
            inMemory = new List<MissingReferenceLocation>();

            foreach (var entry in source)
            {
                if (SerializeReferenceOpenCopyGuard.IsWritable(entry.AssetPath, prefabStagePath)) onDisk.Add(entry);
                else inMemory.Add(entry);
            }
        }

        // skipped counts the entries held back because an open copy would clobber the file edit on its next save.
        public static List<MissingReferenceLocation> FilterWritable(IReadOnlyList<MissingReferenceLocation> source, out int skipped)
        {
            var prefabStagePath = SerializeReferenceOpenCopyGuard.CurrentPrefabStagePath();
            var writable = new List<MissingReferenceLocation>(source.Count);
            skipped = 0;

            foreach (var entry in source)
            {
                if (SerializeReferenceOpenCopyGuard.IsWritable(entry.AssetPath, prefabStagePath)) writable.Add(entry);
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
            var holding = new List<MissingReferenceLocation>(source.Count);
            diverged = 0;

            foreach (var entry in source)
            {
                if (SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, appliedType, out var edit) &&
                    edit.IsValid && string.Equals(edit.OldLine, edit.NewLine, StringComparison.Ordinal))
                    holding.Add(entry);
                else
                    diverged++;
            }

            return holding;
        }

        public static int Rewrite(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName targetType, string progressTitle) =>
            RunBatch(entries, progressTitle, (path, entry) =>
                SerializeReferenceYamlEditor.TryRewriteType(path, entry.Entry.FileId, entry.Entry.Rid, targetType));

        // Nulls every entry to the null managed-reference id and drops its payload.
        public static int Null(IReadOnlyList<MissingReferenceLocation> entries, string progressTitle) =>
            RunBatch(entries, progressTitle, (path, entry) =>
                SerializeReferenceYamlEditor.TryNullReference(path, entry.Entry.FileId, entry.Entry.Rid));

        // Nulls each open entry on its live object; the file rewrite is skipped, so these stay in the audit until
        // the asset is saved.
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

        // The shared per-file loop behind Rewrite and Null; a file is reimported only when something changed in it.
        private static int RunBatch(IReadOnlyList<MissingReferenceLocation> entries, string progressTitle,
            Func<string, MissingReferenceLocation, bool> edit)
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

                    var changed = false;
                    foreach (var entry in file)
                    {
                        if (!edit(file.Key, entry)) continue;

                        applied++;
                        changed = true;
                    }

                    if (changed) AssetDatabase.ImportAsset(file.Key, ImportAssetOptions.ForceUpdate);
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
