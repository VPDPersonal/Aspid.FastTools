using System.Linq;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceEditorCacheInvalidator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (!HasCandidate(imported) && !HasCandidate(deleted) && !HasCandidate(moved)) return;

            SerializeReferenceRepairSuggestions.ClearCache();
            SerializeReferenceHelpers.InvalidateMixedTypesCache();
            SerializeReferenceHelpers.InvalidateMissingTypeMemo();
        }

        private static bool HasCandidate(string[] paths) =>
            paths.Any(SerializeReferenceYaml.IsCandidateAssetPath);
    }
}
