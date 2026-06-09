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
            if (stage is null || !TryMatchAssetFileId(stage, target, go, out fileId)) return false;

            assetPath = stage.assetPath;
            inMemory = true;
            return true;
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
