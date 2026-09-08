#nullable enable
using System;
using UnityEditor;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class SerializableMonoScriptUtility
    {
        internal const string ScriptFieldName = "_script";

        private static Dictionary<Type, MonoScript>? _scriptsByType;

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

        internal static Type? GetCurrentType(SerializedProperty wrapperProperty, out string assemblyQualifiedName)
        {
            assemblyQualifiedName = wrapperProperty.FindPropertyRelative(SerializableTypeUtility.BackingFieldName)?.stringValue ?? string.Empty;

            var script = wrapperProperty.FindPropertyRelative(ScriptFieldName)?.objectReferenceValue as MonoScript;
            if (script && script!.GetClass() is { } declared) return declared;

            return TypeUtility.GetTypeOrNull(assemblyQualifiedName);
        }

        internal static void Assign(SerializedProperty wrapperProperty, Type? type)
        {
            var script = type is not null && TryGetScript(type, out var found) ? found : null;

            wrapperProperty.FindPropertyRelative(ScriptFieldName).objectReferenceValue = script;
            wrapperProperty.FindPropertyRelative(SerializableTypeUtility.BackingFieldName).stringValue =
                type?.AssemblyQualifiedName ?? string.Empty;

            wrapperProperty.serializedObject.ApplyModifiedProperties();
        }

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
