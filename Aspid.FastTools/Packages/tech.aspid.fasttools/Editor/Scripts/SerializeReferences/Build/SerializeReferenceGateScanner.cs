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

        // How many violations a log lists before it says how many more there are; the CI report lists them all.
        public const int MaxListedViolations = 50;

        // How many files a sweep loads before it releases the ones nothing references any more. The required-field
        // sweep and SerializeReferenceConstraintCache both use it. Not const so tests can lower it.
        internal static int UnloadEveryLoadedFiles = 64;

        // Per-run memo of BuildConstraintMap (LoadAllAssetsAtPath + full SerializedObject walk — heavy), built only
        // for assets whose unresolved entries carry a [MovedFrom] claim. Null marks an asset whose map failed to build.
        private static readonly Dictionary<string, Dictionary<(long fileId, long rid), Type[]>> _constraintMapCache =
            new(StringComparer.Ordinal);

        // Script guid -> required field descriptors of the C# type it resolves to. Keyed by guid so an unresolvable
        // script (deleted / non-MonoBehaviour) caches an empty set once instead of re-probing every object.
        private static readonly Dictionary<string, IReadOnlyList<RequiredFieldDescriptor>> _scriptRequiredFieldsCache =
            new(StringComparer.Ordinal);

        // Prefab -> the scan-candidate prefabs that depend on it (its variants and the prefabs nesting it). Built by the
        // first rescan after a project audit and kept until the next one, since GetDependencies of every prefab is
        // slow and a rescan follows each fix. A nesting added after the audit shows up with the next Scan.
        private static Dictionary<string, List<string>> _prefabDependents;

        // `unscanned` collects the candidates the YAML pass had to skip (binary files, LFS pointers, files that could not
        // be opened): their missing types, and a scene's required fields, were not checked. Prefabs and assets still
        // get the object-load required check, which does not depend on the file format. A text file whose managed
        // references are stored in another registry version is added too: its missing types were not checked.
        // `onProgress` runs before each file and returns true to stop; the result is then null, since a partial sweep
        // would pass for a clean project.
        public static IReadOnlyList<GateViolation> Scan(
            GateOptions options,
            Func<float, string, bool> onProgress = null,
            ICollection<(string AssetPath, AssetFileFormat Format)> unscanned = null)
        {
            var violations = new List<GateViolation>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();

            _scriptRequiredFieldsCache.Clear();
            _constraintMapCache.Clear();
            MissingTypeNames.ClearCache();
            ResetPrefabDependents();

            var loadedSinceUnload = 0;
            var stopped = false;

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];

                if (onProgress is not null && onProgress((float)i / Math.Max(1, paths.Length), path))
                {
                    stopped = true;
                    break;
                }

                var isScene = SerializeReferenceHelpers.IsScene(path);

                // Scenes are read as YAML. Any other file is loaded by the required sweep, or earlier by the
                // constraint map of a pending migration, and either load counts toward the next unload.
                var wasLoaded = !options.ScanRequiredFields || isScene || AssetDatabase.IsMainAssetAtPathLoaded(path);

                // Sniffed and read once here, for every YAML pass: managed references, the names stored by SerializableType
                // wrappers and the registry version, which the required-field reader of a scene cannot read either. The
                // scanners below are told the result instead of opening the file again. A file whose read fails after the
                // sniff passed (another program locked it in between) is unscanned too, not a clean one.
                var needsYaml = options.ScanMissingTypes || (options.ScanRequiredFields && isScene);
                var isTextYaml = true;
                string[] lines = null;

                if (needsYaml && File.Exists(path))
                {
                    lines = SerializeReferenceYaml.ReadLines(path, out var format);
                    isTextYaml = format == AssetFileFormat.TextYaml;
                    if (!isTextYaml) unscanned?.Add((path, format));
                }

                if (lines is not null && SerializeReferenceYaml.HasUnsupportedReferencesVersion(lines))
                    unscanned?.Add((path, AssetFileFormat.UnsupportedReferencesVersion));

                if (options.ScanMissingTypes && isTextYaml)
                {
                    foreach (var entry in SerializeReferenceYamlEditor.FindMissingReferences(lines, SerializeReferenceHelpers.StoredTypeResolves))
                    {
                        if (IsPendingMigration(path, entry)) continue;
                        violations.Add(new GateViolation(path, entry.FileId, entry.Rid, entry.StoredType,
                            GateViolationKind.MissingType, entry.FieldPath, entry.IsOverride));
                    }

                    foreach (var location in MissingTypeNames.Find(path, lines))
                        violations.Add(GateViolation.ForTypeName(path, location.Entry));
                }

                if (options.ScanRequiredFields)
                {
                    if (!isScene) CollectRequiredViolations(path, violations, typeNames: null);
                    else if (isTextYaml) CollectSceneRequiredViolations(path, lines, violations);
                }

                if (!wasLoaded && AssetDatabase.IsMainAssetAtPathLoaded(path)) loadedSinceUnload++;

                // Every loaded file stays in memory until unloaded, so a sweep of a large project would otherwise hold
                // all of its prefabs and assets at once.
                if (loadedSinceUnload < UnloadEveryLoadedFiles) continue;

                EditorUtility.UnloadUnusedAssetsImmediate();
                loadedSinceUnload = 0;
            }

            if (loadedSinceUnload > 0) EditorUtility.UnloadUnusedAssetsImmediate();

            return stopped ? null : violations;
        }

        // A warning for the log, or null when nothing worth one was skipped. Outside Force Text every binary file warns.
        // Under Force Text, Unity still writes a few assets binary (LightingData, NavMesh) that cannot hold managed
        // references, so only a binary file that can (CanHoldManagedReferences) warns: a prefab or scene saved before
        // the switch, a [PreferBinarySerialization] ScriptableObject. An LFS pointer always warns: the real file was
        // never pulled. So does a file that could not be opened, since nothing says what it holds, and a file with
        // another managed reference registry version.
        public static string DescribeUnscanned(
            IReadOnlyCollection<(string AssetPath, AssetFileFormat Format)> unscanned, SerializationMode serializationMode)
        {
            var (pointers, binaries, unreadable, otherRegistries) = SplitUnscanned(unscanned, serializationMode);
            var count = pointers.Count + binaries.Count + unreadable.Count + otherRegistries.Count;
            if (count == 0) return null;

            var forceText = serializationMode == SerializationMode.ForceText;
            var builder = new StringBuilder();
            builder.AppendLine($"[Aspid FastTools] {count} file(s) were not checked for SerializeReference problems:");

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

            if (unreadable.Count > 0)
            {
                builder.AppendLine($"  {unreadable.Count} file(s) could not be opened (locked by another program, no read permission, or removed during the check): close that program and run the check again.");
                AppendPaths(builder, unreadable);
            }

            if (otherRegistries.Count > 0)
            {
                builder.AppendLine($"  {otherRegistries.Count} file(s) keep managed references in a registry version other than 2, which the check cannot read: re-save them in this Unity version, for example with AssetDatabase.ForceReserializeAssets.");
                AppendPaths(builder, otherRegistries);
            }

            return builder.ToString();
        }

        // How many skipped files DescribeUnscanned warns about: the ones whose problems the check could have missed.
        public static int CountUnchecked(
            IReadOnlyCollection<(string AssetPath, AssetFileFormat Format)> unscanned, SerializationMode serializationMode)
        {
            var (pointers, binaries, unreadable, otherRegistries) = SplitUnscanned(unscanned, serializationMode);
            return pointers.Count + binaries.Count + unreadable.Count + otherRegistries.Count;
        }

        // How many of the unscanned files DescribeUnscanned names, for a window that shows a count.
        public static int CountReportableUnscanned(
            IEnumerable<(string AssetPath, AssetFileFormat Format)> unscanned, SerializationMode serializationMode)
        {
            var forceText = serializationMode == SerializationMode.ForceText;
            return unscanned.Count(file => IsReportable(file, forceText));
        }

        // For Asset References, which reads one picked asset: true when no scan can read it and DescribeUnscanned would
        // name it. False for a readable asset, a missing file and an asset type the scans never read.
        public static bool TryGetUnreadFormat(string assetPath, SerializationMode serializationMode, out AssetFileFormat format)
        {
            format = AssetFileFormat.TextYaml;
            if (!SerializeReferenceYaml.IsCandidateAssetPath(assetPath) || !File.Exists(assetPath)) return false;

            // Read as well as sniffed: a text file whose read fails shows no references either.
            SerializeReferenceYaml.ReadLines(assetPath, out format);
            return IsReportable((assetPath, format), forceText: serializationMode == SerializationMode.ForceText);
        }

        private static (List<string> Pointers, List<string> Binaries, List<string> Unreadable, List<string> OtherRegistries) SplitUnscanned(
            IReadOnlyCollection<(string AssetPath, AssetFileFormat Format)> unscanned, SerializationMode serializationMode)
        {
            if (unscanned is null || unscanned.Count == 0)
                return (new List<string>(), new List<string>(), new List<string>(), new List<string>());

            var forceText = serializationMode == SerializationMode.ForceText;

            var pointers = unscanned
                .Where(file => file.Format == AssetFileFormat.LfsPointer)
                .Select(file => file.AssetPath)
                .ToList();

            var binaries = unscanned
                .Where(file => file.Format == AssetFileFormat.Binary && IsReportable(file, forceText))
                .Select(file => file.AssetPath)
                .ToList();

            var unreadable = unscanned
                .Where(file => file.Format == AssetFileFormat.Unreadable)
                .Select(file => file.AssetPath)
                .ToList();

            var otherRegistries = unscanned
                .Where(file => file.Format == AssetFileFormat.UnsupportedReferencesVersion)
                .Select(file => file.AssetPath)
                .ToList();

            return (pointers, binaries, unreadable, otherRegistries);
        }

        // An LFS pointer and an unreadable file are reportable under any serialization mode: the first was never pulled,
        // and nothing is known about the second, so it cannot be ruled out as an engine binary.
        private static bool IsReportable((string AssetPath, AssetFileFormat Format) file, bool forceText) =>
            file.Format is AssetFileFormat.LfsPointer or AssetFileFormat.Unreadable ||
            (file.Format == AssetFileFormat.Binary && (!forceText || CanHoldManagedReferences(file.AssetPath)));

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

        // Asset References calls this for the one asset the user picked, so an excluded folder does not hide its
        // required fields, as it does not hide its missing types. The project audit filters excluded folders itself.
        public static IReadOnlyList<GateViolation> ScanAssetRequiredFields(string assetPath) =>
            ScanAssetRequiredFields(assetPath, typeNames: null);

        // typeNames, when given, also gets every type name field the walk passes, in field order, so Asset References
        // maps them without a second walk. A scene cannot be object-loaded, so it adds none.
        internal static IReadOnlyList<GateViolation> ScanAssetRequiredFields(string assetPath, List<TypeNameField> typeNames)
        {
            var violations = new List<GateViolation>();

            // Only a scanned path reports violations; the type names of any other asset the window maps are still read.
            var candidate = SerializeReferenceYaml.IsCandidateAssetPath(assetPath);
            if (!candidate && typeNames is null) return violations;

            _scriptRequiredFieldsCache.Clear();

            if (!SerializeReferenceHelpers.IsScene(assetPath))
                CollectRequiredViolations(assetPath, candidate ? violations : null, typeNames);
            else if (candidate)
                CollectSceneRequiredViolations(assetPath, violations);

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

            foreach (var path in paths.Where(SerializeReferenceHelpers.IsScanCandidate))
                violations.AddRange(ScanAssetRequiredFields(path));

            return violations;
        }

        // A variant of an edited prefab, or a prefab nesting it, inherits the edited values, so its required fields
        // may have changed as well. Scenes store only their overrides of a prefab instance, so they are not affected.
        private static void AddDependentPrefabs(HashSet<string> paths)
        {
            var editedPrefabs = paths.Where(IsPrefab).ToList();
            if (editedPrefabs.Count == 0) return;

            _prefabDependents ??= BuildPrefabDependents();

            foreach (var edited in editedPrefabs)
            {
                if (!_prefabDependents.TryGetValue(edited, out var dependents)) continue;

                // The excluded folders may have changed since the map was built.
                foreach (var dependent in dependents.Where(SerializeReferenceHelpers.IsScanCandidate))
                    paths.Add(dependent);
            }
        }

        internal static void ResetPrefabDependents() => _prefabDependents = null;

        private static Dictionary<string, List<string>> BuildPrefabDependents()
        {
            var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!IsPrefab(path) || !SerializeReferenceHelpers.IsScanCandidate(path)) continue;

                foreach (var dependency in AssetDatabase.GetDependencies(path, recursive: true))
                {
                    if (dependency == path || !IsPrefab(dependency)) continue;

                    if (!map.TryGetValue(dependency, out var dependents))
                        map[dependency] = dependents = new List<string>();

                    dependents.Add(path);
                }
            }

            return map;
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
                SerializeReferenceHelpers.FitsConstraints(target, constraint);
        }

        private static Dictionary<(long fileId, long rid), Type[]> ConstraintMapFor(string assetPath)
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

        private static void CollectSceneRequiredViolations(string assetPath, List<GateViolation> violations) =>
            CollectSceneRequiredViolations(assetPath, SerializeReferenceYaml.ReadLines(assetPath), violations);

        private static void CollectSceneRequiredViolations(string assetPath, string[] lines, List<GateViolation> violations)
        {
            foreach (var entry in SerializeReferenceYamlEditor.FindUnsetRequiredFields(lines, RequiredFieldsForScript))
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
        private static void CollectRequiredViolations(string assetPath, List<GateViolation> violations,
            List<TypeNameField> typeNames)
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

                    if (typeNames is not null && TypeSelectorRequiredGate.IsTypeNameField(iterator, out var selector))
                    {
                        typeNames.Add(new TypeNameField(fileId, TypeSelectorRequiredGate.GetFieldPath(iterator),
                            asset.GetType().Name, iterator.stringValue, required: selector is { Required: true }));
                    }

                    if (violations is null || !TypeSelectorRequiredGate.IsViolation(iterator)) continue;

                    var rid = iterator.propertyType == SerializedPropertyType.ManagedReference ? iterator.managedReferenceId : 0L;
                    violations.Add(new GateViolation(assetPath, fileId, rid, default,
                        GateViolationKind.RequiredUnset, TypeSelectorRequiredGate.GetFieldPath(iterator),
                        componentName: asset.GetType().Name));
                }
                while (iterator.Next(enterChildren));
            }
        }
    }
}
