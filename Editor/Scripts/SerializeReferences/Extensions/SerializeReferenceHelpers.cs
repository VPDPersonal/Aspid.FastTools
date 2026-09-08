using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Runtime.Serialization;
using Aspid.FastTools.Types.Editors;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceHelpers
    {
        // managedReferenceFieldTypename reports the element type for array entries.
        public static Type GetFieldType(SerializedProperty property) =>
            GetTypeFromTypename(property.managedReferenceFieldTypename) ?? typeof(object);

        public static Type GetCurrentType(SerializedProperty property) =>
            property.managedReferenceValue?.GetType();

        private const string ManagedReferenceElementPrefix = "managedReference<";

        public static bool IsManagedReferenceArray(SerializedProperty property) =>
            property is { isArray: true, propertyType: not SerializedPropertyType.String } &&
            property.arrayElementType.StartsWith(ManagedReferenceElementPrefix, StringComparison.Ordinal);

        // An empty array has no element to inspect; resolve its declared type through reflection first.
        public static Type GetArrayElementType(SerializedProperty property)
        {
            if (property.GetFieldInfo() is { } field)
            {
                var elementType = field.FieldType.GetCollectionElementTypeOrSelf();
                if (elementType != field.FieldType) return elementType;
            }

            return property.arraySize > 0
                ? GetFieldType(property.GetArrayElementAtIndex(0))
                : typeof(object);
        }

        #region Project scan helpers
        public static bool IsScanCandidate(string path) =>
            SerializeReferenceYaml.IsCandidateAssetPath(path) && !SerializeReferenceSettings.IsExcluded(path);

        // Scenes cannot be read through LoadAllAssetsAtPath, so every object-loading scanner skips them and takes
        // the YAML pass instead.
        public static bool IsScene(string path) =>
            !string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

        public static string StoredTypeKey(ManagedTypeName type) =>
            $"{type.Assembly}|{type.Namespace}|{type.Class}";

        // Match closed generic usages to their script by removing arguments while retaining the generic arity.
        public static string OpenTypeKey(ManagedTypeName type) =>
            OpenTypeKey(StoredTypeKey(type));

        // The bracket only appears inside the class segment and the arity is kept, so different arities never
        // collapse and namespace and assembly stay intact.
        public static string OpenTypeKey(string storedTypeKey)
        {
            if (string.IsNullOrEmpty(storedTypeKey)) return storedTypeKey ?? string.Empty;

            var bracket = storedTypeKey.IndexOf('[');
            return bracket >= 0 ? storedTypeKey[..bracket] : storedTypeKey;
        }
        #endregion

        #region Multi-object editing
        public static bool IsEditingMultipleObjects(SerializedProperty property) =>
            property.serializedObject.isEditingMultipleObjects;

        public static bool HasMixedTypes(SerializedProperty property)
        {
            if (!property.serializedObject.isEditingMultipleObjects) return false;

            // hasMultipleDifferentValues misses the all-missing case, where every target reads back null but the
            // stored, unloadable type names still differ.
            if (property.hasMultipleDifferentValues) return true;

            if (property.managedReferenceValue is not null) return false;

            var first = property.managedReferenceFullTypename;
            var targets = property.serializedObject.targetObjects;
            if (targets.Length < 2) return false;

            if (TryGetMixedCache(property.propertyPath, first, targets, out var cached)) return cached;

            var result = false;
            foreach (var target in targets)
            {
                if (target == null) continue;

                using var single = new SerializedObject(target);
                var other = single.FindProperty(property.propertyPath);
                if (other is null) continue;
                if (other.managedReferenceFullTypename != first) { result = true; break; }
            }

            StoreMixedCache(property.propertyPath, first, targets, result);
            return result;
        }

        // Cache by property path within one selection; invalidate when the selection changes.
        private static Object[] _mixedTargets;
        private static readonly Dictionary<string, (string first, bool result)> _mixedResults = new(StringComparer.Ordinal);

        // Keyed by selection, not file state, so an external rewrite of the selected assets must drop it explicitly.
        public static void InvalidateMixedTypesCache()
        {
            _mixedTargets = null;
            _mixedResults.Clear();
        }

        private static bool TryGetMixedCache(string path, string first, UnityEngine.Object[] targets, out bool result)
        {
            result = false;
            if (!MixedTargetsMatch(targets)) return false;
            if (!_mixedResults.TryGetValue(path, out var entry) || entry.first != first) return false;

            result = entry.result;
            return true;
        }

        private static bool MixedTargetsMatch(UnityEngine.Object[] targets)
        {
            if (_mixedTargets is null || _mixedTargets.Length != targets.Length) return false;

            for (var i = 0; i < targets.Length; i++)
                if (!ReferenceEquals(_mixedTargets[i], targets[i])) return false;

            return true;
        }

        private static void StoreMixedCache(string path, string first, UnityEngine.Object[] targets, bool result)
        {
            if (!MixedTargetsMatch(targets))
            {
                _mixedResults.Clear();
                _mixedTargets = (Object[])targets.Clone(); // snapshot the references so a reused array can't alias
            }

            _mixedResults[path] = (first, result);
        }

        // Assign per target so each object receives an independent instance; collapse the writes into one Undo step.
        public static void ApplyManagedReferencePerTarget(SerializedProperty property, Func<object, object> factory)
        {
            var serializedObject = property.serializedObject;
            var targets = serializedObject.targetObjects;
            var propertyPath = property.propertyPath;

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();

            foreach (var target in targets)
            {
                if (target == null) continue;

                using var single = new SerializedObject(target);
                var singleProperty = single.FindProperty(propertyPath);
                if (singleProperty is null) continue;

                var previous = singleProperty.managedReferenceValue;
                var instance = factory(previous);

                singleProperty.managedReferenceValue = instance;
                singleProperty.isExpanded = instance is not null;
                single.ApplyModifiedProperties();
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Update() pulls the per-target writes back in; applying instead would write the live object's stale
            // reference back over them.
            serializedObject.Update();
        }

        // Repair notices operate on one backing asset and cannot represent a multi-object selection.
        public static bool NoticesApply(SerializedProperty property) =>
            !property.serializedObject.isEditingMultipleObjects;
        #endregion

        // Unity hides missing type identities in live properties; the original identity survives only in YAML.
        public static bool IsMissingType(SerializedProperty property) =>
            TryGetMissingType(property, out _, out _);

        // Missing-reference probes run repeatedly during repaint; same-frame repairs explicitly invalidate this memo.
        private static int _missingProbeFrame = -1;
        private static readonly Dictionary<(int instanceId, string path), (bool missing, long referenceId, ManagedTypeName storedType)>
            _missingProbeMemo = new();

        public static void InvalidateMissingTypeMemo() => _missingProbeFrame = -1;

        private static bool TryGetMissingType(SerializedProperty property, out long referenceId, out ManagedTypeName storedType)
        {
            referenceId = 0;
            storedType = default;

            if (property.propertyType != SerializedPropertyType.ManagedReference) return false;
            if (property.managedReferenceValue is not null) return false;

            var frame = Time.frameCount;
            if (_missingProbeFrame != frame)
            {
                _missingProbeMemo.Clear();
                _missingProbeFrame = frame;
            }

            var target = property.serializedObject.targetObject;
            var key = (target != null ? target.GetInstanceID() : 0, property.propertyPath);

            if (_missingProbeMemo.TryGetValue(key, out var cached))
            {
                referenceId = cached.referenceId;
                storedType = cached.storedType;
                return cached.missing;
            }

            var missing = ProbeMissingType(property, out referenceId, out storedType);
            _missingProbeMemo[key] = (missing, referenceId, storedType);
            return missing;
        }

        private static bool ProbeMissingType(SerializedProperty property, out long referenceId, out ManagedTypeName storedType)
        {
            referenceId = 0;
            storedType = default;

            if (!TryGetRepairLocation(property, out var assetPath, out var fileId, out _)) return false;
            if (!SerializeReferenceYamlEditor.TryReadStoredType(assetPath, fileId, property.propertyPath, out referenceId, out storedType))
                return false;

            return !storedType.IsEmpty && !StoredTypeResolves(storedType);
        }

        public static bool StoredTypeResolves(ManagedTypeName name)
        {
            if (string.IsNullOrEmpty(name.Class)) return false;

            var className = name.Class.Replace('/', '+');
            var fullName = string.IsNullOrEmpty(name.Namespace) ? className : $"{name.Namespace}.{className}";
            var assemblyQualified = string.IsNullOrEmpty(name.Assembly) ? fullName : $"{fullName}, {name.Assembly}";

            return Type.GetType(assemblyQualified, throwOnError: false) is not null;
        }

        // Managed-reference candidates do not require IsSerializable here; by-value generic arguments are checked separately.
        public static bool IsAssignableManagedReference(Type type) =>
            type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
            type != typeof(string) &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type);

        public static Func<Type, bool> BuildAssignableFilter(Type[] baseTypes)
        {
            var narrowing = FilterNarrowingTypes(baseTypes);
            if (narrowing is null) return IsAssignableManagedReference;

            return type => IsAssignableManagedReference(type) &&
                           Array.Exists(narrowing, baseType => baseType.IsAssignableFrom(type));
        }

        // Return null when no constraint narrows the candidates to avoid allocating a predicate.
        private static Type[] FilterNarrowingTypes(Type[] baseTypes)
        {
            if (baseTypes is null || baseTypes.Length == 0) return null;

            var count = 0;
            foreach (var type in baseTypes)
                if (type is not null && type != typeof(object)) count++;

            if (count == 0) return null;

            var result = new Type[count];
            var index = 0;
            foreach (var type in baseTypes)
                if (type is not null && type != typeof(object)) result[index++] = type;

            return result;
        }

        public static object CreateInstance(Type type)
        {
            if (type is null) return null;

            try
            {
                return Activator.CreateInstance(type, nonPublic: true);
            }
            catch (MissingMethodException)
            {
                return FormatterServices.GetUninitializedObject(type);
            }
        }

        public static object CreateInstancePreservingData(Type newType, object previous)
        {
            var instance = CreateInstance(newType);
            if (instance is null || previous is null) return instance;

            try
            {
                var json = JsonUtility.ToJson(previous);
                if (!string.IsNullOrEmpty(json) && json != "{}")
                    JsonUtility.FromJsonOverwrite(json, instance);
            }
            catch (Exception)
            {
                // Best effort: incompatible layouts just mean nothing is carried over.
            }

            // JsonUtility skips [SerializeReference] fields, so nested references are carried by reflection — the
            // very instances, not copies, so aliases onto them survive the type switch.
            try
            {
                CarryManagedReferences(previous, instance);
            }
            catch (Exception)
            {
                // Same best-effort contract as the JSON pass.
            }

            return instance;
        }

        private static void CarryManagedReferences(object previous, object instance)
        {
            Dictionary<string, FieldInfo> targets = null;

            foreach (var field in EnumerateManagedReferenceFields(previous.GetType()))
            {
                if (targets is null)
                {
                    targets = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
                    foreach (var target in EnumerateManagedReferenceFields(instance.GetType()))
                        targets[target.Name] = target;
                }

                if (!targets.TryGetValue(field.Name, out var into)) continue;

                var value = field.GetValue(previous);
                if (value is null || into.FieldType.IsInstanceOfType(value))
                    into.SetValue(instance, value);
            }
        }

        // Register each clone before cloning its children to preserve aliases and terminate cycles.
        public static object CloneManagedReferenceGraph(object source) =>
            CloneManagedReferenceGraph(source, new Dictionary<object, object>(ReferenceComparer.Instance));

        private static object CloneManagedReferenceGraph(object source, Dictionary<object, object> clones)
        {
            if (source is null) return null;
            if (clones.TryGetValue(source, out var existing)) return existing;

            var clone = CreateInstancePreservingData(source.GetType(), source);
            if (clone is null) return null;
            clones[source] = clone;

            foreach (var field in EnumerateManagedReferenceFields(source.GetType()))
                field.SetValue(clone, CloneManagedReferenceValue(field.GetValue(source), clones));

            return clone;
        }

        private static object CloneManagedReferenceValue(object value, Dictionary<object, object> clones)
        {
            switch (value)
            {
                case null:
                    return null;

                case Array array:
                {
                    var copy = Array.CreateInstance(array.GetType().GetElementType()!, array.Length);
                    for (var i = 0; i < array.Length; i++)
                        copy.SetValue(CloneManagedReferenceGraph(array.GetValue(i), clones), i);
                    return copy;
                }

                case IList list:
                {
                    var copy = (IList)Activator.CreateInstance(value.GetType());
                    foreach (var element in list)
                        copy.Add(CloneManagedReferenceGraph(element, clones));
                    return copy;
                }

                default:
                    return CloneManagedReferenceGraph(value, clones);
            }
        }

        private static IEnumerable<FieldInfo> EnumerateManagedReferenceFields(Type type)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
                foreach (var field in current.GetFields(flags))
                {
                    if (field.IsStatic || field.IsInitOnly || field.IsNotSerialized) continue;
                    if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), inherit: false)) continue;
                    if (field.IsDefined(typeof(SerializeReference), inherit: false)) yield return field;
                }
        }

        // A user-defined Equals must not merge distinct instances in the clone map, or split one.
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        public static Type GetTypeFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename)) return null;

            var separator = typename.IndexOf(' ');
            if (separator < 0) return Type.GetType(typename, throwOnError: false);

            var assembly = typename[..separator];
            var fullName = typename[(separator + 1)..];
            return Type.GetType($"{fullName}, {assembly}", throwOnError: false);
        }

        // Unity serializes these built-in field types despite Type.IsSerializable returning false.
        private static readonly HashSet<Type> UnityNativeSerializableTypes = new()
        {
            typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(Vector2Int), typeof(Vector3Int),
            typeof(Quaternion), typeof(Matrix4x4),
            typeof(Color), typeof(Color32), typeof(Gradient),
            typeof(Rect), typeof(RectInt), typeof(Bounds), typeof(BoundsInt),
            typeof(LayerMask), typeof(AnimationCurve),
            typeof(PropertyName), typeof(UnityEngine.Rendering.SphericalHarmonicsL2),
        };

        public static bool IsValidGenericArgument(Type type)
        {
            if (type is null) return false;
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters) return false;
            if (typeof(Delegate).IsAssignableFrom(type)) return false;

            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   typeof(Object).IsAssignableFrom(type) ||
                   UnityNativeSerializableTypes.Contains(type) ||
                   (type.IsValueType && type.IsSerializable) ||
                   (type.IsClass && type.IsSerializable);
        }

        // Inferred arguments require serializability only when the parameter reaches a field serialized by value.
        public static bool IsAcceptableGenericArgument(Type openDefinition, Type parameter, Type argument)
        {
            if (argument is null || argument.ContainsGenericParameters) return false;
            if (argument.IsPointer || argument.IsByRef || argument == typeof(void)) return false;

            return !GenericArgumentRequirement.RequiresSerializableArgument(openDefinition, parameter) ||
                   IsValidGenericArgument(argument);
        }

        #region Missing-type repair
        public static ManagedTypeName GetMissingTypeName(SerializedProperty property) =>
            TryGetMissingType(property, out _, out var storedType) ? storedType : default;

        public static string GetMissingTypeDisplayName(SerializedProperty property) =>
            GetMissingTypeName(property).DisplayName;

        // Rank only picker-compatible candidates so quick repair cannot bypass the field constraints.
        public static bool TryGetRepairSuggestion(SerializedProperty property, Type[] baseTypes,
            out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            if (!TryGetMissingType(property, out var referenceId, out var storedType)) return false;
            if (!TryGetRepairLocation(property, out var assetPath, out var fileId, out var inMemory)) return false;

            var fieldType = GetFieldType(property);
            var pickerFilter = BuildAssignableFilter(baseTypes);

            var ranked = SerializeReferenceRepairSuggestions.GetCached(assetPath, fileId, referenceId,
                () => SerializeReferenceRepairSuggestions.Rank(
                    storedType,
                    GetMissingFieldNames(property, assetPath, fileId, referenceId, inMemory),
                    fieldType));

            foreach (var candidate in ranked)
            {
                if (!pickerFilter(candidate.Type)) continue;
                suggestion = candidate;
                return true;
            }

            return false;
        }

        public static string GetSuggestionLabel(SerializeReferenceRepairSuggestions.RepairCandidate suggestion) =>
            $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion.Type)}";

        public static string GetSuggestionDetail(SerializeReferenceRepairSuggestions.RepairCandidate suggestion) =>
            $"Suggested: {suggestion.Type.FullName}, {suggestion.Type.Assembly.GetName().Name}.\n" +
            $"Reason: {suggestion.Reason}.\nClick to re-point this reference to it, keeping its data.";

        // Field names of the missing reference's orphaned payload, for the field-shape heuristic. A Prefab Mode
        // object has no committed data block, so the flat payload Unity still exposes is parsed instead.
        private static List<string> GetMissingFieldNames(SerializedProperty property, string assetPath, long fileId, long referenceId, bool inMemory)
        {
            if (!inMemory)
                return SerializeReferenceYamlEditor.GetReferenceFieldNames(assetPath, fileId, referenceId);

            var target = property.serializedObject.targetObject;
            foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                if (entry.referenceId == referenceId)
                    return SerializeReferenceYamlEditor.ParseTopLevelFieldNames(entry.serializedData);

            return new List<string>();
        }

        // The asset path and the target's local file id — the YAML document anchor. False for scene objects and
        // prefab instances, which have no editable asset file of their own.
        public static bool TryGetAssetLocation(SerializedProperty property, out string assetPath, out long fileId)
        {
            fileId = 0;
            var target = property.serializedObject.targetObject;
            assetPath = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(assetPath)) return false;
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out _, out fileId);
        }

        // Prefab Mode edits must stay in memory; its open copy would overwrite a YAML repair on save.
        public static bool TryGetRepairLocation(SerializedProperty property, out string assetPath, out long fileId, out bool inMemory)
        {
            inMemory = false;
            if (TryGetAssetLocation(property, out assetPath, out fileId)) return true;

            assetPath = null;
            fileId = 0;

            var target = property.serializedObject.targetObject;
            var go = target as GameObject ?? (target as Component)?.gameObject;
            if (go is null) return false;

            var stage = PrefabStageUtility.GetPrefabStage(go);
            if (stage is not null)
            {
                if (!TryMatchAssetFileId(stage, target, go, out fileId)) return false;

                assetPath = stage.assetPath;
                inMemory = true;
                return true;
            }

            // A saved scene is the document store and the scene-local file id the anchor, but a loaded scene must
            // not be rewritten on disk under itself, so the repair stays in memory.
            if (TryGetSceneLocation(target, go, out assetPath, out fileId))
            {
                inMemory = true;
                return true;
            }

            return false;
        }

        // Scene YAML is usable only while the scene is clean and the reference is not a prefab override.
        private static bool TryGetSceneLocation(Object target, GameObject go, out string assetPath, out long fileId)
        {
            assetPath = null;
            fileId = 0;

            var scene = go.scene;
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path) || scene.isDirty) return false;

            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(target);
            if (globalId.identifierType != 2) return false;     // 2 == scene object
            if (globalId.targetPrefabId != 0) return false;      // a prefab-instance override — defer to the source prefab

            assetPath = scene.path;
            fileId = unchecked((long)globalId.targetObjectId);
            return true;
        }

        // A nested prefab instance's reference data lives in the source prefab rather than the host.
        public static bool TryGetSourcePrefabPath(Object target, out string sourcePath)
        {
            sourcePath = null;
            if (target == null) return false;

            var go = target as GameObject ?? (target as Component)?.gameObject;
            if (go is null || !PrefabUtility.IsPartOfPrefabInstance(go)) return false;

            sourcePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            return !string.IsNullOrEmpty(sourcePath);
        }

        // A Prefab Mode object is a copy in a preview scene with no file id of its own, so the persisted object is
        // found by replaying its child path from the stage root.
        private static bool TryMatchAssetFileId(PrefabStage stage, Object target, GameObject stageGo, out long fileId)
        {
            fileId = 0;

            // A dirty stage has diverged from the asset, so the index replay would land on the wrong object.
            if (stage.scene.isDirty) return false;

            var indices = new List<int>();
            var transform = stageGo.transform;
            var root = stage.prefabContentsRoot.transform;
            while (transform != root)
            {
                if (transform.parent is null) return false;
                indices.Insert(0, transform.GetSiblingIndex());
                transform = transform.parent;
            }

            var assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
            if (assetRoot is null) return false;

            var assetTransform = assetRoot.transform;
            foreach (var index in indices)
            {
                if (index < 0 || index >= assetTransform.childCount) return false;
                assetTransform = assetTransform.GetChild(index);
            }

            if (target is not Component component)
                return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetTransform.gameObject, out _, out fileId);

            // Disambiguate by component index in case the object carries several components of the same type.
            var stageComponents = stageGo.GetComponents(component.GetType());
            var componentIndex = Array.IndexOf(stageComponents, component);
            var assetComponents = assetTransform.GetComponents(component.GetType());
            if (componentIndex < 0 || componentIndex >= assetComponents.Length) return false;

            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetComponents[componentIndex], out _, out fileId);
        }

        public static bool TryGetMissingReferenceId(SerializedProperty property, out long referenceId) =>
            TryGetMissingType(property, out referenceId, out _);

        // Repair permits hidden types because visibility limits authoring, not recovery of existing data.
        public static void ShowFixTypeSelector(SerializedProperty property, Rect screenRect, Action onFixed, Type[] baseTypes = null)
        {
            var fieldType = GetFieldType(property);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                filter: new TypeSelectorFilter
                {
                    Types = new[] { fieldType },
                    Predicate = BuildAssignableFilter(baseTypes),
                    AdditionalTypes = GenericTypeResolver.GetAssignableGenericDefinitions(fieldType, baseTypes, IsAcceptableGenericArgument),
                    ArgumentFilter = IsValidGenericArgument,
                    InferredArgumentFilter = IsAcceptableGenericArgument,
                    IncludeHidden = true,
                },
                currentAqn: null, // a missing-type Fix has no current value — nothing (not even <None>) wears the check
                onSelected: assemblyQualifiedName =>
                {
                    var type = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    if (type is not null && TryFixMissingType(property, type))
                        onFixed?.Invoke();
                });
        }

        // Repair saved assets through YAML and open Prefab Mode objects through their live serialized state.
        public static bool TryFixMissingType(SerializedProperty property, Type newType)
        {
            if (newType is null) return false;
            if (!TryGetRepairLocation(property, out var assetPath, out var fileId, out var inMemory)) return false;
            if (!TryGetMissingReferenceId(property, out var referenceId)) return false;

            bool repaired;
            if (inMemory)
            {
                repaired = TryFixMissingTypeInMemory(property, newType, referenceId);
            }
            else
            {
                repaired = SerializeReferenceYamlEditor.TryRewriteType(assetPath, fileId, referenceId, ManagedTypeName.FromType(newType));
                // ForceUpdate invalidates the live SerializedObject, so the property must not be touched afterwards.
                if (repaired) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            // An IMGUI repaint can land in the same frame as this click, so the frame-keyed memos must go too.
            if (repaired)
            {
                SerializeReferenceRepairSuggestions.ClearCache();
                SerializeReferenceYamlProbeCache.ClearCache();
                InvalidateSharedReferenceCache();
                InvalidateMissingTypeMemo();
            }

            if (repaired) ScheduleInspectorRebuild();
            return repaired;
        }

        // Unity caches the missing-types banner on editor creation; reselection rebuilds it after repair.
        private static void ScheduleInspectorRebuild()
        {
            var selection = Selection.objects;
            if (selection is null || selection.Length == 0) return;

            EditorApplication.delayCall += () =>
            {
                Selection.objects = Array.Empty<Object>();
                EditorApplication.delayCall += () => Selection.objects = selection;
            };
        }

        // The open stage holds a copy that does not refresh on reimport and would overwrite a file rewrite on save,
        // so the reference is reassigned on the live object and the now-unused missing-type entry cleared.
        private static bool TryFixMissingTypeInMemory(SerializedProperty property, Type newType, long referenceId)
        {
            var target = property.serializedObject.targetObject;
            var instance = CreateInstance(newType);
            if (instance is null) return false;

            foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
            {
                if (entry.referenceId != referenceId) continue;
                RecoverManagedReferenceData(entry.serializedData, instance);
                break;
            }

            property.SetManagedReferenceAndApply(instance);
            ClearMissingSubtree(target, referenceId);
            EditorUtility.SetDirty(target);
            property.serializedObject.Update();

            var scene = (target as Component)?.gameObject.scene ?? (target as GameObject)?.scene ?? default;
            if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

            return true;
        }

        // The in-memory counterpart of the YAML clear, used when a file rewrite would be clobbered by the open copy
        // on save. Marks the owning scene dirty, so the file — and the audit listing — only update once saved.
        public static bool TryClearMissingReferenceInMemory(string assetPath, long rid, ManagedTypeName storedType)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;

            foreach (var target in EnumerateOpenMissingTypeTargets(assetPath))
            {
                var matched = false;
                foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                {
                    if (entry.referenceId != rid) continue;
                    // Also match the stored class when known, in case another live object reuses the rid.
                    if (!string.IsNullOrEmpty(storedType.Class) && entry.className != storedType.Class) continue;
                    matched = true;
                    break;
                }

                if (!matched) continue;

                ClearMissingSubtree(target, rid);
                EditorUtility.SetDirty(target);
                InvalidateMissingTypeMemo();

                var scene = (target as Component)?.gameObject.scene ?? default;
                if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

                return true;
            }

            return false;
        }

        // Open stages remap file IDs, so match missing-reference identities on live MonoBehaviours.
        private static IEnumerable<Object> EnumerateOpenMissingTypeTargets(string assetPath)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && string.Equals(stage.assetPath, assetPath, StringComparison.Ordinal) && stage.prefabContentsRoot != null)
                foreach (var mb in stage.prefabContentsRoot.GetComponentsInChildren<MonoBehaviour>(true))
                    if (mb != null) yield return mb;

            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(assetPath);
            if (scene.IsValid() && scene.isLoaded)
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                        if (mb != null) yield return mb;
        }

        // Preserve any repaired-subtree member referenced from outside it, including that member's descendants.
        private static void ClearMissingSubtree(Object target, long rootReferenceId)
        {
            var dataByRid = new Dictionary<long, string>();
            foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                dataByRid[entry.referenceId] = entry.serializedData;

            var closure = new HashSet<long>();
            var pending = new Stack<long>();
            pending.Push(rootReferenceId);

            while (pending.Count > 0)
            {
                var rid = pending.Pop();
                if (!closure.Add(rid)) continue;
                if (!dataByRid.TryGetValue(rid, out var data)) continue; // a resolvable reference, or already cleared

                foreach (var child in EnumerateRidPointers(data, rid))
                    pending.Push(child);
            }

            // Protect every closure member still referenced from outside it. The repaired field itself now points
            // at the fresh instance, so it no longer counts.
            var keep = new HashSet<long>();

            foreach (var pair in dataByRid)
            {
                if (closure.Contains(pair.Key)) continue;
                foreach (var child in EnumerateRidPointers(pair.Value, pair.Key))
                    if (closure.Contains(child))
                        keep.Add(child);
            }

            using (var serializedObject = new SerializedObject(target))
                TraverseManagedReferences(serializedObject, property =>
                {
                    var id = property.managedReferenceId;
                    if (closure.Contains(id)) keep.Add(id);
                    return false;
                });

            // A kept entry still points at its own children, so protection propagates down the closure.
            foreach (var rid in keep) pending.Push(rid);
            while (pending.Count > 0)
            {
                var rid = pending.Pop();
                if (!dataByRid.TryGetValue(rid, out var data)) continue;

                foreach (var child in EnumerateRidPointers(data, rid))
                    if (closure.Contains(child) && keep.Add(child))
                        pending.Push(child);
            }

            foreach (var rid in closure)
            {
                if (!keep.Contains(rid) && dataByRid.ContainsKey(rid))
                    SerializationUtility.ClearManagedReferenceWithMissingType(target, rid);
            }
        }

        private static IEnumerable<long> EnumerateRidPointers(string data, long self)
        {
            foreach (Match match in Regex.Matches(data ?? string.Empty, @"(?<!\w)rid:\s*(-?\d+)"))
            {
                if (long.TryParse(match.Groups[1].Value, out var child) && child != self)
                    yield return child;
            }
        }

        // Unity surfaces the orphaned payload as YAML scalars; the flat top-level ones are mapped to JSON and
        // overwritten onto the instance. Nested mappings and sequences stay at the new type's defaults.
        private static void RecoverManagedReferenceData(string serializedData, object instance)
        {
            if (string.IsNullOrEmpty(serializedData)) return;

            try
            {
                var json = new StringBuilder("{");
                var first = true;

                foreach (var raw in serializedData.Split('\n'))
                {
                    var line = raw.TrimEnd('\r');
                    if (line.Length == 0 || char.IsWhiteSpace(line[0]) || line[0] == '-') continue;

                    var separator = line.IndexOf(':');
                    if (separator <= 0) continue;

                    var key = line[..separator].Trim();
                    var value = line[(separator + 1)..].Trim();

                    // An empty value is a mapping or array header, and a flow value is not a flat scalar.
                    if (key.Length == 0 || value.Length == 0 || value[0] is '{' or '[') continue;

                    if (!first) json.Append(',');
                    first = false;

                    json.Append('"').Append(key).Append("\":");
                    json.Append(IsJsonNumber(value) ? value : Quote(UnquoteYaml(value)));
                }

                json.Append('}');
                if (!first) JsonUtility.FromJsonOverwrite(json.ToString(), instance);
            }
            catch (Exception)
            {
                // Best effort: an unparseable payload simply leaves the new instance at its defaults.
            }
        }

        private static bool IsJsonNumber(string value) => Regex.IsMatch(value, @"^-?\d+(\.\d+)?$");

        private static string UnquoteYaml(string value) =>
            value.Length >= 2 && value[0] == '\'' && value[^1] == '\''
                ? value[1..^1].Replace("''", "'")
                : value;

        private static string Quote(string value) =>
            $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        #endregion

        #region Constraint map
        // Combine declared live field types with YAML IDs; missing parents and orphaned entries remain unconstrained.
        public static Dictionary<(long fileId, long rid), Type> BuildConstraintMap(string assetPath)
        {
            var map = new Dictionary<(long, long), Type>();
            if (string.IsNullOrEmpty(assetPath)) return map;

            // Scenes cannot be read through LoadAllAssetsAtPath, so an unconstrained picker is the fallback.
            if (IsScene(assetPath)) return map;

            // A cyclic graph would loop the walk forever. Cleared per document, since rids are document-scoped.
            var visited = new HashSet<long>();

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var fileId)) continue;

                visited.Clear();

                using var serialized = new SerializedObject(obj);
                var iterator = serialized.GetIterator();

                var enterChildren = true;
                while (iterator.Next(enterChildren))
                {
                    enterChildren = true;

                    if (iterator.propertyType != SerializedPropertyType.ManagedReference) continue;

                    long rid;
                    if (iterator.managedReferenceValue is not null)
                        rid = iterator.managedReferenceId;
                    else if (!SerializeReferenceYamlEditor.TryReadReferenceId(assetPath, fileId, iterator.propertyPath, out rid))
                        continue;

                    // A back-edge: record the constraint, but do not descend into the subtree again.
                    if (rid >= 0 && !visited.Add(rid)) enterChildren = false;

                    var fieldType = GetFieldType(iterator);
                    if (fieldType is null || fieldType == typeof(object)) continue;

                    map[(fileId, rid)] = fieldType;
                }
            }

            return map;
        }
        #endregion

        #region Cross references
        public static bool HasSharedReference(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return false;

            var id = property.managedReferenceId;

            // Built once per object per frame: GetHeight and Draw each ask this for every field, so a per-property
            // full-object walk would be 2*N walks per repaint.
            return GetReferenceIdCounts(property.serializedObject).TryGetValue(id, out var count) && count > 1;
        }

        // Number shared groups by first appearance so both inspector modes assign the same badge.
        public static int GetSharedReferenceIndex(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return 0;

            var id = property.managedReferenceId;
            return GetSharedReferenceIndices(property.serializedObject).TryGetValue(id, out var index) ? index : 0;
        }

        private static int _aliasFrame = -1;
        private static SerializedObject _aliasSerializedObject;
        private static readonly Dictionary<long, int> AliasCounts = new();

        private static readonly List<long> AliasOrder = new();

        private static Dictionary<long, int> GetReferenceIdCounts(SerializedObject serializedObject)
        {
            var frame = Time.frameCount;
            if (_aliasFrame == frame && ReferenceEquals(_aliasSerializedObject, serializedObject))
                return AliasCounts;

            AliasCounts.Clear();
            AliasOrder.Clear();
            TraverseManagedReferences(serializedObject, other =>
            {
                // Every empty field reports the same sentinel, so counting those would form a phantom group.
                var id = other.managedReferenceId;
                if (id < 0) return false;

                if (!AliasCounts.TryGetValue(id, out var count)) AliasOrder.Add(id);
                AliasCounts[id] = count + 1;
                return false;
            });

            _sharedIndicesFrame = -1;
            _sharedPathsFrame = -1;
            _aliasFrame = frame;
            _aliasSerializedObject = serializedObject;
            return AliasCounts;
        }

        private static int _sharedIndicesFrame = -1;
        private static SerializedObject _sharedIndicesObject;
        private static readonly Dictionary<long, int> SharedIndices = new();

        private static Dictionary<long, int> GetSharedReferenceIndices(SerializedObject serializedObject)
        {
            // Refreshing the counts first also resets this memo's frame when it rebuilds.
            var counts = GetReferenceIdCounts(serializedObject);

            var frame = Time.frameCount;
            if (_sharedIndicesFrame == frame && ReferenceEquals(_sharedIndicesObject, serializedObject))
                return SharedIndices;

            SharedIndices.Clear();
            var next = 1;
            foreach (var id in AliasOrder)
            {
                if (counts.TryGetValue(id, out var count) && count > 1)
                    SharedIndices[id] = next++;
            }

            _sharedIndicesFrame = frame;
            _sharedIndicesObject = serializedObject;
            return SharedIndices;
        }

        // The other fields aliasing this property's instance, in document order — what the notice lists and
        // navigates between.
        public static List<string> GetSharedReferenceAliasPaths(SerializedProperty property)
        {
            var result = new List<string>();
            if (property.managedReferenceValue is null) return result;

            if (!GetSharedReferencePathsById(property.serializedObject)
                    .TryGetValue(property.managedReferenceId, out var paths))
            {
                return result;
            }

            var selfPath = property.propertyPath;
            foreach (var path in paths)
            {
                if (path != selfPath)
                    result.Add(path);
            }

            return result;
        }

        // Both drawers consume this per-frame document order immediately; never retain the returned list.
        public static IReadOnlyList<string> GetSharedReferenceGroupPaths(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return Array.Empty<string>();

            return GetSharedReferencePathsById(property.serializedObject)
                .TryGetValue(property.managedReferenceId, out var paths)
                ? paths
                : (IReadOnlyList<string>)Array.Empty<string>();
        }

        private const int MaxDetailAliasPaths = 6;

        public static string BuildSharedReferenceDetail(SerializedProperty property)
        {
            var builder = new StringBuilder(
                "This reference is shared — editing it in one place changes every field that uses it.");

            var others = GetSharedReferenceAliasPaths(property);
            if (others.Count > 0)
            {
                builder.Append("\nAlso used by:");
                var shown = Mathf.Min(others.Count, MaxDetailAliasPaths);
                for (var i = 0; i < shown; i++)
                    builder.Append("\n• ").Append(GetPropertyDisplayPath(others[i]));

                if (others.Count > shown)
                    builder.Append("\n• …and ").Append(others.Count - shown).Append(" more");
            }

            builder.Append("\n\nClick the message to highlight the other fields; " +
                           "Make unique gives this field its own independent copy.");
            return builder.ToString();
        }

        private static readonly Dictionary<string, string> DisplayPathCache = new();

        // The inspector's own labels for a property path: "sidearms.Array.data[1].onHitEffect" reads as
        // "Sidearms > Element 1 > On Hit Effect".
        public static string GetPropertyDisplayPath(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) return string.Empty;
            if (DisplayPathCache.TryGetValue(propertyPath, out var cached)) return cached;

            var builder = new StringBuilder();

            var segments = SerializePropertyExtensions.SimplifyPropertyPath(propertyPath).Split('.');

            foreach (var segment in segments)
            {
                if (builder.Length > 0) builder.Append(" › ");

                var bracket = segment.IndexOf('[');
                builder.Append(ObjectNames.NicifyVariableName(bracket < 0 ? segment : segment[..bracket]));

                for (var open = bracket; open >= 0; open = segment.IndexOf('[', open + 1))
                {
                    var close = segment.IndexOf(']', open);
                    if (close < 0) break;
                    builder.Append(" › Element ").Append(segment, open + 1, close - open - 1);
                }
            }

            return DisplayPathCache[propertyPath] = builder.ToString();
        }

        private static int _sharedPathsFrame = -1;
        private static SerializedObject _sharedPathsObject;
        private static readonly Dictionary<long, List<string>> SharedPathsById = new();

        private static Dictionary<long, List<string>> GetSharedReferencePathsById(SerializedObject serializedObject)
        {
            // Refreshing the counts first also resets this memo's frame when it rebuilds.
            var counts = GetReferenceIdCounts(serializedObject);

            var frame = Time.frameCount;
            if (_sharedPathsFrame == frame && ReferenceEquals(_sharedPathsObject, serializedObject))
                return SharedPathsById;

            SharedPathsById.Clear();
            TraverseManagedReferences(serializedObject, other =>
            {
                var id = other.managedReferenceId;
                if (!counts.TryGetValue(id, out var count) || count <= 1) return false;

                if (!SharedPathsById.TryGetValue(id, out var paths)) SharedPathsById[id] = paths = new List<string>();
                paths.Add(other.propertyPath);
                return false;
            });

            _sharedPathsFrame = frame;
            _sharedPathsObject = serializedObject;
            return SharedPathsById;
        }

        // Call after a same-frame reassignment: the memo is keyed by frame, so a synchronous re-query would
        // otherwise return the pre-mutation snapshot and still report the just-broken alias as shared.
        public static void InvalidateSharedReferenceCache()
        {
            _aliasFrame = -1;
            _sharedIndicesFrame = -1;
            _sharedPathsFrame = -1;
        }

        // A same-frame repaint after an undo would read the pre-undo snapshot. Registered at domain load, before any
        // per-field handler subscribes, so it always runs first.
        [InitializeOnLoadMethod]
        private static void InvalidateAliasMemoOnUndoRedo() =>
            Undo.undoRedoPerformed += InvalidateSharedReferenceCache;

        public static void MakeReferenceUnique(SerializedProperty property)
        {
            var persistent = property.Persistent();
            var current = persistent.managedReferenceValue;
            if (current is null) return;

            persistent.SetManagedReferenceAndApply(CloneManagedReferenceGraph(current));

            InvalidateSharedReferenceCache();
        }

        // Report revisited IDs but do not enter their children, so cyclic reference graphs terminate.
        private static void TraverseManagedReferences(SerializedObject serializedObject, Func<SerializedProperty, bool> visit)
        {
            using var iterator = serializedObject.GetIterator();
            if (!iterator.Next(enterChildren: true)) return;

            var visited = new HashSet<long>();
            bool enterChildren;

            do
            {
                enterChildren = true;

                if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                {
                    if (visit(iterator)) return;

                    var rid = iterator.managedReferenceId;
                    if (rid >= 0 && !visited.Add(rid)) enterChildren = false;
                }
            }
            while (iterator.Next(enterChildren));
        }
        #endregion
    }
}
