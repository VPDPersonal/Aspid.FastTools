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

            var count = SerializeReferenceTypeUsageIndex.CountUsages(type);
            if (count <= 0) return AssetDeleteResult.DidNotDelete;

            var message =
                $"\"{type.Name}\" is used as a [SerializeReference] managed reference in {count} place(s):\n\n" +
                $"{BuildSample(type)}\n\n" +
                "Deleting the script will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Script", message, "Delete Anyway", "Cancel");

            // FailedDelete aborts the deletion; DidNotDelete lets Unity proceed normally.
            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        private static string BuildSample(Type type)
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var usage in SerializeReferenceTypeUsageIndex.FindUsages(type))
            {
                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
                if (paths.Count >= SamplePathCount) break;
            }

            return string.Join("\n", paths);
        }
    }
}
