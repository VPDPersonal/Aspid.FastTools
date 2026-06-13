using System;
using UnityEditor;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The inverse of Make Unique: deliberately shares one managed-reference instance across several fields of the same
    /// object. Because there is no rid setter, sharing is achieved by assigning the SAME instance to both paths —
    /// Unity then keeps them on a single <see cref="SerializedProperty.managedReferenceId"/> (exactly the aliasing the
    /// shared-reference notice detects), so the rid stripe and notice light up automatically.
    /// </summary>
    internal static class SerializeReferenceLinker
    {
        /// <summary>A sibling managed reference this field could be linked to.</summary>
        internal readonly struct LinkCandidate
        {
            public readonly long Rid;
            public readonly Type Type;
            public readonly string Path;

            public LinkCandidate(long rid, Type type, string path)
            {
                Rid = rid;
                Type = type;
                Path = path;
            }
        }

        /// <summary>
        /// Every other managed reference in the same object that is assignable to this field and is neither this property
        /// nor one of its ancestors/descendants (which would form a self-cycle).
        /// </summary>
        public static List<LinkCandidate> CollectLinkCandidates(SerializedProperty property)
        {
            var result = new List<LinkCandidate>();
            if (property is null) return result;

            var fieldType = SerializeReferenceHelpers.GetFieldType(property);
            var selfPath = property.propertyPath;
            var seen = new HashSet<long>();

            using var iterator = property.serializedObject.GetIterator();
            if (!iterator.Next(enterChildren: true)) return result;

            do
            {
                if (iterator.propertyType != SerializedPropertyType.ManagedReference) continue;

                var path = iterator.propertyPath;
                if (path == selfPath) continue;
                if (IsDescendant(path, selfPath) || IsDescendant(selfPath, path)) continue;

                var value = iterator.managedReferenceValue;
                if (value is null) continue;

                var type = value.GetType();
                if (fieldType != null && !fieldType.IsAssignableFrom(type)) continue;

                var rid = iterator.managedReferenceId;
                if (!seen.Add(rid)) continue; // one representative per shared instance

                result.Add(new LinkCandidate(rid, type, path));
            }
            while (iterator.Next(enterChildren: true));

            return result;
        }

        /// <summary>Points this field at the instance held by <paramref name="sourcePath"/>, sharing its rid.</summary>
        public static bool LinkTo(SerializedProperty property, string sourcePath)
        {
            if (property is null || string.IsNullOrEmpty(sourcePath)) return false;

            var source = property.serializedObject.FindProperty(sourcePath);
            var value = source?.managedReferenceValue;
            if (value is null) return false;

            property.Persistent().SetManagedReferenceAndApply(value);
            return true;
        }

        // True when "candidate" lies inside the "ancestor" property's subtree (a nested field or array element).
        private static bool IsDescendant(string candidate, string ancestor) =>
            candidate.StartsWith(ancestor + ".", StringComparison.Ordinal);
    }
}
