using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Warns before deleting a MonoScript whose type is used as a <c>[SerializeReference]</c> managed reference anywhere
    /// in the project — Unity does this for components but never for managed references, so deleting a referenced script
    /// silently breaks assets. Queries <see cref="SerializeReferenceTypeUsageIndex"/> for the usage count and offers a
    /// confirm dialog; cancelling aborts the delete.
    /// </summary>
    internal sealed class SerializeReferenceDeleteGuard : AssetModificationProcessor
    {
        private const int SamplePathCount = 8;

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            // Never block a headless/CI delete with a dialog.
            if (Application.isBatchMode) return AssetDeleteResult.DidNotDelete;
            if (string.IsNullOrEmpty(assetPath)) return AssetDeleteResult.DidNotDelete;

            // A folder delete fires ONCE with the folder path — the scripts inside never get their own callback, so
            // without this branch a folder full of referenced scripts deletes unwarned.
            if (AssetDatabase.IsValidFolder(assetPath)) return GuardFolder(assetPath);

            if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return AssetDeleteResult.DidNotDelete;

            // The script still exists here (OnWillDeleteAsset fires before deletion), so GetClass() resolves its type.
            var type = ResolveScriptType(assetPath);
            if (type is null) return AssetDeleteResult.DidNotDelete;

            var sample = GatherUsageSample(type, out var count);
            if (count <= 0) return AssetDeleteResult.DidNotDelete;

            var message =
                $"\"{type.Name}\" is used as a [SerializeReference] managed reference in {count} place(s):\n\n" +
                $"{string.Join("\n", sample)}\n\n" +
                "Deleting the script will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Script", message, "Delete Anyway", "Cancel");

            // FailedDelete aborts the deletion; DidNotDelete lets Unity proceed normally.
            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        private static Type ResolveScriptType(string assetPath)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            return script != null ? script.GetClass() : null;
        }

        // Sweeps the folder's contained scripts and raises ONE combined dialog for every referenced type found.
        private static AssetDeleteResult GuardFolder(string folderPath)
        {
            var affected = new List<string>();
            var totalCount = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript", new[] { folderPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var type = ResolveScriptType(path);
                if (type is null) continue;

                GatherUsageSample(type, out var count);
                if (count <= 0) continue;

                totalCount += count;
                affected.Add($"{type.Name} — {count} place(s)");
            }

            if (affected.Count == 0) return AssetDeleteResult.DidNotDelete;

            var message =
                $"This folder contains {affected.Count} script(s) still used as [SerializeReference] managed " +
                $"references ({totalCount} place(s) total):\n\n{string.Join("\n", affected)}\n\n" +
                "Deleting the folder will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Folder", message, "Delete Anyway", "Cancel");
            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        // Uses the warm index when available; a cold index is never warmed (a modal full-project build) just to answer
        // one delete — a targeted, no-modal scan for this single type runs instead.
        private static SortedSet<string> GatherUsageSample(Type type, out int count)
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            count = 0;

            if (SerializeReferenceTypeUsageIndex.IsWarm)
            {
                foreach (var usage in SerializeReferenceTypeUsageIndex.FindUsages(type))
                {
                    count++;
                    if (paths.Count >= SamplePathCount) continue;

                    var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                    if (!string.IsNullOrEmpty(path)) paths.Add(path);
                }

                return paths;
            }

            // Match on the open-generic identity: a script resolves to the open definition while YAML stores each
            // closed instantiation under its own key, so the closed key would never match a generic type's script.
            var key = SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type));
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                var usedHere = false;
                // Data-only scan — skipping display-name resolution keeps this a pure text pass, not an asset load.
                foreach (var document in SerializeReferenceGraphScanner.Build(path, resolveTypeNames: false))
                {
                    foreach (var node in document.Nodes)
                    {
                        if (node.StoredType.IsEmpty) continue;
                        if (!string.Equals(SerializeReferenceHelpers.OpenTypeKey(node.StoredType), key, StringComparison.Ordinal)) continue;

                        count++;
                        usedHere = true;
                    }
                }

                if (usedHere && paths.Count < SamplePathCount) paths.Add(path);
            }

            return paths;
        }
    }
}
