using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Completes a "Create new script" flow across the domain reload that the new <c>.cs</c> triggers. The pending
    /// (target, propertyPath, expected type) is parked in <see cref="SessionState"/> before the reload and resolved on
    /// the next load once the script has compiled — assigning a fresh instance of the new type to the field.
    /// </summary>
    internal static class SerializeReferencePendingAssignment
    {
        private const string Key = "Aspid.FastTools.SerializeReference.PendingAssignment";
        private const char EntrySeparator = '\n';
        private const char FieldSeparator = '|';

        /// <summary>Parks an assignment to complete after the next domain reload (when the new type compiles).</summary>
        public static void Enqueue(UnityEngine.Object target, string propertyPath, string fullTypeName)
        {
            if (target == null || string.IsNullOrEmpty(propertyPath) || string.IsNullOrEmpty(fullTypeName)) return;

            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(target);
            var entry = $"{globalId}{FieldSeparator}{propertyPath}{FieldSeparator}{fullTypeName}";

            var existing = SessionState.GetString(Key, string.Empty);
            SessionState.SetString(Key, string.IsNullOrEmpty(existing) ? entry : existing + EntrySeparator + entry);
        }

        [InitializeOnLoadMethod]
        private static void Hook() => EditorApplication.delayCall += Resolve;

        private static void Resolve()
        {
            var raw = SessionState.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(raw)) return;
            SessionState.EraseString(Key);

            foreach (var line in raw.Split(EntrySeparator))
            {
                var parts = line.Split(FieldSeparator);
                if (parts.Length < 3) continue;

                if (!GlobalObjectId.TryParse(parts[0], out var globalId)) continue;
                var target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
                if (target == null) continue;

                var type = ResolveType(parts[2]);
                if (type is null) continue;

                var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(parts[1]);
                if (property is null || property.propertyType != SerializedPropertyType.ManagedReference) continue;

                property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstance(type));
            }
        }

        private static Type ResolveType(string fullName)
        {
            var direct = Type.GetType(fullName, throwOnError: false);
            if (direct is not null) return direct;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, throwOnError: false);
                if (type is not null) return type;
            }

            return null;
        }
    }
}
