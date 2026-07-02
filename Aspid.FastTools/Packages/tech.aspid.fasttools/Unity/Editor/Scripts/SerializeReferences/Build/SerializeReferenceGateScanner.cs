using System;
using System.Linq;
using UnityEditor;
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
    /// <see cref="SerializeReferenceYamlEditor.FindMissingReferences"/>. Required-field detection loads each saved
    /// asset's objects and checks <see cref="SerializeReferenceRequiredGate.IsViolation"/> per managed-reference and
    /// string type property; scenes — which cannot be read through <see cref="AssetDatabase.LoadAllAssetsAtPath"/> — go
    /// through the pure-YAML <see cref="SerializeReferenceYamlEditor.FindUnsetRequiredFields"/> instead, so a <c>.unity</c>
    /// is covered for top-level required fields too.
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

            // The scan resolves each scene MonoBehaviour's required fields by its m_Script guid; memoise the resolution
            // for the run (many objects share a script), cleared up front so a recompile between runs is never served stale.
            ScriptRequiredFieldsCache.Clear();
            ConstraintMapCache.Clear();

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                onProgress?.Invoke((float)i / Math.Max(1, paths.Length), path);

                if (options.ScanMissingTypes)
                    foreach (var entry in SerializeReferenceYamlEditor.FindMissingReferences(path, SerializeReferenceHelpers.StoredTypeResolves))
                    {
                        if (IsPendingMigration(path, entry)) continue;
                        violations.Add(new GateViolation(path, entry.FileId, entry.Rid, entry.StoredType, GateViolationKind.MissingType, string.Empty));
                    }

                if (options.ScanRequiredFields)
                {
                    // Scenes cannot be object-loaded, so they take the pure-YAML required scan; saved assets keep the
                    // object-load path, which also covers required fields nested inside serializable containers.
                    if (SerializeReferenceHelpers.IsScene(path))
                        CollectSceneRequiredViolations(path, violations);
                    else
                        CollectRequiredViolations(path, violations);
                }
            }

            return violations;
        }

        // A stored name that no longer loads but is claimed by exactly one declared [MovedFrom] is a pending
        // migration, not a violation — Unity migrates the reference in memory at load — provided the target still
        // fits the field's declared type: a rename that also changed the type's bases WOULD null at load, so it
        // stays a violation (the same assignability gate both Editor views apply). Scenes cannot be object-loaded
        // to recover constraints, so a scene entry claimed by a rename is trusted — a base-changing rename inside a
        // scene is the one documented gap, and both Editor views still flag it. Internal for the gate tests.
        internal static bool IsPendingMigration(string assetPath, MissingReferenceEntry entry)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(entry.StoredType, out var target)) return false;
            if (SerializeReferenceHelpers.IsScene(assetPath)) return true;

            // Constraint recovery is best-effort: an unreadable asset or an entry the map cannot place behaves like
            // the views' unresolvable-constraint fallback (unconstrained) rather than manufacturing a violation.
            var constraints = ConstraintMapFor(assetPath);
            if (constraints is null) return true;

            return !constraints.TryGetValue((entry.FileId, entry.Rid), out var constraint) ||
                constraint is null || constraint == typeof(object) || constraint.IsAssignableFrom(target);
        }

        // Per-run memo of BuildConstraintMap (LoadAllAssetsAtPath + full SerializedObject walk — heavy), built only
        // for assets whose unresolved entries carry a [MovedFrom] claim. Null marks an asset whose map failed to build.
        private static readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> ConstraintMapCache =
            new(StringComparer.Ordinal);

        private static Dictionary<(long fileId, long rid), Type> ConstraintMapFor(string assetPath)
        {
            if (ConstraintMapCache.TryGetValue(assetPath, out var map)) return map;

            try
            {
                map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);
            }
            catch (Exception)
            {
                map = null;
            }

            ConstraintMapCache[assetPath] = map;
            return map;
        }

        // Per-run memo: a script guid -> the required field descriptors of the C# type it resolves to. Keyed by guid so
        // an unresolvable script (deleted / non-MonoBehaviour) caches an empty set once instead of re-probing every object.
        private static readonly Dictionary<string, IReadOnlyList<RequiredFieldDescriptor>> ScriptRequiredFieldsCache =
            new(StringComparer.Ordinal);

        // Scenes are scanned for unset required fields straight from YAML — LoadAllAssetsAtPath cannot read scene objects.
        private static void CollectSceneRequiredViolations(string assetPath, List<GateViolation> violations)
        {
            foreach (var entry in SerializeReferenceYamlEditor.FindUnsetRequiredFields(assetPath, RequiredFieldsForScript))
                violations.Add(new GateViolation(assetPath, entry.FileId, entry.Rid, default,
                    GateViolationKind.RequiredUnset, entry.FieldName));
        }

        // Resolves a MonoBehaviour script guid to the required fields of its C# type (via MonoScript), memoised per run.
        private static IReadOnlyList<RequiredFieldDescriptor> RequiredFieldsForScript(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return Array.Empty<RequiredFieldDescriptor>();
            if (ScriptRequiredFieldsCache.TryGetValue(guid, out var cached)) return cached;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var monoScript = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            var required = SerializeReferenceRequiredGate.GetRequiredFields(monoScript != null ? monoScript.GetClass() : null);

            ScriptRequiredFieldsCache[guid] = required;
            return required;
        }

        private static void CollectRequiredViolations(string assetPath, List<GateViolation> violations)
        {
            // Loading objects + walking SerializedObjects is heavier than the pure-YAML missing scan, so this check is
            // opt-in (off by default for the fast build-time mode). Caller dispatches scenes to the YAML path, so this
            // only ever sees saved assets (ScriptableObjects, prefabs) that LoadAllAssetsAtPath can read.
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long fileId)) continue;

                using var serializedObject = new SerializedObject(asset);
                using var iterator = serializedObject.GetIterator();
                if (!iterator.Next(enterChildren: true)) continue;

                // Never re-enter an instance already seen on this walk: a cyclic managed-reference graph (a shape
                // the feature explicitly supports — the graph window renders back-edges) would otherwise spin the
                // headless CI run forever. Mirrors SerializeReferenceHelpers.BuildConstraintMap's guard.
                var visited = new HashSet<long>();
                bool enterChildren;

                do
                {
                    enterChildren = true;

                    if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var id = iterator.managedReferenceId;
                        if (id >= 0 && !visited.Add(id)) enterChildren = false;
                    }

                    // Required applies to a [SerializeReference] managed reference (empty == null) and a [TypeSelector]
                    // string type field (empty == null-or-empty); IsViolation dispatches on the property kind.
                    if (iterator.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String)) continue;
                    if (!SerializeReferenceRequiredGate.IsViolation(iterator)) continue;

                    var rid = iterator.propertyType == SerializedPropertyType.ManagedReference ? iterator.managedReferenceId : 0L;
                    violations.Add(new GateViolation(assetPath, fileId, rid, default,
                        GateViolationKind.RequiredUnset, iterator.propertyPath));
                }
                while (iterator.Next(enterChildren));
            }
        }
    }
}
