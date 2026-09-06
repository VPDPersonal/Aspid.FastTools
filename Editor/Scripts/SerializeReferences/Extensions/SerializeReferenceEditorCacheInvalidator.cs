using System.Linq;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Editor-assembly companion to SerializeReferenceYamlProbeCacheInvalidator, which cannot reach these caches.
    // rid and file id are stable across VCS operations, so after an external rewrite a cached Smart-Fix ranking could
    // describe data that no longer exists and re-point a reference against a stale identity.
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
