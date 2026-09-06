#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Editor-side support for SerializableMonoScript / SerializableMonoScript<T>: the set of types a script asset
    // declares (the only types the wrapper can hold) and reading / writing a wrapper property.
    internal static class SerializableMonoScriptUtility
    {
        internal const string ScriptFieldName = "_script";

        // Built once per domain: adding or renaming a script always recompiles, which reloads the domain.
        private static Dictionary<Type, MonoScript>? _scriptsByType;

        // Every type Unity maps to a runtime script asset (MonoScript.GetClass() on a runtime-assembly script).
        internal static IReadOnlyDictionary<Type, MonoScript> ScriptsByType
        {
            get
            {
                if (_scriptsByType is not null) return _scriptsByType;

                var map = new Dictionary<Type, MonoScript>();

                foreach (var script in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (!script) continue;

                    var type = script.GetClass();
                    if (type is not null) map.TryAdd(type, script);
                }

                return _scriptsByType = map;
            }
        }

        internal static bool IsMonoScriptWrapperField(Type fieldType) =>
            typeof(SerializableMonoScript).IsAssignableFrom(fieldType.GetCollectionElementTypeOrSelf());

        internal static bool HasScript(Type type) =>
            ScriptsByType.ContainsKey(type);

        internal static bool TryGetScript(Type type, out MonoScript? script) =>
            ScriptsByType.TryGetValue(type, out script);

        // The wrapper's current type: the script's class when a live script is referenced, else whatever the stored
        // name resolves to. The stored name is also handed back so a caption can show a missing type by name.
        internal static Type? GetCurrentType(SerializedProperty wrapperProperty, out string assemblyQualifiedName)
        {
            assemblyQualifiedName = wrapperProperty.FindPropertyRelative(SerializableTypeUtility.BackingFieldName)?.stringValue ?? string.Empty;

            var script = wrapperProperty.FindPropertyRelative(ScriptFieldName)?.objectReferenceValue as MonoScript;
            if (script && script!.GetClass() is { } declared) return declared;

            return TypeUtility.GetTypeOrNull(assemblyQualifiedName);
        }

        // Writes both halves of the wrapper: the script asset (the rename-safe reference) and the type name (what a
        // player build resolves). Null clears the field.
        internal static void Assign(SerializedProperty wrapperProperty, Type? type)
        {
            var script = type is not null && TryGetScript(type, out var found) ? found : null;

            wrapperProperty.FindPropertyRelative(ScriptFieldName).objectReferenceValue = script;
            wrapperProperty.FindPropertyRelative(SerializableTypeUtility.BackingFieldName).stringValue =
                type?.AssemblyQualifiedName ?? string.Empty;

            wrapperProperty.serializedObject.ApplyModifiedProperties();
        }

        // For a caller that already wrote the wrapper's backing type-name string: points the sibling script reference
        // at the script declaring that type, so the editor-side sync does not revert the name to the previous script.
        // A no-op when the string does not belong to a SerializableMonoScript wrapper.
        internal static void SyncScriptFromName(SerializedProperty nameProperty)
        {
            var path = nameProperty.propertyPath;
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return;

            using var wrapper = nameProperty.serializedObject.FindProperty(path[..lastDotIndex]);
            var field = wrapper?.GetFieldInfo();
            if (wrapper is null || field is null || !IsMonoScriptWrapperField(field.FieldType)) return;

            var type = TypeUtility.GetTypeOrNull(nameProperty.stringValue);
            var script = type is not null && TryGetScript(type, out var found) ? found : null;

            wrapper.FindPropertyRelative(ScriptFieldName).objectReferenceValue = script;
            wrapper.serializedObject.ApplyModifiedProperties();
        }

        // Resolves a dragged MonoScript to a type the wrapper may hold: declared by the script, assignable to every
        // constraint, and admitted by the kind filter.
        internal static bool TryResolveDroppedType(Type[]? types, TypeAllow allow, out Type? type)
        {
            type = null;

            foreach (var dragged in DragAndDrop.objectReferences)
            {
                if (dragged is not MonoScript script) continue;

                var candidate = script.GetClass();
                if (candidate is null || !HasScript(candidate)) continue;
                if (candidate.IsInterface && !allow.HasFlag(TypeAllow.Interface)) continue;
                if (candidate.IsAbstract && !candidate.IsInterface && !allow.HasFlag(TypeAllow.Abstract)) continue;
                if (types is not null && !Array.TrueForAll(types, constraint => constraint is null || constraint.IsAssignableFrom(candidate))) continue;

                type = candidate;
                return true;
            }

            return false;
        }
    }
}
