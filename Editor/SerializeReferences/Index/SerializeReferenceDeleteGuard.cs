using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Warns before deleting a MonoScript used as a managed reference anywhere in the project. Unity does this for
    // components but never for managed references, so deleting a referenced script silently breaks assets.
    internal sealed class SerializeReferenceDeleteGuard : AssetModificationProcessor
    {
        private const int SamplePathCount = 8;

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            // Never block a headless/CI delete with a dialog.
            if (Application.isBatchMode) return AssetDeleteResult.DidNotDelete;
            if (string.IsNullOrEmpty(assetPath)) return AssetDeleteResult.DidNotDelete;

            // A folder delete fires once with the folder path; the scripts inside get no callback of their own.
            if (AssetDatabase.IsValidFolder(assetPath)) return GuardFolder(assetPath);

            if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return AssetDeleteResult.DidNotDelete;

            // The callback fires before deletion, so the script still resolves its type here.
            var type = ResolveScriptType(assetPath);
            if (type is null) return AssetDeleteResult.DidNotDelete;

            var sample = GatherUsageSample(type, out var count);
            if (count <= 0) return AssetDeleteResult.DidNotDelete;

            var message =
                $"\"{type.Name}\" is used as a [SerializeReference] managed reference in {count} place(s):\n\n" +
                $"{string.Join("\n", sample)}\n\n" +
                "Deleting the script will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Script", message, "Delete Anyway", "Cancel");

            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        private static Type ResolveScriptType(string assetPath)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            return script != null ? script.GetClass() : null;
        }

        // Sweeps the folder's scripts and raises one combined dialog for every referenced type found.
        private static AssetDeleteResult GuardFolder(string folderPath)
        {
            var types = new List<Type>();
            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript", new[] { folderPath }))
            {
                var type = ResolveScriptType(AssetDatabase.GUIDToAssetPath(guid));
                if (type is not null) types.Add(type);
            }

            if (types.Count == 0) return AssetDeleteResult.DidNotDelete;

            var affected = new List<string>();
            var totalCount = 0;

            foreach (var (type, count) in CountUsagesBatch(types))
            {
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

        // A warm index answers per type; a cold one falls back to a single combined sweep matching every type's open
        // key, since one sweep per contained script would freeze the editor on a folder delete.
        private static IEnumerable<(Type type, int count)> CountUsagesBatch(List<Type> types)
        {
            if (SerializeReferenceTypeUsageIndex.IsWarm)
            {
                foreach (var type in types)
                    yield return (type, SerializeReferenceTypeUsageIndex.CountUsages(type));

                yield break;
            }

            var countsByKey = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var type in types)
                countsByKey[SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type))] = 0;

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                // Skipping display-name resolution keeps this a pure text pass rather than an asset load.
                foreach (var document in SerializeReferenceGraphScanner.Build(path, resolveTypeNames: false))
                {
                    foreach (var node in document.Nodes)
                    {
                        if (node.StoredType.IsEmpty) continue;

                        var key = SerializeReferenceHelpers.OpenTypeKey(node.StoredType);
                        if (countsByKey.TryGetValue(key, out var count)) countsByKey[key] = count + 1;
                    }
                }
            }

            foreach (var type in types)
                yield return (type, countsByKey[SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type))]);
        }

        // A cold index is never warmed just to answer one delete, since that is a modal full-project build; a
        // targeted scan for this single type runs instead.
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

            // Matched on the open-generic identity, or a generic type's script would never match its closed keys.
            var key = SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type));
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                var usedHere = false;
                // Skipping display-name resolution keeps this a pure text pass rather than an asset load.
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
