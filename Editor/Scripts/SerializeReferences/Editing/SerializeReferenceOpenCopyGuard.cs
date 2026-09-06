using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The writability test every YAML rewrite applies first: an asset loaded as a scene or open in Prefab Mode keeps
    // an in-memory copy that wins on its next save, so a file edit under it is silently clobbered. Single-asset
    // callers use BlockedByOpenCopy, which explains the refusal through a dialog; a batch hoists the stage path.
    internal static class SerializeReferenceOpenCopyGuard
    {
        public static string CurrentPrefabStagePath() => PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;

        public static bool IsWritable(string assetPath) => IsWritable(assetPath, CurrentPrefabStagePath());

        // prefabStagePath is a pre-resolved CurrentPrefabStagePath, hoisted out of a batch loop.
        public static bool IsWritable(string assetPath, string prefabStagePath) =>
            !IsOpenInScene(assetPath) && !IsOpenInPrefabMode(assetPath, prefabStagePath);

        // True — and explained through a dialog — when the edit must be abandoned.
        public static bool BlockedByOpenCopy(string assetPath)
        {
            var openInPrefabMode = IsOpenInPrefabMode(assetPath, CurrentPrefabStagePath());
            if (!IsOpenInScene(assetPath) && !openInPrefabMode) return false;

            EditorUtility.DisplayDialog(
                "Asset References",
                "This asset is open " + (openInPrefabMode ? "in Prefab Mode" : "as a loaded scene") +
                " — a file rewrite would be overwritten by its next save.\n\n" +
                "Close it and rescan, or repair the field directly in the Inspector.",
                "OK");
            return true;
        }

        private static bool IsOpenInScene(string assetPath) => SceneManager.GetSceneByPath(assetPath).isLoaded;

        private static bool IsOpenInPrefabMode(string assetPath, string prefabStagePath) =>
            !string.IsNullOrEmpty(prefabStagePath) &&
            string.Equals(prefabStagePath, assetPath, System.StringComparison.Ordinal);
    }
}
