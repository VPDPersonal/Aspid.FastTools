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
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return AssetDeleteResult.DidNotDelete;

            // The script still exists here (OnWillDeleteAsset fires before deletion), so GetClass() resolves its type.
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            var type = script != null ? script.GetClass() : null;
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

        // The use-site count plus a sample of affected asset paths. Uses the warm index when available; when the index is
        // cold we do NOT warm it (a modal full-project build) just to answer one delete — instead we run a targeted,
        // no-modal scan for this single type, so deletion protection stays active without freezing the editor behind a bar.
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

            // Match on the open-generic identity: a script resolves to the open definition (Modifier`1[[T]]), while YAML
            // stores each closed instantiation (Modifier`1[[System.Single, …]]) under its own key — comparing the closed
            // stored key would never match a generic type's script and the delete would slip through unwarned.
            var key = SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type));
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                var usedHere = false;
                foreach (var document in SerializeReferenceGraphScanner.Build(path))
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
