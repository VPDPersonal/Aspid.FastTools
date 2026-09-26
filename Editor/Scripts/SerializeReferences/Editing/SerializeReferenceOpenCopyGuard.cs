using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceOpenCopyGuard
    {
        public static string CurrentPrefabStagePath() => PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;

        public static bool IsWritable(string assetPath) => IsWritable(assetPath, CurrentPrefabStagePath());

        // prefabStagePath is a pre-resolved CurrentPrefabStagePath, hoisted out of a batch loop.
        public static bool IsWritable(string assetPath, string prefabStagePath) =>
            !IsOpenInScene(assetPath) && !IsOpenInPrefabMode(assetPath, prefabStagePath) && !HasUnsavedChanges(assetPath);

        // True — and explained through a dialog — when the edit must be abandoned.
        public static bool BlockedByOpenCopy(string assetPath, string title = "Asset References")
        {
            var openInPrefabMode = IsOpenInPrefabMode(assetPath);
            if (!IsOpenInScene(assetPath) && !openInPrefabMode) return false;

            EditorUtility.DisplayDialog(
                title,
                "This asset is open " + (openInPrefabMode ? "in Prefab Mode" : "as a loaded scene") +
                " — a file rewrite would be overwritten by its next save.\n\n" +
                "Close it and rescan, or repair the field directly on the open copy.",
                "OK");
            return true;
        }

        // True when the edit must be abandoned: the asset has unsaved changes and was not saved first. A file rewrite
        // reimports the asset, which reloads it from disk and silently drops those changes.
        public static bool BlockedByUnsavedChanges(string assetPath)
        {
            if (!HasUnsavedChanges(assetPath)) return false;
            if (Application.isBatchMode) return true;

            if (!EditorUtility.DisplayDialog(
                    "Asset References",
                    $"{assetPath} has unsaved changes — a file rewrite reimports the asset and would discard them.\n\n" +
                    "Save the asset first?",
                    "Save and Continue",
                    "Cancel"))
                return true;

            Save(assetPath);
            return HasUnsavedChanges(assetPath);
        }

        // Only a loaded asset can hold unsaved changes; a scene's are the open copy's, checked above.
        public static bool HasUnsavedChanges(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || SerializeReferenceHelpers.IsScene(assetPath)) return false;
            if (!AssetDatabase.IsMainAssetAtPathLoaded(assetPath)) return false;

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                if (obj != null && EditorUtility.IsDirty(obj)) return true;

            return false;
        }

        public static bool IsOpenInPrefabMode(string assetPath) => IsOpenInPrefabMode(assetPath, CurrentPrefabStagePath());

        private static bool IsOpenInScene(string assetPath) => SceneManager.GetSceneByPath(assetPath).isLoaded;

        private static bool IsOpenInPrefabMode(string assetPath, string prefabStagePath) =>
            !string.IsNullOrEmpty(prefabStagePath) &&
            string.Equals(prefabStagePath, assetPath, System.StringComparison.Ordinal);

        // The prefab pipeline owns its serialization, so a prefab saves through its root rather than the generic
        // asset-dirty path.
        private static void Save(string assetPath)
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefabRoot != null) PrefabUtility.SavePrefabAsset(prefabRoot);
            else AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assetPath));
        }
    }
}
