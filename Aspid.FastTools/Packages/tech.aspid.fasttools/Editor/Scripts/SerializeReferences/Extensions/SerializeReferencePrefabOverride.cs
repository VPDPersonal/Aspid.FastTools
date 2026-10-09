using System.IO;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Unity marks an override and offers Revert and Apply only on its own fields, so the custom header does it by hand.
    internal static class SerializeReferencePrefabOverride
    {
        // One selected object only, as in Unity's own fields: with several, the override state is not a single value.
        public static bool IsOverridden(SerializedProperty property) =>
            !property.serializedObject.isEditingMultipleObjects &&
            property.isInstantiatedPrefab &&
            property.prefabOverride;

        // The prefab asset the instance was created from. False for an object that has no source, such as a component
        // added on the instance.
        public static bool TryGetApplyTarget(SerializedProperty property, out string assetPath)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(property.serializedObject.targetObject);
            assetPath = source == null ? null : AssetDatabase.GetAssetPath(source);

            return !string.IsNullOrEmpty(assetPath);
        }

        public static string GetApplyLabel(string assetPath) =>
            $"Apply to Prefab '{Path.GetFileNameWithoutExtension(assetPath)}'";

        public static void Revert(SerializedProperty property) =>
            PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);

        public static void Apply(SerializedProperty property, string assetPath) =>
            PrefabUtility.ApplyPropertyOverride(property, assetPath, InteractionMode.UserAction);
    }
}
