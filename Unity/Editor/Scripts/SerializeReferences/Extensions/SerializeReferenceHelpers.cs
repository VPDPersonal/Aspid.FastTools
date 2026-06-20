using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using UnityEditor.SceneManagement;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared helpers for the <c>[TypeSelector]</c> drawer on <c>[SerializeReference]</c> fields: resolving the
    /// declared managed-reference field type, filtering candidate types, instantiating the selected type, and
    /// parsing Unity's managed-reference type-name format. The open-generic argument flow itself lives in
    /// the shared <see cref="Aspid.FastTools.Types.Editors.GenericTypeResolver"/> /
    /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorWindow"/>; <see cref="IsValidGenericArgument"/>
    /// is supplied to the selector as its argument filter.
    /// </summary>
    internal static class SerializeReferenceHelpers
    {
        /// <summary>
        /// Resolves the declared element type of a managed-reference property — the base type that
        /// constrains the candidate list. Uses <see cref="SerializedProperty.managedReferenceFieldTypename"/>,
        /// which already reports the element type for array/list entries.
        /// </summary>
        public static Type GetFieldType(SerializedProperty property) =>
            GetTypeFromTypename(property.managedReferenceFieldTypename) ?? typeof(object);

        /// <summary>
        /// Resolves the concrete type currently stored in the managed reference, or <see langword="null"/>
        /// when the reference is empty or its stored type can no longer be loaded.
        /// </summary>
        public static Type GetCurrentType(SerializedProperty property) =>
            property.managedReferenceValue?.GetType();

        #region Project scan helpers
        // File kinds that can carry SerializeReference managed-reference documents. Single-sourced here so the Repair
        // window, the usage index, the breakage detector and the build/CI gate all scan the same candidate set.
        internal static readonly string[] ScanExtensions = { ".prefab", ".asset", ".unity" };

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="path"/> is a project asset (under <c>Assets/</c>) whose
        /// extension can host managed references. Promoted from the Repair window so every project-wide scanner shares
        /// one definition.
        /// </summary>
        internal static bool IsScanCandidate(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal)) return false;
            if (SerializeReferenceSettings.IsExcluded(path)) return false;

            foreach (var extension in ScanExtensions)
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="path"/> is a Unity scene. Scenes cannot be read through
        /// <see cref="AssetDatabase.LoadAllAssetsAtPath"/> — it warns "Do not use ReadObjectThreaded on scene objects!"
        /// and returns nothing useful — so every object-loading scanner skips them and relies on the YAML pass instead.
        /// </summary>
        internal static bool IsScene(string path) =>
            !string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Stable grouping key for a stored type identity (class + namespace + assembly). <see cref="ManagedTypeName"/>
        /// carries no value equality, so the three fields are joined into a key string instead.
        /// </summary>
        internal static string StoredTypeKey(ManagedTypeName type) =>
            $"{type.Assembly}|{type.Namespace}|{type.Class}";
        #endregion

        #region Multi-object editing
        /// <summary>
        /// Returns <see langword="true"/> when this property belongs to a <see cref="SerializedObject"/> editing more
        /// than one target object at once (a multi-selection in the inspector).
        /// </summary>
        public static bool IsEditingMultipleObjects(SerializedProperty property) =>
            property.serializedObject.isEditingMultipleObjects;

        /// <summary>
        /// Returns <see langword="true"/> when the selected targets do not all hold the same managed-reference type —
        /// either Unity reports <see cref="SerializedProperty.hasMultipleDifferentValues"/>, or the per-target
        /// <see cref="SerializedProperty.managedReferenceFullTypename"/> differs across
        /// <see cref="SerializedObject.targetObjects"/>. Always <see langword="false"/> for a single target, so the
        /// single-object paths are untouched. Used to drive the dropdown's mixed-value state and to suppress merging
        /// child field UIs of incompatible types.
        /// </summary>
        public static bool HasMixedTypes(SerializedProperty property)
        {
            if (!property.serializedObject.isEditingMultipleObjects) return false;

            // hasMultipleDifferentValues catches differing concrete instances; the explicit type-name comparison also
            // catches the all-missing case, where every target reads back a null value but the stored (unloadable)
            // type names still differ — hasMultipleDifferentValues does not always flag that on its own.
            if (property.hasMultipleDifferentValues) return true;

            // A non-null, agreed value means all targets share the concrete type — skip the per-target probe (and its
            // SerializedObject allocations). The per-target comparison only matters for the all-missing case (null value).
            if (property.managedReferenceValue is not null) return false;

            var first = property.managedReferenceFullTypename;
            var targets = property.serializedObject.targetObjects;
            if (targets.Length < 2) return false;

            foreach (var target in targets)
            {
                if (target == null) continue;

                using var single = new SerializedObject(target);
                var other = single.FindProperty(property.propertyPath);
                if (other is null) continue;
                if (other.managedReferenceFullTypename != first) return true;
            }

            return false;
        }

        /// <summary>
        /// Applies a managed-reference change to <b>every</b> selected target independently, so each object receives its
        /// own instance rather than the shared reference that a single multi-object
        /// <see cref="SerializedProperty.managedReferenceValue"/> assignment would alias across all of them. For each
        /// target, <paramref name="factory"/> is invoked with that target's <i>previous</i> value (to support Keep-Data
        /// through <see cref="CreateInstancePreservingData"/>) and must return a fresh, independent instance (or
        /// <see langword="null"/> to clear). The whole batch is collapsed into a single Undo step so one Ctrl+Z reverts
        /// all targets together. The originating <paramref name="property"/>'s <see cref="SerializedObject"/> is then
        /// refreshed so the live inspector re-reads the new state.
        /// </summary>
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

                // Each target gets its own instance: the factory must not return the same object for two targets,
                // otherwise the managed reference would be aliased across objects again.
                singleProperty.managedReferenceValue = instance;
                singleProperty.isExpanded = instance is not null;
                single.ApplyModifiedProperties();
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Pull the per-target writes back into the inspector's live SerializedObject so the drawer re-reads the new
            // state without waiting for the next external change notification. Update() re-reads from the targets and
            // discards any unapplied edits the live SO still holds — applying it instead would write the live SO's stale
            // (pre-change) managed reference back over the per-target writes. This drawer applies its own change through
            // the per-target path immediately, so the live SO carries no competing pending edits to lose here.
            serializedObject.Update();
        }

        /// <summary>
        /// Whether the per-asset missing/shared notices may be shown for this property. They are file-level operations
        /// keyed to a single backing asset (YAML rewrite, single-object cross-reference scan), so under a multi-object
        /// selection they would either misreport (the probes read only the first target) or apply to a single target
        /// while presenting as if they covered the selection. The conservative rule is therefore to surface a notice
        /// only for a single target; with several targets selected the notices are suppressed and the mixed/same-type
        /// hint takes their place. Returns <see langword="true"/> for the single-target case (notices allowed).
        /// </summary>
        public static bool NoticesApply(SerializedProperty property) =>
            !property.serializedObject.isEditingMultipleObjects;
        #endregion

        /// <summary>
        /// Returns <see langword="true"/> when this property holds a managed reference whose type can no longer be
        /// loaded (renamed / moved / deleted). Unity does not expose a missing type through the per-property API
        /// (the value reads back <see langword="null"/> and <see cref="SerializedProperty.managedReferenceFullTypename"/>
        /// is empty) and even drops it from the live object on prefabs / GameObjects, so detection reads the stored
        /// reference straight from the asset YAML: a null value whose recorded type cannot be resolved is missing.
        /// </summary>
        public static bool IsMissingType(SerializedProperty property) =>
            TryGetMissingType(property, out _, out _);

        // Core missing-type probe shared by the public helpers: reads the property's stored id and type from the
        // asset YAML and reports it missing when the recorded type no longer resolves to a loadable Type.
        private static bool TryGetMissingType(SerializedProperty property, out long referenceId, out ManagedTypeName storedType)
        {
            referenceId = 0;
            storedType = default;

            if (property.propertyType != SerializedPropertyType.ManagedReference) return false;
            if (property.managedReferenceValue is not null) return false;
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

        /// <summary>
        /// Predicate identifying types that can legally be assigned to a <c>[SerializeReference]</c> field:
        /// concrete reference types that are neither <see cref="Object"/>, open generics, strings, nor delegates.
        /// </summary>
        public static bool IsAssignableManagedReference(Type type) =>
            type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
            type != typeof(string) &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type);

        /// <summary>
        /// Builds the candidate predicate for the type picker: the structural <see cref="IsAssignableManagedReference"/>
        /// check, optionally narrowed so only types assignable to one of <paramref name="baseTypes"/> qualify. A null or
        /// empty set — or one that only names <see cref="object"/> (the unconstrained <c>[TypeSelector]</c> default) —
        /// applies no extra narrowing, leaving every concrete type assignable to the field's declared type as a candidate.
        /// </summary>
        public static Func<Type, bool> BuildAssignableFilter(Type[] baseTypes)
        {
            var narrowing = FilterNarrowingTypes(baseTypes);
            if (narrowing is null) return IsAssignableManagedReference;

            return type => IsAssignableManagedReference(type) &&
                           Array.Exists(narrowing, baseType => baseType.IsAssignableFrom(type));
        }

        // Drops nulls and the unconstrained `object` sentinel; returns null when nothing meaningful narrows the set,
        // so the caller can fall back to the plain structural filter without allocating a predicate closure.
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

        /// <summary>
        /// Creates an instance of <paramref name="type"/> for assignment to a managed reference.
        /// Prefers a (public or non-public) parameterless constructor so field initializers run, and
        /// falls back to an uninitialized instance for types that expose no parameterless constructor.
        /// </summary>
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

        /// <summary>
        /// Creates an instance of <paramref name="newType"/> and carries over the data of <paramref name="previous"/>
        /// for every field the two types share by name and serialized shape. Mirrors Unity's own type-change
        /// behaviour: the old value is serialized to JSON and overwritten onto the new instance, so matching fields
        /// survive a type switch (e.g. a shared <c>_radius</c>) while the rest fall back to the new type's defaults.
        /// A structural mismatch simply leaves the new instance untouched.
        /// </summary>
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

            return instance;
        }

        /// <summary>
        /// Parses Unity's managed-reference type-name format (<c>"AssemblyName Namespace.TypeName"</c>)
        /// into a <see cref="Type"/>, or <see langword="null"/> when it is empty or cannot be loaded.
        /// </summary>
        public static Type GetTypeFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename)) return null;

            var separator = typename.IndexOf(' ');
            if (separator < 0) return Type.GetType(typename, throwOnError: false);

            var assembly = typename[..separator];
            var fullName = typename[(separator + 1)..];
            return Type.GetType($"{fullName}, {assembly}", throwOnError: false);
        }

        /// <summary>
        /// Predicate identifying types usable as a generic argument of a serialized managed reference:
        /// concrete, non-generic types Unity can serialize as a field value (primitives, <see cref="string"/>,
        /// enums, <see cref="Object"/>-derived references, or <c>[Serializable]</c> structs/classes). Passed to
        /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorWindow.Show"/> as the argument filter.
        /// </summary>
        public static bool IsValidGenericArgument(Type type)
        {
            if (type is null) return false;
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters) return false;
            if (typeof(Delegate).IsAssignableFrom(type)) return false;

            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   typeof(Object).IsAssignableFrom(type) ||
                   (type.IsValueType && type.IsSerializable) ||
                   (type.IsClass && type.IsSerializable);
        }

        #region Missing-type repair
        /// <summary>
        /// Resolves the stored (now unloadable) type identity of this property's missing managed reference, read from
        /// the asset YAML, for display in the caption / warning. Returns <see langword="default"/> when the property
        /// is not a recognised missing reference.
        /// </summary>
        public static ManagedTypeName GetMissingTypeName(SerializedProperty property) =>
            TryGetMissingType(property, out _, out var storedType) ? storedType : default;

        /// <summary>
        /// Human-readable <c>Namespace.Class</c> of this property's missing type, for the dropdown caption and the
        /// warning message, or an empty string when the property is not a recognised missing reference.
        /// </summary>
        public static string GetMissingTypeDisplayName(SerializedProperty property)
        {
            var name = GetMissingTypeName(property);
            if (name.IsEmpty) return string.Empty;
            return string.IsNullOrEmpty(name.Namespace) ? name.Class : $"{name.Namespace}.{name.Class}";
        }

        /// <summary>
        /// Computes the best <b>Smart Fix</b> repair suggestion for this property's missing managed reference: the
        /// highest-scoring existing type the renamed/moved reference most likely became, ranked by
        /// <see cref="SerializeReferenceRepairSuggestions"/>. The suggestion is never applied automatically — the caller
        /// surfaces it as a one-click action. The candidate pool is constrained to types the picker itself would offer
        /// (assignable to the field's declared type, narrowed by <paramref name="baseTypes"/>), so a suggestion can
        /// never violate the field's constraint. Returns <see langword="false"/> when the property is not a recognised
        /// missing reference, the repair location is unavailable, or no candidate clears the confidence threshold.
        /// </summary>
        public static bool TryGetRepairSuggestion(SerializedProperty property, Type[] baseTypes,
            out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            if (!TryGetMissingType(property, out var referenceId, out var storedType)) return false;
            if (!TryGetRepairLocation(property, out var assetPath, out var fileId, out var inMemory)) return false;

            var fieldType = GetFieldType(property);
            // The same predicate the picker applies, so a surfaced suggestion is guaranteed to be a type the user could
            // have picked manually — never one the [TypeSelector] base-type narrowing would have hidden.
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

        /// <summary>
        /// The trailing notice label for a Smart Fix <paramref name="suggestion"/> — the short type name with the
        /// "<c>·  → Name?</c>" affordance. Shared by the UIToolkit and IMGUI notices so the two never drift.
        /// </summary>
        public static string GetSuggestionLabel(SerializeReferenceRepairSuggestions.RepairCandidate suggestion) =>
            $"·  → {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion.Type)}?";

        /// <summary>
        /// The hover-tooltip detail for a Smart Fix <paramref name="suggestion"/> — the full type identity and the
        /// ranking reason. Shared by the UIToolkit and IMGUI notices so the two never drift.
        /// </summary>
        public static string GetSuggestionDetail(SerializeReferenceRepairSuggestions.RepairCandidate suggestion) =>
            $"Suggested: {suggestion.Type.FullName}, {suggestion.Type.Assembly.GetName().Name}.\n" +
            $"Reason: {suggestion.Reason}.\nClick to re-point this reference to it, keeping its data.";

        // Top-level serialized field names of the missing reference's orphaned payload, for the field-shape heuristic.
        // Saved assets read them straight from the rid's YAML data block; a Prefab Mode object has no committed block
        // for the live copy, so the flat payload Unity still exposes for the missing reference is parsed instead.
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

        /// <summary>
        /// Resolves the on-disk asset path and the target object's local file id (the YAML document anchor) backing
        /// this property. Returns <see langword="false"/> for scene objects and prefab instances, which have no
        /// editable asset file — the YAML repair flow only applies to saved assets (ScriptableObjects, prefabs).
        /// </summary>
        public static bool TryGetAssetLocation(SerializedProperty property, out string assetPath, out long fileId)
        {
            fileId = 0;
            var target = property.serializedObject.targetObject;
            assetPath = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(assetPath)) return false;
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out _, out fileId);
        }

        /// <summary>
        /// Resolves the YAML document backing this property's stored managed reference and reports whether the repair
        /// must be applied in memory. Saved assets (ScriptableObjects, prefab assets selected in the Project) resolve
        /// directly and are repaired by rewriting the file. Objects open in <b>Prefab Mode</b> have no asset path of
        /// their own — the path comes from the prefab stage and the document id is matched back to the asset on disk —
        /// and must be repaired in memory (<paramref name="inMemory"/> = <see langword="true"/>), because the open
        /// stage holds a separate copy that does not refresh on reimport and would overwrite a file rewrite on save.
        /// </summary>
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

            // A plain object in a saved scene: its scene file is the YAML document store and its scene-local file id is
            // the document anchor. Repaired in memory (a loaded scene must not be rewritten on disk under it).
            if (TryGetSceneLocation(target, go, out assetPath, out fileId))
            {
                inMemory = true;
                return true;
            }

            return false;
        }

        // Resolves a saved-scene object's (scene path, document file id). GlobalObjectId.targetObjectId is the scene's
        // local file identifier, which matches the YAML "--- !u!114 &<fileID>" anchor. Bails for unsaved/dirty scenes
        // (the on-disk YAML would not match the live object) and for prefab-instance overrides (their data lives in the
        // source prefab, not this scene — see jump-to-source-prefab).
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

        /// <summary>
        /// Resolves the source prefab asset path for a nested prefab instance's <paramref name="target"/>, whose managed
        /// reference data lives in that source prefab rather than the host. Returns <see langword="false"/> for plain
        /// scene objects and saved assets.
        /// </summary>
        public static bool TryGetSourcePrefabPath(Object target, out string sourcePath)
        {
            sourcePath = null;
            if (target == null) return false;

            var go = target as GameObject ?? (target as Component)?.gameObject;
            if (go is null || !PrefabUtility.IsPartOfPrefabInstance(go)) return false;

            sourcePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            return !string.IsNullOrEmpty(sourcePath);
        }

        // A Prefab Mode object is a copy in a preview scene and carries no file id of its own, so the matching
        // persisted object is located in the asset by replaying its child path from the stage root, and the document
        // id is read from the asset's component (or GameObject) there.
        private static bool TryMatchAssetFileId(PrefabStage stage, Object target, GameObject stageGo, out long fileId)
        {
            fileId = 0;

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

        /// <summary>
        /// Finds the <c>RefIds</c> id of the missing managed reference this property points at, read from the asset
        /// YAML. Detection is strict and per-property: only a field whose own recorded type fails to resolve counts
        /// as missing, so legitimately-empty fields are never flagged.
        /// </summary>
        public static bool TryGetMissingReferenceId(SerializedProperty property, out long referenceId) =>
            TryGetMissingType(property, out referenceId, out _);

        /// <summary>
        /// Opens the same hierarchical type picker the dropdown uses, anchored at <paramref name="screenRect"/>, to
        /// choose the existing type a missing reference should resolve to. <paramref name="baseTypes"/> narrows the
        /// candidates the same way the live dropdown does, so a repair cannot pick a type the attribute excludes. The
        /// chosen type is written into the asset YAML (re-pointing the reference and keeping its stored data);
        /// <paramref name="onFixed"/> runs on success.
        /// </summary>
        public static void ShowFixTypeSelector(SerializedProperty property, Rect screenRect, Action onFixed, Type[] baseTypes = null)
        {
            var fieldType = GetFieldType(property);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { fieldType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName =>
                {
                    var type = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    if (type is not null && TryFixMissingType(property, type))
                        onFixed?.Invoke();
                },
                filter: BuildAssignableFilter(baseTypes),
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(fieldType, baseTypes),
                argumentFilter: IsValidGenericArgument);
        }

        /// <summary>
        /// Re-points this property's missing managed reference to <paramref name="newType"/>, keeping its stored data.
        /// Saved assets are repaired by rewriting the type in the YAML and reimporting; objects open in Prefab Mode are
        /// repaired in memory (see <see cref="TryGetRepairLocation"/>). Returns <see langword="true"/> on success; the
        /// caller refreshes the inspector.
        /// </summary>
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
                // ForceUpdate reloads the asset and invalidates the live SerializedObject, so the property must not be
                // touched afterwards — the inspector is rebuilt below from a fresh selection instead.
                if (repaired) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            // A repair reimports the asset / rewrites the live object: the candidate set changes and the missing
            // reference is gone, so any cached ranking and any cached YAML lines for it are now stale.
            if (repaired)
            {
                SerializeReferenceRepairSuggestions.ClearCache();
                SerializeReferenceYamlProbeCache.ClearCache();
            }

            if (repaired) ScheduleInspectorRebuild();
            return repaired;
        }

        // Forces the inspector to rebuild after a repair. Unity's object-level "contains missing SerializeReference
        // types" banner is drawn from a flag cached when the editor is built and does not react to
        // ClearManagedReferenceWithMissingType or an inspector reload — it only clears on a genuine reselection.
        // A reimport (saved-asset path) likewise leaves the live SerializedObject stale. Deselecting and reselecting
        // the current objects on the next ticks tears the editors down and recreates them from scratch — exactly what
        // a manual reselect does — so the banner clears and the resolved field shows through.
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

        // Prefab Mode objects cannot be repaired by rewriting the asset file: the open stage holds its own copy that
        // does not refresh on reimport and overwrites the change on save. Instead the reference is reassigned on the
        // live object — recovering the orphaned field data Unity still keeps for the missing type — and the now-unused
        // missing-type entry is cleared so the object stops being flagged.
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

            // Mark the owning scene (the prefab stage's preview scene, or a regular scene) dirty so the in-memory
            // repair is offered for save — a file rewrite that the open stage would otherwise discard is avoided.
            var scene = (target as Component)?.gameObject.scene ?? (target as GameObject)?.scene ?? default;
            if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

            return true;
        }

        /// <summary>
        /// Clears a missing managed reference (id <paramref name="rid"/>, stored type <paramref name="storedType"/>) to
        /// <see langword="null"/> on the live object of an asset open in Prefab Mode or a loaded scene — the in-memory
        /// counterpart of the YAML clear. The Project References uses it when a direct file rewrite would be clobbered by the
        /// open copy on save. Marks the owning scene dirty so the change is offered for save (the on-disk file, and so
        /// the audit listing, only updates once saved). Returns whether a matching live entry was found and cleared.
        /// </summary>
        public static bool TryClearMissingReferenceInMemory(string assetPath, long rid, ManagedTypeName storedType)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;

            foreach (var target in EnumerateOpenMissingTypeTargets(assetPath))
            {
                var matched = false;
                foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                {
                    if (entry.referenceId != rid) continue;
                    // Guard against a colliding rid on another live object: also match the stored class when it is known.
                    if (!string.IsNullOrEmpty(storedType.Class) && entry.className != storedType.Class) continue;
                    matched = true;
                    break;
                }

                if (!matched) continue;

                ClearMissingSubtree(target, rid);
                EditorUtility.SetDirty(target);

                var scene = (target as Component)?.gameObject.scene ?? default;
                if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

                return true;
            }

            return false;
        }

        // The live MonoBehaviours of an asset that is open and therefore unsafe to rewrite on disk: the components of a
        // prefab open in Prefab Mode, or of a loaded scene at that path. Managed-reference missing types live on these,
        // so the in-memory clear matches by missing-reference identity rather than by file id (which the open stage
        // remaps). Only MonoBehaviours are probed — GetManagedReferencesWithMissingTypes errors on unsupported types.
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

        // Clears the fixed missing-type entry and any missing-type entries it transitively referenced. The in-memory
        // repair replaces the reference with a fresh instance, dropping the orphaned payload's nested references — so a
        // missing child it carried (e.g. a missing effect nested inside a missing weapon) would otherwise linger as an
        // unreachable orphan and keep Unity's object-level missing-types flag (and its banner) raised.
        private static void ClearMissingSubtree(Object target, long rootReferenceId)
        {
            var dataByRid = new Dictionary<long, string>();
            foreach (var entry in SerializationUtility.GetManagedReferencesWithMissingTypes(target))
                dataByRid[entry.referenceId] = entry.serializedData;

            var pending = new Stack<long>();
            var visited = new HashSet<long>();
            pending.Push(rootReferenceId);

            while (pending.Count > 0)
            {
                var rid = pending.Pop();
                if (!visited.Add(rid)) continue;
                if (!dataByRid.TryGetValue(rid, out var data)) continue; // a resolvable reference, or already cleared

                foreach (Match match in Regex.Matches(data ?? string.Empty, @"rid:\s*(-?\d+)"))
                    if (long.TryParse(match.Groups[1].Value, out var child) && child != rid)
                        pending.Push(child);

                SerializationUtility.ClearManagedReferenceWithMissingType(target, rid);
            }
        }

        // Best-effort recovery of a missing reference's stored data onto the replacement instance. Unity surfaces the
        // orphaned payload as the field block of YAML scalars (e.g. "_damage: 15"); the flat top-level scalars are
        // mapped to JSON and overwritten onto the instance, so a renamed-type fix keeps its values. Nested mappings
        // and sequences are skipped and left at the new type's defaults.
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

                    // Empty value = a mapping/array header (e.g. "_nested:"); complex flow values are not flat scalars.
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

        // Unity single-quotes YAML scalars that contain reserved characters, doubling embedded quotes.
        private static string UnquoteYaml(string value) =>
            value.Length >= 2 && value[0] == '\'' && value[^1] == '\''
                ? value[1..^1].Replace("''", "'")
                : value;

        private static string Quote(string value) =>
            $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        #endregion

        #region Constraint map
        /// <summary>
        /// Maps every managed reference in <paramref name="assetPath"/> to the declared field type that holds it, keyed
        /// by the owning object document (its local file id) and the reference's <c>RefIds</c> id. A missing reference
        /// reads back <see langword="null"/> through the serialization API, but its field still reports the declared
        /// element type via <see cref="SerializedProperty.managedReferenceFieldTypename"/>, and the orphaned rid survives
        /// in the YAML — so the two together recover the constraint the picker should honour. References nested inside a
        /// missing parent are unreachable here (the parent is null) and simply fall back to an unconstrained picker, as do
        /// orphaned rids no field points at.
        /// </summary>
        /// <remarks>
        /// Shared by the asset-level Repair window (per-entry and project-wide group constraints) and the Managed
        /// References graph window, so a single declared-type recovery backs every embedded picker.
        /// </remarks>
        public static Dictionary<(long fileId, long rid), Type> BuildConstraintMap(string assetPath)
        {
            var map = new Dictionary<(long, long), Type>();
            if (string.IsNullOrEmpty(assetPath)) return map;

            // Scenes cannot be read through LoadAllAssetsAtPath (see IsScene); an unconstrained picker is the fallback.
            if (IsScene(assetPath)) return map;

            // A managed-reference graph may be cyclic (the graph window renders back-edges), so descending into a rid
            // already on this document's walk would loop forever. One HashSet per call, cleared per document (rids are
            // only unique within a document), records visited rids; revisiting one advances without entering its
            // already-walked subtree.
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

                    // A rid already walked is a back-edge in a cyclic graph; record the constraint but do not descend
                    // into its subtree again, or the iterator would never terminate.
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
        /// <summary>
        /// Returns <see langword="true"/> when another managed-reference property in the same object aliases this
        /// one (shares its <see cref="SerializedProperty.managedReferenceId"/>) — which happens after duplicating an
        /// array element or pasting, leaving two fields backed by a single instance so edits to one bleed into the other.
        /// </summary>
        public static bool HasSharedReference(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return false;

            var id = property.managedReferenceId;
            var shared = false;
            var path = property.propertyPath;

            TraverseManagedReferences(property.serializedObject, other =>
            {
                if (other.propertyPath != path && other.managedReferenceId == id)
                {
                    shared = true;
                    return true;
                }

                return false;
            });

            return shared;
        }

        /// <summary>
        /// Breaks an aliased managed reference by replacing it with an independent clone that carries the same data
        /// (a fresh instance gets a new <see cref="SerializedProperty.managedReferenceId"/> on assignment), so the
        /// two formerly shared fields no longer affect each other.
        /// </summary>
        public static void MakeReferenceUnique(SerializedProperty property)
        {
            var persistent = property.Persistent();
            var current = persistent.managedReferenceValue;
            if (current is null) return;

            persistent.SetManagedReferenceAndApply(CreateInstancePreservingData(current.GetType(), current));
        }

        // Visits every managed-reference property in the object, descending into nested values; stops early when
        // the visitor returns true.
        private static void TraverseManagedReferences(SerializedObject serializedObject, Func<SerializedProperty, bool> visit)
        {
            using var iterator = serializedObject.GetIterator();
            if (!iterator.Next(enterChildren: true)) return;

            do
            {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference && visit(iterator))
                    return;
            }
            while (iterator.Next(enterChildren: true));
        }
        #endregion
    }
}
