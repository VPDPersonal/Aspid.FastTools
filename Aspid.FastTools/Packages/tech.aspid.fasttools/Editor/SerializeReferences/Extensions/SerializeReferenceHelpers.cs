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
    // Shared helpers for the [TypeSelector] drawer on [SerializeReference] fields: resolving the declared field
    // type, filtering candidates, instantiating the selected type and parsing Unity's managed-reference type-name
    // format. The open-generic argument flow itself lives in GenericTypeResolver and TypeSelectorWindow.
    internal static class SerializeReferenceHelpers
    {
        // The declared element type constraining the candidate list. managedReferenceFieldTypename already reports
        // the element type for array entries.
        public static Type GetFieldType(SerializedProperty property) =>
            GetTypeFromTypename(property.managedReferenceFieldTypename) ?? typeof(object);

        public static Type GetCurrentType(SerializedProperty property) =>
            property.managedReferenceValue?.GetType();

        // SerializedProperty.arrayElementType of a [SerializeReference] array — the only shape whose elements are
        // managed references.
        private const string ManagedReferenceElementPrefix = "managedReference<";

        public static bool IsManagedReferenceArray(SerializedProperty property) =>
            property is { isArray: true, propertyType: not SerializedPropertyType.String } &&
            property.arrayElementType.StartsWith(ManagedReferenceElementPrefix, StringComparison.Ordinal);

        // Constrains the add-picker on a list that may be empty; a non-empty list's elements resolve their own
        // field type. Read from the reflected field's shape, falling back to the first element, then to object.
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
        // Layers the user's excluded folders on top of the engine-level extension test, which is single-sourced so
        // every scanner covers the same set.
        public static bool IsScanCandidate(string path) =>
            SerializeReferenceYaml.IsCandidateAssetPath(path) && !SerializeReferenceSettings.IsExcluded(path);

        // Scenes cannot be read through LoadAllAssetsAtPath, so every object-loading scanner skips them and takes
        // the YAML pass instead.
        public static bool IsScene(string path) =>
            !string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

        // ManagedTypeName carries no value equality, so its three fields are joined into a key string instead.
        public static string StoredTypeKey(ManagedTypeName type) =>
            $"{type.Assembly}|{type.Namespace}|{type.Class}";

        // Like StoredTypeKey but without the closed-argument expansion, so a script's open definition and every
        // closed form YAML stores collapse to one key — that is how the delete guard and usage index match a
        // generic type's instantiations back to its script.
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

        // True when the selected targets do not all hold the same managed-reference type. Always false for a single
        // target. Drives the dropdown's mixed-value state and suppresses merging child fields of unlike types.
        public static bool HasMixedTypes(SerializedProperty property)
        {
            if (!property.serializedObject.isEditingMultipleObjects) return false;

            // hasMultipleDifferentValues misses the all-missing case, where every target reads back null but the
            // stored, unloadable type names still differ.
            if (property.hasMultipleDifferentValues) return true;

            // A non-null agreed value means the targets share the concrete type.
            if (property.managedReferenceValue is not null) return false;

            var first = property.managedReferenceFullTypename;
            var targets = property.serializedObject.targetObjects;
            if (targets.Length < 2) return false;

            // The probe allocates a SerializedObject per selected object on every repaint, while what it measures is
            // stable until the backing assets change.
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

        // Keyed per property path, so several empty fields under one multi-selection stay memoized across a repaint
        // instead of overwriting a single shared slot. Scoped to one selection snapshot and reset when it changes,
        // so it stays bounded by the fields the inspector actually draws.
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

        // Applies a change to every selected target independently, since one multi-object assignment would alias a
        // single instance across all of them. The factory receives that target's previous value, to support keeping
        // data, and must return a fresh instance or null. The batch collapses into one Undo step.
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

        // Whether the per-asset notices may be shown. They are file-level operations keyed to one backing asset, so
        // under a multi-object selection they would misreport or apply to a single target while presenting as if
        // they covered the selection — there the mixed/same-type hint takes their place.
        public static bool NoticesApply(SerializedProperty property) =>
            !property.serializedObject.isEditingMultipleObjects;
        #endregion

        // True when the reference's type can no longer be loaded. Unity exposes no such state per property — the
        // value reads back null and the typename is empty — so detection reads the stored reference from the asset
        // YAML: a null value whose recorded type cannot be resolved is missing.
        public static bool IsMissingType(SerializedProperty property) =>
            TryGetMissingType(property, out _, out _);

        // The probe runs several times per repaint and every legitimately empty field pays a full repair-location
        // resolution plus a YAML parse. Repairs land on later frames; same-frame mutations drop the memo explicitly.
        private static int _missingProbeFrame = -1;
        private static readonly Dictionary<(int instanceId, string path), (bool missing, long referenceId, ManagedTypeName storedType)>
            _missingProbeMemo = new();

        // For mutations that must be visible to a read later in the SAME frame.
        public static void InvalidateMissingTypeMemo() => _missingProbeFrame = -1;

        // Reads the property's stored id and type from the asset YAML; missing when the type no longer resolves.
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

        // True when the YAML-recorded type identity can be loaded — i.e. the reference is intact, not missing.
        public static bool StoredTypeResolves(ManagedTypeName name)
        {
            if (string.IsNullOrEmpty(name.Class)) return false;

            var className = name.Class.Replace('/', '+');
            var fullName = string.IsNullOrEmpty(name.Namespace) ? className : $"{name.Namespace}.{className}";
            var assemblyQualified = string.IsNullOrEmpty(name.Assembly) ? fullName : $"{fullName}, {name.Assembly}";

            return Type.GetType(assemblyQualified, throwOnError: false) is not null;
        }

        // Types that can legally be assigned to a [SerializeReference] field: concrete reference types that are
        // neither UnityEngine.Object, open generics, strings nor delegates. [Serializable] is deliberately NOT
        // required — a managed reference is serialized through the asset's references registry, which records the
        // concrete type and its data with no attribute involved. A generic argument lands in an ordinary field and
        // does need it; that rule lives in IsValidGenericArgument.
        public static bool IsAssignableManagedReference(Type type) =>
            type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
            type != typeof(string) &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type);

        // The picker's candidate predicate: the structural check, narrowed to baseTypes when they say anything —
        // an empty set, or one naming only object, adds no narrowing.
        public static Func<Type, bool> BuildAssignableFilter(Type[] baseTypes)
        {
            var narrowing = FilterNarrowingTypes(baseTypes);
            if (narrowing is null) return IsAssignableManagedReference;

            return type => IsAssignableManagedReference(type) &&
                           Array.Exists(narrowing, baseType => baseType.IsAssignableFrom(type));
        }

        // Null when nothing meaningfully narrows the set, so the caller can skip allocating a predicate closure.
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

        // Prefers a parameterless constructor so field initializers run, falling back to an uninitialized instance.
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

        // Carries over every field the two types share by name and shape, mirroring Unity's own type-change
        // behavior: the old value is serialized to JSON and overwritten onto the new instance.
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

        // Assigns every shared [SerializeReference] field whose value fits the target's declared type, arrays
        // included.
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

        // Deep-copies a managed reference: value fields ride the same JSON round-trip, and every nested
        // [SerializeReference] is replaced with its own copy. Topology is preserved — two fields aliasing one nested
        // instance alias one copy, and a cyclic graph terminates because each copy registers before its children.
        // This is the Make-unique copier; the type-switch flows keep CreateInstancePreservingData, where reusing the
        // nested instances is correct.
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

        // A collection slot is rebuilt rather than shared with the source, with each element cloned.
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

        // Instance fields, public or [SerializeField], declared with [SerializeReference], base chain included.
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

        // Parses Unity's "AssemblyName Namespace.TypeName" format; null when empty or unloadable.
        public static Type GetTypeFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename)) return null;

            var separator = typename.IndexOf(' ');
            if (separator < 0) return Type.GetType(typename, throwOnError: false);

            var assembly = typename[..separator];
            var fullName = typename[(separator + 1)..];
            return Type.GetType($"{fullName}, {assembly}", throwOnError: false);
        }

        // The types the engine serializes natively as a field value. They have to be named one by one because
        // Type.IsSerializable answers false for all of them: the engine writes their layout itself, so none carries
        // [Serializable]. This is the half of "Unity can serialize this" IsSerializable cannot see.
        //
        // Membership was measured on Unity 6000.4, not assumed. Value types of the same family the engine does NOT
        // serialize — Ray, Ray2D, Plane, RangeInt, Keyframe, GradientColorKey — are absent for that reason and must
        // stay absent. Built-ins that do carry the attribute already pass the ordinary check.
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

        // Types the argument PAGE offers: concrete, non-generic types Unity can serialize as a field value.
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

        // Whether an argument the FIELD already determines may close a parameter. IsValidGenericArgument demands
        // serializability because its page has to stay a list a human can read; that is the wrong bar here, where
        // nobody is browsing and whether the argument must be serializable at all depends on where the parameter
        // lands — the question GenericArgumentRequirement answers. The structural half is not a matter of taste:
        // MakeGenericType itself refuses an open definition, a pointer, a by-ref and void.
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

        // The best Smart Fix candidate for this property's missing reference, never applied automatically. The pool
        // is constrained to what the picker would offer, so a suggestion can never violate the field's constraint.
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

        // Shared by both notices and the quick-apply button so the copy never drifts. The separator before it is
        // decoration each notice renders itself, so it is not part of the label.
        public static string GetSuggestionLabel(SerializeReferenceRepairSuggestions.RepairCandidate suggestion) =>
            $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion.Type)}";

        // Shared by both notices so the two never drift.
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

        // The YAML document backing the stored reference, plus whether the repair must be applied in memory. A saved
        // asset is repaired by rewriting its file. A Prefab Mode object has no path of its own — it comes from the
        // stage, and the document id is matched back to the asset — and must be repaired in memory, since the open
        // stage holds a separate copy that would overwrite a file rewrite on save.
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

        // GlobalObjectId.targetObjectId is the scene-local file id matching the YAML document anchor. Bails for a
        // dirty scene, whose YAML would not match the live object, and for prefab-instance overrides, whose data
        // lives in the source prefab.
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
                if (transform.parent is null) return false; // object is not under the stage root
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

        // Strict and per-property: only a field whose own recorded type fails to resolve counts as missing, so a
        // legitimately empty field is never flagged.
        public static bool TryGetMissingReferenceId(SerializedProperty property, out long referenceId) =>
            TryGetMissingType(property, out referenceId, out _);

        // Opens the dropdown's own picker to choose the type a missing reference should resolve to, narrowed the
        // same way so a repair cannot pick a type the attribute excludes. Unlike the authoring dropdown it does
        // offer hidden types: hiding governs what may be authored, not what a broken reference may become.
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

        // Re-points a missing reference at newType, keeping its stored data: a saved asset by rewriting the YAML and
        // reimporting, a Prefab Mode object in memory.
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

        // Unity's object-level missing-types banner is drawn from a flag cached when the editor is built and only
        // clears on a genuine reselection, so the objects are deselected and reselected across the next ticks.
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

            // Mark the owning scene dirty so the in-memory repair is offered for save.
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

        // The live MonoBehaviours of an asset that is unsafe to rewrite, matched by missing-reference identity
        // rather than file id, since the open stage remaps ids. Only MonoBehaviours are probed, because
        // GetManagedReferencesWithMissingTypes errors on other types.
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

        // Clears the fixed entry and everything it transitively referenced, which would otherwise linger as orphans
        // and keep Unity's missing-types banner raised. A member referenced from OUTSIDE the subtree is kept, along
        // with everything only reachable through it, since clearing it would leave that pointer unrepairable.
        private static void ClearMissingSubtree(Object target, long rootReferenceId)
        {
            var dataByRid = new Dictionary<long, string>();
            foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                dataByRid[entry.referenceId] = entry.serializedData;

            // The transitive closure of the fixed entry — the candidates for clearing.
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

        // The rid pointers inside a missing entry's payload. The look-behind keeps a field that merely ends in "rid"
        // from reading as one.
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
                    // Only top-level scalars: skip blanks, indented (nested) lines and sequence items.
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

        // Unity single-quotes scalars containing reserved characters, doubling embedded quotes.
        private static string UnquoteYaml(string value) =>
            value.Length >= 2 && value[0] == '\'' && value[^1] == '\''
                ? value[1..^1].Replace("''", "'")
                : value;

        private static string Quote(string value) =>
            $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        #endregion

        #region Constraint map
        // Maps every managed reference in the asset to the declared field type holding it, keyed by document file id
        // and rid. A missing reference reads back null, but its field still reports the declared element type and
        // the orphaned rid survives in the YAML, so the two together recover the constraint the picker should honor.
        // References under a missing parent are unreachable here and fall back to an unconstrained picker, as do
        // orphaned rids no field points at.
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
        // True when another field in the same object shares this property's rid, so edits to one bleed into the
        // other. Happens after duplicating an array element or pasting.
        public static bool HasSharedReference(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return false;

            var id = property.managedReferenceId;

            // Built once per object per frame: GetHeight and Draw each ask this for every field, so a per-property
            // full-object walk would be 2*N walks per repaint.
            return GetReferenceIdCounts(property.serializedObject).TryGetValue(id, out var count) && count > 1;
        }

        // The 1-based badge number of this property's shared group, or 0 when it is not shared. Numbering follows
        // each rid's first appearance in document order and is shared by both notices, so two fields aliasing one
        // instance always read the same number in either inspector mode.
        public static int GetSharedReferenceIndex(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return 0;

            var id = property.managedReferenceId;
            return GetSharedReferenceIndices(property.serializedObject).TryGetValue(id, out var index) ? index : 0;
        }

        // How many fields carry each id, built by one full-object walk and shared across a repaint.
        private static int _aliasFrame = -1;
        private static SerializedObject _aliasSerializedObject;
        private static readonly Dictionary<long, int> AliasCounts = new();

        // Each id's first-sighting order, so badge numbers follow document order rather than the dictionary's.
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

                if (!AliasCounts.TryGetValue(id, out var count)) AliasOrder.Add(id); // first sighting → record its order
                AliasCounts[id] = count + 1;
                return false;
            });

            // The counts were rebuilt, so the maps derived from them are stale.
            _sharedIndicesFrame = -1;
            _sharedPathsFrame = -1;
            _aliasFrame = frame;
            _aliasSerializedObject = serializedObject;
            return AliasCounts;
        }

        // Each shared id's badge number. Separate from the counts memo, so it is built only when a notice asks.
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

        // The whole group in document order, this property included. Both drawers cycle through this canonical
        // order, so they walk the members the same way. It is a per-frame memo: read it immediately, never cache it.
        public static IReadOnlyList<string> GetSharedReferenceGroupPaths(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return Array.Empty<string>();

            return GetSharedReferencePathsById(property.serializedObject)
                .TryGetValue(property.managedReferenceId, out var paths)
                ? paths
                : (IReadOnlyList<string>)Array.Empty<string>();
        }

        // How many alias paths the tooltip lists before folding the rest into "…and N more".
        private const int MaxDetailAliasPaths = 6;

        // Built here so both notices always tell the same story.
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

        // The same paths recur on every repaint, so the nicified form is built once.
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

        // Each shared id's member paths in document order, built only when a notice needs them.
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

        // Breaks an alias by replacing the reference with an independent clone carrying the same data; a fresh
        // instance gets a new rid on assignment.
        public static void MakeReferenceUnique(SerializedProperty property)
        {
            var persistent = property.Persistent();
            var current = persistent.managedReferenceValue;
            if (current is null) return;

            // Make unique promises independence all the way down, which a shallow clone would not give.
            persistent.SetManagedReferenceAndApply(CloneManagedReferenceGraph(current));

            // A repaint in this same frame would otherwise keep painting the notice on both ex-members.
            InvalidateSharedReferenceCache();
        }

        // Visits every managed-reference property, nested values included, stopping when the visitor returns true.
        // A revisited rid is still reported, but its children are not re-entered, or a cyclic graph would loop.
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
