using System;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Window-free, headless-safe project scanner for managed-reference gate violations, shared by the build gate and
    // the CI entry point. Missing types come from the pure-YAML scan. Required fields are checked per object for saved
    // assets; scenes, which LoadAllAssetsAtPath cannot read, go through the pure-YAML scan instead.
    internal static class SerializeReferenceGateScanner
    {
        // Per-run memo of BuildConstraintMap (LoadAllAssetsAtPath + full SerializedObject walk — heavy), built only
        // for assets whose unresolved entries carry a [MovedFrom] claim. Null marks an asset whose map failed to build.
        private static readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _constraintMapCache =
            new(StringComparer.Ordinal);

        // Script guid -> required field descriptors of the C# type it resolves to. Keyed by guid so an unresolvable
        // script (deleted / non-MonoBehaviour) caches an empty set once instead of re-probing every object.
        private static readonly Dictionary<string, IReadOnlyList<RequiredFieldDescriptor>> _scriptRequiredFieldsCache =
            new(StringComparer.Ordinal);

        // Scans every candidate asset under Assets/ for the enabled checks. onProgress (fraction, label) may be null.
        public static IReadOnlyList<GateViolation> Scan(GateOptions options, Action<float, string> onProgress = null)
        {
            var violations = new List<GateViolation>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();

            // Cleared up front so a recompile between runs is never served stale.
            _scriptRequiredFieldsCache.Clear();
            _constraintMapCache.Clear();

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                onProgress?.Invoke((float)i / Math.Max(1, paths.Length), path);

                if (options.ScanMissingTypes)
                {
                    foreach (var entry in SerializeReferenceYamlEditor.FindMissingReferences(path, SerializeReferenceHelpers.StoredTypeResolves))
                    {
                        if (IsPendingMigration(path, entry)) continue;
                        violations.Add(new GateViolation(path, entry.FileId, entry.Rid, entry.StoredType, GateViolationKind.MissingType, string.Empty));
                    }
                }

                if (options.ScanRequiredFields)
                {
                    if (SerializeReferenceHelpers.IsScene(path)) CollectSceneRequiredViolations(path, violations);
                    else CollectRequiredViolations(path, violations);
                }
            }

            return violations;
        }

        // Required-field scan for a single asset, without a full project sweep — the Inspect Asset graph's Rescan.
        public static IReadOnlyList<GateViolation> ScanAssetRequiredFields(string assetPath)
        {
            var violations = new List<GateViolation>();
            if (string.IsNullOrEmpty(assetPath) || !SerializeReferenceHelpers.IsScanCandidate(assetPath)) return violations;

            _scriptRequiredFieldsCache.Clear();

            if (SerializeReferenceHelpers.IsScene(assetPath)) CollectSceneRequiredViolations(assetPath, violations);
            else CollectRequiredViolations(assetPath, violations);

            return violations;
        }

        // A stored name claimed by exactly one declared [MovedFrom] is a pending migration, not a violation —
        // Unity migrates it in memory at load — provided the target still fits the field's declared type.
        // Scenes cannot be object-loaded to recover constraints, so a scene entry claimed by a rename is trusted.
        public static bool IsPendingMigration(string assetPath, MissingReferenceEntry entry)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(entry.StoredType, out var target)) return false;
            if (SerializeReferenceHelpers.IsScene(assetPath)) return true;

            // Best-effort: an asset whose map cannot be built behaves as unconstrained rather than manufacturing
            // a violation, matching the views' fallback.
            var constraints = ConstraintMapFor(assetPath);
            if (constraints is null) return true;

            return !constraints.TryGetValue((entry.FileId, entry.Rid), out var constraint) ||
                constraint is null || constraint == typeof(object) || constraint.IsAssignableFrom(target);
        }

        private static Dictionary<(long fileId, long rid), Type> ConstraintMapFor(string assetPath)
        {
            if (_constraintMapCache.TryGetValue(assetPath, out var map)) return map;

            try
            {
                map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);
            }
            catch (Exception)
            {
                map = null;
            }

            _constraintMapCache[assetPath] = map;
            return map;
        }

        private static void CollectSceneRequiredViolations(string assetPath, List<GateViolation> violations)
        {
            foreach (var entry in SerializeReferenceYamlEditor.FindUnsetRequiredFields(assetPath, RequiredFieldsForScript))
            {
                violations.Add(new GateViolation(assetPath, entry.FileId, entry.Rid, default,
                    GateViolationKind.RequiredUnset, entry.FieldName));
            }
        }

        private static IReadOnlyList<RequiredFieldDescriptor> RequiredFieldsForScript(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return Array.Empty<RequiredFieldDescriptor>();
            if (_scriptRequiredFieldsCache.TryGetValue(guid, out var cached)) return cached;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var monoScript = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            var required = TypeSelectorRequiredGate.GetRequiredFields(monoScript != null ? monoScript.GetClass() : null);

            _scriptRequiredFieldsCache[guid] = required;
            return required;
        }

        private static void CollectRequiredViolations(string assetPath, List<GateViolation> violations)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out var fileId)) continue;

                using var serializedObject = new SerializedObject(asset);
                using var iterator = serializedObject.GetIterator();
                if (!iterator.Next(enterChildren: true)) continue;

                // A cyclic managed-reference graph is supported, so never re-enter an instance already seen.
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

                    // Required applies to a managed reference (empty == null) and a [TypeSelector] string field
                    // (empty == null-or-empty); IsViolation dispatches on the property kind.
                    if (iterator.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String)) continue;
                    if (!TypeSelectorRequiredGate.IsViolation(iterator)) continue;

                    var rid = iterator.propertyType == SerializedPropertyType.ManagedReference ? iterator.managedReferenceId : 0L;
                    violations.Add(new GateViolation(assetPath, fileId, rid, default,
                        GateViolationKind.RequiredUnset, iterator.propertyPath));
                }
                while (iterator.Next(enterChildren));
            }
        }
    }
}
