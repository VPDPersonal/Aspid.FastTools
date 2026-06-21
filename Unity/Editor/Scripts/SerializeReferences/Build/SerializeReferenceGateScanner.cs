using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>What a gate violation is: a missing managed-reference type, or an unset required reference.</summary>
    internal enum GateViolationKind
    {
        MissingType,
        RequiredUnset,
    }

    /// <summary>Which checks the gate runs.</summary>
    internal readonly struct GateOptions
    {
        public readonly bool ScanMissingTypes;
        public readonly bool ScanRequiredFields;

        public GateOptions(bool scanMissingTypes, bool scanRequiredFields)
        {
            ScanMissingTypes = scanMissingTypes;
            ScanRequiredFields = scanRequiredFields;
        }

        public static GateOptions MissingOnly => new(true, false);
        public static GateOptions Full => new(true, true);
    }

    /// <summary>One gate violation located during a project scan.</summary>
    internal readonly struct GateViolation
    {
        public readonly string AssetPath;
        public readonly long FileId;
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;
        public readonly GateViolationKind Kind;
        public readonly string FieldPath;

        public GateViolation(string assetPath, long fileId, long rid, ManagedTypeName storedType, GateViolationKind kind, string fieldPath)
        {
            AssetPath = assetPath;
            FileId = fileId;
            Rid = rid;
            StoredType = storedType;
            Kind = kind;
            FieldPath = fieldPath;
        }

        public override string ToString()
        {
            var where = string.IsNullOrEmpty(FieldPath) ? $"rid {Rid}" : FieldPath;
            var what = Kind == GateViolationKind.MissingType ? $"missing type {StoredType.Class}" : "required value not set";
            return $"{AssetPath} : {where} -> {what}";
        }
    }

    /// <summary>
    /// Window-free, headless-safe project scanner for managed-reference gate violations, shared by the build gate and
    /// the CI entry point. Missing-type detection reuses the pure-YAML
    /// <see cref="SerializeReferenceYamlEditor.FindMissingReferences"/>; required-field detection loads each asset's
    /// objects and checks <see cref="SerializeReferenceRequiredGate.IsViolation"/> per managed-reference and string
    /// type property.
    /// </summary>
    internal static class SerializeReferenceGateScanner
    {
        /// <summary>
        /// Scans every candidate asset under <c>Assets/</c> and returns all gate violations for the enabled checks.
        /// <paramref name="onProgress"/> (fraction, label) may be null for a headless run.
        /// </summary>
        public static IReadOnlyList<GateViolation> Scan(GateOptions options, Action<float, string> onProgress = null)
        {
            var violations = new List<GateViolation>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();
            var requiredScenesSkipped = 0;

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                onProgress?.Invoke((float)i / Math.Max(1, paths.Length), path);

                if (options.ScanMissingTypes)
                    foreach (var entry in SerializeReferenceYamlEditor.FindMissingReferences(path, SerializeReferenceHelpers.StoredTypeResolves))
                        violations.Add(new GateViolation(path, entry.FileId, entry.Rid, entry.StoredType, GateViolationKind.MissingType, string.Empty));

                if (options.ScanRequiredFields)
                {
                    // Required detection loads objects + walks SerializedObjects, which cannot read scene objects, so
                    // scenes are not covered. Count them and warn once below rather than silently passing — a CI author
                    // must know an unset required value inside a .unity scene is NOT caught by this gate.
                    if (SerializeReferenceHelpers.IsScene(path)) requiredScenesSkipped++;
                    else CollectRequiredViolations(path, violations);
                }
            }

            if (requiredScenesSkipped > 0)
                Debug.LogWarning(
                    $"[Aspid FastTools] Required-field gate does not cover scenes: {requiredScenesSkipped} scene(s) were " +
                    "not checked for unset required references (missing-type checks still cover them). Track required " +
                    "values in prefabs / ScriptableObjects, or verify scenes in-editor.");

            return violations;
        }

        private static void CollectRequiredViolations(string assetPath, List<GateViolation> violations)
        {
            // Loading objects + walking SerializedObjects is heavier than the pure-YAML missing scan, so this check is
            // opt-in (off by default for the fast build-time mode). Scenes cannot be read through LoadAllAssetsAtPath
            // (see SerializeReferenceHelpers.IsScene), so the required-fields check skips them.
            if (SerializeReferenceHelpers.IsScene(assetPath)) return;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long fileId)) continue;

                using var serializedObject = new SerializedObject(asset);
                using var iterator = serializedObject.GetIterator();
                if (!iterator.Next(enterChildren: true)) continue;

                do
                {
                    // Required applies to a [SerializeReference] managed reference (empty == null) and a [TypeSelector]
                    // string type field (empty == null-or-empty); IsViolation dispatches on the property kind.
                    if (iterator.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String)) continue;
                    if (!SerializeReferenceRequiredGate.IsViolation(iterator)) continue;

                    var rid = iterator.propertyType == SerializedPropertyType.ManagedReference ? iterator.managedReferenceId : 0L;
                    violations.Add(new GateViolation(assetPath, fileId, rid, default,
                        GateViolationKind.RequiredUnset, iterator.propertyPath));
                }
                while (iterator.Next(enterChildren: true));
            }
        }
    }
}
