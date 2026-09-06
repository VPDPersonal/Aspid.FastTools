using System;
using UnityEngine;
using UnityEditor;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Every single-entry repair the Asset References graph offers, without any of its UI. Two edit routes, picked by
    // what the entry is rather than by the caller: a healthy or empty slot goes through managedReferenceValue, so
    // Unity writes the RefIds entry exactly as the Inspector would, while a MISSING reference cannot be reassigned
    // through that API at all and is edited by rewriting the YAML in place. That is why the YAML routes confirm
    // first, cannot be undone through Unity's undo stack, and refuse to run against an asset with an open copy.
    //
    // Each entry point reports whether anything changed; re-rendering the graph is the caller's concern.
    internal static class SerializeReferenceGraphEditor
    {
        // Re-points a missing reference by rewriting the stored type name in the YAML, keeping the payload.
        public static bool ApplyFix(string assetPath, long fileId, long rid, string assemblyQualifiedName)
        {
            // <None> emits an empty name, which clears the reference rather than falling through as a no-op.
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return ClearReference(assetPath, fileId, rid);

            if (SerializeReferenceOpenCopyGuard.BlockedByOpenCopy(assetPath)) return false;

            var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (type is null) return false;

            // Only the captured document is rewritten: a rid is unique within one, but collides across them.
            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, fileId, rid, ManagedTypeName.FromType(type)))
                return false;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            return true;
        }

        // Resets a missing reference to <None> in the YAML — nulling every pointer and dropping the RefIds entry,
        // exactly what Unity writes for a cleared field. Confirmed, not undoable, and the payload is discarded.
        public static bool ClearReference(string assetPath, long fileId, long rid)
        {
            if (SerializeReferenceOpenCopyGuard.BlockedByOpenCopy(assetPath)) return false;

            // Name how many fields the clear nulls, so an aliased reference does not silently take down siblings.
            // A non-positive count means the pointers could not be located, so the wording drops the number.
            var fieldCount = SerializeReferenceYamlEditor.CountPointersTo(assetPath, fileId, rid);
            var pointerLine = fieldCount switch
            {
                1 => "This nulls the 1 field pointing at it",
                > 1 => $"This reference is aliased across {fieldCount} fields — clearing it nulls every one of them",
                _ => "This nulls every field pointing at it",
            };

            if (!EditorUtility.DisplayDialog(
                    "Clear Reference",
                    $"Reset this managed reference (rid {rid}) to <None> in\n{assetPath}?\n\n" +
                    $"{pointerLine} and discards its stored data. It edits the asset file directly and cannot be undone.",
                    "Clear", "Cancel"))
                return false;

            if (!SerializeReferenceYamlEditor.TryNullReference(assetPath, fileId, rid)) return false;

            // The forced import lets the index invalidator patch this asset alone; a full ClearCache would dump the
            // warm index and put Project References back on its modal first scan.
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            return true;
        }

        // Drops a dangling RefIds entry no field points at, after confirming. staleRescan returns the fresh scan
        // that disproved the orphan, so the caller re-renders from it instead of reading the file again.
        public static bool TryClearOrphan(string assetPath, long fileId, long rid, out List<ReferenceGraphDocument> staleRescan)
        {
            staleRescan = null;

            if (SerializeReferenceOpenCopyGuard.BlockedByOpenCopy(assetPath)) return false;

            if (!EditorUtility.DisplayDialog(
                    "Drop Orphaned Entry",
                    $"Remove the orphaned managed-reference entry (rid {rid}) from\n{assetPath}?\n\n" +
                    "This edits the asset file directly and cannot be undone.",
                    "Remove", "Cancel"))
                return false;

            // The on-screen graph may be stale, so re-confirm the orphan against a fresh scan before deleting.
            var fresh = SerializeReferenceGraphScanner.Build(assetPath);
            foreach (var document in fresh)
            {
                if (document.FileId != fileId || !document.Orphans.Contains(rid)) continue;

                if (!SerializeReferenceYamlEditor.TryRemoveEntry(assetPath, fileId, rid)) return false;

                // Patched through the import invalidator rather than a full ClearCache, as in ClearReference.
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                SerializeReferenceRepairSuggestions.ClearCache();
                return true;
            }

            staleRescan = fresh;
            return false;
        }

        // Edits a healthy or empty slot through managedReferenceValue, then saves so the disk-read graph reflects it
        // on rescan. A path the API cannot reach is reported through a dialog and skipped.
        public static bool ApplyLive(string assetPath, long fileId, string graphPath, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

            // A non-empty name that fails to load is an unresolved pick, not a clear.
            if (!string.IsNullOrEmpty(assemblyQualifiedName) && type is null) return false;

            if (!TryResolveLiveProperty(assetPath, fileId, graphPath, out var serializedObject, out var property))
            {
                EditorUtility.DisplayDialog(
                    "Edit Reference",
                    "This slot cannot be edited here — its field is not reachable through the serialization API " +
                    "(it may be an orphan, live in a scene, or sit under a missing parent). Edit it in the Inspector " +
                    "or repair its parent first.",
                    "OK");
                return false;
            }

            using (serializedObject)
            {
                var previous = property.managedReferenceValue;
                // A null type clears to <None>; a concrete one carries over the previous value's matching fields.
                property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                property.isExpanded = type is not null;

                var target = serializedObject.targetObject;
                EditorUtility.SetDirty(target);
                PersistEdit(assetPath, target);
            }

            // The save triggers the import that patches the index for this asset alone.
            SerializeReferenceRepairSuggestions.ClearCache();
            SerializeReferenceYamlProbeCache.ClearCache();
            return true;
        }

        // Writes a type name into the backing string of a required [TypeSelector] field — the one required shape the
        // routes above cannot reach, since such a field is never threaded into RefIds.
        public static bool ApplyRequiredString(GateViolation violation, string assemblyQualifiedName)
        {
            // A non-empty name that fails to load is an unresolved pick, not a clear. <None> writes an empty name,
            // which for a required field simply keeps the violation visible.
            if (!string.IsNullOrEmpty(assemblyQualifiedName) &&
                Type.GetType(assemblyQualifiedName, throwOnError: false) is null)
                return false;

            if (!TryResolveRequiredStringProperty(violation, out var serializedObject, out var property))
            {
                EditorUtility.DisplayDialog(
                    "Assign Required Type",
                    "This field cannot be edited here — it is not reachable through the serialization API. " +
                    "Edit it in the Inspector instead.",
                    "OK");
                return false;
            }

            using (serializedObject)
            {
                property.SetStringAndApply(assemblyQualifiedName ?? string.Empty);

                // A SerializableMonoScript treats its script reference as the source of truth, so writing the name
                // alone would be reverted on the next serialization.
                SerializableMonoScriptUtility.SyncScriptFromName(property);

                var target = serializedObject.targetObject;
                EditorUtility.SetDirty(target);
                PersistEdit(violation.AssetPath, target);
            }

            SerializeReferenceYamlProbeCache.ClearCache();
            return true;
        }

        // The caller disposes the returned SerializedObject. False for a path the API cannot reach: an empty path, a
        // scene asset, or a field under a missing or null parent.
        public static bool TryResolveLiveProperty(string assetPath, long fileId, string graphPath,
            out SerializedObject serializedObject, out SerializedProperty property)
        {
            serializedObject = null;
            property = null;

            if (string.IsNullOrEmpty(graphPath)) return false;
            // Scenes are not loadable through LoadAllAssetsAtPath.
            if (SerializeReferenceHelpers.IsScene(assetPath)) return false;

            return TryResolveProperty(assetPath, fileId, ToSerializedPropertyPath(graphPath),
                SerializedPropertyType.ManagedReference, out serializedObject, out property);
        }

        // The caller disposes the returned SerializedObject. A violation's field path is already a property path, so
        // unlike the graph route no conversion applies.
        public static bool TryResolveRequiredStringProperty(GateViolation violation,
            out SerializedObject serializedObject, out SerializedProperty property)
        {
            serializedObject = null;
            property = null;

            if (SerializeReferenceHelpers.IsScene(violation.AssetPath)) return false;

            return TryResolveProperty(violation.AssetPath, violation.FileId, violation.FieldPath,
                SerializedPropertyType.String, out serializedObject, out property);
        }

        // The inverse of the ".Array.data" stripping the YAML editor does when it normalizes a property path.
        public static string ToSerializedPropertyPath(string graphPath) =>
            Regex.Replace(graphPath, @"\[(\d+)\]", ".Array.data[$1]");

        // Finds the sub-asset carrying fileId, then the property at propertyPath, of the expected kind.
        private static bool TryResolveProperty(string assetPath, long fileId, string propertyPath,
            SerializedPropertyType expected, out SerializedObject serializedObject, out SerializedProperty property)
        {
            serializedObject = null;
            property = null;

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var id) || id != fileId) continue;

                var serialized = new SerializedObject(obj);
                var found = serialized.FindProperty(propertyPath);
                if (found is not null && found.propertyType == expected)
                {
                    serializedObject = serialized;
                    property = found;
                    return true;
                }

                // No other document shares this file id, so a wrong kind here means bail rather than scan on.
                serialized.Dispose();
                return false;
            }

            return false;
        }

        // The prefab pipeline owns its serialization, so a component edit does not reliably flush through the generic
        // asset-dirty path; prefabs save through their in-memory root instead.
        private static void PersistEdit(string assetPath, Object target)
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefabRoot != null) PrefabUtility.SavePrefabAsset(prefabRoot);
            else AssetDatabase.SaveAssetIfDirty(target);
        }
    }
}
