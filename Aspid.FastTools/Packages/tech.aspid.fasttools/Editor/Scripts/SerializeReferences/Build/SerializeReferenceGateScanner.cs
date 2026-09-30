using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceGateScanner
    {
        private const int MaxListedPaths = 10;

        // How many files the required-field sweep loads before it releases the ones nothing references any more.
        // Not const so tests can lower it.
        internal static int UnloadEveryLoadedFiles = 64;

        // Per-run memo of BuildConstraintMap (LoadAllAssetsAtPath + full SerializedObject walk — heavy), built only
        // for assets whose unresolved entries carry a [MovedFrom] claim. Null marks an asset whose map failed to build.
        private static readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _constraintMapCache =
            new(StringComparer.Ordinal);

        // Script guid -> required field descriptors of the C# type it resolves to. Keyed by guid so an unresolvable
        // script (deleted / non-MonoBehaviour) caches an empty set once instead of re-probing every object.
        private static readonly Dictionary<string, IReadOnlyList<RequiredFieldDescriptor>> _scriptRequiredFieldsCache =
            new(StringComparer.Ordinal);

        // `unscanned` collects the candidates the YAML pass had to skip (binary files, LFS pointers): their missing
        // types, and a scene's required fields, were not checked. Prefabs and assets still get the object-load
        // required check, which does not depend on the file format.
        public static IReadOnlyList<GateViolation> Scan(
            GateOptions options,
            Action<float, string> onProgress = null,
            ICollection<(string AssetPath, AssetFileFormat Format)> unscanned = null)
        {
            var violations = new List<GateViolation>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();

            _scriptRequiredFieldsCache.Clear();
            _constraintMapCache.Clear();

            var loadedSinceUnload = 0;

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                onProgress?.Invoke((float)i / Math.Max(1, paths.Length), path);

                var isScene = SerializeReferenceHelpers.IsScene(path);

                // Scenes are read as YAML. Any other file is loaded by the required sweep, or earlier by the
                // constraint map of a pending migration, and either load counts toward the next unload.
                var wasLoaded = !options.ScanRequiredFields || isScene || AssetDatabase.IsMainAssetAtPathLoaded(path);

                // Sniffed once here; the YAML scanners below are told the result instead of opening the file again.
                var needsYaml = options.ScanMissingTypes || (options.ScanRequiredFields && isScene);
                var isTextYaml = true;

                if (needsYaml && File.Exists(path))
                {
                    var format = SerializeReferenceYaml.SniffFileFormat(path);
                    isTextYaml = format == AssetFileFormat.TextYaml;
                    if (!isTextYaml) unscanned?.Add((path, format));
                }

                if (options.ScanMissingTypes && isTextYaml)
                {
                    foreach (var entry in SerializeReferenceYamlEditor.FindMissingReferences(path, SerializeReferenceHelpers.StoredTypeResolves, knownTextYaml: true))
                    {
                        if (IsPendingMigration(path, entry)) continue;
                        violations.Add(new GateViolation(path, entry.FileId, entry.Rid, entry.StoredType,
                            GateViolationKind.MissingType, entry.FieldPath, entry.IsOverride));
                    }
                }

                if (options.ScanRequiredFields)
                {
                    if (!isScene) CollectRequiredViolations(path, violations);
                    else if (isTextYaml) CollectSceneRequiredViolations(path, violations, knownTextYaml: true);
                }

                if (!wasLoaded && AssetDatabase.IsMainAssetAtPathLoaded(path)) loadedSinceUnload++;

                // Every loaded file stays in memory until unloaded, so a sweep of a large project would otherwise hold
                // all of its prefabs and assets at once.
                if (loadedSinceUnload < UnloadEveryLoadedFiles) continue;

                EditorUtility.UnloadUnusedAssetsImmediate();
                loadedSinceUnload = 0;
            }

            if (loadedSinceUnload > 0) EditorUtility.UnloadUnusedAssetsImmediate();

            return violations;
        }

        // A warning for the log, or null when nothing worth one was skipped. Outside Force Text every binary file warns.
        // Under Force Text, Unity still writes a few assets binary (LightingData, NavMesh) that cannot hold managed
        // references, so only a binary file that can (CanHoldManagedReferences) warns: a prefab or scene saved before
        // the switch, a [PreferBinarySerialization] ScriptableObject. An LFS pointer always warns: the real file was
        // never pulled.
        public static string DescribeUnscanned(
            IReadOnlyCollection<(string AssetPath, AssetFileFormat Format)> unscanned, SerializationMode serializationMode)
        {
            if (unscanned is null || unscanned.Count == 0) return null;

            var forceText = serializationMode == SerializationMode.ForceText;

            var pointers = unscanned
                .Where(file => file.Format == AssetFileFormat.LfsPointer)
                .Select(file => file.AssetPath)
                .ToList();

            var binaries = unscanned
                .Where(file => file.Format == AssetFileFormat.Binary)
                .Select(file => file.AssetPath)
                .Where(path => !forceText || CanHoldManagedReferences(path))
                .ToList();

            if (pointers.Count == 0 && binaries.Count == 0) return null;

            var builder = new StringBuilder();
            builder.AppendLine($"[Aspid FastTools] {pointers.Count + binaries.Count} file(s) were not checked for SerializeReference problems because they are not text YAML:");

            if (binaries.Count > 0 && forceText)
            {
                builder.AppendLine($"  {binaries.Count} binary prefab, scene or ScriptableObject file(s): save them again to write them as text. A [PreferBinarySerialization] asset stays binary.");
                AppendPaths(builder, binaries);
            }
            else if (binaries.Count > 0)
            {
                builder.AppendLine($"  {binaries.Count} binary file(s): Asset Serialization Mode is {serializationMode}. Set it to Force Text and save the assets again.");
            }

            if (pointers.Count > 0)
            {
                builder.AppendLine($"  {pointers.Count} Git LFS pointer(s): fetch the LFS objects before the check.");
                AppendPaths(builder, pointers);
            }

            return builder.ToString();
        }

        // Whether a binary file may hide managed references: a prefab or scene always may; an .asset when its main
        // asset is a ScriptableObject, or of an unknown type. The binaries Unity writes under Force Text (LightingData,
        // NavMesh) derive from UnityEngine.Object directly.
        public static bool CanHoldManagedReferences(string assetPath)
        {
            if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) return true;

            var mainType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return mainType is null || typeof(ScriptableObject).IsAssignableFrom(mainType);
        }

        private static void AppendPaths(StringBuilder builder, IReadOnlyList<string> paths)
        {
            foreach (var path in paths.Take(MaxListedPaths))
                builder.AppendLine($"    {path}");

            if (paths.Count > MaxListedPaths)
                builder.AppendLine($"    … and {paths.Count - MaxListedPaths} more");
        }

        public static IReadOnlyList<GateViolation> ScanAssetRequiredFields(string assetPath)
        {
            var violations = new List<GateViolation>();
            if (string.IsNullOrEmpty(assetPath) || !SerializeReferenceHelpers.IsScanCandidate(assetPath)) return violations;

            _scriptRequiredFieldsCache.Clear();

            if (SerializeReferenceHelpers.IsScene(assetPath)) CollectSceneRequiredViolations(assetPath, violations);
            else CollectRequiredViolations(assetPath, violations);

            return violations;
        }

        // Re-audits the files an edit touched and swaps their entries in a cached project audit; every other file's
        // violations are kept as they were.
        public static IReadOnlyList<GateViolation> RescanRequiredFields(
            IReadOnlyList<GateViolation> cached, IEnumerable<string> assetPaths)
        {
            var paths = new HashSet<string>(assetPaths, StringComparer.Ordinal);
            AddDependentPrefabs(paths);

            var violations = cached.Where(violation => !paths.Contains(violation.AssetPath)).ToList();

            foreach (var path in paths)
                violations.AddRange(ScanAssetRequiredFields(path));

            return violations;
        }

        // A variant of an edited prefab, or a prefab nesting it, inherits the edited values, so its required fields
        // may have changed as well. Scenes store only their overrides of a prefab instance, so they are not affected.
        private static void AddDependentPrefabs(HashSet<string> paths)
        {
            var editedPrefabs = new HashSet<string>(paths.Where(IsPrefab), StringComparer.Ordinal);
            if (editedPrefabs.Count == 0) return;

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!IsPrefab(path) || paths.Contains(path) || !SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                if (AssetDatabase.GetDependencies(path, recursive: true).Any(editedPrefabs.Contains))
                    paths.Add(path);
            }
        }

        private static bool IsPrefab(string path) => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);

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

        private static void CollectSceneRequiredViolations(
            string assetPath, List<GateViolation> violations, bool knownTextYaml = false)
        {
            foreach (var entry in SerializeReferenceYamlEditor.FindUnsetRequiredFields(assetPath, RequiredFieldsForScript, knownTextYaml))
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

        // A required reference whose type is missing counts as set here, including one inherited from a source prefab
        // or stored in a prefab override: the missing-type scan reports it once, from the file that holds it.
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

                    if (iterator.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String)) continue;
                    if (!TypeSelectorRequiredGate.IsViolation(iterator)) continue;

                    var rid = iterator.propertyType == SerializedPropertyType.ManagedReference ? iterator.managedReferenceId : 0L;
                    violations.Add(new GateViolation(assetPath, fileId, rid, default,
                        GateViolationKind.RequiredUnset, TypeSelectorRequiredGate.GetFieldPath(iterator)));
                }
                while (iterator.Next(enterChildren));
            }
        }
    }
}
