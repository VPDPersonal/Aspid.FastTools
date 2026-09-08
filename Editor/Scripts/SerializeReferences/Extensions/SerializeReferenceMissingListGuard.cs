using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceMissingListGuard : AssetModificationProcessor
    {
        // Consumed once by the post-save pass and dropped, so a later save re-snapshots from the then-current file.
        private static readonly Dictionary<string, List<Snapshot>> PendingByPath = new();

        // Fires with the file still in its pre-save state; the returned set is never altered.
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                if (!SerializeReferenceYaml.IsCandidateAssetPath(path)) continue;

                var snapshots = SnapshotMissingArrayElements(path);
                if (snapshots.Count == 0) continue;

                PendingByPath[path] = snapshots;

                // Anchored to the path, not a SerializedObject: the repair re-reads from disk after Unity writes.
                var captured = path;
                EditorApplication.delayCall += () => RestoreAfterSave(captured);
            }

            return paths;
        }

        private static List<Snapshot> SnapshotMissingArrayElements(string assetPath)
        {
            var result = new List<Snapshot>();

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(assetPath, SerializeReferenceHelpers.StoredTypeResolves);
            if (missing.Count == 0) return result;

            foreach (var entry in missing)
            {
                if (!SerializeReferenceYamlEditor.TryFindTopLevelArrayElementForRid(assetPath, entry.FileId, entry.Rid, out var field, out var index))
                    continue; // a single field or nested pointer is not resized, so not at risk

                var elementPath = $"{field}.Array.data[{index}]";
                if (SerializeReferenceYamlEditor.TryReadArrayElementEntryBlock(assetPath, entry.FileId, elementPath, out _, out var entryLines))
                    result.Add(new Snapshot(entry.FileId, elementPath, entryLines));
            }

            return result;
        }

        private static void RestoreAfterSave(string assetPath)
        {
            if (!PendingByPath.TryGetValue(assetPath, out var snapshots)) return;
            PendingByPath.Remove(assetPath);

            var restored = 0;
            foreach (var snapshot in snapshots)
            {
                if (SerializeReferenceYamlEditor.TryRestoreArrayElementReference(assetPath, snapshot.FileId, snapshot.ElementPath, snapshot.EntryLines))
                    restored++;
            }

            if (restored == 0) return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            UnityEngine.Debug.Log($"[Aspid FastTools] Preserved {restored} missing reference(s) that a list resize would have dropped in '{assetPath}'.");
        }

        private readonly struct Snapshot
        {
            public readonly long FileId;
            public readonly string ElementPath;
            public readonly List<string> EntryLines;

            public Snapshot(long fileId, string elementPath, List<string> entryLines)
            {
                FileId = fileId;
                ElementPath = elementPath;
                EntryLines = entryLines;
            }
        }
    }
}
